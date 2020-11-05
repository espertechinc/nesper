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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeNodeArray
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeNodeArrayCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeNodeArrayPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposeNodeArrayPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "SimpleEventWSchema", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposeNodeArrayCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
                string epl = "@public @buseventtype " +
                             "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                             schemaUriSimpleSchema +
                             "')" +
                             "create xml schema MyEventCreateSchema()";
                RegressionPath path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeName,
            RegressionPath path)
        {
            // try array property insert
            env.CompileDeploy("@Name('s0') select nested3.nested4 as narr from " + eventTypeName + "#lastevent", path);
            CollectionAssert.AreEquivalent(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor(
                        "narr",
                        typeof(XmlNode[]),
                        typeof(XmlNode),
                        false,
                        false,
                        true,
                        false,
                        true)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", eventTypeName);

            var result = env.Statement("s0").First();
            SupportEventTypeAssertionUtil.AssertConsistency(result);
            var fragments = (EventBean[]) result.GetFragment("narr");
            Assert.AreEqual(3, fragments.Length);
            Assert.AreEqual("SAMPLE_V8", fragments[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", fragments[2].Get("prop5[1]"));

            var fragmentItem = (EventBean) result.GetFragment("narr[2]");
            Assert.AreEqual($"{eventTypeName}.nested3.nested4", fragmentItem.EventType.Name);
            Assert.AreEqual("SAMPLE_V10", fragmentItem.Get("prop5[0]"));

            // try array index property insert
            env.CompileDeploy($"@Name('ii') select nested3.nested4[1] as narr from {eventTypeName}#lastevent", path);
            CollectionAssert.AreEquivalent(
                new[] {
                    new EventPropertyDescriptor("narr", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("ii").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("ii").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", eventTypeName);

            var resultItem = env.GetEnumerator("ii").Advance();
            Assert.That(resultItem.Get("narr.id"), Is.EqualTo("b"));
            SupportEventTypeAssertionUtil.AssertConsistency(resultItem);

            var fragmentsInsertItem = (EventBean) resultItem.GetFragment("narr");
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentsInsertItem);
            Assert.That(fragmentsInsertItem.Get("id"), Is.EqualTo("b"));
            Assert.That(fragmentsInsertItem.Get("prop5[0]"), Is.EqualTo("SAMPLE_V9"));

            env.UndeployAll();
        }
    }
} // end of namespace