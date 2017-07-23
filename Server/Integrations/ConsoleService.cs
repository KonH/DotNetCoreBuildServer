using System;
using System.Diagnostics;
using Server.BuildConfig;
using Server.Controllers;
using Server.Runtime;
using Server.Views;

namespace Server.Integrations {
	public class ConsoleService:IService {

		ConsoleServerController _controller = null;
		ConsoleServerView       _view       = null;
		
		public bool TryInit(BuildServer server, Project project) {
			_controller = new ConsoleServerController(server);
			_view       = new ConsoleServerView(server);
			Debug.WriteLine("ConsoleService: initialized");
			return true;
		}

		public void Process() {
			while (_view.Alive) {
				_controller.SendRequest(Console.ReadLine());
			}
		}
	}
}