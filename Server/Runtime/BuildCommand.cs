using System;

namespace Server.Runtime
{
	public class BuildCommand {
		public string                              Description { get; private set; }
		public Action<RequestContext, RequestArgs> Handler     { get; private set; }

		public BuildCommand(string description, Action<RequestContext, RequestArgs> handler) {
			Description = description;
			Handler     = handler;
		}
	}
}
