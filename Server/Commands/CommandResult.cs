namespace Server.Commands {
	public class CommandResult {
		
		public bool   IsSuccess { get; }
		public string Message   { get; }

		CommandResult(bool isSuccess, string message) {
			IsSuccess = isSuccess;
			Message   = message;
		}

		public static CommandResult Success(string message = "") {
			return new CommandResult(true, message);
		}

		public static CommandResult Fail(string message = "") {
			return new CommandResult(false, message);
		}
	}
}