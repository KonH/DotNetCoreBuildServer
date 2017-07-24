using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Server.BuildConfig {
	public class Project {

		public Dictionary<string, string> Keys { get; }

		public string BuildsRoot => Keys.Get("builds");

		Project(Dictionary<string, string> keys) {
			Keys = keys;
		}
		
		public static Project Load(string serverName, string[] pathes) {
			var builder = new ConfigurationBuilder();
			foreach (var path in pathes) {
				builder.AddJsonFile(path);
				Debug.WriteLine($"Project.Load: use file: \"{path}\"");
			}
			var config = builder.Build();
			var keys = new Dictionary<string, string>();
			foreach (var node in config.AsEnumerable()) {
				Debug.WriteLine(
					$"Project.Load: key/value in file: \"{node.Key}\"=>\"{node.Value}\"");
				keys.Add(node.Key, node.Value);
			}
			keys.Add("serverName", serverName);
			var project = new Project(keys);
			Debug.WriteLine($"Project.Load: loaded buildsRoot: \"{project.BuildsRoot}\"");
			return project;
		}
	}
}