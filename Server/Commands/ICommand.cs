using System.Collections.Generic;

namespace Server.Commands {
	public interface ICommand {
		CommandResult Execute(Dictionary<string, string> args);
	}
}