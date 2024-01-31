///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestNestedProperty : AbstractCommonTest
    {
        private NestedProperty[] nested;
        private EventBean theEvent;

        [SetUp]
        public void SetUp()
        {
            nested = new NestedProperty[2];
            nested[0] = MakeProperty(new string[] { "Nested", "NestedValue" });
            nested[1] = MakeProperty(new string[] { "Nested", "NestedNested", "NestedNestedValue" });

            theEvent = SupportEventBeanFactory.CreateObject(
                supportEventTypeFactory, SupportBeanComplexProps.MakeDefaultBean());
        }

        [Test]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = nested[0].GetGetter((BeanEventType) theEvent.EventType,
                EventBeanTypedEventFactoryCompileTime.INSTANCE,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            ClassicAssert.AreEqual("NestedValue", getter.Get(theEvent));

            getter = nested[1].GetGetter((BeanEventType) theEvent.EventType, EventBeanTypedEventFactoryCompileTime.INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            ClassicAssert.AreEqual("NestedNestedValue", getter.Get(theEvent));
        }

        [Test]
        public void TestGetPropertyType()
        {
            ClassicAssert.AreEqual(typeof(string), nested[0].GetPropertyType((BeanEventType) theEvent.EventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            ClassicAssert.AreEqual(typeof(string), nested[1].GetPropertyType((BeanEventType) theEvent.EventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }

        private NestedProperty MakeProperty(string[] propertyNames)
        {
            IList<Property> properties = new List<Property>();
            foreach (string prop in propertyNames)
            {
                properties.Add(new SimpleProperty(prop));
            }
            return new NestedProperty(properties);
        }
    }
} // end of namespace
