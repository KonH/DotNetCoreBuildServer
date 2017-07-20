using System.Collections.Generic;

namespace Server.BuildConfig {
	public class BuildNode {
		
		public string                     Name    { get; }
		public string                     Command { get; }
		public Dictionary<string, string> Args    { get; }

		public BuildNode(string name, string command, Dictionary<string, string> args) {
			Name    = name;
			Command = command;
			Args    = args;
		}
	}
}