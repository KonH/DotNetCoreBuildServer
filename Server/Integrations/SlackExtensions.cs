using Server.Runtime;

namespace Server.Integrations {
	public static class SlackExtensions {
		
		public static SlackManager AddSlackManager(this BuildServer server) {
			string token = null;
			server.Project.Keys.TryGetValue("slack_token", out token);
			string hub = null;
			server.Project.Keys.TryGetValue("slack_hub", out hub);
			var slackManager = new SlackManager(token, hub);
			return slackManager;
		}
	}
}