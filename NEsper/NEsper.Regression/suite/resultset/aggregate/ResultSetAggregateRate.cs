///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil; // AssertProps
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateRate
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateRateDataNonWindowed());
            execs.Add(new ResultSetAggregateRateDataWindowed());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "myrate".SplitCsv();

            SendTimer(env, 1000);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            env.MilestoneInc(milestone);

            SendTimer(env, 1200);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(env, 1600);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            env.MilestoneInc(milestone);

            SendTimer(env, 1600);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(env, 9000);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            SendTimer(env, 9200);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            env.MilestoneInc(milestone);

            SendTimer(env, 10999);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

            env.MilestoneInc(milestone);

            SendTimer(env, 11100);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {0.7});

            SendTimer(env, 11101);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {0.8});

            env.MilestoneInc(milestone);

            SendTimer(env, 11200);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {0.8});

            SendTimer(env, 11600);
            SendEvent(env);
            AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {0.7});
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longPrimitive,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.LongPrimitive = longPrimitive;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEvent(RegressionEnvironment env)
        {
            var bean = new SupportBean();
            env.SendEventBean(bean);
        }

        // rate implementation does not require a data window (may have one)
        // advantage: not retaining events, only timestamp data points
        // disadvantage: output rate limiting without snapshot may be less accurate rate
        internal class ResultSetAggregateRateDataNonWindowed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select rate(10) as myrate from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env, milestone);

                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertion(env, milestone);

                env.UndeployAll();

                TryInvalidCompile(
                    env,
                    "select rate() from SupportBean",
                    "Failed to validate select-clause expression 'rate(*)': The rate aggregation function minimally requires a numeric constant or expression as a parameter. [select rate() from SupportBean]");
                TryInvalidCompile(
                    env,
                    "select rate(true) from SupportBean",
                    "Failed to validate select-clause expression 'rate(true)': The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation [select rate(true) from SupportBean]");
            }
        }

        internal class ResultSetAggregateRateDataWindowed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "myrate,myqtyrate".SplitCsv();
                var epl =
                    "@Name('s0') select RATE(longPrimitive) as myrate, RATE(longPrimitive, IntPrimitive) as myqtyrate from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 1000, 10);
                AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null, null});

                env.Milestone(0);

                SendEvent(env, 1200, 0);
                AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null, null});

                SendEvent(env, 1300, 0);
                AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null, null});

                env.Milestone(1);

                SendEvent(env, 1500, 14);
                AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3 * 1000 / 500d, 14 * 1000 / 500d});

                env.Milestone(2);

                SendEvent(env, 2000, 11);
                AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3 * 1000 / 800d, 25 * 1000 / 800d});

                TryInvalidCompile(
                    env,
                    "select rate(longPrimitive) as myrate from SupportBean",
                    "Failed to validate select-clause expression 'rate(longPrimitive)': The rate aggregation function in the timestamp-property notation requires data windows [select rate(longPrimitive) as myrate from SupportBean]");
                TryInvalidCompile(
                    env,
                    "select rate(current_timestamp) as myrate from SupportBean#time(20)",
                    "Failed to validate select-clause expression 'rate(current_timestamp())': The rate aggregation function does not allow the current runtime timestamp as a parameter [select rate(current_timestamp) as myrate from SupportBean#time(20)]");
                TryInvalidCompile(
                    env,
                    "select rate(TheString) as myrate from SupportBean#time(20)",
                    "Failed to validate select-clause expression 'rate(TheString)': The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation [select rate(TheString) as myrate from SupportBean#time(20)]");

                env.UndeployAll();
            }
        }

        public class RateSendRunnable
        {
            private readonly RegressionEnvironment env;

            public RateSendRunnable(RegressionEnvironment env)
            {
                this.env = env;
            }

            public void Run()
            {
                var bean = new SupportBean();
                bean.LongPrimitive = DateTimeHelper.CurrentTimeMillis;
                env.SendEventBean(bean);
            }
        }
    }
} // end of namespace