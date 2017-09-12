using System;
using System.Collections.Generic;

namespace Server.Services.Stats {
	public class BuildStat : ICommonStat {
		public string         Name     { get; set; }
		public DateTime       Start    { get; set; }
		public TimeSpan       Duration { get; set; }
		public List<TaskStat> Tasks    { get; set; }

		public BuildStat() {
			Tasks = new List<TaskStat>();
		}

		public BuildStat(string name, DateTime start, TimeSpan duration) : this() {
			Name     = name;
			Start    = start;
			Duration = duration;
		}
	}
}
