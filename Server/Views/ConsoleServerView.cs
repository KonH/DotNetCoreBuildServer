using System;
using Server.Runtime;

namespace Server.Views {
	public class ConsoleServerView:BaseServerView {
		
		public ConsoleServerView(BuildServer server) : base(server) { }

		protected override void OnStatusRequest() {
			Console.Write(GetStatusMessage());
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
			Console.WriteLine($"(success: {buildTask.IsSuccess}, message: \"{buildTask.Message}\")");
		}

		protected override void OnBuildProcessDone() {
			Console.WriteLine();
			Console.WriteLine($"Build done: {Process.Name} {GetBuildArgsMessage()}");
			Console.WriteLine($"(success: {Process.IsSuccess}) for {Process.WorkTime}");
			foreach (var task in Process.Tasks) {
				if (task.IsStarted) {
					Console.WriteLine($"{task.Node.Name} (success: {task.IsSuccess}, message: \"{task.Message}\")");
				} else {
					Console.WriteLine($"{task.Node.Name} (skip)");
				}
			}
			base.OnBuildProcessDone();
		}
	}
}