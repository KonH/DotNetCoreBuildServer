using System.Collections.Generic;

namespace Server.Commands {
	public class NotFoundCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			return CommandResult.Fail("Command not found!");
		}
	}
}