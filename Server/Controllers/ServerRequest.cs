using Server.Runtime;

namespace Server.Controllers {	
	public class ServerRequest {
		
		public string         Request { get; }
		public RequestContext Context { get; }
		public RequestArgs    Args    { get; }

		public bool IsValid => !string.IsNullOrEmpty(Request);
		
		public ServerRequest(string request, RequestContext context, RequestArgs args) {
			Request = request;
			Context = context;
			Args    = args;
		}
		
		public static ServerRequest Empty => new ServerRequest(null, null, new RequestArgs());
	}
}