namespace Server.Commands {
	public class CommandResult {
		
		public bool   IsSuccess { get; }
		public string Message   { get; }
		public string Result    { get; }

		CommandResult(bool isSuccess, string message, string result) {
			IsSuccess = isSuccess;
			Message   = message;
			Result    = result;
		}

		public static CommandResult Success(string message = "", string result = "") {
			return new CommandResult(true, message, result);
		}

		public static CommandResult Fail(string message = "") {
			return new CommandResult(false, message, "");
		}
	}
}