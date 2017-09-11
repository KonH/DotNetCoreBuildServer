using System;
using Server.BuildConfig;
using Server.Controllers;
using Server.Runtime;
using Server.Views;
using Microsoft.Extensions.Logging;

namespace Server.Services {
	public class ConsoleService:IService {

		LoggerFactory _loggerFactory;

		ConsoleServerController _controller;
		ConsoleServerView       _view;
		
		public ConsoleService(LoggerFactory loggerFactory) {
			_loggerFactory = loggerFactory;
		}

		public bool TryInit(BuildServer server, Project project) {
			_controller = new ConsoleServerController(_loggerFactory, server);
			_view       = new ConsoleServerView(_loggerFactory, server);
			_loggerFactory.CreateLogger<ConsoleService>().LogDebug("ConsoleService: initialized");
			return true;
		}

		public void Process() {
			while (_view.Alive) {
				_controller.SendRequest(Console.ReadLine());
			}
		}
	}
}