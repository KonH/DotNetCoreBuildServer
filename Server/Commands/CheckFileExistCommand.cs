using System.Collections.Generic;
using System.IO;

namespace Server.Commands {
	public class CheckFileExistCommand:ICommand {
		
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
				File.Exists(path) ?
				CheckSize(path, sizeMore) :
				CommandResult.Fail($"File \"{path}\" does not exists!");
		}
		
		CommandResult CheckSize(string path, string sizeMore) {
			int sizeMoreValue = 0;
			int.TryParse(sizeMore, out sizeMoreValue);
			if (sizeMoreValue > 0) {
				var fileInfo = new FileInfo(path);
				var size = fileInfo.Length;
				return (size > sizeMoreValue) ?
					CommandResult.Success($"File \"{path}\" size is {size} (more than {sizeMoreValue})") : 
					CommandResult.Fail($"File \"{path}\" size is {size} (less than {sizeMoreValue})!");
			} else {
				return CommandResult.Success($"File \"{path}\" is exists");
			}
		}
	}
}