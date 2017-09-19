using System;
using System.Collections.Generic;
using System.Linq;
using Server.Services;
using Server.Runtime;
using Microsoft.Extensions.Logging;
using Server.Commands;

namespace ConsoleClient {
	class Program {

		static bool WithConsoleLog = false;

		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("You need to provide serverName and at least one config path!");
				Console.WriteLine("Closing...");
				return;
			}

			var loggerFactory = new LoggerFactory();
			if ( WithConsoleLog ) {
				loggerFactory.AddConsole(CustomLogFilter);
			}
			loggerFactory.AddFile("log.txt", false);

			var serverName = args[0];
			var serverArgs = args.Skip(1).ToArray();
			var consoleService = new ConsoleService(loggerFactory);
			var services = new List<IService> {
				consoleService,
				new SlackService(loggerFactory),
				new StatService("stats.xml" , loggerFactory, true)
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