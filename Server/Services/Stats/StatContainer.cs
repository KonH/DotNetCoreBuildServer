using System.Collections.Generic;

namespace Server.Services.Stats {
	public class StatContainer {
		public List<BuildStat> Builds { get; set; }

		public StatContainer() {
			Builds = new List<BuildStat>();
		}
	}
}
