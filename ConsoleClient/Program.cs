﻿using System.Collections.Generic;
using Server;
using Server.BuildConfig;

namespace ConsoleClient {
	class Program {
		static void Main(string[] args) {
			var server = new BuildServer();
			var nodes = new BuildNode[] {
				new BuildNode("print", new Dictionary<string, string>(){{"message", "test"}}),
				new BuildNode("check_dir_exist", new Dictionary<string, string>(){{"path", "{root}"}}) 
			};
			var buildConfig = new Build("test", nodes);
			var result = server.InitBuild(buildConfig);
			var writer = new BuildResultWriter(result);
			server.StartBuild();
		}
	}
}