using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReflectionExtensions;
using SlackBotNet;

namespace SlackBotNetExtensions {
	/// <summary>
	/// Workaround to handle invalid websocket state inside SlackBotNet.SlackBot
	/// It can happens randomly, especially when client running for long time
	/// When it happens, bot refused to handle any messages and logs shown such messages:
	/// 	WARN [SlackBotNet.SlackBot] [0] Not pinging because the socket is not open. Current state is: Aborted
	/// Possible cause of this behaviour is SlackRtmDriver.cs:
	/// 	ConnectAsync(...)
	/// 	{
	/// 		Observable
	///				.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
	///				.Subscribe(async _ =>
	/// 			{
	/// 				...
	/// 				if (this.websocket.State != WebSocketState.Open) // This condition prevent driver from further work
	/// 				{
	/// 					logger.LogWarning($"Not pinging because the socket is not open. Current state is: {this.websocket.State}");
	/// 					return;
	/// 				}
	/// 				...
	/// 			}
	/// 		...
	/// 	}
	/// Method to restart websocket connection is ReconnectRtmAsync, but in called twice below this condition and it can't be reached
	/// To workaround this situation without modifying library sources, we need to call it manually using reflection,
	/// when websocket is invalid state
	/// Layout of required classes:
	/// SlackBot.cs:
	/// 	public class SlackBot
	/// 	{
	/// 		...
	/// 		private readonly IMessageBus messageBus;
	/// 		private readonly ILogger`SlackBot` logger;
	/// 		private IDriver driver; // SlackRtmDriver
	/// 		...
	/// 	}
	/// SlackRtmDriver.cs:
	/// 	class SlackRtmDriver
	/// 	{
	/// 		...
	/// 		private ClientWebSocket websocket;
	/// 		...
	/// 		private async Task`SlackBotState` ReconnectRtmAsync(IMessageBus bus, ILogger logger) // result ignored
	/// 		...
	/// 	}
	/// </summary>
	public class WebsocketResurrector {
		static double                     _defaultInterval = 30;
		static Func<WebSocketState, bool> _defaultSelector = state => state == WebSocketState.Aborted;

		readonly SlackBot                   _bot;
		readonly ILogger                    _logger;
		readonly double                     _checkInterval;
		readonly Func<WebSocketState, bool> _invalidStateSelector;

		bool _stopped = false;

		public WebsocketResurrector(
			SlackBot bot, ILoggerFactory loggerFactory,
			double checkInterval, Func<WebSocketState, bool> invalidStateSelector
		) {
			_bot                  = bot;
			_logger               = loggerFactory?.CreateLogger<WebsocketResurrector>();
			_checkInterval        = checkInterval;
			_invalidStateSelector = invalidStateSelector;
		}

		public WebsocketResurrector(SlackBot bot, ILoggerFactory loggerFactory)
			: this(bot, loggerFactory, _defaultInterval, _defaultSelector) {}

		/// <summary>
		/// Start tracking websocket state
		/// </summary>
		public void Start() {
			Task.Run(async () => {
				while ( !_stopped ) {
					await Task.Delay(TimeSpan.FromSeconds(_checkInterval));
					await TryResurrectWebsocket();
				}
			});
		}

		/// <summary>
		/// Stop tracking websocket state
		/// </summary>
		public void Stop() {
			_stopped = true;
		}

		async Task TryResurrectWebsocket() {
			var driver = _bot.GetPrivateField("driver");
			var client = GetClient(driver);
			var oldState = client.State;
			if ( IsNeedToReconnect(oldState) ) {
				_logger?.LogWarning($"Websocket state is invalid: {oldState}, try to resurrect it");
				await ResurrectWebsocket(driver);
			}
		}

		ClientWebSocket GetClient(object driver) => driver.GetPrivateField<ClientWebSocket>("websocket");
		
		bool IsNeedToReconnect(WebSocketState state) => _invalidStateSelector(state);

		async Task ResurrectWebsocket(object driver) {
			var messageBus            = _bot.GetPrivateField("messageBus");
			var logger                = _bot.GetPrivateField<ILogger<SlackBot>>("logger");
			var reconnectRtmAsyncTask = driver.InvokePrivateMethod("ReconnectRtmAsync", messageBus, logger);
			switch ( reconnectRtmAsyncTask ) {
				case Task task: {
					await task;
					var client = GetClient(driver);
					_logger?.LogWarning($"Reconnect completed, new state is: {client.State}");
					break;
				}

				case null: {
					_logger?.LogError($"Unexpected null result of reconnect method");
					break;
				}

				default: {
					_logger?.LogError($"Invalid result of reconnect method ({reconnectRtmAsyncTask})");
					break;
				}
			}
		}
	}
}