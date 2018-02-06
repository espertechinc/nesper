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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.events;

using NUnit.Framework;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.supportregression.util
{
    public static class ArrayAssertionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Compare the objects in the two 2-dim String arrays assuming the exact same
        /// order.
        /// </summary>
        /// <param name="data">is the data to assertEqualsExactOrder against</param>
        /// <param name="expectedValues">is the expected values</param>
        public static void AssertEqualsStringArr(String[][] data, String[][] expectedValues)
        {
            if ((expectedValues == null) && (data == null)) {
                return;
            }
            if (((expectedValues == null) && (data != null)) ||
                ((expectedValues != null) && (data == null))) {
                Assert.Fail();
            }

            Assert.AreEqual(expectedValues.Length, data.Length, "mismatch in number to elements");

            for (int i = 0; i < expectedValues.Length; i++) {
                Assert.IsTrue(Collections.AreEqual(data[i], expectedValues[i]));
            }
        }


        /// <summary>
        /// Iterate through the views collection and check the presence of all values
        /// supplied in the exact same order, using the event bean underlying to compare
        /// </summary>
        /// <param name="enumerator">is the enumerator to iterate over and check returned values</param>
        /// <param name="expectedValues">is an array of expected underlying events</param>
        public static void AreEqualExactOrderUnderlying(IEnumerator<EventBean> enumerator, object[] expectedValues)
        {
            var underlyingValues = new List<object>();
            while (enumerator.MoveNext()) {
                Assert.IsNotNull(enumerator.Current);
                underlyingValues.Add(enumerator.Current.Underlying);
            }

            Assert.IsFalse(enumerator.MoveNext());

            object[] data = null;
            if (underlyingValues.Count > 0) {
                data = underlyingValues.ToArray();
            }

            AreEqualExactOrder(data, expectedValues);
        }


        /// <summary>
        /// Comparing the underlying events to the expected events using equals-semantics.
        /// </summary>
        /// <param name="events">is an event array to get the underlying objects</param>
        /// <param name="expectedValues">is an array of expected underlying events</param>
        public static void AreEqualExactOrderUnderlying(EventBean[] events, object[] expectedValues)
        {
            if ((expectedValues == null) && (events == null)) {
                return;
            }
            if (((expectedValues == null) && (events != null)) ||
                ((expectedValues != null) && (events == null))) {
                Assert.Fail();
            }

            Assert.AreEqual(expectedValues.Length, events.Length);

            AreEqualExactOrder(events.Select(theEvent => theEvent.Underlying).ToArray(), expectedValues);
        }

        public static void AssertPropsPerRow(EventBean[] received, object[][] propertiesPerRow)
        {
            if (propertiesPerRow == null) {
                if ((received == null) || (received.Length == 0)) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesPerRow);
            Assert.AreEqual(propertiesPerRow.Length, received.Length);

            for (int i = 0; i < propertiesPerRow.Length; i++) {
                var name = (String) propertiesPerRow[i][0];
                Object value = propertiesPerRow[i][1];
                Object eventProp = received[i].Get(name);
                Assert.AreEqual(value, eventProp, "Error asserting property named " + name);
            }
        }

        public static void AssertPropsPerRow(IList<object[]> received, object[][] propertiesListPerRow)
        {
            if (propertiesListPerRow == null) {
                if ((received == null) || (received.Count == 0)) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesListPerRow);
            Assert.AreEqual(propertiesListPerRow.Length, received.Count);

            for (int i = 0; i < propertiesListPerRow.Length; i++) {
                object[] receivedThisRow = received[i];
                object[] propertiesThisRow = propertiesListPerRow[i];
                Assert.AreEqual(receivedThisRow.Length, propertiesThisRow.Length);

                for (int j = 0; j < propertiesThisRow.Length; j++) {
                    Object expectedValue = propertiesThisRow[j];
                    Object receivedValue = receivedThisRow[j];
                    Assert.AreEqual(expectedValue, receivedValue, "Error asserting property");
                }
            }
        }

        public static void AssertPropsPerRow(DataMap[] received, String[] propertyNames, object[][] propertiesListPerRow)
        {
            if (propertiesListPerRow == null) {
                if ((received == null) || (received.Length == 0)) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesListPerRow);
            Assert.AreEqual(propertiesListPerRow.Length, received.Length);

            for (int i = 0; i < propertiesListPerRow.Length; i++) {
                object[] propertiesThisRow = propertiesListPerRow[i];
                for (int j = 0; j < propertiesThisRow.Length; j++) {
                    String name = propertyNames[j];
                    Object value = propertiesThisRow[j];
                    Object eventProp = received[i].Get(name);
                    Assert.AreEqual(value, eventProp, "Error asserting property named " + name);
                }
            }
        }

        public static void AssertPropsPerRow(object[][] received, String[] propertyNames,
                                             object[][] propertiesListPerRow)
        {
            if (propertiesListPerRow == null) {
                if ((received == null) || (received.Length == 0)) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesListPerRow);
            Assert.AreEqual(propertiesListPerRow.Length, received.Length);

            for (int i = 0; i < propertiesListPerRow.Length; i++) {
                object[] propertiesThisRow = propertiesListPerRow[i];
                for (int j = 0; j < propertiesThisRow.Length; j++) {
                    String name = propertyNames[j];
                    Object value = propertiesThisRow[j];
                    Object eventProp = received[i][j];
                    Assert.AreEqual(value, eventProp, "Error asserting property named " + name);
                }
            }
        }

        public static void AssertPropsPerRow(IEnumerable<EventBean> enumA,
                                             IEnumerable<EventBean> enumB, // safe enumerator
                                             String[] props,
                                             object[][] propertiesPerRow)
        {
            AssertPropsPerRow(enumA, props, propertiesPerRow);
            AssertPropsPerRow(enumB, props, propertiesPerRow);
        }

        public static void AssertPropsPerRow(IEnumerator<EventBean> enumA,
                                             IEnumerator<EventBean> enumB, // safe enumerator
                                             String[] props,
                                             object[][] propertiesPerRow)
        {
            AssertPropsPerRow(EnumeratorToArray(enumA), props, propertiesPerRow);
            AssertPropsPerRow(EnumeratorToArray(enumB), props, propertiesPerRow);
        }


        public static void AssertPropsPerRow(IEnumerable<EventBean> received,
                                             String[] propertyNames,
                                             object[][] propertiesListPerRow)
        {
            AssertPropsPerRow(received.ToArray(), propertyNames, propertiesListPerRow, "");
        }


        public static void AssertPropsPerRow(EventBean[] received, 
                                             String[] propertyNames,
                                             object[][] propertiesListPerRow)
        {
            AssertPropsPerRow(received, propertyNames, propertiesListPerRow, "");
        }

        public static void AssertPropsPerRow(EventBean[] received, String[] propertyNames,
                                             object[][] propertiesListPerRow, String streamName)
        {
            if (propertiesListPerRow == null) {
                if ((received == null) || (received.Length == 0)) {
                    return;
                }
                
                Assert.Fail("No events expected but received one or more for stream " + streamName);
            }
            if (received == null) {
                Assert.Fail("No events received, however some were expected for stream " + streamName);
            }
            Assert.AreEqual(propertiesListPerRow.Length, received.Length, "Mismatch in the number of rows received");

            for (int i = 0; i < propertiesListPerRow.Length; i++) {
                object[] propertiesThisRow = propertiesListPerRow[i];
                for (int j = 0; j < propertiesThisRow.Length; j++) {
                    String name = propertyNames[j];
                    Object value = propertiesThisRow[j];
                    Object eventProp = received[i].Get(name);
                    AssertProp("Error asserting property named " + name + " for row " + i + " for " + streamName, name, value, eventProp);
                }
            }
        }

        public static void AssertProps(EventBean received, String[] propertyNames, object[] propertiesThisRow)
        {
            if (propertiesThisRow == null) {
                if (received == null) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesThisRow);
            Assert.AreEqual(propertyNames.Length, propertiesThisRow.Length);

            for (int j = 0; j < propertiesThisRow.Length; j++) {
                String name = propertyNames[j].Trim();
                Object value = propertiesThisRow[j];
                Object eventProp = received.Get(name);
                AssertProp("Failed to assert property " + name, name, value, eventProp);
            }
        }

        private static void AssertProp(String message, String name, Object expected, Object received)
        {
            if ((expected != null) && (expected.GetType().IsArray) && 
                (received != null) && (received.GetType().IsArray))
            {
                object[] valueArray = ToObjectArray(expected);
                object[] eventPropArray = ToObjectArray(received);
                AreEqualExactOrder(eventPropArray, valueArray);
                return;
            }

            Assert.AreEqual(expected, received, "Error asserting property named '" + name + "'");
        }

        private static object[] ToObjectArray(Object array)
        {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (!array.GetType().IsArray)
            {
                throw new ArgumentException("Object not an array but type '" + array.GetType().FullName + "'");
            }

            var arrayObj = (Array) array;

            var size = arrayObj.Length;
            var val = new Object[size];
            for (int i = 0; i < size; i++) {
                val[i] = arrayObj.GetValue(i);
            }
            return val;
        }

        public static void AssertAllProps(EventBean received, object[] propertiesSortedByName)
        {
            if (propertiesSortedByName == null) {
                if (received == null) {
                    return;
                }
            }

            String[] propertyNames = received.EventType.PropertyNames.ToArray();
            var propertyNamesSorted = new String[propertyNames.Length];
            Array.Copy(propertyNames, 0, propertyNamesSorted, 0, propertyNames.Length);
            Array.Sort(propertyNamesSorted);

            Assert.IsNotNull(propertiesSortedByName);
            for (int j = 0; j < propertiesSortedByName.Length; j++) {
                String name = propertyNamesSorted[j].Trim();
                Object value = propertiesSortedByName[j];
                Object eventProp = received.Get(name);
                Assert.AreEqual(value, eventProp, "Error asserting property named '" + name + "'");
            }
        }

        public static void AssertPropsMap(DataMap pono, String[] propertyNames, params object[] propertiesThisRow)
        {
            if (propertiesThisRow == null) {
                if (pono == null) {
                    return;
                }
            }

            Assert.IsNotNull(propertiesThisRow);
            for (int j = 0; j < propertiesThisRow.Length; j++)
            {
                String name = propertyNames[j].Trim();
                Object value = propertiesThisRow[j];
                Object eventProp = pono.Get(name);
                Assert.AreEqual(value, eventProp, "Error asserting property named '" + name + "'");
            }
        }

        public static void AssertProps(Object pono, String[] propertyNames, params object[] propertiesThisRow)
        {
            EventBean ponoEvent = SupportContainer.Resolve<EventAdapterService>().AdapterForObject(pono);
            AssertProps(ponoEvent, propertyNames, propertiesThisRow);
        }

        public static void AssertEqualsAnyOrder(IEnumerable<EventBean> enumerable, String[] propertyNames,
                                            object[][] propertiesListPerRow)
        {
            AssertEqualsAnyOrder(enumerable.GetEnumerator(), propertyNames, propertiesListPerRow);
        }

        public static void AssertEqualsAnyOrder(IEnumerator<EventBean> enumerator, String[] propertyNames,
                                                object[][] propertiesListPerRow)
        {
            // convert to array of events
            EventBean[] received = EnumeratorToArray(enumerator);
            if (propertiesListPerRow == null) {
                if ((received == null) || (received.Length == 0)) {
                    return;
                }
                Assert.Fail("Expected no results but received " + received.Length + " events");
            }
            Assert.AreEqual(propertiesListPerRow.Length, received.Length);

            // build map of event and values
            IDictionary<EventBean, object[]> valuesEachEvent = new Dictionary<EventBean, object[]>();
            for (int i = 0; i < received.Length; i++) {
                var values = new Object[propertyNames.Length];
                for (int j = 0; j < propertyNames.Length; j++) {
                    values[j] = received[i].Get(propertyNames[j]);
                }
                valuesEachEvent.Put(received[i], values);
            }

            // Find each list of properties
            for (int i = 0; i < propertiesListPerRow.Length; i++) {
                object[] propertiesThisRow = propertiesListPerRow[i];
                bool isFound = false;

                foreach (var entry in valuesEachEvent) {
                    if (Collections.AreEqual(entry.Value, propertiesThisRow)) {
                        valuesEachEvent[entry.Key] = null;
                        //entry.Value = null;
                        isFound = true;
                        break;
                    }
                }

                if (!isFound) {
                    String text = "Error finding property set: " + propertiesListPerRow[i].Render() +
                                  " among values: \n" + Dump(valuesEachEvent);
                    Assert.Fail(text);
                }
            }

            // Should be all null values
            foreach (var entry in valuesEachEvent) {
                if (entry.Value != null) {
                    Assert.Fail();
                }
            }
        }

        private static String Dump(IDictionary<EventBean, object[]> valuesEachEvent)
        {
            var writer = new StringWriter();
            foreach (var entry in valuesEachEvent) {
                String values = entry.Value.Render();
                writer.WriteLine(values);
            }
            return writer.ToString();
        }

        public static void AssertEqualsAnyOrder(EventBean[][] expected, EventBean[][] received)
        {
            // EmptyFalse lists are fine
            if (received.IsEmptyOrNull() && expected.IsEmptyOrNull()) {
                return;
            }

            // Same number
            Assert.AreEqual(expected.Length, received.Length);

            // For each expected object find a received object
            int numMatches = 0;
            var foundReceived = new bool[received.Length];
            foreach (var expectedObject in expected) {
                bool found = false;
                for (int i = 0; i < received.Length; i++) {
                    // Ignore found received objects
                    if (foundReceived[i]) {
                        continue;
                    }

                    bool match = ArrayCompareUtil.CompareEqualsExactOrder(received[i], expectedObject);
                    if (match) {
                        found = true;
                        numMatches++;
                        foundReceived[i] = true;
                        break;
                    }
                }

                if (!found) {
                    Log.Error(".assertEqualsAnyOrder Not found in received results is expected=" +
                              expectedObject.Render());
                    Log.Error(".assertEqualsAnyOrder received=" + received.Render());
                }
                Assert.IsTrue(found);
            }

            // Must have matched exactly the number of objects times
            Assert.AreEqual(numMatches, expected.Length);
        }

        public static void AssertRefAnyOrderArr(object[][] expected, object[][] received)
        {
            // EmptyFalse lists are fine
            if (((received == null) && (expected == null)) ||
                ((received.Length == 0) && (expected == null)) ||
                ((received == null) && (expected.Length == 0))) {
                return;
            }

            // Same number
            Assert.AreEqual(expected.Length, received.Length);

            // For each expected object find a received object
            int numMatches = 0;
            var foundReceived = new bool[received.Length];
            foreach (var expectedArr in expected) {
                bool found = false;
                for (int i = 0; i < received.Length; i++) {
                    // Ignore found received objects
                    if (foundReceived[i]) {
                        continue;
                    }

                    bool match = ArrayCompareUtil.CompareRefExactOrder(received[i], expectedArr);
                    if (match) {
                        found = true;
                        numMatches++;
                        // Blank out received object so as to not match again
                        foundReceived[i] = true;
                        break;
                    }
                }

                if (!found) {
                    Log.Error(".assertEqualsAnyOrder Not found in received results is expected=" + expectedArr.Render());
                    for (int j = 0; j < received.Length; j++) {
                        Log.Error(".assertEqualsAnyOrder                              received (" + j + "):" +
                                  received[j].Render());
                    }
                    Assert.Fail();
                }
            }

            // Must have matched exactly the number of objects times
            Assert.AreEqual(numMatches, expected.Length);
        }

        /// <summary>
        /// Asserts that all values in the given object array are boolean-typed values and
        /// are true
        /// </summary>
        /// <param name="objects">values to assert that they are all true</param>
        public static void AssertAllBooleanTrue(object[] objects)
        {
            foreach (object item in objects)
            {
                Assert.IsTrue(((bool?) item).GetValueOrDefault(false));
            }
        }

        /// <summary>
        /// Assert the class of the objects in the object array matches the expected classes
        /// in the classes array.
        /// </summary>
        /// <param name="classes">is the expected class</param>
        /// <param name="objects">is the objects to check the class for</param>
        public static void AssertTypeEqualsAnyOrder(Type[] classes, object[] objects)
        {
            Assert.AreEqual(classes.Length, objects.Length);
            var resultClasses = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                resultClasses[i] = objects[i].GetType();
            }
            AssertEqualsAnyOrder(resultClasses, classes);
        }

        public static T[] EnumeratorToArray<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null) {
                Assert.Fail("Null enumerator");
            }

            var events = new List<T>();
            while (enumerator.MoveNext())
                events.Add(enumerator.Current);

            return events.ToArray();
        }

        public static object[] EnumeratorToArrayUnderlying(IEnumerator<EventBean> enumerator)
        {
            var events = new List<object>();
            while (enumerator.MoveNext()) {
                EventBean eventBean = enumerator.Current;
                events.Add(eventBean.Underlying);
            }

            return events.ToArray();
        }

        public static int EnumeratorCount(IEnumerator<EventBean> enumerator)
        {
            int count = 0;
            while (enumerator.MoveNext())
                count++;
            return count;
        }

        public static object[] sum(object[] srcOne, object[] srcTwo)
        {
            var result = new Object[srcOne.Length + srcTwo.Length];
            Array.Copy(srcOne, 0, result, 0, srcOne.Length);
            Array.Copy(srcTwo, 0, result, srcOne.Length, srcTwo.Length);
            return result;
        }

        /// <summary>
        /// Compare the event properties returned by the events of the enumerator with the
        /// supplied values.
        /// </summary>
        /// <param name="enumerator">supplies events</param>
        /// <param name="fields">The fields.</param>
        /// <param name="expectedValues">is the expected values</param>
        public static void AreEqualExactOrder(IEnumerator<EventBean> enumerator, String[] fields, object[][] expectedValues)
        {
            var rows = new List<object[]>();
            while (enumerator.MoveNext()) {
                EventBean theEvent = enumerator.Current;
                var eventProps = new Object[fields.Length];
                for (int i = 0; i < fields.Length; i++) {
                    eventProps[i] = theEvent.Get(fields[i]);
                }
                rows.Add(eventProps);
            }

            Assert.IsFalse(enumerator.MoveNext());

            if (rows.Count == 0) {
                Assert.IsNull(expectedValues, "Expected rows in result but received none");
                return;
            }

            object[][] data = rows.ToArray();
            if ((expectedValues == null) && (data != null)) {
                Assert.Fail("Expected no values but received data: " + data.Length + " elements");
            }

            Assert.AreEqual(expectedValues.Length, data.Length);
            for (int i = 0; i < data.Length; i++) {
                AreEqualExactOrder(data[i], expectedValues[i]);
            }
        }

        public static void Compare(EventBean[] events, IList<IDictionary<String, Object>> expectedValues)
        {
            if ((expectedValues == null) && (events == null)) {
                return;
            }
            if (((expectedValues == null) && (events != null)) ||
                ((expectedValues != null) && (events == null))) {
                Assert.Fail();
            }

            Assert.AreEqual(expectedValues.Count, events.Length);

            for (int i = 0; i < expectedValues.Count; i++) {
                Compare(events[i], expectedValues[i]);
            }
        }

        public static void Compare(IEnumerator<EventBean> enumerator, IList<IDictionary<String, Object>> expectedValues)
        {
            var values = new List<EventBean>();
            while (enumerator.MoveNext())
                values.Add(enumerator.Current);

            Assert.IsFalse(enumerator.MoveNext());

            EventBean[] data = null;
            if (values.Count > 0) {
                data = values.ToArray();
            }

            Compare(data, expectedValues);
        }


        private static void Compare(EventBean theEvent, IDictionary<String, Object> expected)
        {
            foreach (var entry in expected) {
                Object valueExpected = entry.Value;
                Object property = theEvent.Get(entry.Key);

                Assert.AreEqual(valueExpected, property);
            }
        }

        public static object[][] AddArray(object[][] first, params object[][][] more)
        {
            int len = first.Length + more.Sum(next => next.Length);

            var result = new Object[len][];
            int count = 0;
            for (int i = 0; i < first.Length; i++) {
                result[count] = first[i];
                count++;
            }

            for (int i = 0; i < more.Length; i++) {
                object[][] next = more[i];
                for (int j = 0; j < next.Length; j++) {
                    result[count] = next[j];
                    count++;
                }
            }

            return result;
        }

        #region "AreEqualExactOrder"

        /// <summary>
        /// Iterate through the views collection and check the presence of all values
        /// supplied in the exact same order.
        /// </summary>
        /// <param name="receivedValues">is the enumerator to iterate over and check returned values</param>
        /// <param name="expectedValues">is a map of expected values</param>
        public static void AreEqualExactOrder<T>(IEnumerable<T> receivedValues, IEnumerable<T> expectedValues)
        {
            AreEqualExactOrder(
                receivedValues.AsList(),
                expectedValues.AsList());
        }

        /// <summary>
        /// Iterate through the views collection and check the presence of all values
        /// supplied in the exact same order.
        /// </summary>
        /// <param name="enumerator">is the enumerator to iterate over and check returned values</param>
        /// <param name="expectedValues">is a map of expected values</param>
        public static void AreEqualExactOrder<T>(IEnumerator<T> enumerator, IList<T> expectedValues)
        {
            List<T> inputList = null;
            if (enumerator.MoveNext()) {
                inputList = new List<T>();
                do {
                    inputList.Add(enumerator.Current);
                } while (enumerator.MoveNext());
            }

            AreEqualExactOrder(inputList, expectedValues);
        }

        /// <summary>
        /// Reference-equals the objects in the two object list assuming the exact same
        /// order.
        /// </summary>
        /// <param name="data">is the data to check reference against</param>
        /// <param name="expectedValues">is the expected values</param>
        public static void AreEqualExactOrder<T>(IList<T> expectedValues, IList<T> data)
        {
            if ((expectedValues == null) && (data == null)) {
                return;
            }
            if (((expectedValues == null) && (data != null)) ||
                ((expectedValues != null) && (data == null))) {
                Assert.Fail();
            }

            Assert.AreEqual(expectedValues.Count, data.Count);

            for (int i = 0; i < expectedValues.Count; i++) {
                Object value = data[i];
                Object expected = expectedValues[i];
                AssertProp("Failed to assert at element " + i, "element " + i, expected, value);
            }
        }

        #endregion

        #region "AssertSameExactOrder"

        /// <summary>
        /// Reference-equals the objects in the two object arrays assuming the exact same
        /// order.
        /// </summary>
        /// <param name="data">is the data to check reference against</param>
        /// <param name="expectedValues">is the expected values</param>
        public static void AssertSameExactOrder<T>(IList<T> expectedValues, IList<T> data)
        {
            if ((expectedValues == null) && (data == null)) {
                return;
            }
            if (((expectedValues == null) && (data != null)) ||
                ((expectedValues != null) && (data == null))) {
                Assert.Fail();
            }

            Assert.AreEqual(expectedValues.Count, data.Count);

            for (int i = 0; i < expectedValues.Count; i++) {
                Assert.AreSame(expectedValues[i], data[i], "at element " + i);
            }
        }

        #endregion

        #region "AssertEqualsAnyOrder"

        public static void AssertEqualsAnyOrder<T>(ICollection<T> expected, ICollection<T> input)
        {
            if ((input == null) && (expected == null))
                return;

            if (input == null) {
                Assert.IsTrue(expected.Count == 0);
                return;
            }

            if (expected == null) {
                Assert.IsTrue(input.Count == 0);
                return;
            }

            Assert.AreEqual(expected.Count, input.Count, "length mismatch");
            foreach (T expectedItem in expected) {
                bool doesContain = input.Contains(expectedItem);
                Assert.IsTrue(doesContain, "not found: " + expectedItem);
            }
        }

        #endregion
    
        public static EventBean[] Sort(IEnumerable<EventBean> oldevents, String property) 
        {
            return oldevents
                .OrderBy(eventBean => eventBean.Get(property))
                .ToArray();
        }

        public static EventBean[] Sort(EventBean[] oldevents, String property)
        {
            return oldevents
                .OrderBy(eventBean => eventBean.Get(property))
                .ToArray();
        }
        
        public static void AssertUnorderedMap<K,V>(IDictionary<K,V> amap, object[][] expected)
        {
            Assert.AreEqual(expected.Length, amap.Count);

            var matchNumber = new HashSet<int>();
            foreach (var entry in amap) {
                bool matchFound = false;
                for (int i = 0; i < expected.Length; i++) {
                    if (matchNumber.Contains(i)) {
                        continue;
                    }

                    if (Equals(expected[i][0], entry.Key)) {
                        matchFound = true;
                        matchNumber.Add(i);
                        if (expected[i][1] == null && entry.Value == null) {
                            continue;
                        }
                        if (!Equals(expected[i][1], entry.Value)) {
                            Assert.Fail("Failed to match value for key '" + entry.Key + "' expected '" +
                                        expected[i][i] + "' received '" + entry.Value + "'");
                        }
                    }
                }

                if (!matchFound) {
                    Assert.Fail("Failed to find key '" + entry.Key + "'");
                }
            }
        }

        public static void AssertNotContains(IEnumerable<String> strings, params String[] values)
        {
            var set = new HashSet<String>(strings);
            foreach (string value in values) {
                Assert.That(set, Has.No.Member(value));
            }
        }

        public static void AssertContains(IEnumerable<String> strings, params String[] values)
        {
            var set = new HashSet<String>(strings);
            foreach (string value in values) {
                Assert.That(set, Has.Member(value));
            }
        }

        public static void AssertAllValuesSame(EventBean theEvent, String[] fields, Object value)
        {
            foreach (string field in fields)
            {
                Assert.AreEqual(value, theEvent.Get(field), "Field " + field);
            }
        }

        public static object[] AddArrayObjectArr(params object[][] more)
        {
            var list = new List<object>();
            foreach (var array in more)
            {
                list.AddRange(array);
            }

            return list.ToArray();
        }

            public static object[] EventsToObjectArr(EventBean[] events, String field)
            {
                if (events == null)
                {
                    return null;
                }
                object[] objects = new Object[events.Length];
                for (int i = 0; i < events.Length; i++)
                {
                    objects[i] = events[i].Get(field);
                }
                return objects;
            }

            public static object[][] EventsToObjectArr(EventBean[] events, String[] fields)
            {
                if (events == null)
                {
                    return null;
                }
                object[][] objects = new Object[events.Length][];
                for (int i = 0; i < events.Length; i++)
                {
                    EventBean theEvent = events[i];
                    object[] values = new Object[fields.Length];
                    for (int j = 0; j < fields.Length; j++)
                    {
                        values[j] = theEvent.Get(fields[j]);
                    }
                    objects[i] = values;
                }
                return objects;
            }
    }
}
