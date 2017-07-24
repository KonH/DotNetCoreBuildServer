namespace Server.Commands {
	public class CommandResult {
		
		public bool   IsSuccess { get; }
		public string Message   { get; }
		public string Result    { get; }
		public bool   Silent    { get; }

		CommandResult(bool isSuccess, string message, string result, bool silent) {
			IsSuccess = isSuccess;
			Message   = message;
			Result    = result;
			Silent    = silent;
		}

		public static CommandResult Success(string message = "", string result = "") {
			return new CommandResult(true, message, result, false);
		}

		public static CommandResult Fail(string message = "", bool silent = false) {
			return new CommandResult(false, message, "", silent);
		}
	}
}