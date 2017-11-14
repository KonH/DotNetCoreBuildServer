using System;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
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

		Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();

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
			_process            = process;
			_lastMessageTime    = DateTime.Now;
			process.TaskStarted += OnTaskStarted;
			process.TaskDone    += OnTaskDone;
			process.BuildDone   += OnBuildDone;
		}

		private void OnTaskStarted(BuildTask task) {
			if ( _process != null ) {
				_logger.LogDebug($"OnTaskStarted: {_process.Name}: {task.Node.Name}");
				var node = task.Node;
				var warningTimeout = node.Args.Get("warning_timeout");
				if ( !string.IsNullOrEmpty(warningTimeout) ) {
					if ( Utils.TryParseTimeSpan(warningTimeout, out TimeSpan span) ) {
						_logger.LogDebug($"WarningTimeout: {warningTimeout} ({span})");
						StartWarningTimer(task.Node.Name, span);
					}
				}
				var now = DateTime.Now;
				if ( now > _lastMessageTime.Add(_minInterval) ) {
					CallStatus();
					_lastMessageTime = now;
				}
			}
		}

		void StartWarningTimer(string taskName, TimeSpan interval) {
			if ( _timers.ContainsKey(taskName) ) {
				_timers.Remove(taskName);
			}
			var timer = new Timer(interval.TotalMilliseconds);
			timer.Elapsed += OnTimerElapsed;
			_timers.Add(taskName, timer);
			timer.Start();
		}

		private void OnTimerElapsed(object sender, ElapsedEventArgs e) {
			var timer = sender as Timer;
			if ( _timers.ContainsValue(timer) ) {
				var taskName = string.Empty;
				foreach ( var curTimer in _timers ) {
					if ( curTimer.Value == timer ) {
						taskName = curTimer.Key;
						break;
					}
				}
				if ( !string.IsNullOrEmpty(taskName) ) {
					RaiseWarningByTimer(taskName);
					StopWarningTimer(taskName);
				}
			}
		}

		void RaiseWarningByTimer(string taskName) {
			ForEachContextService(service => {
				_server.RaiseCommonMessage(service.Context, $"Task isn't completed, but time is exceeded: '{taskName}'!");
			});
		}

		void StopWarningTimer(string taskName) {
			if ( _timers.TryGetValue(taskName, out Timer timer) ) {
				timer.Stop();
				timer.Elapsed -= OnTimerElapsed; 
				_timers.Remove(taskName);
			}
		}

		private void OnTaskDone(BuildTask task) {
			StopWarningTimer(task.Node.Name);
		}

		void ForEachContextService(Action<IContextService> action) {
			var viewServices = _server.Services.FindAll(service => service is IContextService).Select(s => s as IContextService);
			_logger.LogDebug($"ForEachContextService: ViewServices: {viewServices.Count()}");
			foreach ( var service in viewServices ) {
				if ( (service != null) && (service.Context != null) ) {
					action(service);
				}
			}
		}

		void CallStatus() {
			ForEachContextService(service => {
				_logger.LogDebug($"CallStatus: It is IContextService with Context: '{service.Context.Name}'");
				_server.RequestStatus(service.Context);
			});
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
