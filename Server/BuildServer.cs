using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Server.BuildConfig;
using Server.Commands;
using Server.Runtime;

namespace Server {
	public class BuildServer {

		Project      _project = null;
		Build        _build   = null;
		BuildProcess _process = null;
		
		public BuildServer() {
			_project = Project.Load();
		}

		public BuildProcess InitBuild(Build build) {
			_build = build;
			_process = new BuildProcess(build);
			return _process;
		}

		public void StartBuild() {
			var thread = new Thread(ProcessBuild);
			thread.Start();
		}

		void ProcessBuild() {
			var nodes = _build.Nodes;
			for (int i = 0; i < nodes.Count; i++) {
				var node = nodes[i];
				ProcessCommand(node);
			}
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
		
		void ProcessCommand(BuildNode node) {
			_process.StartTask(node);
			var command = CommandFactory.Create(node);
			var runtimeArgs = CreateRuntimeArgs(_project, node);
			var result = command.Execute(runtimeArgs);
			_process.DoneTask(node, result.IsSuccess, result.Message);
		}
	}
}