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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitFirstHaving
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetHavingNoAvgOutputFirstEvents());
            execs.Add(new ResultSetHavingNoAvgOutputFirstMinutes());
            execs.Add(new ResultSetHavingAvgOutputFirstEveryTwoMinutes());
            return execs;
        }

        private static void TryAssertion2Events(RegressionEnvironment env)
        {
            SendBeanEvent(env, 1);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 2);
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 9);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 1);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 1);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 2);
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 1);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 2);
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 2);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendBeanEvent(env, 2);
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            double doublePrimitive)
        {
            var b = new SupportBean();
            b.DoublePrimitive = doublePrimitive;
            env.SendEventBean(b);
        }

        internal class ResultSetHavingNoAvgOutputFirstEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var query =
                    "@Name('s0') select DoublePrimitive from SupportBean having DoublePrimitive > 1 output first every 2 events";
                env.CompileDeploy(query).AddListener("s0");

                TryAssertion2Events(env);
                env.UndeployAll();

                // test joined
                query =
                    "@Name('s0') select DoublePrimitive from SupportBean#lastevent,SupportBean_ST0#lastevent st0 having DoublePrimitive > 1 output first every 2 events";
                env.CompileDeploy(query).AddListener("s0");
                env.SendEventBean(new SupportBean_ST0("ID", 1));
                TryAssertion2Events(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingNoAvgOutputFirstMinutes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = new [] { "val0" };
                var query =
                    "@Name('s0') select sum(DoublePrimitive) as val0 from SupportBean#length(5) having sum(DoublePrimitive) > 100 output first every 2 seconds";
                env.CompileDeploy(query).AddListener("s0");

                SendBeanEvent(env, 10);
                SendBeanEvent(env, 80);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(1000);
                SendBeanEvent(env, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {101d});

                SendBeanEvent(env, 1);

                env.AdvanceTime(2999);
                SendBeanEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(3000);
                SendBeanEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {114d});

                env.AdvanceTime(4999);
                SendBeanEvent(env, 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(5000);
                SendBeanEvent(env, 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {102d});

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingAvgOutputFirstEveryTwoMinutes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var query =
                    "@Name('s0') select DoublePrimitive, avg(DoublePrimitive) from SupportBean having DoublePrimitive > 2*avg(DoublePrimitive) output first every 2 minutes";
                env.CompileDeploy(query).AddListener("s0");

                SendBeanEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, 9);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace