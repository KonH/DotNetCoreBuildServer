using System;
using System.Threading.Tasks;
using Server.BuildConfig;
using Server.Controllers;
using Server.Runtime;
using Server.Views;
using SlackBotNet;
using Microsoft.Extensions.Logging;
using SlackBotNet.Messages;

namespace Server.Services {
	public class SlackService : IService, IContextService {

		readonly RequestContext _context = new RequestContext("Slack");

		public RequestContext Context {
			get {
				return _context;
			}
		}

		public event Action<string> OnMessage;
		
		public SlackServerController Controller { get; private set; }
		public SlackServerView       View       { get; private set; }



		LoggerFactory _loggerFactory;
		ILogger       _logger;
		MessageFormat _messageFormat;

		string   _name;
		string   _hub;
		SlackBot _bot;

		bool IsValidSettings(string name, string token, string hub) {
			return
				!(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hub));
		}
		
		public SlackService(LoggerFactory loggerFactory, MessageFormat messageFormat) {
			_loggerFactory = loggerFactory;
			_logger = _loggerFactory.CreateLogger<SlackService>();
			_messageFormat = messageFormat;
		}

		public bool TryInit(BuildServer server, Project project) {
			string name = server.Name;
			var keys = project.Keys;
			var token = keys.Get("slack_token");
			var hub = keys.Get("slack_hub");
			if (IsValidSettings(name, token, hub)) {
				return InitBot(server, name, token, hub, _loggerFactory);
			}
			_logger.LogDebug(
				$"TryInit: wrong arguments: name: \"{name}\", token: \"{token}\", hub: \"{hub}\"");
			return false;
		}
		
		bool InitBot(BuildServer server, string name, string token, string hub, LoggerFactory loggerFactory) {
			_name = name;
			_hub  = hub;
			try {
				_logger.LogDebug(
					$"InitBot: Start initialize with  \"{name}\", token: \"{token}\", hub: \"{hub}\"");
				InitBotAsync(token, loggerFactory).GetAwaiter().GetResult();
			} catch (Exception e) {
				_logger.LogError($"InitBot: exception: \"{e}\"");
				return false;
			}
			_bot.When(_bot.State.BotUserId, conv => {
				OnMessage?.Invoke(conv.Text);
				return null;
			});
			Controller = new SlackServerController(_loggerFactory, this, server);
			View       = new SlackServerView(_loggerFactory, this, server, _messageFormat);
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
				logger?.LogWarning($"OnSendMessageFailure. Failed to send message {message.Text}. Tried {message.SendAttempts} times (exception: {exception})");
				await Task.Delay(1000 * message.SendAttempts);
				queue.Enqueue(message);
				return;
			}
			logger?.LogError($"OnSendMessageFailure. Gave up trying to send message {message.Text} (exception: {exception})");
		}

		public async void SendMessage(string message) {
			_logger.LogDebug($"SendMessage: \"{message}\"");
			var hubState = _bot.State.GetHub(_hub);
			try {
				await _bot.SendAsync(hubState, message);
			} catch (Exception e) {
				_logger.LogError($"SendMessage: exception: \"{e}\"");
			}
		}
	}
}