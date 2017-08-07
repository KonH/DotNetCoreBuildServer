using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Commands {
	[CommandAttribute("run")]
	public class RunCommand:ICommand, IAbortableCommand {

		string _inMemoryLog = "";
		bool   _isAborted   = false;
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			var commandArgs = args.Get("args");
			var workDir = args.Get("work_dir");
			var logFile = args.Get("log_file");
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
					while (!process.HasExited) {
						if (_isAborted) {
							process.Kill();
						}
					}
				}
				if (_isAborted) {
					throw new CommandAbortedException();
				}
				var errorRegex = args.Get("error_regex");
				var resultRegex = args.Get("result_regex");
				if (!string.IsNullOrEmpty(logFile)) {
					var msg = $"Log saved to {logFile}.";
					var logContent = File.ReadAllText(logFile);
					var result = GetResultMessage(resultRegex, logContent);
					return CheckCommandResult(errorRegex, logContent, msg, result);
				} else {
					_inMemoryLog = _inMemoryLog.TrimEnd('\n');
					var msg = _inMemoryLog;
					var result = GetResultMessage(resultRegex, msg);
					return CheckCommandResult(errorRegex, msg, msg, result);
				}
			}
			catch (Exception e) {
				if (e is CommandAbortedException) {
					return CommandResult.Fail($"Command is aborted!");
				}
				return CommandResult.Fail($"Failed to run process at \"{path}\": \"{e.ToString()}\"");
			}
		}

		public void Abort() {
			_isAborted = true;
		}

		string GetResultMessage(string resultRegex, string message) {
			if (!string.IsNullOrEmpty(resultRegex)) {
				var regex = new Regex(resultRegex);
				var match = regex.Match(message);
				var value = match.Value;
				return value;
			}
			return "";
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
					Debug.WriteLine($"RunCommand.ReadOutputAsync: line: \"{line}\"");
					var bytes = Encoding.UTF8.GetBytes(endedLine);
					logStream.Write(bytes, 0, bytes.Length);
				} else {
					_inMemoryLog += endedLine;
				}
				ReadOutputAsync(reader, logStream);
			}
		}

		bool ContainsError(string errorRegex, string message) {
			if (!string.IsNullOrEmpty(errorRegex)) {
				var regex = new Regex(errorRegex);
				return regex.IsMatch(message);
			}
			return false;
		}
		
		CommandResult CheckCommandResult(string errorRegex, string messageToCheck, string messageToShow, string result) {
			return
				ContainsError(errorRegex, messageToCheck) ?
					CommandResult.Fail(messageToShow) : 
					CommandResult.Success(messageToShow, result);
		}
	}
}