using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;
using Server.Integrations;

namespace Server.Runtime {
	public class BuildServer {

		public event Action               OnStatusRequest;
		public event Action<string, bool> OnCommonError; 
		public event Action               OnHelpRequest;
		public event Action<BuildProcess> OnInitBuild;
		public event Action               OnStop;
		public event Action<string>       LogFileChanged;

		public string         Name     { get; }
		public Project        Project  { get; private set; }
		public List<IService> Services { get; private set; }
		
		public string ServiceName {
			get {
				var assembly = GetType().GetTypeInfo().Assembly;
				var name = assembly.GetName();
				return $"{name.Name} {name.Version}";
			}
		}
		
		string[]     _buildArgs;
		Build        _build;
		Thread       _thread;
		BuildProcess _process;
		ICommand     _curCommand;
		
		Dictionary<string, string> _taskStates = new Dictionary<string, string>();
		
		DateTime _curTime => DateTime.Now;
		
		public BuildServer(string name) {
			Debug.WriteLine($"BuildServer.ctor: name: \"{name}\"");
			Name = name;
		}

		static string ConvertToBuildName(FileSystemInfo file) {
			var ext = file.Extension;
			return file.Name.Substring(0, file.Name.Length - ext.Length);
		}
		
		public Dictionary<string, Build> FindBuilds() {
			var tempDict = new Dictionary<string, Build>();
			var buildsPath = Project.BuildsRoot;
			if (!Directory.Exists(buildsPath)) {
				return null;
			}
			var files = Directory.EnumerateFiles(buildsPath, "*.json");
			foreach (var filePath in files) {
				var file = new FileInfo(filePath);
				var name = ConvertToBuildName(file);
				var fullPath = file.FullName;
				try {
					var build = Build.Load(name, fullPath);
					tempDict.Add(name, build);
				} catch (Exception e) {
					RaiseCommonError($"Failed to load build at \"{fullPath}\": \"{e}\"", true);	
				}
			}
			var resultDict = new Dictionary<string, Build>();
			foreach (var buildPair in tempDict) {
				try {
					var build = buildPair.Value;
					ProcessSubBuilds(build, tempDict);
					ValidateBuild(build);
					resultDict.Add(buildPair.Key, build);
				} catch(Exception e) {
					RaiseCommonError($"Failed to process build \"{buildPair.Key}\" : \"{e}\"", true);
				}
			}
			return resultDict;
		}

		void ValidateBuild(Build build) {
			foreach (var node in build.Nodes) {
				ValidateNode(node);
			}
		}

		void ValidateNode(BuildNode node) {
			if (string.IsNullOrEmpty(node.Command) || !CommandFactory.ContainsHandler(node.Command)) {
				throw new CommandNotFoundException(node.Command);
			}
		}

		void ProcessSubBuilds(Build build, Dictionary<string, Build> builds) {
			var nodes = build.Nodes;
			for (int i = 0; i < nodes.Count; i++) {
				var node = build.Nodes[i];
				var subBuildNode = node as SubBuildNode;
				if (subBuildNode == null) {
					continue;
				}
				var subBuildName = subBuildNode.Name;
				Debug.WriteLine($"BuildServer.ProcessSubBuilds: Process sub build node: \"{subBuildName}\"");
				Build subBuild;
				if (!builds.TryGetValue(subBuildName, out subBuild)) {
					throw new SubBuildNotFoundException(subBuildName);
				}
				ProcessSubBuilds(subBuild, builds);
				nodes.RemoveAt(i);
				var subNodes = subBuild.Nodes;
				var newNodes = new List<BuildNode>();
				foreach (var subNode in subNodes) {
					var newArgs = new Dictionary<string, string>();
					foreach (var subBuildArg in subBuildNode.Args) {
						var sbKey = subBuildArg.Key;
						var sbValue = subBuildArg.Value;
						foreach (var subNodeArg in subNode.Args) {
							if (!newArgs.ContainsKey(subNodeArg.Key)) {
								newArgs.Add(subNodeArg.Key, string.Empty);
							}
							var subNodeValue = subNodeArg.Value;
							var newValue = TryReplace(subNodeValue, sbKey, sbValue);
							newArgs[subNodeArg.Key] = newValue;
							Debug.WriteLine(
								$"BuildServer.ProcessSubBuilds: Convert value: \"{subNodeValue}\" => \"\"{newValue}\"\"");
						}
					}
					newNodes.Add(new BuildNode(subNode.Name, subNode.Command, newArgs));
				}
				nodes.InsertRange(i, newNodes);
			}
		}
		
		public bool TryInitialize(out string errorMessage, List<IService> services, params string[] projectPathes) {
			Debug.WriteLine(
				$"BuildServer.TryInitialize: services: {services.Count()}, pathes: {projectPathes.Length}");
			try {
				Project = Project.Load(Name, projectPathes);
			} catch (Exception e) {
				errorMessage = $"Failed to parse project settings: \"{e}\"";
				return false;
			}
			InitServices(services, Project);
			if (FindBuilds() == null) {
				errorMessage = $"Failed to load builds directory!";
				return false;
			}
			errorMessage = string.Empty;
			return true;
		}
		
		void InitServices(IEnumerable<IService> services, Project project) {
			Services = new List<IService>();
			foreach (var service in services) {
				Debug.WriteLine($"BuildServer.InitServices: \"{service.GetType().Name}\"");
				var isInited = service.TryInit(this, project);
				Debug.WriteLine($"BuildServer.InitServices: isInited: {isInited}");
				if (isInited) {
					Services.Add(service);
				}
			}
		}
		
		public bool TryInitBuild(Build build) {
			if (_process != null) {
				RaiseCommonError("InitBuild: server is busy!", true);
				return false;
			}
			Debug.WriteLine($"BuildServer.InitBuild: \"{build.Name}\"");
			_build = build;
			_process = new BuildProcess(build);
			var convertedLogFile = ConvertArgValue(Project, _build, null, build.LogFile);
			LogFileChanged?.Invoke(convertedLogFile);
			OnInitBuild?.Invoke(_process);
			return true;
		}

		public void StartBuild(string[] args) {
			if (_process == null) {
				Debug.WriteLine("BuildServer.StartBuild: No build to start!");
				return;
			}
			if (_process.IsStarted) {
				Debug.WriteLine("BuildServer.StartBuild: Build already started!");
				return;
			}
			Debug.WriteLine($"BuildServer.StartBuild: args: {args.Length}");
			_buildArgs = args;
			_thread = new Thread(ProcessBuild);
			_thread.Start();
		}

		void ProcessBuild() {
			Debug.WriteLine("BuildServer.ProcessBuild");
			var nodes = _build.Nodes;
			_process.StartBuild(_curTime);
			if (nodes.Count == 0) {
				Debug.WriteLine("BuildServer.ProcessBuild: No build nodes!");
				_process.Abort(_curTime);
			}
			foreach (var node in nodes) {
				Debug.WriteLine($"BuildServer.ProcessBuild: node: \"{node.Name}\" (\"{node.Command}\")");
				var result = ProcessCommand(_build, _buildArgs, node);
				if (!result) {
					Debug.WriteLine($"BuildServer.ProcessBuild: failed command!");
					_process.Abort(_curTime);
				}
				if (_process.IsAborted) {
					Debug.WriteLine($"BuildServer.ProcessBuild: aborted!");
					break;
				}
			}
			LogFileChanged?.Invoke(null);
			_buildArgs = null;
			_build     = null;
			_thread    = null;
			_process   = null;
			_taskStates = new Dictionary<string, string>();
			Debug.WriteLine("BuildServer.ProcessBuild: cleared");
		}

		string TryReplace(string message, string key, string value) {
			var keyFormat = string.Format("{{{0}}}", key);
			if (message.Contains(keyFormat)) {
				return message.Replace(keyFormat, value);
			}
			return message;
		}
		
		string ConvertArgValue(Project project, Build build, string[] buildArgs, string value) {
			if ( value == null ) {
				return null;
			}
			var result = value;
			foreach (var key in project.Keys) {
				result = TryReplace(result, key.Key, key.Value);
			}
			if (buildArgs != null) {
				for (var i = 0; i < build.Args.Count; i++) {
					var argName = build.Args[i];
					var argValue = buildArgs[i];
					result = TryReplace(result, argName, argValue);
				}
			}
			foreach (var state in _taskStates) {
				result = TryReplace(result, state.Key, state.Value);
			}
			Debug.WriteLine($"BuildServer.ConvertArgValue: \"{value}\" => \"{result}\"");
			return result;
		}

		public Dictionary<string, string> FindCurrentBuildArgs() {
			if ((_buildArgs == null) || (_build == null)) {
				return null;
			}
			var dict = new Dictionary<string, string>();
			for (int i = 0; i < _build.Args.Count; i++) {
				var argName = _build.Args[i];
				var argValue = _buildArgs[i];
				dict.Add(argName, argValue);
			}
			return dict;
		}
		
		Dictionary<string, string> CreateRuntimeArgs(Project project, Build build, string[] buildArgs, BuildNode node) {
			var dict = new Dictionary<string, string>();
			foreach (var arg in node.Args) {
				var value = ConvertArgValue(project, build, buildArgs, arg.Value);
				dict.Add(arg.Key, value);
			}
			return dict;
		}
		
		bool ProcessCommand(Build build, string[] buildArgs, BuildNode node) {
			Debug.WriteLine($"BuildServer.ProcessCommand: \"{node.Name}\" (\"{node.Command}\")");
			_process.StartTask(node);
			var command = CommandFactory.Create(node);
			_curCommand = command;
			Debug.WriteLine($"BuildServer.ProcessCommand: command is \"{command.GetType().Name}\"");
			var runtimeArgs = CreateRuntimeArgs(Project, build, buildArgs, node);
			Debug.WriteLine($"BuildServer.ProcessCommand: runtimeArgs is {runtimeArgs.Count}");
			var result = command.Execute(runtimeArgs);
			_curCommand = null;
			Debug.WriteLine(
				$"BuildServer.ProcessCommand: result is [{result.IsSuccess}, \"{result.Message}\", \"{result.Result}\"]");
			_process.DoneTask(_curTime, result);
			AddTaskState(node.Name, result);
			return result.IsSuccess;
		}

		void AddTaskState(string taskName, CommandResult result) {
			AddTaskState(taskName, "message", result.Message);
			AddTaskState(taskName, "result", result.Result);
		}

		void AddTaskState(string taskName, string key, string value) {
			var fullKey = $"{taskName}:{key}";
			_taskStates.Add(fullKey, value);
			Debug.WriteLine($"BuildServer.AddTaskState: \"{fullKey}\"=>\"{value}\"");
		}
		
		public void RequestStatus() {
			Debug.WriteLine("BuildServer.RequestStatus");
			OnStatusRequest?.Invoke();
		}

		public void AbortBuild() {
			Debug.WriteLine($"BuildServer.AbortBuild: hasProcess: {_process != null}");
			var proc = _process;
			if(proc != null ) {
				Debug.WriteLine("BuildServer.AbortBuild: Abort running process");
				proc.Abort(_curTime);
			}
			var abortableCommand = _curCommand as IAbortableCommand;
			if (abortableCommand != null) {
				Debug.WriteLine("BuildServer.AbortBuild: Abort running command");
				abortableCommand.Abort();
			}
		}
		
		public void StopServer() {
			Debug.WriteLine($"BuildServer.StopServer: hasProcess: {_process != null}");
			AbortBuild();
			while (_process != null) {}
			OnStop?.Invoke();
			Debug.WriteLine("BuildServer.StopServer: done");
		}

		public void RequestHelp() {
			Debug.WriteLine("BuildServer.RequestHelp");
			OnHelpRequest?.Invoke();
		}

		public void RaiseCommonError(string message, bool isFatal) {
			Debug.WriteLine($"BuildServer.RaiseCommonError: \"{message}\", isFatal: {isFatal}");
			OnCommonError?.Invoke(message, isFatal);
		}
	}
}