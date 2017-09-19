using System.Linq;
using Server.Runtime;
using Server.Services;
using Microsoft.Extensions.Logging;

namespace Server.Controllers {
	public class SlackServerController:BaseServerController {

		ILogger      _logger;
		SlackService _service;

		public SlackServerController(LoggerFactory loggerFactory, SlackService service, BuildServer server) : base(loggerFactory, service.Context, server) {
			_logger = loggerFactory.CreateLogger<SlackServerController>();
			_service = service;
			_service.OnMessage += OnSlackMessage;
		}

		void OnSlackMessage(string message) {
			_logger.LogDebug($"SlackServerController.OnSlackMessage: message: \"{message}\"");
			var parts = message.Split(' ');
			if (parts.Length > 1) {
				var actualMessage = string.Join(" ", parts.Skip(1).ToArray());
				_logger.LogDebug($"SlackServerController.OnSlackMessage: actualMessage: \"{actualMessage}\"");
				var request = ConvertMessage(Context, actualMessage);
				Call(request);
			}
		}
	}
}