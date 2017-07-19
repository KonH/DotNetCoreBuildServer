using System.Collections.Generic;
using Server;

namespace ConsoleClient {
	class Program {
		static void Main(string[] args) {
			var server = new BuildServer();
			var commands = new BuildCommand[] {
				new BuildCommand("print", new Dictionary<string, string>(){{"message", "test"}}), 
			};
			var buildConfig = new BuildConfig("test", commands);
			var result = server.InitBuild(buildConfig);
			var writer = new BuildResultWriter(result);
			server.StartBuild();
		}
	}
}