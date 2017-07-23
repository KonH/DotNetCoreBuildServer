using System.Linq;
using Server.Integrations;
using Server.Runtime;

namespace ConsoleClient {
	class Program {
		
		static void Main(string[] args) {
			if (args.Length < 1) {
				return;
			}
			var serverName = args[0];
			var serverArgs = args.Skip(1).ToArray();
			var consoleService = new ConsoleService();
			var services = new IService[] {
				consoleService,
				new SlackService()
			};
			var server = new BuildServer(serverName, services, serverArgs);
			consoleService.Process();
		}
	}
}