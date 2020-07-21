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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowSubquery
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraSubqueryTwoConsumerWindow());
            execs.Add(new InfraSubqueryLateConsumerAggregation());
            execs.Add(new InfraSubqueryWithFilterInParens());
            return execs;
        }

        internal class InfraSubqueryWithFilterInParens : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create window MyWindow#keepall as SupportBean;\n" +
                             "@name('insert') insert into MyWindow select * from SupportBean;\n" +
                             "@name('s0') select exists (select * from MyWindow(TheString='E1')) as c0 from SupportBean_S0;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, false);

                env.SendEventBean(new SupportBean("E2", 1));
                SendAssert(env, false);

                env.SendEventBean(new SupportBean("E1", 1));
                SendAssert(env, true);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                bool expected)
            {
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            }
        }

        internal class InfraSubqueryTwoConsumerWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "\n create window MyWindowTwo#length(1) as (mycount long);" +
                    "\n @Name('insert-count') insert into MyWindowTwo select 1L as mycount from SupportBean;" +
                    "\n create variable long myvar = 0;" +
                    "\n @Name('assign') on MyWindowTwo set myvar = (select mycount from MyWindowTwo);";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(
                    1L,
                    env.Runtime.VariableService.GetVariableValue(
                        env.DeploymentId("assign"),
                        "myvar")); // if the subquery-consumer executes first, this will be null

                env.UndeployAll();
            }
        }

        internal class InfraSubqueryLateConsumerAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));

                env.CompileDeploy("@name('s0') select * from MyWindow where (select count(*) from MyWindow) > 0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 1));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace