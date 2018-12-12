using System.Linq;
using System.Text;
using Server.Services;
using Server.Runtime;
using Microsoft.Extensions.Logging;

namespace Server.Views {
	public class SlackServerView:BaseServerView {
		
		readonly SlackService  _service;

		public SlackServerView(LoggerFactory loggerFactory, SlackService service, BuildServer server, MessageFormat messageFormat) : 
			base(loggerFactory, service.Context, server, messageFormat) {
			_service       = service;
		}

		protected override void OnCommonError(string message, bool isFatal) {
			if ( isFatal ) {
				_service.SendMessage($":bangbang::skull: *Fatal error:* {message}");
			} else {
				_service.SendMessage($":exclamation: *Error:* {message}");
			}
		}

		protected override void OnCommonMessage(string message) {
			_service.SendMessage($"```{message}```");
		}

		protected override void OnHelpRequest(string arg) {
			_service.SendMessage(GetHelpMessage(arg));
		}
		
		protected override void OnStatusRequest() {
			_service.SendMessage(GetStatusMessage());
		}

		protected override string GetBuildProcessStartMessage() {
			return $":arrow_forward: *Build task started:* {Process.Name} {GetBuildArgsMessage()}\n";
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
			sb.Append($":black_square_for_stop: *Build task ended:* {Process.Name} {GetBuildArgsMessage()}\n");
			if ( Process.IsSuccess ) {
				sb.Append(":heavy_check_mark: Result: *Success*\n");
			} else {
				sb.Append(":x: Result: *Fail*\n");
			}
			sb.Append($":stopwatch: Elapsed time: {Utils.FormatTimeSpan(Process.WorkTime)}\n");
			var lastTask = Process.Tasks.Last();
			if (lastTask.IsSuccess) {
				sb.Append(":memo: Last task message:\n```\n");
				sb.Append(lastTask.Message);
				sb.Append("\n```");
			} else {
				sb.Append("```\n");
				sb.Append(GetFailMessage());
				sb.Append("```\n");
			}
			_service.SendMessage(sb.ToString());
			base.OnBuildProcessDone();
		}
	}
}