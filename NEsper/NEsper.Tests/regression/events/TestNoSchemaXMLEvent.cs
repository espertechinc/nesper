///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.support.xml;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestNoSchemaXMLEvent
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;

        private const String XML =
            "<myevent>\n" +
            "  <element1>VAL1</element1>\n" +
            "  <element2>\n" +
            "    <element21 id=\"e21_1\">VAL21-1</element21>\n" +
            "    <element21 id=\"e21_2\">VAL21-2</element21>\n" +
            "  </element2>\n" +
            "  <element3 attrString=\"VAL3\" attrNum=\"5\" attrBool=\"true\"/>\n" +
            "  <element4><element41>VAL4-1</element41></element4>\n" +
            "</myevent>";

        [TearDown]
        public void TearDown()
        {
            _updateListener = null;
        }

        [Test]
        public void TestVariableAndDotMethodResolution()
        {
            // test for ESPER-341
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddVariable("var", typeof(int), 0);

            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNum", "/myevent/@attrnum", XPathResultType.String, "long");
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNumTwo", "/myevent/@attrnumtwo", XPathResultType.String, "long");
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetProvider("TestNoSchemaXML", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmtTextOne = "select var, xpathAttrNum.After(xpathAttrNumTwo) from TestXMLNoSchemaType.win:length(100)";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSimpleXMLXPathProperties()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();

            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.AddXPathProperty("xpathElement1", "/myevent/element1", XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathCountE21", "count(/myevent/element2/element21)", XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrString", "/myevent/element3/@attrString", XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNum", "/myevent/element3/@attrNum", XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrBool", "/myevent/element3/@attrBool", XPathResultType.Boolean);
            xmlDOMEventTypeDesc.AddXPathProperty("stringCastLong", "/myevent/element3/@attrNum", XPathResultType.String, "long");
            xmlDOMEventTypeDesc.AddXPathProperty("stringCastDouble", "/myevent/element3/@attrNum", XPathResultType.String, "double");
            xmlDOMEventTypeDesc.AddXPathProperty("numCastInt", "/myevent/element3/@attrNum", XPathResultType.Number, "int");
            xmlDOMEventTypeDesc.XPathFunctionResolver = typeof(SupportXPathFunctionResolver).FullName;
            xmlDOMEventTypeDesc.XPathVariableResolver = typeof(SupportXPathVariableResolver).FullName;
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);

            xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "my.event2";
            configuration.AddEventType("TestXMLWithDots", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetProvider("TestNoSchemaXML", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            // assert type metadata
            EventTypeSPI type = (EventTypeSPI)((EPServiceProviderSPI)_epService).EventAdapterService.GetEventTypeByName("TestXMLNoSchemaType");
            Assert.AreEqual(ApplicationType.XML, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("TestXMLNoSchemaType", type.Metadata.PrimaryName);
            Assert.AreEqual("TestXMLNoSchemaType", type.Metadata.PublicName);
            Assert.AreEqual("TestXMLNoSchemaType", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);

            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("xpathElement1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("xpathCountE21", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("xpathAttrString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("xpathAttrNum", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("xpathAttrBool", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("stringCastLong", typeof(long), null, false, false, false, false, false),
                    new EventPropertyDescriptor("stringCastDouble", typeof(double), null, false, false, false, false, false),
                    new EventPropertyDescriptor("numCastInt", typeof(int), null, false, false, false, false, false),
            }, type.PropertyDescriptors);

            String stmt =
                    "select xpathElement1, xpathCountE21, xpathAttrString, xpathAttrNum, xpathAttrBool," +
                            "stringCastLong," +
                            "stringCastDouble," +
                            "numCastInt " +
                            "from TestXMLNoSchemaType.win:length(100)";

            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent("EventA");
            AssertDataSimpleXPath("EventA");

            SendEvent("EventB");
            AssertDataSimpleXPath("EventB");

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSimpleXMLDOMGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();

            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.IsXPathPropertyExpr = false;    // <== DOM getter
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetProvider("TestNoSchemaXML", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            
            _updateListener = new SupportUpdateListener();

            String stmt =
                    "select element1, invalidelement, " +
                            "element4.element41 as nestedElement," +
                            "element2.element21('e21_2') as mappedElement," +
                            "element2.element21[1] as indexedElement," +
                            "element3.myattribute as invalidattribute " +
                            "from TestXMLNoSchemaType.win:length(100)";

            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent("EventA");
            AssertDataGetter("EventA", false);

            SendEvent("EventB");
            AssertDataGetter("EventB", false);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSimpleXMLXPathGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();

            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.IsXPathPropertyExpr = true;    // <== XPath getter
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetProvider("TestNoSchemaXML", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            
            _updateListener = new SupportUpdateListener();

            String stmt =
                    "select element1, invalidelement, " +
                            "element4.element41 as nestedElement," +
                            "element2.element21('e21_2') as mappedElement," +
                            "element2.element21[1] as indexedElement," +
                            "element3.myattribute as invalidattribute " +
                            "from TestXMLNoSchemaType.win:length(100)";

            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent("EventA");
            AssertDataGetter("EventA", false);

            SendEvent("EventB");
            AssertDataGetter("EventB", false);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNestedXMLDOMGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "a";
            xmlDOMEventTypeDesc.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.AddEventType("AEvent", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            
            _updateListener = new SupportUpdateListener();

            String stmt = "select b.c as type, element1, result1 from AEvent";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            SendXMLEvent("<a><b><c></c></b></a>");
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("", theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));

            SendXMLEvent("<a><b></b></a>");
            theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("type"));
            Assert.AreEqual(null, theEvent.Get("element1"));

            SendXMLEvent("<a><b><c>text</c></b></a>");
            theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNestedXMLXPathGetter()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "a";
            xmlDOMEventTypeDesc.IsXPathPropertyExpr = true;
            xmlDOMEventTypeDesc.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.AddEventType("AEvent", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            
            _updateListener = new SupportUpdateListener();

            String stmt = "select b.c as type, element1, result1 from AEvent";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            SendXMLEvent("<a><b><c></c></b></a>");
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("", theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));

            SendXMLEvent("<a><b></b></a>");
            theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("type"));
            Assert.AreEqual(null, theEvent.Get("element1"));

            SendXMLEvent("<a><b><c>text</c></b></a>");
            theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestDotEscapeSyntax()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myroot";
            configuration.AddEventType("AEvent", xmlDOMEventTypeDesc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmt = "select a\\.b.c\\.d as val from AEvent";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            SendXMLEvent("<myroot><a.b><c.d>value</c.d></a.b></myroot>");
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("value", theEvent.Get("val"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestEventXML()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.AddXPathProperty("event.type", "/event/@type", XPathResultType.String);
            desc.AddXPathProperty("event.uid", "/event/@uid", XPathResultType.String);
            desc.RootElementName = "event";
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmt = "select event.type as type, event.uid as uid from MyEvent";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            SendXMLEvent("<event type=\"a-f-G\" uid=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"></event>");
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("a-f-G", theEvent.Get("type"));
            Assert.AreEqual("terminal.55", theEvent.Get("uid"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestElementNode()
        {
            // test for Esper-129
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.AddXPathProperty("event.type", "//event/@type", XPathResultType.String);
            desc.AddXPathProperty("event.uid", "//event/@uid", XPathResultType.String);
            desc.RootElementName = "batch-event";
            configuration.AddEventType("MyEvent", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmt = "select event.type as type, event.uid as uid from MyEvent";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            String xml =
                "<batch-event>" +
                "<event type=\"a-f-G\" uid=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"/>" +
                "</batch-event>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement topElement = doc.DocumentElement;

            _epService.EPRuntime.SendEvent(topElement);
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("a-f-G", theEvent.Get("type"));
            Assert.AreEqual("terminal.55", theEvent.Get("uid"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNamespaceXPathRelative()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "getQuote";
            desc.DefaultNamespace = "http://services.samples/xsd";
            desc.RootElementNamespace = "http://services.samples/xsd";
            desc.AddNamespacePrefix("m0", "http://services.samples/xsd");
            desc.IsXPathResolvePropertiesAbsolute = false;
            desc.IsXPathPropertyExpr = true;
            configuration.AddEventType("StockQuote", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmt = "select request.symbol as symbol_a, symbol as symbol_b from StockQuote";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            String xml = "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            _epService.EPRuntime.SendEvent(doc);
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNamespaceXPathAbsolute()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.AddXPathProperty("symbol_a", "//m0:symbol", XPathResultType.String);
            desc.AddXPathProperty("symbol_b", "//*[local-name(.) = 'getQuote' and namespace-uri(.) = 'http://services.samples/xsd']", XPathResultType.String);
            desc.AddXPathProperty("symbol_c", "/m0:getQuote/m0:request/m0:symbol", XPathResultType.String);
            desc.RootElementName = "getQuote";
            desc.DefaultNamespace = "http://services.samples/xsd";
            desc.RootElementNamespace = "http://services.samples/xsd";
            desc.AddNamespacePrefix("m0", "http://services.samples/xsd");
            desc.IsXPathResolvePropertiesAbsolute = true;
            desc.IsXPathPropertyExpr = true;
            configuration.AddEventType("StockQuote", desc);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _updateListener = new SupportUpdateListener();

            String stmt = "select symbol_a, symbol_b, symbol_c, request.symbol as symbol_d, symbol as symbol_e from StockQuote";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += _updateListener.Update;

            String xml = "<m0:getQuote xmlns:m0=\"http://services.samples/xsd\"><m0:request><m0:symbol>IBM</m0:symbol></m0:request></m0:getQuote>";
            //String xml = "<getQuote><request><symbol>IBM</symbol></request></getQuote>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // For XPath resolution testing and namespaces...
            /*
            XPathFactory xPathFactory = XPathFactory.NewInstance();
            XPath xPath = xPathFactory.NewXPath();
            XPathNamespaceContext ctx = new XPathNamespaceContext();
            ctx.AddPrefix("m0", "http://services.samples/xsd");
            xPath.NamespaceContext = ctx;
            XPathExpression expression = xPath.Compile("/m0:getQuote/m0:request/m0:symbol");
            xPath.NamespaceContext = ctx;
            Console.WriteLine("result=" + expression.Evaluate(doc,XPathResultType.String));
            */

            _epService.EPRuntime.SendEvent(doc);
            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("IBM", theEvent.Get("symbol_a"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_b"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_c"));
            Assert.AreEqual("IBM", theEvent.Get("symbol_d"));
            Assert.AreEqual(null, theEvent.Get("symbol_e"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestXPathArray()
        {
            String xml = "<Event IsTriggering=\"True\">\n" +
                    "<Field Name=\"A\" Value=\"987654321\"/>\n" +
                    "<Field Name=\"B\" Value=\"2196958725202\"/>\n" +
                    "<Field Name=\"C\" Value=\"1232363702\"/>\n" +
                    "<Participants>\n" +
                    "<Participant>\n" +
                    "<Field Name=\"A\" Value=\"9876543210\"/>\n" +
                    "<Field Name=\"B\" Value=\"966607340\"/>\n" +
                    "<Field Name=\"D\" Value=\"353263010930650\"/>\n" +
                    "</Participant>\n" +
                    "</Participants>\n" +
                    "</Event>";

            ConfigurationEventTypeXMLDOM desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "Event";
            desc.AddXPathProperty("A", "//Field[@Name='A']/@Value", XPathResultType.NodeSet, "String[]");

            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddEventType("Event", desc);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from Event");
            _updateListener = new SupportUpdateListener();
            stmt.Events += _updateListener.Update;

            XmlDocument doc = SupportXML.GetDocument(xml);
            _epService.EPRuntime.SendEvent(doc);

            EventBean theEvent = _updateListener.AssertOneGetNewAndReset();
            Object value = theEvent.Get("A");
            EPAssertionUtil.AssertProps(theEvent, "A".Split(','), new Object[] { new Object[] { "987654321", "9876543210" } });

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void AssertDataSimpleXPath(String element1)
        {
            Assert.NotNull(_updateListener.LastNewData);
            EventBean theEvent = _updateListener.LastNewData[0];

            Assert.AreEqual(element1, theEvent.Get("xpathElement1"));
            Assert.AreEqual(2.0, theEvent.Get("xpathCountE21"));
            Assert.AreEqual("VAL3", theEvent.Get("xpathAttrString"));
            Assert.AreEqual(5d, theEvent.Get("xpathAttrNum"));
            Assert.AreEqual(true, theEvent.Get("xpathAttrBool"));
            Assert.AreEqual(5L, theEvent.Get("stringCastLong"));
            Assert.AreEqual(5d, theEvent.Get("stringCastDouble"));
            Assert.AreEqual(5, theEvent.Get("numCastInt"));
        }

        private void AssertDataGetter(String element1, bool isInvalidReturnsEmptyString)
        {
            Assert.NotNull(_updateListener.LastNewData);
            EventBean theEvent = _updateListener.LastNewData[0];

            Assert.AreEqual(element1, theEvent.Get("element1"));
            Assert.AreEqual("VAL4-1", theEvent.Get("nestedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("mappedElement"));
            Assert.AreEqual("VAL21-2", theEvent.Get("indexedElement"));

            if (isInvalidReturnsEmptyString)
            {
                Assert.AreEqual("", theEvent.Get("invalidelement"));
                Assert.AreEqual("", theEvent.Get("invalidattribute"));
            }
            else
            {
                Assert.AreEqual(null, theEvent.Get("invalidelement"));
                Assert.AreEqual(null, theEvent.Get("invalidattribute"));
            }
        }

        private void SendEvent(String value)
        {
            String xml = XML.Replace("VAL1", value);
            Log.Debug(".sendEvent value=" + value);

            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);

            _epService.EPRuntime.SendEvent(simpleDoc);
        }

        private void SendXMLEvent(String xml)
        {
            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
            _epService.EPRuntime.SendEvent(simpleDoc);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
