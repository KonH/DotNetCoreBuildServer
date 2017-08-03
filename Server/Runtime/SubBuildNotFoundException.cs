using System;

namespace Server.Runtime {
	public class SubBuildNotFoundException : Exception {
		public SubBuildNotFoundException(string subBuildName):base($"Sub-build not found: \"{subBuildName}\"") { }
	}
}