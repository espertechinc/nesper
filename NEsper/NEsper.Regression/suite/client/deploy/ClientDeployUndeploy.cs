///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployUndeploy
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientUndeployInvalid());
            execs.Add(new ClientUndeployDependencyChain());
            execs.Add(new ClientUndeployPrecondDepScript());
            execs.Add(new ClientUndeployPrecondDepNamedWindow());
            execs.Add(new ClientUndeployPrecondDepVariable());
            execs.Add(new ClientUndeployPrecondDepContext());
            execs.Add(new ClientUndeployPrecondDepEventType());
            execs.Add(new ClientUndeployPrecondDepExprDecl());
            execs.Add(new ClientUndeployPrecondDepTable());
            execs.Add(new ClientUndeployPrecondDepIndex());
            return execs;
        }

        private static void TryDeployInvalidUndeploy(
            RegressionEnvironment env,
            RegressionPath path,
            string thingStatementName,
            string epl,
            string dependingStatementName,
            string text)
        {
            env.CompileDeploy(epl, path);
            log.Info("Deployed as " + env.DeploymentId(dependingStatementName) + ": " + epl);
            var message = "A precondition is not satisfied: " +
                          text +
                          " cannot be un-deployed as it is referenced by deployment '" +
                          env.DeploymentId(dependingStatementName) +
                          "'";
            TryInvalidUndeploy(env, thingStatementName, message);
            env.UndeployModuleContaining(dependingStatementName);
        }

        private static void TryInvalidUndeploy(
            RegressionEnvironment env,
            string statementName,
            string message)
        {
            try {
                env.Runtime.DeploymentService.Undeploy(env.Statement(statementName).DeploymentId);
                Assert.Fail();
            }
            catch (EPUndeployPreconditionException ex) {
                if (!message.Equals("skip")) {
                    SupportMessageAssertUtil.AssertMessage(ex.Message, message);
                }
            }
            catch (EPUndeployException ex) {
                Assert.Fail();
            }
        }

        internal class ClientUndeployDependencyChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int A = 10", path);
                env.CompileDeploy("create variable int B = A", path);
                env.CompileDeploy("create variable int C = B", path);
                env.CompileDeploy("@Name('s0') create variable int D = C", path);

                Assert.AreEqual(10, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("s0"), "D"));

                env.UndeployAll();
            }
        }

        internal class ClientUndeployInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.Deployment.Undeploy("nofound");
                    Assert.Fail();
                }
                catch (EPUndeployNotFoundException ex) {
                    SupportMessageAssertUtil.AssertMessage(ex.Message, "Deployment id 'nofound' cannot be found");
                }
                catch (EPUndeployException t) {
                    Assert.Fail();
                }
            }
        }

        public class ClientUndeployPrecondDepNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('infra') create window SimpleWindow#keepall as SupportBean", path);

                var text = "Named window 'SimpleWindow'";
                TryDeployInvalidUndeploy(env, path, "infra", "@Name('A') select * from SimpleWindow", "A", text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@Name('B') select (select * from SimpleWindow) from SupportBean",
                    "B",
                    text);

                env.UndeployModuleContaining("infra");
            }
        }

        public class ClientUndeployPrecondDepTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('infra') create table SimpleTable(col1 string primary key, col2 string)",
                    path);

                var text = "Table 'SimpleTable'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@Name('A') select SimpleTable['a'] from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@Name('B') select (select * from SimpleTable) from SupportBean",
                    "B",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@Name('C') create index MyIndex on SimpleTable(col2)",
                    "C",
                    text);

                env.UndeployModuleContaining("infra");
            }
        }

        public class ClientUndeployPrecondDepVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('variable') create variable string varstring", path);

                var text = "Variable 'varstring'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "variable",
                    "@Name('A') select varstring from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "variable",
                    "@Name('B') on SupportBean set varstring='a'",
                    "B",
                    text);

                env.UndeployModuleContaining("variable");
            }
        }

        public class ClientUndeployPrecondDepContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context MyContext partition by TheString from SupportBean",
                    path);

                var text = "Context 'MyContext'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "ctx",
                    "@Name('A') context MyContext select count(*) from SupportBean",
                    "A",
                    text);

                env.UndeployModuleContaining("ctx");
            }
        }

        public class ClientUndeployPrecondDepEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('schema') create schema MySchema(col string)", path);

                var text = "Event type 'MySchema'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "schema",
                    "@Name('A') insert into MySchema select 'a' as col from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(env, path, "schema", "@Name('B') select count(*) from MySchema", "B", text);

                env.UndeployModuleContaining("schema");
            }
        }

        public class ClientUndeployPrecondDepExprDecl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('expr') create expression myexpression { 0 }", path);

                var text = "Declared-expression 'myexpression'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "expr",
                    "@Name('A') select myexpression() as col from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "expr",
                    "@Name('B') select (select myexpression from SupportBean#keepall) from SupportBean",
                    "B",
                    text);

                env.UndeployModuleContaining("expr");
            }
        }

        public class ClientUndeployPrecondDepScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('script') create expression double myscript(stringvalue) [0]", path);

                var text = "Script 'myscript (1 parameters)'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "script",
                    "@Name('A') select myscript('a') as col from SupportBean",
                    "A",
                    text);

                env.UndeployModuleContaining("script");
            }
        }

        public class ClientUndeployPrecondDepIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string text;

                // Table
                env.CompileDeploy("@Name('infra') create table MyTable(k1 string primary key, i1 int)", path);
                env.CompileDeploy("@Name('index') create index MyIndexOnTable on MyTable(i1)", path);

                text = "Index 'MyIndexOnTable'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@Name('A') select * from SupportBean as sb, MyTable as mt where sb.IntPrimitive = mt.i1",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@Name('B') select * from SupportBean as sb where exists (select * from MyTable as mt where sb.IntPrimitive = mt.i1)",
                    "B",
                    text);

                env.UndeployModuleContaining("index");
                env.UndeployModuleContaining("infra");

                // Named window
                env.CompileDeploy("@Name('infra') create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("@Name('index') create index MyIndexOnNW on MyWindow(IntPrimitive)", path);

                text = "Index 'MyIndexOnNW'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@Name('A') on SupportBean_S0 as s0 delete from MyWindow as mw where mw.IntPrimitive = s0.Id",
                    "A",
                    text);

                env.UndeployModuleContaining("index");
                env.UndeployModuleContaining("infra");
            }
        }
    }
} // end of namespace