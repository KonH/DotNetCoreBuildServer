using System.Collections.Generic;
using System.IO;

namespace Server.Commands {
	public class CheckDirExistCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string path = null;
			args.TryGetValue("path", out path);
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			return 
				Directory.Exists(path) ?
				CommandResult.Success($"Directory \"{path}\" is exists") :
				CommandResult.Fail($"Directory \"{path}\" does not exists!");
		}
	}
}