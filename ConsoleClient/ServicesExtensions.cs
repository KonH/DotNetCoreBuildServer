using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server;
using Server.Services;

namespace ConsoleClient {
	static class ServicesExtensions {
		public static void TryAddNotificationService(this List<IService> services, ILoggerFactory loggerFactory) {
			var logger = loggerFactory.CreateLogger("ServicesExtensions");
			var notifyValue = EnvManager.FindArgumentValue("notify");
			if ( notifyValue != null ) {
				logger.LogDebug($"TryAddNotificationService: Found notify arg value: '{notifyValue}'");
				if ( Utils.TryParseTimeSpan(notifyValue, out TimeSpan span) ) {
					logger.LogDebug($"TryAddNotificationService: Parsed TimeSpan: '{span}'");
					var service = new NotificationService(loggerFactory, span);
					services.Add(service);
					return;
				}
			}
			logger.LogDebug("TryAddNotificationService: Notify arg isn't found.");
		}
	}
}
