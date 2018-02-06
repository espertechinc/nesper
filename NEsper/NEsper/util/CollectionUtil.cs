///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility for handling collection or array tasks.
    /// </summary>
    public class CollectionUtil
    {
        public readonly static IEnumerator<EventBean> NULL_EVENT_ITERATOR = new NullEnumerator<EventBean>();
        public readonly static IEnumerable<EventBean> NULL_EVENT_ITERABLE = new ProxyEnumerable<EventBean> { ProcEnumerator = () => NULL_EVENT_ITERATOR };
        public readonly static ISet<MultiKey<EventBean>> EMPTY_ROW_SET = new HashSet<MultiKey<EventBean>>();
        public readonly static EventBean[] EVENTBEANARRAY_EMPTY = new EventBean[0];
        public readonly static ICollection<EventBean> SINGLE_NULL_ROW_EVENT_SET = new HashSet<EventBean>();
        public readonly static string[] EMPTY_STRING_ARRAY = new string[0];

        public static readonly StopCallback STOP_CALLBACK_NONE;
    
        static CollectionUtil()
        {
            SINGLE_NULL_ROW_EVENT_SET.Add(null);
            STOP_CALLBACK_NONE = new ProxyStopCallback(() => { });
        }

        public static IComparer<object> GetComparator(
            ExprEvaluator[] sortCriteriaEvaluators,
            bool isSortUsingCollator,
            bool[] isDescendingValues)
        {
            // determine string-type sorting
            var hasStringTypes = false;
            var stringTypes = new bool[sortCriteriaEvaluators.Length];

            int count = 0;
            foreach (ExprEvaluator node in sortCriteriaEvaluators)
            {
                if (node.ReturnType == typeof (string))
                {
                    hasStringTypes = true;
                    stringTypes[count] = true;
                }
                count++;
            }

            if (sortCriteriaEvaluators.Length > 1)
            {
                if ((!hasStringTypes) || (!isSortUsingCollator))
                {
                    var comparatorMK = new MultiKeyComparator(isDescendingValues);
                    return new MultiKeyCastingComparator(comparatorMK);
                }
                else
                {
                    var comparatorMk = new MultiKeyCollatingComparator(isDescendingValues, stringTypes);
                    return new MultiKeyCastingComparator(comparatorMk);
                }
            }
            else
            {
                if ((!hasStringTypes) || (!isSortUsingCollator))
                {
                    return new ObjectComparator(isDescendingValues[0]);
                }
                else
                {
                    return new ObjectCollatingComparator(isDescendingValues[0]);
                }
            }
        }

        public static string ToString(ICollection<int> stack, string delimiterChars)
        {
            if (stack.IsEmpty())
            {
                return "";
            }
            if (stack.Count == 1)
            {
                return Convert.ToString(stack.First());
            }
            var writer = new StringWriter();
            var delimiter = "";
            foreach (int? item in stack)
            {
                writer.Write(delimiter);
                writer.Write(Convert.ToString(item));
                delimiter = delimiterChars;
            }
            return writer.ToString();
        }

        public static object ArrayExpandAddElements<T>(Array array, T[] elementsToAdd)
        {
            var length = array.Length;
            var newLength = length + elementsToAdd.Length;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);

            Array.Copy(array, 0, newArray, 0, length);
            for (int i = 0; i < elementsToAdd.Length; i++)
            {
                newArray.SetValue(elementsToAdd[i], length + i);
            }
            return newArray;
        }

        public static object ArrayExpandAddElements<T>(object array, T[] elementsToAdd)
        {
            var cl = array.GetType();
            if (!cl.IsArray) return null;
            return ArrayExpandAddElements((Array) array, elementsToAdd);
        }
    
        public static object ArrayShrinkRemoveSingle(Array array, int index)
        {
            var length = array.Length;
            var newLength = length - 1;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);

            if (index > 0)
            {
                Array.Copy(array, 0, newArray, 0, index);
            }

            if (index < newLength)
            {
                Array.Copy(array, index + 1, newArray, index, newLength - index);
            }

            return newArray;
        }

        public static object ArrayShrinkRemoveSingle(object array, int index)
        {
            Type cl = array.GetType();
            if (!cl.IsArray) return null;
            return ArrayShrinkRemoveSingle((Array) array, index);
        }

        public static object ArrayExpandAddElements<T>(Array array, ICollection<T> elementsToAdd)
        {
            var length = array.Length;
            var newLength = length + elementsToAdd.Count;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);

            Array.Copy(array, 0, newArray, 0, length);
            int count = 0;
            foreach (var element in elementsToAdd) {
                newArray.SetValue(element, length + count);
                count++;
            }
            return newArray;
        }

        public static object ArrayExpandAddElements(object array, int index)
        {
            Type cl = array.GetType();
            if (!cl.IsArray) return null;
            return ArrayExpandAddElements((Array)array, index);
        }

        public static object ArrayExpandAddSingle(Array array, object elementsToAdd)
        {
            var length = array.Length;
            int newLength = length + 1;
            var componentType = array.GetType().GetElementType();
            var newArray = Array.CreateInstance(componentType, newLength);

            Array.Copy(array, 0, newArray, 0, length);
            newArray.SetValue(elementsToAdd, length);
            return newArray;
        }

        public static object ArrayExpandAddSingle(object array, object elementsToAdd)
        {
            Type cl = array.GetType();
            if (!cl.IsArray) return null;
            return ArrayExpandAddSingle((Array)array, elementsToAdd);
        }

        public static int[] AddValue(int[] ints, int i) {
            int[] copy = new int[ints.Length + 1];
            Array.Copy(ints, 0, copy, 0, ints.Length);
            copy[ints.Length] = i;
            return copy;
        }
    
        public static int FindItem(string[] items, string item) {
            for (int i = 0; i < items.Length; i++) {
                if (items[i].Equals(item)) {
                    return i;
                }
            }
            return -1;
        }
    
        /// <summary>Returns an array of integer values from the set of integer values </summary>
        /// <param name="set">to return array for</param>
        /// <returns>array</returns>
        public static int[] IntArray(ICollection<int> set)
        {
            if (set == null)
            {
                return new int[0];
            }

            return set.ToArray();
        }
    
        public static string[] CopySortArray(IEnumerable<string> values)
        {
            if (values == null)
            {
                return null;
            }

            return values.OrderBy(v => v).ToArray();
        }
    
        public static bool SortCompare(string[] valuesOne, string[] valuesTwo)
        {
            if (valuesOne == null) {
                return valuesTwo == null;
            }
            if (valuesTwo == null) {
                return false;
            }
            string[] copyOne = CopySortArray(valuesOne);
            string[] copyTwo = CopySortArray(valuesTwo);
            return Collections.AreEqual(copyOne, copyTwo);
        }
    
        /// <summary>Returns a list of the elements invoking toString on non-null elements. </summary>
        /// <param name="collection">to render</param>
        /// <returns>comma-separate list of values (no escape)</returns>
        public static string ToString<T>(ICollection<T> collection)
        {
            if (collection == null)
            {
                return "null";
            }
            if (collection.IsEmpty())
            {
                return "";
            }

            var buf = new StringBuilder();
            var delimiter = "";
            foreach (T t in collection)
            {
                if (t == null)
                {
                    continue;
                }
                buf.Append(delimiter);
                buf.Append(t);
                delimiter = ", ";
            }
            return buf.ToString();
        }

        public static bool Compare(string[] otherIndexProps, string[] thisIndexProps)
        {
            if (otherIndexProps != null && thisIndexProps != null)
            {
                return Collections.AreEqual(otherIndexProps, thisIndexProps);
            }
            return otherIndexProps == null && thisIndexProps == null;
        }

        public static bool IsAllNullArray(object array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            var asArray = array as Array;
            if (asArray == null)
            {
                throw new ArgumentException("Expected array but received " + array.GetType());
            }

            for (int i = 0; i < asArray.Length; i++)
            {
                if (asArray.GetValue(i) != null)
                {
                    return false;
                }
            }
            return true;
        }

        public static string ToStringArray(Array received)
        {
            var buf = new StringBuilder();
            var delimiter = "";
            buf.Append("[");
            foreach (object t in received)
            {
                buf.Append(delimiter);
                if (t == null)
                {
                    buf.Append("null");
                }
                else if (t is Array)
                {
                    buf.Append(ToStringArray((Array) t));
                }
                else
                {
                    buf.Append(t);
                }
                delimiter = ", ";
            }
            buf.Append("]");
            return buf.ToString();
        }

        public static IDictionary<string, object> PopulateNameValueMap(params object[] values)
        {
            var result = new LinkedHashMap<string, object>();
            var count = values.Length/2;
            if (values.Length != count*2)
            {
                throw new ArgumentException(
                    "Expected an event number of name-value pairs");
            }
            for (int i = 0; i < count; i++)
            {
                var index = i*2;
                var keyValue = values[index];
                if (!(keyValue is string))
                {
                    throw new ArgumentException(
                        "Expected string-type key value at index " + index + " but found " + keyValue);
                }
                var key = (string) keyValue;
                var value = values[index + 1];
                if (result.ContainsKey(key))
                {
                    throw new ArgumentException(
                        "Found two or more values for key '" + key + "'");
                }
                result[key] = value;
            }
            return result;
        }

        public static object AddArrays(object first, object second)
        {
            var firstAsArray = first as Array;
            var secondAsArray = second as Array;

            if ((first != null) && (firstAsArray == null))
                throw new ArgumentException("Parameter is not an array: " + first);
            if ((second != null) && (secondAsArray == null))
                throw new ArgumentException("Parameter is not an array: " + second);
            if (firstAsArray == null)
                return secondAsArray;
            if (secondAsArray == null)
                return firstAsArray;

            var firstType = firstAsArray.GetType().GetElementType();
            var firstLength = firstAsArray.Length;
            var secondLength = secondAsArray.Length;
            var total = firstLength + secondLength;
            var dest = Array.CreateInstance(firstType, total);

            Array.Copy(firstAsArray, 0, dest, 0, firstLength);
            Array.Copy(secondAsArray, 0, dest, firstLength, secondLength);

            return dest;
        }
    
        public static EventBean[] AddArrayWithSetSemantics(EventBean[] arrayOne, EventBean[] arrayTwo)
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
                else {
                    return new EventBean[] {arrayOne[0], arrayOne[0]};
                }
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
            ICollection<EventBean> set = new HashSet<EventBean>();
            foreach (EventBean @event in arrayOne) {
                set.Add(@event);
            }
            foreach (EventBean @event in arrayTwo) {
                set.Add(@event);
            }
            return set.ToArray();
        }

        public static string[] ToArray(ICollection<string> strings)
        {
            if (strings.IsEmpty())
            {
                return EMPTY_STRING_ARRAY;
            }
            return strings.ToArray();
        }

        public static int SearchArray<T>(T[] array, T item)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool RemoveEventByKeyLazyListMap(object key, EventBean bean, IDictionary<object, object> eventMap)
        {
            var listOfBeans = eventMap.Get(key);
            if (listOfBeans == null)
            {
                return false;
            }

            if (listOfBeans is IList<EventBean>)
            {
                var events = (IList<EventBean>) listOfBeans;
                var result = events.Remove(bean);
                if (events.IsEmpty())
                {
                    eventMap.Remove(key);
                }
                return result;
            }
            else if (listOfBeans.Equals(bean))
            {
                eventMap.Remove(key);
                return true;
            }

            return false;
        }

        public static void AddEventByKeyLazyListMapBack(object sortKey, EventBean eventBean, IDictionary<object, object> eventMap)
        {
            var existing = eventMap.Get(sortKey);
            if (existing == null)
            {
                eventMap.Put(sortKey, eventBean);
            }
            else
            {
                if (existing is IList<EventBean>)
                {
                    var existingList = (IList<EventBean>) existing;
                    existingList.Add(eventBean);
                }
                else
                {
                    var existingList = new List<EventBean>();
                    existingList.Add((EventBean) existing);
                    existingList.Add(eventBean);
                    eventMap.Put(sortKey, existingList);
                }
            }
        }

        public static void AddEventByKeyLazyListMapFront(object key, EventBean bean, IDictionary<object, object> eventMap)
        {
            var current = eventMap.Get(key);
            if (current != null)
            {
                if (current is IList<EventBean>)
                {
                    var events = (IList<EventBean>) current;
                    events.Insert(0, bean); // add to front, newest are listed first
                }
                else
                {
                    var theEvent = (EventBean) current;
                    var events = new List<EventBean>();
                    events.Add(bean);
                    events.Add(theEvent);
                    eventMap.Put(key, events);
                }
            }
            else
            {
                eventMap.Put(key, bean);
            }
        }

        public static bool IsAnySet(bool[] array)
        {
            return array.Any(t => t);
        }

        public static IDictionary<TKey, TValue> TwoEntryMap<TKey,TValue>(TKey keyOne, TValue valueOne, TKey keyTwo, TValue valueTwo)
        {
            var map = new Dictionary<TKey, TValue>();
            map.Put(keyOne, valueOne);
            map.Put(keyTwo, valueTwo);
            return map;
        }

    }
}
