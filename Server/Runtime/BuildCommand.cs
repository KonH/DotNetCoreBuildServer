using System;

namespace Server.Runtime
{
	public class BuildCommand {
		public object              Target      { get; private set; }
		public string              Description { get; private set; }
		public Action<RequestArgs> Handler     { get; private set; }

		public BuildCommand(object target, string description, Action<RequestArgs> handler) {
			Target      = target;
			Description = description;
			Handler     = handler;
		}
	}
}
