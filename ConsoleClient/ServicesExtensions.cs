using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server.Services;

namespace ConsoleClient {
	static class ServicesExtensions {
		static bool TryParseTimeSpan(string str, string ending, Func<int, TimeSpan> generator, out TimeSpan span) {
			if ( str.EndsWith(ending) ) {
				var valueStr = str.Substring(0, str.Length - ending.Length);
				if ( int.TryParse(valueStr, out int value) ) {
					span = generator(value);
					return true;
				}
			}
			return false;
		}

		static bool TryParseTimeSpan(string str, out TimeSpan span) {
			return
				TryParseTimeSpan(str, "h", t => TimeSpan.FromHours(t), out span) ||
				TryParseTimeSpan(str, "m", t => TimeSpan.FromMinutes(t), out span) ||
				TryParseTimeSpan(str, "s", t => TimeSpan.FromSeconds(t), out span);
		}

		public static void TryAddNotificationService(this List<IService> services, ILoggerFactory loggerFactory) {
			var logger = loggerFactory.CreateLogger("ServicesExtensions");
			var notifyValue = EnvManager.FindArgumentValue("notify");
			if ( notifyValue != null ) {
				logger.LogDebug($"TryAddNotificationService: Found notify arg value: '{notifyValue}'");
				if ( TryParseTimeSpan(notifyValue, out TimeSpan span) ) {
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
