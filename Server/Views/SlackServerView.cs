using Server.Integrations;
using Server.Runtime;

namespace Server.Views {
	public class SlackServerView:BaseServerView {
		
		readonly SlackService _service = null;

		public SlackServerView(SlackService service, BuildServer server) : base(server) {
			_service = service;
		}
		
		protected override void OnStatusRequest() {
			_service.SendMessage(GetStatusMessage());
		}
		
		protected override void OnBuildProcessStarted() {
			_service.SendMessage($"Build started: {Process.Name}");
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
			_service.SendMessage(message);
			base.OnBuildProcessDone();
		}
	}
}