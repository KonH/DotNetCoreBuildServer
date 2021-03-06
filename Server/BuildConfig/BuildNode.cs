using System.Collections.Generic;

namespace Server.BuildConfig {
	public class BuildNode {		
		public string                     Name    { get; }
		public string                     Command { get; }
		public Dictionary<string, string> Args    { get; }

		public bool IsParallel {
			get {
				return Args.GetBoolean("parallel", false);
			}
		}

		public int ParallelQueue {
			get {
				return Args.GetInt("parallel_queue", 0);
			}
		}

		public BuildNode(string name, string command, Dictionary<string, string> args) {
			Name    = name;
			Command = command;
			Args    = args;
		}
	}
}