using System;

namespace Server.Services.Stats {
	public class TaskStat {
		public string   Name     { get; set; }
		public TimeSpan Duration { get; set; }

		public TaskStat() { }

		public TaskStat(string name, TimeSpan duration) {
			Name     = name;
			Duration = duration;
		}
	}
}
