using System;
using System.Collections.Generic;
using Server.Runtime;

namespace Server.Manager {
	public class DirectServerManager:ServerManager {

		public DirectServerManager(BuildServer server) : base(server) { }

		protected override void ProcessStatus(Dictionary<string, string> builds) {
			Console.WriteLine("Builds:");
			foreach (var build in builds) {
				Console.WriteLine(build.Key);
			}
		}
		
		public void SendRequest(string message) {
			var request = ConvertMessage(message);
			if (!string.IsNullOrEmpty(request.Item1)) {
				Call(request.Item1, request.Item2);
			}
		}
	}
}