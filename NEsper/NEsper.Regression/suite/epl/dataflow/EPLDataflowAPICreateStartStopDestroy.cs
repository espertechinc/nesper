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
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPICreateStartStopDestroy
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
WithCreateStartStop(execs);
WithDeploymentAdmin(execs);
            return execs;
        }
public static IList<RegressionExecution> WithDeploymentAdmin(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowDeploymentAdmin());
    return execs;
}public static IList<RegressionExecution> WithCreateStartStop(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowCreateStartStop());
    return execs;
}
        private static void TryInstantiate(
            RegressionEnvironment env,
            string deploymentId,
            string graph,
            string message)
        {
            try {
                env.Runtime.DataFlowService.Instantiate(deploymentId, graph);
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void AssertException(
            string expected,
            string message)
        {
            var received = message.Substring(0, message.IndexOf("[") + 1);
            Assert.AreEqual(expected, received);
        }

        internal class EPLDataflowCreateStartStop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('flow') create dataflow MyGraph Emitter -> outstream<?> {}";
                var compiledGraph = env.Compile(epl);
                try {
                    env.Deployment.Deploy(compiledGraph, new DeploymentOptions().WithDeploymentId("DEP1"));
                }
                catch (EPDeployException ex) {
                    throw new EPException(ex);
                }

                var dfruntime = env.Runtime.DataFlowService;
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new[] {new DeploymentIdNamePair(env.DeploymentId("flow"), "MyGraph")},
                    dfruntime.DataFlows);
                var desc = dfruntime.GetDataFlow("DEP1", "MyGraph");
                Assert.AreEqual("MyGraph", desc.DataFlowName);
                Assert.AreEqual("flow", desc.StatementName);

                dfruntime.Instantiate(env.DeploymentId("flow"), "MyGraph");

                // stop - can no longer instantiate but still exists
                env.UndeployModuleContaining("flow");
                TryInstantiate(
                    env,
                    "DEP1",
                    "MyGraph",
                    "Data flow by name 'MyGraph' for deployment id 'DEP1' has not been defined");
                TryInstantiate(
                    env,
                    "DEP1",
                    "DUMMY",
                    "Data flow by name 'DUMMY' for deployment id 'DEP1' has not been defined");

                // destroy - should be gone
                Assert.AreEqual(null, dfruntime.GetDataFlow("DEP1", "MyGraph"));
                Assert.AreEqual(0, dfruntime.DataFlows.Length);
                TryInstantiate(
                    env,
                    "DEP1",
                    "MyGraph",
                    "Data flow by name 'MyGraph' for deployment id 'DEP1' has not been defined");

                // new one, try start-stop-start
                env.CompileDeploy(epl);
                dfruntime.Instantiate(env.DeploymentId("flow"), "MyGraph");
                env.UndeployAll();
            }
        }

        internal class EPLDataflowDeploymentAdmin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var epl = "@Name('flow') create dataflow TheGraph\n" +
                          "create schema ABC as " +
                          typeof(SupportBean).FullName +
                          "," +
                          "DefaultSupportSourceOp -> outstream<SupportBean> {}\n" +
                          "select(outstream) -> selectedData {select: (select TheString, IntPrimitive from outstream) }\n" +
                          "DefaultSupportCaptureOp(selectedData) {};";

                Module module = null;
                try {
                    module = env.Compiler.ParseModule(epl);
                }
                catch (Exception e) {
                    Assert.Fail(e.Message);
                }

                Assert.AreEqual(1, module.Items.Count);
                env.CompileDeploy(epl);

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "TheGraph");

                env.UndeployAll();
            }
        }
    }
} // end of namespace