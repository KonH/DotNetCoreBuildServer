using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Server.Commands {
	[Command("copy_dir")]
	public class CopyDirCommand:ICommand {
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var fromPath = args.Get("from");
			var toPath = args.Get("to");
			if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath)) {
				return CommandResult.Fail("No pathes provided!");
			}
			var ifExist = args.Get("if_exist");
			try {
				var ifExistValue = string.IsNullOrEmpty(ifExist) || bool.Parse(ifExist);
				if ( !Directory.Exists(fromPath) ) {
					return ifExistValue ?
						CommandResult.Fail($"Directory \"{fromPath}\" does not exists!") :
						CommandResult.Success();
				}
				CopyDirectory(fromPath, toPath);
				return CommandResult.Success($"Directory copied from \"{fromPath}\" to \"{toPath}\".");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't copy directory from \"{fromPath}\" to \"{toPath}\": \"{e}\"");
			}
		}
		
		void CopyDirectory(string sourceDirName, string destDirName) {
			var dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					$"Source directory does not exist or could not be found: \"{sourceDirName}\"");
			}

			var dirs = dir.GetDirectories();
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			var files = dir.GetFiles();
			foreach (var file in files) {
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, true);
			}
			foreach (DirectoryInfo subdir in dirs) {
				var temppath = Path.Combine(destDirName, subdir.Name);
				CopyDirectory(subdir.FullName, temppath);
			}
		}
	}
}