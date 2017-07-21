using Server.Runtime;

namespace Server.Manager {
	public class DirectServerManager:ServerManager {

		public DirectServerManager(BuildServer server) : base(server) { }
		
		public void SendRequest(string request, params string[] args) {
			Call(request, args);
		}
	}
}