///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateNTh : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();

            var epl = "@Name('s0') select " +
                      "TheString, " +
                      "nth(IntPrimitive,0) as int1, " + // current
                      "nth(IntPrimitive,1) as int2 " + // one before
                      "from SupportBean#keepall group by TheString output last every 3 events order by TheString";
            env.CompileDeploy(epl).AddListener("s0");

            RunAssertion(env, milestone);

            env.MilestoneInc(milestone);
            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");

            RunAssertion(env, milestone);

            env.UndeployAll();

            TryInvalidCompile(
                env,
                "select nth() from SupportBean",
                "Failed to validate select-clause expression 'nth(*)': The nth aggregation function requires two parameters, an expression returning aggregation values and a numeric index constant [select nth() from SupportBean]");
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new [] { "TheString","int1","int2" };

            env.SendEventBean(new SupportBean("G1", 10));
            env.SendEventBean(new SupportBean("G2", 11));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G1", 12));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"G1", 12, 10}, new object[] {"G2", 11, null}});

            env.SendEventBean(new SupportBean("G2", 30));
            env.SendEventBean(new SupportBean("G2", 20));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G2", 25));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"G2", 25, 20}});

            env.SendEventBean(new SupportBean("G1", -1));
            env.SendEventBean(new SupportBean("G1", -2));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G2", 8));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"G1", -2, -1}, new object[] {"G2", 8, 25}});
        }
    }
} // end of namespace