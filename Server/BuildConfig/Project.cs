using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Server.BuildConfig {
	public class Project {

		public Dictionary<string, string> Keys { get; }

		public string BuildsRoot => Keys.Get("builds");

		Project(Dictionary<string, string> keys) {
			Keys = keys;
		}
		
		public static Project Load(LoggerFactory loggerFactory, string serverName, string[] pathes) {
			var logger = loggerFactory.CreateLogger<Project>();
			var builder = new ConfigurationBuilder();
			foreach (var path in pathes) {
				builder.AddJsonFile(path);
				logger.LogDebug($"Load: use file: \"{path}\"");
			}
			var config = builder.Build();
			var keys = new Dictionary<string, string>();
			foreach (var node in config.AsEnumerable()) {
				logger.LogDebug(
					$"Load: key/value in file: \"{node.Key}\"=>\"{node.Value}\"");
				keys.Add(node.Key, node.Value);
			}
			keys.Add("serverName", serverName);
			var project = new Project(keys);
			logger.LogDebug($"Project.Load: loaded buildsRoot: \"{project.BuildsRoot}\"");
			return project;
		}
	}
}