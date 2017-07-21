using System.Collections.Generic;
using System.Linq;
using Server.Integrations;
using Server.Runtime;

namespace Server.Manager {
	public class SlackServerManager:ServerManager {

		readonly SlackManager _manager = null;

		public SlackServerManager(SlackManager manager, BuildServer server) : base(server) {
			_manager = manager;
			_manager.OnMessage += OnSlackMessage;
		}

		void OnSlackMessage(string message) {
			var parts = message.Split(' ');
			if (parts.Length > 1) {
				var request = parts[1];
				var args = parts.Skip(2).ToArray();
				Call(request, args);
			}
		}
		
		protected override void ProcessStatus(Dictionary<string, string> builds) {
			var msg = "Builds: ";
			foreach (var build in builds) {
				msg += build.Key + "; ";
			}
			_manager.SendMessage(msg);
		}
	}
}