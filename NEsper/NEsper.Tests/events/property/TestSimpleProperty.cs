///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.events.property
{
    [TestFixture]
    public class TestSimpleProperty 
    {
        private SimpleProperty _prop;
        private SimpleProperty _invalidPropMap;
        private SimpleProperty _invalidPropIndexed;
        private SimpleProperty _invalidDummy;
        private EventBean _theEvent;
        private BeanEventType _eventType;
    
        [SetUp]
        public void SetUp()
        {
            _prop = new SimpleProperty("SimpleProperty");
            _invalidPropMap = new SimpleProperty("Mapped");
            _invalidPropIndexed = new SimpleProperty("Indexed");
            _invalidDummy = new SimpleProperty("Dummy");
            _theEvent = SupportEventBeanFactory.CreateObject(SupportBeanComplexProps.MakeDefaultBean());
            _eventType = (BeanEventType)_theEvent.EventType;
        }
    
        [Test]
        public void TestGetGetter()
        {
            EventPropertyGetter getter = _prop.GetGetter(_eventType, SupportEventAdapterService.Service);
            Assert.AreEqual("Simple", getter.Get(_theEvent));
    
            Assert.IsNull(_invalidDummy.GetGetter(_eventType, SupportEventAdapterService.Service));
            Assert.IsNull(_invalidPropMap.GetGetter(_eventType, SupportEventAdapterService.Service));
            Assert.IsNull(_invalidPropIndexed.GetGetter(_eventType, SupportEventAdapterService.Service));
        }
    
        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string), _prop.GetPropertyType(_eventType, SupportEventAdapterService.Service));
    
            Assert.IsNull(_invalidDummy.GetGetter(_eventType, SupportEventAdapterService.Service));
            Assert.IsNull(_invalidPropMap.GetGetter(_eventType, SupportEventAdapterService.Service));
            Assert.IsNull(_invalidPropIndexed.GetGetter(_eventType, SupportEventAdapterService.Service));
        }
    }
}
