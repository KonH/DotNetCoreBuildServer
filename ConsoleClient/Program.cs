using System;
using System.Collections.Generic;
using Server.Services;
using Server.Runtime;
using Microsoft.Extensions.Logging;
using Server.Commands;

namespace ConsoleClient {
	class Program {
		static void Main(string[] args) {
			var serverName = EnvManager.FindArgumentValue("server");
			var configPathes = EnvManager.FindArgumentValues("config");
			if ( string.IsNullOrEmpty(serverName) || (configPathes.Count < 1) ) {
				Console.WriteLine("You need to provide serverName and at least one config path!");
				Console.WriteLine("Closing...");
				Console.ReadKey();
				return;
			}

			var loggerFactory = new LoggerFactory();
			if ( EnvManager.HasArgument("-console-log") ) {
				loggerFactory.AddConsole(CustomLogFilter);
			}
			loggerFactory.AddFile("log.txt", false);

			var consoleService = new ConsoleService(loggerFactory);

			var services = new List<IService> {
				consoleService,
				new SlackService(loggerFactory),
				new StatService("stats.xml" , loggerFactory, true)
			};
			services.TryAddNotificationService(loggerFactory);

			var commandFactory = new CommandFactory(loggerFactory);
			var server = new BuildServer(commandFactory, loggerFactory, serverName);
			string startUpError = null;
			if (!server.TryInitialize(out startUpError, services, configPathes)) {
				Console.WriteLine(startUpError);
				Console.WriteLine("Closing...");
				Console.ReadKey();
				return;
			}
			Console.WriteLine($"{server.ServiceName} started and ready to use.");
			consoleService.Process();
		}

		static bool CustomLogFilter(string category, LogLevel level) {
			if ( category.StartsWith("SlackBotNet") && (level < LogLevel.Warning) ) {
				return false;
			}
			return true;
		}
	}
}