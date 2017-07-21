using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Server.BuildConfig {
	class Project {

		public Dictionary<string, string> Keys { get; }
		
		Project(Dictionary<string, string> keys) {
			Keys = keys;
		}
		
		public static Project Load(string path) {
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var keys = new Dictionary<string, string>();
			foreach (var node in config.AsEnumerable()) {
				keys.Add(node.Key, node.Value);
			}
			return new Project(keys);
		}
	}
}