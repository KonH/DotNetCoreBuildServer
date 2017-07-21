using System.Collections.Generic;
using System.Linq;

namespace Server.BuildConfig {
	public class Build {
		
		public string          Name  { get; }
		public List<BuildNode> Nodes { get; }

		Build(string name, IEnumerable<BuildNode> nodes) {
			Name  = name;
			Nodes = nodes.ToList();
		}

		public static Build Load() {
			var nodes = new BuildNode[] {
				new BuildNode("validate_project_root", "check_dir_exist", new Dictionary<string, string>(){{"path", "{root}"}, {"size_more", "1"}}),
				new BuildNode("validate_sources", "check_file_exist", new Dictionary<string, string>(){{"path", "{root}/sources.txt"}}),
				new BuildNode("clean_project", "delete_file", new Dictionary<string, string>(){{"path", "{root}/build.txt"},{"if_exist", "true"}}), 
				new BuildNode("show_project_root", "run", new Dictionary<string, string>(){{"path", "ls"}, {"args", "{root}"}}), 
				new BuildNode("make_build", "run", new Dictionary<string, string>(){{"path", "{root}/run.sh"}, {"work_dir", "{root}"}, {"log_file", "{root}/log.txt"}}),
				new BuildNode("validate_build", "check_file_exist", new Dictionary<string, string>{{"path", "{root}/build.txt"}, {"size_more", "1"}}), 
			};
			var build = new Build("test", nodes);
			return build;
		}
	}
}