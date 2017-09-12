using System;

namespace Server.Services.Stats {
	public class TaskStat : ICommonStat {
		public string   Name     { get; set; }
		public DateTime Start    { get; set; }
		public TimeSpan Duration { get; set; }

		public TaskStat() { }

		public TaskStat(string name, DateTime start, TimeSpan duration) {
			Name     = name;
			Start    = start;
			Duration = duration;
		}
	}
}
