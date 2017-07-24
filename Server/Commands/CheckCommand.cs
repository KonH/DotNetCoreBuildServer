using System.Collections.Generic;

namespace Server.Commands {
	[Command("check")]
	public class CheckCommand:ICommand {
		
		public CommandResult Execute(Dictionary<string, string> args) {
			var condition = args.Get("condition");
			var value = args.Get("value");
			var silent = args.Get("silent");
			if (string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(value)) {
				return CommandResult.Fail($"Wrong arguments: condition: '{condition}', value: '{value}'!");
			}
			if (condition == value) {
				return CommandResult.Success($"Check passed: '{condition}' == '{value}'");
			}
			bool silentValue;
			bool.TryParse(silent, out silentValue);
			return CommandResult.Fail($"Check failed: '{condition}' != '{value}'!", silentValue);
		}
	}
}