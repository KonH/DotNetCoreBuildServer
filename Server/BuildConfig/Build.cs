using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Server.BuildConfig {
	public class Build {		
		public string          Name             { get; }
		public string          ShortDescription { get; }
		public string          LongDescription  { get; }
		public string          LogFile          { get; }
		public List<string>    ArgsDescription  { get; }	
		public List<string>    Args             { get; }
		public List<string>    Checks           { get; }
		public List<BuildNode> Nodes            { get; }

		Build(string name, string shortDescription, string longDescription, string logFile,
				IEnumerable<string> args, IEnumerable<string> argsDescs, IEnumerable<string> checks, IEnumerable<BuildNode> nodes) {
			Name             = name;
			ShortDescription = shortDescription;
			LongDescription  = longDescription;
			LogFile          = logFile;
			Args             = args.ToList();
			Checks           = checks.ToList();
			Nodes            = nodes.ToList();
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

		static void ProcessArray(string name, ILogger logger, IConfiguration node, List<string> args) {
			var argsContent = node.GetChildren();
			args.AddRange(argsContent.Select(arg => arg.Value));
			logger.LogDebug($"Load: {name}: {args.Count}");
		}
		
		public static Build Load(LoggerFactory loggerFactory, string name, string path) {
			var logger = loggerFactory.CreateLogger<Build>();
			var builder = new ConfigurationBuilder().AddJsonFile(path);
			var config = builder.Build();
			var buildNodes = new List<BuildNode>();
			var buildArgs = new List<string>();
			var argsChecks = new List<string>();
			var argsDesc = new List<string>();
			var rootNodes = config.GetChildren();
			var shortDescription = "";
			var longDescription = "";
			string logFile = null;
			foreach (var node in rootNodes) {
				logger.LogDebug($"Build.Load: rootNode: \"{node.Key}\"");
				switch ( node.Key ) {
					case "tasks": {
							ProcessTasks(logger, node, buildNodes);
							break;
						}

					case "args": {
							ProcessArray("args", logger, node, buildArgs);
							break;
						}
					case "args_check": {
							ProcessArray("args_check", logger, node, argsChecks);
							break;
						}

					case "log_file": {
							logFile = node.Value;
							logger.LogDebug($"Build.Load: Log file is \"{logFile}\"");
							break;
						}
					case "short_description": {
							shortDescription = node.Value;
							break;
					}
					case "long_description": {
							longDescription = node.Value;
							break;
					}
					case "args_description": {
							ProcessArray("args_description", logger, node, argsDesc);
							break;
					}
				}
			}
			var build = new Build(name, shortDescription, longDescription, logFile, buildArgs, argsDesc, argsChecks, buildNodes);
			return build;
		}
	}
}