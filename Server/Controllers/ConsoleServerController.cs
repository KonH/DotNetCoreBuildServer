using Server.Runtime;

namespace Server.Controllers {
	public class ConsoleServerController:BaseServerController {
		
		public ConsoleServerController(BuildServer server):base(server) { }
		
		public void SendRequest(string message) {
			var request = ConvertMessage(message);
			Call(request);
		}
	}
}