///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestEventAdapterServiceImpl 
    {
        private EventAdapterServiceImpl _adapterService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _adapterService = new EventAdapterServiceImpl(
                _container,
                new EventTypeIdGeneratorImpl(), 5, null,
                SupportEngineImportServiceFactory.Make(_container));
        }
    
        [Test]
        public void TestSelfRefEvent()
        {
            var originalBean = _adapterService.AdapterForObject(new SupportSelfReferenceEvent());
            Assert.AreEqual(null, originalBean.Get("SelfRef.SelfRef.SelfRef.Value"));
        }
    
        [Test]
        public void TestCreateMapType()
        {
            IDictionary<String, Object> testTypesMap;
            testTypesMap = new Dictionary<String, Object>();
            testTypesMap.Put("key1", typeof(String));
            var eventType = _adapterService.CreateAnonymousMapType("test", testTypesMap, true);
    
            Assert.AreEqual(typeof(DataMap), eventType.UnderlyingType);
            Assert.AreEqual(1, eventType.PropertyNames.Length);
            Assert.AreEqual("key1", eventType.PropertyNames[0]);
        }
    
        [Test]
        public void TestGetType()
        {
            _adapterService.AddBeanType("NAME", typeof(TestEventAdapterServiceImpl).FullName, false, false, false, false);

            var type = _adapterService.GetEventTypeByName("NAME");
            Assert.AreEqual(typeof(TestEventAdapterServiceImpl), type.UnderlyingType);

            var typeTwo = _adapterService.GetEventTypeByName(typeof(TestEventAdapterServiceImpl).FullName);
            Assert.AreSame(typeTwo, typeTwo);

            Assert.IsNull(_adapterService.GetEventTypeByName("xx"));
        }
    
        [Test]
        public void TestAddInvalid()
        {
            try
            {
                _adapterService.AddBeanType("x", "xx", false, false, false, false);
                Assert.Fail();
            }
            catch (EventAdapterException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestAddMapType()
        {
            var props = new Dictionary<String, Object>();
            props.Put("a", typeof(long));
            props.Put("b", typeof(string));
    
            // check result type
            var typeOne = _adapterService.AddNestableMapType("latencyEvent", props, null, true, true, true, false, false);
            Assert.AreEqual(typeof(long), typeOne.GetPropertyType("a"));
            Assert.AreEqual(typeof(string), typeOne.GetPropertyType("b"));
            Assert.AreEqual(2, typeOne.PropertyNames.Length);
    
            Assert.AreSame(typeOne, _adapterService.GetEventTypeByName("latencyEvent"));
    
            // add the same type with the same name, should succeed and return the same reference
            var typeTwo = _adapterService.AddNestableMapType("latencyEvent", props, null, true, true, true, false, false);
            Assert.AreSame(typeOne, typeTwo);
    
            // add the same name with a different type, should fail
            props.Put("b", typeof(bool));
            try
            {
                _adapterService.AddNestableMapType("latencyEvent", props, null, true, true, true, false, false);
                Assert.Fail();
            }
            catch (EventAdapterException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestAddWrapperType()
        {
            var beanEventType = _adapterService.AddBeanType("mybean", typeof(SupportMarketDataBean), true, true, true);
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put("a", typeof(long));
            props.Put("b", typeof(string));
    
            // check result type
            var typeOne = _adapterService.AddWrapperType("latencyEvent", beanEventType, props, false, true);
            Assert.AreEqual(typeof(long), typeOne.GetPropertyType("a"));
            Assert.AreEqual(typeof(string), typeOne.GetPropertyType("b"));
            Assert.AreEqual(7, typeOne.PropertyNames.Length);
    
            Assert.AreSame(typeOne, _adapterService.GetEventTypeByName("latencyEvent"));
    
            // add the same name with a different type, should fail
            props.Put("b", typeof(bool));
            try
            {
                var beanTwoEventType = _adapterService.AddBeanType("mybean", typeof(SupportBean), true, true, true);
                _adapterService.AddWrapperType("latencyEvent", beanTwoEventType, props, false, false);
                Assert.Fail();
            }
            catch (EventAdapterException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestAddClassName()
        {
            var typeOne = _adapterService.AddBeanType("latencyEvent", typeof(SupportBean).FullName, true, false, false, false);
            Assert.AreEqual(typeof(SupportBean), typeOne.UnderlyingType);

            Assert.AreSame(typeOne, _adapterService.GetEventTypeByName("latencyEvent"));
    
            var typeTwo = _adapterService.AddBeanType("latencyEvent", typeof(SupportBean).FullName, false, false, false, false);
            Assert.AreSame(typeOne, typeTwo);
    
            try
            {
                _adapterService.AddBeanType("latencyEvent", typeof(SupportBean_A).FullName, true, false, false, false);
                Assert.Fail();
            }
            catch (EventAdapterException ex)
            {
                Assert.AreEqual("Event type named 'latencyEvent' has already been declared with differing underlying type information: Class " + typeof(SupportBean).FullName + " versus " + typeof(SupportBean_A).FullName, ex.Message);
            }
        }
    
        [Test]
        public void TestAddClass()
        {
            var typeOne = _adapterService.AddBeanType("latencyEvent", typeof(SupportBean), false, false, false);
            Assert.AreEqual(typeof(SupportBean), typeOne.UnderlyingType);

            Assert.AreSame(typeOne, _adapterService.GetEventTypeByName("latencyEvent"));
    
            var typeTwo = _adapterService.AddBeanType("latencyEvent", typeof(SupportBean), false, false, false);
            Assert.AreSame(typeOne, typeTwo);
    
            try
            {
                _adapterService.AddBeanType("latencyEvent", typeof(SupportBean_A).FullName, false, false, false, false);
                Assert.Fail();
            }
            catch (EventAdapterException ex)
            {
                Assert.AreEqual("Event type named 'latencyEvent' has already been declared with differing underlying type information: Class " + typeof(SupportBean).FullName + " versus " + typeof(SupportBean_A).FullName, ex.Message);
            }
        }
    
        [Test]
        public void TestWrap()
        {
            var bean = new SupportBean();
            var theEvent = _adapterService.AdapterForObject(bean);
            Assert.AreSame(theEvent.Underlying, bean);
        }
    
        [Test]
        public void TestAddXMLDOMType()
        {
            _adapterService.AddXMLDOMType("XMLDOMTypeOne", GetXMLDOMConfig(), null, true);
            var eventType = _adapterService.GetEventTypeByName("XMLDOMTypeOne");
            Assert.AreEqual(typeof(XmlNode), eventType.UnderlyingType);

            Assert.AreSame(eventType, _adapterService.GetEventTypeByName("XMLDOMTypeOne"));
            
            try
            {
                _adapterService.AddXMLDOMType("a", new ConfigurationEventTypeXMLDOM(), null, true);
                Assert.Fail();
            }
            catch (EventAdapterException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestAdapterForDOM()
        {
            _adapterService.AddXMLDOMType("XMLDOMTypeOne", GetXMLDOMConfig(), null, true);
    
            const string xml = 
                "<simpleEvent>\n" +
                "  <nested1>value</nested1>\n" +
                "</simpleEvent>";

            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
    
            var bean = _adapterService.AdapterForDOM(simpleDoc);
            Assert.AreEqual("value", bean.Get("nested1"));
        }
    
        private static ConfigurationEventTypeXMLDOM GetXMLDOMConfig()
        {
            var config = new ConfigurationEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            config.AddXPathProperty("nested1", "/simpleEvent/nested1", XPathResultType.String);
            return config;
        }
    }
}
