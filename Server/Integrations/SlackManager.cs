using System;
using System.Threading.Tasks;
using SlackBotNet;

namespace Server.Integrations {
	public class SlackManager {

		public event Action<string> OnMessage;
		
		SlackBot _bot = null;
		
		readonly string _hub = null;
		
		public SlackManager(string token, string hub) {
			_hub = hub;
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
			await _bot.SendAsync(hubState, message);
		}
	}
}