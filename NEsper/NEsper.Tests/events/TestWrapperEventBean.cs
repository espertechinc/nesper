///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events
{
    [TestFixture]
    public class TestWrapperEventBean  
    {
    	private EventBean _eventBeanSimple;
    	private EventBean _eventBeanCombined;
    	private IDictionary<String, Object> _properties;
    	private EventType _eventTypeSimple;
    	private EventType _eventTypeCombined;
    	private EventAdapterService _eventService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _eventService = _container.Resolve<EventAdapterService>();
    		EventType underlyingEventTypeSimple = _eventService.AddBeanType("UnderlyingSimpleBean", typeof(SupportBeanSimple), true, true, true);
    		EventType underlyingEventTypeCombined = _eventService.AddBeanType("UnderlyingCombinedBean", typeof(SupportBeanCombinedProps), true, true, true);
    		
    		IDictionary<String, Object> typeMap = new Dictionary<String, Object>();
    		typeMap["string"] = typeof(string);
    		typeMap["int"] = typeof(int);
    
            EventTypeMetadata meta = EventTypeMetadata.CreateWrapper("test", true, false, false);
    		_eventTypeSimple = new WrapperEventType(meta, "mytype", 1, underlyingEventTypeSimple, typeMap, _eventService);
    		_eventTypeCombined = new WrapperEventType(meta, "mytype", 1, underlyingEventTypeCombined, typeMap, _eventService);
    		_properties = new Dictionary<String, Object>();
    		_properties["string"] = "xx";
    		_properties["int"] = 11;
    
            EventBean wrappedSimple = _eventService.AdapterForObject(new SupportBeanSimple("EventString", 0));
            _eventBeanSimple = _eventService.AdapterForTypedWrapper(wrappedSimple, _properties, _eventTypeSimple);

            EventBean wrappedCombined = _eventService.AdapterForObject(SupportBeanCombinedProps.MakeDefaultBean());
            _eventBeanCombined = _eventService.AdapterForTypedWrapper(wrappedCombined, _properties, _eventTypeCombined);
    	}
    	
        [Test]
    	public void TestGetSimple()
    	{	
    		Assert.AreEqual("EventString", _eventBeanSimple.Get("MyString"));
    		Assert.AreEqual(0, _eventBeanSimple.Get("MyInt"));
    		AssertMap(_eventBeanSimple);
    	}
    	
        [Test]
    	public void TestGetCombined()
    	{
            Assert.AreEqual("0ma0", _eventBeanCombined.Get("Indexed[0].Mapped('0ma').Value"));
            Assert.AreEqual("0ma1", _eventBeanCombined.Get("Indexed[0].Mapped('0mb').Value"));
            Assert.AreEqual("1ma0", _eventBeanCombined.Get("Indexed[1].Mapped('1ma').Value"));
            Assert.AreEqual("1ma1", _eventBeanCombined.Get("Indexed[1].Mapped('1mb').Value"));
    
            Assert.AreEqual("0ma0", _eventBeanCombined.Get("Array[0].Mapped('0ma').Value"));
            Assert.AreEqual("1ma1", _eventBeanCombined.Get("Array[1].Mapped('1mb').Value"));
            
    		AssertMap(_eventBeanCombined);
    	}
    	
    	private void AssertMap(EventBean eventBean)
    	{
    		Assert.AreEqual("xx", eventBean.Get("string"));
    		Assert.AreEqual(11, eventBean.Get("int"));
    	}
    }
}
