using System.Collections.Generic;
using System.Diagnostics;
using Server.Runtime;

namespace Server.Views {
	public abstract class BaseServerView {

		public bool Alive => Server != null;
		
		protected BuildServer  Server  = null;
		protected BuildProcess Process = null;
		
		protected BaseServerView(BuildServer server) {
			Server = server;
			Server.OnCommonError   += OnCommonError;
			Server.OnInitBuild     += OnInitBuild;
			Server.OnHelpRequest   += OnHelpRequest;
			Server.OnStatusRequest += OnStatusRequest;
			Server.OnStop          += OnStop;
		}

		protected abstract void OnCommonError(string message);
		
		protected string GetHelpMessage() {
			var message = "Commands:\n";
			message += "- \"status\" - current server status\n";
			message += "- \"stop\" - stop current build, if it is started\n";
			message += "- \"build arg0 arg1 ... argN\" - start build with given parameters\n";
			message += "- \"help\" - show this message\n";
			return message;
		}

		protected abstract void OnHelpRequest();
		
		protected string GetStatusMessage() {
			var message = $"{Server.ServiceName}\n";
			message += $"Is busy: {Process != null}\n";
			message += "Services:\n";
			foreach (var service in Server.Services) {
				message += $"- {service.GetType().Name}\n";
			}
			message += "Builds:\n";
			foreach (var build in Server.Builds) {
				message += $"- {build.Key}";
				var args = build.Value.Args;
				if (args.Count > 0) {
					message += " (";
					for (int i = 0; i < args.Count; i++) {
						var arg = build.Value.Args[i];
						message += arg;
						if (i < args.Count - 1) {
							message += "; ";
						}
					}
					message += ")";
				}
				message += "\n";
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
			Debug.WriteLine($"OnInitBuild: {process.Name}");
		}

		protected string GetBuildArgsMessage() {
			var msg = "";
			var args = Server.FindCurrentBuildArgs();
			if ((args != null) && (args.Count > 0)) {
				msg += "(";
				foreach (var arg in args) {
					msg += $"{arg.Key}: {arg.Value}, ";
				}
				msg = msg.Substring(0, msg.Length- 2);
				msg += ")";
			}
			return msg;
		}
		
		protected string GetBuildProcessStartMessage() {
			return $"Build started: {Process.Name} {GetBuildArgsMessage()}\n";
		}
		
		protected abstract void OnBuildProcessStarted();
		protected abstract void OnTaskStarted(BuildTask buildTask);
		protected abstract void OnTaskDone(BuildTask buildTask);

		protected virtual void OnBuildProcessDone() {
			Debug.WriteLine($"OnBuildProcessDone: {Process.Name}");
			Process.BuildStarted -= OnBuildProcessStarted;
			Process.TaskStarted  -= OnTaskStarted;
			Process.TaskDone     -= OnTaskDone;
			Process.BuildDone    -= OnBuildProcessDone;
			Process = null;
		}

		void OnStop() {
			Server = null;
		}

		protected string GetTaskInfo(BuildTask task) {
			if (task.IsStarted) {
				var msg = $"{task.Node.Name} (success: {task.IsSuccess})";
				if (!string.IsNullOrEmpty(task.Message)) {
					msg += $", message: \"{task.Message}\"";
				}
				if (!string.IsNullOrEmpty(task.Result)) {
					msg += $", result: \"{task.Result}\"";
				}
				msg += ")";
				return msg;
			} else {
				return $"{task.Node.Name} (skip)";
			}
		}

		protected string GetTasksInfo(List<BuildTask> tasks) {
			var msg = "";
			foreach (var task in tasks) {
				msg += $"{GetTaskInfo(task)}\n";
			}
			return msg;
		}
	}
}