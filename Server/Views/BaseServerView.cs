using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Server.Runtime;

namespace Server.Views {
	public abstract class BaseServerView {

		public bool Alive => Server != null;
		
		protected BuildServer  Server;
		protected BuildProcess Process;
		
		protected BaseServerView(BuildServer server) {
			Server = server;
			Server.OnCommonError   += OnCommonError;
			Server.OnInitBuild     += OnInitBuild;
			Server.OnHelpRequest   += OnHelpRequest;
			Server.OnStatusRequest += OnStatusRequest;
			Server.OnStop          += OnStop;
		}

		protected abstract void OnCommonError(string message, bool isFatal);
		
		protected string GetHelpMessage() {
			var sb = new StringBuilder();
			sb.Append("Commands:\n");
			sb.Append("- \"status\" - current server status\n");
			sb.Append("- \"stop\" - stop current build, if it is started\n");
			sb.Append("- \"build arg0 arg1 ... argN\" - start build with given parameters\n");
			sb.Append("- \"help\" - show this message\n");
			return sb.ToString();
		}

		protected abstract void OnHelpRequest();
		
		protected string GetStatusMessage() {
			var sb = new StringBuilder();
			sb.Append($"{Server.ServiceName}\n");
			sb.Append($"Is busy: {Process != null}\n");
			var curTask = Process?.CurrentTask;
			if (curTask != null) {
				var allTasks = Process.Tasks;
				var curTaskName = curTask.Node.Name;
				var taskIndex = allTasks.IndexOf(curTask);
				var totalTasks = allTasks.Count;
				sb.Append($"Task: {curTaskName} ({taskIndex}/{totalTasks})\n");
			}
			sb.Append("Services:\n");
			foreach (var service in Server.Services) {
				sb.Append($"- {service.GetType().Name}\n");
			}
			sb.Append("Builds:\n");
			foreach (var build in Server.Builds) {
				sb.Append($"- {build.Key}");
				var args = build.Value.Args;
				if (args.Count > 0) {
					sb.Append(" (");
					for (var i = 0; i < args.Count; i++) {
						var arg = build.Value.Args[i];
						sb.Append(arg);
						if (i < args.Count - 1) {
							sb.Append("; ");
						}
					}
					sb.Append(")");
				}
				sb.Append("\n");
			}
			return sb.ToString();
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
			var sb = new StringBuilder();
			var args = Server.FindCurrentBuildArgs();
			if ((args == null) || (args.Count <= 0)) {
				return "";
			}
			sb.Append("(");
			foreach (var arg in args) {
				sb.Append($"{arg.Key}: {arg.Value}, ");
			}
			var msg = sb.ToString().Substring(0, sb.Length- 2);
			msg += ")";
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