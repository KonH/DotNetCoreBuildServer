using System.Collections.Generic;
using Server;
using Server.BuildConfig;

namespace ConsoleClient {
	class Program {
		static void Main(string[] args) {
			var server = new BuildServer();
			var nodes = new BuildNode[] {
				new BuildNode("validate_project_root", "check_dir_exist", new Dictionary<string, string>(){{"path", "{root}"}, {"size_more", "1"}}),
				new BuildNode("validate_sources", "check_file_exist", new Dictionary<string, string>(){{"path", "{root}/sources.txt"}}),
				new BuildNode("clean_project", "delete_file", new Dictionary<string, string>(){{"path", "{root}/build.txt"},{"if_exist", "true"}}), 
				new BuildNode("show_project_root", "run", new Dictionary<string, string>(){{"path", "ls"}, {"args", "{root}"}}), 
				new BuildNode("make_build", "run", new Dictionary<string, string>(){{"path", "{root}/run.sh"}, {"work_dir", "{root}"}}),
				new BuildNode("validate_build", "check_file_exist", new Dictionary<string, string>{{"path", "{root}/build.txt"}, {"size_more", "1"}}), 
			};
			var buildConfig = new Build("test", nodes);
			var result = server.InitBuild(buildConfig);
			var writer = new BuildResultWriter(result);
			server.StartBuild();
		}
	}
}