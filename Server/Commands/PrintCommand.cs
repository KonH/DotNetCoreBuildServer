using System;
using System.Collections.Generic;

namespace Server.Commands {
	public class PrintCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string message = null;
			args.TryGetValue("message", out message);
			if (string.IsNullOrEmpty(message)) {
				return CommandResult.Fail("No message provided!");
			}
			return CommandResult.Success(message);
		}
	}
}