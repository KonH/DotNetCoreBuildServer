using System;
using System.Threading.Tasks;
using SlackBotNet;

namespace Server.Integrations {
	public class SlackManager {

		public event Action<string> OnMessage;
		
		SlackBot _bot = null;

		readonly string _name = null;
		readonly string _hub  = null;
		
		public SlackManager(string name, string token, string hub) {
			_name = name;
			_hub  = hub;
			Initialize(token).GetAwaiter().GetResult();
			_bot.When(_bot.State.BotUserId, conv => {
				OnMessage?.Invoke(conv.Text);
				return null;
			});
		}

		async Task Initialize(string token) {
			_bot = await SlackBot.InitializeAsync(token);
		}

		public async void SendMessage(string message) {
			var hubState = _bot.State.GetHub(_hub);
			var fullMessage = string.Format("[{0}]\n {1}", _name, message);
			await _bot.SendAsync(hubState, fullMessage);
		}
	}
}