///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.events.property
{
    [TestFixture]
    public class TestMappedProperty 
    {
        private MappedProperty[] _mapped;
        private EventBean _theEvent;
        private BeanEventType _eventType;
    
        [SetUp]
        public void SetUp()
        {
            _mapped = new MappedProperty[2];
            _mapped[0] = new MappedProperty("Mapped", "keyOne");
            _mapped[1] = new MappedProperty("Mapped", "keyTwo");
    
            _theEvent = SupportEventBeanFactory.CreateObject(SupportBeanComplexProps.MakeDefaultBean());
            _eventType = (BeanEventType)_theEvent.EventType;
        }
    
        [Test]
        public void TestGetGetter()
        {
            Object[] expected = new String[] {"valueOne", "valueTwo"};
            for (int i = 0; i < _mapped.Length; i++)
            {
                EventPropertyGetter getter = _mapped[i].GetGetter(_eventType, SupportEventAdapterService.Service);
                Assert.AreEqual(expected[i], getter.Get(_theEvent));
            }
    
            // try invalid case
            MappedProperty mpd = new MappedProperty("Dummy", "dummy");
            Assert.IsNull(mpd.GetGetter(_eventType, SupportEventAdapterService.Service));
        }
    
        [Test]
        public void TestGetPropertyType()
        {
            var expected = new Type[] {typeof(string), typeof(string)};
            for (int i = 0; i < _mapped.Length; i++)
            {
                Assert.AreEqual(expected[i], _mapped[i].GetPropertyType(_eventType, SupportEventAdapterService.Service));
            }
    
            // try invalid case
            var mpd = new MappedProperty("Dummy", "dummy");
            Assert.IsNull(mpd.GetPropertyType(_eventType, SupportEventAdapterService.Service));
            mpd = new MappedProperty("MapProperty", "dummy");
            Assert.AreEqual(typeof(string), mpd.GetPropertyType(_eventType, SupportEventAdapterService.Service));
        }
    }
}
