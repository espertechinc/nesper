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

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;
using com.espertech.esper.core.support;
using com.espertech.esper.events;

namespace com.espertech.esper.client.scopetest
{
    /// <summary>
    /// Assertion methods for event processing applications.
    /// </summary>
    public class EPAssertionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Deep compare two 2-dimensional string arrays for the exact same length of arrays and order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder(string[][] expected, string[][] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                ScopeTestHelper.AssertTrue(Collections.AreEqual(actual[i], expected[i]));
            }
        }

        /// <summary>Compare two 2-dimensional arrays, and using property names for messages, against expected values. </summary>
        /// <param name="actual">array of objects</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertEqualsExactOrder(object[][] actual, string[] propertyNames, object[][] expected)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                object[] propertiesThisRow = expected[i];
                for (int j = 0; j < propertiesThisRow.Length; j++)
                {
                    string name = propertyNames[j];
                    object value = propertiesThisRow[j];
                    object eventProp = actual[i][j];
                    ScopeTestHelper.AssertEquals("Error asserting property named " + name, value, eventProp);
                }
            }
        }

        /// <summary>Compare the collection of object arrays, and using property names for messages, against expected values. </summary>
        /// <param name="actual">colleciton of array of objects</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertEqualsExactOrder(ICollection<object[]> actual, string[] propertyNames, object[][] expected)
        {
            object[][] arr = actual.ToArray();
            AssertEqualsExactOrder(arr, propertyNames, expected);
        }

        /// <summary>Compare the objects in the expected arrays and actual collection assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder(object[] expected, ICollection actual)
        {
            object[] actualArray = null;
            if (actual != null)
            {
                actualArray = actual.UnwrapIntoArray<object>();
            }
            AssertEqualsExactOrder(expected, actualArray);
        }

        /// <summary>Compare the enumerator-returned events against the expected events </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder(EventBean[] expected, IEnumerator<EventBean> actual)
        {
            AssertEqualsExactOrder((object[]) expected, actual);
        }

        /// <summary>Compare the underlying events returned by the enumerator to the expected values. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrderUnderlying(object[] expected, IEnumerator<EventBean> actual)
        {
            var underlyingValues = new List<object>();
            while (actual.MoveNext())
            {
                underlyingValues.Add(actual.Current.Underlying);
            }

            if (actual.MoveNext())
            {
                ScopeTestHelper.Fail();
            }

            object[] data = null;
            if (underlyingValues.Count > 0)
            {
                data = underlyingValues.ToArray();
            }

            AssertEqualsExactOrder(expected, data);
        }

        /// <summary>Comparing the underlying events to the expected events using equals-semantics. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrderUnderlying(object[] expected, EventBean[] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            AssertEqualsExactOrder(expected, actual.Select(theEvent => theEvent.Underlying).ToArray());
        }

        /// <summary>
        /// Converts to an object collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static ICollection<object> ToObjectCollection<T>(ICollection<T> collection)
        {
            if (collection == null)
                return null;
            return new MagicCollection<T>(collection);
        }

        /// <summary>Compare the objects in the 2-dimension object arrays assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder(object[][] expected, IList<object[]> actual)
        {
            var transpose = ToObjectCollection(actual);
            if (CompareArrayAndCollSize(expected, transpose))
            {
                return;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                object[] receivedThisRow = actual[i];
                object[] propertiesThisRow = expected[i];
                ScopeTestHelper.AssertEquals(receivedThisRow.Length, propertiesThisRow.Length);

                for (int j = 0; j < propertiesThisRow.Length; j++)
                {
                    object expectedValue = propertiesThisRow[j];
                    object receivedValue = receivedThisRow[j];
                    ScopeTestHelper.AssertEquals("Error asserting property", expectedValue, receivedValue);
                }
            }
        }

        /// <summary>Compare the objects in the two object arrays assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder(object[] expected, object[] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            if ((expected == null) && (actual == null))
            {
                return;
            }

            if (expected == null)
            {
                expected = new object[0];
            }

            for (int i = 0; i < expected.Length; i++)
            {
                object value = actual[i];
                object expectedValue = expected[i];
                AssertEqualsAllowArray("Failed to assert at element " + i, expectedValue, value);
            }
        }

        /// <summary>Reference-equals the objects in the two object arrays assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertSameExactOrder(object[] expected, object[] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                ScopeTestHelper.AssertSame("at element " + i, expected[i], actual[i]);
            }
        }

        public static void AssertEqualsExactOrder<T>(T[][] expected, T[][] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            if ((expected == null) && (actual == null))
                return;
            if (expected == null)
                ScopeTestHelper.Fail("expected was null; actual was not");
            if (actual == null)
                ScopeTestHelper.Fail("actual was null; expected was not");

            for (int i = 0; i < expected.Length; i++)
            {
                AssertEqualsExactOrder(expected[i], actual[i]);
            }
        }

        /// <summary>Compare the short values in the two  arrays assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder<T>(T[] expected, T[] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            if ((expected == null) && (actual == null))
                return;
            if (expected == null)
                ScopeTestHelper.Fail("expected was null; actual was not");
            if (actual == null)
                ScopeTestHelper.Fail("actual was null; expected was not");

            for (int i = 0; i < expected.Length; i++)
            {
                ScopeTestHelper.AssertEquals(expected[i], actual[i]);
            }
        }

        /// <summary>Compare the short values in the two  arrays assuming the exact same order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder<T>(T[] expected, T?[] actual) where T : struct
        {
            if (CompareArraySize(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                ScopeTestHelper.AssertNotNull(actual[i]);
                ScopeTestHelper.AssertEquals(expected[i], actual[i].Value);
            }
        }

        /// <summary>Compare the objects returned by the enumerable to the an object array. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder<T>(ICollection<T> expected, ICollection<T> actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            foreach (var tuple in actual.Merge(expected))
            {
                ScopeTestHelper.AssertEquals(tuple.A, tuple.B);
            }
        }

        /// <summary>Compare the objects returned by the enumerator to the an object array. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsExactOrder<T>(T[] expected, IEnumerator<T> actual)
        {
            var values = new List<T>();
            while (actual.MoveNext())
            {
                values.Add(actual.Current);
            }

            if (actual.MoveNext())
            {
                ScopeTestHelper.Fail();
            }

            T[] data = null;
            if (values.Count > 0)
            {
                data = values.ToArray();
            }

            AssertEqualsExactOrder(expected, data);
        }

        /// <summary>Assert that each integer value in the expected array is contained in the actual array. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsAnyOrder<T>(T[] expected, ICollection<T> actual)
        {
            var transposeActual = ToObjectCollection(actual);
            if (CompareArrayAndCollSize(expected, transposeActual))
            {
                return;
            }

            foreach (T anExpected in expected)
            {
                if (anExpected is ICollection)
                {
                    bool containsExpected = transposeActual.Any(i => SmartEquals(i, anExpected));
                    ScopeTestHelper.AssertTrue("not found: " + ((ICollection)anExpected).Render(), containsExpected);
                }
                else
                {
                    ScopeTestHelper.AssertTrue("not found: " + anExpected, actual.Contains(anExpected));
                }
            }
        }

        /// <summary>
        /// Performs an intelligent equality test.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueA">The value A.</param>
        /// <param name="valueB">The value B.</param>
        /// <returns></returns>
        private static bool SmartEquals<T>(T valueA, T valueB)
        {
            if ((valueA is ICollection) && (valueB is ICollection))
            {
                var collectionA = ((ICollection)valueA).Cast<object>().ToList();
                var collectionB = ((ICollection)valueA).Cast<object>().ToList();
                return Collections.AreEqual(collectionA, collectionB);
            }
            else
            {
                return Equals(valueA, valueB);
            }
        }

        /// <summary>Compare the two object arrays allowing any order. </summary>
        /// <param name="expected">is the expected values</param>
        /// <param name="actual">is the actual values</param>
        public static void AssertEqualsAnyOrder<T>(ICollection<T> expected, ICollection<T> actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            var received = new LinkedList<T>(actual);
            foreach (T expectedObject in expected)
            {
                var node = received.FirstNode(value => SmartEquals(value, expectedObject));
                if (node == null)
                {
                    AssertProxy.Fail(string.Format("missing expected value: {0}", expectedObject));
                }
                else
                {
                    received.Remove(node);
                }
            }

            AssertProxy.True(received.Count == 0);
        }

        /// <summary>Compare the property values returned by events of both iterators with the expected values, using exact-order semantics. </summary>
        /// <param name="enumerator">provides events</param>
        /// <param name="safeEnumerator">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(IEnumerator<EventBean> enumerator, IEnumerator<EventBean> safeEnumerator, string[] propertyNames, object[][] expected)
        {
            AssertPropsPerRow(EnumeratorToArray(enumerator), propertyNames, expected);
            AssertPropsPerRow(EnumeratorToArray(safeEnumerator), propertyNames, expected);
            safeEnumerator.Dispose();
        }

        /// <summary>Compare the property values returned by events of both iterators with the expected values, using any-order semantics. </summary>
        /// <param name="enumerator">provides events</param>
        /// <param name="safeEnumerator">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRowAnyOrder(IEnumerator<EventBean> enumerator, IEnumerator<EventBean> safeEnumerator, string[] propertyNames, object[][] expected)
        {
            AssertPropsPerRowAnyOrder(EnumeratorToArray(enumerator), propertyNames, expected);
            AssertPropsPerRowAnyOrder(EnumeratorToArray(safeEnumerator), propertyNames, expected);
            safeEnumerator.Dispose();
        }

        /// <summary>Compare the property values returned by events of the enumerator with the expected values, using any-order semantics. </summary>
        /// <param name="enumerator">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRowAnyOrder(IEnumerator<EventBean> enumerator, string[] propertyNames, object[][] expected)
        {
            AssertPropsPerRowAnyOrder(EnumeratorToArray(enumerator), propertyNames, expected);
        }

        /// <summary>Compare the property values returned by events of both iterators with the expected values, using exact-order semantics. </summary>
        /// <param name="enumerator">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(IEnumerator<EventBean> enumerator, string[] propertyNames, object[][] expected)
        {
            AssertPropsPerRow(EnumeratorToArray(enumerator), propertyNames, expected);
        }

        /// <summary>
        /// Assert that property values of rows, wherein each row can either be Map or PONO objects, matches the expected values.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="received">array of objects may contain Map and PONO events</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected value</param>
        public static void AssertPropsPerRow(
            IContainer container,
            IList<object[]> received, 
            string[] propertyNames, 
            object[][] expected)
        {
            ScopeTestHelper.AssertEquals(received.Count, expected.Length);
            for (int row = 0; row < received.Count; row++)
            {
                AssertProps(container, received[row], propertyNames, expected[row]);
            }
        }

        /// <summary>Compare the Map values identified by property names against expected values. </summary>
        /// <param name="actual">array of Maps, one for each row</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(IDictionary<string, object>[] actual, string[] propertyNames, object[][] expected)
        {
            if (CompareArraySize(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                object[] propertiesThisRow = expected[i];
                for (int j = 0; j < propertiesThisRow.Length; j++)
                {
                    string name = propertyNames[j];
                    object value = propertiesThisRow[j];
                    object eventProp = actual[i].Get(name);
                    ScopeTestHelper.AssertEquals("Error asserting property named " + name, value, eventProp);
                }
            }
        }

        /// <summary>Compare the property values of events with the expected values, using exact-order semantics. </summary>
        /// <param name="received">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(EventBean[] received, string[] propertyNames, object[][] expected)
        {
            AssertPropsPerRow(received, propertyNames, expected, "");
        }

        /// <summary>Compare the property values of events with the expected values, using exact-order semantics. </summary>
        /// <param name="actual">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        /// <param name="streamName">an optional name for the stream for use in messages</param>
        public static void AssertPropsPerRow(EventBean[] actual, string[] propertyNames, object[][] expected, string streamName)
        {
            if (CompareArraySize(expected, actual))
            {
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                object[] propertiesThisRow = expected[i];
                ScopeTestHelper.AssertEquals("Number of properties expected mismatches for row " + i, propertyNames.Length, propertiesThisRow.Length);
                for (int j = 0; j < propertiesThisRow.Length; j++)
                {
                    string name = propertyNames[j];
                    object value = propertiesThisRow[j];
                    object eventProp = actual[i].Get(name);
                    StringWriter writer = new StringWriter();
                    writer.Write("Error asserting property named ");
                    writer.Write(name);
                    writer.Write(" for row ");
                    writer.Write(i);
                    if (streamName != null && streamName.Trim().Length != 0)
                    {
                        writer.Write(" for stream ");
                        writer.Write(streamName);
                    }
                    AssertEqualsAllowArray(writer.ToString(), value, eventProp);
                }
            }
        }

        /// <summary>Compare the property values of events with the expected values, using any-order semantics. </summary>
        /// <param name="actual">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRowAnyOrder(EventBean[] actual, string[] propertyNames, object[][] expected)
        {
            if (CompareArraySize(expected, actual))
            {
                return;
            }

            // build expected
            var expectedArray = new object[expected.Length];
            Array.Copy(expected, 0, expectedArray, 0, expectedArray.Length);

            // build received
            var receivedArray = new object[actual.Length];
            for (int i = 0; i < actual.Length; i++)
            {
                var data = new object[propertyNames.Length];
                receivedArray[i] = data;
                for (int j = 0; j < propertyNames.Length; j++)
                {
                    var name = propertyNames[j];
                    var eventProp = actual[i].Get(name);
                    data[j] = eventProp;
                }
            }

            AssertEqualsAnyOrder(expectedArray, receivedArray);
        }

        public static void AssertPropsPerRowAnyOrder(UniformPair<EventBean[]> pair, string[] propertyNames, object[][] expectedNew, object[][] expectedOld)
        {
            AssertPropsPerRowAnyOrder(pair.First, propertyNames, expectedNew);
            AssertPropsPerRowAnyOrder(pair.Second, propertyNames, expectedOld);
        }

        /// <summary>Assert that the property values of a single event match the expected values. </summary>
        /// <param name="received">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expected">expected values</param>
        public static void AssertProps(EventBean received, string[] propertyNames, object[] expected)
        {
            if (CompareCount(expected, propertyNames))
            {
                return;
            }

            for (int j = 0; j < expected.Length; j++)
            {
                string name = propertyNames[j].Trim();
                object value = expected[j];
                object eventProp = received.Get(name);
                AssertEqualsAllowArray("Failed to assert property '" + name + "'", value, eventProp);
            }
        }

        /// <summary>Assert that the property values of a new event and a removed event match the expected insert and removed values. </summary>
        /// <param name="received">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expectedInsert">expected values insert stream</param>
        /// <param name="expectedRemoved">expected values remove stream</param>
        public static void AssertProps(UniformPair<EventBean> received, string[] propertyNames, object[] expectedInsert, object[] expectedRemoved)
        {
            AssertProps(received.First, propertyNames, expectedInsert);
            AssertProps(received.Second, propertyNames, expectedRemoved);
        }

        /// <summary>Assert that the property values of a new event and a removed event match the expected insert and removed values. </summary>
        /// <param name="received">provides events</param>
        /// <param name="propertyNames">array of property names</param>
        /// <param name="expectedInsert">expected values insert stream</param>
        /// <param name="expectedRemoved">expected values remove stream</param>
        public static void AssertPropsPerRow(UniformPair<EventBean[]> received, string[] propertyNames, object[][] expectedInsert, object[][] expectedRemoved)
        {
            AssertPropsPerRow(received.First, propertyNames, expectedInsert);
            AssertPropsPerRow(received.Second, propertyNames, expectedRemoved);
        }

        /// <summary>Assert that the property values of the events (insert and remove pair) match the expected insert and removed values for a single property. </summary>
        /// <param name="received">provides events</param>
        /// <param name="propertyName">property name</param>
        /// <param name="expectedInsert">expected values insert stream</param>
        /// <param name="expectedRemoved">expected values remove stream</param>
        public static void AssertPropsPerRow(UniformPair<EventBean[]> received, string propertyName, object[] expectedInsert, object[] expectedRemoved)
        {
            object[] propsInsert = EventsToObjectArr(received.First, propertyName);
            AssertEqualsExactOrder(expectedInsert, propsInsert);

            object[] propsRemove = EventsToObjectArr(received.Second, propertyName);
            AssertEqualsExactOrder(expectedRemoved, propsRemove);
        }

        /// <summary>Assert that the underlying objects of the events (insert and remove pair) match the expected insert and removed objects.  </summary>
        /// <param name="received">provides events</param>
        /// <param name="expectedUnderlyingInsert">expected underlying object insert stream</param>
        /// <param name="expectedUnderlyingRemove">expected underlying object remove stream</param>
        public static void AssertUnderlyingPerRow(UniformPair<EventBean[]> received, object[] expectedUnderlyingInsert, object[] expectedUnderlyingRemove)
        {
            EventBean[] newEvents = received.First;
            EventBean[] oldEvents = received.Second;

            if (expectedUnderlyingInsert != null)
            {
                ScopeTestHelper.AssertEquals(expectedUnderlyingInsert.Length, newEvents.Length);
                for (int i = 0; i < expectedUnderlyingInsert.Length; i++)
                {
                    ScopeTestHelper.AssertSame(expectedUnderlyingInsert[i], newEvents[i].Underlying);
                }
            }
            else
            {
                ScopeTestHelper.AssertNull(newEvents);
            }

            if (expectedUnderlyingRemove != null)
            {
                ScopeTestHelper.AssertEquals(expectedUnderlyingRemove.Length, oldEvents.Length);
                for (int i = 0; i < expectedUnderlyingRemove.Length; i++)
                {
                    ScopeTestHelper.AssertSame(expectedUnderlyingRemove[i], oldEvents[i].Underlying);
                }
            }
            else
            {
                ScopeTestHelper.AssertNull(oldEvents);
            }
        }

        /// <summary>Asserts that the property values of a single event, using property names as provided by the event type in sorted order by property name, match against the expected values. </summary>
        /// <param name="received">provides events</param>
        /// <param name="expected">expected values</param>
        public static void AssertAllPropsSortedByName(EventBean received, object[] expected)
        {
            if (expected == null)
            {
                if (received == null)
                {
                    return;
                }
            }
            else
            {
                ScopeTestHelper.AssertNotNull(received);
            }

            if (expected != null)
            {
                var propertyNames = received.EventType.PropertyNames.ToArray();
                var propertyNamesSorted = new string[propertyNames.Length];
                Array.Copy(propertyNames, 0, propertyNamesSorted, 0, propertyNames.Length);
                Array.Sort(propertyNamesSorted);

                for (int j = 0; j < expected.Length; j++)
                {
                    var name = propertyNamesSorted[j].Trim();
                    var value = expected[j];
                    var eventProp = received.Get(name);
                    ScopeTestHelper.AssertEquals("Error asserting property named '" + name + "'", value, eventProp);
                }
            }
        }

        public static void AssertPropsMap(IDictionary<object, object> received, string[] propertyNames, params object[] expected)
        {
            if (expected == null)
            {
                if (received == null)
                {
                    return;
                }
            }
            else
            {
                ScopeTestHelper.AssertNotNull(received);
                ScopeTestHelper.AssertEquals("Mismatch in number of values to compare", expected.Length, propertyNames.Length);
            }

            if (expected != null)
            {
                for (int j = 0; j < expected.Length; j++)
                {
                    string name = propertyNames[j].Trim();
                    object value = expected[j];
                    object eventProp = received.Get(name);
                    AssertEqualsAllowArray("Error asserting property named '" + name + "'", value, eventProp);
                }
            }
        }

        /// <summary>Compare the values of a Map against the expected values. </summary>
        /// <param name="received">provides events</param>
        /// <param name="expected">expected values</param>
        /// <param name="propertyNames">property names to assert</param>
        public static void AssertPropsMap(IDictionary<string, object> received, string[] propertyNames, params object[] expected)
        {
            if (expected == null)
            {
                if (received == null)
                {
                    return;
                }
            }
            else
            {
                ScopeTestHelper.AssertNotNull(received);
                ScopeTestHelper.AssertEquals("Mismatch in number of values to compare", expected.Length, propertyNames.Length);
            }

            if (expected != null)
            {
                for (int j = 0; j < expected.Length; j++)
                {
                    string name = propertyNames[j].Trim();
                    object value = expected[j];
                    object eventProp = received.Get(name);
                    AssertEqualsAllowArray("Error asserting property named '" + name + "'", value, eventProp);
                }
            }
        }

        /// <summary>Compare the values of a object array (single row) against the expected values. </summary>
        /// <param name="received">provides properties</param>
        /// <param name="expected">expected values</param>
        /// <param name="propertyNames">property names to assert</param>
        public static void AssertPropsObjectArray(object[] received, string[] propertyNames, params object[] expected)
        {
            if (expected == null)
            {
                if (received == null)
                {
                    return;
                }
            }
            else {
                ScopeTestHelper.AssertNotNull(received);
            }

            if (expected != null) {
                for (int j = 0; j < expected.Length; j++)
                {
                    string name = propertyNames[j].Trim();
                    object value = expected[j];
                    object eventProp = received[j];
                    ScopeTestHelper.AssertEquals("Error asserting property named '" + name + "'", value, eventProp);
                }
            }
        }

        /// <summary>
        /// Compare the properties of an object against the expected values.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="received">provides events</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected values</param>
        /// <exception cref="ArgumentException">No object provided to compare to</exception>
        public static void AssertPropsPono(
            IContainer container, 
            object received, 
            string[] propertyNames, 
            params object[] expected)
        {
            if (received == null)
            {
                throw new ArgumentException("No object provided to compare to");
            }
            var theEvent = container.Resolve<EventAdapterService>().AdapterForObject(received);
            AssertProps(theEvent, propertyNames, expected);
        }

        /// <summary>Compare two 2-dimensional event arrays. </summary>
        /// <param name="expected">expected values</param>
        /// <param name="actual">actual values</param>
        public static void AssertEqualsAnyOrder(EventBean[][] expected, EventBean[][] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            // For each expected object find a received object
            var numMatches = 0;
            var foundReceived = new bool[actual.Length];
            foreach (EventBean[] expectedObject in expected)
            {
                bool found = false;
                for (int i = 0; i < actual.Length; i++)
                {
                    // Ignore found received objects
                    if (foundReceived[i])
                    {
                        continue;
                    }

                    bool match = CompareEqualsExactOrder(actual[i], expectedObject);
                    if (match)
                    {
                        found = true;
                        numMatches++;
                        foundReceived[i] = true;
                        break;
                    }
                }

                if (!found)
                {
                    Log.Error(".assertEqualsAnyOrder Not found in received results is expected=" + CompatExtensions.Render(expectedObject));
                    Log.Error(".assertEqualsAnyOrder received=" + CompatExtensions.Render(actual));
                }

                ScopeTestHelper.AssertTrue("Failed to find value " + expectedObject + ", check the error logs", found);
            }

            // Must have matched exactly the number of objects times
            ScopeTestHelper.AssertEquals(numMatches, expected.Length);
        }

        /// <summary>Compare two 2-dimensional object arrays using reference-equals semantics. </summary>
        /// <param name="expected">expected values</param>
        /// <param name="actual">actual values</param>
        public static void AssertSameAnyOrder(object[][] expected, object[][] actual)
        {
            if (CompareCount(expected, actual))
            {
                return;
            }

            // For each expected object find a received object
            var numMatches = 0;
            var foundReceived = new bool[actual.Length];
            foreach (object[] expectedArr in expected)
            {
                bool found = false;
                for (int i = 0; i < actual.Length; i++)
                {
                    // Ignore found received objects
                    if (foundReceived[i])
                    {
                        continue;
                    }

                    bool match = CompareRefExactOrder(actual[i], expectedArr);
                    if (match)
                    {
                        found = true;
                        numMatches++;
                        // Blank out received object so as to not match again
                        foundReceived[i] = true;
                        break;
                    }
                }

                if (!found)
                {
                    Log.Error(".assertEqualsAnyOrder Not found in received results is expected=" + CompatExtensions.Render(expectedArr));
                    for (int j = 0; j < actual.Length; j++)
                    {
                        Log.Error(".assertEqualsAnyOrder received (" + j + "):" + CompatExtensions.Render(actual[j]));
                    }
                    ScopeTestHelper.Fail();
                }
            }

            // Must have matched exactly the number of objects times
            ScopeTestHelper.AssertEquals(numMatches, expected.Length);
        }

        /// <summary>Asserts that all values in the given object array are bool-typed values and are true </summary>
        /// <param name="objects">values to assert that they are all true</param>
        public static void AssertAllBooleanTrue(object[] objects)
        {
            foreach (object @object in objects)
            {
                ScopeTestHelper.AssertTrue(@object.AsBoolean());
            }
        }

        /// <summary>Assert the class of the objects in the object array matches the expected classes in the classes array. </summary>
        /// <param name="classes">is the expected class</param>
        /// <param name="objects">is the objects to check the class for</param>
        public static void AssertTypeEqualsAnyOrder(Type[] classes, object[] objects)
        {
            ScopeTestHelper.AssertEquals(classes.Length, objects.Length);
            var resultClasses = new Type[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                resultClasses[i] = objects[i].GetType();
            }
            AssertEqualsAnyOrder(resultClasses, classes);
        }

        /// <summary>Convert an enumerator of event beans to an array of event beans. </summary>
        /// <param name="enumerator">to convert</param>
        /// <returns>array of events</returns>
        public static EventBean[] EnumeratorToArray(IEnumerator<EventBean> enumerator)
        {
            if (enumerator == null)
            {
                ScopeTestHelper.Fail("Null enumerator");
                return null;
            }
            var events = new List<EventBean>();
            for (; enumerator.MoveNext(); )
            {
                events.Add(enumerator.Current);
            }
            return events.ToArray();
        }

        /// <summary>Convert an enumerator of event beans to an array of underlying objects. </summary>
        /// <param name="enumerator">to convert</param>
        /// <returns>array of event underlying objects</returns>
        public static object[] EnumeratorToArrayUnderlying(IEnumerator<EventBean> enumerator)
        {
            var events = new List<object>();
            for (; enumerator.MoveNext(); )
            {
                events.Add(enumerator.Current.Underlying);
            }
            return events.ToArray();
        }

        /// <summary>
        /// Count the number of object provided by an enumerator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator">to count</param>
        /// <returns>
        /// count
        /// </returns>
        public static int EnumeratorCount<T>(IEnumerator<T> enumerator)
        {
            int count = 0;
            for (; enumerator.MoveNext(); )
            {
                count++;
            }
            return count;
        }

        /// <summary>Compare properties of events against a list of maps. </summary>
        /// <param name="received">actual events</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(EventBean[] received, IList<IDictionary<string, object>> expected)
        {
            if ((expected == null) && (received == null))
            {
                return;
            }
            if (expected == null || received == null)
            {
                ScopeTestHelper.Fail();
            }
            else
            {
                ScopeTestHelper.AssertEquals(expected.Count, received.Length);
                for (int i = 0; i < expected.Count; i++)
                {
                    AssertProps(received[i], expected[i]);
                }
            }
        }

        /// <summary>Compare properties of events against a list of maps. </summary>
        /// <param name="enumerator">actual events</param>
        /// <param name="expected">expected values</param>
        public static void AssertPropsPerRow(IEnumerator<EventBean> enumerator, IList<IDictionary<string, object>> expected)
        {
            var values = new List<EventBean>();
            while (enumerator.MoveNext())
            {
                values.Add(enumerator.Current);
            }

            if (enumerator.MoveNext())
            {
                ScopeTestHelper.Fail();
            }

            EventBean[] data = null;
            if (values.Count > 0)
            {
                data = values.ToArray();
            }

            AssertPropsPerRow(data, expected);
        }

        /// <summary>Concatenate two arrays. </summary>
        /// <param name="srcOne">array to concatenate</param>
        /// <param name="srcTwo">array to concatenate</param>
        /// <returns>concatenated array</returns>
        public static object[] ConcatenateArray(object[] srcOne, object[] srcTwo)
        {
            var result = new object[srcOne.Length + srcTwo.Length];
            Array.Copy(srcOne, 0, result, 0, srcOne.Length);
            Array.Copy(srcTwo, 0, result, srcOne.Length, srcTwo.Length);
            return result;
        }

        /// <summary>Concatenate two arrays. </summary>
        /// <param name="first">array to concatenate</param>
        /// <param name="more">array to concatenate</param>
        /// <returns>concatenated array</returns>
        public static object[][] ConcatenateArray2Dim(object[][] first, params object[][][] more)
        {
            int len = first.Length;
            for (int i = 0; i < more.Length; i++)
            {
                object[][] next = more[i];
                len += next.Length;
            }

            var result = new object[len][];
            int count = 0;
            for (int i = 0; i < first.Length; i++)
            {
                result[count] = first[i];
                count++;
            }

            for (int i = 0; i < more.Length; i++)
            {
                object[][] next = more[i];
                for (int j = 0; j < next.Length; j++)
                {
                    result[count] = next[j];
                    count++;
                }
            }

            return result;
        }

        /// <summary>Concatenate multiple arrays. </summary>
        /// <param name="more">arrays to concatenate</param>
        /// <returns>concatenated array</returns>
        public static object[] ConcatenateArray(params object[][] more)
        {
            var list = new List<object>();
            for (int i = 0; i < more.Length; i++)
            {
                for (int j = 0; j < more[i].Length; j++)
                {
                    list.Add(more[i][j]);
                }
            }
            return list.ToArray();
        }

        /// <summary>Sort events according to natural ordering of the values or a property. </summary>
        /// <param name="events">to sort</param>
        /// <param name="property">name of property providing sort values</param>
        /// <returns>sorted array</returns>
        public static EventBean[] Sort(IEnumerator<EventBean> events, string property)
        {
            return Sort(EnumeratorToArray(events), property);
        }

        /// <summary>Sort events according to natural ordering of the values or a property. </summary>
        /// <param name="events">to sort</param>
        /// <param name="property">name of property providing sort values</param>
        /// <returns>sorted array</returns>
        public static EventBean[] Sort(EventBean[] events, string property)
        {
            var list = new List<EventBean>(events);
            var standardComparer = new StandardComparer<EventBean>(
                (o1, o2) =>
                {
                    var val1 = (IComparable)o1.Get(property);
                    var val2 = (IComparable)o2.Get(property);
                    return val1.CompareTo(val2);
                });

            list.Sort(standardComparer);
            return list.ToArray();
        }

        public static void AssertNotContains(ICollection<string> stringSet, params string[] values)
        {
            ICollection<string> set = new HashSet<string>(stringSet);
            foreach (string value in values)
            {
                ScopeTestHelper.AssertFalse(set.Contains(value));
            }
        }

        /// <summary>Assert that a string set does not contain one or more values. </summary>
        /// <param name="stringSet">to compare against</param>
        /// <param name="values">to find</param>
        public static void AssertNotContains(string[] stringSet, params string[] values)
        {
            ICollection<string> set = new HashSet<string>(stringSet);
            foreach (string value in values)
            {
                ScopeTestHelper.AssertFalse(set.Contains(value));
            }
        }

        /// <summary>Assert that a string set does contain each of one or more values. </summary>
        /// <param name="stringSet">to compare against</param>
        /// <param name="values">to find</param>
        public static void AssertContains(ICollection<string> stringSet, params string[] values)
        {
            ICollection<string> set = new HashSet<string>(stringSet);
            foreach (string value in values)
            {
                ScopeTestHelper.AssertTrue(set.Contains(value));
            }
        }

        /// <summary>Assert that a string set does contain each of one or more values. </summary>
        /// <param name="stringSet">to compare against</param>
        /// <param name="values">to find</param>
        public static void AssertContains(string[] stringSet, params string[] values)
        {
            ICollection<string> set = new HashSet<string>(stringSet);
            foreach (string value in values)
            {
                ScopeTestHelper.AssertTrue(set.Contains(value));
            }
        }

        /// <summary>Return an array of underlying objects for an array of events. </summary>
        /// <param name="events">to return underlying objects</param>
        /// <returns>events</returns>
        public static object[] GetUnderlying(EventBean[] events)
        {
            var arr = new object[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                arr[i] = events[i].Underlying;
            }
            return arr;
        }

        /// <summary>Assert that all properties of an event have the same value as passed in. </summary>
        /// <param name="received">to inspect</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">value</param>
        public static void AssertPropsAllValuesSame(EventBean received, string[] propertyNames, object expected)
        {
            foreach (string field in propertyNames)
            {
                ScopeTestHelper.AssertEquals("Field " + field, expected, received.Get(field));
            }
        }

        /// <summary>Extract the property value of the event property for the given events and return an object array of values. </summary>
        /// <param name="events">to extract value from</param>
        /// <param name="propertyName">name of property to extract values for</param>
        /// <returns>value object array</returns>
        public static object[] EventsToObjectArr(EventBean[] events, string propertyName)
        {
            if (events == null)
            {
                return null;
            }
            var objects = new object[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                objects[i] = events[i].Get(propertyName);
            }
            return objects;
        }

        /// <summary>Extract the property value of the event properties for the given events and return an object array of values. </summary>
        /// <param name="events">to extract value from</param>
        /// <param name="propertyNames">names of properties to extract values for</param>
        /// <returns>value object array</returns>
        public static object[][] EventsToObjectArr(EventBean[] events, string[] propertyNames)
        {
            if (events == null)
            {
                return null;
            }

            return events
                .Select(ev => propertyNames.Select(ev.Get).ToArray())
                .ToArray();
        }

        /// <summary>Extract the property value of the event property for the given events and return an object array of values. </summary>
        /// <param name="enumerator">events to extract value from</param>
        /// <param name="propertyName">name of property to extract values for</param>
        /// <returns>value object array</returns>
        public static object[] EnumeratorToObjectArr(IEnumerator<EventBean> enumerator, string propertyName)
        {
            if (enumerator == null)
            {
                return null;
            }
            return EventsToObjectArr(EnumeratorToArray(enumerator), propertyName);
        }

        /// <summary>Extract the property value of the event properties for the given events and return an object array of values. </summary>
        /// <param name="enumerator">events to extract value from</param>
        /// <param name="propertyNames">names of properties to extract values for</param>
        /// <returns>value object array</returns>
        public static object[][] EnumeratorToObjectArr(IEnumerator<EventBean> enumerator, string[] propertyNames)
        {
            if (enumerator == null)
            {
                return null;
            }
            return EventsToObjectArr(EnumeratorToArray(enumerator), propertyNames);
        }

        /// <summary>Compare the events in the two object arrays assuming the exact same order. </summary>
        /// <param name="actual">is the actual results</param>
        /// <param name="expected">is the expected values</param>
        /// <returns>indicate whether compared successfully</returns>
        public static bool CompareEqualsExactOrder(EventBean[] actual, EventBean[] expected)
        {
            if ((expected == null) && (actual == null))
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                return false;
            }

            if (expected.Length != actual.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if ((actual[i] == null) && (expected[i] == null))
                {
                    continue;
                }

                if (!actual[i].Equals(expected[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Reference-compare the objects in the two object arrays assuming the exact same order. </summary>
        /// <param name="actual">is the actual results</param>
        /// <param name="expected">is the expected values</param>
        /// <returns>indicate whether compared successfully</returns>
        public static bool CompareRefExactOrder(object[] actual, object[] expected)
        {
            if ((expected == null) && (actual == null))
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                return false;
            }

            if (expected.Length != actual.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Assert that property values of rows, wherein each row can either be Map or PONO objects, matches the expected values.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="received">array of objects may contain Map and PONO events</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected value</param>
        public static void AssertPropsPerRow(
            IContainer container, 
            object[] received, 
            string[] propertyNames, 
            object[][] expected)
        {
            ScopeTestHelper.AssertEquals(received.Length, expected.Length);
            for (int row = 0; row < received.Length; row++)
            {
                AssertProps(container, received[row], propertyNames, expected[row]);
            }
        }

        /// <summary>
        /// Assert that property values, wherein the row can either be a Map or a PONO object, matches the expected values.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="received">Map or PONO</param>
        /// <param name="propertyNames">property names</param>
        /// <param name="expected">expected value</param>
        public static void AssertProps(
            IContainer container,
            object received, 
            string[] propertyNames, 
            object[] expected)
        {
            if (received is IDictionary<string, object>)
            {
                AssertPropsMap((IDictionary<string, object>)received, propertyNames, expected);
            }
            else if (received is object[])
            {
                AssertPropsObjectArray((object[]) received, propertyNames, expected);
            }
            else if (received is EventBean)
            {
                AssertProps((EventBean)received, propertyNames, expected);
            }
            else
            {
                AssertPropsPono(container, received, propertyNames, expected);
            }
        }

        /// <summary>For a given array, copy the array elements into a new array of Object[] type. </summary>
        /// <param name="array">input array</param>
        /// <returns>object array</returns>
        public static object[] ToObjectArray(object array)
        {
            var asArray = array as Array;
            if (asArray == null)
            {
                throw new ArgumentException("Object not an array but type '" + (array == null ? "null" : array.GetType().FullName) + "'");
            }
            var size = asArray.Length;
            var val = new object[size];
            for (int i = 0; i < size; i++)
            {
                val[i] = asArray.GetValue(i);
            }
            return val;
        }

        /// <summary>Assert that two property values are the same, allowing arrays as properties. </summary>
        /// <param name="message">to use</param>
        /// <param name="expected">expected value</param>
        /// <param name="actual">actual value</param>
        public static void AssertEqualsAllowArray(string message, object expected, object actual)
        {
            if ((expected != null) && (expected.GetType().IsArray) && (actual != null) && (actual.GetType().IsArray))
            {
                object[] valueArray = ToObjectArray(expected);
                object[] eventPropArray = ToObjectArray(actual);
                AssertEqualsExactOrder(valueArray, eventPropArray);
                return;
            }
            ScopeTestHelper.AssertEquals(message, expected, actual);
        }

        /// <summary>Assert that the event properties of the event match the properties provided by the map, taking the map properties as the comparison source. </summary>
        /// <param name="received">event</param>
        /// <param name="expected">expected values</param>
        public static void AssertProps(EventBean received, IDictionary<string, object> expected)
        {
            foreach (KeyValuePair<string, object> entry in expected)
            {
                object valueExpected = entry.Value;
                object property = received.Get(entry.Key);

                ScopeTestHelper.AssertEquals(valueExpected, property);
            }
        }

        private static bool CompareArrayAndCollSize(object expected, ICollection<object> actual)
        {
            if (expected == null && (actual == null || actual.Count == 0))
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                if (expected == null)
                {
                    ScopeTestHelper.AssertNull("Expected is null but actual is not null", actual);
                }
                ScopeTestHelper.AssertNull("Actual is null but expected is not null", expected);
            }
            else
            {
                var expectedArray = expected as Array;
                int expectedLength = expectedArray.Length;
                int actualLength = actual.Count;
                ScopeTestHelper.AssertEquals("Mismatch in the number of expected and actual length", expectedLength, actualLength);
            }
            return false;
        }

        private static bool CompareCount<T>(ICollection<T> expected, ICollection<T> actual)
        {
            if ((expected == null) && (actual == null))
                return false;
            if ((expected == null))
                ScopeTestHelper.AssertNull("Expected is null but actual is not null", actual);
            if ((actual == null))
                ScopeTestHelper.AssertNull("Actual is null but expected is not null", expected);

            int expectedLength = expected.Count;
            int actualLength = actual.Count;
            ScopeTestHelper.AssertEquals("Mismatch in the number of expected and actual length", expectedLength, actualLength);

            return false;
        }

        private static bool CompareArraySize(object expected, object actual)
        {
            var actualArray = actual as Array;
            var expectedArray = expected as Array;

            if ((expectedArray == null) && (actualArray == null || actualArray.Length == 0))
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                if (expected == null)
                {
                    ScopeTestHelper.AssertNull("Expected is null but actual is not null", actual);
                }
                ScopeTestHelper.AssertNull("Actual is null but expected is not null", expected);
            }
            else
            {
                int expectedLength = expectedArray.Length;
                int actualLength = actualArray.Length;
                ScopeTestHelper.AssertEquals("Mismatch in the number of expected and actual number of values asserted", expectedLength, actualLength);
            }
            return false;
        }


        /// <summary>Compare two strings removing all NewLine characters. </summary>
        /// <param name="expected">expected value</param>
        /// <param name="received">received value</param>
        public static void AssertEqualsIgnoreNewline(string expected, string received)
        {
            string expectedClean = RemoveNewline(expected);
            string receivedClean = RemoveNewline(received);
            if (expectedClean != receivedClean)
            {
                Log.Error("Expected: " + expectedClean);
                Log.Error("Received: " + receivedClean);
                ScopeTestHelper.AssertEquals("Mismatch ", expected, received);
            }
        }

        /// <summary>Assert that a map of collections (IDictionary&lt;String, ICollection&gt;) has expected keys and values. </summary>
        /// <param name="map">of string keys and collection-type values</param>
        /// <param name="keys">array of key values</param>
        /// <param name="expectedList">for each key a string that is a comma-separated list of values</param>
        /// <param name="collectionValue">the function to apply to each collection value to convert to a string</param>
        public static void AssertMapOfCollection<V>(
            IDictionary<object,V> map, 
            string[] keys, 
            string[] expectedList, 
            AssertionCollectionValueString collectionValue)
        {
            ScopeTestHelper.AssertEquals(expectedList.Length, keys.Length);
            if (keys.Length == 0 && map.IsEmpty())
            {
                return;
            }

            ScopeTestHelper.AssertEquals(map.Count, keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                var value = (ICollection) map.Get(keys[i]);
                string[] itemsExpected = expectedList[i].Split(',');
                ScopeTestHelper.AssertEquals(itemsExpected.Length, value.Count);

                IEnumerator it = value.GetEnumerator();
                for (int j = 0; j < itemsExpected.Length; j++)
                {
                    ScopeTestHelper.AssertTrue(it.MoveNext());
                    string received = collectionValue.ExtractValue(it.Current);
                    ScopeTestHelper.AssertEquals(itemsExpected[j], received);
                }
            }
        }

        public static void AssertMapOfCollection<V>(
            IDictionary<string, V> map,
            string[] keys,
            string[] expectedList,
            AssertionCollectionValueString collectionValue) {

            AssertMapOfCollection(
                map.TransformDown<string, object, V>(),
                keys,
                expectedList,
                collectionValue
            );
        }

        private static string RemoveNewline(string raw)
        {
            raw = raw.Replace("\t", "");
            raw = raw.Replace("\n", "");
            raw = raw.Replace("\r", "");
            return raw;
        }

        /// <summary>Callback for extracting individual collection items for assertion. </summary>
        public interface AssertionCollectionValueString
        {
            /// <summary>Extract value. </summary>
            /// <param name="collectionItem">to extract from</param>
            /// <returns>extracted value</returns>
            string ExtractValue(object collectionItem);
        }

        public class ProxyAssertionCollectionValueString : AssertionCollectionValueString
        {
            public Func<object, string> ProcExtractValue { get; set; }

            public ProxyAssertionCollectionValueString(Func<object, string> procExtractValue)
            {
                ProcExtractValue = procExtractValue;
            }

            public ProxyAssertionCollectionValueString()
            {
            }

            public string ExtractValue(object collectionItem)
            {
                return ProcExtractValue.Invoke(collectionItem);
            }
        }
    }
}
