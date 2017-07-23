using Server.Runtime;

namespace Server.Views {
	public abstract class BaseServerView {

		public bool Alive => Server != null;
		
		protected BuildServer  Server  = null;
		protected BuildProcess Process = null;
		
		protected BaseServerView(BuildServer server) {
			Server = server;
			Server.OnInitBuild     += OnInitBuild;
			Server.OnStatusRequest += OnStatusRequest;
			Server.OnStop          += OnStop;
		}

		protected string GetStatusMessage() {
			var message = $"Is busy: {Process != null}\n";
			message += "Services:\n";
			foreach (var service in Server.Services) {
				message += $"- {service.GetType().Name}\n";
			}
			message += "Builds:\n";
			foreach (var build in Server.Builds) {
				message += $"- {build.Key}\n";
			}
			return message;
		}
		
		protected abstract void OnStatusRequest();
		
		void OnInitBuild(BuildProcess process) {
			Process = process;
			Process.BuildStarted += OnBuildProcessStarted;
			Process.TaskStarted  += OnTaskStarted;
			Process.TaskDone     += OnTaskDone;
			Process.BuildDone    += OnBuildProcessDone;
		}

		protected abstract void OnBuildProcessStarted();
		protected abstract void OnTaskStarted(BuildTask buildTask);
		protected abstract void OnTaskDone(BuildTask buildTask);

		protected virtual void OnBuildProcessDone() {
			Process.BuildStarted -= OnBuildProcessStarted;
			Process.TaskStarted  -= OnTaskStarted;
			Process.TaskDone     -= OnTaskDone;
			Process.BuildDone    -= OnBuildProcessDone;
			Process = null;
		}

		void OnStop() {
			Server = null;
		}
	}
}