using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;

namespace Server.Runtime {
	public class BuildServer {

		public event Action<BuildProcess> OnInitBuild;

		string ConvertToBuildName(FileInfo file) {
			var ext = file.Extension;
			return file.Name.Substring(0, file.Name.Length - ext.Length);
		}
		
		public Dictionary<string, string> Builds {
			get {
				var dict = new Dictionary<string, string>();
				var buildsPath = _project.BuildsRoot;
				if (Directory.Exists(buildsPath)) {
					var files = Directory.EnumerateFiles(buildsPath, "*.json");
					foreach (var filePath in files) {
						var file = new FileInfo(filePath);
						var fullPath = file.FullName;
						dict.Add(ConvertToBuildName(file), fullPath);
					}
				}
				return dict;
			}
		}
		
		readonly Project _project = null;
		
		Build        _build   = null;
		Thread       _thread  = null;
		BuildProcess _process = null;
		
		public BuildServer(string projectPath) {
			_project = Project.Load(projectPath);
		}

		public string FindBuildPath(string buildName) {
			var builds = Builds;
			foreach (var build in builds) {
				if (build.Key == buildName) {
					return build.Value;
				}
			}
			return null;
		}
		
		public void InitBuild(Build build) {
			if (_process != null) {
				return;
			}
			_build = build;
			_process = new BuildProcess(build);
			OnInitBuild?.Invoke(_process);
		}

		public void StartBuild() {
			if (_process == null) {
				return;
			}
			_thread = new Thread(ProcessBuild);
			_thread.Start();
		}

		public void StopBuild() {
			_process?.Abort();
		}

		void ProcessBuild() {
			var nodes = _build.Nodes;
			_process.StartBuild();
			if (nodes.Count == 0) {
				_process.Abort();
			}
			foreach (var node in nodes) {
				var result = ProcessCommand(node);
				if (!result) {
					_process.Abort();
				}
				if (_process.IsAborted) {
					break;
				}
			}
			_process = null;
		}

		string ConvertArgValue(Project project, string value) {
			var result = value;
			foreach (var key in project.Keys) {
				var keyFormat = string.Format("{{{0}}}", key.Key);
				if (result.Contains(keyFormat)) {
					result = result.Replace(keyFormat, key.Value);
				}
			}
			return result;
		}
		
		Dictionary<string, string> CreateRuntimeArgs(Project project, BuildNode node) {
			var dict = new Dictionary<string, string>();
			foreach (var arg in node.Args) {
				var value = ConvertArgValue(project, arg.Value);
				dict.Add(arg.Key, value);
			}
			return dict;
		}
		
		bool ProcessCommand(BuildNode node) {
			_process.StartTask(node);
			var command = CommandFactory.Create(node);
			var runtimeArgs = CreateRuntimeArgs(_project, node);
			var result = command.Execute(runtimeArgs);
			_process.DoneTask(result.IsSuccess, result.Message);
			return result.IsSuccess;
		}
	}
}