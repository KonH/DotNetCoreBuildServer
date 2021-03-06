using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Commands {
	[Command("delete_file")]
	public class DeleteFileCommand:ICommand {
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			var ifExist = args.Get("if_exist");
			try {
				var ifExistValue = !string.IsNullOrEmpty(ifExist) && bool.Parse(ifExist);
				if (!File.Exists(path)) {
					return ifExistValue ?
						CommandResult.Fail($"File \"{path}\" does not exists!") :
						CommandResult.Success();
				}
				File.Delete(path);
				return CommandResult.Success($"File \"{path}\" deleted.");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't delete file at \"{path}\": \"{e}\"");
			}
		}
	}
}