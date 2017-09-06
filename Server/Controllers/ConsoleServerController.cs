using Microsoft.Extensions.Logging;
using Server.Runtime;

namespace Server.Controllers {
	public class ConsoleServerController:BaseServerController {
		
		public ConsoleServerController(LoggerFactory loggerFactory, BuildServer server):base(loggerFactory, server) { }
		
		public void SendRequest(string message) {
			var request = ConvertMessage(message);
			Call(request);
		}
	}
}