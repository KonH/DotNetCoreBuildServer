using System;
using System.Diagnostics;
using System.Linq;
using Server.Integrations;
using Server.Runtime;

namespace Server.Views {
	public class SlackServerView:BaseServerView {
		
		readonly SlackService _service = null;

		public SlackServerView(SlackService service, BuildServer server) : base(server) {
			_service = service;
		}

		protected override void OnCommonError(string message) {
			_service.SendMessage($"Error: {message}");
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
			var lastTask = Process.Tasks.Last();
			if (lastTask.IsSuccess) {
				message += "Last task message:\n```\n";
				message += lastTask.Message;
				message += "\n```";
			} else {
				message += "```\n";
				message += GetTasksInfo(Process.Tasks);
				message += "```\n";
			}
			_service.SendMessage(message);
			base.OnBuildProcessDone();
		}
	}
}