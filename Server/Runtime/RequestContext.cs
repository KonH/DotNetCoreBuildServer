namespace Server.Runtime {
	public class RequestContext {
		public string Name { get; private set; }

		public RequestContext(string name) {
			Name = name;
		}
	}
}
