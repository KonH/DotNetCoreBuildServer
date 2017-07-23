using System;
using System.Linq;
using Server.Runtime;

namespace Server.Views {
	public class ConsoleServerView:BaseServerView {
		
		public ConsoleServerView(BuildServer server) : base(server) { }

		protected override void OnCommonError(string message) {
			Console.WriteLine($"Error: {message}");
		}
		
		protected override void OnStatusRequest() {
			Console.Write(GetStatusMessage());
		}

		protected override void OnHelpRequest() {
			Console.WriteLine();
			Console.WriteLine(GetHelpMessage());
		}
		
		protected override void OnBuildProcessStarted() {
			Console.WriteLine();
			Console.WriteLine(GetBuildProcessStartMessage());
		}
		
		protected override void OnTaskStarted(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task started: {buildTask.Node.Name}");
		}
		
		protected override void OnTaskDone(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task done: {buildTask.Node.Name}");
			Console.WriteLine(GetTaskInfo(buildTask));
		}

		protected override void OnBuildProcessDone() {
			Console.WriteLine();
			Console.WriteLine($"Build done: {Process.Name} {GetBuildArgsMessage()}");
			Console.WriteLine($"(success: {Process.IsSuccess}) for {Process.WorkTime}");
			var lastTask = Process.Tasks.Last();
			if (lastTask.IsSuccess) {
				Console.WriteLine("Last task message:");
				Console.WriteLine(lastTask.Message);
			} else {
				Console.WriteLine(GetTasksInfo(Process.Tasks));
			}
			base.OnBuildProcessDone();
		}
	}
}