using System;

namespace Server.Commands {
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandAttribute:Attribute {
		
		public string Name { get; }

		public CommandAttribute(string name) {
			Name = name;
		}
	}
}