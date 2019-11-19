///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeXPathConfigured : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@Name('insert') insert into Nested3Stream select nested1simple, nested4array from MyXMLEvent");
            env.CompileDeploy("@Name('s0') select * from MyXMLEvent");
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("insert").EventType);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor(
                        "nested1simple",
                        typeof(XmlNode),
                        null,
                        false,
                        false,
                        false,
                        false,
                        true),
                    new EventPropertyDescriptor(
                        "nested4array",
                        typeof(XmlNode[]),
                        typeof(XmlNode),
                        false,
                        false,
                        true,
                        false,
                        true)
                },
                env.Statement("insert").EventType.PropertyDescriptors);

            var fragmentTypeNested1 = env.Statement("insert").EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(fragmentTypeNested1.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested1.FragmentType.PropertyDescriptors.Count);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);

            var fragmentTypeNested4 = env.Statement("insert").EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            Assert.AreEqual(0, fragmentTypeNested4.FragmentType.PropertyDescriptors.Count);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);

            SupportXML.SendDefaultEvent(env.EventService, "ABC", "MyXMLEvent");

            var received = env.GetEnumerator("insert").Advance();
            EPAssertionUtil.AssertProps(
                received,
                new[] { "nested1simple.prop1", "nested1simple.prop2", "nested1simple.attr1", "nested1simple.nested2.prop3[1]" },
                new object[] { "SAMPLE_V1", "true", "SAMPLE_ATTR1", "4" });
            EPAssertionUtil.AssertProps(
                received,
                new[] { "nested4array[0].id", "nested4array[0].prop5[1]", "nested4array[1].id" },
                new object[] { "a", "SAMPLE_V8", "b" });

            // assert event and fragments alone
            var wildcardStmtEventEnum = env.GetEnumerator("s0");
            Assert.That(wildcardStmtEventEnum.MoveNext(), Is.True);

            var wildcardStmtEvent = wildcardStmtEventEnum.Current;
            SupportEventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);

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

            env.UndeployAll();
        }
    }
} // end of namespace