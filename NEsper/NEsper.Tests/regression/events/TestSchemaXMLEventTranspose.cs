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
using com.espertech.esper.events.xml;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestSchemaXMLEventTranspose
    {
        private const String CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";

        private EPServiceProvider _epService;
        private String _schemaURI;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _schemaURI = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            _listener = new SupportUpdateListener();
    
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestXPathConfigured()
        {
            var rootMeta = new ConfigurationEventTypeXMLDOM();
            rootMeta.RootElementName = "simpleEvent";
            rootMeta.SchemaResource = _schemaURI;
            rootMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            rootMeta.AddXPathPropertyFragment("nested1simple", "/ss:simpleEvent/ss:nested1", XPathResultType.Any, "MyNestedEvent");
            rootMeta.AddXPathPropertyFragment("nested4array", "//ss:nested4", XPathResultType.NodeSet, "MyNestedArrayEvent");
            rootMeta.IsAutoFragment = false;
            _epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", rootMeta);
    
            var metaNested = new ConfigurationEventTypeXMLDOM();
            metaNested.RootElementName = "//nested1";
            metaNested.SchemaResource = _schemaURI;
            metaNested.IsAutoFragment = false;
            _epService.EPAdministrator.Configuration.AddEventType("MyNestedEvent", metaNested);
    
            var metaNestedArray = new ConfigurationEventTypeXMLDOM();
            metaNestedArray.RootElementName = "//nested4";
            metaNestedArray.SchemaResource = _schemaURI;
            _epService.EPAdministrator.Configuration.AddEventType("MyNestedArrayEvent", metaNestedArray);
    
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into Nested3Stream select nested1simple, nested4array from MyXMLEvent.std:lastevent()");
            var stmtWildcard = _epService.EPAdministrator.CreateEPL("select * from MyXMLEvent.std:lastevent()");
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
            EventTypeAssertionUtil.AssertConsistency(stmtWildcard.EventType);
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1simple", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("nested4array", typeof(XmlNode[]), typeof(XmlNode), false, false, true, false, true)
            }, stmtInsert.EventType.PropertyDescriptors);
    
            var fragmentTypeNested1 = stmtInsert.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(fragmentTypeNested1.IsIndexed);
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, false)
            }, fragmentTypeNested1.FragmentType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);
    
            var fragmentTypeNested4 = stmtInsert.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop5", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("id", typeof(string), typeof(char), false, false, true, false, false)
            }, fragmentTypeNested4.FragmentType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);
    
            var fragmentTypeNested4Item = stmtInsert.EventType.GetFragmentType("nested4array[0]");
            Assert.IsFalse(fragmentTypeNested4Item.IsIndexed);
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop5", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("id", typeof(string), typeof(char), false, false, true, false, false)
            }, fragmentTypeNested4Item.FragmentType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4Item.FragmentType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "ABC");
    
            var received = stmtInsert.First();
            EPAssertionUtil.AssertProps(received, "nested1simple.prop1,nested1simple.prop2,nested1simple.attr1,nested1simple.nested2.prop3[1]".Split(','), new Object[]{"SAMPLE_V1", true, "SAMPLE_ATTR1", 4});
            EPAssertionUtil.AssertProps(received, "nested4array[0].id,nested4array[0].prop5[1],nested4array[1].id".Split(','), new Object[]{"a", "SAMPLE_V8", "b"});
    
            // assert event and fragments alone
            var wildcardStmtEvent = stmtWildcard.First();
            EventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);
    
            var eventType = wildcardStmtEvent.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedEvent", eventType.FragmentType.Name);
            Assert.IsTrue(wildcardStmtEvent.Get("nested1simple") is XmlNode);
            Assert.AreEqual("SAMPLE_V1", ((EventBean) wildcardStmtEvent.GetFragment("nested1simple")).Get("prop1"));
    
            eventType = wildcardStmtEvent.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedArrayEvent", eventType.FragmentType.Name);
            var eventsArray = (EventBean[]) wildcardStmtEvent.GetFragment("nested4array");
            Assert.AreEqual(3, eventsArray.Length);
            Assert.AreEqual("SAMPLE_V8", eventsArray[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V9", eventsArray[1].Get("prop5[0]"));
            Assert.AreEqual(typeof(XmlNodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
            Assert.IsTrue(wildcardStmtEvent.Get("nested4array") is XmlNodeList);
    
            var nested4arrayItem = (EventBean) wildcardStmtEvent.GetFragment("nested4array[1]");
            Assert.AreEqual("b", nested4arrayItem.Get("id"));
        }
    
        [Test]
        public void TestExpressionSimpleDOMGetter()
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            var schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            // eventTypeMeta.XPathPropertyExpr = false; <== the default
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);

            var stmtSelect = _epService.EPAdministrator.CreateEPL("select nested1.attr1 as attr1, nested1.prop1 as prop1, nested1.prop2 as prop2, nested1.nested2.prop3 as prop3, nested1.nested2.prop3[0] as prop3_0, nested1.nested2 as nested2 from MyNestedStream.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop3", typeof(int?[]), typeof(int?), false, false, true, false, false),
                    new EventPropertyDescriptor("prop3_0", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true)
            }, stmtSelect.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtSelect.EventType);
    
            var stmtSelectWildcard = _epService.EPAdministrator.CreateEPL("select * from MyNestedStream");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true)
            }, stmtSelectWildcard.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtSelectWildcard.EventType);

            var stmtInsertWildcard = _epService.EPAdministrator.CreateEPL("insert into MyNestedStreamTwo select nested1.* from TestXMLSchemaType.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true)
            }, stmtInsertWildcard.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsertWildcard.EventType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
            var stmtInsertWildcardBean = stmtInsertWildcard.First();
            EPAssertionUtil.AssertProps(stmtInsertWildcardBean, "prop1,prop2,attr1".Split(','),
                    new Object[]{"SAMPLE_V1", true, "SAMPLE_ATTR1"});
    
            EventTypeAssertionUtil.AssertConsistency(stmtSelect.First());
            var stmtInsertBean = stmtInsert.First();
            EventTypeAssertionUtil.AssertConsistency(stmtInsertWildcard.First());
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
    
            var fragmentNested1 = (EventBean) stmtInsertBean.GetFragment("nested1");
            Assert.AreEqual(5, fragmentNested1.Get("nested2.prop3[2]"));
            Assert.AreEqual("TestXMLSchemaType.nested1", fragmentNested1.EventType.Name);
    
            var fragmentNested2 = (EventBean) stmtInsertWildcardBean.GetFragment("nested2");
            Assert.AreEqual(4, fragmentNested2.Get("prop3[1]"));
            Assert.AreEqual("TestXMLSchemaType.nested1.nested2", fragmentNested2.EventType.Name);
        }
    
        // Note that XPath Node results when transposed must be queried by XPath that is also absolute.
        // For example: "nested1" => "/n0:simpleEvent/n0:nested1" results in a Node.
        // That result Node's "prop1" =>  "/n0:simpleEvent/n0:nested1/n0:prop1" and "/n0:nested1/n0:prop1" does NOT result in a value.
        // Therefore property transposal is disabled for Property-XPath expressions.
        [Test]
        public void TestExpressionSimpleXPathGetter()
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            var schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            eventTypeMeta.IsXPathPropertyExpr = true;       // <== note this
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // note class not a fragment
            var stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false)
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            var type = ((EPServiceProviderSPI) _epService).EventAdapterService.GetEventTypeByName("TestXMLSchemaType");
            EventTypeAssertionUtil.AssertConsistency(type);
            Assert.IsNull(type.GetFragmentType("nested1"));
            Assert.IsNull(type.GetFragmentType("nested1.nested2"));
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "ABC");
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
        }
    
        [Test]
        public void TestExpressionNodeArray()
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            var schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // try array property insert
            var stmtInsert = _epService.EPAdministrator.CreateEPL("select nested3.nested4 as narr from TestXMLSchemaType.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("narr", typeof(XmlNode[]), typeof(XmlNode), false, false, true, false, true)
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
    
            var result = stmtInsert.First();
            EventTypeAssertionUtil.AssertConsistency(result);
            var fragments = (EventBean[]) result.GetFragment("narr");
            Assert.AreEqual(3, fragments.Length);
            Assert.AreEqual("SAMPLE_V8", fragments[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", fragments[2].Get("prop5[1]"));
    
            var fragmentItem = (EventBean) result.GetFragment("narr[2]");
            Assert.AreEqual("TestXMLSchemaType.nested3.nested4", fragmentItem.EventType.Name);
            Assert.AreEqual("SAMPLE_V10", fragmentItem.Get("prop5[0]"));
    
            // try array index property insert
            var stmtInsertItem = _epService.EPAdministrator.CreateEPL("select nested3.nested4[1] as narr from TestXMLSchemaType.std:lastevent()");
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("narr", typeof(XmlNode), null, false, false, false, false, true)
            }, stmtInsertItem.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsertItem.EventType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
    
            var resultItem = stmtInsertItem.First();
            Assert.AreEqual("b", resultItem.Get("narr.id"));
            EventTypeAssertionUtil.AssertConsistency(resultItem);
            var fragmentsInsertItem = (EventBean) resultItem.GetFragment("narr");
            EventTypeAssertionUtil.AssertConsistency(fragmentsInsertItem);
            Assert.AreEqual("b", fragmentsInsertItem.Get("id"));
            Assert.AreEqual("SAMPLE_V9", fragmentsInsertItem.Get("prop5[0]"));
        }
    
        [Test]
        public void TestExpressionPrimitiveArray()
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            eventTypeMeta.SchemaResource = _schemaURI;
            _epService.EPAdministrator.Configuration.AddEventType("ABCType", eventTypeMeta);
    
            eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "//nested2";
            eventTypeMeta.SchemaResource = _schemaURI;
            eventTypeMeta.IsEventSenderValidatesRoot = false;
            _epService.EPAdministrator.Configuration.AddEventType("TestNested2", eventTypeMeta);
    
            // try array property in select
            var stmtInsert = _epService.EPAdministrator.CreateEPL("select * from TestNested2.std:lastevent()");
            stmtInsert.Events += _listener.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop3", typeof(int?[]), typeof(int?), false, false, true, false, false)
            }, stmtInsert.EventType.PropertyDescriptors);
            EventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
            Assert.IsFalse(_listener.IsInvoked);
    
            var sender = _epService.EPRuntime.GetEventSender("TestNested2");
            sender.SendEvent(SupportXML.GetDocument("<nested2><prop3>2</prop3><prop3></prop3><prop3>4</prop3></nested2>"));
            var theEvent = stmtInsert.First();
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("prop3"), new int?[]{2, null, 4});
            EventTypeAssertionUtil.AssertConsistency(theEvent);
    
            // try array property nested
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select nested3.* from ABCType.std:lastevent()");
            SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");
            var stmtSelectResult = stmtSelect.First();
            EventTypeAssertionUtil.AssertConsistency(stmtSelectResult);
            Assert.AreEqual(typeof(String[]), stmtSelectResult.EventType.GetPropertyType("nested4[2].prop5"));
            Assert.AreEqual("SAMPLE_V8", stmtSelectResult.Get("nested4[0].prop5[1]"));
            EPAssertionUtil.AssertEqualsExactOrder((String[]) stmtSelectResult.Get("nested4[2].prop5"), new Object[]{"SAMPLE_V10", "SAMPLE_V11"});
    
            var fragmentNested4 = (EventBean) stmtSelectResult.GetFragment("nested4[2]");
            EPAssertionUtil.AssertEqualsExactOrder((String[]) fragmentNested4.Get("prop5"), new Object[]{"SAMPLE_V10", "SAMPLE_V11"});
            Assert.AreEqual("SAMPLE_V11", fragmentNested4.Get("prop5[1]"));
            EventTypeAssertionUtil.AssertConsistency(fragmentNested4);
        }
    
        /// <summary>For testing XPath expressions. </summary>
        [Test]
        public void TestXPathExpression()
        {
            var ctx = new XPathNamespaceContext();
            ctx.AddNamespace("n0", "samples:schemas:simpleSchema");
    
            var node = SupportXML.GetDocument().DocumentElement;

            var pathExprOne = XPathExpression.Compile(
                "/n0:simpleEvent/n0:nested1",
                ctx);

            var nav = node.CreateNavigator();
            var iterator = (XPathNodeIterator) nav.Evaluate(pathExprOne);
            Assert.AreEqual(iterator.Count, 1);
            Assert.IsTrue(iterator.MoveNext());
            var result = ((IHasXmlNode) iterator.Current).GetNode();
            Assert.IsNotNull(result);

            //Console.WriteLine("Result:\n" + SchemaUtil.Serialize(result));
    
            var pathExprTwo = XPathExpression.Compile("/n0:simpleEvent/n0:nested1/n0:prop1", ctx);
            iterator = (XPathNodeIterator) nav.Evaluate(pathExprTwo);
            Assert.AreEqual(iterator.Count, 1);
            Assert.IsTrue(iterator.MoveNext());
            var resultTwo = (string) iterator.Current.TypedValue;
            //Console.WriteLine("Result 2: <" + resultTwo + ">");

            var pathExprThree = XPathExpression.Compile("/n0:simpleEvent/n0:nested3", ctx);
            iterator = (XPathNodeIterator)nav.Evaluate(pathExprThree);
            Assert.AreEqual(iterator.Count, 1);
            Assert.IsTrue(iterator.MoveNext());
            var resultThree = (string)iterator.Current.TypedValue;
            //Console.WriteLine("Result 3: <" + resultThress + ">");
        }
    }
}
