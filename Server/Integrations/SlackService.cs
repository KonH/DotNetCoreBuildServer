using System;
using System.Diagnostics;
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
				return InitBot(server, name, token, hub);
			}
			Debug.WriteLine(
				$"SlackService.TryInit: wrong arguments: name: '{name}', token: '{token}', hub: '{hub}'");
			return false;
		}
		
		bool InitBot(BuildServer server, string name, string token, string hub) {
			_name = name;
			_hub  = hub;
			try {
				InitBotAsync(token).GetAwaiter().GetResult();
			} catch (Exception e) {
				Debug.WriteLine($"SlackService.InitBot: exception: {e.ToString()}");
				return false;
			}
			_bot.When(_bot.State.BotUserId, conv => {
				OnMessage?.Invoke(conv.Text);
				return null;
			});
			_controller = new SlackServerController(this, server);
			_view       = new SlackServerView(this, server);
			return true;
		}

		async Task InitBotAsync(string token) {
			_bot = await SlackBot.InitializeAsync(token);
		}

		public async void SendMessage(string message) {
			Debug.WriteLine($"SlackService.SendMessage: '{message}'");
			var hubState = _bot.State.GetHub(_hub);
			var fullMessage = string.Format("[{0}]\n {1}", _name, message);
			try {
				await _bot.SendAsync(hubState, fullMessage);
			} catch (Exception e) {
				Debug.WriteLine($"SlackService.SendMessage: exception: ${e.ToString()}");
			}
		}
	}
}