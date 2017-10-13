using System;
using System.Linq;
using System.Collections.Generic;

namespace Server.Services.Stats {
	public class DictItem {
		public string Key   { get; set; }
		public string Value { get; set; }
	}

	public class BuildStat : ICommonStat {
		public string         Name     { get; set; }
		public DateTime       Start    { get; set; }
		public TimeSpan       Duration { get; set; }
		public List<TaskStat> Tasks    { get; set; }
		public List<DictItem> Args     { get; set; }

		public BuildStat() {
			Tasks = new List<TaskStat>();
		}

		public BuildStat(string name, Dictionary<string, string> args, DateTime start, TimeSpan duration) : this() {
			Name     = name;
			Start    = start;
			Duration = duration;
			Args     = args.Select(p => new DictItem() { Key = p.Key, Value = p.Value }).ToList();
		}
	}
}
