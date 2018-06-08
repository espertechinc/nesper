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
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestBeanEventAdapter
    {
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
        }

        [Test]
        public void TestCreateBeanType()
        {
            BeanEventType eventType = _beanEventTypeFactory.CreateBeanType("a", typeof(SupportBeanSimple), true, true, true);

            Assert.AreEqual(typeof(SupportBeanSimple), eventType.UnderlyingType);
            Assert.AreEqual(2, eventType.PropertyNames.Length);

            // Second call to create the event type, should be the same instance as the first
            EventType eventTypeTwo = _beanEventTypeFactory.CreateBeanType("b", typeof(SupportBeanSimple), true, true, true);
            Assert.IsTrue(eventTypeTwo == eventType);

            // Third call to create the event type, getting a given event type id
            EventType eventTypeThree = _beanEventTypeFactory.CreateBeanType("c", typeof(SupportBeanSimple), true, true, true);
            Assert.IsTrue(eventTypeThree == eventType);
        }

        [Test]
        public void TestInterfaceProperty()
        {
            // Assert implementations have full set of properties
            ISupportDImpl theEvent = new ISupportDImpl("D", "BaseD", "BaseDBase");
            EventType typeBean = _beanEventTypeFactory.CreateBeanType(theEvent.GetType().FullName, theEvent.GetType(), true, true, true);
            EventBean bean = new BeanEventBean(theEvent, typeBean);
            Assert.AreEqual("D", bean.Get("D"));
            Assert.AreEqual("BaseD", bean.Get("BaseD"));
            Assert.AreEqual("BaseDBase", bean.Get("BaseDBase"));
            Assert.AreEqual(3, bean.EventType.PropertyNames.Length);
            EPAssertionUtil.AssertEqualsAnyOrder(bean.EventType.PropertyNames,
                    new[] { "D", "BaseD", "BaseDBase" });

            // Assert intermediate interfaces have full set of fields
            EventType interfaceType = _beanEventTypeFactory.CreateBeanType("d", typeof(ISupportD), true, true, true);
            EPAssertionUtil.AssertEqualsAnyOrder(interfaceType.PropertyNames,
                    new[] { "D", "BaseD", "BaseDBase" });
        }

        [Test]
        public void TestMappedIndexedNestedProperty()
        {
            EventType eventType = _beanEventTypeFactory.CreateBeanType("e", typeof(SupportBeanComplexProps), true, true, true);

            Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("MapProperty"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("Mapped('x')"));
            Assert.AreEqual(typeof(int), eventType.GetPropertyType("Indexed[1]"));
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), eventType.GetPropertyType("Nested"));
            Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("ArrayProperty"));
        }
    }
}
