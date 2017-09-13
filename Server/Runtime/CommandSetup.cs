using System;
using System.Collections.Generic;

namespace Server.Runtime {
	public class CommandSetup {
		public Dictionary<string, List<BuildCommand>> All => new Dictionary<string, List<BuildCommand>>(_commands);

		Dictionary<string, List<BuildCommand>>           _commands      = new Dictionary<string, List<BuildCommand>>();
		List<Tuple<object, Action<string, RequestArgs>>> _buildHandlers = new List<Tuple<object, Action<string, RequestArgs>>>();

		public void Add(string name, BuildCommand command) {
			if ( _commands.ContainsKey(name) ) {
				_commands[name].Add(command);
			} else {
				var commands = new List<BuildCommand>();
				commands.Add(command);
				_commands.Add(name, commands);
			}
		}

		public void AddBuildHandler(object target, Action<string, RequestArgs> handler) {
			_buildHandlers.Add(new Tuple<object, Action<string, RequestArgs>>(target, handler));
		}

		public void Call(string name, object caller, RequestArgs args) {
			var handlers = _commands.Get(name);
			if ( handlers != null ) {
				foreach ( var handler in handlers ) {
					if ( (handler.Target == caller) || (handler.Target == null) ) {
						handler.Handler.Invoke(args);
					}
				}
			}
		}

		public void CallBuildHandler(object caller, string buildName, RequestArgs args) {
			foreach ( var buildHandler in _buildHandlers ) {
				if ( buildHandler.Item1 == caller ) {
					buildHandler.Item2.Invoke(buildName, args);
				}
			}
		}
	}
}
