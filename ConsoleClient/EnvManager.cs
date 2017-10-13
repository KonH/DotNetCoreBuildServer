using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleClient {
	static class EnvManager {
		const string ArgStart     = "-";
		const string ArgDelimiter = "=";

		public static string FindArgumentValue(string argName) {
			return FindArgumentValues(argName).FirstOrDefault();
		}

		public static List<string> FindArgumentValues(string argName) {
			var args = Environment.GetCommandLineArgs();
			var argItems = args.Where(a => a.StartsWith(ArgStart + argName + ArgDelimiter));
			var argValues = new List<string>();
			foreach ( var argItem in argItems ) {
				var parts = argItem.Split(ArgDelimiter);
				if ( parts.Length >= 2 ) {
					argValues.Add(parts[1]);
				}
			}
			return argValues;
		}
	}
}
