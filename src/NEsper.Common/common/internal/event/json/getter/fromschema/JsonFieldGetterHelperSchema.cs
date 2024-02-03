///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.@event.json.getter.fromschema.JsonExceptionMessages;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    public class JsonFieldGetterHelperSchema
    {
        public static object GetJsonSimpleProp(
            JsonUnderlyingField field,
            object @object)
        {
            var und = (JsonEventObjectBase)@object;
            return und.GetNativeValue(field.PropertyNumber);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">object</param>
        /// <param name="propertyNumber"></param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException property access exceptions</throws>
        public static object GetJsonIndexedProp(
            object @object,
            int propertyNumber,
            int index)
        {
            var und = (JsonEventObjectBase)@object;
            if (und.TryGetNativeValue(propertyNumber, out var value)) {
                if (value == null) {
                    return null;
                }

                if (value is Array array) {
                    return CollectionUtil.ArrayValueAtIndex(array, index);
                }

                throw new InvalidOperationException(MESSAGE_VALUE_NOT_AN_ARRAY);
            }

            throw new KeyNotFoundException();
        }

        public static bool GetJsonIndexedPropExists(
            object @object,
            JsonUnderlyingField field,
            int index)
        {
            var und = (JsonEventObjectBase)@object;
            if (und.TryGetNativeValue(field.PropertyNumber, out var value)) {
                if (value == null) {
                    return false;
                }

                if (value is Array array) {
                    return CollectionUtil.ArrayExistsAtIndex(array, index);
                }

                throw new InvalidOperationException(MESSAGE_VALUE_NOT_AN_ARRAY);
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">object</param>
        /// <param name="propertyNumber">property number</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException property access exceptions</throws>
        public static object GetJsonMappedProp(
            object @object,
            int propertyNumber,
            string key)
        {
            var und = (JsonEventObjectBase)@object;
            if (und.TryGetNativeValue(propertyNumber, out var result)) {
                return CollectionUtil.GetMapValueChecked(result, key);
            }

            throw new KeyNotFoundException();
        }

        public static bool GetJsonMappedExists(
            object @object,
            int propertyNumber,
            string key)
        {
            var und = (JsonEventObjectBase)@object;
            if (und.TryGetNativeValue(propertyNumber, out var result)) {
                return CollectionUtil.GetMapKeyExistsChecked(result, key);
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propertyNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <returns>event bean or null</returns>
        public static EventBean HandleJsonCreateFragmentSimple(
            JsonEventObjectBase und,
            int propertyNumber,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            if (und.TryGetNativeValue(propertyNumber, out var prop)) {
                if (prop == null) {
                    return null;
                }

                return factory.AdapterForTypedJson(prop, fragmentType);
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propertyNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <param name="index">index</param>
        /// <returns>event bean or null</returns>
        public static EventBean HandleJsonCreateFragmentIndexed(
            JsonEventObjectBase und,
            int propertyNumber,
            int index,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            if (und.TryGetNativeValue(propertyNumber, out var prop)) {
                if (prop is Array array) {
                    prop = CollectionUtil.ArrayValueAtIndex(array, index);
                    if (prop == null) {
                        return null;
                    }

                    return factory.AdapterForTypedJson(prop, fragmentType);
                }

                throw new InvalidOperationException(MESSAGE_VALUE_NOT_AN_ARRAY);
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propertyNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <returns>event bean or null</returns>
        public static EventBean[] HandleJsonCreateFragmentArray(
            JsonEventObjectBase und,
            int propertyNumber,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            if (und.TryGetNativeValue(propertyNumber, out var value)) {
                if (value == null) {
                    return null;
                }

                var asArray = (Array)value;
                var len = asArray.Length;
                var events = new EventBean[len];
                for (var i = 0; i < len; i++) {
                    var item = asArray.GetValue(i);
                    events[i] = factory.AdapterForTypedJson(item, fragmentType);
                }

                return events;
            }

            throw new KeyNotFoundException();
        }
    }
} // end of namespace