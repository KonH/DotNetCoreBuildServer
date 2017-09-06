using System;
using System.Collections.Generic;
using System.Reflection;
using Server.BuildConfig;
using Microsoft.Extensions.Logging;

namespace Server.Commands {
	public class CommandFactory {

		readonly Dictionary<string, Type> _commands = new Dictionary<string, Type>();

		LoggerFactory _loggerFactory;
		ILogger       _logger;

		public CommandFactory(LoggerFactory loggerFactory) {
			_loggerFactory = loggerFactory;
			_logger = loggerFactory.CreateLogger<CommandFactory>();
			_logger.LogDebug($"ctor()");
			try {
				AddAllCurrentHandlers();
				_logger.LogInformation($"ctor(): commands: '{_commands.Count}'");
			} catch (Exception e) {
				_logger.LogError($"ctor(): exception: {e}");
			}

		}

		static bool IsCommandType(Type type) {
			return type.IsAssignableFrom(typeof(ICommand));
		}
		
		void AddAllCurrentHandlers() {
			var assembly = typeof(CommandFactory).GetTypeInfo().Assembly;
			var types = assembly.DefinedTypes;
			foreach (var type in types) {
				var attr = type.GetCustomAttribute<CommandAttribute>();
				if (attr != null) {
					AddCommandHandler(attr.Name, type.AsType());
				}
			}
		}
		
		public void AddCommandHandler(string command, Type type) {
			if (IsCommandType(type)) {
				return;
			}
			_commands.Add(command, type);
			_logger.LogDebug($"AddCommandHandler: '{command}' => {type.FullName}");
		}
		
		public ICommand Create(BuildNode node) {
			Type commandType;
			if (!_commands.TryGetValue(node.Command, out commandType)) {
				return new NotFoundCommand();
			}
			var commandInstance = Activator.CreateInstance(commandType) as ICommand;
			return commandInstance;
		}

		public bool ContainsHandler(string command) {
			return _commands.ContainsKey(command);
		}
	}
}