using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;
using Server.Integrations;

namespace Server.Runtime {
	public class BuildServer {

		public string ServiceName {
			get {
				var assembly = GetType().GetTypeInfo().Assembly;
				var name = assembly.GetName();
				return $"{name.Name} {name.Version}";
			}
		}

		public event Action               OnStatusRequest;
		public event Action               OnHelpRequest;
		public event Action<BuildProcess> OnInitBuild;
		public event Action               OnStop;

		string ConvertToBuildName(FileInfo file) {
			var ext = file.Extension;
			return file.Name.Substring(0, file.Name.Length - ext.Length);
		}
		
		public Project Project { get; }
		
		public Dictionary<string, Build> Builds {
			get {
				var dict = new Dictionary<string, Build>();
				var buildsPath = Project.BuildsRoot;
				if (Directory.Exists(buildsPath)) {
					var files = Directory.EnumerateFiles(buildsPath, "*.json");
					foreach (var filePath in files) {
						var file = new FileInfo(filePath);
						var name = ConvertToBuildName(file);
						var fullPath = file.FullName;
						var build = Build.Load(name, fullPath);
						if (build != null) {
							dict.Add(name, build);
						}
					}
				}
				return dict;
			}
		}
		
		public List<IService> Services { get; private set; }

		public string Name { get; }
		
		string[]     _buildArgs = null;
		Build        _build     = null;
		Thread       _thread    = null;
		BuildProcess _process   = null;
		
		Dictionary<string, string> _taskStates = new Dictionary<string, string>();
		
		DateTime _curTime => DateTime.Now;
		
		public BuildServer(string name, IEnumerable<IService> services, params string[] projectPathes) {
			Name = name;
			Project = Project.Load(projectPathes);
			InitServices(services, Project);
		}

		void InitServices(IEnumerable<IService> services, Project project) {
			Services = new List<IService>();
			foreach (var service in services) {
				if (service.TryInit(this, project)) {
					Services.Add(service);
				}
			}
		}
		
		public void InitBuild(Build build) {
			if (_process != null) {
				return;
			}
			_build = build;
			_process = new BuildProcess(build);
			OnInitBuild?.Invoke(_process);
		}

		public void StartBuild(string[] args) {
			if (_process == null) {
				return;
			}
			_buildArgs = args;
			_thread = new Thread(ProcessBuild);
			_thread.Start();
		}
		
		public void StopBuild() {
			_process?.Abort(_curTime);
		}

		void ProcessBuild() {
			var nodes = _build.Nodes;
			_process.StartBuild(_curTime);
			if (nodes.Count == 0) {
				_process.Abort(_curTime);
			}
			foreach (var node in nodes) {
				var result = ProcessCommand(_build, _buildArgs, node);
				if (!result) {
					_process.Abort(_curTime);
				}
				if (_process.IsAborted) {
					break;
				}
			}
			_buildArgs = null;
			_build     = null;
			_thread    = null;
			_process   = null;

			_taskStates = new Dictionary<string, string>();
		}

		string TryReplace(string message, string key, string value) {
			var keyFormat = string.Format("{{{0}}}", key);
			if (message.Contains(keyFormat)) {
				return message.Replace(keyFormat, value);
			}
			return message;
		}
		
		string ConvertArgValue(Project project, Build build, string[] buildArgs, string value) {
			var result = value;
			foreach (var key in project.Keys) {
				result = TryReplace(result, key.Key, key.Value);
			}
			for (int i = 0; i < build.Args.Count; i++) {
				var argName = build.Args[i];
				var argValue = buildArgs[i];
				result = TryReplace(result, argName, argValue);
			}
			foreach (var state in _taskStates) {
				result = TryReplace(result, state.Key, state.Value);
			}
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
			_process.StartTask(node);
			var command = CommandFactory.Create(node);
			var runtimeArgs = CreateRuntimeArgs(Project, build, buildArgs, node);
			var result = command.Execute(runtimeArgs);
			_process.DoneTask(_curTime, result.IsSuccess, result.Message, result.Result);
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
		}
		
		public void RequestStatus() {
			OnStatusRequest?.Invoke();
		}

		public void StopServer() {
			StopBuild();
			while (_process != null) {}
			OnStop?.Invoke();
		}

		public void RequestHelp() {
			OnHelpRequest?.Invoke();
		}
	}
}