namespace Server {
	public class BuildTask {
		
		public string Name      { get; }
		public bool   IsStarted { get; private set; }
		public bool   IsDone    { get; private set; }
		public bool   IsSuccess { get; private set; }
		public string Message   { get; private set; }
		
		public BuildTask(string name) {
			Name = name;
		}

		public void Start() {
			IsStarted = true;
		}

		public void Done(bool isSuccess, string message) {
			IsDone    = true;
			IsSuccess = isSuccess;
			Message   = message;
		}
	}
}