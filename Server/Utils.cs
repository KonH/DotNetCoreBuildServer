using System;
using System.Collections.Generic;

namespace Server {
	public static class Utils {

		public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) {
			var value = default(TValue);
			dict?.TryGetValue(key, out value);
			return value;
		}

		public static bool GetBoolean<TKey>(this Dictionary<TKey, string> dict, TKey key, bool def) {
			var value = Get(dict, key);
			if ( !string.IsNullOrEmpty(value) ) {
				return bool.Parse(value);
			}
			return def;
		}

		public static int GetInt<TKey>(this Dictionary<TKey, string> dict, TKey key, int def) {
			var value = Get(dict, key);
			if ( !string.IsNullOrEmpty(value) ) {
				return int.Parse(value);
			}
			return def;
		}

		public static string FormatTimeSpan(TimeSpan ts) {
			if ( ts.TotalHours < 1 ) {
				return ts.ToString(@"mm\:ss");
			} else if ( ts.TotalDays < 1 ) {
				return ts.ToString(@"hh\:mm\:ss");
			}
			return ts.ToString(@"dd\.hh\:mm\:ss");
		}

		public static string FormatSeconds(double value) {
			var ts = TimeSpan.FromSeconds(value);
			return FormatTimeSpan(ts);
		}
	}
}