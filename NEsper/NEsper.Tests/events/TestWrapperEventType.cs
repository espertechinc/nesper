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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events
{
    [TestFixture]
    public class TestWrapperEventType
    {
        private EventType _underlyingEventTypeOne;
        private EventType _underlyingEventTypeTwo;
        private EventTypeSPI _eventType;
        private IDictionary<String, Object> _properties;
        private EventAdapterService _eventAdapterService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _underlyingEventTypeOne = new BeanEventType(_container, null, 1, typeof(SupportBeanSimple), _container.Resolve<EventAdapterService>(), null);
            _underlyingEventTypeTwo = new BeanEventType(_container, null, 1, typeof(SupportBean_A), _container.Resolve<EventAdapterService>(), null);
            _properties = new Dictionary<String, Object>();
            _properties["additionalString"] = typeof(string);
            _properties["AdditionalInt"] = typeof(int);
            _eventAdapterService = _container.Resolve<EventAdapterService>();
            EventTypeMetadata meta = EventTypeMetadata.CreateWrapper("test", true, false, false);
            _eventType = new WrapperEventType(meta, "mytype", 1, _underlyingEventTypeOne, _properties, _eventAdapterService);
        }

        [Test]
        public void TestTypeUpdate()
        {
            IDictionary<String, Object> typeOne = new Dictionary<String, Object>();
            typeOne["field1"] = typeof(string);
            MapEventType underlying = new MapEventType(EventTypeMetadata.CreateAnonymous("noname", ApplicationType.MAP), "noname", 1, _eventAdapterService, typeOne, null, null, null);
            EventTypeMetadata meta = EventTypeMetadata.CreateWrapper("test", true, false, false);
            _eventType = new WrapperEventType(meta, "mytype", 1, underlying, _properties, _eventAdapterService);

            EPAssertionUtil.AssertEqualsAnyOrder(new[] { "additionalString", "AdditionalInt", "field1" }, _eventType.PropertyNames);
            underlying.AddAdditionalProperties(Collections.SingletonDataMap("field2", typeof(string)), _eventAdapterService);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { "additionalString", "AdditionalInt", "field1", "field2" }, _eventType.PropertyNames);
            Assert.AreEqual(4, _eventType.PropertyDescriptors.Count);
            Assert.AreEqual(typeof(string), _eventType.GetPropertyDescriptor("field2").PropertyType);
        }

        [Test]
        public void TestInvalidRepeatedNames()
        {
            _properties.Clear();
            _properties["MyString"] = typeof(string);

            try
            {
                // The MyString property occurs in both the event and the map
                _eventType = new WrapperEventType(null, "mytype", 1, _underlyingEventTypeOne, _properties, _eventAdapterService);
                Assert.Fail();
            }
            catch (EPException)
            {
                // Expected
            }
        }

        [Test]
        public void TestGetPropertyNames()
        {
            String[] expected = new[] { "MyInt", "MyString", "AdditionalInt", "additionalString" };
            EPAssertionUtil.AssertEqualsAnyOrder(expected, _eventType.PropertyNames);
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(int), _eventType.GetPropertyType("MyInt"));
            Assert.AreEqual(typeof(int), _eventType.GetPropertyType("AdditionalInt"));
            Assert.AreEqual(typeof(string), _eventType.GetPropertyType("additionalString"));
            Assert.AreEqual(typeof(string), _eventType.GetPropertyType("MyString"));
            Assert.IsNull(_eventType.GetPropertyType("unknownProperty"));
        }

        [Test]
        public void TestIsProperty()
        {
            Assert.IsTrue(_eventType.IsProperty("MyInt"));
            Assert.IsTrue(_eventType.IsProperty("AdditionalInt"));
            Assert.IsTrue(_eventType.IsProperty("additionalString"));
            Assert.IsTrue(_eventType.IsProperty("MyString"));
            Assert.IsFalse(_eventType.IsProperty("unknownProperty"));
        }

        [Test]
        public void TestEquals()
        {
            IDictionary<String, Object> otherProperties = new Dictionary<String, Object>(_properties);
            EventTypeMetadata meta = EventTypeMetadata.CreateWrapper("test", true, false, false);
            EventTypeSPI otherType = new WrapperEventType(meta, "mytype", 1, _underlyingEventTypeOne, otherProperties, _eventAdapterService);
            Assert.IsTrue(_eventType.EqualsCompareType(otherType));
            Assert.IsTrue(otherType.EqualsCompareType(_eventType));

            otherType = new WrapperEventType(meta, "mytype", 1, _underlyingEventTypeTwo, otherProperties, _eventAdapterService);
            Assert.IsFalse(_eventType.EqualsCompareType(otherType));
            Assert.IsFalse(otherType.EqualsCompareType(_eventType));

            otherProperties["anotherProperty"] = typeof(int);
            otherType = new WrapperEventType(meta, "mytype", 1, _underlyingEventTypeOne, otherProperties, _eventAdapterService);
            Assert.IsFalse(_eventType.EqualsCompareType(otherType));
            Assert.IsFalse(otherType.EqualsCompareType(_eventType));

            otherType = (EventTypeSPI)_underlyingEventTypeOne;
            Assert.IsFalse(_eventType.EqualsCompareType(otherType));
            Assert.IsFalse(otherType.EqualsCompareType(_eventType));
        }
    }
}