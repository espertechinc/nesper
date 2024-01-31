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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeXPathConfigured
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventTransposeXPathConfiguredCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventTransposeXPathConfiguredPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaEventTransposeXPathConfiguredPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "MyXMLEvent", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaEventTransposeXPathConfiguredCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent')" +
                          "@XMLSchemaNamespacePrefix(Prefix='ss', Namespace='samples:schemas:simpleSchema')" +
                          "@XMLSchemaField(Name='nested1simple', XPath='/ss:simpleEvent/ss:nested1', Type='any', EventTypeName='MyNestedEvent')" +
                          "@XMLSchemaField(Name='nested4array', XPath='//ss:nested4', Type='nodeset', EventTypeName='MyNestedArrayEvent')" +
                          "create xml schema MyEventCreateSchema();\n";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            env.CompileDeploy(
                "@name('insert') insert into Nested3Stream select nested1simple, nested4array from " + eventTypeName,
                path);
            env.CompileDeploy("@name('s0') select * from " + eventTypeName, path);
            env.AssertStatement(
                "insert",
                statement => {
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1simple", typeof(XmlNode)).WithFragment(),
                        new SupportEventPropDesc("nested4array", typeof(XmlNode[])).WithIndexed().WithFragment());
                });
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType));

            env.AssertStatement(
                "insert",
                statement => {
                    var fragmentTypeNested1 = statement.EventType.GetFragmentType("nested1simple");
                    ClassicAssert.IsFalse(fragmentTypeNested1.IsIndexed);
                    ClassicAssert.AreEqual(0, fragmentTypeNested1.FragmentType.PropertyDescriptors.Count);
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested1.FragmentType);

                    var fragmentTypeNested4 = statement.EventType.GetFragmentType("nested4array");
                    ClassicAssert.IsTrue(fragmentTypeNested4.IsIndexed);
                    ClassicAssert.AreEqual(0, fragmentTypeNested4.FragmentType.PropertyDescriptors.Count);
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentTypeNested4.FragmentType);
                });

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
                        new object[] { "SAMPLE_V1", "true", "SAMPLE_ATTR1", "4" });
                    EPAssertionUtil.AssertProps(
                        received,
                        "nested4array[0].id,nested4array[0].prop5[1],nested4array[1].id".SplitCsv(),
                        new object[] { "a", "SAMPLE_V8", "b" });
                });

            // assert event and fragments alone
            env.AssertIterator(
                "s0",
                iterator => {
                    var wildcardStmtEvent = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(wildcardStmtEvent);

                    var eventType = wildcardStmtEvent.EventType.GetFragmentType("nested1simple");
                    ClassicAssert.IsFalse(eventType.IsIndexed);
                    ClassicAssert.IsFalse(eventType.IsNative);
                    ClassicAssert.AreEqual("MyNestedEvent", eventType.FragmentType.Name);
                    ClassicAssert.IsTrue(wildcardStmtEvent.Get("nested1simple") is XmlNode);
                    ClassicAssert.AreEqual(
                        "SAMPLE_V1",
                        ((EventBean)wildcardStmtEvent.GetFragment("nested1simple")).Get("prop1"));

                    eventType = wildcardStmtEvent.EventType.GetFragmentType("nested4array");
                    ClassicAssert.IsTrue(eventType.IsIndexed);
                    ClassicAssert.IsFalse(eventType.IsNative);
                    ClassicAssert.AreEqual("MyNestedArrayEvent", eventType.FragmentType.Name);
                    var eventsArray = (EventBean[])wildcardStmtEvent.GetFragment("nested4array");
                    ClassicAssert.AreEqual(3, eventsArray.Length);
                    ClassicAssert.AreEqual("SAMPLE_V8", eventsArray[0].Get("prop5[1]"));
                    ClassicAssert.AreEqual("SAMPLE_V9", eventsArray[1].Get("prop5[0]"));
                    ClassicAssert.AreEqual(typeof(XmlNodeList), wildcardStmtEvent.EventType.GetPropertyType("nested4array"));
                    ClassicAssert.IsTrue(wildcardStmtEvent.Get("nested4array") is XmlNodeList);
                });

            env.UndeployAll();
        }
    }
} // end of namespace