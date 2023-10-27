///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework; // assertEquals

// fail

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployUndeploy
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientDeployUndeploy));

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInvalid(execs);
            WithDependencyChain(execs);
            WithPrecondDepScript(execs);
            WithPrecondDepNamedWindow(execs);
            WithPrecondDepVariable(execs);
            WithPrecondDepContext(execs);
            WithPrecondDepEventType(execs);
            WithPrecondDepExprDecl(execs);
            WithPrecondDepTable(execs);
            WithPrecondDepIndex(execs);
            WithPrecondDepClass(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepClass());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepTable());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepExprDecl(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepExprDecl());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepContext());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecondDepScript(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployPrecondDepScript());
            return execs;
        }

        public static IList<RegressionExecution> WithDependencyChain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployDependencyChain());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientUndeployInvalid());
            return execs;
        }

        private class ClientUndeployDependencyChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int A = 10", path);
                env.CompileDeploy("@public create variable int B = A", path);
                env.CompileDeploy("@public create variable int C = B", path);
                env.CompileDeploy("@name('s0') @public create variable int D = C", path);

                Assert.AreEqual(10, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("s0"), "D"));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ClientUndeployInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                try {
                    env.Deployment.Undeploy("nofound");
                    Assert.Fail();
                }
                catch (EPUndeployNotFoundException ex) {
                    SupportMessageAssertUtil.AssertMessage(ex.Message, "Deployment Id 'nofound' cannot be found");
                }
                catch (EPUndeployException) {
                    Assert.Fail();
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('infra') @public create window SimpleWindow#keepall as SupportBean", path);

                var text = "Named window 'SimpleWindow'";
                TryDeployInvalidUndeploy(env, path, "infra", "@name('A') select * from SimpleWindow", "A", text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@name('B') select (select * from SimpleWindow) from SupportBean",
                    "B",
                    text);

                env.UndeployModuleContaining("infra");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('infra') @public create table SimpleTable(col1 string primary key, col2 string)",
                    path);

                var text = "Table 'SimpleTable'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@name('A') select SimpleTable['a'] from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@name('B') select (select * from SimpleTable) from SupportBean",
                    "B",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "infra",
                    "@name('C') create index MyIndex on SimpleTable(col2)",
                    "C",
                    text);

                env.UndeployModuleContaining("infra");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('variable') @public create variable string varstring", path);

                var text = "Variable 'varstring'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "variable",
                    "@name('A') select varstring from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "variable",
                    "@name('B') on SupportBean set varstring='a'",
                    "B",
                    text);

                env.UndeployModuleContaining("variable");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyContext partition by TheString from SupportBean",
                    path);

                var text = "Context 'MyContext'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "ctx",
                    "@name('A') context MyContext select count(*) from SupportBean",
                    "A",
                    text);

                env.UndeployModuleContaining("ctx");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('schema') @public create schema MySchema(col string)", path);

                var text = "Event type 'MySchema'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "schema",
                    "@name('A') insert into MySchema select 'a' as col from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(env, path, "schema", "@name('B') select count(*) from MySchema", "B", text);

                env.UndeployModuleContaining("schema");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepExprDecl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('expr') @public create expression myexpression { 0 }", path);

                var text = "Declared-expression 'myexpression'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "expr",
                    "@name('A') select myexpression() as col from SupportBean",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "expr",
                    "@name('B') select (select myexpression from SupportBean#keepall) from SupportBean",
                    "B",
                    text);

                env.UndeployModuleContaining("expr");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('clazz') @public create inlined_class \"\"\" public class MyClass { public static String doIt() { return \"def\"; } }\"\"\"",
                    path);

                var text = "Application-inlined class 'MyClass'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "clazz",
                    "@name('A') select MyClass.doIt() as col from SupportBean",
                    "A",
                    text);

                env.UndeployModuleContaining("clazz");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('script') @public create expression double myscript(stringvalue) [0]", path);

                var text = "Script 'myscript (1 parameters)'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "script",
                    "@name('A') select myscript('a') as col from SupportBean",
                    "A",
                    text);

                env.UndeployModuleContaining("script");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientUndeployPrecondDepIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string text;

                // Table
                env.CompileDeploy("@name('infra') @public create table MyTable(k1 string primary key, i1 int)", path);
                env.CompileDeploy("@name('index') @public create index MyIndexOnTable on MyTable(i1)", path);

                text = "Index 'MyIndexOnTable'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@name('A') select * from SupportBean as sb, MyTable as mt where sb.IntPrimitive = mt.i1",
                    "A",
                    text);
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@name('B') select * from SupportBean as sb where exists (select * from MyTable as mt where sb.IntPrimitive = mt.i1)",
                    "B",
                    text);

                env.UndeployModuleContaining("index");
                env.UndeployModuleContaining("infra");

                // Named window
                env.CompileDeploy("@name('infra') @public create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("@name('index') @public create index MyIndexOnNW on MyWindow(IntPrimitive)", path);

                text = "Index 'MyIndexOnNW'";
                TryDeployInvalidUndeploy(
                    env,
                    path,
                    "index",
                    "@name('A') on SupportBean_S0 as s0 delete from MyWindow as mw where mw.IntPrimitive = s0.Id",
                    "A",
                    text);

                env.UndeployModuleContaining("index");
                env.UndeployModuleContaining("infra");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
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
    }
} // end of namespace