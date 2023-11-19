///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposePrimitiveArray
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithPreconfig(execs);
            With(CreateSchema)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposePrimitiveArrayCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposePrimitiveArrayPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposePrimitiveArrayPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestNested2", "ABCType", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposePrimitiveArrayCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='//nested2', SchemaResource='" +
                          schemaUriSimpleSchema +
                          "', EventSenderValidatesRoot=false)" +
                          "create xml schema MyEventCreateSchemaNested();\n" +
                          "" +
                          "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                          schemaUriSimpleSchema +
                          "', EventSenderValidatesRoot=false)" +
                          "create xml schema MyEventCreateSchemaABC();\n";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchemaNested", "MyEventCreateSchemaABC", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeNameNested,
            string eventTypeNameABC,
            RegressionPath path)
        {
            // try array property in select
            env.CompileDeploy("@name('s0') select * from " + eventTypeNameNested + "#lastevent", path)
                .AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("prop3", typeof(int?[])).WithIndexed());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.SendEventXMLDOM(
                SupportXML.GetDocument("<nested2><prop3>2</prop3><prop3></prop3><prop3>4</prop3></nested2>"),
                eventTypeNameNested);
            env.AssertIterator(
                "s0",
                iterator => {
                    var theEvent = iterator.Advance();
                    var theValues = theEvent.Get("prop3").Unwrap<object>(true);
                    EPAssertionUtil.AssertEqualsExactOrder(
                        theValues,
                        new object[] { 2, null, 4 });
                    SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
                });

            env.UndeployModuleContaining("s0");

            // try array property nested
            env.CompileDeploy("@name('s0') select nested3.* from " + eventTypeNameABC + "#lastevent", path);
            var doc = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(doc, eventTypeNameABC);
            env.AssertIterator(
                "s0",
                iterator => {
                    var stmtSelectResult = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectResult);
                    Assert.AreEqual(typeof(string[]), stmtSelectResult.EventType.GetPropertyType("Nested4[2].prop5"));
                    Assert.AreEqual("SAMPLE_V8", stmtSelectResult.Get("Nested4[0].prop5[1]"));
                    EPAssertionUtil.AssertEqualsExactOrder(
                        (string[])stmtSelectResult.Get("Nested4[2].prop5"),
                        new object[] { "SAMPLE_V10", "SAMPLE_V11" });

                    var fragmentNested4 = (EventBean)stmtSelectResult.GetFragment("Nested4[2]");
                    EPAssertionUtil.AssertEqualsExactOrder(
                        (string[])fragmentNested4.Get("prop5"),
                        new object[] { "SAMPLE_V10", "SAMPLE_V11" });
                    Assert.AreEqual("SAMPLE_V11", fragmentNested4.Get("prop5[1]"));
                    SupportEventTypeAssertionUtil.AssertConsistency(fragmentNested4);
                });

            env.UndeployAll();
        }
    }
} // end of namespace