///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventPropertyDynamicXML
    {
        private static String CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";

        private static String NOSCHEMA_XML = "<simpleEvent>\n" +
                "\t<type>abc</type>\n" +
                "\t<dyn>1</dyn>\n" +
                "\t<dyn>2</dyn>\n" +
                "\t<nested>\n" +
                "\t\t<nes2>3</nes2>\n" +
                "\t</nested>\n" +
                "\t<map id='a'>4</map>\n" +
                "</simpleEvent>";

        private static String SCHEMA_XML = "<simpleEvent xmlns=\"samples:schemas:simpleSchema\" \n" +
                "  xmlns:ss=\"samples:schemas:simpleSchema\" \n" +
                "  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \n" +
                "  xsi:schemaLocation=\"samples:schemas:simpleSchema simpleSchema.xsd\">" +
                "<type>abc</type>\n" +
                "<dyn>1</dyn>\n" +
                "<dyn>2</dyn>\n" +
                "<nested>\n" +
                "<nes2>3</nes2>\n" +
                "</nested>\n" +
                "<map id='a'>4</map>\n" +
                "</simpleEvent>";

        private SupportUpdateListener _listener;
        private EPServiceProvider _epService;

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

        [Test]
        public void TestSchemaXPathGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "simpleEvent";
            String schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            desc.SchemaResource = schemaUri;
            desc.IsXPathPropertyExpr = true;
            desc.IsEventSenderValidatesRoot = false;
            desc.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            desc.DefaultNamespace = "samples:schemas:simpleSchema";
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String stmtText = "select type?,dyn[1]?,nested.nes2?,map('a')? from MyEvent";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested.nes2?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null, false, false, false, false, false),
            }, stmt.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmt.EventType);

            EventSender sender = _epService.EPRuntime.GetEventSender("MyEvent");
            XmlDocument root = SupportXML.SendEvent(sender, SCHEMA_XML);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
            EventTypeAssertionUtil.AssertConsistency(theEvent);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSchemaDOMGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "simpleEvent";
            String schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            desc.SchemaResource = schemaUri;
            desc.IsXPathPropertyExpr = false;
            desc.IsEventSenderValidatesRoot = false;
            desc.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            desc.DefaultNamespace = "samples:schemas:simpleSchema";
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String stmtText = "select type?,dyn[1]?,nested.nes2?,map('a')? from MyEvent";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested.nes2?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null, false, false, false, false, false),
            }, stmt.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmt.EventType);

            EventSender sender = _epService.EPRuntime.GetEventSender("MyEvent");
            XmlDocument root = SupportXML.SendEvent(sender, SCHEMA_XML);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
            EventTypeAssertionUtil.AssertConsistency(theEvent);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNoSchemaXPathGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "simpleEvent";
            desc.IsXPathPropertyExpr = true;
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String stmtText = "select type?,dyn[1]?,nested.nes2?,map('a')?,other? from MyEvent";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested.nes2?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null, false, false, false, false, false),
                    new EventPropertyDescriptor("other?", typeof(XmlNode), null, false, false, false, false, false),
            }, stmt.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmt.EventType);

            XmlDocument root = SupportXML.SendEvent(_epService.EPRuntime, NOSCHEMA_XML);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
            Assert.IsNull(theEvent.Get("other?"));
            EventTypeAssertionUtil.AssertConsistency(theEvent);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNoSchemaDOMGetter()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "simpleEvent";
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var stmtText = "select type?,dyn[1]?,nested.nes2?,map('a')? from MyEvent";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            EPAssertionUtil.AssertEqualsAnyOrder(
                new[]
                {
                    new EventPropertyDescriptor("type?", typeof(XmlNode), null, false,
                                                false, false, false, false),
                    new EventPropertyDescriptor("dyn[1]?", typeof(XmlNode), null,
                                                false, false, false, false, false),
                    new EventPropertyDescriptor("nested.nes2?", typeof(XmlNode), null,
                                                false, false, false, false, false),
                    new EventPropertyDescriptor("map('a')?", typeof(XmlNode), null,
                                                false, false, false, false, false),
                }, stmt.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmt.EventType);

            XmlDocument root = SupportXML.SendEvent(_epService.EPRuntime, NOSCHEMA_XML);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(0), theEvent.Get("type?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(2), theEvent.Get("dyn[1]?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(3).ChildNodes.Item(0), theEvent.Get("nested.nes2?"));
            Assert.AreSame(root.DocumentElement.ChildNodes.Item(4), theEvent.Get("map('a')?"));
            EventTypeAssertionUtil.AssertConsistency(theEvent);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
