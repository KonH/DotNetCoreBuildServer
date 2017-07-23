using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Server.BuildConfig;

namespace Server.Commands {
	public static class CommandFactory {

		static readonly Dictionary<string, Type> Commands = new Dictionary<string, Type>();

		static CommandFactory() {
			AddAllCurrentHandlers();
		}

		static bool IsCommandType(Type type) {
			return type.IsAssignableFrom(typeof(ICommand));
		}
		
		static void AddAllCurrentHandlers() {
			var assembly = typeof(CommandFactory).GetTypeInfo().Assembly;
			var types = assembly.DefinedTypes;
			foreach (var type in types) {
				var attr = type.GetCustomAttribute<CommandAttribute>();
				if (attr != null) {
					AddCommandHandler(attr.Name, type.AsType());
				}
			}
		}
		
		public static void AddCommandHandler(string command, Type type) {
			if (IsCommandType(type)) {
				return;
			}
			Commands.Add(command, type);
		}
		
		public static ICommand Create(BuildNode node) {
			Type commandType = null;
			if (Commands.TryGetValue(node.Command, out commandType)) {
				var commandInstance = Activator.CreateInstance(commandType) as ICommand;
				return commandInstance;
			}
			return new NotFoundCommand();
		}
	}
}