///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Utility for handling properties for the purpose of merging and versioning by revision
    /// event types.
    /// </summary>
    public class PropertyUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Returns a multi-key for an event and key property getters </summary>
        /// <param name="theEvent">to get keys for</param>
        /// <param name="keyPropertyGetters">getters to use</param>
        /// <returns>key</returns>
        public static Object GetKeys(EventBean theEvent, EventPropertyGetter[] keyPropertyGetters)
        {
            if (keyPropertyGetters.Length == 1)
            {
                return keyPropertyGetters[0].Get(theEvent);
            }

            var keys = new Object[keyPropertyGetters.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keyPropertyGetters[i].Get(theEvent);
            }
            return new MultiKeyUntyped(keys);
        }

        /// <summary>From a list of property groups that include contributing event types, build a map of contributing event types and their type descriptor. </summary>
        /// <param name="groups">property groups</param>
        /// <param name="changesetProperties">properties that change between groups</param>
        /// <param name="keyProperties">key properties</param>
        /// <returns>map of event type and type information</returns>
        public static IDictionary<EventType, RevisionTypeDesc> GetPerType(PropertyGroupDesc[] groups,
                                                                          String[] changesetProperties,
                                                                          String[] keyProperties)
        {
            IDictionary<EventType, RevisionTypeDesc> perType = new Dictionary<EventType, RevisionTypeDesc>();
            foreach (PropertyGroupDesc group in groups)
            {
                foreach (EventType type in group.Types.Keys)
                {
                    EventPropertyGetter[] changesetGetters = GetGetters(type, changesetProperties);
                    EventPropertyGetter[] keyGetters = GetGetters(type, keyProperties);
                    var pair = new RevisionTypeDesc(keyGetters, changesetGetters, group);
                    perType.Put(type, pair);
                }
            }
            return perType;
        }

        /// <summary>From a list of property groups that include multiple group numbers for each property, make a map of group numbers per property. </summary>
        /// <param name="groups">property groups</param>
        /// <returns>map of property name and group number</returns>
        public static IDictionary<String, int[]> GetGroupsPerProperty(PropertyGroupDesc[] groups)
        {
            IDictionary<String, int[]> groupsNumsPerProp = new Dictionary<String, int[]>();
            foreach (PropertyGroupDesc group in groups)
            {
                foreach (String property in group.Properties)
                {
                    int[] value = groupsNumsPerProp.Get(property);
                    if (value == null)
                    {
                        value = new int[1];
                        groupsNumsPerProp.Put(property, value);
                        value[0] = group.GroupNum;
                    }
                    else
                    {
                        var copy = new int[value.Length + 1];
                        Array.Copy(value, 0, copy, 0, value.Length);
                        copy[value.Length] = group.GroupNum;
                        Array.Sort(copy);
                        groupsNumsPerProp.Put(property, copy);
                    }
                }
            }
            return groupsNumsPerProp;
        }

        /// <summary>Analyze multiple event types and determine common property sets that form property groups. </summary>
        /// <param name="allProperties">property names to look at</param>
        /// <param name="deltaEventTypes">all types contributing</param>
        /// <param name="names">names of properies</param>
        /// <returns>groups</returns>
        public static PropertyGroupDesc[] AnalyzeGroups(String[] allProperties, EventType[] deltaEventTypes,
                                                        String[] names)
        {
            if (deltaEventTypes.Length != names.Length)
            {
                throw new ArgumentException("Delta event type number and name number of elements don't match");
            }
            allProperties = CopyAndSort(allProperties);

            var result = new LinkedHashMap<MultiKey<String>, PropertyGroupDesc>();
            var currentGroupNum = 0;

            for (int i = 0; i < deltaEventTypes.Length; i++)
            {
                MultiKey<String> props = GetPropertiesContributed(deltaEventTypes[i], allProperties);
                if (props.Array.Length == 0)
                {
                    Log.Warn("Event type named '" + names[i] +
                             "' does not contribute (or override) any properties of the revision event type");
                    continue;
                }

                PropertyGroupDesc propertyGroup = result.Get(props);
                IDictionary<EventType, String> typesForGroup;
                if (propertyGroup == null)
                {
                    typesForGroup = new Dictionary<EventType, String>();
                    propertyGroup = new PropertyGroupDesc(currentGroupNum++, typesForGroup, props.Array);
                    result.Put(props, propertyGroup);
                }
                else
                {
                    typesForGroup = propertyGroup.Types;
                }
                typesForGroup.Put(deltaEventTypes[i], names[i]);
            }

            PropertyGroupDesc[] array = Collections.ToArray(result.Values);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".analyzeGroups " + array.Render());
            }
            return array;
        }

        private static MultiKey<String> GetPropertiesContributed(EventType deltaEventType,
                                                                 ICollection<string> allPropertiesSorted)
        {
            var props = new SortedSet<String>();
            foreach (String property in deltaEventType.PropertyNames)
            {
                if (allPropertiesSorted.Contains(property))
                {
                    props.Add(property);
                }
            }
            return new MultiKey<String>(props.ToArray());
        }

        /// <summary>Copy an sort the input array. </summary>
        /// <param name="input">to sort</param>
        /// <returns>sorted copied array</returns>
        protected internal static String[] CopyAndSort(ICollection<string> input)
        {
            String[] result = Collections.ToArray(input);
            Array.Sort(result);
            return result;
        }

        /// <summary>Return getters for property names. </summary>
        /// <param name="eventType">type to get getters from</param>
        /// <param name="propertyNames">names to get</param>
        /// <returns>getters</returns>
        public static EventPropertyGetter[] GetGetters(EventType eventType, String[] propertyNames)
        {
            var getters = new EventPropertyGetter[propertyNames.Length];
            for (int i = 0; i < getters.Length; i++)
            {
                getters[i] = eventType.GetGetter(propertyNames[i]);
            }
            return getters;
        }

        /// <summary>
        /// Remove from values all removeValues and build a unique sorted result array.
        /// </summary>
        /// <param name="values">to consider</param>
        /// <param name="removeValues">values to remove from values</param>
        /// <returns>sorted unique</returns>
        public static string[] UniqueExclusiveSort(ICollection<string> values, string[] removeValues)
        {
            var unique = new HashSet<String>();
            foreach (var value in values)
                unique.Add(value);
            foreach (var value in removeValues)
                unique.Remove(value);

            String[] uniqueArr = unique.ToArray();
            Array.Sort(uniqueArr);
            return uniqueArr;
        }

        public static PropertyAccessException GetMismatchException(MethodInfo method, Object @object, InvalidCastException e)
        {
            return GetMismatchException(method.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetMismatchException(FieldInfo field, Object @object, InvalidCastException e)
        {
            return GetMismatchException(field.DeclaringType, @object, e);
        }

        public static PropertyAccessException GetGenericException(MethodInfo method, Exception e)
        {
            Type declaring = method.DeclaringType;
            String eMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            String message = "Failed to invoke method " + method.Name + " on class " +
                             declaring.GetCleanName() + ": " +
                             eMessage;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetInvocationTargetException(MethodInfo method, TargetInvocationException e)
        {
            Type declaring = method.DeclaringType;
            String message = "Failed to invoke method " + method.Name + " on class " + declaring.GetCleanName() + ": " + e.InnerException?.Message;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetIllegalAccessException(FieldInfo field, MemberAccessException e)
        {
            return GetAccessExceptionField(field, e);
        }

        public static PropertyAccessException GetIllegalArgumentException(FieldInfo field, ArgumentException e)
        {
            return GetAccessExceptionField(field, e);
        }

        public static PropertyAccessException GetAccessExceptionField(FieldInfo field, Exception e)
        {
            Type declaring = field.DeclaringType;
            String message = "Failed to obtain field value for field " + field.Name + " on class " + declaring.GetCleanName() + ": " + e.Message;
            throw new PropertyAccessException(message, e);
        }

        private static PropertyAccessException GetMismatchException(Type declared, Object @object, InvalidCastException e)
        {
            String classNameExpected = declared.GetCleanName();
            String classNameReceived;
            if (@object != null)
            {
                classNameReceived = @object.GetType().GetCleanName();
            }
            else
            {
                classNameReceived = "null";
            }

            if (classNameExpected.Equals(classNameReceived))
            {
                classNameExpected = declared.GetCleanName();
                classNameReceived = @object != null ? @object.GetType().GetCleanName() : "null";
            }

            var message = "Mismatched getter instance to event bean type, expected " + classNameExpected + " but received " + classNameReceived;
            throw new PropertyAccessException(message, e);
        }

        public static PropertyAccessException GetIllegalAccessException(MethodInfo method, MemberAccessException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        public static PropertyAccessException GetIllegalArgumentException(MethodInfo method, ArgumentException e)
        {
            return GetAccessExceptionMethod(method, e);
        }

        public static PropertyAccessException GetAccessExceptionMethod(MethodInfo method, Exception e)
        {
            Type declaring = method.DeclaringType;
            String message = "Failed to invoke method " + method.Name + " on class " + declaring.GetCleanName() + ": " + e.Message;
            throw new PropertyAccessException(message, e);
        }
    }
}
