using System;
using Server;
using Server.Runtime;

namespace ConsoleClient {
	public class BuildResultWriter {
		
		readonly BuildProcess _buildProcess = null;
		
		public BuildResultWriter(BuildProcess buildProcess) {
			_buildProcess = buildProcess;
			_buildProcess.BuildStarted += OnBuildProcessStarted;
			_buildProcess.TaskStarted  += OnTaskStarted;
			_buildProcess.TaskDone     += OnTaskDone;
			_buildProcess.BuildDone    += OnBuildProcessDone;
		}

		void OnBuildProcessStarted() {
			Console.WriteLine();
			Console.WriteLine($"Build started: {_buildProcess.Name}");
		}
		
		void OnTaskStarted(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task started: {buildTask.Node.Name}");
		}
		
		void OnTaskDone(BuildTask buildTask) {
			Console.WriteLine();
			Console.WriteLine($"Task done: {buildTask.Node.Name}");
			Console.WriteLine($"(success: {buildTask.IsSuccess}, message: \"{buildTask.Message}\")");
		}

		void OnBuildProcessDone() {
			Console.WriteLine();
			Console.WriteLine($"Build done: {_buildProcess.Name} (success: {_buildProcess.IsSuccess})");
			foreach (var task in _buildProcess.Tasks) {
				if (task.IsStarted) {
					Console.WriteLine($"{task.Node.Name} (success: {task.IsSuccess}, message: \"{task.Message}\")");
				} else {
					Console.WriteLine($"{task.Node.Name} (skip)");
				}
			}
		}
	}
}