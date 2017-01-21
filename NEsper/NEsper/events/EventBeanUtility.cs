///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Method to getSelectListEvents events in collections to other collections or other event types.
    /// </summary>
    public class EventBeanUtility
    {
        public static EventBean[] AllocatePerStreamShift(EventBean[] eventsPerStream)
        {
            var evalEvents = new EventBean[eventsPerStream.Length + 1];
            Array.Copy(eventsPerStream, 0, evalEvents, 1, eventsPerStream.Length);
            return evalEvents;
        }

        public static Object GetNonemptyFirstEventUnderlying(ICollection<EventBean> matchingEvents)
        {
            EventBean @event = GetNonemptyFirstEvent(matchingEvents);
            return @event.Underlying;
        }

        public static EventBean GetNonemptyFirstEvent(ICollection<EventBean> matchingEvents)
        {
            if (matchingEvents is IList<EventBean>)
            {
                return ((IList<EventBean>) matchingEvents)[0];
            }
            if (matchingEvents is LinkedList<EventBean>)
            {
                return ((LinkedList<EventBean>) matchingEvents).First.Value;
            }
            return matchingEvents.First();
        }

        /// <summary>
        /// Gets the assert property getter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static EventPropertyGetter GetAssertPropertyGetter(EventType type, String propertyName)
        {
            EventPropertyGetter getter = type.GetGetter(propertyName);
            if (getter == null)
            {
                throw new IllegalStateException("Property " + propertyName + " not found in type " + type.Name);
            }
            return getter;
        }

        public static EventPropertyGetter GetAssertPropertyGetter(EventType[] eventTypes, int keyStreamNum, String property)
        {
            return GetAssertPropertyGetter(eventTypes[keyStreamNum], property);
        }

        /// <summary>Resizes an array of events to a new size.
        /// <para/>
        /// Returns the same array reference if the size is the same. </summary>
        /// <param name="oldArray">array to resize</param>
        /// <param name="newSize">new array size</param>
        /// <returns>/// resized array</returns>
        public static T[] ResizeArray<T>(T[] oldArray, int newSize)
        {
            if (oldArray == null)
            {
                return null;
            }
            if (oldArray.Length == newSize)
            {
                return oldArray;
            }
            var newArray = new T[newSize];
            int preserveLength = Math.Min(oldArray.Length, newSize);
            if (preserveLength > 0)
            {
                Array.Copy(oldArray, 0, newArray, 0, preserveLength);
            }
            return newArray;
        }

        /// <summary>
        /// Flatten the vector of arrays to an array. Return null if an empty vector was passed, else return an array containing all the events.
        /// </summary>
        /// <param name="eventVector">vector</param>
        /// <returns>array with all events</returns>
        public static UniformPair<T[]> FlattenList<T>(LinkedList<UniformPair<T[]>> eventVector)
        {
            int count = eventVector.Count;
            if (count == 0)
            {
                return null;
            }

            if (count == 1)
            {
                return eventVector.First.Value;
            }

            int totalNew = 0;
            int totalOld = 0;
            foreach (var pair in eventVector)
            {
                if (pair != null)
                {
                    T[] first = pair.First;
                    if (first != null)
                    {
                        totalNew += first.Length;
                    }

                    T[] second = pair.Second;
                    if (second != null)
                    {
                        totalOld += second.Length;
                    }
                }
            }

            if ((totalNew + totalOld) == 0)
            {
                return null;
            }

            T[] resultNew = null;
            if (totalNew > 0)
            {
                resultNew = new T[totalNew];
            }

            T[] resultOld = null;
            if (totalOld > 0)
            {
                resultOld = new T[totalOld];
            }

            int destPosNew = 0;
            int destPosOld = 0;
            foreach (var pair in eventVector)
            {
                if (pair != null)
                {
                    T[] first = pair.First;
                    if (first != null)
                    {
                        Array.Copy(first, 0, resultNew, destPosNew, first.Length);
                        destPosNew += first.Length;
                    }

                    T[] second = pair.Second;
                    if (second != null)
                    {
                        Array.Copy(second, 0, resultOld, destPosOld, second.Length);
                        destPosOld += second.Length;
                    }
                }
            }

            return new UniformPair<T[]>(resultNew, resultOld);
        }

        /// <summary>
        /// Flatten the vector of arrays to an array. Return null if an empty vector was passed, else return
        /// an array containing all the events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventVector">vector</param>
        /// <returns>array with all events</returns>
        public static T[] Flatten<T>(ICollection<T[]> eventVector)
        {
            if (eventVector.IsEmpty())
            {
                return null;
            }

            if (eventVector.Count == 1)
            {
                return eventVector.First();
            }

            int totalElements = eventVector
                .Where(arr => arr != null)
                .Sum(arr => arr.Length);

            if (totalElements == 0)
            {
                return null;
            }

            var result = new T[totalElements];
            int destPos = 0;
            foreach (var arr in eventVector)
            {
                if (arr != null)
                {
                    Array.Copy(arr, 0, result, destPos, arr.Length);
                    destPos += arr.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// Flatten the vector of arrays to an array. Return null if an empty vector was passed, else return
        /// an array containing all the events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updateVector">is a list of updates of old and new events</param>
        /// <returns>array with all events</returns>
        public static UniformPair<T[]> FlattenBatchStream<T>(IList<UniformPair<T[]>> updateVector)
        {
            if (updateVector.IsEmpty())
            {
                return new UniformPair<T[]>(null, null);
            }

            if (updateVector.Count == 1)
            {
                return new UniformPair<T[]>(updateVector[0].First, updateVector[0].Second);
            }

            int totalNewEvents = 0;
            int totalOldEvents = 0;
            foreach (var pair in updateVector)
            {
                if (pair.First != null)
                {
                    totalNewEvents += pair.First.Length;
                }
                if (pair.Second != null)
                {
                    totalOldEvents += pair.Second.Length;
                }
            }

            if ((totalNewEvents == 0) && (totalOldEvents == 0))
            {
                return new UniformPair<T[]>(null, null);
            }

            T[] newEvents = null;
            T[] oldEvents = null;
            if (totalNewEvents != 0)
            {
                newEvents = new T[totalNewEvents];
            }
            if (totalOldEvents != 0)
            {
                oldEvents = new T[totalOldEvents];
            }

            int destPosNew = 0;
            int destPosOld = 0;
            foreach (var pair in updateVector)
            {
                T[] newData = pair.First;
                T[] oldData = pair.Second;

                if (newData != null)
                {
                    int newDataLen = newData.Length;
                    Array.Copy(newData, 0, newEvents, destPosNew, newDataLen);
                    destPosNew += newDataLen;
                }
                if (oldData != null)
                {
                    int oldDataLen = oldData.Length;
                    Array.Copy(oldData, 0, oldEvents, destPosOld, oldDataLen);
                    destPosOld += oldDataLen;
                }
            }

            return new UniformPair<T[]>(newEvents, oldEvents);
        }

        /// <summary>
        /// Returns object array containing property values of given properties, retrieved via EventPropertyGetter instances.
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="propertyGetters">getters to use for getting property values</param>
        /// <returns>object array with property values</returns>
        public static Object[] GetPropertyArray(EventBean theEvent, IList<EventPropertyGetter> propertyGetters)
        {
            unchecked
            {
                var propertyGettersLength = propertyGetters.Count;
                var keyValues = new Object[propertyGettersLength];
                for (int ii = 0; ii < propertyGettersLength; ii++)
                {
                    keyValues[ii] = propertyGetters[ii].Get(theEvent);
                }
                return keyValues;
            }
        }

        public static Object[] GetPropertyArray(EventBean[] eventsPerStream, IList<EventPropertyGetter> propertyGetters,  int[] streamNums)
        {
            unchecked
            {
                var propertyGettersLength = propertyGetters.Count;
                var keyValues = new Object[propertyGettersLength];
                for (int i = 0; i < propertyGettersLength; i++)
                {
                    keyValues[i] = propertyGetters[i].Get(eventsPerStream[streamNums[i]]);
                }
                return keyValues;
            }
        }

        /// <summary>
        /// Returns Multikey instance for given event and getters.
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="propertyGetters">getters for access to properties</param>
        /// <returns>MultiKey with property values</returns>
        public static MultiKeyUntyped GetMultiKey(EventBean theEvent,
                                                  IList<EventPropertyGetter> propertyGetters)
        {
            Object[] keyValues = GetPropertyArray(theEvent, propertyGetters);
            return new MultiKeyUntyped(keyValues);
        }

        public static MultiKeyUntyped GetMultiKey(EventBean theEvent,
                                                  IList<EventPropertyGetter> propertyGetters,
                                                  IList<Type> coercionTypes)
        {
            Object[] keyValues = GetPropertyArray(theEvent, propertyGetters);
            if (coercionTypes == null)
            {
                return new MultiKeyUntyped(keyValues);
            }
            for (int i = 0; i < coercionTypes.Count; i++)
            {
                Object key = keyValues[i];
                if ((key != null) && (!Equals(key.GetType(), coercionTypes[i])))
                {
                    if (key.IsNumber())
                    {
                        key = CoercerFactory.CoerceBoxed(key, coercionTypes[i]);
                        keyValues[i] = key;
                    }
                }
            }
            return new MultiKeyUntyped(keyValues);
        }

        public static MultiKeyUntyped GetMultiKey(EventBean[] eventsPerStream,
                                                  ExprEvaluator[] evaluators,
                                                  ExprEvaluatorContext context,
                                                  IList<Type> coercionTypes)
        {
            Object[] keyValues = GetPropertyArray(eventsPerStream, evaluators, context);
            if (coercionTypes == null)
            {
                return new MultiKeyUntyped(keyValues);
            }
            for (int i = 0; i < coercionTypes.Count; i++)
            {
                Object key = keyValues[i];
                if ((key != null) && (!Equals(key.GetType(), coercionTypes[i])))
                {
                    if (key.IsNumber())
                    {
                        key = CoercerFactory.CoerceBoxed(key, coercionTypes[i]);
                        keyValues[i] = key;
                    }
                }
            }
            return new MultiKeyUntyped(keyValues);
        }

        private static Object[] GetPropertyArray(EventBean[] eventsPerStream,
                                                 ExprEvaluator[] evaluators,
                                                 ExprEvaluatorContext context)
        {
            var keys = new Object[evaluators.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, true, context));
            }
            return keys;
        }

        public static MultiKeyUntyped GetMultiKey(EventBean[] eventPerStream,
                                                  EventPropertyGetter[] propertyGetters,
                                                  int[] keyStreamNums,
                                                  Type[] coercionTypes)
        {
            Object[] keyValues = GetPropertyArray(eventPerStream, propertyGetters, keyStreamNums);
            if (coercionTypes == null)
            {
                return new MultiKeyUntyped(keyValues);
            }

            for (int ii = 0; ii < coercionTypes.Length; ii++)
            {
                var key = keyValues[ii];
                if ((key != null) && (key.GetType() != coercionTypes[ii]))
                {
                    if (key.IsNumber())
                    {
                        key = CoercerFactory.CoerceBoxed(key, coercionTypes[ii]);
                        keyValues[ii] = key;
                    }
                }
            }

            return new MultiKeyUntyped(keyValues);
        }

        public static Object Coerce(Object target, Type coercionType)
        {
            if (coercionType == null)
            {
                return target;
            }

            if ((target != null) && (target.GetType().GetBoxedType() != coercionType))
            {
                if (target.IsNumber())
                {
                    return CoercerFactory.CoerceBoxed(target, coercionType);
                }
            }
            return target;
        }

        /// <summary>Format the event and return a string representation. </summary>
        /// <param name="theEvent">is the event to format.</param>
        /// <returns>string representation of event</returns>
        public static String PrintEvent(EventBean theEvent)
        {
            var writer = new StringWriter();
            PrintEvent(writer, theEvent);
            return writer.ToString();
        }

        private static void PrintEvent(TextWriter writer, EventBean theEvent)
        {
            IList<string> properties = theEvent.EventType.PropertyNames;
            for (int i = 0; i < properties.Count; i++)
            {
                string propName = properties[i];
                object property = theEvent.Get(propName);
                String printProperty;
                if (property == null)
                {
                    printProperty = "null";
                }
                else if (property.GetType().IsArray)
                {
                    printProperty = "Array :" + ((Object[])property).Render();
                }
                else
                {
                    printProperty = property.ToString();
                }
                writer.WriteLine("#" + i + "  " + propName + " = " + printProperty);
            }
        }

        public static void AppendEvent(TextWriter writer, EventBean theEvent)
        {
            IList<string> properties = theEvent.EventType.PropertyNames;
            string delimiter = "";
            for (int i = 0; i < properties.Count; i++)
            {
                String propName = properties[i];
                Object property = theEvent.Get(propName);
                String printProperty;
                if (property == null)
                {
                    printProperty = "null";
                }
                else if (property.GetType().IsArray)
                {
                    printProperty = "Array :" + ((Object[])property).Render();
                }
                else
                {
                    printProperty = property.ToString();
                }
                writer.Write(delimiter);
                writer.Write(propName);
                writer.Write("=");
                writer.Write(printProperty);
                delimiter = ",";
            }
        }

        /// <summary>Flattens a list of pairs of join result sets. </summary>
        /// <param name="joinPostings">is the list</param>
        /// <returns>is the consolidate sets</returns>
        public static UniformPair<ISet<MultiKey<EventBean>>> FlattenBatchJoin(
            IList<UniformPair<ISet<MultiKey<EventBean>>>> joinPostings)
        {
            if (joinPostings.IsEmpty())
            {
                return new UniformPair<ISet<MultiKey<EventBean>>>(null, null);
            }

            if (joinPostings.Count == 1)
            {
                return new UniformPair<ISet<MultiKey<EventBean>>>(joinPostings[0].First, joinPostings[0].Second);
            }

            ISet<MultiKey<EventBean>> newEvents = new LinkedHashSet<MultiKey<EventBean>>();
            ISet<MultiKey<EventBean>> oldEvents = new LinkedHashSet<MultiKey<EventBean>>();

            foreach (var pair in joinPostings)
            {
                ISet<MultiKey<EventBean>> newData = pair.First;
                ISet<MultiKey<EventBean>> oldData = pair.Second;

                if (newData != null)
                {
                    newEvents.AddAll(newData);
                }
                if (oldData != null)
                {
                    oldEvents.AddAll(oldData);
                }
            }

            return new UniformPair<ISet<MultiKey<EventBean>>>(newEvents, oldEvents);
        }

        /// <summary>Expand the array passed in by the single element to add. </summary>
        /// <param name="array">to expand</param>
        /// <param name="eventToAdd">element to add</param>
        /// <returns>resized array</returns>
        public static EventBean[] AddToArray(EventBean[] array,
                                             EventBean eventToAdd)
        {
            var newArray = new EventBean[array.Length + 1];
            Array.Copy(array, 0, newArray, 0, array.Length);
            newArray[newArray.Length - 1] = eventToAdd;
            return newArray;
        }

        /// <summary>Expand the array passed in by the multiple elements to add. </summary>
        /// <param name="array">to expand</param>
        /// <param name="eventsToAdd">elements to add</param>
        /// <returns>resized array</returns>
        public static EventBean[] AddToArray(EventBean[] array, ICollection<EventBean> eventsToAdd)
        {
            var newArray = new EventBean[array.Length + eventsToAdd.Count];
            Array.Copy(array, 0, newArray, 0, array.Length);

            int counter = array.Length;
            foreach (EventBean eventToAdd in eventsToAdd)
            {
                newArray[counter++] = eventToAdd;
            }
            return newArray;
        }

        /// <summary>Create a fragment event type. </summary>
        /// <param name="propertyType">property return type</param>
        /// <param name="genericType">property generic type parameter, or null if none</param>
        /// <param name="eventAdapterService">for event types</param>
        /// <returns>fragment type</returns>
        public static FragmentEventType CreateNativeFragmentType(Type propertyType,
                                                                 Type genericType,
                                                                 EventAdapterService eventAdapterService)
        {
            var isIndexed = false;

            if (propertyType.IsArray)
            {
                isIndexed = true;
                propertyType = propertyType.GetElementType();
            }
            else if (propertyType.IsGenericDictionary())
            {
            }
            else if (propertyType.IsImplementsInterface(typeof(IEnumerable)))
            {
                isIndexed = true;
                if (genericType == null)
                {
                    return null;
                }
                propertyType = genericType;
            }

            if (!propertyType.IsFragmentableType())
            {
                return null;
            }

            var type = eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                propertyType.FullName, propertyType, false, false, false);
            return new FragmentEventType(type, isIndexed, true);
        }

        /// <summary>Returns the distinct events by properties. </summary>
        /// <param name="events">to inspect</param>
        /// <param name="reader">for retrieving properties</param>
        /// <returns>distinct events</returns>
        public static EventBean[] GetDistinctByProp(LinkedList<EventBean> events,
                                                    EventBeanReader reader)
        {
            if (events == null || events.IsEmpty())
            {
                return new EventBean[0];
            }
            if (events.Count < 2)
            {
                return events.ToArray();
            }

            var set = new LinkedHashSet<MultiKeyUntypedEventPair>();
            if (events.First.Value is NaturalEventBean)
            {
                foreach (EventBean theEvent in events)
                {
                    var inner = ((NaturalEventBean)theEvent).OptionalSynthetic;
                    var keys = reader.Read(inner);
                    var pair = new MultiKeyUntypedEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }
            else
            {
                foreach (EventBean theEvent in events)
                {
                    Object[] keys = reader.Read(theEvent);
                    var pair = new MultiKeyUntypedEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }

            var result = new EventBean[set.Count];
            var count = 0;
            foreach (MultiKeyUntypedEventPair row in set)
            {
                result[count++] = row.EventBean;
            }
            return result;
        }

        /// <summary>Returns the distinct events by properties. </summary>
        /// <param name="events">to inspect</param>
        /// <param name="reader">for retrieving properties</param>
        /// <returns>distinct events</returns>
        public static EventBean[] GetDistinctByProp(EventBean[] events,
                                                    EventBeanReader reader)
        {
            if ((events == null) || (events.Length < 2))
            {
                return events;
            }

            var set = new LinkedHashSet<MultiKeyUntypedEventPair>();
            if (events[0] is NaturalEventBean)
            {
                foreach (EventBean theEvent in events)
                {
                    var inner = ((NaturalEventBean)theEvent).OptionalSynthetic;
                    var keys = reader.Read(inner);
                    var pair = new MultiKeyUntypedEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }
            else
            {
                foreach (EventBean theEvent in events)
                {
                    Object[] keys = reader.Read(theEvent);
                    var pair = new MultiKeyUntypedEventPair(keys, theEvent);
                    set.Add(pair);
                }
            }

            var result = new EventBean[set.Count];
            int count = 0;
            foreach (MultiKeyUntypedEventPair row in set)
            {
                result[count++] = row.EventBean;
            }
            return result;
        }

        public static EventBean[] Denaturalize(EventBean[] naturals)
        {
            if (naturals == null || naturals.Length == 0)
            {
                return null;
            }
            if (!(naturals[0] is NaturalEventBean))
            {
                return naturals;
            }
            if (naturals.Length == 1)
            {
                return new[] { ((NaturalEventBean)naturals[0]).OptionalSynthetic };
            }
            var result = new EventBean[naturals.Length];
            for (int i = 0; i < naturals.Length; i++)
            {
                result[i] = ((NaturalEventBean)naturals[i]).OptionalSynthetic;
            }
            return result;
        }

        public static bool EventsAreEqualsAllowNull(EventBean first, EventBean second)
        {
            if (first == null)
            {
                return second == null;
            }
            return second != null && Equals(first, second);
        }

        public static String Summarize(EventBean theEvent)
        {
            if (theEvent == null)
            {
                return "(null)";
            }
            StringWriter writer = new StringWriter();
            Summarize(theEvent, writer);
            return writer.ToString();
        }


        public static void Summarize(EventBean theEvent, TextWriter writer)
        {
            if (theEvent == null)
            {
                writer.Write("(null)");
                return;
            }
            writer.Write(theEvent.EventType.Name);
            writer.Write("[");
            SummarizeUnderlying(theEvent.Underlying, writer);
            writer.Write("]");
        }

        public static String SummarizeUnderlying(Object underlying)
        {
            if (underlying == null)
            {
                return "(null)";
            }
            StringWriter writer = new StringWriter();
            SummarizeUnderlying(underlying, writer);
            return writer.ToString();
        }

        public static void SummarizeUnderlying(Object underlying, TextWriter writer)
        {
            if (underlying.GetType().IsArray)
            {
                if (underlying is Object[])
                {
                    var asArray = (Object[]) underlying;
                    writer.Write(asArray.Render());
                }
                else
                {
                    var delimiter = "";
                    var asArray = underlying as Array;

                    writer.Write("[");
                    for (int i = 0; i < asArray.Length; i++)
                    {
                        writer.Write(delimiter);
                        delimiter = ",";

                        var value = asArray.GetValue(i);
                        if (value != null)
                        {
                            writer.Write(value.ToString());
                        }
                        else
                        {
                            writer.Write("(null)");
                        }
                    }
                    writer.Write("]");
                }
            }
            else
            {
                writer.Write(underlying.ToString());
            }
        }

        public static String Summarize(EventBean[] events)
        {
            if (events == null)
            {
                return "(null)";
            }
            if (events.Length == 0)
            {
                return "(empty)";
            }

            var writer = new StringWriter();
            var delimiter = "";
            for (int i = 0; i < events.Length; i++)
            {
                writer.Write(delimiter);
                writer.Write("event ");
                writer.Write(i);
                writer.Write(":");
                Summarize(events[i], writer);
                delimiter = ", ";
            }
            return writer.ToString();
        }


        public static void SafeArrayCopy(EventBean[] eventsPerStream,
                                         EventBean[] eventsLambda)
        {
            if (eventsPerStream.Length <= eventsLambda.Length)
            {
                Array.Copy(eventsPerStream, 0, eventsLambda, 0, eventsPerStream.Length);
            }
            else
            {
                Array.Copy(eventsPerStream, 0, eventsLambda, 0, eventsLambda.Length);
            }
        }

        public static T[] GetNewDataNonRemoved<T>(T[] newData, ICollection<T> removedEvents)
        {
            var filter = false;
            foreach (var newItem in newData)
            {
                if (removedEvents.Contains(newItem))
                {
                    filter = true;
                }
            }

            if (!filter)
            {
                return newData;
            }

            if (newData.Length == 1)
            {
                return null;
            }

            var events = new LinkedList<T>();
            foreach (var newItem in newData)
            {
                if (!removedEvents.Contains(newItem))
                {
                    events.AddLast(newItem);
                }
            }

            if (events.IsEmpty())
            {
                return null;
            }

            return events.ToArray();
        }

        public static T[] GetNewDataNonRemoved<T>(T[] newData, ICollection<T> removedEvents, T[][] newEventsPerView)
        {
            if (newData == null || newData.Length == 0)
            {
                return null;
            }
            if (newData.Length == 1)
            {
                if (removedEvents.Contains(newData[0]))
                {
                    return null;
                }
                var pass = FindEvent(newData[0], newEventsPerView);
                return pass ? newData : null;
            }

            var events = new LinkedList<T>();
            foreach (var newItem in newData)
            {
                if (!removedEvents.Contains(newItem))
                {
                    var pass = FindEvent(newItem, newEventsPerView);
                    if (pass)
                    {
                        events.AddLast(newItem);
                    }
                }
            }
            if (events.IsEmpty())
            {
                return null;
            }
            return events.ToArray();
        }

        /// <summary>
        /// Renders a map of elements, in which elements can be events or event arrays interspersed with other objects,
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>
        /// comma-separated list of map entry name-value pairs
        /// </returns>
        public static String ToString(IDictionary<String, Object> map)
        {
            if (map == null)
            {
                return "null";
            }
            if (map.IsEmpty())
            {
                return "";
            }
            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var entry in map)
            {
                buf.Append(delimiter);
                buf.Append(entry.Key);
                buf.Append("=");
                if (entry.Value is EventBean) {
                    buf.Append(Summarize((EventBean) entry.Value));
                }
                else if (entry.Value is EventBean[]) {
                    buf.Append(Summarize((EventBean[]) entry.Value));
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

        public static void AddToCollection(EventBean[] toAdd, ICollection<EventBean> events)
        {
            if (toAdd == null)
            {
                return;
            }
            events.AddAll(toAdd);
        }

        public static void AddToCollection(ISet<MultiKey<EventBean>> toAdd, ICollection<MultiKey<EventBean>> events)
        {
            if (toAdd == null)
            {
                return;
            }
            events.AddAll(toAdd);
        }

        public static EventBean[] ToArrayNullIfEmpty(ICollection<EventBean> events)
        {
            if (events == null || events.IsEmpty())
            {
                return null;
            }
            return events.ToArray();
        }

        public static ISet<MultiKey<EventBean>> ToLinkedHashSetNullIfEmpty(ICollection<MultiKey<EventBean>> events)
        {
            if (events == null || events.IsEmpty())
            {
                return null;
            }
            return new LinkedHashSet<MultiKey<EventBean>>(events);
        }

        public static ISet<MultiKey<EventBean>> ToSingletonSetIfNotNull(MultiKey<EventBean> row)
        {
            if (row == null)
            {
                return null;
            }
            return Collections.SingletonSet(row);
        }

        public static MultiKey<EventBean> GetLastInSet(ISet<MultiKey<EventBean>> events)
        {
            if (events.IsEmpty()) {
                return null;
            }
            int count = 0;
            foreach (MultiKey<EventBean> row in events) {
                count++;
                if (count == events.Count) {
                    return row;
                }
            }
            throw new IllegalStateException("Cannot get last on empty collection");
        }

        public static EventBean[] ToArrayIfNotNull(EventBean optionalEvent)
        {
            if (optionalEvent == null)
            {
                return null;
            }
            return new EventBean[] { optionalEvent };
        }

        public static bool CompareEventReferences(EventBean[] firstNonNull, EventBean[] secondNonNull)
        {
            if (firstNonNull.Length != secondNonNull.Length)
            {
                return false;
            }
            for (int i = 0; i < firstNonNull.Length; i++)
            {
                if (firstNonNull[i] != secondNonNull[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static T[] CopyArray<T>(T[] events)
        {
            T[] copy = new T[events.Length];
            Array.Copy(events, 0, copy, 0, copy.Length);
            return copy;
        }

        private static bool FindEvent<T>(T theEvent, T[][] eventsPerView)
        {
            for (int i = 0; i < eventsPerView.Length; i++)
            {
                if (eventsPerView[i] == null)
                {
                    continue;
                }
                for (int j = 0; j < eventsPerView[i].Length; j++)
                {
                    if (Equals(eventsPerView[i][j], theEvent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}