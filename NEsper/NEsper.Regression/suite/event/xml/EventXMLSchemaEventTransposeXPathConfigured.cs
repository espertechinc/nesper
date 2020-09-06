///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeXPathConfigured
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredPreconfig());
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredCreateSchema());
            execs.Add(new EventXMLSchemaEventTransposeXPathConfiguredXPathExpression());
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
		        var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
		        var epl = "@public @buseventtype " +
		                  "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" + schemaUriSimpleSchema + "', AutoFragment=false)" +
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

		        XmlNode node = SupportXML.GetDocument().DocumentElement;

		        var pathExprOne = XPathExpression.Compile("/n0:simpleEvent/n0:nested1", ctx);
		        var pathExprOneIterator = node.CreateNavigator().Select(pathExprOne);
		        Assert.That(pathExprOne.ReturnType, Is.EqualTo(XPathResultType.NodeSet));
		        Assert.That(pathExprOneIterator.MoveNext(), Is.True);
		        Assert.That(pathExprOneIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
		        Assert.That(pathExprOneIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));

		        //System.out.println("Result:\n" + SchemaUtil.serialize(result));

		        var pathExprTwo = XPathExpression.Compile("/n0:simpleEvent/n0:nested1/n0:prop1", ctx);
		        var pathExprTwoIterator = node.CreateNavigator().Select(pathExprTwo);
		        Assert.That(pathExprTwoIterator.MoveNext(), Is.True);
		        Assert.That(pathExprTwoIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
		        Assert.That(pathExprTwoIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));
		        Assert.That(pathExprTwoIterator.Current.MoveToFirstChild(), Is.True);
		        Assert.That(pathExprTwoIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Text));
		        Assert.That(pathExprTwoIterator.Current.TypedValue, Is.EqualTo("SAMPLE_V1"));

		        //System.out.println("Result 2: <" + resultTwo + ">");

		        var pathExprThree = XPathExpression.Compile("/n0:simpleEvent/n0:nested3", ctx);
		        var pathExprThreeIterator = node.CreateNavigator().Select(pathExprThree);
		        Assert.That(pathExprThreeIterator.MoveNext(), Is.True);
		        Assert.That(pathExprThreeIterator.Current.UnderlyingObject, Is.InstanceOf<XmlElement>());
		        Assert.That(pathExprThreeIterator.Current.NodeType, Is.EqualTo(XPathNodeType.Element));
		        Assert.That(pathExprThreeIterator.Current.HasChildren, Is.True);

		        //System.out.println("Result 3: <" + resultThree + ">");
	        }
        }

        private static void RunAssertion(RegressionEnvironment env, String eventTypeName, RegressionPath path)
        {
            env.CompileDeploy("@Name('insert') insert into Nested3Stream select nested1simple, nested4array from " + eventTypeName + "#lastevent", path);
            env.CompileDeploy("@Name('sw') select * from " + eventTypeName + "#lastevent", path);
 
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("insert").EventType);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("sw").EventType);
            CollectionAssert.AreEquivalent(
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
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, false)
                },
                fragmentTypeNested1.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);

            var fragmentTypeNested4 = env.Statement("insert").EventType.GetFragmentType("nested4array");
            Assert.IsTrue(fragmentTypeNested4.IsIndexed);
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("prop5", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("id", typeof(string), typeof(char), false, false, true, false, false)
                },
                fragmentTypeNested4.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);

            var fragmentTypeNested4Item = env.Statement("insert").EventType.GetFragmentType("nested4array[0]");
            Assert.IsFalse(fragmentTypeNested4Item.IsIndexed);
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("prop5", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop6", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop7", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("prop8", typeof(string[]), typeof(string), false, false, true, false, false),
                    new EventPropertyDescriptor("id", typeof(string), typeof(char), false, false, true, false, false)
                },
                fragmentTypeNested4Item.FragmentType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4Item.FragmentType);

            SupportXML.SendDefaultEvent(env.EventService, "ABC", eventTypeName);

            var received = env.Statement("insert").First();
            EPAssertionUtil.AssertProps(
                received,
                new [] { "nested1simple.prop1","nested1simple.prop2","nested1simple.attr1","nested1simple.nested2.prop3[1]" },
                new object[] {"SAMPLE_V1", true, "SAMPLE_ATTR1", 4});
            EPAssertionUtil.AssertProps(
                received,
                new [] { "nested4array[0].id","nested4array[0].prop5[1]","nested4array[1].id" },
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
            Assert.AreEqual(typeof(XmlNodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
            Assert.That(wildcardStmtEvent.Get("nested4array"), Is.InstanceOf<XmlNodeList>());

            var nested4arrayItem = (EventBean) wildcardStmtEvent.GetFragment("nested4array[1]");
            Assert.AreEqual("b", nested4arrayItem.Get("id"));

            env.UndeployAll();
        }
    }
} // end of namespace