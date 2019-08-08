///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingDisableRuntime : RegressionExecution
    {
        private const long CPUGOALONENANO = 80 * 1000 * 1000;

        public void Run(RegressionEnvironment env)
        {
            var statements = new EPStatement[5];
            SendTimer(env, 1000);

            statements[0] = env.CompileDeploy("@Name('stmtmetric') select * from " + typeof(StatementMetric).FullName)
                .Statement("stmtmetric");
            statements[0].AddListener(env.ListenerNew());

            statements[1] = env.CompileDeploy("@Name('runtimemetric') select * from " + typeof(RuntimeMetric).FullName)
                .Statement("runtimemetric");
            statements[1].AddListener(env.ListenerNew());

            statements[2] = env.CompileDeploy(
                    "@Name('stmt-1') select * from SupportBean(IntPrimitive=1)#keepall where MyMetricFunctions.TakeNanos(LongPrimitive)")
                .Statement("stmt-1");
            SendEvent(env, "E1", 1, CPUGOALONENANO);

            SendTimer(env, 11000);
            Assert.IsTrue(env.Listener("stmtmetric").GetAndClearIsInvoked());
            Assert.IsTrue(env.Listener("runtimemetric").GetAndClearIsInvoked());

            env.Runtime.MetricsService.SetMetricsReportingDisabled();
            SendEvent(env, "E2", 2, CPUGOALONENANO);
            SendTimer(env, 21000);
            Assert.IsFalse(env.Listener("stmtmetric").GetAndClearIsInvoked());
            Assert.IsFalse(env.Listener("runtimemetric").GetAndClearIsInvoked());

            SendTimer(env, 31000);
            SendEvent(env, "E3", 3, CPUGOALONENANO);
            Assert.IsFalse(env.Listener("stmtmetric").GetAndClearIsInvoked());
            Assert.IsFalse(env.Listener("runtimemetric").GetAndClearIsInvoked());

            env.Runtime.MetricsService.SetMetricsReportingEnabled();
            SendEvent(env, "E4", 4, CPUGOALONENANO);
            SendTimer(env, 41000);
            Assert.IsTrue(env.Listener("stmtmetric").GetAndClearIsInvoked());
            Assert.IsTrue(env.Listener("runtimemetric").GetAndClearIsInvoked());

            env.UndeployModuleContaining(statements[2].Name);
            SendTimer(env, 51000);
            Assert.IsTrue(env.Listener("stmtmetric").IsInvoked); // metrics statements reported themselves
            Assert.IsTrue(env.Listener("runtimemetric").IsInvoked);

            env.UndeployAll();
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