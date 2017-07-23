using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Server.BuildConfig;

namespace Server.Commands {
	public static class CommandFactory {

		static readonly Dictionary<string, Type> Commands = new Dictionary<string, Type>();

		static CommandFactory() {
			Debug.WriteLine($"CommandFactory.static ctor()");
			try {
				AddAllCurrentHandlers();
				Debug.WriteLine($"CommandFactory.static ctor(): commands: '{Commands.Count}'");
			} catch (Exception e) {
				Debug.WriteLine($"CommandFactory.static ctor(): exception: {e}");
			}

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
			Debug.WriteLine($"CommandFactory.AddCommandHandler: '{command}' => {type.FullName}");
		}
		
		public static ICommand Create(BuildNode node) {
			Type commandType;
			if (!Commands.TryGetValue(node.Command, out commandType)) {
				return new NotFoundCommand();
			}
			var commandInstance = Activator.CreateInstance(commandType) as ICommand;
			return commandInstance;
		}
	}
}