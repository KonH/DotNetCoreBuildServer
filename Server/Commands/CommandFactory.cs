using System.Collections.Generic;

namespace Server.Commands {
	public static class CommandFactory {

		static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand> {
			{"print", new PrintCommand()},
			{"check_dir_exist", new CheckDirExistCommand()}
		};
		
		public static ICommand Create(string name) {
			ICommand command = null;
			if (Commands.TryGetValue(name, out command)) {
				return command;
			}
			return new NotFoundCommand();
		}
	}
}