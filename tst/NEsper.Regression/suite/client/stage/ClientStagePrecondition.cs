///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.stage;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.client.stage
{
    public class ClientStagePrecondition
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStagePreconditionNamedWindow(execs);
            WithStagePreconditionContext(execs);
            WithStagePreconditionVariable(execs);
            WithUnstagePrecondition(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUnstagePrecondition(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageUnstagePrecondition());
            return execs;
        }

        public static IList<RegressionExecution> WithStagePreconditionVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageStagePreconditionVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithStagePreconditionContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientStageStagePreconditionContext());
            return execs;
        }

        public static IList<RegressionExecution> WithStagePreconditionNamedWindow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            // Precondition checking of dependencies does not require each object type as general dependency reporting is tested elsewhere
            execs.Add(new ClientStageStagePreconditionNamedWindow());
            return execs;
        }

        private class ClientStageUnstagePrecondition : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('context') @public create context MyContext initiated by SupportBean", path);
                env.CompileDeploy("@name('stmt') context MyContext select count(*) from SupportBean_S0", path);
                var idCreate = env.DeploymentId("context");
                var idStmt = env.DeploymentId("stmt");

                env.Runtime.StageService.GetStage("ST");
                StageIt(env, "ST", idCreate, idStmt);

                TryInvalidUnstage(env, idCreate);
                TryInvalidUnstage(env, idStmt);

                UnstageIt(env, "ST", idCreate, idStmt);

                env.UndeployAll();
            }
        }

        private class ClientStageStagePreconditionVariable : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('variable') @public create variable int MyVariable", path);
                env.CompileDeploy("@name('stmt') select MyVariable from SupportBean_S0", path);

                var stage = env.Runtime.StageService.GetStage("S1");
                var idCreate = env.DeploymentId("variable");
                var idStmt = env.DeploymentId("stmt");

                TryInvalidStageProvides(env, stage, idCreate, idStmt, "variable 'MyVariable'");
                TryInvalidStageConsumes(env, stage, idStmt, idCreate, "variable 'MyVariable'");

                env.UndeployAll();
            }
        }

        private class ClientStageStagePreconditionContext : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('context') @public create context MyContext initiated by SupportBean", path);
                env.CompileDeploy("@name('stmt') context MyContext select count(*) from SupportBean_S0", path);

                var stage = env.Runtime.StageService.GetStage("S1");
                var idCreate = env.DeploymentId("context");
                var idStmt = env.DeploymentId("stmt");

                TryInvalidStageProvides(env, stage, idCreate, idStmt, "context 'MyContext'");
                TryInvalidStageConsumes(env, stage, idStmt, idCreate, "context 'MyContext'");

                env.CompileDeploy("@name('stmt-2') context MyContext select count(*) from SupportBean_S1", path);
                var idStmt2 = env.DeploymentId("stmt-2");

                TryInvalidStage(env, stage, new string[] { idCreate, idStmt });
                TryInvalidStage(env, stage, new string[] { idStmt2, idStmt });
                TryInvalidStage(env, stage, new string[] { idStmt2, idCreate });

                env.UndeployAll();
            }
        }

        private class ClientStageStagePreconditionNamedWindow : ClientStageRegressionExecution
        {
            public override void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('create') @public create window MyWindow#keepall as SupportBean", path);
                var stage = env.Runtime.StageService.GetStage("S1");
                var idCreate = env.DeploymentId("create");

                var namedWindowObjectName = "named window 'MyWindow'";
                var eventTypeObjectName = "event type 'MyWindow'";
                var namedWindowDependencies = new string[][] {
                    new string[] { "select * from MyWindow", namedWindowObjectName },
                    new string[] { "insert into MyWindow select * from SupportBean", namedWindowObjectName },
                    new string[] { "select (select count(*) from MyWindow) from SupportBean", namedWindowObjectName },
                    new string[] { "on SupportBean delete from MyWindow", namedWindowObjectName },
                    new string[] { "select * from pattern[every MyWindow]", eventTypeObjectName },
                    new string[] {
                        "on MyWindow merge MyWindow where 1=1 when not matched then insert into ABC select *",
                        namedWindowObjectName
                    }
                };
                foreach (var line in namedWindowDependencies) {
                    AssertPrecondition(env, path, stage, idCreate, line[1], line[0]);
                }

                env.UndeployAll();
            }
        }

        private static void AssertPrecondition(
            RegressionEnvironment env,
            RegressionPath path,
            EPStage stage,
            string idCreate,
            string objectName,
            string epl)
        {
            env.CompileDeploy("@name('tester') " + epl, path);
            var idTester = env.DeploymentId("tester");
            TryInvalidStageProvides(env, stage, idCreate, idTester, objectName);
            TryInvalidStageConsumes(env, stage, idTester, idCreate, objectName);
            env.UndeployModuleContaining("tester");
        }

        private static void TryInvalidStageProvides(
            RegressionEnvironment env,
            EPStage stage,
            string idStaged,
            string idConsuming,
            string objectName)
        {
            var expected = "Failed to stage deployment '" +
                           idStaged +
                           "': Deployment provides " +
                           objectName +
                           " to deployment '" +
                           idConsuming +
                           "' and must therefore also be staged";
            TryInvalidStage(env, stage, new string[] { idStaged }, expected);
        }

        private static void TryInvalidStageConsumes(
            RegressionEnvironment env,
            EPStage stage,
            string idStaged,
            string idProviding,
            string objectName)
        {
            var expected = "Failed to stage deployment '" +
                           idStaged +
                           "': Deployment consumes " +
                           objectName +
                           " from deployment '" +
                           idProviding +
                           "' and must therefore also be staged";
            TryInvalidStage(env, stage, new string[] { idStaged }, expected);
        }

        private static void TryInvalidStage(
            RegressionEnvironment env,
            EPStage stage,
            string[] idsStaged)
        {
            TryInvalidStage(env, stage, idsStaged, "skip");
        }

        private static void TryInvalidStage(
            RegressionEnvironment env,
            EPStage stage,
            string[] idsStaged,
            string message)
        {
            try {
                stage.Stage(Arrays.AsList(idsStaged));
                Assert.Fail();
            }
            catch (EPStagePreconditionException ex) {
                if (!message.Equals("skip")) {
                    AssertMessage(ex.Message, message);
                }
            }
            catch (EPStageException) {
                throw;
            }
        }

        private static void TryInvalidUnstage(
            RegressionEnvironment env,
            string id)
        {
            try {
                env.StageService.GetStage("ST").Unstage(Arrays.AsList(id));
                Assert.Fail();
            }
            catch (EPStageException) {
                // expected
            }
        }
    }
} // end of namespace