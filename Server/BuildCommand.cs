using System.Collections.Generic;

namespace Server {
	public class BuildCommand {
		
		public string                     Name { get; }
		public Dictionary<string, string> Args { get; }

		public BuildCommand(string name, Dictionary<string, string> args) {
			Name = name;
			Args = args;
		}
	}
}