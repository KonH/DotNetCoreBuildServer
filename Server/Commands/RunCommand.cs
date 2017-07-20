using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Server.Commands {
	public class RunCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string path = null;
			args.TryGetValue("path", out path);
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			string commandArgs = null;
			args.TryGetValue("args", out commandArgs);
			try {
				var startInfo = new ProcessStartInfo(path, commandArgs);
				var process = new Process {
					StartInfo = startInfo
				};
				process.Start();
				process.WaitForExit();
				return CommandResult.Success("");
			}
			catch (Exception e) {
				return CommandResult.Fail($"Failed to run process at \"{path}\": \"{e.ToString()}\"");
			}
		}
	}
}