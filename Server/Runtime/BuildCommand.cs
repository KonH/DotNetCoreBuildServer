using System;

namespace Server.Runtime
{
	public class BuildCommand {
		public string              Description { get; private set; }
		public Action<RequestArgs> Handler     { get; private set; }

		public BuildCommand(string description, Action<RequestArgs> handler) {
			Description = description;
			Handler     = handler;
		}
	}
}
