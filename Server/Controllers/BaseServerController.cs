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

		ILogger _logger;
		
		protected BaseServerController(LoggerFactory loggerFactory, BuildServer server) {
			_logger = loggerFactory.CreateLogger<BaseServerController>();
			server.AddCommand(this, "help",   "show this message",                 RequestHelp);
			server.AddCommand(this, "status", "current server status",             RequestStatus);
			server.AddCommand(this, "stop",   "stop server",                       StopServer);
			server.AddCommand(this, "abort",  "stop current build immediately",    AbortBuild);
			server.AddBuildHandler(this, StartBuild);
			Server = server;
		}

		void RequestHelp() {
			_logger.LogDebug("RequestHelp");
			Server.RequestHelp();
		}
		
		void RequestStatus() {
			_logger.LogDebug("RequestStatus");
			Server.RequestStatus();
		}
		
		protected void StartBuild(string buildName, RequestArgs args) {
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
			if (args.Count < build.Args.Count) {
				Server.RaiseCommonError(
					$"StartBuild: build required {build.Args.Count} args, but {args.Count - 1} args is provided!",
					true);
				return;
			}
			if (Server.TryInitBuild(build)) {
				var buildArgs = args.ToArray();
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
			message = message.Trim();
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
				Server.Commands.Call("help", this, request.Args);
				return;
			}
			var builds = Server.FindBuilds();
			foreach ( var build in builds ) {
				if ( request.Request == build.Key ) {
					Server.Commands.CallBuildHandler(this, build.Key, request.Args);
					return;
				}
			}
			var command = Server.Commands.All.Get(request.Request);
			_logger.LogDebug($"BaseServerController.Call: handler: \"{command}\" (is null: {command == null})");
			Server.Commands.Call(request.Request, this, request.Args);
		}
	}
}