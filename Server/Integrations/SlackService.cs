using System;
using System.Threading.Tasks;
using Server.BuildConfig;
using Server.Controllers;
using Server.Runtime;
using Server.Views;
using SlackBotNet;

namespace Server.Integrations {
	public class SlackService:IService {
		
		public event Action<string> OnMessage;
		
		string   _name = null;
		string   _hub  = null;
		SlackBot _bot  = null;

		SlackServerController _controller = null;
		SlackServerView       _view       = null;
		
		bool IsValidSettings(string name, string token, string hub) {
			return 
				!(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hub));
		}
		
		public bool TryInit(BuildServer server, Project project) {
			string name = server.Name;
			var keys = project.Keys;
			var token = keys.Get("slack_token");
			var hub = keys.Get("slack_hub");
			if (IsValidSettings(name, token, hub)) {
				Init(server, name, token, hub);
				return true;
			}
			return false;
		}
		
		void Init(BuildServer server, string name, string token, string hub) {
			_name = name;
			_hub  = hub;
			InitBotAsync(token).GetAwaiter().GetResult();
			_bot.When(_bot.State.BotUserId, conv => {
				OnMessage?.Invoke(conv.Text);
				return null;
			});
			_controller = new SlackServerController(this, server);
			_view       = new SlackServerView(this, server);
		}

		async Task InitBotAsync(string token) {
			_bot = await SlackBot.InitializeAsync(token);
		}

		public async void SendMessage(string message) {
			var hubState = _bot.State.GetHub(_hub);
			var fullMessage = string.Format("[{0}]\n {1}", _name, message);
			await _bot.SendAsync(hubState, fullMessage);
		}
	}
}