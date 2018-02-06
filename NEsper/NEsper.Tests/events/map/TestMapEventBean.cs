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
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
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
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _testTypesMap = new Dictionary<String, Object>();
            _testTypesMap["aString"] = typeof(string);
            _testTypesMap["anInt"] = typeof(int);
            _testTypesMap["myComplexBean"] = typeof(SupportBeanComplexProps);
    
            _testValuesMap = new Dictionary<String, Object>();
            _testValuesMap["aString"] = "test";
            _testValuesMap["anInt"] = 10;
            _testValuesMap["myComplexBean"] = _supportBean;

            EventTypeMetadata metadata = EventTypeMetadata.CreateNonPonoApplicationType(ApplicationType.MAP, "testtype", true, true, true, false, false);
            _eventType = new MapEventType(metadata, "", 1, _container.Resolve<EventAdapterService>(), _testTypesMap, null, null, null);
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
            EventType eventType = _container.Resolve<EventAdapterService>().CreateAnonymousMapType("test", _testTypesMap, true);
    
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
