using System;
using Server.Integrations;
using Server.Runtime;

namespace Server.Views {
	public class SlackServerView:BaseServerView {
		
		readonly SlackService _service = null;

		public SlackServerView(SlackService service, BuildServer server) : base(server) {
			_service = service;
		}

		protected override void OnHelpRequest() {
			_service.SendMessage(GetHelpMessage());
		}
		
		protected override void OnStatusRequest() {
			_service.SendMessage(GetStatusMessage());
		}
		
		protected override void OnBuildProcessStarted() {
			_service.SendMessage(GetBuildProcessStartMessage());
		}

		protected override void OnTaskStarted(BuildTask buildTask) {}

		protected override void OnTaskDone(BuildTask buildTask) {}

		protected override void OnBuildProcessDone() {
			var message = $"Build done: {Process.Name} {GetBuildArgsMessage()}\n"; 
			message += $"(success: {Process.IsSuccess}) for {Process.WorkTime}\n";
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