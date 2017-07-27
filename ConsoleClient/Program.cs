using System;
using System.Collections.Generic;
using System.Linq;
using Server.Integrations;
using Server.Runtime;

namespace ConsoleClient {
	class Program {
		
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("You need to provide serverName and at least one config path!");
				Console.WriteLine("Closing...");
				return;
			}
			var serverName = args[0];
			var serverArgs = args.Skip(1).ToArray();
			var consoleService = new ConsoleService();
			var services = new List<IService> {
				consoleService,
				new SlackService()
			};
			var server = new BuildServer(serverName);
			string startUpError = null;
			if (!server.TryInitialize(out startUpError, services, serverArgs)) {
				Console.WriteLine(startUpError);
				Console.WriteLine("Closing...");
				return;
			}
			Console.WriteLine($"{server.ServiceName} started and ready to use.");
			consoleService.Process();
		}
	}
}