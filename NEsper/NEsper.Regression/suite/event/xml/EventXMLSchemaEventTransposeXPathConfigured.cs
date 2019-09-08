///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeXPathConfigured : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionXPathConfigured(env);
            RunAssertionXPathExpression();
        }

        private void RunAssertionXPathConfigured(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@Name('insert') insert into Nested3Stream select nested1simple, nested4array from MyXMLEventXPC#lastevent");
            env.CompileDeploy("@Name('sw') select * from MyXMLEventXPC#lastevent");
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("insert").EventType);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("sw").EventType);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
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
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, false)
                },
                fragmentTypeNested1.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);

            var fragmentTypeNested4 = env.Statement("insert").EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop5", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("Id", typeof(string), null, false, false, false, false, false)
                },
                fragmentTypeNested4.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);

            var fragmentTypeNested4Item = env.Statement("insert").EventType.GetFragmentType("nested4array[0]");
            Assert.IsFalse(fragmentTypeNested4Item.IsIndexed);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop5", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), null, false, false, true, false, false),
                    new EventPropertyDescriptor("Id", typeof(string), null, false, false, false, false, false)
                },
                fragmentTypeNested4Item.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4Item.FragmentType);

            SupportXML.SendDefaultEvent(env.EventService, "ABC", "MyXMLEventXPC");

            var received = env.Statement("insert").First();
            EPAssertionUtil.AssertProps(
                received,
                new [] { "nested1simple.prop1","nested1simple.prop2","nested1simple.attr1","nested1simple.Nested2.prop3[1]" },
                new object[] {"SAMPLE_V1", true, "SAMPLE_ATTR1", 4});
            EPAssertionUtil.AssertProps(
                received,
                new [] { "nested4array[0].Id","nested4array[0].prop5[1]","nested4array[1].Id" },
                new object[] {"a", "SAMPLE_V8", "b"});

            // assert event and fragments alone
            var wildcardStmtEvent = env.Statement("sw").First();
            SupportEventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);

            var eventType = wildcardStmtEvent.EventType.GetFragmentType("nested1simple");
            Assert.IsFalse(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedEventXPC", eventType.FragmentType.Name);
            Assert.IsTrue(wildcardStmtEvent.Get("nested1simple") is XmlNode);
            Assert.AreEqual("SAMPLE_V1", ((EventBean) wildcardStmtEvent.GetFragment("nested1simple")).Get("prop1"));

            eventType = wildcardStmtEvent.EventType.GetFragmentType("nested4array");
            Assert.IsTrue(eventType.IsIndexed);
            Assert.IsFalse(eventType.IsNative);
            Assert.AreEqual("MyNestedArrayEventXPC", eventType.FragmentType.Name);
            var eventsArray = (EventBean[]) wildcardStmtEvent.GetFragment("nested4array");
            Assert.AreEqual(3, eventsArray.Length);
            Assert.AreEqual("SAMPLE_V8", eventsArray[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V9", eventsArray[1].Get("prop5[0]"));
            Assert.AreEqual(typeof(NodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
            Assert.IsTrue(wildcardStmtEvent.Get("nested4array") is NodeList);

            var nested4arrayItem = (EventBean) wildcardStmtEvent.GetFragment("nested4array[1]");
            Assert.AreEqual("b", nested4arrayItem.Get("Id"));

            env.UndeployAll();
        }

        private void RunAssertionXPathExpression()
        {
            try {
                var ctx = new XPathNamespaceContext();
                ctx.AddNamespace("n0", "samples:schemas:simpleSchema");

                XmlNode node = SupportXML.GetDocument().DocumentElement;

                var pathExprOne = XPathExpression.Compile("/n0:simpleEvent/n0:nested1", ctx);
                var pathExprOneResult = node.CreateNavigator().Evaluate(pathExprOne);
                Assert.That(pathExprOne.ReturnType, Is.EqualTo(XPathResultType.Any));
                Assert.That(pathExprOneResult, Is.InstanceOf<XmlNode>());
                //System.out.println("Result:\n" + SchemaUtil.serialize(result));

                var pathExprOneNode = (XmlNode) pathExprOneResult;

                var pathExprTwo = XPathExpression.Compile("/n0:simpleEvent/n0:nested1/n0:prop1", ctx);
                var pathExprTwoResult = pathExprOneNode.CreateNavigator().Evaluate(pathExprTwo);
                Assert.That(pathExprTwo.ReturnType, Is.EqualTo(XPathResultType.String));
                Assert.That(pathExprTwoResult, Is.InstanceOf<string>());
                //System.out.println("Result 2: <" + resultTwo + ">");

                var pathExprThree = XPathExpression.Compile("/n0:simpleEvent/n0:nested3", ctx);
                var pathExprThreeResult = pathExprOneNode.CreateNavigator().Evaluate(pathExprThree);
                Assert.That(pathExprThree.ReturnType, Is.EqualTo(XPathResultType.String));
                Assert.That(pathExprThreeResult, Is.InstanceOf<string>());
                //System.out.println("Result 3: <" + resultThress + ">");
            }
            catch (Exception t) {
                Assert.Fail();
            }
        }
    }
} // end of namespace