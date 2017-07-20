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

		void ProcessCommand(BuildNode node) {
			_process.StartTask(node);
			var command = CommandFactory.Create(node);
			var result = command.Execute(node.Args);
			_process.DoneTask(node, result.IsSuccess, result.Message);
		}
	}
}