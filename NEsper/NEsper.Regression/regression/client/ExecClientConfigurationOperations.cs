///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientConfigurationOperations : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAutoName(this.GetType().Namespace);
            epService.EPAdministrator.Configuration.AddEventTypeAutoName(typeof(SupportBean).Namespace);
    
            RunAssertionAutoNamePackage(epService);
            RunAssertionAutoNamePackageAmbigous(epService);
            RunAssertionAddDOMType(epService);
            RunAssertionAddMapByClass(epService);
            RunAssertionAddMapProperties(epService);
            RunAssertionAddAliasClassName(epService);
            RunAssertionAddNameClass(epService);
        }
    
        private void RunAssertionAutoNamePackage(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAutoName(typeof(MyAutoNamedEventType).Namespace);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(MyAutoNamedEventType).FullName);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new MyAutoNamedEventType(10);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, testListener.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
        }
    
        private void RunAssertionAutoNamePackageAmbigous(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportAmbigousEventType",
                    "Failed to resolve event type: Failed to resolve name 'SupportAmbigousEventType', the class was ambigously found both in namespace 'com.espertech.esper.supportregression.bean' and in namespace 'com.espertech.esper.supportregression.client'");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from XXXX",
                    "Failed to resolve event type: Event type or class named 'XXXX' was not found");
        }
    
        private void RunAssertionAddDOMType(EPServiceProvider epService) {
            TryInvalid(epService, "AddedDOMOne");
    
            // First statement with new name
            var domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            epService.EPAdministrator.Configuration.AddEventType("AddedDOMOne", domConfig);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from AddedDOMOne");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            XmlDocument eventOne = MakeDOMEvent("RootAddedDOMOne");
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne.DocumentElement, testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid(epService, "AddedMapNameSecond");
    
            // Second statement using a new name to the same type, should both receive
            domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            epService.EPAdministrator.Configuration.AddEventType("AddedDOMSecond", domConfig);
    
            epService.EPAdministrator.Configuration.AddEventType("AddedMapNameSecond", domConfig);
            var testListenerTwo = new SupportUpdateListener();
            stmt = epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            XmlDocument eventTwo = MakeDOMEvent("RootAddedDOMOne");
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.IsTrue(testListener.IsInvoked);
            Assert.AreEqual(eventTwo.DocumentElement, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            domConfig = new ConfigurationEventTypeXMLDOM();
            domConfig.RootElementName = "RootAddedDOMOne";
            epService.EPAdministrator.Configuration.AddEventType("AddedDOMSecond", domConfig);
    
            // Add the same name and a different type
            try {
                domConfig = new ConfigurationEventTypeXMLDOM();
                domConfig.RootElementName = "RootAddedDOMXXX";
                epService.EPAdministrator.Configuration.AddEventType("AddedDOMSecond", domConfig);
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapNameSecond", true);
        }
    
        private void RunAssertionAddMapByClass(EPServiceProvider epService) {
            TryInvalid(epService, "AddedMapOne");
    
            // First statement with new name
            var mapProps = new Dictionary<string, object>();
            mapProps.Put("prop1", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("AddedMapOne", mapProps);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from AddedMapOne");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new Dictionary<string, object>();
            eventOne.Put("prop1", 1);
            epService.EPRuntime.SendEvent(eventOne, "AddedMapOne");
            Assert.AreEqual(eventOne, testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid(epService, "AddedMapNameSecond");
    
            // Second statement using a new name to the same type, should only one receive
            epService.EPAdministrator.Configuration.AddEventType("AddedMapNameSecond", mapProps);
            var testListenerTwo = new SupportUpdateListener();
            stmt = epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            var eventTwo = new Dictionary<string, object>();
            eventTwo.Put("prop1", 1);
            epService.EPRuntime.SendEvent(eventTwo, "AddedMapNameSecond");
            Assert.IsFalse(testListener.IsInvoked);
            Assert.AreEqual(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            mapProps.Clear();
            mapProps.Put("prop1", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", mapProps);
    
            // Add the same name and a different type
            try {
                mapProps.Put("XX", typeof(int));
                epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", mapProps);
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapOne", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapTwo", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapNameSecond", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedNameSecond", true);
        }
    
        private void RunAssertionAddMapProperties(EPServiceProvider epService) {
            TryInvalid(epService, "AddedMapOne");
    
            // First statement with new name
            var mapProps = new Dictionary<string, object>();
            mapProps.Put("prop1", typeof(int).FullName);
            epService.EPAdministrator.Configuration.AddEventType("AddedMapOne", mapProps);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from AddedMapOne");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new Dictionary<string, object>();
            eventOne.Put("prop1", 1);
            epService.EPRuntime.SendEvent(eventOne, "AddedMapOne");
            Assert.AreEqual(eventOne, testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid(epService, "AddedMapNameSecond");
    
            // Second statement using a new alias to the same type, should only one receive
            epService.EPAdministrator.Configuration.AddEventType("AddedMapNameSecond", mapProps);
            var testListenerTwo = new SupportUpdateListener();
            stmt = epService.EPAdministrator.CreateEPL("select * from AddedMapNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            var eventTwo = new Dictionary<string, object>();
            eventTwo.Put("prop1", 1);
            epService.EPRuntime.SendEvent(eventTwo, "AddedMapNameSecond");
            Assert.IsFalse(testListener.IsInvoked);
            Assert.AreEqual(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            mapProps.Clear();
            mapProps.Put("prop1", typeof(int).FullName);
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", mapProps);
    
            // Add the same name and a different type
            try {
                mapProps.Put("XX", typeof(int).FullName);
                epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", mapProps);
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapOne", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapTwo", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedMapNameSecond", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedNameSecond", true);
        }
    
        private void RunAssertionAddAliasClassName(EPServiceProvider epService) {
            TryInvalid(epService, "AddedName");
    
            // First statement with new name
            epService.EPAdministrator.Configuration.AddEventType("AddedName", typeof(SupportBean));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from AddedName");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new SupportBean("a", 1);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid(epService, "AddedNameSecond");
    
            // Second statement using a new alias to the same type, should both receive
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean));
            var testListenerTwo = new SupportUpdateListener();
            stmt = epService.EPAdministrator.CreateEPL("select * from AddedNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            var eventTwo = new SupportBean("b", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, testListener.AssertOneGetNewAndReset().Underlying);
            Assert.AreSame(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean));
    
            // Add the same name and a different type
            try {
                epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean_A));
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("AddedName", true);
            epService.EPAdministrator.Configuration.RemoveEventType("AddedNameSecond", true);
        }
    
        private void RunAssertionAddNameClass(EPServiceProvider epService) {
            TryInvalid(epService, "AddedName");
    
            // First statement with new name
            epService.EPAdministrator.Configuration.AddEventType("AddedName", typeof(SupportBean));
            Assert.IsTrue(epService.EPAdministrator.Configuration.IsEventTypeExists("AddedName"));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from AddedName");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new SupportBean("a", 1);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, testListener.AssertOneGetNewAndReset().Underlying);
    
            TryInvalid(epService, "AddedNameSecond");
    
            // Second statement using a new alias to the same type, should both receive
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean));
            var testListenerTwo = new SupportUpdateListener();
            stmt = epService.EPAdministrator.CreateEPL("select * from AddedNameSecond");
            stmt.Events += testListenerTwo.Update;
    
            var eventTwo = new SupportBean("b", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, testListener.AssertOneGetNewAndReset().Underlying);
            Assert.AreSame(eventTwo, testListenerTwo.AssertOneGetNewAndReset().Underlying);
    
            // Add the same name and type again
            epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean));
    
            // Add the same name and a different type
            try {
                epService.EPAdministrator.Configuration.AddEventType("AddedNameSecond", typeof(SupportBean_A));
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("AddedNameSecond", true);
        }
    
        private void TryInvalid(EPServiceProvider epService, string name) {
            try {
                epService.EPAdministrator.CreateEPL("select * from " + name);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private XmlDocument MakeDOMEvent(string rootElementName) {
            string xmlTemplate =
                    "<VAL1>\n" +
                            "  <someelement/>\n" +
                            "</VAL1>";
    
            var xml = xmlTemplate.RegexReplaceAll("VAL1", rootElementName);
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }
    }
} // end of namespace
