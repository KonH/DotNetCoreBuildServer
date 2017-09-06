using System.Linq;
using Server.Runtime;
using Server.Integrations;
using Microsoft.Extensions.Logging;

namespace Server.Controllers {
	public class SlackServerController:BaseServerController {

		ILogger _logger;

		public SlackServerController(LoggerFactory loggerFactory, SlackService service, BuildServer server) : base(loggerFactory, server) {
			_logger = loggerFactory.CreateLogger<SlackServerController>();
			service.OnMessage += OnSlackMessage;
		}

		void OnSlackMessage(string message) {
			_logger.LogDebug($"SlackServerController.OnSlackMessage: message: \"{message}\"");
			var parts = message.Split(' ');
			if (parts.Length > 1) {
				var actualMessage = string.Join(" ", parts.Skip(1).ToArray());
				_logger.LogDebug($"SlackServerController.OnSlackMessage: actualMessage: \"{actualMessage}\"");
				var request = ConvertMessage(actualMessage);
				Call(request);
			}
		}
	}
}