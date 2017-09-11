using System;
using System.IO;
using System.Linq;
using Server.Runtime;
using Microsoft.Extensions.Logging;

namespace Server.Views {
	public class ConsoleServerView:BaseServerView {

		public string LogFile { get; set; }

		ILogger _logger;

		public ConsoleServerView(LoggerFactory loggerFactory, BuildServer server) : base(loggerFactory, server) {
			_logger = loggerFactory.CreateLogger<ConsoleServerView>();
		}

		void WriteLine(string message = "") {
			Console.WriteLine(message);
			try {
				if (!string.IsNullOrEmpty(LogFile)) {
					File.AppendAllText(LogFile, message + '\n');
				}
			}
			catch (Exception e) {
				_logger.LogDebug($"ConsoleServerView.WriteLine: write to log at \"{LogFile}\" failed: \"{e}\"");
			}
		}
		
		protected override void OnCommonError(string message, bool isFatal) {
			WriteLine($"Error: {message}, isFatal: {isFatal}");
		}

		protected override void OnCommonMessage(string message) {
			WriteLine(message);
		}

		protected override void OnStatusRequest() {
			WriteLine(GetStatusMessage());
		}

		protected override void OnHelpRequest() {
			WriteLine();
			WriteLine(GetHelpMessage());
		}
		
		protected override void OnBuildProcessStarted() {
			WriteLine();
			WriteLine(GetBuildProcessStartMessage());
		}
		
		protected override void OnTaskStarted(BuildTask buildTask) {
			WriteLine();
			WriteLine($"Task started: {buildTask.Node.Name}");
		}
		
		protected override void OnTaskDone(BuildTask buildTask) {
			WriteLine();
			WriteLine($"Task done: {buildTask.Node.Name}");
			WriteLine(GetTaskInfo(buildTask));
		}

		protected override void OnBuildProcessDone() {
			if (Process.Silent) {
				return;
			}
			WriteLine();
			WriteLine($"Build done: {Process.Name} {GetBuildArgsMessage()}");
			WriteLine($"(success: {Process.IsSuccess}) for {Process.WorkTime}");
			var lastTask = Process.Tasks.Last();
			if (lastTask.IsSuccess) {
				WriteLine("Last task message:");
				WriteLine(lastTask.Message);
			} else {
				WriteLine(GetTasksInfo(Process.Tasks));
			}
			base.OnBuildProcessDone();
		}

		protected override void OnLogFileChanged(string logFile) {
			LogFile = logFile;
		}
	}
}