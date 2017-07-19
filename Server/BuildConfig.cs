using System.Collections.Generic;
using System.Linq;

namespace Server {
	public class BuildConfig {
		
		public string       Name  { get; }
		public List<string> Tasks { get; }

		public BuildConfig(string name, IEnumerable<string> tasks) {
			Name  = name;
			Tasks = tasks.ToList();
		}
	}
}