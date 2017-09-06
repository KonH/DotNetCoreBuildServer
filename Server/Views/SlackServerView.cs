using System.Linq;
using System.Text;
using Server.Integrations;
using Server.Runtime;
using Microsoft.Extensions.Logging;

namespace Server.Views {
	public class SlackServerView:BaseServerView {
		
		readonly SlackService _service;

		public SlackServerView(LoggerFactory loggerFactory, SlackService service, BuildServer server) : base(loggerFactory, server) {
			_service = service;
		}

		protected override void OnCommonError(string message, bool isFatal) {
			if (isFatal) {
				_service.SendMessage($"Error: {message}");
			}
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
			if (Process.Silent) {
				return;
			}
			var sb = new StringBuilder();
			sb.Append($"Build done: {Process.Name} {GetBuildArgsMessage()}\n"); 
			sb.Append($"(success: {Process.IsSuccess}) for {Process.WorkTime}\n");
			var lastTask = Process.Tasks.Last();
			if (lastTask.IsSuccess) {
				sb.Append("Last task message:\n```\n");
				sb.Append(lastTask.Message);
				sb.Append("\n```");
			} else {
				sb.Append("```\n");
				sb.Append(GetTasksInfo(Process.Tasks));
				sb.Append("```\n");
			}
			_service.SendMessage(sb.ToString());
			base.OnBuildProcessDone();
		}
	}
}