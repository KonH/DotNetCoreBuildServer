using System;
using System.Linq;
using Server.Manager;
using Server.Runtime;

namespace ConsoleClient {
	class Program {
		
		static void Main(string[] args) {
			var server = new BuildServer("project.json");
			server.OnInitBuild += OnBuildInited;
			var serverManager = new DirectServerManager(server);
			while (serverManager.Alive) {
				var line = Console.ReadLine();
				var allParts = line.Split(' ');
				if (allParts.Length > 0) {
					var request = allParts[0];
					var requestArgs = allParts.Skip(1).ToList();
					serverManager.SendRequest(request, requestArgs.ToArray());
				}
			}
		}

		static void OnBuildInited(BuildProcess build) {
			var writer = new BuildResultWriter(build);
		}
	}
}