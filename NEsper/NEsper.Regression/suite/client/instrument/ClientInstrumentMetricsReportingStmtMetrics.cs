///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingStmtMetrics : RegressionExecution
    {
        private const long CPU_GOAL_ONE_NANO = 80 * 1000 * 1000;
        private const long CPU_GOAL_TWO_NANO = 50 * 1000 * 1000;
        private const long WALL_GOAL_ONE_MSEC = 200;
        private const long WALL_GOAL_TWO_MSEC = 400;

        public void Run(RegressionEnvironment env)
        {
            SendTimer(env, 1000);

            var statements = new EPStatement[5];
            statements[0] = env.CompileDeploy("@Name('stmt_metrics') select * from " + typeof(StatementMetric).Name)
                .Statement("stmt_metrics");
            statements[0].AddListener(env.ListenerNew());

            statements[1] = env.CompileDeploy(
                    "@Name('cpuStmtOne') select * from SupportBean(IntPrimitive=1)#keepall where MyMetricFunctions.takeCPUTime(LongPrimitive)")
                .Statement("cpuStmtOne");
            statements[1].AddListener(env.ListenerNew());
            statements[2] = env.CompileDeploy(
                    "@Name('cpuStmtTwo') select * from SupportBean(IntPrimitive=2)#keepall where MyMetricFunctions.takeCPUTime(LongPrimitive)")
                .Statement("cpuStmtTwo");
            statements[2].AddListener(env.ListenerNew());
            statements[3] = env.CompileDeploy(
                    "@Name('wallStmtThree') select * from SupportBean(IntPrimitive=3)#keepall where MyMetricFunctions.takeWallTime(LongPrimitive)")
                .Statement("wallStmtThree");
            statements[3].AddListener(env.ListenerNew());
            statements[4] = env.CompileDeploy(
                    "@Name('wallStmtFour') select * from SupportBean(IntPrimitive=4)#keepall where MyMetricFunctions.takeWallTime(LongPrimitive)")
                .Statement("wallStmtFour");
            statements[4].AddListener(env.ListenerNew());

            SendEvent(env, "E1", 1, CPU_GOAL_ONE_NANO);
            SendEvent(env, "E2", 2, CPU_GOAL_TWO_NANO);
            SendEvent(env, "E3", 3, WALL_GOAL_ONE_MSEC);
            SendEvent(env, "E4", 4, WALL_GOAL_TWO_MSEC);

            SendTimer(env, 10999);
            Assert.IsFalse(env.Listener("stmt_metrics").IsInvoked);

            SendTimer(env, 11000);
            TryAssertion(env, 11000);

            SendEvent(env, "E1", 1, CPU_GOAL_ONE_NANO);
            SendEvent(env, "E2", 2, CPU_GOAL_TWO_NANO);
            SendEvent(env, "E3", 3, WALL_GOAL_ONE_MSEC);
            SendEvent(env, "E4", 4, WALL_GOAL_TWO_MSEC);

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
            var fields = "runtimeURI,statementName".SplitCsv();

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

            var cpuOne = received[0].Get("cpuTime").AsLong();
            var cpuTwo = received[1].Get("cpuTime").AsLong();
            var wallOne = received[2].Get("wallTime").AsLong();
            var wallTwo = received[3].Get("wallTime").AsLong();

            Assert.IsTrue(cpuOne > CPU_GOAL_ONE_NANO, "cpuOne=" + cpuOne);
            Assert.IsTrue(cpuTwo > CPU_GOAL_TWO_NANO, "cpuTwo=" + cpuTwo);
            Assert.IsTrue(wallOne + 50 > WALL_GOAL_ONE_MSEC, "wallOne=" + wallOne);
            Assert.IsTrue(wallTwo + 50 > WALL_GOAL_TWO_MSEC, "wallTwo=" + wallTwo);

            for (var i = 0; i < 4; i++) {
                Assert.AreEqual(1L, received[i].Get("numOutputIStream"));
                Assert.AreEqual(0L, received[i].Get("numOutputRStream"));
                Assert.AreEqual(timestamp, received[i].Get("timestamp"));
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
            long longPrimitive)
        {
            var bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }
    }
} // end of namespace