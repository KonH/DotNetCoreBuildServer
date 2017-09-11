using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.BuildConfig;
using Server.Runtime;
using Microsoft.Extensions.Logging;
using System;

namespace Server.Views {
	public abstract class BaseServerView {

		public bool Alive => Server != null;
		
		protected BuildServer  Server;
		protected BuildProcess Process;

		ILogger _logger;

		protected BaseServerView(LoggerFactory loggerFactory, BuildServer server) {
			_logger = loggerFactory.CreateLogger<BaseServerView>();
			Server = server;
			Server.OnCommonError   += OnCommonError;
			Server.OnCommonMessage += OnCommonMessage;
			Server.OnInitBuild     += OnInitBuild;
			Server.OnHelpRequest   += OnHelpRequest;
			Server.OnStatusRequest += OnStatusRequest;
			Server.OnStop          += OnStop;
			Server.LogFileChanged  += OnLogFileChanged;
		}

		protected abstract void OnCommonError(string message, bool isFatal);
		protected abstract void OnCommonMessage(string message);

		protected string GetHelpMessage() {
			var sb = new StringBuilder();
			sb.Append("Commands:\n");
			foreach ( var handler in Server.Commands ) {
				sb.Append($"- \"{handler.Key}\" - {handler.Value.Description}\n");
			}
			return sb.ToString();
		}

		protected abstract void OnHelpRequest();
		
		protected string GetStatusMessage() {
			var sb = new StringBuilder();
			sb.Append($"{Server.Name} ({Server.ServiceName})\n");
			sb.Append($"Is busy: {Process != null}\n");
			var curTask = Process?.CurrentTask;
			if (curTask != null) {
				var allTasks = Process.Tasks;
				var curTaskName = curTask.Node.Name;
				var taskIndex = allTasks.IndexOf(curTask);
				var totalTasks = allTasks.Count;
				sb.Append($"Task: {curTaskName} ({taskIndex}/{totalTasks}) started: {curTask.StartTime}, duration: {DateTime.Now - curTask.StartTime}\n");
			}
			sb.Append("Services:\n");
			foreach (var service in Server.Services) {
				sb.Append($"- {service.GetType().Name}\n");
			}
			sb.Append("Builds:\n");
			var builds = Server.FindBuilds();
			AppendBuildsInfo(sb, builds);
			return sb.ToString();
		}

		void AppendBuildsInfo(StringBuilder sb, Dictionary<string, Build> builds) {
			if (builds == null) {
				sb.Append("Failed to load builds directory!\n");
				return;
			}
			if (builds.Count == 0) {
				sb.Append("No builds found!\n");
				return;
			}
			foreach (var build in builds) {
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
		}
		
		protected abstract void OnStatusRequest();
		
		void OnInitBuild(BuildProcess process) {
			Process = process;
			Process.BuildStarted += OnBuildProcessStarted;
			Process.TaskStarted  += OnTaskStarted;
			Process.TaskDone     += OnTaskDone;
			Process.BuildDone    += OnBuildProcessDone;
			_logger.LogDebug($"OnInitBuild: {process.Name}");
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
			_logger.LogDebug($"OnBuildProcessDone: {Process.Name}");
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
			return tasks.Aggregate("", (current, task) => current + $"{GetTaskInfo(task)}\n");
		}

		protected virtual void OnLogFileChanged(string logFile) { }
	}
}