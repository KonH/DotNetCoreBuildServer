using System.Collections.Generic;

namespace Server {
	public static class Utils {

		public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) {
			var value = default(TValue);
			dict?.TryGetValue(key, out value);
			return value;
		}
	}
}