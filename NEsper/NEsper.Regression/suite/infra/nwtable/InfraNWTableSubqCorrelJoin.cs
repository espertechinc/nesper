///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqCorrelJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            // named window
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(true, false)); // disable index-share
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(true, true)); // enable-index-share

            // table
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(false, false));
            return execs;
        }

        internal class InfraNWTableSubqCorrelJoinAssertion : RegressionExecution
        {
            private readonly bool enableIndexShareCreate;
            private readonly bool namedWindow;

            public InfraNWTableSubqCorrelJoinAssertion(
                bool namedWindow,
                bool enableIndexShareCreate)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = namedWindow
                    ? "create window MyInfra#unique(TheString) as select * from SupportBean"
                    : "create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
                if (enableIndexShareCreate) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean", path);

                var consumeEpl =
                    "@name('s0') select (select IntPrimitive from MyInfra where TheString = S1.P10) as val from SupportBean_S0#lastevent as S0, SupportBean_S1#lastevent as S1";
                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                var fields = new [] { "val" };

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E3", 30));

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(1, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20});

                env.SendEventBean(new SupportBean_S0(1, "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20});

                env.SendEventBean(new SupportBean_S1(1, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});

                env.SendEventBean(new SupportBean_S1(1, "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {30});

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }
    }
} // end of namespace