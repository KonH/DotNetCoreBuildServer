using System.Collections.Generic;
using System.Linq;

namespace Server {
	public class BuildConfig {
		
		public string             Name     { get; }
		public List<BuildCommand> Commands { get; }

		public BuildConfig(string name, IEnumerable<BuildCommand> commands) {
			Name  = name;
			Commands = commands.ToList();
		}
	}
}