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
		
		public static Build Load(string name, string path) {
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var nodes = new List<BuildNode>();
			var rootNodes = config.GetChildren();
			foreach (var rootNode in rootNodes) {
				if (rootNode.Key != "tasks") {
					continue;
				}
				var tasksContent = rootNode.GetChildren();
				foreach (var taskParentNode in tasksContent) {
					var taskNode = taskParentNode.GetChildren().FirstOrDefault();
					var nodeName = taskNode.Key;
					var command = taskNode.GetChildren().FirstOrDefault();
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
			}
			var build = new Build(name, nodes);
			return build;
		}
	}
}