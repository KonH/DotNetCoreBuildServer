using System;
using System.IO;
using System.Collections.Generic;

namespace Server.Commands {
	[CommandAttribute("delete_dir")]
	public class DeleteDirCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			var recursive = args.Get("recursive");
			try {
				var recursiveValue = !string.IsNullOrEmpty(recursive) && bool.Parse(recursive);
				Directory.Delete(path, recursiveValue);
				return CommandResult.Success($"Directory \"{path}\" deleted.");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't delete directory at \"{path}\": \"{e}\"");
			}
		}
	}
}