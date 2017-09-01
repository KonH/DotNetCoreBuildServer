using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Server.BuildConfig;
using Server.Controllers;
using Server.Runtime;
using Server.Views;
using SlackBotNet;
using Microsoft.Extensions.Logging;
using SlackBotNet.Messages;

namespace Server.Integrations {
	public class SlackService:IService {
		
		public event Action<string> OnMessage;
		
		public SlackServerController Controller { get; private set; }
		public SlackServerView       View       { get; private set; }
		
		string   _name;
		string   _hub;
		SlackBot _bot;

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
				var loggerFactory = new LoggerFactory();
				loggerFactory.AddConsole();
				return InitBot(server, name, token, hub, loggerFactory);
			}
			Debug.WriteLine(
				$"SlackService.TryInit: wrong arguments: name: \"{name}\", token: \"{token}\", hub: \"{hub}\"");
			return false;
		}
		
		bool InitBot(BuildServer server, string name, string token, string hub, LoggerFactory loggerFactory) {
			_name = name;
			_hub  = hub;
			try {
				InitBotAsync(token, loggerFactory).GetAwaiter().GetResult();
			} catch (Exception e) {
				Debug.WriteLine($"SlackService.InitBot: exception: \"{e}\"");
				return false;
			}
			_bot.When(_bot.State.BotUserId, conv => {
				OnMessage?.Invoke(conv.Text);
				return null;
			});
			Controller = new SlackServerController(this, server);
			View       = new SlackServerView(this, server);
			return true;
		}

		async Task InitBotAsync(string token, LoggerFactory loggerFactory) {
			_bot = await SlackBot.InitializeAsync(token, config => {
				config.LoggerFactory = loggerFactory;
				config.OnSendMessageFailure = OnSendMessageFailure;
			});
		}

		async void OnSendMessageFailure(ISendMessageQueue queue, IMessage message, ILogger logger, Exception exception) {
			if ( message.SendAttempts <= 5 ) {
				logger?.LogWarning($"Failed to send message {message.Text}. Tried {message.SendAttempts} times (exception: {exception})");
				await Task.Delay(1000 * message.SendAttempts);
				queue.Enqueue(message);
				return;
			}
			logger?.LogError($"Gave up trying to send message {message.Text} (exception: {exception})");
		}

		public async void SendMessage(string message) {
			Debug.WriteLine($"SlackService.SendMessage: \"{message}\"");
			var hubState = _bot.State.GetHub(_hub);
			var fullMessage = string.Format("[{0}]\n {1}", _name, message);
			try {
				await _bot.SendAsync(hubState, fullMessage);
			} catch (Exception e) {
				Debug.WriteLine($"SlackService.SendMessage: exception: \"{e}\"");
			}
		}
	}
}