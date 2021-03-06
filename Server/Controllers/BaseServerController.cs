using System.Linq;
using Server.BuildConfig;
using Server.Runtime;
using Microsoft.Extensions.Logging;

namespace Server.Controllers {
	public class BaseServerController {
		
		protected BuildServer Server;
		protected RequestContext Context;

		ILogger _logger;
		
		protected BaseServerController(LoggerFactory loggerFactory, RequestContext context, BuildServer server) {
			_logger = loggerFactory.CreateLogger<BaseServerController>();
			server.AddCommand("help",   "show this message",                 RequestHelp);
			server.AddCommand("status", "current server status",             RequestStatus);
			server.AddCommand("stop",   "stop server",                       StopServer);
			server.AddCommand("abort",  "stop current build immediately",    AbortBuild);
			server.AddBuildHandler(StartBuild);
			Server = server;
			Context = context;
		}

		void RequestHelp(RequestContext context, RequestArgs args) {
			if ( context != Context ) {
				return;
			}
			_logger.LogDebug("RequestHelp");
			if ( args.Count > 0 ) {
				Server.RequestHelp(Context, args[0]);
			} else {
				Server.RequestHelp(Context,null);
			}
		}
		
		void RequestStatus(RequestContext context) {
			if ( context != Context ) {
				return;
			}
			_logger.LogDebug("RequestStatus");
			Server.RequestStatus(Context);
		}
		
		protected void StartBuild(string buildName, RequestContext context, RequestArgs args) {
			if ( context != Context ) {
				return;
			}
			Build build;
			var builds = Server.FindBuilds();
			if (builds == null) {
				Server.RaiseCommonError("StartBuild: Failed to load builds directory!", true);
				return;
			}
			if (!builds.TryGetValue(buildName, out build)) {
				Server.RaiseCommonError("StartBuild: Wrong Build task name!", false);
				return;
			}
			if (args.Count < build.Args.Count) {
				Server.RaiseCommonError(
					$"StartBuild: Build task requires {build.Args.Count} arguments, but {args.Count} arguments are provided!\nUse 'help {build.Name}' to get more info.",
					true);
				return;
			}
			var buildArgs = args.ToArray();
			if (Server.TryInitBuild(Context, build, buildArgs)) {
				Server.StartBuild(buildArgs);
			}
		}

		protected void AbortBuild(RequestContext context) {
			if ( context != Context ) {
				return;
			}
			_logger.LogDebug("AbortBuild");
			Server.AbortBuild();
		}

		protected void StopServer(RequestContext context) {
			if ( context != Context ) {
				return;
			}
			_logger.LogDebug("StopServer");
			Server.StopServer();
		}
		
		protected ServerRequest ConvertMessage(RequestContext context, string message) {
			message = message.Trim();
			var allParts = message.SplitByWhitespaces();
			if (allParts.Count <= 0) {
				return ServerRequest.Empty;
			}
			var request = allParts[0];
			var requestArgs = new RequestArgs(allParts.Skip(1));
			_logger.LogDebug($"ConvertMessage: \"{message}\" => [\"{request}\", {requestArgs.Count}]");
			return new ServerRequest(request, context, requestArgs);
		}
		
		protected void Call(ServerRequest request) {
			_logger.LogDebug($"Call: \"{request.Request}\"");
			if (!request.IsValid) {
				_logger.LogWarning("Call: invalid request, call 'help'");
				Server.Commands.Call("help", Context, request.Args);
				return;
			}
			var builds = Server.FindBuilds();
			foreach ( var build in builds ) {
				if ( request.Request == build.Key ) {
					Server.Commands.CallBuildHandler(build.Key, Context, request.Args);
					return;
				}
			}
			var command = Server.Commands.All.Get(request.Request);
			_logger.LogDebug($"BaseServerController.Call: handler: \"{command}\" (is null: {command == null})");
			Server.Commands.Call(request.Request, Context, request.Args);
		}
	}
}