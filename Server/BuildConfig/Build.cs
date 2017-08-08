using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Server.BuildConfig {
	public class Build {
		
		public string          Name    { get; }
		public string          LogFile { get; }
		public List<string>    Args    { get; }
		public List<BuildNode> Nodes   { get; }

		Build(string name, string logFile, IEnumerable<string> args, IEnumerable<BuildNode> nodes) {
			Name    = name;
			LogFile = logFile;
			Args    = args.ToList();
			Nodes   = nodes.ToList();
		}

		static void ProcessTasks(IConfiguration configNode, ICollection<BuildNode> buildNodes) {
			var tasksContent = configNode.GetChildren();
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var taskParentNode in tasksContent) {
				Debug.WriteLine($"Build.Load: taskParentNode: \"{taskParentNode.Key}\"");
				var taskNode = taskParentNode.GetChildren().FirstOrDefault();
				Debug.WriteLine($"Build.Load: taskNode: \"{taskNode.Key}\"");
				var buildNode = ExtractBuildNode(taskNode);
				if (buildNode != null) {
					buildNodes.Add(buildNode);
				}
			}
		}

		static BuildNode ExtractBuildNode(IConfigurationSection taskNode) {
			var nodeName = taskNode.Key;
			if (nodeName == "_build") {
				return ExtractSubBuildNode(taskNode);
			}
			var command = taskNode.GetChildren().FirstOrDefault();
			Debug.WriteLine($"Build.Load: command: \"{command?.Key}\"");
			if (command == null) {
				return null;
			}
			var commandName = command.Key;
			var args = command.GetChildren();
			var commandArgs = args.ToDictionary(arg => arg.Key, arg => arg.Value);
			Debug.WriteLine($"Build.Load: buildNode: [\"{nodeName}\", \"{commandName}\", {commandArgs.Count}]");
			var buildNode = new BuildNode(nodeName, commandName, commandArgs);
			return buildNode;
		}

		static BuildNode ExtractSubBuildNode(IConfigurationSection taskNode) {
			var command = taskNode.GetChildren().FirstOrDefault();
			var buildName = command.Key;
			var args = command.GetChildren();
			var buildArgs = args.ToDictionary(arg => arg.Key, arg => arg.Value);
			Debug.WriteLine($"Sub-build name: \"{buildName}\", args: {buildArgs.Count}");
			return new SubBuildNode(buildName, buildArgs);
		}

		static void ProcessArgs(IConfiguration node, List<string> args) {
			var argsContent = node.GetChildren();
			args.AddRange(argsContent.Select(buildArg => buildArg.Value));
			Debug.WriteLine($"Build.Load: args: {args.Count}");
		}
		
		public static Build Load(string name, string path) {
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var buildNodes = new List<BuildNode>();
			var buildArgs = new List<string>();
			var rootNodes = config.GetChildren();
			string logFile = null;
			foreach (var node in rootNodes) {
				Debug.WriteLine($"Build.Load: rootNode: \"{node.Key}\"");
				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (node.Key == "tasks") {
					ProcessTasks(node, buildNodes);
				} else if (node.Key == "args") {
					ProcessArgs(node, buildArgs);
				} else if (node.Key == "log_file") {
					logFile = node.Value;
					Debug.WriteLine($"Build.Load: Log file is \"{logFile}\"");
				}
			}
			var build = new Build(name, logFile, buildArgs, buildNodes);
			return build;
		}
	}
}