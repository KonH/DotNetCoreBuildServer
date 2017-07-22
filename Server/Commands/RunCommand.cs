using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Server.Commands {
	public class RunCommand:ICommand {

		string _inMemoryLog = "";
		
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
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				var process = new Process {
					StartInfo = startInfo
				};
				_inMemoryLog = "";
				process.Start();
				using (var logStream = OpenLogFile(logFile)) {
					ReadOutputAsync(process.StandardOutput, logStream);
					ReadOutputAsync(process.StandardError, logStream);
					process.WaitForExit();
				}
				string resultMessage = null;
				if (!string.IsNullOrEmpty(logFile)) {
					resultMessage = $"Log saved to {logFile}.";
				} else {
					_inMemoryLog = _inMemoryLog.TrimEnd('\n');
					resultMessage = _inMemoryLog;
				}
				string errorFilter = null;
				args.TryGetValue("error_filter", out errorFilter);
				return CheckCommandResult(errorFilter, resultMessage);
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
				var endedLine = line + "\n";
				if (logStream != null) {
					var bytes = Encoding.UTF8.GetBytes(endedLine);
					logStream.Write(bytes, 0, bytes.Length);
				} else {
					_inMemoryLog += endedLine;
				}
				ReadOutputAsync(reader, logStream);
			}
		}

		bool ContainsError(string errorFilter, string message) {
			if (!string.IsNullOrEmpty(errorFilter)) {
				return message.Contains(errorFilter);
			}
			return false;
		}
		
		CommandResult CheckCommandResult(string errorFilter, string message) {
			return
				ContainsError(errorFilter, message) ?
					CommandResult.Fail(message) : 
					CommandResult.Success(message);
		}
	}
}