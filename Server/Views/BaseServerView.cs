using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.BuildConfig;
using Server.Runtime;
using Microsoft.Extensions.Logging;
using System;
using Server.Services;

namespace Server.Views {
	public abstract class BaseServerView {

		class ContextWrapper {
			RequestContext _context;

			public ContextWrapper(RequestContext context) {
				_context = context;
			}

			void CallOnContext(RequestContext context, Action action) {
				if ( context.Name == _context.Name ) {
					action();
				}
			}

			public Action<RequestContext> Subscribe(Action action) {
				return (c) => CallOnContext(c, action);
			}

			public Action<RequestContext, T1> Subscribe<T1>(Action<T1> action) {
				return (c, a1) => CallOnContext(c, () => action(a1));
			}

			public Action<RequestContext, T1, T2> Subscribe<T1, T2>(Action<T1, T2> action) {
				return (c, a1, a2) => CallOnContext(c, () => action(a1, a2));
			}
		}

		public bool Alive => Server != null;
		
		protected BuildServer  Server;
		protected BuildProcess Process;

		ILogger       _logger;
		MessageFormat _messageFormat;

		protected BaseServerView(LoggerFactory loggerFactory, RequestContext context, BuildServer server, MessageFormat messageFormat) {
			_logger = loggerFactory.CreateLogger<BaseServerView>();
			_messageFormat = messageFormat;
			Server = server;
			var wrap = WithContext(context);
			Server.OnCommonMessage += wrap.Subscribe<string>(OnCommonMessage);
			Server.OnInitBuild     += wrap.Subscribe<BuildProcess>(OnInitBuild);
			Server.OnHelpRequest   += wrap.Subscribe<string>(OnHelpRequest);
			Server.OnStatusRequest += wrap.Subscribe(OnStatusRequest);

			Server.OnStop         += OnStop;
			Server.OnCommonError  += OnCommonError;
			Server.LogFileChanged += OnLogFileChanged;
		}

		ContextWrapper WithContext(RequestContext context) {
			return new ContextWrapper(context);
		}

		protected abstract void OnCommonError(string message, bool isFatal);
		protected abstract void OnCommonMessage(string message);

		protected string GetHelpMessage(string arg) {
			if ( string.IsNullOrEmpty(arg) ) {
				return GetHelpMessage();
			}
			var sb = new StringBuilder();
			var builds = Server.FindBuilds();
			Build buildInfo = null;
			if ( arg == "list" ) {
				GetBuildsList(sb);
			} else if ( builds.TryGetValue(arg, out buildInfo) ) {
				if ( string.IsNullOrEmpty(buildInfo.LongDescription) ) {
					sb.Append($"Help for '{arg}' not found.\n");
				} else {
					sb.Append($"*{buildInfo.Name}:*\n");
					sb.Append(buildInfo.LongDescription);
					sb.Append("\n");
					AppendBuildArgsInfo(sb, buildInfo);
				}
			} else {
				sb.Append($"Build task '{arg}' not found.\n");
			}
			return sb.ToString();
		}

		protected string GetHelpMessage() {
			var sb = new StringBuilder();
			sb.Append($":desktop_computer:*{Server.Name} ({Server.ServiceName})*\n");
			sb.Append(":diamonds:*Commands:*\n");
			foreach (var handler in Server.Commands.All) {
				sb.Append($"\t • \"{handler.Key}\" - {handler.Value.First().Description}\n");
			}
			sb.Append("\n");
			sb.Append(":diamonds:*Services:*\n");
			foreach (var service in Server.Services) {
				sb.Append($"\t • {service.GetType().Name}\n");
			}
			sb.Append("\n");
			sb.Append(":memo: To print a list of all available Build tasks, use 'help list'.\n");
			sb.Append(":memo: To get help on the Build task, enter 'help' and the task name separated by a space.'\n");
			return sb.ToString();
		}

		protected string GetBuildsList(StringBuilder sb) {
			sb.Append(":hammer_and_pick:*Builds tasks:*\n");
			var builds = Server.FindBuilds();
			AppendBuildsInfo(sb, builds);
			return sb.ToString();
		}

		protected abstract void OnHelpRequest(string arg);
		
		protected void AppendTaskInfo(BuildTask task, StringBuilder sb) {
			var allTasks = Process.Tasks;
			var curTaskName = task.Node.Name;
			var taskIndex = allTasks.IndexOf(task);
			var totalTasks = allTasks.Count;
			sb.AppendLine($":repeat: *Current task:* {curTaskName} ({taskIndex}/{totalTasks})\n:checkered_flag: Start time: {task.StartTime}, duration: {Utils.FormatTimeSpan(DateTime.Now - task.StartTime)}");
		}

		protected void AppendEstimateTime(StringBuilder sb) {
			var statService = Server.FindService<StatService>();
			if ( statService != null ) {
				if ( statService.HasStatistics(Process.Name)) {
					sb.Append(":hourglass: Estimated end time: ");
					sb.Append(statService.FindEstimateEndTime(Process.Name, Process.StartTime));
					sb.AppendLine();
				}
			}
		}

		protected string GetStatusMessage() {
			var sb = new StringBuilder();
			if ( Process == null ) {
				sb.Append("*Status:* Server is not busy :heavy_check_mark:\n");
			}
			var curTasks = Process?.CurrentTasks;
			if ((curTasks != null) && (curTasks.Count > 0)) {
				curTasks.ForEach(t => AppendTaskInfo(t, sb));
				AppendEstimateTime(sb);
			}
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
				sb.Append($"\t • {build.Key}");
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
				var desc = build.Value.ShortDescription;
				if ( !string.IsNullOrEmpty(desc) ) {
					sb.Append(" - ");
					sb.Append(desc);
				}
				sb.Append("\n");
			}
		}

		void AppendBuildArgsInfo(StringBuilder sb, Build build) {
			sb.Append("*Arguments list:*\n");
			if ( build.Args == null || build.Args.Count == 0 ) {
				sb.Append("No arguments.");
				return;
			}
			var haveDescriptions = build.ArgsDescription != null && build.ArgsDescription.Count > 0;
			int counter = 0;
			foreach (var arg in build.Args) {
				counter++;
				var printDescriptions = haveDescriptions && build.ArgsDescription.Count >= counter;
				if ( printDescriptions ) {
					sb.Append($"\t{counter}) {arg} : {build.ArgsDescription[counter - 1]}\n");
				} else {
					sb.Append($"\t{counter}) {arg}\n");
				}
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
		
		protected virtual string GetBuildProcessStartMessage() {
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

		protected string GetFailedTasksInfo(List<BuildTask> tasks) {
			return GetTasksInfo(tasks.Where(t => t.IsStarted && !t.IsSuccess).ToList());
		}

		protected virtual void OnLogFileChanged(string logFile) { }

		protected string GetFailMessage() {
			switch ( _messageFormat ) {
				case MessageFormat.FullFailMessage    : return GetTasksInfo(Process.Tasks);
				case MessageFormat.LastTaskFailMessage: return GetFailedTasksInfo(Process.Tasks);
				default                               : return string.Empty;
			}
		}
	}
}