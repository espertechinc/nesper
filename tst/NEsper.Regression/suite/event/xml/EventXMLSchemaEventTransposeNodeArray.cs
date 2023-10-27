///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeNodeArray
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
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
                var schemaUriSimpleSchema = env.Container.ResourceManager()
                    .ResolveResourceURL("regression/simpleSchema.xsd")
                    .ToString();
                var epl = "@public @buseventtype " +
                          "@XMLSchema(rootElementName='simpleEvent', schemaResource='" +
                          schemaUriSimpleSchema +
                          "')" +
                          "create xml schema MyEventCreateSchema()";
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
            // try array property insert
            env.CompileDeploy("@name('s0') select nested3.Nested4 as narr from " + eventTypeName + "#lastevent", path);
            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("narr", typeof(XmlNode[])).WithIndexed().WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var doc = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(doc, eventTypeName);

            env.AssertIterator(
                "s0",
                it => {
                    var result = it.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(result);
                    var fragments = (EventBean[])result.GetFragment("narr");
                    Assert.AreEqual(3, fragments.Length);
                    Assert.AreEqual("SAMPLE_V8", fragments[0].Get("prop5[1]"));
                    Assert.AreEqual("SAMPLE_V11", fragments[2].Get("prop5[1]"));

                    var fragmentItem = (EventBean)result.GetFragment("narr[2]");
                    Assert.AreEqual(eventTypeName + ".Nested3.Nested4", fragmentItem.EventType.Name);
                    Assert.AreEqual("SAMPLE_V10", fragmentItem.Get("prop5[0]"));
                });

            // try array index property insert
            env.CompileDeploy(
                "@name('ii') select nested3.Nested4[1] as narr from " + eventTypeName + "#lastevent",
                path);
            env.AssertStatement(
                "ii",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("narr", typeof(XmlNode)).WithFragment());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var docTwo = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(docTwo, eventTypeName);

            env.AssertIterator(
                "ii",
                iterator => {
                    var resultItem = iterator.Advance();
                    Assert.AreEqual("b", resultItem.Get("narr.Id"));
                    SupportEventTypeAssertionUtil.AssertConsistency(resultItem);
                    var fragmentsInsertItem = (EventBean)resultItem.GetFragment("narr");
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentsInsertItem);
                    Assert.AreEqual("b", fragmentsInsertItem.Get("Id"));
                    Assert.AreEqual("SAMPLE_V9", fragmentsInsertItem.Get("prop5[0]"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace