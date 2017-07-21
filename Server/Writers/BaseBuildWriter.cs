using Server.Runtime;

namespace Server.Writers {
	public abstract class BaseBuildWriter {

		protected BuildProcess Process = null;
		
		protected BaseBuildWriter(BuildProcess buildProcess) {
			Process = buildProcess;
			Process.BuildStarted += OnBuildProcessStarted;
			Process.TaskStarted  += OnTaskStarted;
			Process.TaskDone     += OnTaskDone;
			Process.BuildDone    += OnBuildProcessDone;
		}

		protected abstract void OnBuildProcessStarted();
		protected abstract void OnTaskStarted(BuildTask buildTask);
		protected abstract void OnTaskDone(BuildTask buildTask);
		protected abstract void OnBuildProcessDone();
	}
}