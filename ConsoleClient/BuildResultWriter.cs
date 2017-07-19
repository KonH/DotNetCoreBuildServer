using System;
using Server;

namespace ConsoleClient {
	public class BuildResultWriter {
		
		readonly Build _build = null;
		
		public BuildResultWriter(Build build) {
			_build = build;
			_build.BuildStarted += OnBuildStarted;
			_build.TaskStarted  += OnTaskStarted;
			_build.TaskDone     += OnTaskDone;
			_build.BuildDone    += OnBuildDone;
		}

		void OnBuildStarted() {
			Console.WriteLine();
			Console.WriteLine($"Build started: {_build.Name}");
		}
		
		void OnTaskStarted(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task started: {buildTask.Name}");
		}
		
		void OnTaskDone(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task done: {buildTask.Name}");
			Console.WriteLine($"(success: {buildTask.IsSuccess}, message: \"{buildTask.Message}\")");
		}

		void OnBuildDone() {
			Console.WriteLine();
			Console.WriteLine($"Build done: {_build.Name} (success: {_build.IsSuccess})");
			foreach (var task in _build.Tasks) {
				Console.WriteLine($"{task.Name} (success: {task.IsSuccess}, message: \"{task.Message}\")");	
			}
		}
	}
}