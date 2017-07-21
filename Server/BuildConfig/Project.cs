using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Server.BuildConfig {
	class Project {

		public Dictionary<string, string> Keys { get; }

		public string BuildsRoot {
			get {
				string path = null;
				Keys.TryGetValue("builds", out path);
				return path;
			}
		}

		Project(Dictionary<string, string> keys) {
			Keys = keys;
		}
		
		public static Project Load(string[] pathes) {
			var builder = new ConfigurationBuilder();
			foreach (var path in pathes) {
				builder.AddJsonFile(path);
			}
			var config = builder.Build();
			var keys = new Dictionary<string, string>();
			foreach (var node in config.AsEnumerable()) {
				keys.Add(node.Key, node.Value);
			}
			return new Project(keys);
		}
	}
}