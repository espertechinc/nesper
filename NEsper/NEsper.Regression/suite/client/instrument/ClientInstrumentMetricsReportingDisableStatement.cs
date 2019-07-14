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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingDisableStatement : RegressionExecution
    {
        private const long CPUGOALONENANO = 80 * 1000 * 1000;

        public void Run(RegressionEnvironment env)
        {
            string[] fields = {"statementName"};
            var statements = new EPStatement[5];

            SendTimer(env, 1000);

            statements[0] =
                env.CompileDeploy("@Name('MyStatement@METRIC') select * from " + typeof(StatementMetric).Name)
                    .Statement("MyStatement@METRIC");
            statements[0].AddListener(env.ListenerNew());

            statements[1] =
                env.CompileDeploy("@Name('stmtone') select * from SupportBean(intPrimitive=1)#keepall where 2=2")
                    .Statement("stmtone");
            SendEvent(env, "E1", 1, CPUGOALONENANO);
            statements[2] =
                env.CompileDeploy("@Name('stmttwo') select * from SupportBean(intPrimitive>0)#lastevent where 1=1")
                    .Statement("stmttwo");
            SendEvent(env, "E2", 1, CPUGOALONENANO);

            SendTimer(env, 11000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("MyStatement@METRIC").NewDataListFlattened,
                fields,
                new[] {new object[] {"stmtone"}, new object[] {"stmttwo"}});
            env.Listener("MyStatement@METRIC").Reset();

            SendEvent(env, "E1", 1, CPUGOALONENANO);
            SendTimer(env, 21000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("MyStatement@METRIC").NewDataListFlattened,
                fields,
                new[] {new object[] {"stmtone"}, new object[] {"stmttwo"}});
            env.Listener("MyStatement@METRIC").Reset();

            env.Runtime.MetricsService.SetMetricsReportingStmtDisabled(env.DeploymentId("stmtone"), "stmtone");

            SendEvent(env, "E1", 1, CPUGOALONENANO);
            SendTimer(env, 31000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("MyStatement@METRIC").NewDataListFlattened,
                fields,
                new[] {new object[] {"stmttwo"}});
            env.Listener("MyStatement@METRIC").Reset();

            env.Runtime.MetricsService.SetMetricsReportingStmtEnabled(env.DeploymentId("stmtone"), "stmtone");
            env.Runtime.MetricsService.SetMetricsReportingStmtDisabled(env.DeploymentId("stmttwo"), "stmttwo");

            SendEvent(env, "E1", 1, CPUGOALONENANO);
            SendTimer(env, 41000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("MyStatement@METRIC").NewDataListFlattened,
                fields,
                new[] {new object[] {"stmtone"}});

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