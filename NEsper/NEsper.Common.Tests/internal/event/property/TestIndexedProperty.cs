///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestIndexedProperty : AbstractTestBase
    {
        private IndexedProperty[] indexed;
        private EventBean theEvent;
        private BeanEventType eventType;

        [SetUp]
        public void SetUp()
        {
            indexed = new IndexedProperty[4];
            indexed[0] = new IndexedProperty("indexed", 0);
            indexed[1] = new IndexedProperty("indexed", 1);
            indexed[2] = new IndexedProperty("arrayProperty", 0);
            indexed[3] = new IndexedProperty("arrayProperty", 1);

            theEvent = SupportEventBeanFactory.CreateObject(
                supportEventTypeFactory, 
                SupportBeanComplexProps.MakeDefaultBean());
            eventType = (BeanEventType) theEvent.EventType;
        }

        [Test]
        public void TestGetGetter()
        {
            int[] expected = new int[] { 1, 2, 10, 20 };
            for (int i = 0; i < indexed.Length; i++)
            {
                EventPropertyGetter getter = indexed[i].GetGetter(eventType, EventBeanTypedEventFactoryCompileTime.INSTANCE,
                    supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
                Assert.AreEqual(expected[i], getter.Get(theEvent));
            }

            // try invalid case
            IndexedProperty ind = new IndexedProperty("dummy", 0);
            Assert.IsNull(ind.GetGetter(eventType, EventBeanTypedEventFactoryCompileTime.INSTANCE,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }

        [Test]
        public void TestGetPropertyType()
        {
            Type[] expected = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };
            for (int i = 0; i < indexed.Length; i++)
            {
                Assert.AreEqual(expected[i], indexed[i].GetPropertyType(eventType,
                    supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            }

            // try invalid case
            IndexedProperty ind = new IndexedProperty("dummy", 0);
            Assert.IsNull(ind.GetPropertyType(eventType,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }
    }
} // end of namespace