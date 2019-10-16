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
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingStmtGroups : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            SendTimer(env, 0);

            env.CompileDeploy("@Name('GroupOne') select * from SupportBean(IntPrimitive = 1)#keepall");
            env.CompileDeploy("@Name('GroupTwo') select * from SupportBean(IntPrimitive = 2)#keepall");
            env.Statement("GroupTwo").Subscriber = new SupportSubscriber();
            env.CompileDeploy("@Name('Default') select * from SupportBean(IntPrimitive = 3)#keepall"); // no listener

            env.CompileDeploy("@Name('StmtMetrics') select * from " + typeof(StatementMetric).FullName)
                .AddListener("StmtMetrics");

            SendTimer(env, 6000);
            SendTimer(env, 7000);
            Assert.IsFalse(env.Listener("StmtMetrics").IsInvoked);

            SendTimer(env, 8000);
            var fields = new [] { "StatementName","NumOutputIStream","NumInput" };
            EPAssertionUtil.AssertProps(
                env.Listener("StmtMetrics").AssertOneGetNewAndReset(),
                fields,
                new object[] {"GroupOne", 0L, 0L});

            SendTimer(env, 12000);
            SendTimer(env, 14000);
            SendTimer(env, 15999);
            Assert.IsFalse(env.Listener("StmtMetrics").IsInvoked);

            SendTimer(env, 16000);
            EPAssertionUtil.AssertProps(
                env.Listener("StmtMetrics").AssertOneGetNewAndReset(),
                fields,
                new object[] {"GroupOne", 0L, 0L});

            // should report as groupTwo
            env.SendEventBean(new SupportBean("E1", 2));
            SendTimer(env, 17999);
            Assert.IsFalse(env.Listener("StmtMetrics").IsInvoked);

            SendTimer(env, 18000);
            EPAssertionUtil.AssertProps(
                env.Listener("StmtMetrics").AssertOneGetNewAndReset(),
                fields,
                new object[] {"GroupTwo", 1L, 1L});

            // should report as groupTwo
            env.SendEventBean(new SupportBean("E1", 3));
            SendTimer(env, 20999);
            Assert.IsFalse(env.Listener("StmtMetrics").IsInvoked);

            SendTimer(env, 21000);
            EPAssertionUtil.AssertProps(
                env.Listener("StmtMetrics").AssertOneGetNewAndReset(),
                fields,
                new object[] {"Default", 0L, 1L});

            // turn off group 1
            env.Runtime.MetricsService.SetMetricsReportingInterval("GroupOneStatements", -1);
            SendTimer(env, 24000);
            Assert.IsFalse(env.Listener("StmtMetrics").IsInvoked);

            // turn on group 1
            env.Runtime.MetricsService.SetMetricsReportingInterval("GroupOneStatements", 1000);
            SendTimer(env, 25000);
            EPAssertionUtil.AssertProps(
                env.Listener("StmtMetrics").AssertOneGetNewAndReset(),
                fields,
                new object[] {"GroupOne", 0L, 0L});

            env.UndeployAll();
        }

        private void SendTimer(
            RegressionEnvironment env,
            long currentTime)
        {
            env.AdvanceTime(currentTime);
        }
    }
} // end of namespace