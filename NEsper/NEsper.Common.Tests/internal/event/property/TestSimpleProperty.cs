///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;
using NUnit.Framework;

using static com.espertech.esper.common.@internal.@event.core.EventBeanTypedEventFactoryCompileTime;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestSimpleProperty : AbstractCommonTest
    {
        private SimpleProperty prop;
        private SimpleProperty invalidPropMap;
        private SimpleProperty invalidPropIndexed;
        private SimpleProperty invalidDummy;
        private EventBean theEvent;
        private BeanEventType eventType;

        [SetUp]
        public void SetUp()
        {
            prop = new SimpleProperty("SimpleProperty");
            invalidPropMap = new SimpleProperty("Mapped");
            invalidPropIndexed = new SimpleProperty("Indexed");
            invalidDummy = new SimpleProperty("dummy");
            theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, SupportBeanComplexProps.MakeDefaultBean());
            eventType = (BeanEventType) theEvent.EventType;
        }

        [Test, RunInApplicationDomain]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = prop.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            Assert.AreEqual("simple", getter.Get(theEvent));

            Assert.IsNull(invalidDummy.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            Assert.IsNull(invalidPropMap.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            Assert.IsNull(invalidPropIndexed.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }

        [Test, RunInApplicationDomain]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string), prop.GetPropertyType(eventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));

            Assert.IsNull(invalidDummy.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            Assert.IsNull(invalidPropMap.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            Assert.IsNull(invalidPropIndexed.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }
    }
} // end of namespace
