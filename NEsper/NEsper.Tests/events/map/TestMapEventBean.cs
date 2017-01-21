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
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.events.map
{
    [TestFixture]
    public class TestMapEventBean 
    {
        private IDictionary<String, Object> _testTypesMap;
        private IDictionary<String, Object> _testValuesMap;
    
        private EventType _eventType;
        private MapEventBean _eventBean;
    
        private readonly SupportBeanComplexProps _supportBean = SupportBeanComplexProps.MakeDefaultBean();
    
        [SetUp]
        public void SetUp()
        {
            _testTypesMap = new Dictionary<String, Object>();
            _testTypesMap["aString"] = typeof(string);
            _testTypesMap["anInt"] = typeof(int);
            _testTypesMap["myComplexBean"] = typeof(SupportBeanComplexProps);
    
            _testValuesMap = new Dictionary<String, Object>();
            _testValuesMap["aString"] = "test";
            _testValuesMap["anInt"] = 10;
            _testValuesMap["myComplexBean"] = _supportBean;
    
            _eventType = new MapEventType(null, "", 1, SupportEventAdapterService.Service, _testTypesMap, null, null, null);
            _eventBean = new MapEventBean(_testValuesMap, _eventType);
        }
    
        [Test]
        public void TestGet()
        {
            Assert.AreEqual(_eventType, _eventBean.EventType);
            Assert.AreEqual(_testValuesMap, _eventBean.Underlying);
    
            Assert.AreEqual("test", _eventBean.Get("aString"));
            Assert.AreEqual(10, _eventBean.Get("anInt"));
    
            Assert.AreEqual("NestedValue", _eventBean.Get("myComplexBean.Nested.NestedValue"));
    
            // test wrong property name
            try
            {
                _eventBean.Get("dummy");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }
        }
    
        [Test]
        public void TestCreateUnderlying()
        {
            SupportBean beanOne = new SupportBean();
            SupportBean_A beanTwo = new SupportBean_A("a");

            // Set up event type
            _testTypesMap.Clear();
            _testTypesMap["a"] = typeof(SupportBean);
            _testTypesMap["b"] = typeof(SupportBean_A);
            EventType eventType = SupportEventAdapterService.Service.CreateAnonymousMapType("test", _testTypesMap, true);
    
            IDictionary<String, Object> events = new Dictionary<String, Object>();
            events["a"] = beanOne;
            events["b"] = beanTwo;
    
            MapEventBean theEvent = new MapEventBean(events, eventType);
            Assert.AreSame(theEvent.Get("a"), beanOne);
            Assert.AreSame(theEvent.Get("b"), beanTwo);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
