using System.Collections.Generic;

namespace Server.Runtime {
	public class CommandSetup {
		public Dictionary<string, List<BuildCommand>> All => new Dictionary<string, List<BuildCommand>>(_commands);

		Dictionary<string, List<BuildCommand>> _commands = new Dictionary<string, List<BuildCommand>>();

		public void Add(string name, BuildCommand command) {
			if ( _commands.ContainsKey(name) ) {
				_commands[name].Add(command);
			} else {
				var commands = new List<BuildCommand>();
				commands.Add(command);
				_commands.Add(name, commands);
			}
		}

		public void Call(string name, object caller, RequestArgs args) {
			var handlers = _commands.Get(name);
			if ( handlers != null ) {
				foreach ( var handler in handlers ) {
					if ( handler.Target == caller ) {
						handler.Handler.Invoke(args);
					}
				}
			}
		}
	}
}
