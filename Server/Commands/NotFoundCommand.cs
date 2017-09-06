using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Server.Commands {
	public class NotFoundCommand:ICommand {
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			return CommandResult.Fail("Command not found!");
		}
	}
}