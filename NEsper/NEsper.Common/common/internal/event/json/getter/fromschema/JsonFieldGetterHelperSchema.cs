///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    public class JsonFieldGetterHelperSchema
    {
        public static object GetJsonSimpleProp(
            JsonUnderlyingField field,
            object @object)
        {
            var und = (JsonEventObjectBase) @object;
            return und.GetNativeValue(field.PropertyNumber);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">object</param>
        /// <param name="propertyNumber">field number</param>
        /// <param name="index">index</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException property access exceptions</throws>
        public static object GetJsonIndexedProp(
            object @object,
            int propertyNumber,
            int index)
        {
            var und = (JsonEventObjectBase) @object;
            var array = und.GetNativeValue(propertyNumber);
            return ArrayValueAtIndex(array, index);
        }

        public static bool GetJsonIndexedPropExists(
            object @object,
            JsonUnderlyingField field,
            int index)
        {
            var und = (JsonEventObjectBase) @object;
            var array = und.GetNativeValue(field.PropertyNumber);
            return CollectionUtil.ArrayExistsAtIndex(array, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">object</param>
        /// <param name="propertyNumber">field number</param>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException property access exceptions</throws>
        public static object GetJsonMappedProp(
            object @object,
            int propertyNumber,
            string key)
        {
            var und = (JsonEventObjectBase) @object;
            var result = und.GetNativeValue(propertyNumber);
            return GetMapValueChecked(result, key);
        }

        public static bool GetJsonMappedExists(
            object @object,
            int propertyNumber,
            string key)
        {
            var und = (JsonEventObjectBase) @object;
            var result = und.GetNativeValue(propertyNumber);
            return GetMapKeyExistsChecked(result, key);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <returns>event bean or null</returns>
        public static EventBean HandleJsonCreateFragmentSimple(
            JsonEventObjectBase und,
            int propNumber,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            var prop = und.GetNativeValue(propNumber);
            if (prop == null) {
                return null;
            }

            return factory.AdapterForTypedJson(prop, fragmentType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <param name="index">index</param>
        /// <returns>event bean or null</returns>
        public static EventBean HandleJsonCreateFragmentIndexed(
            JsonEventObjectBase und,
            int propNumber,
            int index,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            var prop = und.GetNativeValue(propNumber);
            prop = CollectionUtil.ArrayValueAtIndex(prop, index);
            if (prop == null) {
                return null;
            }

            return factory.AdapterForTypedJson(prop, fragmentType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="und">underlying</param>
        /// <param name="propNumber">property number</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <returns>event bean or null</returns>
        public static EventBean[] HandleJsonCreateFragmentArray(
            JsonEventObjectBase und,
            int propNumber,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            var value = und.GetNativeValue(propNumber);
            if (value == null) {
                return null;
            }

            var asArray = (Array) value;
            var len = asArray.Length;
            var events = new EventBean[len];
            for (var i = 0; i < len; i++) {
                object item = asArray.GetValue(i);
                events[i] = factory.AdapterForTypedJson(item, fragmentType);
            }

            return events;
        }
    }
} // end of namespace