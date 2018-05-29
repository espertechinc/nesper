///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.property
{
    [TestFixture]
    public class TestNestedProperty
    {
        private NestedProperty[] _nested;
        private EventBean _event;
        private BeanEventTypeFactory _beanEventTypeFactory;

        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _beanEventTypeFactory = new BeanEventAdapter(
                _container,
                new ConcurrentDictionary<Type, BeanEventType>(),
                _container.Resolve<EventAdapterService>(),
                new EventTypeIdGeneratorImpl());

            _nested = new NestedProperty[2];
            _nested[0] = MakeProperty(new[] {"Nested", "NestedValue"});
            _nested[1] = MakeProperty(new[] {"Nested", "NestedNested", "NestedNestedValue"});

            _event = SupportEventBeanFactory.CreateObject(SupportBeanComplexProps.MakeDefaultBean());
        }

        private static NestedProperty MakeProperty(IEnumerable<string> propertyNames)
        {
            var properties = propertyNames.Select(prop => new SimpleProperty(prop)).Cast<Property>().ToList();
            return new NestedProperty(properties);
        }

        [Test]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = _nested[0].GetGetter((BeanEventType) _event.EventType, _container.Resolve<EventAdapterService>());
            Assert.AreEqual("NestedValue", getter.Get(_event));

            getter = _nested[1].GetGetter((BeanEventType) _event.EventType, _container.Resolve<EventAdapterService>());
            Assert.AreEqual("NestedNestedValue", getter.Get(_event));
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string),
                            _nested[0].GetPropertyType((BeanEventType) _event.EventType,
                                                      _container.Resolve<EventAdapterService>()));
            Assert.AreEqual(typeof(string),
                            _nested[1].GetPropertyType((BeanEventType) _event.EventType,
                                                      _container.Resolve<EventAdapterService>()));
        }
    }
}