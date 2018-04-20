using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Server.Commands {
	[Command("run")]
	public class RunCommand:ICommand, IAbortableCommand {

		const int MaxReadAttempts  = 10;
		const int ReadAttemptSleep = 500;

		ILogger _logger;

		string _inMemoryLog = "";
		bool   _isAborted   = false;
		
		public CommandResult Execute(LoggerFactory loggerFactory, Dictionary<string, string> args) {
			_logger = loggerFactory.CreateLogger<RunCommand>();
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
			var isExternalLog = args.Get("is_external_log");
			try {
				var startInfo = new ProcessStartInfo(path, commandArgs);
				if (!string.IsNullOrEmpty(workDir)) {
					startInfo.WorkingDirectory = workDir;
				}
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				_logger.LogDebug($"Execute: Initialize process: \"{path}\" \"{commandArgs}\"");
				var process = new Process {
					StartInfo = startInfo
				};
				_inMemoryLog = "";
				process.Start();
				var processName = $"{process.ProcessName} ({process.Id})";
				_logger.LogDebug($"Execute: Process is started: {processName}.");
				var isExternalLogValue = !string.IsNullOrEmpty(isExternalLog) && bool.Parse(isExternalLog);
				_logger.LogDebug($"Execute: logFile: \"{logFile}\", isExternalLog: {isExternalLogValue}");
				using ( var logStream = OpenLogFile(logFile, isExternalLogValue) ) {
					_logger.LogDebug($"Execute: Used logStream: {(logStream != null ? logStream.ToString() : "null")}");
					ReadOutputAsync(processName, process.StandardOutput, logStream);
					ReadOutputAsync(processName, process.StandardError, logStream);
					while ( !process.HasExited ) {
						if ( _isAborted ) {
							process.Kill();
						}
					}
					_logger.LogDebug($"Execute: Process is exited: {processName}.");
				}
				if (_isAborted) {
					throw new CommandAbortedException();
				}
				var errorRegex         = args.Get("error_regex");
				var errorExitCode      = args.GetBoolean("error_exit_code", false);
				var resultRegex        = args.Get("result_regex");
				var checkResultValue   = !string.IsNullOrEmpty(resultRegex);
				var isRightToLeft      = args.Get("result_right_to_left");
				var isRightToLeftValue = !string.IsNullOrEmpty(isRightToLeft) && bool.Parse(isRightToLeft);
				var messageToCheck     = string.Empty;
				var messageToShow      = string.Empty;
				if (!string.IsNullOrEmpty(logFile)) {
					var msg = $"Log saved to {logFile}.";
					string logContent = null;
					int curAttempt = 0;
					bool isDone = false;
					do {
						try {
							_logger.LogDebug($"Execute: Try to read log file at: \"{logFile}\"");
							logContent = File.ReadAllText(logFile);
							isDone = true;
						} catch ( Exception e ) {
							_logger.LogError($"Execute: Read log Exception: \"{e}\", attempt: {curAttempt}/{MaxReadAttempts}");
							if ( curAttempt > MaxReadAttempts ) {
								throw;
							} else {
								curAttempt++;
								Thread.Sleep(ReadAttemptSleep);
							}
						}
					} while (!isDone);
					messageToCheck = logContent;
					messageToShow  = msg;
				} else {
					_inMemoryLog   = _inMemoryLog.TrimEnd('\n');
					messageToCheck = _inMemoryLog;
					messageToShow  = _inMemoryLog;
				}
				var result = GetResultMessage(resultRegex, messageToCheck, isRightToLeftValue);
				if ( errorExitCode ) {
					return CheckCommandResultViaExitCode(process, messageToShow, result);
				}
				return CheckCommandResult(errorRegex, messageToCheck, messageToShow, checkResultValue, result);
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

		string GetResultMessage(string resultRegex, string message, bool isRightToLeft) {
			if (!string.IsNullOrEmpty(resultRegex)) {
				var regex = 
					isRightToLeft ? 
					new Regex(resultRegex, RegexOptions.RightToLeft) : 
					new Regex(resultRegex);
				var match = regex.Match(message);
				var value = match.Value;
				return value;
			}
			return "";
		}
		
		FileStream OpenLogFile(string logFile, bool isExternalLog) {
			if ((logFile != null) && !isExternalLog) {
				return File.OpenWrite(logFile);
			}
			return null;
		}
		
		async void ReadOutputAsync(string name, StreamReader reader, FileStream logStream) {
			string line = await reader.ReadLineAsync();
			if (!string.IsNullOrEmpty(line)) {
				var endedLine = line + "\n";
				if (logStream != null) {
					_logger.LogDebug($"ReadOutputAsync({name}): line: \"{line}\"");
					var bytes = Encoding.UTF8.GetBytes(endedLine);
					try {
						if ( logStream.CanWrite ) {
							logStream.Write(bytes, 0, bytes.Length);
						} else {
							_logger.LogDebug($"ReadOutputAsync({name}): Can't write to LogStream.");
						}
					} catch (Exception e) {
						_logger.LogError($"ReadOutputAsync({name}): exception: \"{e}\"");
					}
				} else {
					_inMemoryLog += endedLine;
				}
				ReadOutputAsync(name, reader, logStream);
			}
		}

		bool ContainsError(string errorRegex, string message) {
			if (!string.IsNullOrEmpty(errorRegex)) {
				var regex = new Regex(errorRegex);
				return regex.IsMatch(message);
			}
			return false;
		}
		
		CommandResult CheckCommandResultViaExitCode(Process process, string messageToShow, string result) {
			_logger.LogDebug($"Process exit code: {process.ExitCode}");
			if ( process.ExitCode != 0 ) {
				return CommandResult.Fail($"(exit code: {process.ExitCode}) {messageToShow}");
			}
			return CommandResult.Success(messageToShow, result);
		}

		CommandResult CheckCommandResult(string errorRegex, string messageToCheck, string messageToShow, bool checkResultValue, string result) {
			return
				ContainsError(errorRegex, messageToCheck) || (checkResultValue && string.IsNullOrEmpty(result)) ?
					CommandResult.Fail(messageToShow) : 
					CommandResult.Success(messageToShow, result);
		}
	}
}