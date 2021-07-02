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
            List<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposePrimitiveArrayCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
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
                string epl = "@public @buseventtype " +
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
                RegressionPath path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchemaNested", "MyEventCreateSchemaABC", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeNameNested,
            String eventTypeNameABC,
            RegressionPath path)
        {
            // try array property in select
            env.CompileDeploy("@Name('s0') select * from " + eventTypeNameNested + "#lastevent", path).AddListener("s0");

            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("s0").EventType.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("prop3", typeof(int?[])).WithComponentType(typeof(int?)).WithIndexed());
                
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            EventSender sender = env.EventService.GetEventSender(eventTypeNameNested);
            sender.SendEvent(
                SupportXML.GetDocument(
                    "<nested2><prop3>2</prop3><prop3></prop3><prop3>4</prop3></nested2>"));
            var theEvent = env.GetEnumerator("s0").Advance();
            var theValues = theEvent.Get("prop3").Unwrap<object>(true);
            EPAssertionUtil.AssertEqualsExactOrder(
                theValues,
                new object[] {2, null, 4});
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
            env.UndeployModuleContaining("s0");

            // try array property nested
            env.CompileDeploy("@Name('s0') select nested3.* from " + eventTypeNameABC + "#lastevent", path);
            SupportXML.SendDefaultEvent(env.EventService, "test", eventTypeNameABC);
            var stmtSelectResult = env.GetEnumerator("s0").Advance();
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectResult);
            Assert.AreEqual(typeof(string[]), stmtSelectResult.EventType.GetPropertyType("nested4[2].prop5"));
            Assert.AreEqual("SAMPLE_V8", stmtSelectResult.Get("nested4[0].prop5[1]"));
            EPAssertionUtil.AssertEqualsExactOrder(
                (string[]) stmtSelectResult.Get("nested4[2].prop5"),
                new object[] {"SAMPLE_V10", "SAMPLE_V11"});

            var fragmentNested4 = (EventBean) stmtSelectResult.GetFragment("nested4[2]");
            EPAssertionUtil.AssertEqualsExactOrder(
                (string[]) fragmentNested4.Get("prop5"),
                new object[] {"SAMPLE_V10", "SAMPLE_V11"});
            Assert.AreEqual("SAMPLE_V11", fragmentNested4.Get("prop5[1]"));
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentNested4);

            env.UndeployAll();
        }
    }
} // end of namespace