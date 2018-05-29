///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaEventTransposeXPathConfigured : RegressionExecution
    {
        private const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            var rootMeta = new ConfigurationEventTypeXMLDOM();
            rootMeta.RootElementName = "simpleEvent";
            rootMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            rootMeta.AddXPathPropertyFragment("nested1simple", "/ss:simpleEvent/ss:nested1", XPathResultType.Any, "MyNestedEvent");
            rootMeta.AddXPathPropertyFragment("nested4array", "//ss:nested4", XPathResultType.NodeSet, "MyNestedArrayEvent");
            epService.EPAdministrator.Configuration.AddEventType("MyXMLEvent", rootMeta);
    
            var metaNested = new ConfigurationEventTypeXMLDOM();
            metaNested.RootElementName = "nested1";
            epService.EPAdministrator.Configuration.AddEventType("MyNestedEvent", metaNested);
    
            var metaNestedArray = new ConfigurationEventTypeXMLDOM();
            metaNestedArray.RootElementName = "nested4";
            epService.EPAdministrator.Configuration.AddEventType("MyNestedArrayEvent", metaNestedArray);
    
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into Nested3Stream select nested1simple, nested4array from MyXMLEvent");
            EPStatement stmtWildcard = epService.EPAdministrator.CreateEPL("select * from MyXMLEvent");
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtWildcard.EventType);
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1simple", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("nested4array", typeof(XmlNode[]), typeof(XmlNode), false, false, true, false, true),
            }, stmtInsert.EventType.PropertyDescriptors);
    
            FragmentEventType fragmentTypeNested1 = stmtInsert.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(fragmentTypeNested1.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested1.FragmentType.PropertyDescriptors.Count);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);
    
            FragmentEventType fragmentTypeNested4 = stmtInsert.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested4.FragmentType.PropertyDescriptors.Count);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "ABC");
    
            EventBean received = stmtInsert.First();
            EPAssertionUtil.AssertProps(received, "nested1simple.prop1,nested1simple.prop2,nested1simple.attr1,nested1simple.nested2.prop3[1]".Split(','), new object[]{"SAMPLE_V1", "true", "SAMPLE_ATTR1", "4"});
            EPAssertionUtil.AssertProps(received, "nested4array[0].id,nested4array[0].prop5[1],nested4array[1].id".Split(','), new object[]{"a", "SAMPLE_V8", "b"});
    
            // assert event and fragments alone
            EventBean wildcardStmtEvent = stmtWildcard.First();
            SupportEventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);
    
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
    }
} // end of namespace
