///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingStmtMetrics : RegressionExecution
    {
        private readonly TimeSpan USER_GOAL_ONE = TimeSpan.FromMilliseconds(800);
        private readonly TimeSpan USER_GOAL_TWO = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan TOTAL_GOAL_ONE = TimeSpan.FromMilliseconds(1000);
        private readonly TimeSpan TOTAL_GOAL_TWO = TimeSpan.FromMilliseconds(2000);

        public void Run(RegressionEnvironment env)
        {
            SendTimer(env, 1000);

            var statements = new EPStatement[5];
            statements[0] = env.CompileDeploy("@Name('stmt_metrics') select * from " + typeof(StatementMetric).FullName)
                .Statement("stmt_metrics");
            statements[0].AddListener(env.ListenerNew());

            statements[1] = env.CompileDeploy(
                    "@Name('cpuStmtOne') select * from SupportBean(IntPrimitive=1)#keepall where MyMetricFunctions.TakeMillis(LongPrimitive)")
                .Statement("cpuStmtOne");
            statements[1].AddListener(env.ListenerNew());
            statements[2] = env.CompileDeploy(
                    "@Name('cpuStmtTwo') select * from SupportBean(IntPrimitive=2)#keepall where MyMetricFunctions.TakeMillis(LongPrimitive)")
                .Statement("cpuStmtTwo");
            statements[2].AddListener(env.ListenerNew());
            statements[3] = env.CompileDeploy(
                    "@Name('wallStmtThree') select * from SupportBean(IntPrimitive=3)#keepall where MyMetricFunctions.TakeWallTime(LongPrimitive)")
                .Statement("wallStmtThree");
            statements[3].AddListener(env.ListenerNew());
            statements[4] = env.CompileDeploy(
                    "@Name('wallStmtFour') select * from SupportBean(IntPrimitive=4)#keepall where MyMetricFunctions.TakeWallTime(LongPrimitive)")
                .Statement("wallStmtFour");
            statements[4].AddListener(env.ListenerNew());

            SendEvent(env, "E1", 1, USER_GOAL_ONE);
            SendEvent(env, "E2", 2, USER_GOAL_TWO);
            SendEvent(env, "E3", 3, TOTAL_GOAL_ONE);
            SendEvent(env, "E4", 4, TOTAL_GOAL_TWO);

            var listener = env.Listener("stmt_metrics");
            SendTimer(env, 10999);
            Assert.IsFalse(env.Listener("stmt_metrics").IsInvoked);

            SendTimer(env, 11000);
            TryAssertion(env, 11000);

            SendEvent(env, "E1", 1, USER_GOAL_ONE);
            SendEvent(env, "E2", 2, USER_GOAL_TWO);
            SendEvent(env, "E3", 3, TOTAL_GOAL_ONE);
            SendEvent(env, "E4", 4, TOTAL_GOAL_TWO);

            SendTimer(env, 21000);
            TryAssertion(env, 21000);

            // destroy all application stmts
            for (var i = 1; i < 5; i++) {
                env.UndeployModuleContaining(statements[i].Name);
            }

            SendTimer(env, 31000);
            Assert.IsFalse(env.Listener("stmt_metrics").IsInvoked);

            env.UndeployAll();
        }

        private void TryAssertion(
            RegressionEnvironment env,
            long timestamp)
        {
            var fields = new [] { "RuntimeURI","StatementName" };

            var listener = env.Listener("stmt_metrics");
            Assert.AreEqual(4, listener.NewDataList.Count);
            var received = listener.NewDataListFlattened;

            EPAssertionUtil.AssertProps(
                received[0],
                fields,
                new object[] {"default", "cpuStmtOne"});
            EPAssertionUtil.AssertProps(
                received[1],
                fields,
                new object[] {"default", "cpuStmtTwo"});
            EPAssertionUtil.AssertProps(
                received[2],
                fields,
                new object[] {"default", "wallStmtThree"});
            EPAssertionUtil.AssertProps(
                received[3],
                fields,
                new object[] {"default", "wallStmtFour"});

#if !NETCORE
            var userOne = (TimeSpan) received[0].Get("PerformanceMetrics.UserTime");
            var userTwo = (TimeSpan) received[1].Get("PerformanceMetrics.UserTime");
            var totalOne = (TimeSpan) received[2].Get("PerformanceMetrics.TotalTime");
            var totalTwo = (TimeSpan) received[3].Get("PerformanceMetrics.TotalTime");

            var fiftyMillis = TimeSpan.FromMilliseconds(50);

            Assert.That(userOne, Is.GreaterThanOrEqualTo(USER_GOAL_ONE), "userOne=" + userOne);
            Assert.That(userTwo, Is.GreaterThanOrEqualTo(USER_GOAL_TWO), "userTwo=" + userTwo);
            Assert.That(totalOne + fiftyMillis, Is.GreaterThanOrEqualTo(TOTAL_GOAL_ONE), "totalOne=" + totalOne);
            Assert.That(totalTwo + fiftyMillis, Is.GreaterThanOrEqualTo(TOTAL_GOAL_TWO), "totalTwo=" + totalTwo);
#endif

            for (var i = 0; i < 4; i++) {
                Assert.AreEqual(1L, received[i].Get("OutputIStreamCount"));
                Assert.AreEqual(0L, received[i].Get("OutputRStreamCount"));
                Assert.AreEqual(timestamp, received[i].Get("Timestamp"));
            }

            listener.Reset();
        }

        private void SendTimer(
            RegressionEnvironment env,
            long currentTime)
        {
            env.AdvanceTime(currentTime);
        }

        private void SendEvent(
            RegressionEnvironment env,
            string id,
            int intPrimitive,
            TimeSpan timeSpan)
        {
            var longPrimitive = (long) timeSpan.TotalMilliseconds;
            var bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace