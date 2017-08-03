using System;

namespace Server.Commands {
	public class CommandNotFoundException : Exception {
		public CommandNotFoundException(string commandName):base($"Unknown command: \"{commandName}\"") { }
	}
}