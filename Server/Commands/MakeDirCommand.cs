using System;
using System.IO;
using System.Collections.Generic;

namespace Server.Commands {
	[CommandAttribute("make_dir")]
	public class MakeDirCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			if (Directory.Exists(path)) {
				return CommandResult.Success($"Directory at \"{path}\" aleady exist");
			}

			try {
				Directory.CreateDirectory(path);
				return CommandResult.Success($"Directory at \"{path}\" created");
			} catch (Exception e) {
				return CommandResult.Fail($"Failed to create directory at \"{path}\": \"{e}\"");
			}
		}
	}
}