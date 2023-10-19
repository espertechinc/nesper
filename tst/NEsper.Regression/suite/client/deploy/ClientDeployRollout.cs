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
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployRollout
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFourInterdepModulesWStmtId(execs);
            WithTwoInterdepModules(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRolloutInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoInterdepModules(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRolloutTwoInterdepModules());
            return execs;
        }

        public static IList<RegressionExecution> WithFourInterdepModulesWStmtId(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRolloutFourInterdepModulesWStmtId());
            return execs;
        }

        private class ClientDeployRolloutFourInterdepModulesWStmtId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var @base = env.Compile("@name('basevar') @public create constant variable int basevar = 1");
                var child0 = env.Compile(
                    "@name('s0') select basevar from SupportBean",
                    new RegressionPath().Add(@base));
                var child1 = env.Compile(
                    "@name('child1var') @public create constant variable int child1var = 2;\n" +
                    "@name('s1') select basevar, child1var from SupportBean;\n",
                    new RegressionPath().Add(@base));
                var child11 = env.Compile(
                    "@name('s2') select basevar, child1var from SupportBean;\n",
                    new RegressionPath().Add(@base).Add(child1));

                env.Rollout(Arrays.AsList(ToRolloutItems(@base, child0, child1, child11)), null);
                env.AddListener("s0").AddListener("s1").AddListener("s2");

                SendAssert(env, "s1,s2");

                env.Milestone(0);

                SendAssert(env, "s1,s2");
                AssertStatementIds(env, "basevar,s0,child1var,s1,s2", 1, 2, 3, 4, 5);

                var item = new EPDeploymentRolloutCompiled(
                    env.Compile(
                        "@name('s3') select basevar, child1var from SupportBean",
                        new RegressionPath().Add(@base).Add(child1)),
                    null);
                env.Rollout(Collections.SingletonList(item), null).AddListener("s3");
                var deploymentChild11 = env.Deployment.GetDeployment(env.DeploymentId("s2"));
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new string[] { env.DeploymentId("basevar"), env.DeploymentId("child1var") },
                    deploymentChild11.DeploymentIdDependencies);

                env.Milestone(1);

                SendAssert(env, "s1,s2,s3");
                AssertStatementIds(env, "basevar,s0,child1var,s1,s2,s3", 1, 2, 3, 4, 5, 6);

                env.UndeployAll();

                env.Milestone(2);

                env.CompileDeploy("@name('s1') select * from SupportBean");
                TryInvalidRollout(
                    env,
                    "A precondition is not satisfied: Required dependency variable 'basevar' cannot be found",
                    0,
                    typeof(EPDeployPreconditionException),
                    child0);
                AssertStatementIds(env, "s1", 7);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }

            private void SendAssert(
                RegressionEnvironment env,
                string stmtNameCsv)
            {
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "basevar", 1);
                foreach (var stmtName in stmtNameCsv.SplitCsv()) {
                    EPAssertionUtil.AssertProps(
                        env.Listener(stmtName).AssertOneGetNewAndReset(),
                        "basevar,child1var".SplitCsv(),
                        new object[] { 1, 2 });
                }
            }
        }

        private class ClientDeployRolloutInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = env.Compile("@name('s0') @public @buseventtype create schema MyEvent(p string)");
                var selectMyEvent = env.Compile(
                    "@name('s0') select * from MyEvent",
                    new RegressionPath().Add(type));
                var selectSB = env.Compile("@name('s0') select * from SupportBean");
                var selectSBParameterized =
                    env.Compile("@name('s0') select * from SupportBean(theString = ?::string)");
                env.CompileDeploy("@name('s1') select * from SupportBean");

                // dependency not found
                var msg =
                    "A precondition is not satisfied: Required dependency event type 'MyEvent' cannot be found";
                TryInvalidRollout(env, msg, 1, typeof(EPDeployPreconditionException), selectSB, selectMyEvent);
                TryInvalidRollout(env, msg, 0, typeof(EPDeployPreconditionException), selectMyEvent);
                TryInvalidRollout(
                    env,
                    msg,
                    2,
                    typeof(EPDeployPreconditionException),
                    selectSB,
                    selectSB,
                    selectMyEvent);
                TryInvalidRollout(
                    env,
                    msg,
                    1,
                    typeof(EPDeployPreconditionException),
                    selectSB,
                    selectMyEvent,
                    selectSB,
                    selectSB);

                // already defined
                TryInvalidRollout(
                    env,
                    "Event type by name 'MyEvent' already registered",
                    1,
                    typeof(EPDeployException),
                    type,
                    type);

                // duplicate deployment id
                TryInvalidRollout(
                    env,
                    "Deployment id 'a' occurs multiple times in the rollout",
                    1,
                    typeof(EPDeployException),
                    new EPDeploymentRolloutCompiled(selectSB, new DeploymentOptions().WithDeploymentId("a")),
                    new EPDeploymentRolloutCompiled(selectSB, new DeploymentOptions().WithDeploymentId("a")));

                // deployment id exists
                TryInvalidRollout(
                    env,
                    "Deployment by id '" + env.DeploymentId("s1") + "' already exists",
                    1,
                    typeof(EPDeployDeploymentExistsException),
                    new EPDeploymentRolloutCompiled(selectSB, new DeploymentOptions().WithDeploymentId("a")),
                    new EPDeploymentRolloutCompiled(
                        selectSB,
                        new DeploymentOptions().WithDeploymentId(env.DeploymentId("s1"))));

                // substitution param problem
                TryInvalidRollout(
                    env,
                    "Substitution parameters have not been provided: Statement 's0' has 1 substitution parameters",
                    1,
                    typeof(EPDeploySubstitutionParameterException),
                    selectSB,
                    selectSBParameterized);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class ClientDeployRolloutTwoInterdepModules : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplOne = "@name('type') @public @buseventtype create schema MyEvent(p string)";
                var compiledOne = env.Compile(eplOne, path);
                var eplTwo = "@name('s0') select * from MyEvent";
                var compiledTwo = env.Compile(eplTwo, path);

                IList<EPDeploymentRolloutCompiled> items = new List<EPDeploymentRolloutCompiled>();
                items.Add(new EPDeploymentRolloutCompiled(compiledOne));
                items.Add(new EPDeploymentRolloutCompiled(compiledTwo));

                EPDeploymentRollout rollout;
                try {
                    rollout = env.Deployment.Rollout(items);
                    env.AddListener("s0");
                }
                catch (EPDeployException ex) {
                    throw new EPRuntimeException(ex);
                }

                Assert.AreEqual(2, rollout.Items.Length);
                AssertDeployment(env, rollout.Items[0].Deployment, "type");
                AssertDeployment(env, rollout.Items[1].Deployment, "s0");

                AssertSendAndReceive(env, "a");

                env.Milestone(0);

                AssertSendAndReceive(env, "b");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }

            private void AssertDeployment(
                RegressionEnvironment env,
                EPDeployment deployment,
                string statementName)
            {
                Assert.AreEqual(1, deployment.Statements.Length);
                Assert.AreEqual(statementName, deployment.Statements[0].Name);
                Assert.AreEqual(env.DeploymentId(statementName), deployment.DeploymentId);
            }

            private void AssertSendAndReceive(
                RegressionEnvironment env,
                string value)
            {
                env.SendEventMap(Collections.SingletonDataMap("p", value), "MyEvent");
                env.AssertEqualsNew("s0", "p", value);
            }
        }

        private static EPDeploymentRolloutCompiled[] ToRolloutItems(params EPCompiled[] compileds)
        {
            var items = new EPDeploymentRolloutCompiled[compileds.Length];
            for (var i = 0; i < compileds.Length; i++) {
                items[i] = new EPDeploymentRolloutCompiled(compileds[i]);
            }

            return items;
        }

        private static void TryInvalidRollout(
            RegressionEnvironment env,
            string expectedMsg,
            int rolloutNumber,
            Type exceptionType,
            params EPCompiled[] compileds)
        {
            TryInvalidRollout(env, expectedMsg, rolloutNumber, exceptionType, ToRolloutItems(compileds));
        }

        private static void TryInvalidRollout(
            RegressionEnvironment env,
            string expectedMsg,
            int rolloutNumber,
            Type exceptionType,
            params EPDeploymentRolloutCompiled[] items)
        {
            try {
                env.Runtime.DeploymentService.Rollout(Arrays.AsList(items));
                Assert.Fail();
            }
            catch (EPDeployException ex) {
                Assert.AreEqual(rolloutNumber, ex.RolloutItemNumber);
                SupportMessageAssertUtil.AssertMessage(ex.Message, expectedMsg);
                Assert.AreEqual(exceptionType, ex.GetType());
            }

            try {
                env.DeploymentId("s0");
                Assert.Fail();
            }
            catch (Exception) {
                // expected
            }

            Assert.IsNotNull(env.DeploymentId("s1"));
        }

        private static void AssertStatementIds(
            RegressionEnvironment env,
            string nameCSV,
            params int[] statementIds)
        {
            var names = nameCSV.SplitCsv();
            for (var i = 0; i < names.Length; i++) {
                var index = i;
                env.AssertStatement(
                    names[i],
                    statement => {
                        var spi = (EPStatementSPI)statement;
                        Assert.AreEqual(statementIds[index], spi.StatementId);
                    });
            }
        }
    }
} // end of namespace