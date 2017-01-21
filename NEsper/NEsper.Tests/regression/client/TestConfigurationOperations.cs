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

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestConfigurationOperations
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
        private ConfigurationOperations _configOps;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _configOps = _epService.EPAdministrator.Configuration;
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestAutoNamePackage()
        {
            _configOps.AddEventTypeAutoName(GetType().Namespace);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(MyAutoNamedEventType).FullName);
            stmt.Events += _testListener.Update;
    
            MyAutoNamedEventType eventOne = new MyAutoNamedEventType(10);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _testListener.AssertOneGetNewAndReset().Underlying);
        }
    
        [Test]
        public void TestAutoNamePackageAmbigous() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventTypeAutoName(GetType().Namespace);
            _configOps.AddEventTypeAutoName(GetType().Namespace);
            _configOps.AddEventTypeAutoName(typeof(SupportBean).Namespace);
    
            try {
                _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportAmbigousEventType).Name);
                Assert.Fail();
            } catch (Exception ex) {
                Assert.AreEqual("Failed to resolve event type: Failed to resolve name 'SupportAmbigousEventType', the class was ambigously found both in namespace 'com.espertech.esper.regression.client' and in namespace 'com.espertech.esper.support.bean' [select * from SupportAmbigousEventType]", ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("select * from XXXX");
                Assert.Fail();
            } catch (Exception ex) {
                Assert.AreEqual("Failed to resolve event type: Event type or class named 'XXXX' was not found [select * from XXXX]", ex.Message);
            }
        }
    
        [Test]
        public void TestAddDOMType() {
            TryInvalid("AddedDOMOne");
    
            // First statement with new name
            ConfigurationEventTypeXMLDOM domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            _configOps.AddEventType("AddedDOMOne", domConfig);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from AddedDOMOne");
            stmt.Events += _testListener.Update;

            XmlDocument eventOne = MakeDOMEvent("RootAddedDOMOne");
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne.DocumentElement, _testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid("AddedMapNameSecond");
    
            // Second statement using a new name to the same type, should both receive
            domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            _configOps.AddEventType("AddedDOMSecond", domConfig);
    
            _configOps.AddEventType("AddedMapNameSecond", domConfig);
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmt = _epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            XmlDocument eventTwo = MakeDOMEvent("RootAddedDOMOne");
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.IsTrue(_testListener.IsInvoked);
            Assert.AreEqual(eventTwo.DocumentElement, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            _configOps.AddEventType("AddedDOMSecond", domConfig);
    
            // Add the same name and a different type
            try {
                domConfig = new ConfigurationEventTypeXMLDOM();
                domConfig.RootElementName = "RootAddedDOMXXX";
                _configOps.AddEventType("AddedDOMSecond", domConfig);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestAddMapByClass() {
            TryInvalid("AddedMapOne");
    
            // First statement with new name
            IDictionary<String, Object> mapProps = new Dictionary<String, Object>();
            mapProps["prop1"] = typeof(int);
            _configOps.AddEventType("AddedMapOne", mapProps);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from AddedMapOne");
            stmt.Events += _testListener.Update;
    
            IDictionary<String, Object> eventOne = new Dictionary<String, Object>();
            eventOne["prop1"] = 1;
            _epService.EPRuntime.SendEvent(eventOne, "AddedMapOne");
            Assert.AreEqual(eventOne, _testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid("AddedMapNameSecond");
    
            // Second statement using a new name to the same type, should only one receive
            _configOps.AddEventType("AddedMapNameSecond", mapProps);
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmt = _epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            IDictionary<String, Object> eventTwo = new Dictionary<String, Object>();
            eventTwo["prop1"] = 1;
            _epService.EPRuntime.SendEvent(eventTwo, "AddedMapNameSecond");
            Assert.IsFalse(_testListener.IsInvoked);
            Assert.AreEqual(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            mapProps.Clear();
            mapProps["prop1"] = typeof(int);
            _configOps.AddEventType("AddedNameSecond", mapProps);
    
            // Add the same name and a different type
            try {
                mapProps["XX"] = typeof(int);
                _configOps.AddEventType("AddedNameSecond", mapProps);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestAddMapProperties() {
            TryInvalid("AddedMapOne");
    
            // First statement with new name
            IDictionary<String, Object> mapProps = new Dictionary<String, Object>();
            mapProps["prop1"] = typeof(int).FullName;
            _configOps.AddEventType("AddedMapOne", mapProps);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from AddedMapOne");
            stmt.Events += _testListener.Update;
    
            IDictionary<String, Object> eventOne = new Dictionary<String, Object>();
            eventOne["prop1"] = 1;
            _epService.EPRuntime.SendEvent(eventOne, "AddedMapOne");
            Assert.AreEqual(eventOne, _testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid("AddedMapNameSecond");
    
            // Second statement using a new alias to the same type, should only one receive
            _configOps.AddEventType("AddedMapNameSecond", mapProps);
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmt = _epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            IDictionary<String, Object> eventTwo = new Dictionary<String, Object>();
            eventTwo["prop1"] = 1;
            _epService.EPRuntime.SendEvent(eventTwo, "AddedMapNameSecond");
            Assert.IsFalse(_testListener.IsInvoked);
            Assert.AreEqual(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            mapProps.Clear();
            mapProps["prop1"] = typeof(int).FullName;
            _configOps.AddEventType("AddedNameSecond", mapProps);
    
            // Add the same name and a different type
            try {
                mapProps["XX"] = typeof(int).FullName;
                _configOps.AddEventType("AddedNameSecond", mapProps);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestAddAliasClassName() {
            TryInvalid("AddedName");
    
            // First statement with new name
            _configOps.AddEventType("AddedName", typeof(SupportBean).FullName);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from AddedName");
            stmt.Events += _testListener.Update;
    
            SupportBean eventOne = new SupportBean("a", 1);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid("AddedNameSecond");
    
            // Second statement using a new alias to the same type, should both receive
            _configOps.AddEventType("AddedNameSecond", typeof(SupportBean).FullName);
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmt = _epService.EPAdministrator.CreateEPL("select * from AddedNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            SupportBean eventTwo = new SupportBean("b", 2);
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, _testListener.AssertOneGetNewAndReset().Underlying);
            Assert.AreSame(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            _configOps.AddEventType("AddedNameSecond", typeof(SupportBean).FullName);
    
            // Add the same name and a different type
            try {
                _configOps.AddEventType("AddedNameSecond", typeof(SupportBean_A).FullName);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestAddNameClass() {
            TryInvalid("AddedName");
    
            // First statement with new name
            _configOps.AddEventType("AddedName", typeof(SupportBean));
            Assert.IsTrue(_configOps.IsEventTypeExists("AddedName"));
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from AddedName");
            stmt.Events += _testListener.Update;
    
            SupportBean eventOne = new SupportBean("a", 1);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid("AddedNameSecond");
    
            // Second statement using a new alias to the same type, should both receive
            _configOps.AddEventType("AddedNameSecond", typeof(SupportBean));
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmt = _epService.EPAdministrator.CreateEPL("select * from AddedNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            SupportBean eventTwo = new SupportBean("b", 2);
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, _testListener.AssertOneGetNewAndReset().Underlying);
            Assert.AreSame(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            _configOps.AddEventType("AddedNameSecond", typeof(SupportBean));
    
            // Add the same name and a different type
            try {
                _configOps.AddEventType("AddedNameSecond", typeof(SupportBean_A));
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        private void TryInvalid(String name) {
            try {
                _epService.EPAdministrator.CreateEPL("select * from " + name);
                Assert.Fail();
            } catch (EPStatementException ex) {
                // expected
            }
        }
    
        private XmlDocument MakeDOMEvent(String rootElementName)
        {
            String XML =
                "<VAL1>\n" +
                "  <someelement/>\n" +
                "</VAL1>";
    
            String xml = XML.Replace("VAL1", rootElementName);

            XmlDocument simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
            return simpleDoc;
        }
    }
}
