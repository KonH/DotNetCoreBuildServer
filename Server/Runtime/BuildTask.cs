using Microsoft.Extensions.Logging;
using Server.BuildConfig;

namespace Server.Runtime {
	public class BuildTask {
		
		public BuildNode Node      { get; }
		public bool      IsStarted { get; private set; }
		public bool      IsDone    { get; private set; }
		public bool      IsSuccess { get; private set; }
		public string    Message   { get; private set; }
		public string    Result    { get; private set; }

		ILogger _logger;

		public BuildTask(LoggerFactory loggerFactory, BuildNode node) {
			_logger = loggerFactory.CreateLogger<BuildTask>();
			Node = node;
		}

		public void Start() {
			_logger.LogInformation($"BuildTask(\"{Node.Name}\").Start");
			IsStarted = true;
		}

		public void Done(bool isSuccess, string message, string result) {
			_logger.LogInformation($"BuildTask(\"{Node.Name}\").Done({isSuccess}, \"{message}\", \"{result}\")");
			IsDone    = true;
			IsSuccess = isSuccess;
			Message   = message;
			Result    = result;
		}
	}
}