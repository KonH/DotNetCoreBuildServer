using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;
using Server.Services;
using Microsoft.Extensions.Logging;

namespace Server.Runtime {
	public class BuildServer {

		public event Action               OnStatusRequest;
		public event Action<string, bool> OnCommonError;
		public event Action<string>       OnCommonMessage;
		public event Action               OnHelpRequest;
		public event Action<BuildProcess> OnInitBuild;
		public event Action               OnStop;
		public event Action<string>       LogFileChanged;

		public string         Name     { get; }
		public Project        Project  { get; private set; }
		public List<IService> Services { get; private set; }

		public Dictionary<string, BuildCommand> Commands { get; private set; }

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

		CommandFactory _commandFactory;
		LoggerFactory  _loggerFactory;
		ILogger        _logger;
		
		public BuildServer(CommandFactory commandFactory, LoggerFactory loggerFactory, string name) {
			_commandFactory = commandFactory;
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory.CreateLogger<BuildServer>();
			_logger.LogDebug($"ctor: name: \"{name}\"");
			Name = name;
			Commands = new Dictionary<string, BuildCommand>();
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
					var build = Build.Load(_loggerFactory, name, fullPath);
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
			if (string.IsNullOrEmpty(node.Command) || !_commandFactory.ContainsHandler(node.Command)) {
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
				_logger.LogDebug($"ProcessSubBuilds: Process sub build node: \"{subBuildName}\"");
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
							var subNodeValue = subNodeArg.Value;
							string newValue = null;
							if (!newArgs.ContainsKey(subNodeArg.Key)) {
								newValue = TryReplace(subNodeValue, sbKey, sbValue);
								newArgs.Add(subNodeArg.Key, newValue);
							} else {
								newValue = TryReplace(newArgs[subNodeArg.Key], sbKey, sbValue);
								newArgs[subNodeArg.Key] = newValue;
							}
							_logger.LogDebug(
								$"ProcessSubBuilds: Convert value: \"{subNodeValue}\" => \"\"{newValue}\"\"");
						}
					}
					if ( newArgs.Count == 0 ) {
						newArgs = subNode.Args;
					}
					newNodes.Add(new BuildNode(subNode.Name, subNode.Command, newArgs));
					_logger.LogDebug(
						$"ProcessSubBuilds: Converted node: \"{subNode.Name}\", args: {newArgs.Count}");
				}
				nodes.InsertRange(i, newNodes);
			}
		}
		
		public bool TryInitialize(out string errorMessage, List<IService> services, params string[] projectPathes) {
			_logger.LogDebug(
				$"TryInitialize: services: {services.Count()}, pathes: {projectPathes.Length}");
			try {
				Project = Project.Load(_loggerFactory, Name, projectPathes);
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
				_logger.LogDebug($"InitServices: \"{service.GetType().Name}\"");
				var isInited = service.TryInit(this, project);
				_logger.LogDebug($"InitServices: isInited: {isInited}");
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
			_logger.LogDebug($"InitBuild: \"{build.Name}\"");
			_build = build;
			_process = new BuildProcess(_loggerFactory, build);
			var convertedLogFile = ConvertArgValue(Project, _build, null, build.LogFile);
			LogFileChanged?.Invoke(convertedLogFile);
			OnInitBuild?.Invoke(_process);
			return true;
		}

		public void StartBuild(string[] args) {
			if (_process == null) {
				_logger.LogError("StartBuild: No build to start!");
				return;
			}
			if (_process.IsStarted) {
				_logger.LogError("StartBuild: Build already started!");
				return;
			}
			_logger.LogDebug($"StartBuild: args: {args.Length}");
			_buildArgs = args;
			_thread = new Thread(ProcessBuild);
			_thread.Start();
		}

		void ProcessBuild() {
			_logger.LogDebug("ProcessBuild");
			var nodes = _build.Nodes;
			_process.StartBuild(_curTime);
			if (nodes.Count == 0) {
				_logger.LogError("ProcessBuild: No build nodes!");
				_process.Abort(_curTime);
			}
			foreach (var node in nodes) {
				_logger.LogDebug($"ProcessBuild: node: \"{node.Name}\" (\"{node.Command}\")");
				var result = ProcessCommand(_build, _buildArgs, node);
				if (!result) {
					_logger.LogDebug($"ProcessBuild: failed command!");
					_process.Abort(_curTime);
				}
				if (_process.IsAborted) {
					_logger.LogDebug($"ProcessBuild: aborted!");
					break;
				}
			}
			LogFileChanged?.Invoke(null);
			_buildArgs = null;
			_build     = null;
			_thread    = null;
			_process   = null;
			_taskStates = new Dictionary<string, string>();
			_logger.LogDebug("ProcessBuild: cleared");
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
			_logger.LogDebug($"ConvertArgValue: \"{value}\" => \"{result}\"");
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
			_logger.LogDebug($"ProcessCommand: \"{node.Name}\" (\"{node.Command}\")");
			_process.StartTask(node);
			var command = _commandFactory.Create(node);
			_curCommand = command;
			_logger.LogDebug($"ProcessCommand: command is \"{command.GetType().Name}\"");
			var runtimeArgs = CreateRuntimeArgs(Project, build, buildArgs, node);
			_logger.LogDebug($"ProcessCommand: runtimeArgs is {runtimeArgs.Count}");
			var result = command.Execute(_loggerFactory, runtimeArgs);
			_curCommand = null;
			_logger.LogDebug(
				$"ProcessCommand: result is [{result.IsSuccess}, \"{result.Message}\", \"{result.Result}\"]");
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
			if ( _taskStates.ContainsKey(fullKey) ) {
				_taskStates[fullKey] = value;
				_logger.LogDebug($"AddTaskState: Override \"{fullKey}\"=>\"{value}\"");
			} else {
				_taskStates.Add(fullKey, value);
				_logger.LogDebug($"AddTaskState: Add \"{fullKey}\"=>\"{value}\"");
			}
		}
		
		public void RequestStatus() {
			_logger.LogDebug("RequestStatus");
			OnStatusRequest?.Invoke();
		}

		public void AbortBuild() {
			_logger.LogDebug($"AbortBuild: hasProcess: {_process != null}");
			var proc = _process;
			if(proc != null ) {
				_logger.LogDebug("AbortBuild: Abort running process");
				proc.Abort(_curTime);
			}
			var abortableCommand = _curCommand as IAbortableCommand;
			if (abortableCommand != null) {
				_logger.LogDebug("AbortBuild: Abort running command");
				abortableCommand.Abort();
			}
		}
		
		public void StopServer() {
			_logger.LogDebug($"StopServer: hasProcess: {_process != null}");
			AbortBuild();
			while (_process != null) {}
			OnStop?.Invoke();
			_logger.LogDebug("StopServer: done");
		}

		public void RequestHelp() {
			_logger.LogDebug("RequestHelp");
			OnHelpRequest?.Invoke();
		}

		public void RaiseCommonError(string message, bool isFatal) {
			_logger.LogError($"RaiseCommonError: \"{message}\", isFatal: {isFatal}");
			OnCommonError?.Invoke(message, isFatal);
		}

		public void RaiseCommonMessage(string message) {
			_logger.LogDebug($"RaiseCommonMessage: \"{message}\"");
			OnCommonMessage?.Invoke(message);
		}

		public void AddCommand(string name, string description, Action<RequestArgs> handler) {
			_logger.LogDebug($"AddHandler: \"{name}\" => \"{handler.GetMethodInfo().Name}\"");
			Commands.Add(name, new BuildCommand(description, handler));
		}

		public void AddCommands(string name, string description, Action handler) {
			_logger.LogDebug($"AddHandler: \"{name}\" => \"{handler.GetMethodInfo().Name}\"");
			Commands.Add(name, new BuildCommand(description, (_) => handler.Invoke()));
		}
	}
}