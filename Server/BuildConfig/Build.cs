using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Server.BuildConfig {
	public class Build {
		
		public string          Name  { get; }
		public List<BuildNode> Nodes { get; }

		Build(string name, IEnumerable<BuildNode> nodes) {
			Name  = name;
			Nodes = nodes.ToList();
		}
		
		public static Build Load(string path) {
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var nodes = new List<BuildNode>();
			foreach (var configNode in config.GetChildren()) {
				var nodeName = configNode.Key;
				var command = configNode.GetChildren().FirstOrDefault();
				if (command != null) {
					var commandName = command.Key;
					var commandArgs = new Dictionary<string, string>();
					var args = command.GetChildren();
					foreach (var arg in args) {
						commandArgs.Add(arg.Key, arg.Value);
					}
					var node = new BuildNode(nodeName, commandName, commandArgs);
					nodes.Add(node);
				}
			}
			var build = new Build(path, nodes);
			return build;
		}
	}
}