///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.events.bean;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;
using NUnit.Framework;

namespace com.espertech.esper.events.property
{
    [TestFixture]
    public class TestNestedProperty
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _beanEventTypeFactory = new BeanEventAdapter(new ConcurrentDictionary<Type, BeanEventType>(),
                                                        SupportEventAdapterService.Service,
                                                        new EventTypeIdGeneratorImpl());

            _nested = new NestedProperty[2];
            _nested[0] = MakeProperty(new[] {"Nested", "NestedValue"});
            _nested[1] = MakeProperty(new[] {"Nested", "NestedNested", "NestedNestedValue"});

            _event = SupportEventBeanFactory.CreateObject(SupportBeanComplexProps.MakeDefaultBean());
        }

        #endregion

        private NestedProperty[] _nested;
        private EventBean _event;
        private BeanEventTypeFactory _beanEventTypeFactory;

        private static NestedProperty MakeProperty(IEnumerable<string> propertyNames)
        {
            var properties = propertyNames.Select(prop => new SimpleProperty(prop)).Cast<Property>().ToList();
            return new NestedProperty(properties);
        }

        [Test]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = _nested[0].GetGetter((BeanEventType) _event.EventType, SupportEventAdapterService.Service);
            Assert.AreEqual("NestedValue", getter.Get(_event));

            getter = _nested[1].GetGetter((BeanEventType) _event.EventType, SupportEventAdapterService.Service);
            Assert.AreEqual("NestedNestedValue", getter.Get(_event));
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string),
                            _nested[0].GetPropertyType((BeanEventType) _event.EventType,
                                                      SupportEventAdapterService.Service));
            Assert.AreEqual(typeof(string),
                            _nested[1].GetPropertyType((BeanEventType) _event.EventType,
                                                      SupportEventAdapterService.Service));
        }
    }
}