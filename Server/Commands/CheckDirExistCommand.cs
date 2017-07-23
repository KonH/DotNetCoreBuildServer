using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Server.Commands {
	[CommandAttribute("check_dir_exist")]
	public class CheckDirExistCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			string path = null;
			args.TryGetValue("path", out path);
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			string sizeMore = null;
			args.TryGetValue("size_more", out sizeMore);
			return 
				Directory.Exists(path) ?
				CheckSize(path, sizeMore) :
				CommandResult.Fail($"Directory \"{path}\" does not exists!");
		}

		CommandResult CheckSize(string path, string sizeMore) {
			int sizeMoreValue = 0;
			int.TryParse(sizeMore, out sizeMoreValue);
			if (sizeMoreValue > 0) {
				var size = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
				return (size > sizeMoreValue) ?
					CommandResult.Success($"Directory \"{path}\" size is {size} (more than {sizeMoreValue})") : 
					CommandResult.Fail($"Directory \"{path}\" size is {size} (less than {sizeMoreValue})!");
			} else {
				return CommandResult.Success($"Directory \"{path}\" is exists");
			}
		}
	}
}