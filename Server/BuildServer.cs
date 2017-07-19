using System;
using System.Threading;
using Server.Commands;

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
			var commands = _buildConfig.Commands;
			for (int i = 0; i < commands.Count; i++) {
				var command = commands[i];
				ProcessCommand(command);
			}
		}

		void ProcessCommand(BuildCommand buildCommand) {
			var taskName = buildCommand.Name;
			_build.StartTask(taskName);
			var commandImpl = CommandFactory.Create(taskName);
			var result = commandImpl.Execute(buildCommand.Args);
			_build.DoneTask(taskName, result.IsSuccess, result.Message);
		}
	}
}