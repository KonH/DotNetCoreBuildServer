using System.Collections.Generic;
using System.Linq;

namespace Server.BuildConfig {
	public class Build {
		
		public string          Name  { get; }
		public List<BuildNode> Nodes { get; }

		public Build(string name, IEnumerable<BuildNode> nodes) {
			Name  = name;
			Nodes = nodes.ToList();
		}
	}
}