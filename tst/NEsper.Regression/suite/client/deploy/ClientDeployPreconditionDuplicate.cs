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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployPreconditionDuplicate
    {
        private const string MODULE_NAME_UNNAMED = StringValue.UNNAMED;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNamedWindow(execs);
            WithTable(execs);
            WithEventType(execs);
            WithVariable(execs);
            WithExprDecl(execs);
            WithScript(execs);
            WithContext(execs);
            WithIndex(execs);
            WithClass(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupClass());
            return execs;
        }

        public static IList<RegressionExecution> WithIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupContext());
            return execs;
        }

        public static IList<RegressionExecution> WithScript(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupScript());
            return execs;
        }

        public static IList<RegressionExecution> WithExprDecl(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupExprDecl());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupTable());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupNamedWindow());
            return execs;
        }

        public class ClientDeployPrecondDupNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create window SimpleWindow#keepall as SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A named window by name 'SimpleWindow'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
                path.Clear();

                epl = "module ABC; @public create window SimpleWindow#keepall as SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A named window by name 'SimpleWindow'", "ABC", options);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt => opt.AccessModifierTable = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create table SimpleTable(col1 string)";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A table by name 'SimpleTable'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create schema MySchema (col1 string)";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "An event type by name 'MySchema'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.AccessModifierVariable = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create variable string myvariable";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A variable by name 'myvariable'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupExprDecl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.AccessModifierExpression = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create expression expr_one {0}";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A declared-expression by name 'expr_one'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt => opt.AccessModifierScript = ctx => NameAccessModifier.PUBLIC;
                var path = new RegressionPath();
                var epl = "@public create expression double myscript(stringvalue) [0]";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A script by name 'myscript (1 parameters)'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.SetAccessModifierInlinedClass(ctx => NameAccessModifier.PUBLIC);
                var path = new RegressionPath();
                var epl =
                    "@public create inlined_class \"\"\" public class MyClass { public static String doIt() { return \"def\"; } }\"\"\"";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(
                    env,
                    epl,
                    "An application-inlined class by name 'MyClass'",
                    MODULE_NAME_UNNAMED,
                    options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Consumer<CompilerOptions> options = opt =>
                    opt.SetAccessModifierContext(ctx => NameAccessModifier.PUBLIC);
                var path = new RegressionPath();
                var epl = "@public create context MyContext as partition by theString from SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A context by name 'MyContext'", MODULE_NAME_UNNAMED, options);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientDeployPrecondDupIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string epl;
                EPCompiled compiled;

                env.CompileDeploy("@public create table MyTable (col1 string primary key, col2 string)", path);
                epl = "create index MyIndexOnTable on MyTable(col2)";
                compiled = env.Compile(epl, path);
                env.Deploy(compiled);
                TryInvalidDeploy(env, compiled, "An index by name 'MyIndexOnTable'", MODULE_NAME_UNNAMED);

                env.CompileDeploy("@public create window MyWindow#keepall as SupportBean", path);
                epl = "create index MyIndexOnNW on MyWindow(intPrimitive)";
                compiled = env.Compile(epl, path);
                env.Deploy(compiled);
                TryInvalidDeploy(env, compiled, "An index by name 'MyIndexOnNW'", MODULE_NAME_UNNAMED);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private static void TryInvalidDeploy(
            RegressionEnvironment env,
            string epl,
            string text,
            string moduleName,
            Consumer<CompilerOptions> options)
        {
            var compiled = env.Compile(epl, options);
            TryInvalidDeploy(env, compiled, text, moduleName);
        }

        private static void TryInvalidDeploy(
            RegressionEnvironment env,
            EPCompiled compiled,
            string text,
            string moduleName)
        {
            var message = "A precondition is not satisfied: " +
                          text +
                          " has already been created for module '" +
                          moduleName +
                          "'";
            try {
                env.Runtime.DeploymentService.Deploy(compiled);
                Assert.Fail();
            }
            catch (EPDeployPreconditionException ex) {
                Assert.AreEqual(-1, ex.RolloutItemNumber);
                if (!message.Equals("skip")) {
                    SupportMessageAssertUtil.AssertMessage(ex.Message, message);
                }
            }
            catch (EPDeployException ex) {
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
    }
} // end of namespace