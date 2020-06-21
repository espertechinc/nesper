///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    public class JsonFieldGetterHelperProvided
    {
        public static object GetJsonProvidedMappedProp(
            object underlying,
            FieldInfo field,
            string key)
        {
            var result = GetJsonProvidedSimpleProp(underlying, field);
            return CollectionUtil.GetMapValueChecked(result, key);
        }

        public static object GetJsonProvidedIndexedProp(
            object underlying,
            FieldInfo field,
            int index)
        {
            var result = GetJsonProvidedSimpleProp(underlying, field);
            return CollectionUtil.ArrayValueAtIndex(result, index);
        }

        public static object HandleJsonProvidedCreateFragmentSimple(
            object underlying,
            FieldInfo field,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            var prop = GetJsonProvidedSimpleProp(underlying, field);
            if (prop == null) {
                return null;
            }

            if (fragmentType is JsonEventType) {
                return factory.AdapterForTypedJson(prop, fragmentType);
            }

            return factory.AdapterForTypedBean(prop, fragmentType);
        }

        public static object GetJsonProvidedSimpleProp(
            object @object,
            FieldInfo field)
        {
            try {
                return field.GetValue(@object);
            }
            catch (MemberAccessException ex) {
                throw new PropertyAccessException("Failed to access field '" + field.Name + "' of class '" + field.DeclaringType.Name + "': " + ex.Message, ex);
            }
        }

        public static object HandleJsonProvidedCreateFragmentArray(
            object value,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            if (value == null) {
                return null;
            }

            var len = Array.GetLength(value);
            var events = new EventBean[len];
            if (fragmentType is JsonEventType) {
                for (var i = 0; i < len; i++) {
                    object item = Array.Get(value, i);
                    events[i] = factory.AdapterForTypedJson(item, fragmentType);
                }
            }
            else {
                for (var i = 0; i < len; i++) {
                    object item = Array.Get(value, i);
                    events[i] = factory.AdapterForTypedBean(item, fragmentType);
                }
            }

            return events;
        }

        public static bool GetJsonProvidedMappedExists(
            object underlying,
            FieldInfo field,
            string key)
        {
            var result = GetJsonProvidedSimpleProp(underlying, field);
            return CollectionUtil.GetMapKeyExistsChecked(result, key);
        }

        public static bool GetJsonProvidedIndexedPropExists(
            object @object,
            FieldInfo field,
            int index)
        {
            var array = GetJsonProvidedSimpleProp(@object, field);
            return CollectionUtil.ArrayExistsAtIndex(array, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="prop">value</param>
        /// <param name="fragmentType">event type</param>
        /// <param name="factory">factory</param>
        /// <param name="index">index</param>
        /// <returns>event bean or null</returns>
        public static EventBean HandleJsonProvidedCreateFragmentIndexed(
            object prop,
            int index,
            EventType fragmentType,
            EventBeanTypedEventFactory factory)
        {
            prop = CollectionUtil.ArrayValueAtIndex(prop, index);
            if (prop == null) {
                return null;
            }

            return factory.AdapterForTypedJson(prop, fragmentType);
        }
    }
} // end of namespace