using System.Collections.Generic;

namespace Server.Controllers {
	public class RequestArgs : List<string> {
		
		public RequestArgs() { }
		
		public RequestArgs(IEnumerable<string> args) {
			foreach (var s in args) {
				Add(s);
			}
		}
	}
}