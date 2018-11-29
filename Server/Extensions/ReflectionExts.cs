using System;
using System.Data;
using System.Reflection;

namespace ReflectionExtensions {
	public static class ReflectionExts {
		static void ArgumentNotNull(object instance, string name) {
			if ( instance == null ) {
				throw new ArgumentNullException(name);
			}
		}

		static void ValueNotNull(object instance, string message) {
			if ( instance == null ) {
				throw new InvalidExpressionException(message);
			}
		}
		
		public static object GetPrivateField(this object instance, string name) {
			ArgumentNotNull(instance, nameof(instance));
			var type      = instance.GetType();
			var fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
			ValueNotNull(fieldInfo, $"Can't find private instance field '{name}' on object of type {type.FullName}");
			var fieldValue = fieldInfo.GetValue(instance);
			return fieldValue;
		}
		
		public static T GetPrivateField<T>(this object instance, string name) {
			var rawFieldValue = GetPrivateField(instance, name);
			var fieldValue    = (T)rawFieldValue;
			return fieldValue;
		}

		public static object InvokePrivateMethod(this object instance, string name, params object[] args) {
			ArgumentNotNull(instance, nameof(instance));
			var type       = instance.GetType();
			var methodInfo = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
			ValueNotNull(methodInfo, $"Can't find private instance method '{name}' on object of type {type.FullName}");
			var result = methodInfo.Invoke(instance, args);
			return result;
		}
	}
}