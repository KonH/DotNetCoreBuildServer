using System;
using System.Threading;

namespace Server {
	public class BuildServer {

		ProjectConfig _projectConfig = null;
		BuildConfig   _buildConfig   = null;
		Build         _build         = null;
		
		public BuildServer() {
			_projectConfig = ProjectConfig.Load();
		}

		public Build InitBuild(BuildConfig buildConfig) {
			_buildConfig = buildConfig;
			_build = new Build(buildConfig);
			return _build;
		}

		public void StartBuild() {
			var thread = new Thread(ProcessBuild);
			thread.Start();
		}

		void ProcessBuild() {
			for (int i = 0; i < _buildConfig.Tasks.Count; i++) {
				var task = _buildConfig.Tasks[i];
				ProcessTask(task);
			}
		}

		void ProcessTask(string task) {
			_build.StartTask(task);
			_build.DoneTask(task, true);
		}
	}
}