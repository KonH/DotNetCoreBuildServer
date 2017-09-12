using System;
using System.Collections.Generic;
using System.Linq;
using Server.Services;
using Server.Runtime;
using Microsoft.Extensions.Logging;
using Server.Commands;

namespace ConsoleClient {
	class Program {
		
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("You need to provide serverName and at least one config path!");
				Console.WriteLine("Closing...");
				return;
			}

			var loggerFactory = new LoggerFactory();
			loggerFactory.AddConsole(CustomLogFilter);
			loggerFactory.AddFile("log.txt");

			var logger = loggerFactory.CreateLogger<Program>();
			for ( int i = 0; i < 5; i++ ) {
				logger.LogDebug("* * * * * * * * * *");
			}
			logger.LogDebug("Start session");

			var serverName = args[0];
			var serverArgs = args.Skip(1).ToArray();
			var consoleService = new ConsoleService(loggerFactory);
			var services = new List<IService> {
				consoleService,
				new SlackService(loggerFactory),
				new StatService("stats.xml" , loggerFactory)
			};
			var commandFactory = new CommandFactory(loggerFactory);
			var server = new BuildServer(commandFactory, loggerFactory, serverName);
			string startUpError = null;
			if (!server.TryInitialize(out startUpError, services, serverArgs)) {
				Console.WriteLine(startUpError);
				Console.WriteLine("Closing...");
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