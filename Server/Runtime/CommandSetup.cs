using System;
using System.Collections.Generic;

namespace Server.Runtime {
	public class CommandSetup {
		public Dictionary<string, List<BuildCommand>> All => new Dictionary<string, List<BuildCommand>>(_commands);

		Dictionary<string, List<BuildCommand>>            _commands      = new Dictionary<string, List<BuildCommand>>();
		List<Action<string, RequestContext, RequestArgs>> _buildHandlers = new List<Action<string, RequestContext, RequestArgs>>();

		public void Add(string name, BuildCommand command) {
			if ( _commands.ContainsKey(name) ) {
				_commands[name].Add(command);
			} else {
				var commands = new List<BuildCommand>();
				commands.Add(command);
				_commands.Add(name, commands);
			}
		}

		public void AddBuildHandler(Action<string, RequestContext, RequestArgs> handler) {
			_buildHandlers.Add(handler);
		}

		public void Call(string name, RequestContext context, RequestArgs args) {
			var handlers = _commands.Get(name);
			if ( handlers != null ) {
				foreach ( var handler in handlers ) {
					handler.Handler.Invoke(context, args);
				}
			}
		}

		public void CallBuildHandler(string buildName, RequestContext context, RequestArgs args) {
			foreach ( var buildHandler in _buildHandlers ) {
				buildHandler.Invoke(buildName, context, args);
			}
		}
	}
}
