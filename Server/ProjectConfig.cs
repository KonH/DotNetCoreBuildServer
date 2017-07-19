using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Server {
	class ProjectConfig {

		public string Root { get; }
		
		ProjectConfig(string root) {
			Root = root;
		}
		
		public static ProjectConfig Load() {
			var builder = new ConfigurationBuilder().
				AddInMemoryCollection(new [] {
					new KeyValuePair<string, string>("root", "/Users/konh/Projects/CSharp/BuildServerExample") 
				});
			var config = builder.Build();
			var root = config["root"];
			return new ProjectConfig(root);
		}
	}
}