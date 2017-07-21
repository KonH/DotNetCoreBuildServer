using Server.Integrations;
using Server.Runtime;

namespace Server.Writers {
	public class SlackBuildWriter:BaseBuildWriter {

		readonly SlackManager _manager = null;

		public SlackBuildWriter(SlackManager manager, BuildProcess buildProcess) : base(buildProcess) {
			_manager = manager;
		}
		
		protected override void OnBuildProcessStarted() {
			_manager.SendMessage($"Build started: {Process.Name}");
		}

		protected override void OnTaskStarted(BuildTask buildTask) {}

		protected override void OnTaskDone(BuildTask buildTask) {}

		protected override void OnBuildProcessDone() {
			var message = $"Build done: {Process.Name} (success: {Process.IsSuccess})\n";
			message += "```\n";
			foreach (var task in Process.Tasks) {
				if (task.IsStarted) {
					message += $"{task.Node.Name} (success: {task.IsSuccess}, message: \"{task.Message}\")\n";
				} else {
					message += $"{task.Node.Name} (skip)\n";
				}
			}
			message += "```";
			_manager.SendMessage(message);
		}
	}
}