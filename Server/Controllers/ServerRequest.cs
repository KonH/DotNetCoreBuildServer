using Server.Runtime;

namespace Server.Controllers {	
	public class ServerRequest {
		
		public string      Request { get; }
		public RequestArgs Args    { get; }

		public bool IsValid => !string.IsNullOrEmpty(Request);
		
		public ServerRequest(string request, RequestArgs args) {
			Request = request;
			Args    = args;
		}
		
		public static ServerRequest Empty => new ServerRequest(null, new RequestArgs());
	}
}