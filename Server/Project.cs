using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Server {
	class Project {

		public string Root { get; }
		
		Project(string root) {
			Root = root;
		}
		
		public static Project Load() {
			var builder = new ConfigurationBuilder().
				AddInMemoryCollection(new [] {
					new KeyValuePair<string, string>("root", "/Users/konh/Projects/CSharp/BuildServerExample") 
				});
			var config = builder.Build();
			var root = config["root"];
			return new Project(root);
		}
	}
}