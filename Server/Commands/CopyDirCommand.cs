﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Server.Commands {
	public class CopyDirCommand:ICommand {
		
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
				CopyDirectory(fromPath, toPath);
				return CommandResult.Success($"Directory copied from \"{fromPath}\" to \"{toPath}\".");
			} catch (Exception e) {
				return CommandResult.Fail($"Can't copy directory from \"{fromPath}\" to \"{toPath}\": \"{e.ToString()}\"");
			}
		}
		
		void CopyDirectory(string sourceDirName, string destDirName) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					$"Source directory does not exist or could not be found: {sourceDirName}");
			}

			var dirs = dir.GetDirectories();
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			var files = dir.GetFiles();
			foreach (var file in files) {
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}
			foreach (DirectoryInfo subdir in dirs) {
				var temppath = Path.Combine(destDirName, subdir.Name);
				CopyDirectory(subdir.FullName, temppath);
			}
		}
	}
}