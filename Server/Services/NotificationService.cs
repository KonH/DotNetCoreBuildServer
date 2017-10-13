using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Server.BuildConfig;
using Server.Runtime;

namespace Server.Services {
	public class NotificationService : IService {
		ILogger _logger;

		BuildServer  _server;
		BuildProcess _process;

		TimeSpan _minInterval;
		DateTime _lastMessageTime;

		public NotificationService(ILoggerFactory loggerFactory, TimeSpan minInterval) {
			_logger = loggerFactory.CreateLogger<NotificationService>();
			_minInterval = minInterval;
		}

		public bool TryInit(BuildServer server, Project project) {
			_server = server;
			_server.OnInitBuild += OnInitBuild;
			return true;
		}

		private void OnInitBuild(RequestContext _, BuildProcess process) {
			_logger.LogDebug("OnInitBuild");
			_process = process;
			process.TaskStarted += OnTaskStarted;
			process.BuildDone += OnBuildDone;
			_lastMessageTime = DateTime.Now;
		}

		private void OnTaskStarted(BuildTask task) {
			if ( _process != null ) {
				_logger.LogDebug($"OnTaskDone: {_process.Name}: {task.Node.Name}");
				var now = DateTime.Now;
				if ( now > _lastMessageTime.Add(_minInterval) ) {
					CallStatus();
					_lastMessageTime = now;
				}
			}
		}

		void CallStatus() {
			var viewServices =_server.Services.FindAll(service => service is IContextService).Select(s => s as IContextService);
			_logger.LogDebug($"CallStatus: ViewServices: {viewServices.Count()}");
			foreach ( var service in viewServices ) {
				_logger.LogDebug($"CallStatus: service: {service.GetType().Name}");
				if ( (service != null) && (service.Context != null) ) {
					_logger.LogDebug($"CallStatus: It is IContextService with Context: '{service.Context.Name}'");
					_server.RequestStatus(service.Context);
				}
			}
		}

		private void OnBuildDone() {
			if ( _process != null ) {
				_logger.LogDebug($"OnBuildDone: {_process.Name}: {_process.IsSuccess}");
				_process.BuildDone -= OnBuildDone;
				_process.TaskStarted -= OnTaskStarted;
			}
		}
	}
}
