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
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.compiler.@internal.parse.ParseHelper;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployResult
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithResultSimple(execs);
            WithStateListener(execs);
            WithGetStmtByDepIdAndName(execs);
            WithSameDeploymentId(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSameDeploymentId(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeploySameDeploymentId());
            return execs;
        }

        public static IList<RegressionExecution> WithGetStmtByDepIdAndName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployGetStmtByDepIdAndName());
            return execs;
        }

        public static IList<RegressionExecution> WithStateListener(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployStateListener());
            return execs;
        }

        public static IList<RegressionExecution> WithResultSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployResultSimple());
            return execs;
        }

        private static EPStatement[] CreateStmts(
            RegressionEnvironment env,
            string[] deploymentIds,
            string[] statementNames)
        {
            ClassicAssert.AreEqual(deploymentIds.Length, statementNames.Length);
            var statements = new EPStatement[statementNames.Length];
            var compiled = env.Compile("select * from SupportBean");

            for (var i = 0; i < statementNames.Length; i++) {
                var num = i;
                try {
                    var deployed = env.Deployment.Deploy(
                        compiled,
                        new DeploymentOptions()
                            .WithDeploymentId(deploymentIds[i])
                            .WithStatementNameRuntime(_ => statementNames[num]));
                    statements[i] = deployed.Statements[0];
                }
                catch (EPDeployException e) {
                    throw new EPException(e);
                }
            }

            return statements;
        }


        private static void AssertEvent(
            DeploymentStateEvent @event,
            bool isDeploy,
            string deploymentId,
            string runtimeURI,
            int numStatements,
            int rolloutItemNumber)
        {
            ClassicAssert.AreEqual(
                isDeploy ? typeof(DeploymentStateEventDeployed) : typeof(DeploymentStateEventUndeployed),
                @event.GetType());
            ClassicAssert.AreEqual(deploymentId, @event.DeploymentId);
            ClassicAssert.AreEqual(runtimeURI, @event.RuntimeURI);
            ClassicAssert.AreEqual(numStatements, @event.Statements.Length);
            ClassicAssert.AreEqual(rolloutItemNumber, @event.RolloutItemNumber);
        }

        internal class ClientDeploySameDeploymentId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select * from SupportBean");
                env.Deploy(compiled, new DeploymentOptions().WithDeploymentId("ABC"));

                try {
                    env.Runtime.DeploymentService.Deploy(compiled, new DeploymentOptions().WithDeploymentId("ABC"));
                    Assert.Fail();
                }
                catch (EPDeployException ex) {
                    Assert.That(ex.RolloutItemNumber, Is.EqualTo(-1));
                    SupportMessageAssertUtil.AssertMessage(ex, "Deployment by id 'ABC' already exists");
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        internal class ClientDeployResultSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.ReadCompile("regression/test_module_9.epl");

                EPDeployment result;
                try {
                    result = env.Runtime.DeploymentService.Deploy(compiled);
                }
                catch (EPDeployException ex) {
                    throw new EPException(ex);
                }

                ClassicAssert.IsNotNull(result.DeploymentId);
                ClassicAssert.AreEqual(2, result.Statements.Length);
                ClassicAssert.AreEqual(1, env.Deployment.Deployments.Length);

                env.AssertStatement(
                    "StmtOne",
                    statement => ClassicAssert.AreEqual(
                        "@Name(\"StmtOne\")" +
                        NEWLINE +
                        "create schema MyEvent(id String, val1 int, val2 int)",
                        statement.GetProperty(StatementProperty.EPL)));
                env.AssertStatement(
                    "StmtTwo",
                    statement => ClassicAssert.AreEqual(
                        "@Name(\"StmtTwo\")" +
                        NEWLINE +
                        "select * from MyEvent",
                        statement.GetProperty(StatementProperty.EPL)));

                ClassicAssert.AreEqual(0, result.DeploymentIdDependencies.Length);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        internal class ClientDeployStateListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportDeploymentStateListener.Reset();
                var listener = new SupportDeploymentStateListener();
                env.Deployment.AddDeploymentStateListener(listener);

                env.CompileDeploy("@name('s0') select * from SupportBean");
                var deploymentId = env.DeploymentId("s0");
                AssertEvent(
                    SupportDeploymentStateListener.GetSingleEventAndReset(),
                    true,
                    deploymentId,
                    env.RuntimeURI,
                    1,
                    -1);

                env.UndeployAll();
                AssertEvent(
                    SupportDeploymentStateListener.GetSingleEventAndReset(),
                    false,
                    deploymentId,
                    env.RuntimeURI,
                    1,
                    -1);

                Assert.That(() => env.Deployment.DeploymentStateListeners.Current, Throws.Nothing);
                env.Deployment.RemoveDeploymentStateListener(listener);
                ClassicAssert.IsFalse(env.Deployment.DeploymentStateListeners.MoveNext());

                env.Deployment.AddDeploymentStateListener(listener);
                env.Deployment.RemoveAllDeploymentStateListeners();
                ClassicAssert.IsFalse(env.Deployment.DeploymentStateListeners.MoveNext());

                env.Deployment.AddDeploymentStateListener(listener);
                var compiledOne = env.Compile(
                    "@name('s0') select * from SupportBean;\n @name('s1') select * from SupportBean;\n");
                var compiledTwo = env.Compile("@name('s2') select * from SupportBean");
                var rolloutItems = new List<EPDeploymentRolloutCompiled>() {
                    new EPDeploymentRolloutCompiled(compiledOne),
                    new EPDeploymentRolloutCompiled(compiledTwo)
                };
                env.Rollout(rolloutItems, null);
                var events = SupportDeploymentStateListener.GetNEventsAndReset(2);
                AssertEvent(events[0], true, env.DeploymentId("s0"), env.RuntimeURI, 2, 0);
                AssertEvent(events[1], true, env.DeploymentId("s2"), env.RuntimeURI, 1, 1);
                env.Deployment.RemoveAllDeploymentStateListeners();

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        internal class ClientDeployGetStmtByDepIdAndName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var deploymentIds = new[] { "A", "B", "C", "D", "E" };
                var names = new[] { "s1", "s2", "s3--0", "s3", "s3" };

                var stmts = CreateStmts(env, deploymentIds, names);
                for (var i = 0; i < stmts.Length; i++) {
                    ClassicAssert.AreSame(stmts[i], env.Deployment.GetStatement(deploymentIds[i], names[i]));
                }

                // test statement name trim
                env.CompileDeploy("@name(' stmt0  ') select * from SupportBean_S0");
                ClassicAssert.IsNotNull(env.Deployment.GetStatement(env.DeploymentId("stmt0"), "stmt0"));

                try {
                    env.Deployment.GetStatement(null, null);
                    Assert.Fail();
                }
                catch (ArgumentException ex) {
                    ClassicAssert.AreEqual("Missing deployment-id parameter", ex.Message);
                }

                try {
                    env.Deployment.GetStatement("x", null);
                    Assert.Fail();
                }
                catch (ArgumentException ex) {
                    ClassicAssert.AreEqual("Missing statement-name parameter", ex.Message);
                }

                ClassicAssert.IsNull(env.Deployment.GetStatement("x", "y"));
                ClassicAssert.IsNull(env.Deployment.GetStatement(env.DeploymentId("stmt0"), "y"));
                ClassicAssert.IsNull(env.Deployment.GetStatement("x", "stmt0"));
                ClassicAssert.IsNotNull(env.Deployment.GetStatement(env.DeploymentId("stmt0"), "stmt0"));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }
    }
} // end of namespace