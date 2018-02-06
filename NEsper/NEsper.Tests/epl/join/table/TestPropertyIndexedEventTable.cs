///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.table
{
    [TestFixture]
    public class TestPropertyIndexedEventTable
    {
        private String[] _propertyNames;
        private EventType _eventType;
        private EventBean[] _testEvents;
        private Object[] _testEventsUnd;
        private PropertyIndexedEventTable _index;

        [SetUp]
        public void SetUp()
        {
            _propertyNames = new[] {"IntPrimitive", "TheString"};
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var factory = new PropertyIndexedEventTableFactory(1, _eventType, _propertyNames, false, null);
            _index = (PropertyIndexedEventTable) factory.MakeEventTables(null, null)[0];

            // Populate with testEvents
            var intValues = new[] {0, 1, 1, 2, 1, 0};
            var stringValues = new[] {"a", "b", "c", "a", "b", "c"};

            _testEvents = new EventBean[intValues.Length];
            _testEventsUnd = new Object[intValues.Length];
            for (int i = 0; i < intValues.Length; i++)
            {
                _testEvents[i] = MakeBean(intValues[i], stringValues[i]);
                _testEventsUnd[i] = _testEvents[i].Underlying;
            }
            _index.Add(_testEvents, null);
        }

        private EventBean MakeBean(int intValue, String stringValue)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intValue;
            bean.TheString = stringValue;
            return SupportEventBeanFactory.CreateObject(bean);
        }

        [Test]
        public void TestAdd()
        {
            // Add event without these properties should fail
            EventBean theEvent = SupportEventBeanFactory.CreateObject(new SupportBean_A("d"));
            try
            {
                _index.Add(new[] {theEvent}, null);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // Expected
            }

            // Add null should fail
            try
            {
                _index.Add(new EventBean[] {null}, null);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // Expected
            }
        }

        [Test]
        public void TestAddArray()
        {
            var factory = new PropertyIndexedEventTableFactory(1, _eventType, _propertyNames, false, null);
            _index = (PropertyIndexedEventTable) factory.MakeEventTables(null, null)[0];

            // Add just 2
            var events = new EventBean[2];
            events[0] = _testEvents[1];
            events[1] = _testEvents[4];
            _index.Add(events, null);

            ICollection<EventBean> result = _index.Lookup(new Object[] {1, "b"});
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void TestEnumerator()
        {
            Object[] underlying = ArrayAssertionUtil.EnumeratorToArrayUnderlying(_index.GetEnumerator());
            EPAssertionUtil.AssertEqualsAnyOrder(_testEventsUnd, underlying);
        }

        [Test]
        public void TestFind()
        {
            ICollection<EventBean> result = _index.Lookup(new Object[] {1, "a"});
            Assert.IsNull(result);

            result = _index.Lookup(new Object[] {1, "b"});
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(_testEvents[1]));
            Assert.IsTrue(result.Contains(_testEvents[4]));

            result = _index.Lookup(new Object[] {0, "c"});
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(_testEvents[5]));

            result = _index.Lookup(new Object[] {0, "a"});
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(_testEvents[0]));
        }

        [Test]
        public void TestMixed()
        {
            _index.Remove(new[] {_testEvents[1]}, null);
            ICollection<EventBean> result = _index.Lookup(new Object[] {1, "b"});
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(_testEvents[4]));

            // iterate
            Object[] underlying = ArrayAssertionUtil.EnumeratorToArrayUnderlying(_index.GetEnumerator());
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {_testEventsUnd[0], _testEventsUnd[2], _testEventsUnd[3], _testEventsUnd[4], _testEventsUnd[5]},
                underlying);

            _index.Remove(new[] {_testEvents[4]}, null);
            result = _index.Lookup(new Object[] {1, "b"});
            Assert.IsNull(result);

            // iterate
            underlying = ArrayAssertionUtil.EnumeratorToArrayUnderlying(_index.GetEnumerator());
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {_testEventsUnd[0], _testEventsUnd[2], _testEventsUnd[3], _testEventsUnd[5]}, underlying);

            _index.Add(new[] {_testEvents[1]}, null);
            result = _index.Lookup(new Object[] {1, "b"});
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(_testEvents[1]));

            // iterate
            underlying = ArrayAssertionUtil.EnumeratorToArrayUnderlying(_index.GetEnumerator());
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {_testEventsUnd[0], _testEventsUnd[1], _testEventsUnd[2], _testEventsUnd[3], _testEventsUnd[5]},
                underlying);
        }

        [Test]
        public void TestRemove()
        {
            _index.Remove(_testEvents, null);
        }

        [Test]
        public void TestRemoveArray()
        {
            _index.Remove(_testEvents, null);

            ICollection<EventBean> result = _index.Lookup(new Object[] {1, "b"});
            Assert.IsNull(result);

            // Remove again - already removed but won't throw an exception
            _index.Remove(_testEvents, null);
        }
    }
}