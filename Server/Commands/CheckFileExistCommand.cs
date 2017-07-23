using System.Collections.Generic;
using System.IO;

namespace Server.Commands {
	[CommandAttribute("check_file_exist")]
	public class CheckFileExistCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			if (args == null) {
				return CommandResult.Fail("No arguments provided!");
			}
			var path = args.Get("path");
			if (string.IsNullOrEmpty(path)) {
				return CommandResult.Fail("No path provided!");
			}
			var sizeMore = args.Get("size_more");
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