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
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectWithinHaving
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectHavingSubselectWithGroupBy(true));
            execs.Add(new EPLSubselectHavingSubselectWithGroupBy(false));
            return execs;
        }

        internal class EPLSubselectHavingSubselectWithGroupBy : RegressionExecution
        {
            private readonly bool namedWindow;

            public EPLSubselectHavingSubselectWithGroupBy(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfra#unique(Key) as SupportMaxAmountEvent"
                    : "create table MyInfra(Key string primary key, MaxAmount double)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfra select * from SupportMaxAmountEvent", path);

                var stmtText = "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 " +
                               "from SupportBean#groupwin(TheString)#length(2) as sb " +
                               "group by TheString " +
                               "having sum(IntPrimitive) > (select MaxAmount from MyInfra as mw where sb.TheString = mw.Key)";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                var fields = new [] { "c0", "c1" };

                // set some amounts
                env.SendEventBean(new SupportMaxAmountEvent("G1", 10));
                env.SendEventBean(new SupportMaxAmountEvent("G2", 20));
                env.SendEventBean(new SupportMaxAmountEvent("G3", 30));

                // send some events
                env.SendEventBean(new SupportBean("G1", 5));
                env.SendEventBean(new SupportBean("G2", 19));
                env.SendEventBean(new SupportBean("G3", 28));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 21});

                env.SendEventBean(new SupportBean("G2", 18));
                env.SendEventBean(new SupportBean("G1", 4));
                env.SendEventBean(new SupportBean("G3", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G3", 29));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 31});

                env.SendEventBean(new SupportBean("G3", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 33});

                env.SendEventBean(new SupportBean("G1", 6));
                env.SendEventBean(new SupportBean("G2", 2));
                env.SendEventBean(new SupportBean("G3", 26));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G1", 99));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 105});

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 100});

                env.UndeployAll();
            }
        }
    }
} // end of namespace