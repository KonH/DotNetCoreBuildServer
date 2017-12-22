using System;
using System.Collections.Generic;

namespace Server {
	public static class Utils {

		public static bool TryParseTimeSpan(string str, string ending, Func<int, TimeSpan> generator, out TimeSpan span) {
			if ( str.EndsWith(ending) ) {
				var valueStr = str.Substring(0, str.Length - ending.Length);
				if ( int.TryParse(valueStr, out int value) ) {
					span = generator(value);
					return true;
				}
			}
			return false;
		}

		public static bool TryParseTimeSpan(string str, out TimeSpan span) {
			return
				TryParseTimeSpan(str, "h", t => TimeSpan.FromHours(t), out span) ||
				TryParseTimeSpan(str, "m", t => TimeSpan.FromMinutes(t), out span) ||
				TryParseTimeSpan(str, "s", t => TimeSpan.FromSeconds(t), out span);
		}

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

		public static List<string> SplitByWhitespaces(this string str) {
			var parts = new List<string>();
			var accum = "";
			foreach ( var c in str ) {
				if ( char.IsWhiteSpace(c) ) {
					if ( !string.IsNullOrWhiteSpace(accum) ) {
						parts.Add(accum);
						accum = "";
					}
				} else {
					accum += c;
				}
			}
			if ( !string.IsNullOrWhiteSpace(accum) ) {
				parts.Add(accum);
			}
			return parts;
		}
	}
}