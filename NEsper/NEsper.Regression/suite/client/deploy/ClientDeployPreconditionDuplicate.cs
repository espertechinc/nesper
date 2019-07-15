///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployPreconditionDuplicate
    {
        private const string MODULE_NAME_UNNAMED = StringValue.UNNAMED;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientDeployPrecondDupNamedWindow());
            execs.Add(new ClientDeployPrecondDupTable());
            execs.Add(new ClientDeployPrecondDupEventType());
            execs.Add(new ClientDeployPrecondDupVariable());
            execs.Add(new ClientDeployPrecondDupExprDecl());
            execs.Add(new ClientDeployPrecondDupScript());
            execs.Add(new ClientDeployPrecondDupContext());
            execs.Add(new ClientDeployPrecondDupIndex());
            return execs;
        }

        private static void TryInvalidDeploy(
            RegressionEnvironment env,
            string epl,
            string text,
            string moduleName)
        {
            var compiled = env.Compile(
                epl,
                options => options
                    .SetAccessModifierNamedWindow(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierTable(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierEventType(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierVariable(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierExpression(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierScript(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierContext(ctx => NameAccessModifier.PUBLIC));
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
                if (!message.Equals("skip")) {
                    SupportMessageAssertUtil.AssertMessage(ex.Message, message);
                }
            }
            catch (EPDeployException ex) {
                Assert.Fail();
            }
        }

        public class ClientDeployPrecondDupNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SimpleWindow#keepall as SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A named window by name 'SimpleWindow'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
                path.Clear();

                epl = "module ABC; create window SimpleWindow#keepall as SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A named window by name 'SimpleWindow'", "ABC");

                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table SimpleTable(col1 string)";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A table by name 'SimpleTable'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create schema MySchema (col1 string)";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "An event type by name 'MySchema'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create variable string myvariable";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A variable by name 'myvariable'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupExprDecl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create expression expr_one {0}";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A declared-expression by name 'expr_one'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create expression double myscript(stringvalue) [0]";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A script by name 'myscript (1 parameters)'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create context MyContext as partition by TheString from SupportBean";
                env.CompileDeploy(epl, path);
                TryInvalidDeploy(env, epl, "A context by name 'MyContext'", MODULE_NAME_UNNAMED);
                env.UndeployAll();
            }
        }

        public class ClientDeployPrecondDupIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string epl;
                EPCompiled compiled;

                env.CompileDeploy("create table MyTable (col1 string primary key, col2 string)", path);
                epl = "create index MyIndexOnTable on MyTable(col2)";
                compiled = env.Compile(epl, path);
                env.Deploy(compiled);
                TryInvalidDeploy(env, compiled, "An index by name 'MyIndexOnTable'", MODULE_NAME_UNNAMED);

                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);
                epl = "create index MyIndexOnNW on MyWindow(IntPrimitive)";
                compiled = env.Compile(epl, path);
                env.Deploy(compiled);
                TryInvalidDeploy(env, compiled, "An index by name 'MyIndexOnNW'", MODULE_NAME_UNNAMED);

                env.UndeployAll();
            }
        }
    }
} // end of namespace