///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Method to getSelectListEvents events in collections to other collections or other event types.
    /// </summary>
    public class EventBeanUtility
    {
        public const string METHOD_FLATTENBATCHJOIN = "FlattenBatchJoin";
        public const string METHOD_FLATTENBATCHSTREAM = "FlattenBatchStream";

        /// <summary>
        ///     Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <returns>shifted</returns>
        public static EventBean[] AllocatePerStreamShift(EventBean[] eventsPerStream)
        {
            var evalEvents = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, evalEvents, 1, eventsPerStream.Length);
            return evalEvents;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="matchingEvents">matching</param>
        /// <returns>first</returns>
        public static object GetNonemptyFirstEventUnderlying(ICollection<EventBean> matchingEvents)
        {
            var @event = GetNonemptyFirstEvent(matchingEvents);
            return @event.Underlying;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="matchingEvents">events</param>
        /// <returns>event</returns>
        public static EventBean GetNonemptyFirstEvent(ICollection<EventBean> matchingEvents)
        {
            if (matchingEvents is IList<EventBean> matchingEventsList) {
                return matchingEventsList[0];
            }

            if (matchingEvents is Deque<EventBean> matchingEventsDeque) {
                return matchingEventsDeque.First;
            }

            return matchingEvents.First();
        }

        public static EventPropertyGetter GetAssertPropertyGetter(
            EventType type,
            string propertyName)
        {
            var getter = type.GetGetter(propertyName);
            if (getter == null) {
                throw new IllegalStateException("Property " + propertyName + " not found in type " + type.Name);
            }

            return getter;
        }

        public static EventPropertyGetter GetAssertPropertyGetter(
            EventType[] eventTypes,
            int keyStreamNum,
            string property)
        {
            return GetAssertPropertyGetter(eventTypes[keyStreamNum], property);
        }

        /// <summary>
        ///     Resizes an array of events to a new size.
        ///     <para />
        ///     Returns the same array reference if the size is the same.
        /// </summary>
        /// <param name="oldArray">array to resize</param>
        /// <param name="newSize">new array size</param>
        /// <returns>resized array</returns>
        public static EventBean[] ResizeArray(
            EventBean[] oldArray,
            int newSize)
        {
            if (oldArray == null) {
                return null;
            }

            if (oldArray.Length == newSize) {
                return oldArray;
            }

            var newArray = new EventBean[newSize];
            var preserveLength = Math.Min(oldArray.Length, newSize);
            if (preserveLength > 0) {
                Array.Copy(oldArray, 0, newArray, 0, preserveLength);
            }

            return newArray;
        }

        /// <summary>
        ///     Flatten the vector of arrays to an array. Return null if an empty vector was passed, else
        ///     return an array containing all the events.
        /// </summary>
        /// <param name="eventVector">vector</param>
        /// <returns>array with all events</returns>
        public static UniformPair<EventBean[]> FlattenList(ArrayDeque<UniformPair<EventBean[]>> eventVector)
        {
            if (eventVector.IsEmpty()) {
                return null;
            }

            if (eventVector.Count == 1) {
                return eventVector.First;
            }

            var totalNew = 0;
            var totalOld = 0;
            foreach (var pair in eventVector) {
                if (pair != null) {
                    if (pair.First != null) {
                        totalNew += pair.First.Length;
                    }

                    if (pair.Second != null) {
                        totalOld += pair.Second.Length;
                    }
                }
            }

            if (totalNew + totalOld == 0) {
                return null;
            }

            EventBean[] resultNew = null;
            if (totalNew > 0) {
                resultNew = new EventBean[totalNew];
            }

            EventBean[] resultOld = null;
            if (totalOld > 0) {
                resultOld = new EventBean[totalOld];
            }

            var destPosNew = 0;
            var destPosOld = 0;
            foreach (var pair in eventVector) {
                if (pair != null) {
                    if (pair.First != null) {
                        Array.Copy(pair.First, 0, resultNew, destPosNew, pair.First.Length);
                        destPosNew += pair.First.Length;
                    }

                    if (pair.Second != null) {
                        Array.Copy(pair.Second, 0, resultOld, destPosOld, pair.Second.Length);
                        destPosOld += pair.Second.Length;
                    }
                }
            }

            return new UniformPair<EventBean[]>(resultNew, resultOld);
        }

        /// <summary>
        ///     Flatten the vector of arrays to an array. Return null if an empty vector was passed, else
        ///     return an array containing all the events.
        /// </summary>
        /// <param name="eventVector">vector</param>
        /// <returns>array with all events</returns>
        public static EventBean[] Flatten(ArrayDeque<EventBean[]> eventVector)
        {
            if (eventVector.IsEmpty()) {
                return null;
            }

            if (eventVector.Count == 1) {
                return eventVector.First;
            }

            var totalElements = 0;
            foreach (var arr in eventVector) {
                if (arr != null) {
                    totalElements += arr.Length;
                }
            }

            if (totalElements == 0) {
                return null;
            }

            var result = new EventBean[totalElements];
            var destPos = 0;
            foreach (var arr in eventVector) {
                if (arr != null) {
                    Array.Copy(arr, 0, result, destPos, arr.Length);
                    destPos += arr.Length;
                }
            }

            return result;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        ///     Flatten the vector of arrays to an array. Return null if an empty vector was passed, else
        ///     return an array containing all the events.
        /// </summary>
        /// <param name="updateVector">is a list of updates of old and new events</param>
        /// <returns>array with all events</returns>
        public static UniformPair<EventBean[]> FlattenBatchStream(IList<UniformPair<EventBean[]>> updateVector)
        {
            if (updateVector.IsEmpty()) {
                return new UniformPair<EventBean[]>(null, null);
            }

            if (updateVector.Count == 1) {
                return new UniformPair<EventBean[]>(updateVector[0].First, updateVector[0].Second);
            }

            var totalNewEvents = 0;
            var totalOldEvents = 0;
            foreach (var pair in updateVector) {
                if (pair.First != null) {
                    totalNewEvents += pair.First.Length;
                }

                if (pair.Second != null) {
                    totalOldEvents += pair.Second.Length;
                }
            }

            if (totalNewEvents == 0 && totalOldEvents == 0) {
                return new UniformPair<EventBean[]>(null, null);
            }

            EventBean[] newEvents = null;
            EventBean[] oldEvents = null;
            if (totalNewEvents != 0) {
                newEvents = new EventBean[totalNewEvents];
            }

            if (totalOldEvents != 0) {
                oldEvents = new EventBean[totalOldEvents];
            }

            var destPosNew = 0;
            var destPosOld = 0;
            foreach (var pair in updateVector) {
                var newData = pair.First;
                var oldData = pair.Second;

                if (newData != null) {
                    var newDataLen = newData.Length;
                    Array.Copy(newData, 0, newEvents, destPosNew, newDataLen);
                    destPosNew += newDataLen;
                }

                if (oldData != null) {
                    var oldDataLen = oldData.Length;
                    Array.Copy(oldData, 0, oldEvents, destPosOld, oldDataLen);
                    destPosOld += oldDataLen;
                }
            }

            return new UniformPair<EventBean[]>(newEvents, oldEvents);
        }

        /// <summary>
        ///     Append arrays.
        /// </summary>
        /// <param name="source">array</param>
        /// <param name="append">array</param>
        /// <returns>appended array</returns>
        public static EventBean[] Append(
            EventBean[] source,
            EventBean[] append)
        {
            var result = new EventBean[source.Length + append.Length];
            Array.Copy(source, 0, result, 0, source.Length);
            Array.Copy(append, 0, result, source.Length, append.Length);
            return result;
        }

        /// <summary>
        ///     Convert list of events to array, returning null for empty or null lists.
        /// </summary>
        /// <param name="eventList">is a list of events to convert</param>
        /// <returns>array of events</returns>
        public static EventBean[] ToArray(ICollection<EventBean> eventList)
        {
            if (eventList == null || eventList.IsEmpty()) {
                return null;
            }

            return eventList.ToArray();
        }

        /// <summary>
        ///     Returns object array containing property values of given properties, retrieved via EventPropertyGetter
        ///     instances.
        /// </summary>
        /// <param name="theEvent">event to get property values from</param>
        /// <param name="propertyGetters">getters to use for getting property values</param>
        /// <returns>object array with property values</returns>
        public static object[] GetPropertyArray(
            EventBean theEvent,
            EventPropertyGetter[] propertyGetters)
        {
            var keyValues = new object[propertyGetters.Length];
            for (var i = 0; i < propertyGetters.Length; i++) {
                keyValues[i] = propertyGetters[i].Get(theEvent);
            }

            return keyValues;
        }

        public static object[] GetPropertyArray(
            EventBean[] eventsPerStream,
            EventPropertyGetter[] propertyGetters,
            int[] streamNums)
        {
            var keyValues = new object[propertyGetters.Length];
            for (var i = 0; i < propertyGetters.Length; i++) {
                keyValues[i] = propertyGetters[i].Get(eventsPerStream[streamNums[i]]);
            }

            return keyValues;
        }

        /// <summary>
        ///     Returns Multikey instance for given event and getters.
        /// </summary>
        /// <param name="theEvent">event to get property values from</param>
        /// <param name="propertyGetters">getters for access to properties</param>
        /// <returns>MultiKey with property values</returns>
        public static HashableMultiKey GetMultiKey(
            EventBean theEvent,
            EventPropertyGetter[] propertyGetters)
        {
            var keyValues = GetPropertyArray(theEvent, propertyGetters);
            return new HashableMultiKey(keyValues);
        }

        public static HashableMultiKey GetMultiKey(
            EventBean theEvent,
            EventPropertyGetter[] propertyGetters,
            Type[] coercionTypes)
        {
            var keyValues = GetPropertyArray(theEvent, propertyGetters);
            if (coercionTypes == null) {
                return new HashableMultiKey(keyValues);
            }

            for (var i = 0; i < coercionTypes.Length; i++) {
                var key = keyValues[i];
                if (key != null && key.GetType() != coercionTypes[i]) {
                    if (key.IsNumber()) {
                        key = TypeHelper.CoerceBoxed(key, coercionTypes[i]);
                        keyValues[i] = key;
                    }
                }
            }

            return new HashableMultiKey(keyValues);
        }

        public static HashableMultiKey GetMultiKey(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            ExprEvaluatorContext context,
            Type[] coercionTypes)
        {
            var keyValues = GetPropertyArray(eventsPerStream, evaluators, context);
            if (coercionTypes == null) {
                return new HashableMultiKey(keyValues);
            }

            for (var i = 0; i < coercionTypes.Length; i++) {
                var key = keyValues[i];
                if (key != null && key.GetType() != coercionTypes[i]) {
                    if (key.IsNumber()) {
                        key = TypeHelper.CoerceBoxed(key, coercionTypes[i]);
                        keyValues[i] = key;
                    }
                }
            }

            return new HashableMultiKey(keyValues);
        }

        private static object[] GetPropertyArray(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            ExprEvaluatorContext context)
        {
            var keys = new object[evaluators.Length];
            for (var i = 0; i < keys.Length; i++) {
                keys[i] = evaluators[i].Evaluate(eventsPerStream, true, context);
            }

            return keys;
        }

        public static object Coerce(
            object target,
            Type coercionType)
        {
            if (coercionType == null) {
                return target;
            }

            if (target != null && target.GetType() != coercionType) {
                if (target.IsNumber()) {
                    return TypeHelper.CoerceBoxed(target, coercionType);
                }
            }

            return target;
        }

        /// <summary>
        ///     Format the event and return a string representation.
        /// </summary>
        /// <param name="theEvent">is the event to format.</param>
        /// <returns>string representation of event</returns>
        public static string PrintEvent(EventBean theEvent)
        {
            var writer = new StringWriter();
            PrintEvent(writer, theEvent);
            return writer.ToString();
        }

        public static string PrintEvents(EventBean[] events)
        {
            var writer = new StringWriter();
            var count = 0;
            foreach (var theEvent in events) {
                count++;
                writer.WriteLine("Event " + string.Format("%6d:", count));
                PrintEvent(writer, theEvent);
            }

            return writer.ToString();
        }

        private static void PrintEvent(
            TextWriter writer,
            EventBean theEvent)
        {
            var properties = theEvent.EventType.PropertyNames;
            for (var i = 0; i < properties.Length; i++) {
                var propName = properties[i];
                var property = theEvent.Get(propName);
                string printProperty;
                if (property == null) {
                    printProperty = "null";
                }
                else if (property is object[]) {
                    printProperty = "Array :" + ((object[]) property).RenderAny();
                }
                else if (property is Array propertyArray) {
                    printProperty = "Array :" + PrintArray(propertyArray);
                }
                else {
                    printProperty = property.ToString();
                }

                writer.WriteLine("#" + i + "  " + propName + " = " + printProperty);
            }
        }

        private static string PrintArray(Array array)
        {
            var objects = new object[array.Length];
            for (var i = 0; i < array.Length; i++) {
                objects[i] = array.GetValue(i);
            }

            return objects.RenderAny();
        }

        public static void AppendEvent(
            TextWriter writer,
            EventBean theEvent)
        {
            var properties = theEvent.EventType.PropertyNames;
            var delimiter = "";
            for (var i = 0; i < properties.Length; i++) {
                var propName = properties[i];
                var property = theEvent.Get(propName);
                string printProperty;
                if (property == null) {
                    printProperty = "null";
                }
                else if (property.GetType().IsArray) {
                    printProperty = "Array :" + ((object[]) property).RenderAny();
                }
                else {
                    printProperty = property.ToString();
                }

                writer.Write(delimiter);
                writer.Write(propName);
                writer.Write("=");
                writer.Write(printProperty);
                delimiter = ",";
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        ///     Flattens a list of pairs of join result sets.
        /// </summary>
        /// <param name="joinPostings">is the list</param>
        /// <returns>is the consolidate sets</returns>
        public static UniformPair<ISet<MultiKey<EventBean>>> FlattenBatchJoin(
            IList<UniformPair<ISet<MultiKey<EventBean>>>> joinPostings)
        {
            if (joinPostings.IsEmpty()) {
                return new UniformPair<ISet<MultiKey<EventBean>>>(null, null);
            }

            if (joinPostings.Count == 1) {
                return new UniformPair<ISet<MultiKey<EventBean>>>(
                    joinPostings[0].First,
                    joinPostings[0].Second);
            }

            ISet<MultiKey<EventBean>> newEvents = new LinkedHashSet<MultiKey<EventBean>>();
            ISet<MultiKey<EventBean>> oldEvents = new LinkedHashSet<MultiKey<EventBean>>();

            foreach (var pair in joinPostings) {
                var newData = pair.First;
                var oldData = pair.Second;

                if (newData != null) {
                    newEvents.AddAll(newData);
                }

                if (oldData != null) {
                    oldEvents.AddAll(oldData);
                }
            }

            return new UniformPair<ISet<MultiKey<EventBean>>>(newEvents, oldEvents);
        }

        /// <summary>
        ///     Expand the array passed in by the single element to add.
        /// </summary>
        /// <param name="array">to expand</param>
        /// <param name="eventToAdd">element to add</param>
        /// <returns>resized array</returns>
        public static EventBean[] AddToArray(
            EventBean[] array,
            EventBean eventToAdd)
        {
            var newArray = new EventBean[array.Length + 1];
            Array.Copy(array, 0, newArray, 0, array.Length);
            newArray[newArray.Length - 1] = eventToAdd;
            return newArray;
        }

        /// <summary>
        ///     Expand the array passed in by the multiple elements to add.
        /// </summary>
        /// <param name="array">to expand</param>
        /// <param name="eventsToAdd">elements to add</param>
        /// <returns>resized array</returns>
        public static EventBean[] AddToArray(
            EventBean[] array,
            ICollection<EventBean> eventsToAdd)
        {
            var newArray = new EventBean[array.Length + eventsToAdd.Count];
            Array.Copy(array, 0, newArray, 0, array.Length);

            var counter = array.Length;
            foreach (var eventToAdd in eventsToAdd) {
                newArray[counter++] = eventToAdd;
            }

            return newArray;
        }

        /// <summary>
        ///     Create a fragment event type.
        /// </summary>
        /// <param name="propertyType">property return type</param>
        /// <param name="genericType">property generic type parameter, or null if none</param>
        /// <param name="beanEventTypeFactory">for event types</param>
        /// <returns>fragment type</returns>
        public static FragmentEventType CreateNativeFragmentType(
            Type propertyType,
            Type genericType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var isIndexed = false;

            if (propertyType.IsArray) {
                isIndexed = true;
                propertyType = propertyType.GetElementType();
            }
            else if (propertyType.IsGenericDictionary()) {
                // Ignore this - technically enumerable
            }
            else if (propertyType.IsGenericEnumerable()) {
                propertyType = GenericExtensions
                    .FindGenericEnumerationInterface(propertyType)
                    .GetGenericArguments()[0];
                isIndexed = true;

#if false
                if (genericType == null) {
                    return null;
                }

                propertyType = genericType;
#endif
            }

            if (!propertyType.IsFragmentableType()) {
                return null;
            }

            EventType type = beanEventTypeFactory.GetCreateBeanType(propertyType);
            return new FragmentEventType(type, isIndexed, true);
        }

        /// <summary>
        ///     Returns the distinct events by properties.
        /// </summary>
        /// <param name="events">to inspect</param>
        /// <param name="reader">for retrieving properties</param>
        /// <returns>distinct events</returns>
        public static ICollection<EventBean> GetDistinctByProp(
            ArrayDeque<EventBean> events,
            EventBeanReader reader)
        {
            if (events == null || events.IsEmpty()) {
                return new EventBean[0];
            }

            if (events.Count < 2) {
                return events;
            }

            ISet<HashableMultiKeyEventPair> set = new LinkedHashSet<HashableMultiKeyEventPair>();
            if (events.First is NaturalEventBean) {
                foreach (var theEvent in events) {
                    var inner = ((NaturalEventBean) theEvent).OptionalSynthetic;
                    var keys = reader.Read(inner);
                    var pair = new HashableMultiKeyEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }
            else {
                foreach (var theEvent in events) {
                    var keys = reader.Read(theEvent);
                    var pair = new HashableMultiKeyEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }

            var result = new EventBean[set.Count];
            var count = 0;
            foreach (var row in set) {
                result[count++] = row.EventBean;
            }

            return result;
        }

        /// <summary>
        ///     Returns the distinct events by properties.
        /// </summary>
        /// <param name="events">to inspect</param>
        /// <param name="reader">for retrieving properties</param>
        /// <returns>distinct events</returns>
        public static EventBean[] GetDistinctByProp(
            EventBean[] events,
            EventBeanReader reader)
        {
            if (events == null || events.Length < 2 || reader == null) {
                return events;
            }

            ISet<HashableMultiKeyEventPair> set = new LinkedHashSet<HashableMultiKeyEventPair>();
            if (events[0] is NaturalEventBean) {
                foreach (var theEvent in events) {
                    var inner = ((NaturalEventBean) theEvent).OptionalSynthetic;
                    var keys = reader.Read(inner);
                    var pair = new HashableMultiKeyEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }
            else {
                foreach (var theEvent in events) {
                    var keys = reader.Read(theEvent);
                    var pair = new HashableMultiKeyEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }

            var result = new EventBean[set.Count];
            var count = 0;
            foreach (var row in set) {
                result[count++] = row.EventBean;
            }

            return result;
        }

        public static EventBean[] Denaturalize(EventBean[] naturals)
        {
            if (naturals == null || naturals.Length == 0) {
                return null;
            }

            if (!(naturals[0] is NaturalEventBean)) {
                return naturals;
            }

            if (naturals.Length == 1) {
                return new[] {((NaturalEventBean) naturals[0]).OptionalSynthetic};
            }

            var result = new EventBean[naturals.Length];
            for (var i = 0; i < naturals.Length; i++) {
                result[i] = ((NaturalEventBean) naturals[i]).OptionalSynthetic;
            }

            return result;
        }

        public static bool EventsAreEqualsAllowNull(
            EventBean first,
            EventBean second)
        {
            if (first == null) {
                return second == null;
            }

            return second != null && first.Equals(second);
        }

        public static void SafeArrayCopy(
            EventBean[] eventsPerStream,
            EventBean[] eventsLambda)
        {
            if (eventsPerStream.Length <= eventsLambda.Length) {
                Array.Copy(eventsPerStream, 0, eventsLambda, 0, eventsPerStream.Length);
            }
            else {
                Array.Copy(eventsPerStream, 0, eventsLambda, 0, eventsLambda.Length);
            }
        }

        public static EventBean[] GetNewDataNonRemoved(
            EventBean[] newData,
            ISet<EventBean> removedEvents)
        {
            var filter = false;
            for (var i = 0; i < newData.Length; i++) {
                if (removedEvents.Contains(newData[i])) {
                    filter = true;
                }
            }

            if (!filter) {
                return newData;
            }

            if (newData.Length == 1) {
                return null;
            }

            var events = new ArrayDeque<EventBean>(newData.Length - 1);
            for (var i = 0; i < newData.Length; i++) {
                if (!removedEvents.Contains(newData[i])) {
                    events.Add(newData[i]);
                }
            }

            if (events.IsEmpty()) {
                return null;
            }

            return events.ToArray();
        }

        public static EventBean[] GetNewDataNonRemoved(
            EventBean[] newData,
            ISet<EventBean> removedEvents,
            EventBean[][] newEventsPerView)
        {
            if (newData == null || newData.Length == 0) {
                return null;
            }

            if (newData.Length == 1) {
                if (removedEvents.Contains(newData[0])) {
                    return null;
                }

                var pass = FindEvent(newData[0], newEventsPerView);
                return pass ? newData : null;
            }

            var events = new ArrayDeque<EventBean>(newData.Length - 1);
            for (var i = 0; i < newData.Length; i++) {
                if (!removedEvents.Contains(newData[i])) {
                    var pass = FindEvent(newData[i], newEventsPerView);
                    if (pass) {
                        events.Add(newData[i]);
                    }
                }
            }

            if (events.IsEmpty()) {
                return null;
            }

            return events.ToArray();
        }

        /// <summary>
        ///     Renders a map of elements, in which elements can be events or event arrays interspersed with other objects,
        /// </summary>
        /// <param name="map">to render</param>
        /// <returns>comma-separated list of map entry name-value pairs</returns>
        public static string ToString(IDictionary<string, object> map)
        {
            if (map == null) {
                return "null";
            }

            if (map.IsEmpty()) {
                return "";
            }

            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var entry in map) {
                buf.Append(delimiter);
                buf.Append(entry.Key);
                buf.Append("=");
                if (entry.Value is EventBean) {
                    buf.Append(EventBeanSummarizer.Summarize((EventBean) entry.Value));
                }
                else if (entry.Value is EventBean[]) {
                    buf.Append(EventBeanSummarizer.Summarize((EventBean[]) entry.Value));
                }
                else if (entry.Value == null) {
                    buf.Append("null");
                }
                else {
                    buf.Append(entry.Value);
                }

                delimiter = ", ";
            }

            return buf.ToString();
        }

        public static void AddToCollection(
            EventBean[] toAdd,
            ICollection<EventBean> events)
        {
            if (toAdd == null) {
                return;
            }

            events.AddAll(toAdd);
        }

        public static void AddToCollection(
            ISet<MultiKey<EventBean>> toAdd,
            ICollection<MultiKey<EventBean>> events)
        {
            if (toAdd == null) {
                return;
            }

            events.AddAll(toAdd);
        }

        public static EventBean[] ToArrayNullIfEmpty(ICollection<EventBean> events)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            return events.ToArray();
        }

        public static ISet<MultiKey<EventBean>> ToLinkedHashSetNullIfEmpty(ICollection<MultiKey<EventBean>> events)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            return new LinkedHashSet<MultiKey<EventBean>>(events);
        }

        public static ISet<MultiKey<EventBean>> ToSingletonSetIfNotNull(MultiKey<EventBean> row)
        {
            if (row == null) {
                return null;
            }

            return Collections.SingletonSet(row);
        }

        public static MultiKey<EventBean> GetLastInSet(ISet<MultiKey<EventBean>> events)
        {
            if (events.IsEmpty()) {
                return null;
            }

            var count = 0;
            foreach (var row in events) {
                count++;
                if (count == events.Count) {
                    return row;
                }
            }

            throw new IllegalStateException("Cannot get last on empty collection");
        }

        public static EventBean[] ToArrayIfNotNull(EventBean optionalEvent)
        {
            if (optionalEvent == null) {
                return null;
            }

            return new[] {optionalEvent};
        }

        public static bool CompareEventReferences(
            EventBean[] firstNonNull,
            EventBean[] secondNonNull)
        {
            if (firstNonNull.Length != secondNonNull.Length) {
                return false;
            }

            for (var i = 0; i < firstNonNull.Length; i++) {
                if (firstNonNull[i] != secondNonNull[i]) {
                    return false;
                }
            }

            return true;
        }

        public static EventBean[] CopyArray(EventBean[] events)
        {
            var copy = new EventBean[events.Length];
            Array.Copy(events, 0, copy, 0, copy.Length);
            return copy;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventsZeroSubselect">events</param>
        /// <param name="newData">new data flag</param>
        /// <param name="matchingEvents">collection of events</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <param name="filter">filter expression</param>
        /// <returns>first matching</returns>
        public static EventBean EvaluateFilterExpectSingleMatch(
            EventBean[] eventsZeroSubselect,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprEvaluator filter)
        {
            EventBean subSelectResult = null;
            foreach (var subselectEvent in matchingEvents) {
                // Prepare filter expression event list
                eventsZeroSubselect[0] = subselectEvent;

                var pass = filter.Evaluate(eventsZeroSubselect, newData, exprEvaluatorContext);
                if (pass != null && true.Equals(pass)) {
                    if (subSelectResult != null) {
                        return null;
                    }

                    subSelectResult = subselectEvent;
                }
            }

            return subSelectResult;
        }

        private static bool FindEvent(
            EventBean theEvent,
            EventBean[][] eventsPerView)
        {
            for (var i = 0; i < eventsPerView.Length; i++) {
                if (eventsPerView[i] == null) {
                    continue;
                }

                for (var j = 0; j < eventsPerView[i].Length; j++) {
                    if (eventsPerView[i][j] == theEvent) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
} // end of namespace