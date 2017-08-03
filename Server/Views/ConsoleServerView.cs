using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Server.Runtime;

namespace Server.Views {
	public class ConsoleServerView:BaseServerView {

		public string LogFile { get; set; }
		
		public ConsoleServerView(BuildServer server) : base(server) { }

		void WriteLine(string message = "") {
			Console.WriteLine(message);
			try {
				if (!string.IsNullOrEmpty(LogFile)) {
					File.AppendAllText(LogFile, message + '\n');
				}
			}
			catch (Exception e) {
				Debug.WriteLine($"ConsoleServerView.WriteLine: write to log at \"{LogFile}\" failed: \"{e}\"");
			}
		}
		
		protected override void OnCommonError(string message, bool isFatal) {
			WriteLine($"Error: {message}, isFatal: {isFatal}");
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