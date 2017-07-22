using System.Collections.Generic;
using Server.BuildConfig;

namespace Server.Commands {
	public static class CommandFactory {

		static readonly Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand> {
			{"print", new PrintCommand()},
			{"check_dir_exist", new CheckDirExistCommand()},
			{"delete_dir", new DeleteDirCommand()},
			{"copy_dir", new CopyDirCommand()},
			{"check_file_exist", new CheckFileExistCommand()},
			{"delete_file", new DeleteFileCommand()},
			{"copy_file", new CopyFileCommand()},
			{"run", new RunCommand()}
		};
		
		public static ICommand Create(BuildNode node) {
			ICommand command = null;
			if (Commands.TryGetValue(node.Command, out command)) {
				return command;
			}
			return new NotFoundCommand();
		}
	}
}