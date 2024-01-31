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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestMappedProperty : AbstractCommonTest
    {
        private MappedProperty[] mapped;
        private EventBean theEvent;
        private BeanEventType eventType;

        [SetUp]
        public void SetUp()
        {
            mapped = new MappedProperty[2];
            mapped[0] = new MappedProperty("Mapped", "keyOne");
            mapped[1] = new MappedProperty("Mapped", "keyTwo");

            theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory,
                SupportBeanComplexProps.MakeDefaultBean());
            eventType = (BeanEventType) theEvent.EventType;
        }

        [Test]
        public void TestGetGetter()
        {
            object[] expected = new string[] { "valueOne", "valueTwo" };
            for (int i = 0; i < mapped.Length; i++)
            {
                EventPropertyGetter getter = mapped[i].GetGetter(eventType, EventBeanTypedEventFactoryCompileTime.INSTANCE,
                    supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
                ClassicAssert.AreEqual(expected[i], getter.Get(theEvent));
            }

            // try invalid case
            MappedProperty mpd = new MappedProperty("dummy", "dummy");
            ClassicAssert.IsNull(mpd.GetGetter(eventType, EventBeanTypedEventFactoryCompileTime.INSTANCE,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }

        [Test]
        public void TestGetPropertyType()
        {
            for (int i = 0; i < mapped.Length; i++)
            {
                ClassicAssert.AreEqual(typeof(string), mapped[i].GetPropertyType(eventType,
                    supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            }

            // try invalid case
            MappedProperty mpd = new MappedProperty("dummy", "dummy");
            ClassicAssert.IsNull(mpd.GetPropertyType(eventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
            mpd = new MappedProperty("MapProperty", "dummy");
            ClassicAssert.AreEqual(typeof(string), mpd.GetPropertyType(eventType, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY));
        }
    }
} // end of namespace
