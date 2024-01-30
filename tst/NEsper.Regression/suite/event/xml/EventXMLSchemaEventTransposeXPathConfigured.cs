///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeXPathConfigured
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            WithXPathExpression(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithXPathExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredXPathExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposeXPathConfiguredPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "MyXMLEventXPC", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposeXPathConfiguredCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema =
                    resourceManager.ResolveResourceURL("regression/simpleSchema.xsd").ToString();
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                          schemaUriSimpleSchema +
                          "', AutoFragment=false)" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchema')" +
                          "@XMLSchemaField(Name='nested1simple', XPath='/ss:simpleEvent/ss:nested1', Type='ANY', EventTypeName='MyNestedEventXPC')" +
                          "@XMLSchemaField(Name='nested4array', XPath='//ss:nested4', Type='nodeset', EventTypeName='MyNestedArrayEventXPC')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        public class EventXMLSchemaEventTransposeXPathConfiguredXPathExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ctx = new XPathNamespaceContext();
                ctx.AddNamespace("n0", "samples:schemas:simpleSchema");

                var node = SupportXML.GetDocument().DocumentElement;

                var pathExprOne = XPathExpression.Compile("/n0:simpleEvent/n0:nested1", ctx);
                var pathExprOneIterator = node.CreateNavigator().Select(pathExprOne);
                Assert.That(pathExprOne.ReturnType, Is.EqualTo(XPathResultType.NodeSet));
                Assert.That(pathExprOneIterator.MoveNext(), Is.True);
                Assert.That(pathExprOneIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
                Assert.That(pathExprOneIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));
                
                //Console.WriteLine("Result:\n" + SchemaUtil.serialize(result));

                var pathExprTwo = XPathExpression.Compile("/n0:simpleEvent/n0:nested1/n0:prop1", ctx);
                var pathExprTwoIterator = node.CreateNavigator().Select(pathExprTwo);
                Assert.That(pathExprTwoIterator.MoveNext(), Is.True);
                Assert.That(pathExprTwoIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
                Assert.That(pathExprTwoIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));
                Assert.That(pathExprTwoIterator.Current.MoveToFirstChild(), Is.True);
                Assert.That(pathExprTwoIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Text));
                Assert.That(pathExprTwoIterator.Current.TypedValue, Is.EqualTo("SAMPLE_V1"));

                //Console.WriteLine("Result 2: <" + resultTwo + ">");

                var pathExprThree = XPathExpression.Compile("/n0:simpleEvent/n0:nested3", ctx);
                var pathExprThreeIterator = node.CreateNavigator().Select(pathExprThree);
                Assert.That(pathExprThreeIterator.MoveNext(), Is.True);
                Assert.That(pathExprThreeIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
                Assert.That(pathExprThreeIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));
                Assert.That(pathExprThreeIterator.Current.HasChildren, Is.True);
                
                //Console.WriteLine("Result 3: <" + resultThree + ">");
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            env.CompileDeploy(
                "@name('insert') insert into Nested3Stream select nested1simple, nested4array from " +
                eventTypeName +
                "#lastevent",
                path);
            env.CompileDeploy("@name('sw') select * from " + eventTypeName + "#lastevent", path);
            env.AssertStatement(
                "insert",
                statement => {
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1simple", typeof(XmlNode)).WithFragment(),
                        new SupportEventPropDesc("nested4array", typeof(XmlNode[])).WithComponentType(typeof(XmlNode))
                            .WithIndexed()
                            .WithFragment());

                    var fragmentTypeNested1 = statement.EventType.GetFragmentType("nested1simple");
                    Assert.IsFalse(fragmentTypeNested1.IsIndexed);
                    SupportEventPropUtil.AssertPropsEquals(
                        fragmentTypeNested1.FragmentType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop1", typeof(string)),
                        new SupportEventPropDesc("prop2", typeof(bool?)),
                        new SupportEventPropDesc("attr1", typeof(string)),
                        new SupportEventPropDesc("nested2", typeof(XmlNode)));
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);

                    var fragmentTypeNested4 = statement.EventType.GetFragmentType("nested4array");
                    Assert.IsTrue(fragmentTypeNested4.IsIndexed);
                    SupportEventPropUtil.AssertPropsEquals(
                        fragmentTypeNested4.FragmentType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop5", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop6", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop7", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop8", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("id", typeof(string)));
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);

                    var fragmentTypeNested4Item = statement.EventType.GetFragmentType("nested4array[0]");
                    Assert.IsFalse(fragmentTypeNested4Item.IsIndexed);
                    SupportEventPropUtil.AssertPropsEquals(
                        fragmentTypeNested4Item.FragmentType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop5", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop6", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop7", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("prop8", typeof(string[])).WithComponentType(typeof(string))
                            .WithIndexed(),
                        new SupportEventPropDesc("id", typeof(string)));
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4Item.FragmentType);
                });
            env.AssertStatement(
                "sw",
                statement => SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType));

            var doc = SupportXML.MakeDefaultEvent("ABC");
            env.SendEventXMLDOM(doc, eventTypeName);

            env.AssertIterator(
                "insert",
                iterator => {
                    var received = iterator.Advance();
                    EPAssertionUtil.AssertProps(
                        received,
                        "nested1simple.prop1,nested1simple.prop2,nested1simple.attr1,nested1simple.nested2.prop3[1]"
                            .SplitCsv(),
                        new object[] { "SAMPLE_V1", true, "SAMPLE_ATTR1", 4 });
                    EPAssertionUtil.AssertProps(
                        received,
                        "nested4array[0].id,nested4array[0].prop5[1],nested4array[1].id".SplitCsv(),
                        new object[] { "a", "SAMPLE_V8", "b" });
                });

            // assert event and fragments alone
            env.AssertIterator(
                "sw",
                iterator => {
                    var wildcardStmtEvent = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);

                    var eventType = wildcardStmtEvent.EventType.GetFragmentType("nested1simple");
                    Assert.IsFalse(eventType.IsIndexed);
                    Assert.IsFalse(eventType.IsNative);
                    Assert.AreEqual("MyNestedEventXPC", eventType.FragmentType.Name);
                    Assert.IsTrue(wildcardStmtEvent.Get("nested1simple") is XmlNode);
                    Assert.AreEqual(
                        "SAMPLE_V1",
                        ((EventBean)wildcardStmtEvent.GetFragment("nested1simple")).Get("prop1"));

                    eventType = wildcardStmtEvent.EventType.GetFragmentType("nested4array");
                    Assert.IsTrue(eventType.IsIndexed);
                    Assert.IsFalse(eventType.IsNative);
                    Assert.AreEqual("MyNestedArrayEventXPC", eventType.FragmentType.Name);
                    var eventsArray = (EventBean[])wildcardStmtEvent.GetFragment("nested4array");
                    Assert.AreEqual(3, eventsArray.Length);
                    Assert.AreEqual("SAMPLE_V8", eventsArray[0].Get("prop5[1]"));
                    Assert.AreEqual("SAMPLE_V9", eventsArray[1].Get("prop5[0]"));
                    Assert.AreEqual(typeof(XmlNodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
                    Assert.IsTrue(wildcardStmtEvent.Get("nested4array") is XmlNodeList);

                    var nested4arrayItem = (EventBean)wildcardStmtEvent.GetFragment("nested4array[1]");
                    Assert.AreEqual("b", nested4arrayItem.Get("id"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace