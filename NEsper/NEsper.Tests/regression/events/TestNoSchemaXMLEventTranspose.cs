///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestNoSchemaXMLEventTranspose
    {
        private static String CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsIterableUnbound = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [Test]
        public void TestXPathConfigured() {
            ConfigurationEventTypeXMLDOM rootMeta = new ConfigurationEventTypeXMLDOM();
            rootMeta.RootElementName = "simpleEvent";
            rootMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            rootMeta.AddXPathPropertyFragment("nested1simple", "/ss:simpleEvent/ss:nested1", XPathResultType.Any, "MyNestedEvent");
            rootMeta.AddXPathPropertyFragment("nested4array", "//ss:nested4", XPathResultType.NodeSet, "MyNestedArrayEvent");
            _epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", rootMeta);
    
            ConfigurationEventTypeXMLDOM metaNested = new ConfigurationEventTypeXMLDOM();
            metaNested.RootElementName = "nested1";
            _epService.EPAdministrator.Configuration.AddEventType("MyNestedEvent", metaNested);
    
            ConfigurationEventTypeXMLDOM metaNestedArray = new ConfigurationEventTypeXMLDOM();
            metaNestedArray.RootElementName = "nested4";
            _epService.EPAdministrator.Configuration.AddEventType("MyNestedArrayEvent", metaNestedArray);
    
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into Nested3Stream select nested1simple, nested4array from MyXMLEvent");
            EPStatement stmtWildcard = _epService.EPAdministrator.CreateEPL("select * from MyXMLEvent");
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
            EventTypeAssertionUtil.AssertConsistency(stmtWildcard.EventType);
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1simple", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("nested4array", typeof(XmlNode[]), typeof(XmlNode), false, false, true, false, true),
            }, stmtInsert.EventType.PropertyDescriptors);
    
            FragmentEventType fragmentTypeNested1 = stmtInsert.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(fragmentTypeNested1.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested1.FragmentType.PropertyDescriptors.Count);
            EventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);
    
            FragmentEventType fragmentTypeNested4 = stmtInsert.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested4.FragmentType.PropertyDescriptors.Count);
            EventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "ABC");
    
            EventBean received = stmtInsert.First();
            EPAssertionUtil.AssertProps(received, "nested1simple.prop1,nested1simple.prop2,nested1simple.attr1,nested1simple.nested2.prop3[1]".Split(','), new Object[]{"SAMPLE_V1", "true", "SAMPLE_ATTR1", "4"});
            EPAssertionUtil.AssertProps(received, "nested4array[0].id,nested4array[0].prop5[1],nested4array[1].id".Split(','), new Object[]{"a", "SAMPLE_V8", "b"});
    
            // assert event and fragments alone
            EventBean wildcardStmtEvent = stmtWildcard.First();
            EventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);
    
            FragmentEventType eventType = wildcardStmtEvent.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedEvent", eventType.FragmentType.Name);
            Assert.IsTrue(wildcardStmtEvent.Get("nested1simple") is XmlNode);
            Assert.AreEqual("SAMPLE_V1", ((EventBean) wildcardStmtEvent.GetFragment("nested1simple")).Get("prop1"));
    
            eventType = wildcardStmtEvent.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedArrayEvent", eventType.FragmentType.Name);
            EventBean[] eventsArray = (EventBean[]) wildcardStmtEvent.GetFragment("nested4array");
            Assert.AreEqual(3, eventsArray.Length);
            Assert.AreEqual("SAMPLE_V8", eventsArray[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V9", eventsArray[1].Get("prop5[0]"));
            Assert.AreEqual(typeof(XmlNodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
            Assert.IsTrue(wildcardStmtEvent.Get("nested4array") is XmlNodeList);
        }
    
        [Test]
        public void TestExpressionSimpleDOMGetter()
        {
            ConfigurationEventTypeXMLDOM eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            // eventTypeMeta.XPathPropertyExpr = false; <== the default
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1", typeof(string), typeof(char), false, false, true, false, false),
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            EPStatement stmtSelectWildcard = _epService.EPAdministrator.CreateEPL("select * from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[0], stmtSelectWildcard.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtSelectWildcard.EventType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
            EventBean stmtInsertWildcardBean = stmtInsert.First();
            EventBean stmtSelectWildcardBean = stmtSelectWildcard.First();
            Assert.NotNull(stmtInsertWildcardBean.Get("nested1"));
            EventTypeAssertionUtil.AssertConsistency(stmtSelectWildcardBean);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
    
            Assert.AreEqual(0, stmtSelectWildcardBean.EventType.PropertyNames.Length);
        }
    
        // Note that XPath Node results when transposed must be queried by XPath that is also absolute.
        // For example: "nested1" => "/n0:simpleEvent/n0:nested1" results in a Node.
        // That result Node's "prop1" =>  "/n0:simpleEvent/n0:nested1/n0:prop1" and "/n0:nested1/n0:prop1" does NOT result in a value.
        // Therefore property transposal is disabled for Property-XPath expressions.
        [Test]
        public void TestExpressionSimpleXPathGetter() {
            ConfigurationEventTypeXMLDOM eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            String schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            eventTypeMeta.IsXPathPropertyExpr = true;       // <== note this
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // note class not a fragment
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false),
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            EventType type = ((EPServiceProviderSPI) _epService).EventAdapterService.GetEventTypeByName("TestXMLSchemaType");
            EventTypeAssertionUtil.AssertConsistency(type);
            Assert.IsNull(type.GetFragmentType("nested1"));
            Assert.IsNull(type.GetFragmentType("nested1.nested2"));
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "ABC");
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
        }
    }
}
