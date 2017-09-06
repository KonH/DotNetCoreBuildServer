using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Server.Commands {
	public interface ICommand {
		CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args);
	}
}