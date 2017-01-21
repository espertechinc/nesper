///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventSenderBuiltin
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private static IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            var result = new Dictionary<String, Object>();
            for (int i = 0; i < entries.Length; i++)
            {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }

        private XmlDocument GetDocument(String xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        [Test]
        public void TestInvalid()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof (SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            try
            {
                _epService.EPRuntime.GetEventSender("ABC");
                Assert.Fail();
            }
            catch (EventTypeException ex)
            {
                Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
            }

            EPStatement stmt =
                _epService.EPAdministrator.CreateEPL("insert into ABC select *, TheString as value from SupportBean");
            stmt.Events += _listener.Update;

            try
            {
                _epService.EPRuntime.GetEventSender("ABC");
                Assert.Fail("Event type named 'ABC' could not be found");
            }
            catch (EventTypeException ex)
            {
                Assert.AreEqual(
                    "An event sender for event type named 'ABC' could not be created as the type is internal",
                    ex.Message);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSenderMap()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            IDictionary<String, Object> myMapType = MakeMap(new[] {new Object[] {"f1", typeof (int)}});
            configuration.AddEventType("MyMap", myMapType);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            // type resolved for each by the first event representation picking both up, i.e. the one with "r2" since that is the most specific URI
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyMap");
            stmt.Events += _listener.Update;

            // send right event
            EventSender sender = _epService.EPRuntime.GetEventSender("MyMap");
            IDictionary<String, Object> myMap = MakeMap(new[] {new Object[] {"f1", 10}});
            sender.SendEvent(myMap);
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("f1"));

            // send wrong event
            try
            {
                sender.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Unexpected event object of type com.espertech.esper.support.bean.SupportBean, expected " +
                    typeof (IDictionary<string, object>).FullName, ex.Message);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSenderObjectArray()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("MyObjectArray", new String[] {"f1"}, new Object[] {typeof (int)});

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            // type resolved for each by the first event representation picking both up, i.e. the one with "r2" since that is the most specific URI
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyObjectArray");
            stmt.Events += _listener.Update;

            // send right event
            EventSender sender = _epService.EPRuntime.GetEventSender("MyObjectArray");
            sender.SendEvent(new Object[] {10});
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("f1"));

            // send wrong event
            try
            {
                sender.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Unexpected event object of type com.espertech.esper.support.bean.SupportBean, expected Object[]",
                    ex.Message);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSenderPONO()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof (SupportBean));
            configuration.AddEventType("Marker", typeof (SupportMarkerInterface));

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            // type resolved for each by the first event representation picking both up, i.e. the one with "r2" since that is the most specific URI
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmt.Events += _listener.Update;

            // send right event
            EventSender sender = _epService.EPRuntime.GetEventSender("SupportBean");
            Object supportBean = new SupportBean();
            sender.SendEvent(supportBean);
            Assert.AreSame(supportBean, _listener.AssertOneGetNewAndReset().Underlying);

            // send wrong event
            try
            {
                sender.SendEvent(new SupportBean_G("G1"));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Event object of type com.espertech.esper.support.bean.SupportBean_G does not equal, extend or implement the type com.espertech.esper.support.bean.SupportBean of event type 'SupportBean'",
                    ex.Message);
            }

            // test an interface
            sender = _epService.EPRuntime.GetEventSender("Marker");
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select * from Marker");
            stmt.Events += _listener.Update;
            var implA = new SupportMarkerImplA("Q2");
            sender.SendEvent(implA);
            Assert.AreSame(implA, _listener.AssertOneGetNewAndReset().Underlying);
            var implB = new SupportBean_G("Q3");
            sender.SendEvent(implB);
            Assert.AreSame(implB, _listener.AssertOneGetNewAndReset().Underlying);
            sender.SendEvent(implB);
            Assert.AreSame(implB, _listener.AssertOneGetNewAndReset().Underlying);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestXML()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            var typeMeta = new ConfigurationEventTypeXMLDOM();
            typeMeta.RootElementName = "a";
            typeMeta.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.AddEventType("AEvent", typeMeta);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String stmtText = "select b.c as type, element1 from AEvent";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            XmlDocument doc = GetDocument("<a><b><c>text</c></b></a>");
            EventSender sender = _epService.EPRuntime.GetEventSender("AEvent");
            sender.SendEvent(doc);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));

            // send wrong event
            try
            {
                sender.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>"));
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Unexpected root element name 'xxxx' encountered, expected a root element name of 'a'",
                                ex.Message);
            }

            try
            {
                sender.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Unexpected event object type 'com.espertech.esper.support.bean.SupportBean' encountered, please supply a System.Xml.XmlDocument or Element node",
                    ex.Message);
            }

            // test adding a second type for the same root element
            configuration = SupportConfigFactory.GetConfiguration();
            typeMeta = new ConfigurationEventTypeXMLDOM();
            typeMeta.RootElementName = "a";
            typeMeta.AddXPathProperty("element2", "//c", XPathResultType.String);
            typeMeta.IsEventSenderValidatesRoot = false;
            _epService.EPAdministrator.Configuration.AddEventType("BEvent", typeMeta);

            stmtText = "select element2 from BEvent.std:lastevent()";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtText);

            // test sender that doesn't care about the root element
            EventSender senderTwo = _epService.EPRuntime.GetEventSender("BEvent");
            senderTwo.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>")); // allowed, not checking

            theEvent = stmtTwo.FirstOrDefault();
            Assert.AreEqual("text", theEvent.Get("element2"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}