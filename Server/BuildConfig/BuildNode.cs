using System.Collections.Generic;

namespace Server.BuildConfig {
	public class BuildNode {
		
		public string                     Name { get; }
		public Dictionary<string, string> Args { get; }

		public BuildNode(string name, Dictionary<string, string> args) {
			Name = name;
			Args = args;
		}
	}
}