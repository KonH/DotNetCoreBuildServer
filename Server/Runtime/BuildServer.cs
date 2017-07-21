using System;
using System.Collections.Generic;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;

namespace Server.Runtime {
	public class BuildServer {

		public event Action<BuildProcess> OnInitBuild;
		
		readonly Project _project = null;
		
		Build        _build   = null;
		Thread       _thread  = null;
		BuildProcess _process = null;
		
		public BuildServer(string projectPath) {
			_project = Project.Load(projectPath);
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