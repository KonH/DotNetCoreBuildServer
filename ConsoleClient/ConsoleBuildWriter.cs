using System;
using Server.Runtime;
using Server.Writers;

namespace ConsoleClient {
	public class ConsoleBuildWriter:BaseBuildWriter {
		
		public ConsoleBuildWriter(BuildProcess buildProcess):base(buildProcess) { }

		protected override void OnBuildProcessStarted() {
			Console.WriteLine();
			Console.WriteLine($"Build started: {Process.Name}");
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
			Console.WriteLine($"Build done: {Process.Name} (success: {Process.IsSuccess})");
			foreach (var task in Process.Tasks) {
				if (task.IsStarted) {
					Console.WriteLine($"{task.Node.Name} (success: {task.IsSuccess}, message: \"{task.Message}\")");
				} else {
					Console.WriteLine($"{task.Node.Name} (skip)");
				}
			}
		}
	}
}