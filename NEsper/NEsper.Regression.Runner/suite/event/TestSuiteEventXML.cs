///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Xml.XPath;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.suite.@event.xml;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventXML
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            configuration.Common.AddVariable("var", typeof(int), 0);

            foreach (var clazz in new[] { typeof(SupportBean) })
            {
                configuration.Common.AddEventType(clazz);
            }

            var resourceManager = configuration.Container.ResourceManager();
            string schemaUriSimpleSchema = resourceManager
                .ResolveResourceURL("regression/simpleSchema.xsd")
                .ToString();
            string schemaUriTypeTestSchema = resourceManager
                .ResolveResourceURL("regression/typeTestSchema.xsd")
                .ToString();
            string schemaUriSimpleSchemaWithAll = resourceManager
                .ResolveResourceURL("regression/simpleSchemaWithAll.xsd")
                .ToString();
            string schemaUriSensorEvent = resourceManager
                .ResolveResourceURL("regression/sensorSchema.xsd")
                .ToString();

            var schemaStream = resourceManager
                .GetResourceAsStream("regression/simpleSchemaWithRestriction.xsd");
            var schemaReader = new StreamReader(schemaStream);
            Assert.IsNotNull(schemaStream);
            var schemaTextSimpleSchemaWithRestriction = FileUtil.LinesToText(
                FileUtil.ReadFile(schemaReader));

            var aEventConfig = new ConfigurationCommonEventTypeXMLDOM();
            aEventConfig.RootElementName = "myroot";
            configuration.Common.AddEventType("AEvent", aEventConfig);

            var aEventWithXPath = new ConfigurationCommonEventTypeXMLDOM();
            aEventWithXPath.RootElementName = "a";
            aEventWithXPath.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.Common.AddEventType("AEventWithXPath", aEventWithXPath);

            var aEventMoreXPath = new ConfigurationCommonEventTypeXMLDOM();
            aEventMoreXPath.RootElementName = "a";
            aEventMoreXPath.IsXPathPropertyExpr = true;
            aEventMoreXPath.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.Common.AddEventType("AEventMoreXPath", aEventMoreXPath);

            var desc = new ConfigurationCommonEventTypeXMLDOM();
            desc.AddXPathProperty("event.type", "//event/@type", XPathResultType.String);
            desc.AddXPathProperty("event.uid", "//event/@uid", XPathResultType.String);
            desc.RootElementName = "batch-event";
            configuration.Common.AddEventType("MyEvent", desc);

            var myEventSimpleEvent = new ConfigurationCommonEventTypeXMLDOM();
            myEventSimpleEvent.RootElementName = "simpleEvent";
            configuration.Common.AddEventType("MyEventSimpleEvent", myEventSimpleEvent);

            var mwEventWXPathExprTrue = new ConfigurationCommonEventTypeXMLDOM();
            mwEventWXPathExprTrue.RootElementName = "simpleEvent";
            mwEventWXPathExprTrue.IsXPathPropertyExpr = true;
            configuration.Common.AddEventType("MyEventWXPathExprTrue", mwEventWXPathExprTrue);

            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            configuration.Common.AddEventType("TestXMLSchemaType", eventTypeMeta);

            var rootMeta = new ConfigurationCommonEventTypeXMLDOM();
            rootMeta.RootElementName = "simpleEvent";
            rootMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            rootMeta.AddXPathPropertyFragment(
                "nested1simple",
                "/ss:simpleEvent/ss:nested1",
                XPathResultType.Any,
                "MyNestedEvent");
            rootMeta.AddXPathPropertyFragment(
                "nested4array",
                "//ss:nested4",
                XPathResultType.NodeSet,
                "MyNestedArrayEvent");
            configuration.Common.AddEventType("MyXMLEvent", rootMeta);

            var metaNested = new ConfigurationCommonEventTypeXMLDOM();
            metaNested.RootElementName = "nested1";
            configuration.Common.AddEventType("MyNestedEvent", metaNested);

            var metaNestedArray = new ConfigurationCommonEventTypeXMLDOM();
            metaNestedArray.RootElementName = "nested4";
            configuration.Common.AddEventType("MyNestedArrayEvent", metaNestedArray);

            configuration.Compiler.ViewResources.IterableUnbound = true;

            var testXMLSchemaTypeTXG = new ConfigurationCommonEventTypeXMLDOM();
            testXMLSchemaTypeTXG.RootElementName = "simpleEvent";
            testXMLSchemaTypeTXG.SchemaResource = schemaUriSimpleSchema;
            testXMLSchemaTypeTXG.IsXPathPropertyExpr = true; // <== note this
            testXMLSchemaTypeTXG.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            configuration.Common.AddEventType("TestXMLSchemaTypeTXG", testXMLSchemaTypeTXG);

            var myEventWTypeAndUID = new ConfigurationCommonEventTypeXMLDOM();
            myEventWTypeAndUID.AddXPathProperty("event.type", "/event/@type", XPathResultType.String);
            myEventWTypeAndUID.AddXPathProperty("event.uid", "/event/@uid", XPathResultType.String);
            myEventWTypeAndUID.RootElementName = "event";
            configuration.Common.AddEventType("MyEventWTypeAndUID", myEventWTypeAndUID);

            var stockQuote = new ConfigurationCommonEventTypeXMLDOM();
            stockQuote.AddXPathProperty("symbol_a", "//m0:symbol", XPathResultType.String);
            stockQuote.AddXPathProperty(
                "symbol_b",
                "//*[local-name(.) = 'getQuote' and namespace-uri(.) = 'http://services.samples/xsd']",
                XPathResultType.String);
            stockQuote.AddXPathProperty("symbol_c", "/m0:getQuote/m0:request/m0:symbol", XPathResultType.String);
            stockQuote.RootElementName = "getQuote";
            stockQuote.DefaultNamespace = "http://services.samples/xsd";
            stockQuote.RootElementNamespace = "http://services.samples/xsd";
            stockQuote.AddNamespacePrefix("m0", "http://services.samples/xsd");
            stockQuote.IsXPathResolvePropertiesAbsolute = true;
            stockQuote.IsXPathPropertyExpr = true;
            configuration.Common.AddEventType("StockQuote", stockQuote);

            var stockQuoteSimpleConfig = new ConfigurationCommonEventTypeXMLDOM();
            stockQuoteSimpleConfig.RootElementName = "getQuote";
            stockQuoteSimpleConfig.DefaultNamespace = "http://services.samples/xsd";
            stockQuoteSimpleConfig.RootElementNamespace = "http://services.samples/xsd";
            stockQuoteSimpleConfig.AddNamespacePrefix("m0", "http://services.samples/xsd");
            stockQuoteSimpleConfig.IsXPathResolvePropertiesAbsolute = false;
            stockQuoteSimpleConfig.IsXPathPropertyExpr = true;
            configuration.Common.AddEventType("StockQuoteSimpleConfig", stockQuoteSimpleConfig);

            var testXMLNoSchemaType = new ConfigurationCommonEventTypeXMLDOM();
            testXMLNoSchemaType.RootElementName = "myevent";
            testXMLNoSchemaType.IsXPathPropertyExpr = false; // <== DOM getter
            configuration.Common.AddEventType("TestXMLNoSchemaType", testXMLNoSchemaType);

            var testXMLNoSchemaTypeWXPathPropTrue = new ConfigurationCommonEventTypeXMLDOM();
            testXMLNoSchemaTypeWXPathPropTrue.RootElementName = "myevent";
            testXMLNoSchemaTypeWXPathPropTrue.IsXPathPropertyExpr = true; // <== XPath getter
            configuration.Common.AddEventType("TestXMLNoSchemaTypeWXPathPropTrue", testXMLNoSchemaTypeWXPathPropTrue);

            var xmlDOMEventTypeDesc = new ConfigurationCommonEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.AddXPathProperty("xpathElement1", "/myevent/element1", XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty(
                "xpathCountE21",
                "count(/myevent/element2/element21)",
                XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty(
                "xpathAttrString",
                "/myevent/element3/@attrString",
                XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNum", "/myevent/element3/@attrNum", XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty(
                "xpathAttrBool",
                "/myevent/element3/@attrBool",
                XPathResultType.Boolean);
            xmlDOMEventTypeDesc.AddXPathProperty(
                "stringCastLong",
                "/myevent/element3/@attrNum",
                XPathResultType.String,
                "long");
            xmlDOMEventTypeDesc.AddXPathProperty(
                "stringCastDouble",
                "/myevent/element3/@attrNum",
                XPathResultType.String,
                "double");
            xmlDOMEventTypeDesc.AddXPathProperty(
                "numCastInt",
                "/myevent/element3/@attrNum",
                XPathResultType.Number,
                "int");
            xmlDOMEventTypeDesc.XPathFunctionResolver = typeof(SupportXPathFunctionResolver).FullName;
            xmlDOMEventTypeDesc.XPathVariableResolver = typeof(SupportXPathVariableResolver).FullName;
            configuration.Common.AddEventType("TestXMLNoSchemaTypeWMoreXPath", xmlDOMEventTypeDesc);

            xmlDOMEventTypeDesc = new ConfigurationCommonEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "my.event2";
            configuration.Common.AddEventType("TestXMLWithDots", xmlDOMEventTypeDesc);

            var testXMLNoSchemaTypeWNum = new ConfigurationCommonEventTypeXMLDOM();
            testXMLNoSchemaTypeWNum.RootElementName = "myevent";
            testXMLNoSchemaTypeWNum.AddXPathProperty(
                "xpathAttrNum",
                "/myevent/@attrnum",
                XPathResultType.String,
                "long");
            testXMLNoSchemaTypeWNum.AddXPathProperty(
                "xpathAttrNumTwo",
                "/myevent/@attrnumtwo",
                XPathResultType.String,
                "long");
            configuration.Common.AddEventType("TestXMLNoSchemaTypeWNum", testXMLNoSchemaTypeWNum);

            var @event = new ConfigurationCommonEventTypeXMLDOM();
            @event.RootElementName = "Event";
            @event.AddXPathProperty("A", "//Field[@Name='A']/@Value", XPathResultType.NodeSet, "String[]");
            configuration.Common.AddEventType("Event", @event);

            configuration.Common.AddEventType(
                "XMLSchemaConfigOne",
                GetConfigTestType(null, true, schemaUriSimpleSchema));
            configuration.Common.AddEventType(
                "XMLSchemaConfigTwo",
                GetConfigTestType(null, false, schemaUriSimpleSchema));

            var typecfg = new ConfigurationCommonEventTypeXMLDOM();
            typecfg.RootElementName = "Sensor";
            typecfg.SchemaResource = schemaUriSensorEvent;
            configuration.Compiler.ViewResources.IterableUnbound = true;
            configuration.Common.AddEventType("SensorEvent", typecfg);

            var sensorcfg = new ConfigurationCommonEventTypeXMLDOM();
            sensorcfg.RootElementName = "Sensor";
            sensorcfg.AddXPathProperty("countTags", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number);
            sensorcfg.AddXPathProperty("countTagsInt", "count(/ss:Sensor/ss:Observation/ss:Tag)", XPathResultType.Number, "int");
            sensorcfg.AddNamespacePrefix("ss", "SensorSchema");
            sensorcfg.AddXPathProperty("idarray", "//ss:Tag/ss:ID", XPathResultType.NodeSet, "String[]");
            sensorcfg.AddXPathPropertyFragment("tagArray", "//ss:Tag", XPathResultType.NodeSet, "TagEvent");
            sensorcfg.AddXPathPropertyFragment("tagOne", "//ss:Tag[position() = 1]", XPathResultType.Any, "TagEvent");
            sensorcfg.SchemaResource = schemaUriSensorEvent;
            configuration.Common.AddEventType("SensorEventWithXPath", sensorcfg);

            var tagcfg = new ConfigurationCommonEventTypeXMLDOM();
            tagcfg.RootElementName = "//Tag";
            tagcfg.SchemaResource = schemaUriSensorEvent;
            configuration.Common.AddEventType("TagEvent", tagcfg);

            var eventABC = new ConfigurationCommonEventTypeXMLDOM();
            eventABC.RootElementName = "a";
            eventABC.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.Common.AddEventType("EventABC", eventABC);

            var bEvent = new ConfigurationCommonEventTypeXMLDOM();
            bEvent.RootElementName = "a";
            bEvent.AddXPathProperty("element2", "//c", XPathResultType.String);
            bEvent.IsEventSenderValidatesRoot = false;
            configuration.Common.AddEventType("BEvent", bEvent);

            var simpleEventWSchema = new ConfigurationCommonEventTypeXMLDOM();
            simpleEventWSchema.RootElementName = "simpleEvent";
            simpleEventWSchema.SchemaResource = schemaUriSimpleSchema;
            // eventTypeMeta.setXPathPropertyExpr(false); <== the default
            configuration.Common.AddEventType("SimpleEventWSchema", simpleEventWSchema);

            var abcType = new ConfigurationCommonEventTypeXMLDOM();
            abcType.RootElementName = "simpleEvent";
            abcType.SchemaResource = schemaUriSimpleSchema;
            configuration.Common.AddEventType("ABCType", abcType);

            var testNested2 = new ConfigurationCommonEventTypeXMLDOM();
            testNested2.RootElementName = "//nested2";
            testNested2.SchemaResource = schemaUriSimpleSchema;
            testNested2.IsEventSenderValidatesRoot = false;
            configuration.Common.AddEventType("TestNested2", testNested2);

            var myXMLEventXPC = new ConfigurationCommonEventTypeXMLDOM();
            myXMLEventXPC.RootElementName = "simpleEvent";
            myXMLEventXPC.SchemaResource = schemaUriSimpleSchema;
            myXMLEventXPC.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            myXMLEventXPC.AddXPathPropertyFragment(
                "nested1simple",
                "/ss:simpleEvent/ss:nested1",
                XPathResultType.Any,
                "MyNestedEventXPC");
            myXMLEventXPC.AddXPathPropertyFragment(
                "nested4array",
                "//ss:nested4",
                XPathResultType.NodeSet,
                "MyNestedArrayEventXPC");
            myXMLEventXPC.IsAutoFragment = false;
            configuration.Common.AddEventType("MyXMLEventXPC", myXMLEventXPC);

            var myNestedEventXPC = new ConfigurationCommonEventTypeXMLDOM();
            myNestedEventXPC.RootElementName = "//nested1";
            myNestedEventXPC.SchemaResource = schemaUriSimpleSchema;
            myNestedEventXPC.IsAutoFragment = false;
            configuration.Common.AddEventType("MyNestedEventXPC", myNestedEventXPC);

            var myNestedArrayEventXPC = new ConfigurationCommonEventTypeXMLDOM();
            myNestedArrayEventXPC.RootElementName = "//nested4";
            myNestedArrayEventXPC.SchemaResource = schemaUriSimpleSchema;
            configuration.Common.AddEventType("MyNestedArrayEventXPC", myNestedArrayEventXPC);

            var testXMLSchemaTypeWithSS = new ConfigurationCommonEventTypeXMLDOM();
            testXMLSchemaTypeWithSS.RootElementName = "simpleEvent";
            testXMLSchemaTypeWithSS.SchemaResource = schemaUriSimpleSchema;
            testXMLSchemaTypeWithSS.IsXPathPropertyExpr = true; // <== note this
            testXMLSchemaTypeWithSS.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            configuration.Common.AddEventType("TestXMLSchemaTypeWithSS", testXMLSchemaTypeWithSS);

            var testTypesEvent = new ConfigurationCommonEventTypeXMLDOM();
            testTypesEvent.RootElementName = "typesEvent";
            testTypesEvent.SchemaResource = schemaUriTypeTestSchema;
            configuration.Common.AddEventType("TestTypesEvent", testTypesEvent);

            var myEventWithPrefix = new ConfigurationCommonEventTypeXMLDOM();
            myEventWithPrefix.RootElementName = "simpleEvent";
            myEventWithPrefix.SchemaResource = schemaUriSimpleSchema;
            myEventWithPrefix.IsXPathPropertyExpr = false;
            myEventWithPrefix.IsEventSenderValidatesRoot = false;
            myEventWithPrefix.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            myEventWithPrefix.DefaultNamespace = "samples:schemas:simpleSchema";
            configuration.Common.AddEventType("MyEventWithPrefix", myEventWithPrefix);

            var myEventWithXPath = new ConfigurationCommonEventTypeXMLDOM();
            myEventWithXPath.RootElementName = "simpleEvent";
            myEventWithXPath.SchemaResource = schemaUriSimpleSchema;
            myEventWithXPath.IsXPathPropertyExpr = true;
            myEventWithXPath.IsEventSenderValidatesRoot = false;
            myEventWithXPath.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            myEventWithXPath.DefaultNamespace = "samples:schemas:simpleSchema";
            configuration.Common.AddEventType("MyEventWithXPath", myEventWithXPath);

            var pageVisitEvent = new ConfigurationCommonEventTypeXMLDOM();
            pageVisitEvent.RootElementName = "event-page-visit";
            pageVisitEvent.SchemaResource = schemaUriSimpleSchemaWithAll;
            pageVisitEvent.AddNamespacePrefix("ss", "samples:schemas:simpleSchemaWithAll");
            pageVisitEvent.AddXPathProperty("url", "/ss:event-page-visit/ss:url", XPathResultType.String);
            configuration.Common.AddEventType("PageVisitEvent", pageVisitEvent);

            var orderEvent = new ConfigurationCommonEventTypeXMLDOM();
            orderEvent.RootElementName = "order";
            orderEvent.SchemaText = schemaTextSimpleSchemaWithRestriction;
            configuration.Common.AddEventType("OrderEvent", orderEvent);
        }

        protected static ConfigurationCommonEventTypeXMLDOM GetConfigTestType(
            string additionalXPathProperty,
            bool isUseXPathPropertyExpression,
            string schemaUriSimpleSchema)
        {
            var eventTypeMeta = new ConfigurationCommonEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            eventTypeMeta.SchemaResource = schemaUriSimpleSchema;
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            eventTypeMeta.AddXPathProperty(
                "customProp",
                "count(/ss:simpleEvent/ss:nested3/ss:nested4)",
                XPathResultType.Number);
            eventTypeMeta.IsXPathPropertyExpr = isUseXPathPropertyExpression;
            if (additionalXPathProperty != null)
            {
                eventTypeMeta.AddXPathProperty(
                    additionalXPathProperty,
                    "count(/ss:simpleEvent/ss:nested3/ss:nested4)",
                    XPathResultType.Number);
            }

            return eventTypeMeta;
        }

        [Test]
        public void TestEventXMLNoSchemaDotEscape()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaDotEscape());
        }

        [Test]
        public void TestEventXMLNoSchemaElementNode()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaElementNode());
        }

        [Test]
        public void TestEventXMLNoSchemaEventTransposeDOM()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaEventTransposeDOM());
        }

        [Test]
        public void TestEventXMLNoSchemaEventTransposeXPathConfigured()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaEventTransposeXPathConfigured());
        }

        [Test]
        public void TestEventXMLNoSchemaEventTransposeXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaEventTransposeXPathGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaEventXML()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaEventXML());
        }

        [Test]
        public void TestEventXMLNoSchemaNamespaceXPathAbsolute()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaNamespaceXPathAbsolute());
        }

        [Test]
        public void TestEventXMLNoSchemaNamespaceXPathRelative()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaNamespaceXPathRelative());
        }

        [Test]
        public void TestEventXMLNoSchemaNestedXMLDOMGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaNestedXMLDOMGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaNestedXMLXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaNestedXMLXPathGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaPropertyDynamicDOMGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaPropertyDynamicDOMGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaPropertyDynamicXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaPropertyDynamicXPathGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaSimpleXMLDOMGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaSimpleXMLDOMGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaSimpleXMLXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaSimpleXMLXPathGetter());
        }

        [Test]
        public void TestEventXMLNoSchemaSimpleXMLXPathProperties()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaSimpleXMLXPathProperties());
        }

        [Test]
        public void TestEventXMLNoSchemaVariableAndDotMethodResolution()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaVariableAndDotMethodResolution());
        }

        [Test]
        public void TestEventXMLNoSchemaXPathArray()
        {
            RegressionRunner.Run(session, new EventXMLNoSchemaXPathArray());
        }

        [Test]
        public void TestEventXMLSchemaDOMGetterBacked()
        {
            RegressionRunner.Run(session, new EventXMLSchemaDOMGetterBacked());
        }

        [Test]
        public void TestEventXMLSchemaEventObservationDOM()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventObservationDOM());
        }

        [Test]
        public void TestEventXMLSchemaEventObservationXPath()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventObservationXPath());
        }

        [Test]
        public void TestEventXMLSchemaEventSender()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventSender());
        }

        [Test]
        public void TestEventXMLSchemaEventTransposeDOMGetter()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTransposeDOMGetter());
        }

        [Test]
        public void TestEventXMLSchemaEventTransposeNodeArray()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTransposeNodeArray());
        }

        [Test]
        public void TestEventXMLSchemaEventTransposePrimitiveArray()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTransposePrimitiveArray());
        }

        [Test]
        public void TestEventXMLSchemaEventTransposeXPathConfigured()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTransposeXPathConfigured());
        }

        [Test]
        public void TestEventXMLSchemaEventTransposeXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTransposeXPathGetter());
        }

        [Test]
        public void TestEventXMLSchemaEventTypes()
        {
            RegressionRunner.Run(session, new EventXMLSchemaEventTypes());
        }

        [Test]
        public void TestEventXMLSchemaInvalid()
        {
            RegressionRunner.Run(session, new EventXMLSchemaInvalid());
        }

        [Test]
        public void TestEventXMLSchemaPropertyDynamicDOMGetter()
        {
            RegressionRunner.Run(session, new EventXMLSchemaPropertyDynamicDOMGetter());
        }

        [Test]
        public void TestEventXMLSchemaPropertyDynamicXPathGetter()
        {
            RegressionRunner.Run(session, new EventXMLSchemaPropertyDynamicXPathGetter());
        }

        [Test]
        public void TestEventXMLSchemaWithAll()
        {
            RegressionRunner.Run(session, new EventXMLSchemaWithAll());
        }

        [Test]
        public void TestEventXMLSchemaWithRestriction()
        {
            RegressionRunner.Run(session, new EventXMLSchemaWithRestriction());
        }

        [Test]
        public void TestEventXMLSchemaXPathBacked()
        {
            RegressionRunner.Run(session, new EventXMLSchemaXPathBacked());
        }
    }
} // end of namespace