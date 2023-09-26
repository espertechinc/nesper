///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

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
            if (value == null) {
                return null;
            }

            if (value is Array array) {
                if (index < array.Length) {
                    return array.GetValue(index);
                }
            }
            else if (value is IList<object> list) {
                if (list.Count > index) {
                    list[index] = value;
                }
            }
            else if (value.GetType().IsGenericList()) {
                list = MagicMarker.SingletonInstance.GetList(value);
                if (list.Count > index) {
                    list[index] = value;
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
            if (value == null) {
                return false;
            }

            if (value is Array array) {
                return index < array.Length;
            }
            else if (value is IList<object> list) {
                return index < list.Count;
            }
            else if (value.GetType().IsGenericList()) {
                list = MagicMarker.SingletonInstance.GetList(value);
                return index < list.Count;
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
            if (value == null) {
                return null;
            }

            if (value is IDictionary<string, object> map) {
                return map.Get(key);
            }

            var valueType = value.GetType();
            if (valueType.IsGenericStringDictionary()) {
                var magicMap = MagicMarker.SingletonInstance.GetStringDictionaryFactory(valueType).Invoke(value);
                if (magicMap != null) {
                    return magicMap;
                }
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
            if (value == null) {
                return false;
            }

            if (value is IDictionary<string, object> map) {
                return map.ContainsKey(key);
            }

            var valueType = value.GetType();
            if (valueType.IsGenericStringDictionary()) {
                var magicMap = MagicMarker.SingletonInstance.GetStringDictionaryFactory(valueType).Invoke(value);
                if (magicMap != null) {
                    return magicMap.ContainsKey(key);
                }
            }

            return false;
        }
    }
} // end of namespace