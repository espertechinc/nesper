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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventXML : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            configuration.Compiler.ViewResources.IsIterableUnbound = true;
            configuration.Common.AddVariable("var", typeof(int), 0);

            foreach (var clazz in new[] {typeof(SupportBean)}) {
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
            configuration.Common.AddEventType("TestXMLJustRootElementType", eventTypeMeta);

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

            var testXMLSchemaTypeWithSS = new ConfigurationCommonEventTypeXMLDOM();
            testXMLSchemaTypeWithSS.RootElementName = "simpleEvent";
            testXMLSchemaTypeWithSS.SchemaResource = schemaUriSimpleSchema;
            testXMLSchemaTypeWithSS.IsXPathPropertyExpr = true; // <== note this
            testXMLSchemaTypeWithSS.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            configuration.Common.AddEventType("TestXMLSchemaTypeWithSS", testXMLSchemaTypeWithSS);

            var myEventWTypeAndUID = new ConfigurationCommonEventTypeXMLDOM();
            myEventWTypeAndUID.AddXPathProperty("event.type", "/event/@type", XPathResultType.String);
            myEventWTypeAndUID.AddXPathProperty("event.uid", "/event/@uid", XPathResultType.String);
            myEventWTypeAndUID.RootElementName = "event";
            configuration.Common.AddEventType("MyEventWTypeAndUID", myEventWTypeAndUID);

            var stockQuote = new ConfigurationCommonEventTypeXMLDOM();
            stockQuote.AddXPathProperty("symbol_a", "//m0:Symbol", XPathResultType.String);
            stockQuote.AddXPathProperty(
                "symbol_b",
                "//*[local-name(.) = 'getQuote' and namespace-uri(.) = 'http://services.samples/xsd']",
                XPathResultType.String);
            stockQuote.AddXPathProperty("symbol_c", "/m0:getQuote/m0:request/m0:Symbol", XPathResultType.String);
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

        private static ConfigurationCommonEventTypeXMLDOM GetConfigTestType(
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
            if (additionalXPathProperty != null) {
                eventTypeMeta.AddXPathProperty(
                    additionalXPathProperty,
                    "count(/ss:simpleEvent/ss:nested3/ss:nested4)",
                    XPathResultType.Number);
            }

            return eventTypeMeta;
        }

        [Test, RunInApplicationDomain]
        public void TestEventXMLSchemaInvalid()
        {
            RegressionRunner.Run(_session, new EventXMLSchemaInvalid());
        }

        [Test, RunInApplicationDomain]
        public void TestEventXMLNoSchemaVariableAndDotMethodResolution()
        {
            RegressionRunner.Run(_session, new EventXMLNoSchemaVariableAndDotMethodResolution());
        }

        [Test, RunInApplicationDomain]
        public void TestEventXMLCreateSchemaInvalid()
        {
            RegressionRunner.Run(_session, new EventXMLCreateSchemaInvalid());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaEventXML
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaEventXML.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaEventXML : AbstractTestBase
        {
            public TestEventXMLNoSchemaEventXML() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaEventXML.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaEventXML.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaEventTransposeXPathConfigured
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeXPathConfigured.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaEventTransposeXPathConfigured : AbstractTestBase
        {
            public TestEventXMLNoSchemaEventTransposeXPathConfigured() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeXPathConfigured.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeXPathConfigured.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaEventTransposeDOM
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeDOM.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaEventTransposeDOM : AbstractTestBase
        {
            public TestEventXMLNoSchemaEventTransposeDOM() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeDOM.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaEventTransposeDOM.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTypes
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTypes.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTypes : AbstractTestBase
        {
            public TestEventXMLSchemaEventTypes() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTypes.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfigured() => RegressionRunner.Run(_session, EventXMLSchemaEventTypes.WithPreconfigured());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaPropertyDynamicXPathGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicXPathGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaPropertyDynamicXPathGetter : AbstractTestBase
        {
            public TestEventXMLSchemaPropertyDynamicXPathGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicXPathGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicXPathGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventObservationDOM
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventObservationDOM.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventObservationDOM : AbstractTestBase
        {
            public TestEventXMLSchemaEventObservationDOM() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventObservationDOM.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventObservationDOM.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventObservationXPath
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventObservationXPath.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventObservationXPath : AbstractTestBase
        {
            public TestEventXMLSchemaEventObservationXPath() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventObservationXPath.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventObservationXPath.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventSender
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventSender.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventSender : AbstractTestBase
        {
            public TestEventXMLSchemaEventSender() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventSender.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventSender.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTransposeDOMGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTransposeDOMGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTransposeDOMGetter : AbstractTestBase
        {
            public TestEventXMLSchemaEventTransposeDOMGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeDOMGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeDOMGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTransposeXPathConfigured
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathConfigured.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTransposeXPathConfigured : AbstractTestBase
        {
            public TestEventXMLSchemaEventTransposeXPathConfigured() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithXPathExpression() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathConfigured.WithXPathExpression());

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathConfigured.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathConfigured.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTransposeXPathGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTransposeXPathGetter : AbstractTestBase
        {
            public TestEventXMLSchemaEventTransposeXPathGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeXPathGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTransposePrimitiveArray
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTransposePrimitiveArray.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTransposePrimitiveArray : AbstractTestBase
        {
            public TestEventXMLSchemaEventTransposePrimitiveArray() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposePrimitiveArray.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposePrimitiveArray.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaEventTransposeNodeArray
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaEventTransposeNodeArray.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaEventTransposeNodeArray : AbstractTestBase
        {
            public TestEventXMLSchemaEventTransposeNodeArray() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeNodeArray.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaEventTransposeNodeArray.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaWithRestriction
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaWithRestriction.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaWithRestriction : AbstractTestBase
        {
            public TestEventXMLSchemaWithRestriction() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaWithRestriction.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaWithRestriction.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaWithAll
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaWithAll.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaWithAll : AbstractTestBase
        {
            public TestEventXMLSchemaWithAll() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaWithAll.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaWithAll.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaDOMGetterBacked
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaDOMGetterBacked.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaDOMGetterBacked : AbstractTestBase
        {
            public TestEventXMLSchemaDOMGetterBacked() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaDOMGetterBacked.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaDOMGetterBacked.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaXPathBacked
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaXPathBacked.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaXPathBacked : AbstractTestBase
        {
            public TestEventXMLSchemaXPathBacked() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaXPathBacked.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaXPathBacked.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaSimpleXMLXPathProperties
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathProperties.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaSimpleXMLXPathProperties : AbstractTestBase
        {
            public TestEventXMLNoSchemaSimpleXMLXPathProperties() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathProperties.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathProperties.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaSimpleXMLDOMGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLDOMGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaSimpleXMLDOMGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaSimpleXMLDOMGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLDOMGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLDOMGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaSimpleXMLXPathGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaSimpleXMLXPathGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaSimpleXMLXPathGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaSimpleXMLXPathGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaNestedXMLDOMGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLDOMGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaNestedXMLDOMGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaNestedXMLDOMGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLDOMGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLDOMGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaNestedXMLXPathGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLXPathGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaNestedXMLXPathGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaNestedXMLXPathGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLXPathGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaNestedXMLXPathGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaDotEscape
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaDotEscape.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaDotEscape : AbstractTestBase
        {
            public TestEventXMLNoSchemaDotEscape() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaDotEscape.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaDotEscape.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaElementNode
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaElementNode.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaElementNode : AbstractTestBase
        {
            public TestEventXMLNoSchemaElementNode() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaElementNode.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaElementNode.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaNamespaceXPathRelative
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathRelative.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaNamespaceXPathRelative : AbstractTestBase
        {
            public TestEventXMLNoSchemaNamespaceXPathRelative() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathRelative.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathRelative.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaNamespaceXPathAbsolute
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathAbsolute.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaNamespaceXPathAbsolute : AbstractTestBase
        {
            public TestEventXMLNoSchemaNamespaceXPathAbsolute() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathAbsolute.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaNamespaceXPathAbsolute.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaXPathArray
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaXPathArray.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaXPathArray : AbstractTestBase
        {
            public TestEventXMLNoSchemaXPathArray() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaXPathArray.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaXPathArray.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaPropertyDynamicDOMGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicDOMGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaPropertyDynamicDOMGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaPropertyDynamicDOMGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicDOMGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicDOMGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLNoSchemaPropertyDynamicXPathGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicXPathGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLNoSchemaPropertyDynamicXPathGetter : AbstractTestBase
        {
            public TestEventXMLNoSchemaPropertyDynamicXPathGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicXPathGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLNoSchemaPropertyDynamicXPathGetter.WithPreconfig());
        }

        /// <summary>
        /// Auto-test(s): EventXMLSchemaPropertyDynamicDOMGetter
        /// <code>
        /// RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicDOMGetter.Executions());
        /// </code>
        /// </summary>

        public class TestEventXMLSchemaPropertyDynamicDOMGetter : AbstractTestBase
        {
            public TestEventXMLSchemaPropertyDynamicDOMGetter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCreateSchema() => RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicDOMGetter.WithCreateSchema());

            [Test, RunInApplicationDomain]
            public void WithPreconfig() => RegressionRunner.Run(_session, EventXMLSchemaPropertyDynamicDOMGetter.WithPreconfig());
        }
    }
} // end of namespace