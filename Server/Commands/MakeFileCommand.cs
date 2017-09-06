using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Server.Commands {
	[Command("make_file")]
	public class MakeFileCommand:ICommand {
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			var content = args.Get("content");
			var alreadyExist = File.Exists(path);
			try {
				File.WriteAllText(path, content);
				return alreadyExist ? 
					CommandResult.Success($"File at \"{path}\" is exist, content is replaced") :
					CommandResult.Success($"File at \"{path}\" created");
			} catch (Exception e) {
				return CommandResult.Fail($"Failed to create file at \"{path}\": \"{e}\"");
			}
		}
	}
}