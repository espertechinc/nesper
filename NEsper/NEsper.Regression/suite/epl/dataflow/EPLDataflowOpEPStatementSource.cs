///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.dataflow;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.epl.SupportStaticMethodLib;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpEPStatementSource
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowAllTypes());
            execs.Add(new EPLDataflowStmtNameDynamic());
            execs.Add(new EPLDataflowStatementFilter());
            execs.Add(new EPLDataflowInvalid());
            return execs;
        }

        private static void RunAssertionStatementNameExists(
            RegressionEnvironment env,
            string typeName,
            object[] events)
        {
            env.CompileDeploy("@Name('MyStatement') select * from " + typeName);

            env.CompileDeploy(
                "@Name('flow') create dataflow MyDataFlowOne " +
                "create schema AllObject System.Object," +
                "EPStatementSource -> thedata:<AllObject> {" +
                "  statementDeploymentId : '" +
                env.DeploymentId("MyStatement") +
                "'," +
                "  statementName : 'MyStatement'," +
                "} " +
                "DefaultSupportCaptureOp(thedata) {}");

            var captureOp = new DefaultSupportCaptureOp(2, env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(captureOp));

            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
            df.Start();

            var sender = env.EventService.GetEventSender(typeName);
            foreach (var @event in events) {
                sender.SendEvent(@event);
            }

            try {
                captureOp.GetValue(1, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPException(t);
            }

            EPAssertionUtil.AssertEqualsExactOrder(events, captureOp.Current);

            df.Cancel();
            env.UndeployAll();
        }

        internal class EPLDataflowStmtNameDynamic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "create map schema SingleProp (Id string), " +
                    "EPStatementSource -> thedata:<SingleProp> {" +
                    "  statementDeploymentId : 'MyDeploymentId'," +
                    "  statementName : 'MyStatement'," +
                    "} " +
                    "DefaultSupportCaptureOp(thedata) {}");

                var captureOp = new DefaultSupportCaptureOp();
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(captureOp));

                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                Assert.IsNull(df.UserObject);
                Assert.IsNull(df.InstanceId);
                df.Start();

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(0, captureOp.Current.Length);

                var epl = "@Name('MyStatement') select TheString as Id from SupportBean";
                var compiled = env.Compile(epl);
                try {
                    env.Deployment.Deploy(compiled, new DeploymentOptions().WithDeploymentId("MyDeploymentId"));
                }
                catch (EPDeployException e) {
                    Assert.Fail(e.Message);
                }

                env.SendEventBean(new SupportBean("E2", 2));
                captureOp.WaitForInvocation(100, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"E2"});

                env.UndeployModuleContaining("MyStatement");

                env.SendEventBean(new SupportBean("E3", 3));
                Assert.AreEqual(0, captureOp.Current.Length);

                try {
                    env.Deployment.Deploy(compiled, new DeploymentOptions().WithDeploymentId("MyDeploymentId"));
                }
                catch (EPDeployException e) {
                    Assert.Fail(e.Message);
                }

                env.SendEventBean(new SupportBean("E4", 4));
                captureOp.WaitForInvocation(100, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"E4"});

                env.UndeployModuleContaining("MyStatement");

                env.SendEventBean(new SupportBean("E5", 5));
                Assert.AreEqual(0, captureOp.Current.Length);

                compiled = env.Compile("@Name('MyStatement') select 'X'||TheString||'X' as Id from SupportBean");
                try {
                    env.Deployment.Deploy(compiled, new DeploymentOptions().WithDeploymentId("MyDeploymentId"));
                }
                catch (EPDeployException e) {
                    Assert.Fail(e.Message);
                }

                env.SendEventBean(new SupportBean("E6", 6));
                captureOp.WaitForInvocation(100, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"XE6X"});

                df.Cancel();
                env.UndeployAll();
            }
        }

        internal class EPLDataflowAllTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionStatementNameExists(
                    env,
                    DefaultSupportGraphEventUtil.EVENTTYPENAME,
                    DefaultSupportGraphEventUtil.GetPONOEvents());
                RunAssertionStatementNameExists(env, "MyMapEvent", DefaultSupportGraphEventUtil.GetMapEvents());
                RunAssertionStatementNameExists(env, "MyOAEvent", DefaultSupportGraphEventUtil.GetOAEvents());
                RunAssertionStatementNameExists(env, "MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEvents());

                // test doc samples
                var epl = "@Name('flow') create dataflow MyDataFlow\n" +
                          "  create schema SampleSchema(tagId string, locX double),\t// sample type\t\t\t\n" +
                          "\t\t\t\n" +
                          "  // ConsIder only the statement named MySelectStatement when it exists.\n" +
                          "  EPStatementSource -> stream.one:<eventbean<?>> {\n" +
                          "    statementDeploymentId : 'MyDeploymentABC',\n" +
                          "    statementName : 'MySelectStatement'\n" +
                          "  }\n" +
                          "  \n" +
                          "  // ConsIder all statements that match the filter object provided.\n" +
                          "  EPStatementSource -> stream.two<eventbean<?>> {\n" +
                          "    statementFilter : {\n" +
                          "      class : '" +
                          typeof(MyFilter).MaskTypeName() +
                          "'\n" +
                          "    }\n" +
                          "  }\n" +
                          "  \n" +
                          "  // ConsIder all statements that match the filter object provided.\n" +
                          "  // With collector that performs transformation.\n" +
                          "  EPStatementSource -> stream.two<SampleSchema> {\n" +
                          "    collector : {\n" +
                          "      class : '" +
                          typeof(MyCollector).MaskTypeName() +
                          "'\n" +
                          "    },\n" +
                          "    statementFilter : {\n" +
                          "      class : '" +
                          typeof(MyFilter).MaskTypeName() +
                          "'\n" +
                          "    }\n" +
                          "  }";
                env.CompileDeploy(epl);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");

                env.UndeployAll();
            }
        }

        internal class EPLDataflowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test no statement name or statement filter provided
                var epl = "create dataflow DF1 " +
                          "create schema AllObjects as System.Object," +
                          "EPStatementSource -> thedata:<AllObjects> {} " +
                          "DefaultSupportCaptureOp(thedata) {}";
                SupportDataFlowAssertionUtil.TryInvalidInstantiate(
                    env,
                    "DF1",
                    epl,
                    "Failed to instantiate data flow 'DF1': Failed to obtain operator instance for 'EPStatementSource': Failed to find required 'statementName' or 'statementFilter' parameter");

                // invalid: no output stream
                TryInvalidCompile(
                    env,
                    "create dataflow DF1 EPStatementSource { statementName : 'abc' }",
                    "Failed to obtain operator 'EPStatementSource': EPStatementSource operator requires one output stream but produces 0 streams");

                // invalid: no statement deployment id
                TryInvalidCompile(
                    env,
                    "create dataflow DF1 EPStatementSource ->abc { statementName : 'abc' }",
                    "Failed to obtain operator 'EPStatementSource': Both 'statementDeploymentId' and 'statementName' are required when either of these are specified");
            }
        }

        internal class EPLDataflowStatementFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // one statement exists before the data flow
                env.CompileDeploy("select Id from SupportBean_B");

                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "create schema AllObjects as System.Object," +
                    "EPStatementSource -> thedata:<AllObjects> {} " +
                    "DefaultSupportCaptureOp(thedata) {}");

                var captureOp = new DefaultSupportCaptureOp(env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions();
                var myFilter = new MyFilter();
                options.WithParameterProvider(
                    new DefaultSupportGraphParamProvider(
                        Collections.SingletonMap<string, object>("statementFilter", myFilter)));
                options.WithOperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                df.Start();

                env.SendEventBean(new SupportBean_B("B1"));
                captureOp.WaitForInvocation(200, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"B1"});

                env.CompileDeploy("select TheString, IntPrimitive from SupportBean");
                env.SendEventBean(new SupportBean("E1", 1));
                captureOp.WaitForInvocation(200, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "TheString","IntPrimitive" },
                    new object[] {"E1", 1});

                env.CompileDeploy("@Name('s2') select Id from SupportBean_A");
                env.SendEventBean(new SupportBean_A("A1"));
                captureOp.WaitForInvocation(200, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"A1"});

                env.UndeployModuleContaining("s2");

                env.SendEventBean(new SupportBean_A("A2"));
                Sleep(50);
                Assert.AreEqual(0, captureOp.Current.Length);

                env.CompileDeploy("@Name('s2') select Id from SupportBean_A");

                env.SendEventBean(new SupportBean_A("A3"));
                captureOp.WaitForInvocation(200, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"A3"});

                env.SendEventBean(new SupportBean_B("B2"));
                captureOp.WaitForInvocation(200, 1);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    captureOp.GetCurrentAndReset()[0],
                    new [] { "Id" },
                    new object[] {"B2"});

                df.Cancel();

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B3"));
                Assert.AreEqual(0, captureOp.Current.Length);

                env.UndeployAll();
            }
        }

        public class MyFilter : EPDataFlowEPStatementFilter
        {
            public bool Pass(EPDataFlowEPStatementFilterContext statement)
            {
                return true;
            }
        }

        public class MyCollector : EPDataFlowIRStreamCollector
        {
            public void Collect(EPDataFlowIRStreamCollectorContext data)
            {
            }
        }
    }
} // end of namespace