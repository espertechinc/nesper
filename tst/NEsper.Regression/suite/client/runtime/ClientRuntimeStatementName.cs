///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeStatementName
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithtatementNameDuplicate(execs);
            WithingleModuleTwoStatementsNoDep(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithingleModuleTwoStatementsNoDep(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSingleModuleTwoStatementsNoDep());
            return execs;
        }

        public static IList<RegressionExecution> WithtatementNameDuplicate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementNameDuplicate());
            return execs;
        }

        public class ClientRuntimeStatementNameDuplicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("@Name('a') select * from SupportBean;\n");

                try {
                    env.Deployment.Deploy(compiled);

                    env.Milestone(0);

                    env.Deployment.Deploy(compiled);
                }
                catch (EPDeployException ex) {
                    Assert.Fail(ex.Message);
                }

                env.UndeployAll();
            }
        }

        public class ClientRuntimeSingleModuleTwoStatementsNoDep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select IntPrimitive from SupportBean;" +
                    "@Name('s1') select TheString from SupportBean;";
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
                Assert.AreEqual(intPrimitive, env.Listener("s0").AssertOneGetNewAndReset().Get("IntPrimitive"));
                Assert.AreEqual(theString, env.Listener("s1").AssertOneGetNewAndReset().Get("TheString"));
            }
        }
    }
} // end of namespace