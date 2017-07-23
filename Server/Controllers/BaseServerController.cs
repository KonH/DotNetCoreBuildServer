using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Server.BuildConfig;
using Server.Runtime;

namespace Server.Controllers {
	public class BaseServerController {
		
		protected BuildServer Server;

		readonly Dictionary<string, Action<RequestArgs>> _handlers = 
			new Dictionary<string, Action<RequestArgs>>();
		
		protected BaseServerController(BuildServer server) {
			AddHandler("help",   RequestHelp);
			AddHandler("status", RequestStatus);
			AddHandler("build",  StartBuild);
			AddHandler("stop",   StopServer);
			Server = server;
		}
		
		void AddHandler(string name, Action<RequestArgs> handler) {
			Debug.WriteLine($"BaseServerController.AddHandler: '{name}' => {handler.GetMethodInfo().Name}");
			_handlers.Add(name, handler);
		}

		void AddHandler(string name, Action handler) {
			Debug.WriteLine($"BaseServerController.AddHandler: '{name}' => {handler.GetMethodInfo().Name}");
			_handlers.Add(name, (_) => handler.Invoke());
		}

		void RequestHelp() {
			Debug.WriteLine("BaseServerController.RequestHelp");
			Server.RequestHelp();
		}
		
		void RequestStatus() {
			Debug.WriteLine("BaseServerController.RequestStatus");
			Server.RequestStatus();
		}
		
		protected void StartBuild(RequestArgs args) {
			if ((args == null) || (args.Count == 0)) {
				Server.RaiseCommonError("StartBuild: No arguments!", true);
				return;
			}
			var buildName = args[0];
			Build build;
			if (!Server.Builds.TryGetValue(buildName, out build)) {
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

		protected void StopServer() {
			Debug.WriteLine("BaseServerController.StopServer");
			Server.StopServer();
		}
		
		protected ServerRequest ConvertMessage(string message) {
			var allParts = message.Split(' ');
			if (allParts.Length <= 0) {
				return ServerRequest.Empty;
			}
			var request = allParts[0];
			var requestArgs = new RequestArgs(allParts.Skip(1));
			Debug.WriteLine($"BaseServerController: '{message}' => ['{request}', '{requestArgs.Count}']");
			return new ServerRequest(request, requestArgs);
		}
		
		protected void Call(ServerRequest request) {
			Debug.WriteLine($"BaseServerController.Call: '{request.Request}'");
			if (!request.IsValid) {
				Debug.WriteLine("BaseServerController.Call: invalid request, call 'help'");
				_handlers["help"]?.Invoke(request.Args);
				return;
			}
			var handler = _handlers.Get(request.Request);
			Debug.WriteLine($"BaseServerController.Call: handler: {handler} (is null: {handler == null})");
			handler?.Invoke(request.Args);
		}
	}
}