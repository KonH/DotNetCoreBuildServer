using System;
using System.Linq;
using Server.Integrations;
using Server.Manager;
using Server.Runtime;
using Server.Writers;

namespace ConsoleClient {
	class Program {

		static SlackManager _slackManager = null;
		
		static void Main(string[] args) {
			var server = new BuildServer("project.json");//, "project_private.json");
			_slackManager = server.AddSlackManager();
			server.OnInitBuild += OnBuildInited;
			if (_slackManager != null) {
				var slackServerManager = new SlackServerManager(_slackManager, server);
			}
			var directServerManager = new DirectServerManager(server);
			while (directServerManager.Alive) {
				directServerManager.SendRequest(Console.ReadLine());
			}
		}

		static void OnBuildInited(BuildProcess build) {
			var consoleWriter = new ConsoleBuildWriter(build);
			if (_slackManager != null) {
				var slackWriter = new SlackBuildWriter(_slackManager, build);
			}
		}
	}
}