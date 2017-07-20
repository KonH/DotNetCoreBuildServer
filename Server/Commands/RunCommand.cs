using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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
			string workDir = null;
			args.TryGetValue("work_dir", out workDir);
			string logFile = null;
			args.TryGetValue("log_file", out logFile);
			try {
				var startInfo = new ProcessStartInfo(path, commandArgs);
				if (!string.IsNullOrEmpty(workDir)) {
					startInfo.WorkingDirectory = workDir;
				}
				if (!string.IsNullOrEmpty(logFile)) {
					startInfo.RedirectStandardOutput = true;
				}
				var process = new Process {
					StartInfo = startInfo
				};
				process.Start();
				using (var logStream = OpenLogFile(logFile)) {
					if (logStream != null) {
						ReadOutputAsync(process.StandardOutput, logStream);
					}
					process.WaitForExit();
				}
				var resultMessage = "";
				if (!string.IsNullOrEmpty(logFile)) {
					resultMessage = $"Log saved to {logFile}.";
				}
				return CommandResult.Success(resultMessage);
			}
			catch (Exception e) {
				return CommandResult.Fail($"Failed to run process at \"{path}\": \"{e.ToString()}\"");
			}
		}

		FileStream OpenLogFile(string logFile) {
			if (logFile != null) {
				return File.OpenWrite(logFile);
			}
			return null;
		}
		
		async void ReadOutputAsync(StreamReader reader, FileStream logStream) {
			string line = await reader.ReadLineAsync();
			if (!string.IsNullOrEmpty(line)) {
				var bytes = Encoding.UTF8.GetBytes(line + "\n");
				logStream.Write(bytes, 0, bytes.Length);
			}
			ReadOutputAsync(reader, logStream);
		}
	}
}