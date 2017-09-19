using Microsoft.Extensions.Logging;
using Server.Runtime;

namespace Server.Controllers {
	public class ConsoleServerController:BaseServerController {
		
		public ConsoleServerController(LoggerFactory loggerFactory, RequestContext context, BuildServer server):base(loggerFactory, context, server) { }
		
		public void SendRequest(RequestContext context, string message) {
			var request = ConvertMessage(context, message);
			Call(request);
		}
	}
}