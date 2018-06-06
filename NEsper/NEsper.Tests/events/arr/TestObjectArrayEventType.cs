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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.arr
{
    [TestFixture]
    public class TestObjectArrayEventType
    {
        private EventAdapterService _eventAdapterService;
        private ObjectArrayEventType _eventType;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _eventAdapterService = _container.Resolve<EventAdapterService>();
    
            EventTypeMetadata metadata = EventTypeMetadata.CreateNonPonoApplicationType(ApplicationType.OBJECTARR, "typename", true, true, true, false, false);
            String[] names = {"myInt", "myIntBoxed", "myString", "mySupportBean", "myComplexBean", "myNullType"};
            Object[] types = {typeof(int), typeof(int?), typeof(string), typeof(SupportBean), typeof(SupportBeanComplexProps), null};
    
            IDictionary<String, Object> namesAndTypes = new LinkedHashMap<String, Object>();
            for (int i = 0; i < names.Length; i++) {
                namesAndTypes.Put(names[i], types[i]);
            }
    
            _eventType = new ObjectArrayEventType(metadata, "typename", 1, _eventAdapterService, namesAndTypes, null, null, null);
        }
    
        [Test]
        public void TestGetPropertyNames()
        {
            var properties = _eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(properties, new String[]{"myInt", "myIntBoxed", "myString", "mySupportBean", "myComplexBean", "myNullType"});
        }
    
        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(int), _eventType.GetPropertyType("myInt"));
            Assert.AreEqual(typeof(int?), _eventType.GetPropertyType("myIntBoxed"));
            Assert.AreEqual(typeof(string), _eventType.GetPropertyType("myString"));
            Assert.AreEqual(typeof(SupportBean), _eventType.GetPropertyType("mySupportBean"));
            Assert.AreEqual(typeof(SupportBeanComplexProps), _eventType.GetPropertyType("myComplexBean"));
            Assert.AreEqual(typeof(int), _eventType.GetPropertyType("mySupportBean.IntPrimitive"));
            Assert.AreEqual(typeof(string), _eventType.GetPropertyType("myComplexBean.Nested.NestedValue"));
            Assert.AreEqual(typeof(int), _eventType.GetPropertyType("myComplexBean.Indexed[1]"));
            Assert.AreEqual(typeof(string), _eventType.GetPropertyType("myComplexBean.Mapped('a')"));
            Assert.AreEqual(null, _eventType.GetPropertyType("myNullType"));
    
            Assert.IsNull(_eventType.GetPropertyType("dummy"));
            Assert.IsNull(_eventType.GetPropertyType("mySupportBean.dfgdg"));
            Assert.IsNull(_eventType.GetPropertyType("xxx.IntPrimitive"));
            Assert.IsNull(_eventType.GetPropertyType("myComplexBean.Nested.nestedValueXXX"));
        }
        
        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(object[]), _eventType.UnderlyingType);
        }
    
        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(_eventType.IsProperty("myInt"));
            Assert.IsTrue(_eventType.IsProperty("myIntBoxed"));
            Assert.IsTrue(_eventType.IsProperty("myString"));
            Assert.IsTrue(_eventType.IsProperty("mySupportBean.IntPrimitive"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Nested.NestedValue"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Indexed[1]"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Mapped('a')"));
            Assert.IsTrue(_eventType.IsProperty("myNullType"));
    
            Assert.IsFalse(_eventType.IsProperty("dummy"));
            Assert.IsFalse(_eventType.IsProperty("mySupportBean.dfgdg"));
            Assert.IsFalse(_eventType.IsProperty("xxx.IntPrimitive"));
            Assert.IsFalse(_eventType.IsProperty("myComplexBean.Nested.NestedValueXXX"));
        }
    
        [Test]
        public void TestGetGetter()
        {
            SupportBean nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            SupportBeanComplexProps complexPropBean = SupportBeanComplexProps.MakeDefaultBean();
    
            Assert.AreEqual(null, _eventType.GetGetter("dummy"));
    
            Object[] values = new Object[] {20, 20, "a", nestedSupportBean, complexPropBean, null};
            EventBean eventBean = new ObjectArrayEventBean(values, _eventType);
    
            Assert.AreEqual(20, _eventType.GetGetter("myInt").Get(eventBean));
            Assert.AreEqual(20, _eventType.GetGetter("myIntBoxed").Get(eventBean));
            Assert.AreEqual("a", _eventType.GetGetter("myString").Get(eventBean));
            Assert.AreEqual(nestedSupportBean, _eventType.GetGetter("mySupportBean").Get(eventBean));
            Assert.AreEqual(100, _eventType.GetGetter("mySupportBean.IntPrimitive").Get(eventBean));
            Assert.AreEqual("NestedValue", _eventType.GetGetter("myComplexBean.Nested.NestedValue").Get(eventBean));
    
            try
            {
                eventBean = SupportEventBeanFactory.CreateObject(new Object());
                _eventType.GetGetter("myInt").Get(eventBean);
                Assert.IsTrue(false);
            }
            catch (InvalidCastException)
            {
            }
        }
    
        [Test]
        public void TestGetSuperTypes()
        {
            Assert.IsNull(_eventType.SuperTypes);
        }
    }
}
