using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Server.Commands {
	[Command("print")]
	public class PrintCommand:ICommand {
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var message = args.Get("message");
			if (string.IsNullOrEmpty(message)) {
				return CommandResult.Fail("No message provided!");
			}
			return CommandResult.Success(message);
		}
	}
}