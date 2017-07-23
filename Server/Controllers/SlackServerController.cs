using System.Linq;
using Server.Runtime;
using Server.Integrations;

namespace Server.Controllers {
	public class SlackServerController:BaseServerController {
		
		public SlackServerController(SlackService service, BuildServer server) : base(server) {
			service.OnMessage += OnSlackMessage;
		}

		void OnSlackMessage(string message) {
			var parts = message.Split(' ');
			if (parts.Length > 1) {
				var actualMessage = string.Join(" ", parts.Skip(1).ToArray());
				var request = ConvertMessage(actualMessage);
				Call(request);
			}
		}
	}
}