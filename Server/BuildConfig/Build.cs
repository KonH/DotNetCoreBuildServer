using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

		static void ProcessTasks(ILogger logger, IConfiguration configNode, ICollection<BuildNode> buildNodes) {
			var tasksContent = configNode.GetChildren();
			foreach (var taskParentNode in tasksContent) {
				logger.LogDebug($"Load: taskParentNode: \"{taskParentNode.Key}\"");
				var taskNode = taskParentNode.GetChildren().FirstOrDefault();
				logger.LogDebug($"Load: taskNode: \"{taskNode.Key}\"");
				var buildNode = ExtractBuildNode(logger, taskNode);
				if (buildNode != null) {
					buildNodes.Add(buildNode);
				}
			}
		}

		static BuildNode ExtractBuildNode(ILogger logger, IConfigurationSection taskNode) {
			var nodeName = taskNode.Key;
			if (nodeName == "_build") {
				return ExtractSubBuildNode(logger, taskNode);
			}
			var command = taskNode.GetChildren().FirstOrDefault();
			logger.LogDebug($"Load: command: \"{command?.Key}\"");
			if (command == null) {
				return null;
			}
			var commandName = command.Key;
			var args = command.GetChildren();
			var commandArgs = args.ToDictionary(arg => arg.Key, arg => arg.Value);
			logger.LogDebug($"Load: buildNode: [\"{nodeName}\", \"{commandName}\", {commandArgs.Count}]");
			var buildNode = new BuildNode(nodeName, commandName, commandArgs);
			return buildNode;
		}

		static BuildNode ExtractSubBuildNode(ILogger logger, IConfigurationSection taskNode) {
			var command = taskNode.GetChildren().FirstOrDefault();
			var buildName = command.Key;
			var args = command.GetChildren();
			var buildArgs = args.ToDictionary(arg => arg.Key, arg => arg.Value);
			logger.LogDebug($"Sub-build name: \"{buildName}\", args: {buildArgs.Count}");
			return new SubBuildNode(buildName, buildArgs);
		}

		static void ProcessArgs(ILogger logger, IConfiguration node, List<string> args) {
			var argsContent = node.GetChildren();
			args.AddRange(argsContent.Select(buildArg => buildArg.Value));
			logger.LogDebug($"Load: args: {args.Count}");
		}
		
		public static Build Load(LoggerFactory loggerFactory, string name, string path) {
			var logger = loggerFactory.CreateLogger<Build>();
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var buildNodes = new List<BuildNode>();
			var buildArgs = new List<string>();
			var rootNodes = config.GetChildren();
			string logFile = null;
			foreach (var node in rootNodes) {
				logger.LogDebug($"Build.Load: rootNode: \"{node.Key}\"");
				if (node.Key == "tasks") {
					ProcessTasks(logger, node, buildNodes);
				} else if (node.Key == "args") {
					ProcessArgs(logger, node, buildArgs);
				} else if (node.Key == "log_file") {
					logFile = node.Value;
					logger.LogDebug($"Build.Load: Log file is \"{logFile}\"");
				}
			}
			var build = new Build(name, logFile, buildArgs, buildNodes);
			return build;
		}
	}
}