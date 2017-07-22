using System;
using System.IO;
using System.Collections.Generic;

namespace Server.Commands {
	public class CopyFileCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string fromPath = null;
			args.TryGetValue("from", out fromPath);
			string toPath = null;
			args.TryGetValue("to", out toPath);
			if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath)) {
				return CommandResult.Fail("No pathes provided!");
			}
			try {
				File.Copy(fromPath, toPath, true);
				return CommandResult.Success($"File copied from \"{fromPath}\" to \"{toPath}\".");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't copy file from \"{fromPath}\" to \"{toPath}\": \"{e.ToString()}\"");
			}
		}
	}
}