///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpEventBusSink
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowAllTypes());
            execs.Add(new EPLDataflowBeacon());
            execs.Add(new EPLDataflowSendEventDynamicType());
            return execs;
        }

        private static void RunAssertionAllTypes(
            RegressionEnvironment env,
            string typeName,
            object[] events)
        {
            var graph = "@Name('flow') create dataflow MyGraph " +
                        "DefaultSupportSourceOp -> instream<" +
                        typeName +
                        ">{}" +
                        "EventBusSink(instream) {}";
            env.CompileDeploy(graph);

            env.CompileDeploy("@Name('s0') select * from " + typeName).AddListener("s0");

            var source = new DefaultSupportSourceOp(events);
            var options = new EPDataFlowInstantiationOptions();
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(source));
            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyGraph", options);
            instance.Run();

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").NewDataListFlattened,
                new[] {"MyDouble", "MyInt", "MyString"},
                new[] {
                    new object[] {1.1d, 1, "one"},
                    new object[] {2.2d, 2, "two"}
                });

            env.UndeployAll();
        }

        internal class EPLDataflowAllTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionAllTypes(env, "MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEvents());
                RunAssertionAllTypes(env, "MyOAEvent", DefaultSupportGraphEventUtil.GetOAEvents());
                RunAssertionAllTypes(env, "MyMapEvent", DefaultSupportGraphEventUtil.GetMapEvents());
                RunAssertionAllTypes(
                    env,
                    DefaultSupportGraphEventUtil.EVENTTYPENAME,
                    DefaultSupportGraphEventUtil.GetPONOEvents());

                // invalid: output stream
                TryInvalidCompile(
                    env,
                    "create dataflow DF1 EventBusSink -> s1 {}",
                    "Failed to obtain operator 'EventBusSink': EventBusSink operator does not provide an output stream");

                var path = new RegressionPath();
                env.CompileDeploy("create schema SampleSchema(tagId string, locX double, locY double)", path);
                var docSmple = "@Name('s0') create dataflow MyDataFlow\n" +
                               "BeaconSource -> instream<SampleSchema> {} // produces sample stream to\n" +
                               "//demonstrate below\n" +
                               "// Send SampleSchema events produced by beacon to the event bus.\n" +
                               "EventBusSink(instream) {}\n" +
                               "\n" +
                               "// Send SampleSchema events produced by beacon to the event bus.\n" +
                               "// With collector that performs transformation.\n" +
                               "EventBusSink(instream) {\n" +
                               "collector : {\n" +
                               "class : '" +
                               typeof(MyTransformToEventBus).FullName +
                               "'\n" +
                               "}\n" +
                               "}";
                env.CompileDeploy(docSmple, path);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("s0"), "MyDataFlow");

                env.UndeployAll();
            }
        }

        internal class EPLDataflowBeacon : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create objectarray schema MyEventBeacon(P0 string, P1 long)", path);
                env.CompileDeploy("@Name('s0') select * from MyEventBeacon", path).AddListener("s0");
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "" +
                    "BeaconSource -> BeaconStream<MyEventBeacon> {" +
                    "  iterations : 3," +
                    "  P0 : 'abc'," +
                    "  P1 : 1," +
                    "}" +
                    "EventBusSink(BeaconStream) {}",
                    path);

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne").Start();
                env.Listener("s0").WaitForInvocation(3000, 3);
                var events = env.Listener("s0").NewDataListFlattened;

                for (var i = 0; i < 3; i++) {
                    Assert.AreEqual("abc", events[i].Get("P0"));
                    var val = events[i].Get("P1").AsLong();
                    Assert.IsTrue(val > 0 && val < 10);
                }

                env.UndeployAll();
            }
        }

        internal class EPLDataflowSendEventDynamicType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schemaEPL =
                    "create objectarray schema MyEventOne(type string, p0 int, p1 string);\n" +
                    "create objectarray schema MyEventTwo(type string, f0 string, f1 int);\n";
                env.CompileDeployWBusPublicType(schemaEPL, path);

                env.CompileDeploy("@Name('s0') select * from MyEventOne", path).AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from MyEventTwo", path).AddListener("s1");

                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlow " +
                    "MyObjectArrayGraphSource -> OutStream<?> {}" +
                    "EventBusSink(OutStream) {" +
                    "  collector : {" +
                    "    class: '" +
                    typeof(MyTransformToEventBus).FullName +
                    "'" +
                    "  }" +
                    "}");

                var source = new MyObjectArrayGraphSource(
                    Arrays.AsList(
                            new object[] {"type1", 100, "abc"},
                            new object[] {"type2", "GE", -1}
                        )
                        .GetEnumerator());
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(source));
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", options).Start();

                env.Listener("s0").WaitForInvocation(3000, 1);
                env.Listener("s1").WaitForInvocation(3000, 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "p0","p1" },
                    new object[] {100, "abc"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    new [] { "f0","f1" },
                    new object[] {"GE", -1});

                env.UndeployAll();
            }
        }

        public class MyTransformToEventBus : EPDataFlowEventCollector
        {
            public void Collect(EPDataFlowEventCollectorContext context)
            {
                if (!(context.Event is object[])) {
                    return; // ignoring other types of events
                }

                var eventObj = (object[]) context.Event;
                if (eventObj[0].Equals("type1")) {
                    context.Sender.SendEventObjectArray(eventObj, "MyEventOne");
                }
                else {
                    context.Sender.SendEventObjectArray(eventObj, "MyEventTwo");
                }
            }
        }
    }
} // end of namespace