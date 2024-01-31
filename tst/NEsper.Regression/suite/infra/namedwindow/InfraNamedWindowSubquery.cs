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
using NUnit.Framework.Legacy;

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
            WithTwoConsumerWindow(execs);
            WithLateConsumerAggregation(execs);
            WithWithFilterInParens(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithFilterInParens(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubqueryWithFilterInParens());
            return execs;
        }

        public static IList<RegressionExecution> WithLateConsumerAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubqueryLateConsumerAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoConsumerWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubqueryTwoConsumerWindow());
            return execs;
        }

        internal class InfraSubqueryWithFilterInParens : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
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
                env.AssertEqualsNew("s0", "c0", expected);
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
                env.AssertRuntime(
                    runtime => ClassicAssert.AreEqual(
                        1L,
                        runtime.VariableService.GetVariableValue(
                            env.DeploymentId("assign"),
                            "myvar"))); // if the subquery-consumer executes first, this will be null

                env.UndeployAll();
            }
        }

        internal class InfraSubqueryLateConsumerAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));

                env.CompileDeploy("@name('s0') select * from MyWindow where (select count(*) from MyWindow) > 0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 1));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }
    }
} // end of namespace