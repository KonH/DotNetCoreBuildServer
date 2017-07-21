using System;
using System.Collections.Generic;
using Server.BuildConfig;
using Server.Runtime;

namespace Server.Manager {
	public abstract class ServerManager {
		
		public bool Alive => Server != null;
		
		protected BuildServer Server = null;
		
		readonly Dictionary<string, Action<string[]>> _handlers = 
			new Dictionary<string, Action<string[]>>();
		
		protected ServerManager(BuildServer server) {
			AddHandler("build", StartBuild);
			AddHandler("stop", StopServer);
			Server = server;
		}
		
		void AddHandler(string name, Action<string[]> handler) {
			_handlers.Add(name, handler);
		}

		void AddHandler(string name, Action handler) {
			_handlers.Add(name, (_) => handler.Invoke());
		}

		protected void StartBuild() {
			var build = Build.Load("dev_build.json");
			Server.InitBuild(build);
			Server.StartBuild();
		}

		protected void StopServer() {
			Server.StopBuild();
			Server = null;
		}
		
		protected void Call(string request, params string[] args) {
			Action<string[]> handler = null;
			_handlers.TryGetValue(request, out handler);
			handler?.Invoke(args);
		}
	}
}