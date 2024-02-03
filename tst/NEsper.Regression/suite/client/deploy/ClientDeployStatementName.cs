///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployStatementName
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Witht(execs);
            return execs;
        }

        public static IList<RegressionExecution> Witht(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployStatementNameResolveContext());
            return execs;
        }

        internal class ClientDeployStatementNameResolveContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyStatementNameRuntimeResolver.Contexts.Clear();
                var epl = "@name('s0') select * from SupportBean";
                var compiled = env.Compile(epl);
                var options = new DeploymentOptions();
                options.StatementNameRuntime = new MyStatementNameRuntimeResolver().GetStatementName;

                env.Deployment.Deploy(compiled, options);

                env.AssertThat(
                    () => {
                        var ctx = MyStatementNameRuntimeResolver.Contexts[0];
                        ClassicAssert.AreEqual("s0", ctx.StatementName);
                        ClassicAssert.AreEqual(env.DeploymentId("hello"), ctx.DeploymentId);
                        ClassicAssert.AreSame(env.Statement("hello").Annotations, ctx.Annotations);
                        ClassicAssert.AreEqual(epl, ctx.Epl);
                        ClassicAssert.AreEqual("hello", env.Statement("hello").Name);
                    });

                env.Milestone(0);

                env.AssertStatement("hello", statement => ClassicAssert.AreEqual("hello", statement.Name));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        internal class MyStatementNameRuntimeResolver
        {
            public static IList<StatementNameRuntimeContext> Contexts { get; } =
                new List<StatementNameRuntimeContext>();

            public string GetStatementName(StatementNameRuntimeContext env)
            {
                Contexts.Add(env);
                return "hello";
            }
        }
    }
} // end of namespace