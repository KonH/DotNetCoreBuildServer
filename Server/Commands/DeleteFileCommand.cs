using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Commands {
	public class DeleteFileCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string path = null;
			args.TryGetValue("path", out path);
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			string ifExist = null;
			args.TryGetValue("if_exist", out ifExist);
			try {
				var ifExistValue = !string.IsNullOrEmpty(ifExist) && bool.Parse(ifExist);
				if (!File.Exists(path)) {
					return ifExistValue ? 
						CommandResult.Success() : 
						CommandResult.Fail($"File \"{path}\" does not exists!");
				}
				File.Delete(path);
				return CommandResult.Success($"File \"{path}\" deleted.");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't delete file at \"{path}\": \"{e.ToString()}\"");
			}
		}
	}
}