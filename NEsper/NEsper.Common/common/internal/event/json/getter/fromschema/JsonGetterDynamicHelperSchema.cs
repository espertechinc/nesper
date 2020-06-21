///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
	public class JsonGetterDynamicHelperSchema
	{
		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns the json object prop.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static object GetJsonPropertySimpleValue(
			string propertyName,
			IDictionary<string, object> @object)
		{
			return @object.Get(propertyName);
		}

		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns flag whether the json object prop exists.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static bool GetJsonPropertySimpleExists(
			string propertyName,
			IDictionary<string, object> @object)
		{
			return @object.ContainsKey(propertyName);
		}

		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns the json object prop.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="index">index</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static object GetJsonPropertyIndexedValue(
			string propertyName,
			int index,
			IDictionary<string, object> @object)
		{
			var value = @object.Get(propertyName);
			if (value is object[] array) {
				if (index < array.Length) {
					return array[index];
				}
			}

			return null;
		}

		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns the json object prop.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="index">index</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static bool GetJsonPropertyIndexedExists(
			string propertyName,
			int index,
			IDictionary<string, object> @object)
		{
			var value = @object.Get(propertyName);
			if (value is object[] array) {
				return index < array.Length;
			}

			return false;
		}

		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns the json object prop.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="key">key</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static object GetJsonPropertyMappedValue(
			string propertyName,
			string key,
			IDictionary<string, object> @object)
		{
			var value = @object.Get(propertyName);
			if (value is IDictionary<string, object> map) {
				return map.Get(key);
			}

			return null;
		}

		/// <summary>
		///     NOTE: Code-generation-invoked method, method name and parameter order matters
		///     Returns the json object prop.
		/// </summary>
		/// <param name="propertyName">property name</param>
		/// <param name="key">key</param>
		/// <param name="object">json object</param>
		/// <returns>value</returns>
		public static bool GetJsonPropertyMappedExists(
			string propertyName,
			string key,
			IDictionary<string, object> @object)
		{
			var value = @object.Get(propertyName);
			if (value is IDictionary<string, object> map) {
				return map.ContainsKey(key);
			}

			return false;
		}
	}
} // end of namespace