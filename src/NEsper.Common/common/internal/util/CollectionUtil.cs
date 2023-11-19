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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Utility for handling collection or array tasks.
    /// </summary>
    public class CollectionUtil
    {
        public const string METHOD_SHRINKARRAYEVENTS = "ShrinkArrayEvents";
        public const string METHOD_SHRINKARRAYEVENTARRAY = "ShrinkArrayEventArray";
        public const string METHOD_SHRINKARRAYOBJECTS = "ShrinkArrayObjects";
        public const string METHOD_TOARRAYEVENTS = "ToArrayEvents";
        public const string METHOD_TOARRAYOBJECTS = "ToArrayObjects";
        public const string METHOD_TOARRAYEVENTSARRAY = "ToArrayEventsArray";
        public const string METHOD_TOARRAYNULLFOREMPTYEVENTS = "ToArrayNullForEmptyEvents";
        public const string METHOD_TOARRAYNULLFOREMPTYOBJECTS = "ToArrayNullForEmptyObjects";
        public const string METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS = "ToArrayNullForEmptyValueEvents";
        public const string METHOD_TOARRAYNULLFOREMPTYVALUEVALUES = "ToArrayNullForEmptyValueValues";
        public const string METHOD_TOARRAYMAYNULL = "ToArrayMayNull";
        public const string METHOD_ENUMERATORTOARRAYEVENTS = "EnumeratorToArrayEvents";

        private const int MAX_POWER_OF_TWO = 1 << 30; //(Int32.SIZE - 2);

        public static readonly IEnumerator<EventBean> NULL_EVENT_ITERATOR = NullEnumerator<EventBean>.Singleton;
        public static readonly IEnumerable<EventBean> NULL_EVENT_ITERABLE = NullEnumerable<EventBean>.Singleton;

        public static readonly IOrderedDictionary<object, object> EMPTY_SORTED_MAP =
            new OrderedListDictionary<object, object>();

        public static readonly EventBean[] EVENTBEANARRAY_EMPTY = Array.Empty<EventBean>();
        public static readonly EventBean[][] EVENTBEANARRAYARRAY_EMPTY = Array.Empty<EventBean[]>();
        public static readonly ISet<EventBean> SINGLE_NULL_ROW_EVENT_SET = new HashSet<EventBean>();
        public static readonly string[] STRINGARRAY_EMPTY = Array.Empty<string>();

        public static readonly object[] OBJECTARRAY_EMPTY = Array.Empty<object>();
        public static readonly object[][] OBJECTARRAYARRAY_EMPTY = Array.Empty<object[]>();

        public static readonly CodegenExpression EMPTY_LIST_EXPRESSION = EnumValue(typeof(FlexCollection), "Empty");

        public static readonly StopCallback STOP_CALLBACK_NONE;

        static CollectionUtil()
        {
            SINGLE_NULL_ROW_EVENT_SET.Add(null);
            STOP_CALLBACK_NONE = new ProxyStopCallback(() => { });
        }


        public static string ToString<T>(
            ICollection<T> stack,
            string delimiterChars)
        {
            if (stack.IsEmpty()) {
                return "";
            }

            if (stack.Count == 1) {
                return Convert.ToString(stack.First());
            }

            var writer = new StringWriter();
            var delimiter = "";
            foreach (var item in stack) {
                writer.Write(delimiter);
                writer.Write(Convert.ToString(item));
                delimiter = delimiterChars;
            }

            return writer.ToString();
        }

        public static object ArrayExpandAddElements(
            Array array,
            object[] elementsToAdd)
        {
            var cl = array.GetType();
            if (!cl.IsArray) {
                return null;
            }

            var length = array.Length;
            var newLength = length + elementsToAdd.Length;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);
            Array.Copy(array, 0, newArray, 0, length);
            for (var i = 0; i < elementsToAdd.Length; i++) {
                newArray.SetValue(elementsToAdd[i], length + i);
            }

            return newArray;
        }

        public static object ArrayShrinkRemoveSingle(
            Array array,
            int index)
        {
            var cl = array.GetType();
            if (!cl.IsArray) {
                return null;
            }

            var length = array.Length;
            var newLength = length - 1;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);
            if (index > 0) {
                Array.Copy(array, 0, newArray, 0, index);
            }

            if (index < newLength) {
                Array.Copy(array, index + 1, newArray, index, newLength - index);
            }

            return newArray;
        }

        public static T[] ArrayExpandAddElements<T>(
            Array array,
            ICollection<T> elementsToAdd)
        {
            var cl = array.GetType();
            if (!cl.IsArray) {
                return null;
            }

            var length = array.Length;
            var newLength = length + elementsToAdd.Count;
            var newArray = new T[newLength];
            Array.Copy(array, 0, newArray, 0, length);
            var count = 0;
            foreach (object element in elementsToAdd) {
                newArray.SetValue(element, length + count);
                count++;
            }

            return newArray;
        }

        public static object ArrayExpandAddSingle(
            Array array,
            object elementsToAdd)
        {
            var cl = array.GetType();
            if (!cl.IsArray) {
                return null;
            }

            var length = array.Length;
            var newLength = length + 1;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);
            Array.Copy(array, 0, newArray, 0, length);
            newArray.SetValue(elementsToAdd, length);
            return newArray;
        }

        public static int[] AddValue(
            int[] ints,
            int i)
        {
            var copy = new int[ints.Length + 1];
            Array.Copy(ints, 0, copy, 0, ints.Length);
            copy[ints.Length] = i;
            return copy;
        }

        public static object[] AddValue(
            object[] values,
            object value)
        {
            var copy = new object[values.Length + 1];
            Array.Copy(values, 0, copy, 0, values.Length);
            copy[values.Length] = value;
            return copy;
        }

        public static int FindItem(
            string[] items,
            string item)
        {
            for (var i = 0; i < items.Length; i++) {
                if (items[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Returns an array of integer values from the set of integer values
        /// </summary>
        /// <param name="set">to return array for</param>
        /// <returns>array</returns>
        public static int[] IntArray(ICollection<int> set)
        {
            if (set == null) {
                return Array.Empty<int>();
            }

            return set.ToArray();
        }

        public static string[] CopySortArray(string[] values)
        {
            if (values == null) {
                return null;
            }

            var copy = new string[values.Length];
            Array.Copy(values, 0, copy, 0, values.Length);
            Array.Sort(copy);
            return copy;
        }

        public static bool SortCompare(
            string[] valuesOne,
            string[] valuesTwo)
        {
            if (valuesOne == null) {
                return valuesTwo == null;
            }

            if (valuesTwo == null) {
                return false;
            }

            var copyOne = CopySortArray(valuesOne);
            var copyTwo = CopySortArray(valuesTwo);
            return copyOne.AreEqual(copyTwo);
        }

        /// <summary>
        ///     Returns a list of the elements invoking toString on non-null elements.
        /// </summary>
        /// <param name="collection">to render</param>
        /// <returns>comma-separate list of values (no escape)</returns>
        public static string ToString<T>(ICollection<T> collection)
        {
            if (collection == null) {
                return "null";
            }

            if (collection.IsEmpty()) {
                return "";
            }

            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var t in collection) {
                if (t == null) {
                    continue;
                }

                buf.Append(delimiter);
                buf.Append(t);
                delimiter = ", ";
            }

            return buf.ToString();
        }

        public static bool Compare(
            string[] otherIndexProps,
            string[] thisIndexProps)
        {
            if (otherIndexProps != null && thisIndexProps != null) {
                return otherIndexProps.AreEqual(thisIndexProps);
            }

            return otherIndexProps == null && thisIndexProps == null;
        }

        public static bool IsAllNullArray(Array array)
        {
            if (array == null) {
                throw new ArgumentNullException();
            }

            if (!array.GetType().IsArray) {
                throw new ArgumentException("Expected array but received " + array.GetType());
            }

            for (var i = 0; i < array.Length; i++) {
                if (array.GetValue(i) != null) {
                    return false;
                }
            }

            return true;
        }

        public static string ToStringArray(object[] received)
        {
            var buf = new StringBuilder();
            var delimiter = "";
            buf.Append("[");
            foreach (var t in received) {
                buf.Append(delimiter);
                if (t == null) {
                    buf.Append("null");
                }
                else if (t is object[] objects) {
                    buf.Append(ToStringArray(objects));
                }
                else {
                    buf.Append(t);
                }

                delimiter = ", ";
            }

            buf.Append("]");
            return buf.ToString();
        }

        public static IDictionary<string, object> PopulateNameValueMap(params object[] values)
        {
            IDictionary<string, object> result = new LinkedHashMap<string, object>();
            var count = values.Length / 2;
            if (values.Length != count * 2) {
                throw new ArgumentException("Expected an event number of name-value pairs");
            }

            for (var i = 0; i < count; i++) {
                var index = i * 2;
                var keyValue = values[index];
                if (!(keyValue is string key)) {
                    throw new ArgumentException(
                        "Expected string-type key value at index " + index + " but found " + keyValue);
                }

                var value = values[index + 1];
                if (result.ContainsKey(key)) {
                    throw new ArgumentException("Found two or more values for key '" + key + "'");
                }

                result.Put(key, value);
            }

            return result;
        }

        public static object AddArrays(
            object first,
            object second)
        {
            if (first != null && !first.GetType().IsArray) {
                throw new ArgumentException("Parameter is not an array: " + first);
            }

            if (second != null && !second.GetType().IsArray) {
                throw new ArgumentException("Parameter is not an array: " + second);
            }

            if (first == null) {
                return second;
            }

            if (second == null) {
                return first;
            }

            var firstArray = (Array)first;
            var secondArray = (Array)second;
            var firstLength = firstArray.Length;
            var secondLength = secondArray.Length;
            var total = firstLength + secondLength;
            var dest = Array.CreateInstance(first.GetType().GetElementType(), total);
            Array.Copy(firstArray, 0, dest, 0, firstLength);
            Array.Copy(secondArray, 0, dest, firstLength, secondLength);
            return dest;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="events">events</param>
        /// <returns>array or null</returns>
        public static EventBean[] ToArrayNullForEmptyEvents(ICollection<EventBean> events)
        {
            return events.IsEmpty() ? null : events.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">values</param>
        /// <returns>array or null</returns>
        public static object[] ToArrayNullForEmptyObjects(ICollection<object> values)
        {
            return values.IsEmpty() ? null : values.ToArray();
        }

        public static EventBean[] AddArrayWithSetSemantics(
            EventBean[] arrayOne,
            EventBean[] arrayTwo)
        {
            if (arrayOne.Length == 0) {
                return arrayTwo;
            }

            if (arrayTwo.Length == 0) {
                return arrayOne;
            }

            if (arrayOne.Length == 1 && arrayTwo.Length == 1) {
                if (arrayOne[0].Equals(arrayTwo[0])) {
                    return arrayOne;
                }

                return new[] { arrayOne[0], arrayOne[0] };
            }

            if (arrayOne.Length == 1 && arrayTwo.Length > 1) {
                if (SearchArray(arrayTwo, arrayOne[0]) != -1) {
                    return arrayTwo;
                }
            }

            if (arrayOne.Length > 1 && arrayTwo.Length == 1) {
                if (SearchArray(arrayOne, arrayTwo[0]) != -1) {
                    return arrayOne;
                }
            }

            ISet<EventBean> set = new HashSet<EventBean>();
            foreach (var @event in arrayOne) {
                set.Add(@event);
            }

            foreach (var @event in arrayTwo) {
                set.Add(@event);
            }

            return set.ToArray();
        }

        public static string[] ToArray(ICollection<string> strings)
        {
            if (strings.IsEmpty()) {
                return STRINGARRAY_EMPTY;
            }

            return strings.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="events">values</param>
        /// <returns>array</returns>
        public static EventBean[] ToArrayEvents(ICollection<EventBean> events)
        {
            if (events.IsEmpty()) {
                return EVENTBEANARRAY_EMPTY;
            }

            return events.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">values</param>
        /// <returns>array</returns>
        public static object[] ToArrayObjects(IList<object> values)
        {
            if (values.IsEmpty()) {
                return OBJECTARRAY_EMPTY;
            }

            return values.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="arrays">values</param>
        /// <returns>array</returns>
        public static EventBean[][] ToArrayEventsArray(ArrayDeque<EventBean[]> arrays)
        {
            if (arrays.IsEmpty()) {
                return EVENTBEANARRAYARRAY_EMPTY;
            }

            return arrays.ToArray();
        }

        public static int SearchArray<T>(
            T[] array,
            T item)
        {
            for (var i = 0; i < array.Length; i++) {
                if (array[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        public static bool RemoveEventByKeyLazyListMap(
            object key,
            EventBean bean,
            IDictionary<object, object> eventMap)
        {
            var listOfBeans = eventMap.Get(key);
            if (listOfBeans == null) {
                return false;
            }

            if (listOfBeans is IList<EventBean> events) {
                var result = events.Remove(bean);
                if (events.IsEmpty()) {
                    eventMap.Remove(key);
                }

                return result;
            }

            if (listOfBeans != null && listOfBeans.Equals(bean)) {
                eventMap.Remove(key);
                return true;
            }

            return false;
        }


        public static bool RemoveEventUnkeyedLazyListMap(
            EventBean bean,
            IDictionary<object, object> eventMap)
        {
            // TODO: ConcurrentModificationException
            foreach (var entry in eventMap) {
                if (entry.Value is IList<EventBean> existingListLocal) {
                    var result = existingListLocal.Remove(bean);
                    if (result) {
                        if (existingListLocal.IsEmpty()) {
                            eventMap.Remove(entry.Key);
                        }

                        return true;
                    }
                }
                else if (entry.Value != null && entry.Value.Equals(bean)) {
                    eventMap.Remove(entry.Key);
                    return true;
                }
            }

            return false;
        }

        public static void AddEventByKeyLazyListMapBack(
            object sortKey,
            EventBean eventBean,
            IDictionary<object, object> eventMap)
        {
            var existing = eventMap.Get(sortKey);
            if (existing == null) {
                eventMap.Put(sortKey, eventBean);
            }
            else {
                if (existing is IList<EventBean> existingListLocal) {
                    existingListLocal.Add(eventBean);
                }
                else {
                    IList<EventBean> existingList = new List<EventBean>();
                    existingList.Add((EventBean)existing);
                    existingList.Add(eventBean);
                    eventMap.Put(sortKey, existingList);
                }
            }
        }

        public static void AddEventByKeyLazyListMapFront(
            object key,
            EventBean bean,
            IDictionary<object, object> eventMap)
        {
            var current = eventMap.Get(key);
            if (current != null) {
                if (current is IList<EventBean> eventsLocal) {
                    eventsLocal.Insert(0, bean); // add to front, newest are listed first
                }
                else {
                    var theEvent = (EventBean)current;
                    IList<EventBean> events = new List<EventBean>();
                    events.Add(bean);
                    events.Add(theEvent);
                    eventMap.Put(key, events);
                }
            }
            else {
                eventMap.Put(key, bean);
            }
        }

        public static bool IsAnySet(bool[] array)
        {
            for (var i = 0; i < array.Length; i++) {
                if (array[i]) {
                    return true;
                }
            }

            return false;
        }

        public static IDictionary<K, V> TwoEntryMap<K, V>(
            K k1,
            V v1,
            K k2,
            V v2)
        {
            IDictionary<K, V> map = new LinkedHashMap<K, V>();
            map.Put(k1, v1);
            map.Put(k2, v2);
            return map;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">the array to be checked</param>
        /// <param name="index">the index</param>
        /// <returns>null or array value</returns>
        public static object ArrayValueAtIndex(
            Array array,
            int index)
        {
            if (array == null) {
                return null;
            }

            if (array.Length <= index) {
                return null;
            }

            return array.GetValue(index);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">the array to be checked</param>
        /// <param name="index">the index</param>
        /// <returns>true if the index is a valid index in the array</returns>
        public static bool ArrayExistsAtIndex(
            Array array,
            int index)
        {
            if (array == null) {
                return false;
            }

            return array.Length > index;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">the map / dictionary to be checked</param>
        /// <param name="key">the key to be tested</param>
        /// <returns>the value associated with the key or null</returns>
        public static T MapValueForKey<T>(
            IDictionary<string, T> map,
            string key)
        {
            if (map != null && map.TryGetValue(key, out var value)) {
                return value;
            }

            return default;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="map">the map / dictionary to be checked</param>
        /// <param name="key">the key to be tested</param>
        /// <returns>true if the key s contained in the map.</returns>
        public static bool MapExistsForKey<T>(
            IDictionary<string, T> map,
            string key)
        {
            return map != null && map.ContainsKey(key);
        }

        public static ICollection<T> ArrayToCollectionAllowNull<T>(object array)
        {
            if (array == null) {
                return null;
            }

            var asArray = array as Array;
            var len = asArray.Length;
            if (len == 0) {
                return Collections.GetEmptyList<T>();
            }

            if (len == 1) {
                return Collections.SingletonList((T)asArray.GetValue(0));
            }

            Deque<T> dq = new ArrayDeque<T>(len);
            for (var i = 0; i < len; i++) {
                dq.Add((T)asArray.GetValue(i));
            }

            return dq;
        }

        public static CodegenExpression ArrayToCollectionAllowNullCodegen(
            CodegenMethodScope codegenMethodScope,
            Type arrayType,
            CodegenExpression array,
            CodegenClassScope codegenClassScope)
        {
            if (!arrayType.IsArray) {
                throw new ArgumentException("Expected array type and received " + arrayType);
            }

            var arrayElementType = arrayType.GetComponentType();

            return StaticMethod(
                        typeof(CompatExtensions),
                        "Unwrap",
                        new[] { arrayElementType },
                        array,
                        ConstantTrue());
        }


        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="iterable">iterable</param>
        /// <returns>collection</returns>
        public static ICollection<T> IterableToCollection<T>(IEnumerable<T> iterable)
        {
            return iterable.ToList();
        }

        public static int CapacityHashMap(int expectedSize)
        {
            if (expectedSize < 3) {
                return expectedSize + 1;
            }

            if (expectedSize < MAX_POWER_OF_TWO) {
                // This is the calculation used in JDK8 to resize when a putAll
                // happens; it seems to be the most conservative calculation we
                // can make.  0.75 is the default load factor.
                return (int)(expectedSize / 0.75F + 1.0F);
            }

            return int.MaxValue; // any large value
        }

        public static EventBean[] ToArrayMayNull(EventBean @event)
        {
            return @event != null ? new[] { @event } : null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="collection">collection</param>
        /// <returns>array or null</returns>
        public static EventBean[] ToArrayMayNull(ICollection<EventBean> collection)
        {
            return collection?.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="count">cnt</param>
        /// <param name="events">events</param>
        /// <returns>shrank array</returns>
        public static EventBean[] ShrinkArrayEvents(
            int count,
            EventBean[] events)
        {
            var outEvents = new EventBean[count];
            Array.Copy(events, 0, outEvents, 0, count);
            return outEvents;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="count">cnt</param>
        /// <param name="keys">values</param>
        /// <returns>shrank array</returns>
        public static object[] ShrinkArrayObjects(
            int count,
            object[] keys)
        {
            var outKeys = new object[count];
            Array.Copy(keys, 0, outKeys, 0, count);
            return outKeys;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="count">cnt</param>
        /// <param name="eventArrays">events</param>
        /// <returns>shrank array</returns>
        public static EventBean[][] ShrinkArrayEventArray(
            int count,
            EventBean[][] eventArrays)
        {
            var outGens = new EventBean[count][];
            Array.Copy(eventArrays, 0, outGens, 0, count);
            return outGens;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="events">events</param>
        /// <returns>null or array</returns>
        public static EventBean[] ToArrayNullForEmptyValueEvents(IDictionary<object, EventBean> events)
        {
            return events.IsEmpty() ? null : events.Values.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="values">events</param>
        /// <returns>null or array</returns>
        public static object[] ToArrayNullForEmptyValueValues(IDictionary<object, object> values)
        {
            return values.IsEmpty() ? null : values.Values.ToArray();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumerator">the enumerator</param>
        /// <returns>array of events</returns>
        public static EventBean[] EnumeratorToArrayEvents(IEnumerator<EventBean> enumerator)
        {
            if (enumerator == null) {
                return null;
            }

            var events = new List<EventBean>();
            while (enumerator.MoveNext()) {
                events.Add(enumerator.Current);
            }

            return events.ToArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <returns>a dictionary build from the pairs</returns>
        public static IDictionary<string, object> BuildMap(params object[] pairs)
        {
            if (pairs.Length % 2 != 0) {
                throw new ArgumentException("Requires even number of args");
            }

            IDictionary<string, object> result = new Dictionary<string, object>();
            for (var i = 0; i < pairs.Length / 2; i++) {
                result.Put((string)pairs[i * 2], pairs[i * 2 + 1]);
            }

            return result;
        }

        public static IDictionary<string, object> BuildMap(object[][] entries)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            if (entries == null) {
                return result;
            }

            for (var i = 0; i < entries.Length; i++) {
                result.Put((string)entries[i][0], entries[i][1]);
            }

            return result;
        }

        /// <summary>
        ///     Compares two nullable values using Collator, for use with string-typed values.
        /// </summary>
        /// <param name="valueOne">first value to compare</param>
        /// <param name="valueTwo">second value to compare</param>
        /// <param name="isDescending">true for descending</param>
        /// <param name="collator">the Collator for comparing</param>
        /// <returns>compare result</returns>
        public static int CompareValuesCollated(
            object valueOne,
            object valueTwo,
            bool isDescending,
            IComparer<object> collator)
        {
            if (valueOne == null || valueTwo == null) {
                // A null value is considered equal to another null
                // value and smaller than any nonnull value
                if (valueOne == null && valueTwo == null) {
                    return 0;
                }

                if (valueOne == null) {
                    if (isDescending) {
                        return 1;
                    }

                    return -1;
                }

                if (isDescending) {
                    return -1;
                }

                return 1;
            }

            if (isDescending) {
                return collator.Compare(valueTwo, valueOne);
            }

            return collator.Compare(valueOne, valueTwo);
        }

        /// <summary>
        ///     Compares two nullable values.
        /// </summary>
        /// <param name="valueOne">first value to compare</param>
        /// <param name="valueTwo">second value to compare</param>
        /// <param name="isDescending">true for descending</param>
        /// <returns>compare result</returns>
        public static int CompareValues(
            object valueOne,
            object valueTwo,
            bool isDescending)
        {
            if (valueOne == null || valueTwo == null) {
                // A null value is considered equal to another null
                // value and smaller than any nonnull value
                if (valueOne == null && valueTwo == null) {
                    return 0;
                }

                if (valueOne == null) {
                    if (isDescending) {
                        return 1;
                    }

                    return -1;
                }

                if (isDescending) {
                    return -1;
                }

                return 1;
            }

            IComparable comparable1;
            if (valueOne is IComparable one) {
                comparable1 = one;
            }
            else {
                throw new InvalidCastException("Cannot sort objects of type " + valueOne.GetType());
            }

            if (isDescending) {
                return -1 * comparable1.CompareTo(valueTwo);
            }

            return comparable1.CompareTo(valueTwo);
        }

        public static string[] CopyArray(string[] arrayToCopy)
        {
            if (arrayToCopy.Length == 0) {
                return arrayToCopy;
            }

            var copy = new string[arrayToCopy.Length];
            Array.Copy(arrayToCopy, 0, copy, 0, copy.Length);
            return copy;
        }

        public static string[] AppendArrayConditional(
            string[] appendedTo,
            bool test,
            string appended)
        {
            if (!test) {
                return appendedTo;
            }

            return AppendArray(appendedTo, appended);
        }

        public static string[] AppendArrayConditional(
            string appendedTo,
            bool test,
            string appended)
        {
            if (!test) {
                return new[] { appendedTo };
            }

            return new[] { appendedTo, appended };
        }

        /// <summary>
        ///     Copy an sort the input array.
        /// </summary>
        /// <param name="input">to sort</param>
        /// <returns>sorted copied array</returns>
        public static string[] CopyAndSort(string[] input)
        {
            var result = new string[input.Length];
            Array.Copy(input, 0, result, 0, input.Length);
            Array.Sort(result);
            return result;
        }

        private static string[] AppendArray(
            string[] appendedTo,
            string appended)
        {
            var result = new string[appendedTo.Length + 1];
            Array.Copy(appendedTo, 0, result, 0, appendedTo.Length);
            result[appendedTo.Length] = appended;
            return result;
        }

        public static T[] AppendArray<T>(
            T[] a,
            T[] b)
        {
            var aLen = a.Length;
            var bLen = b.Length;
            var c = new T[aLen + bLen];

            Array.Copy(a, 0, c, 0, aLen);
            Array.Copy(b, 0, c, aLen, bLen);

            return c;
        }

        public static IList<IList<T>> Subdivide<T>(
            IList<T> items,
            int size)
        {
            if (size < 1) {
                throw new ArgumentException("Invalid size " + size);
            }

            if (items.Count <= size) {
                return Collections.SingletonList(items);
            }

            IList<IList<T>> lists = new List<IList<T>>();
            var start = 0;
            var remainder = items.Count;
            while (remainder > size) {
                lists.Add(items.SubList(start, start + size));
                start += size;
                remainder -= size;
            }

            lists.Add(items.SubList(start, start + remainder));
            return lists;
        }

        public static bool IsArrayAllNull(Array array)
        {
            if (array == null || array.Length == 0) {
                return true;
            }

            for (var i = 0; i < array.Length; i++) {
                if (array.GetValue(i) != null) {
                    return false;
                }
            }

            return true;
        }

        public static bool IsArraySameReferences(
            Array arrayOne,
            Array arrayTwo)
        {
            if (arrayOne == null || arrayTwo == null) {
                throw new ArgumentException("Null arrays");
            }

            if (arrayOne.Length != arrayTwo.Length) {
                return false;
            }

            for (var i = 0; i < arrayOne.Length; i++) {
                if (arrayOne.GetValue(i) != arrayTwo.GetValue(i)) {
                    return false;
                }
            }

            return true;
        }

        public static object GetMapValueChecked(
            object candidate,
            object key)
        {
            if (candidate == null) {
                return null;
            }
            else if (candidate is IDictionary<object, object> map) {
                return map.Get(key);
            }
            else if (candidate.GetType().IsGenericStringDictionary()) {
                var dictionary = candidate.AsObjectDictionary();
                return dictionary.Get(key);
            }

            return null;
        }

        public static bool GetMapKeyExistsChecked(
            object candidate,
            object key)
        {
            if (candidate == null) {
                return false;
            }
            else if (candidate is IDictionary<object, object> map) {
                return map.ContainsKey(key);
            }
            else if (candidate.GetType().IsGenericStringDictionary()) {
                var dictionary = candidate.AsObjectDictionary();
                return dictionary.ContainsKey(key);
            }

            return false;
        }
    }
} // end of namespace