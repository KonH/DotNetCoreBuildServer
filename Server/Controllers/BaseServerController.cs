using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Server.BuildConfig;
using Server.Runtime;
using Microsoft.Extensions.Logging;

namespace Server.Controllers {
	public class BaseServerController {
		
		protected BuildServer Server;

		readonly Dictionary<string, Action<RequestArgs>> _handlers = 
			new Dictionary<string, Action<RequestArgs>>();

		ILogger _logger;
		
		protected BaseServerController(LoggerFactory loggerFactory, BuildServer server) {
			_logger = loggerFactory.CreateLogger<BaseServerController>();
			AddHandler("help",   RequestHelp);
			AddHandler("status", RequestStatus);
			AddHandler("build",  StartBuild);
			AddHandler("stop",   StopServer);
			AddHandler("abort",  AbortBuild);
			Server = server;
		}
		
		void AddHandler(string name, Action<RequestArgs> handler) {
			_logger.LogDebug($"AddHandler: \"{name}\" => \"{handler.GetMethodInfo().Name}\"");
			_handlers.Add(name, handler);
		}

		void AddHandler(string name, Action handler) {
			_logger.LogDebug($"AddHandler: \"{name}\" => \"{handler.GetMethodInfo().Name}\"");
			_handlers.Add(name, (_) => handler.Invoke());
		}

		void RequestHelp() {
			_logger.LogDebug("RequestHelp");
			Server.RequestHelp();
		}
		
		void RequestStatus() {
			_logger.LogDebug("RequestStatus");
			Server.RequestStatus();
		}
		
		protected void StartBuild(RequestArgs args) {
			if ((args == null) || (args.Count == 0)) {
				Server.RaiseCommonError("StartBuild: No arguments!", true);
				return;
			}
			var buildName = args[0];
			Build build;
			var builds = Server.FindBuilds();
			if (builds == null) {
				Server.RaiseCommonError("StartBuild: Failed to load builds directory!", true);
				return;
			}
			if (!builds.TryGetValue(buildName, out build)) {
				Server.RaiseCommonError("StartBuild: Wrong build name!", false);
				return;
			}
			if (args.Count - 1 < build.Args.Count) {
				Server.RaiseCommonError(
					$"StartBuild: build required {build.Args.Count} args, but {args.Count - 1} args is provided!",
					true);
				return;
			}
			if (Server.TryInitBuild(build)) {
				var buildArgs = args.Skip(1).ToArray();
				Server.StartBuild(buildArgs);
			}
		}

		protected void AbortBuild() {
			_logger.LogDebug("AbortBuild");
			Server.AbortBuild();
		}

		protected void StopServer() {
			_logger.LogDebug("StopServer");
			Server.StopServer();
		}
		
		protected ServerRequest ConvertMessage(string message) {
			var allParts = message.Split(' ');
			if (allParts.Length <= 0) {
				return ServerRequest.Empty;
			}
			var request = allParts[0];
			var requestArgs = new RequestArgs(allParts.Skip(1));
			_logger.LogDebug($"ConvertMessage: \"{message}\" => [\"{request}\", {requestArgs.Count}]");
			return new ServerRequest(request, requestArgs);
		}
		
		protected void Call(ServerRequest request) {
			_logger.LogDebug($"Call: \"{request.Request}\"");
			if (!request.IsValid) {
				_logger.LogWarning("Call: invalid request, call 'help'");
				_handlers["help"]?.Invoke(request.Args);
				return;
			}
			var handler = _handlers.Get(request.Request);
			_logger.LogDebug($"BaseServerController.Call: handler: \"{handler}\" (is null: {handler == null})");
			handler?.Invoke(request.Args);
		}
	}
}