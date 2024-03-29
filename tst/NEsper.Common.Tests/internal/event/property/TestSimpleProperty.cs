///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;
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

        [Test]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = prop.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            ClassicAssert.AreEqual("simple", getter.Get(theEvent));

            ClassicAssert.IsNull(invalidDummy.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            ClassicAssert.IsNull(invalidPropMap.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            ClassicAssert.IsNull(invalidPropIndexed.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }

        [Test]
        public void TestGetPropertyType()
        {
            ClassicAssert.AreEqual(typeof(string), prop.GetPropertyType(eventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));

            ClassicAssert.IsNull(invalidDummy.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            ClassicAssert.IsNull(invalidPropMap.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            ClassicAssert.IsNull(invalidPropIndexed.GetGetter(eventType, INSTANCE, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }
    }
} // end of namespace
