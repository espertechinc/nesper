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

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateLeaving : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();

            var epl = "@Name('s0') select leaving() as val from SupportBean#length(3)";
            env.CompileDeploy(epl).AddListener("s0");
            RunAssertion(env, milestone);

            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");

            RunAssertion(env, milestone);

            env.UndeployAll();

            TryInvalidCompile(
                env,
                "select leaving(1) from SupportBean",
                "Failed to validate select-clause expression 'leaving(1)': The 'leaving' function expects no parameters");
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new [] { "val" };

            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {false});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {false});

            env.SendEventBean(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {false});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {true});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {true});
        }
    }
} // end of namespace