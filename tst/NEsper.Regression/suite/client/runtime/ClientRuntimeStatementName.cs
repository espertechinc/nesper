///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeStatementName
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStatementAllowNameDuplicate(execs);
            WithSingleModuleTwoStatementsNoDep(execs);
            WithStatementNameUnassigned(execs);
            WithStatementNameRuntimeResolverDuplicate(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithStatementNameRuntimeResolverDuplicate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementNameRuntimeResolverDuplicate());
            return execs;
        }

        public static IList<RegressionExecution> WithStatementNameUnassigned(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementNameUnassigned());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleModuleTwoStatementsNoDep(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSingleModuleTwoStatementsNoDep());
            return execs;
        }

        public static IList<RegressionExecution> WithStatementAllowNameDuplicate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAllowNameDuplicate());
            return execs;
        }

        public class ClientRuntimeStatementNameRuntimeResolverDuplicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select * from SupportBean;select * from SupportBean;");
                try {
                    env.Deployment.Deploy(compiled, new DeploymentOptions().WithStatementNameRuntime(env => "x"));
                    Assert.Fail();
                }
                catch (EPDeployException e) {
                    SupportMessageAssertUtil.AssertMessage(
                        e,
                        "Duplicate statement name provide by statement name resolver for statement name 'x'");
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientRuntimeStatementNameUnassigned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select * from SupportBean;select * from SupportBean;");
                EPDeployment deployment;
                try {
                    deployment = env.Deployment.Deploy(compiled);
                }
                catch (EPDeployException e) {
                    throw new EPRuntimeException(e);
                }

                ClassicAssert.AreEqual("stmt-0", deployment.Statements[0].Name);
                ClassicAssert.AreEqual("stmt-1", deployment.Statements[1].Name);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        public class ClientRuntimeStatementAllowNameDuplicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("@name('a') select * from SupportBean;\n");
                env.Deploy(compiled);
                env.Deploy(compiled);
                env.UndeployAll();
            }
        }

        public class ClientRuntimeSingleModuleTwoStatementsNoDep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select IntPrimitive from SupportBean;" +
                    "@name('s1') select TheString from SupportBean;";
                var compiled = env.Compile(epl);

                env.Deploy(compiled).AddListener("s0").AddListener("s1").Milestone(0);

                SendAssert(env, "E1", 10);
                env.Milestone(1);

                SendAssert(env, "E2", 20);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                int intPrimitive)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                env.AssertEqualsNew("s0", "IntPrimitive", intPrimitive);
                env.AssertEqualsNew("s1", "TheString", theString);
            }
        }
    }
}