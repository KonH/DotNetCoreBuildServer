using System.Collections.Generic;

namespace Server.BuildConfig {
	public class SubBuildNode:BuildNode {
		public SubBuildNode(string buildName, Dictionary<string, string> args) : base(buildName, null, args) { }
	}
}