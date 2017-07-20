﻿using Server.BuildConfig;

namespace Server.Runtime {
	public class BuildTask {
		
		public BuildNode Node      { get; }
		public bool      IsStarted { get; private set; }
		public bool      IsDone    { get; private set; }
		public bool      IsSuccess { get; private set; }
		public string    Message   { get; private set; }
		
		public BuildTask(BuildNode node) {
			Node = node;
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