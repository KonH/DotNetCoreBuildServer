using System;

namespace Server.Services.Stats {
	public interface ICommonStat {
		string   Name     { get; set; }
		DateTime Start    { get; set; }
		TimeSpan Duration { get; set; }
	}
}
