///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingStmtGroups : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            SendTimer(env, 0);

            env.CompileDeploy("@name('GroupOne') select * from SupportBean(intPrimitive = 1)#keepall");
            env.CompileDeploy("@name('GroupTwo') select * from SupportBean(intPrimitive = 2)#keepall")
                .SetSubscriber("GroupTwo");
            env.CompileDeploy("@name('Default') select * from SupportBean(intPrimitive = 3)#keepall"); // no listener

            env.CompileDeploy("@name('StmtMetrics') select * from " + typeof(StatementMetric).FullName)
                .AddListener("StmtMetrics");

            SendTimer(env, 6000);
            SendTimer(env, 7000);
            env.AssertListenerNotInvoked("StmtMetrics");

            SendTimer(env, 8000);
            var fields = "statementName,numOutputIStream,numInput".SplitCsv();
            env.AssertPropsNew("StmtMetrics", fields, new object[] { "GroupOne", 0L, 0L });

            SendTimer(env, 12000);
            SendTimer(env, 14000);
            SendTimer(env, 15999);
            env.AssertListenerNotInvoked("StmtMetrics");

            SendTimer(env, 16000);
            env.AssertPropsNew("StmtMetrics", fields, new object[] { "GroupOne", 0L, 0L });

            // should report as groupTwo
            env.SendEventBean(new SupportBean("E1", 2));
            SendTimer(env, 17999);
            env.AssertListenerNotInvoked("StmtMetrics");

            SendTimer(env, 18000);
            env.AssertPropsNew("StmtMetrics", fields, new object[] { "GroupTwo", 1L, 1L });

            // should report as groupTwo
            env.SendEventBean(new SupportBean("E1", 3));
            SendTimer(env, 20999);
            env.AssertListenerNotInvoked("StmtMetrics");

            SendTimer(env, 21000);
            env.AssertPropsNew("StmtMetrics", fields, new object[] { "Default", 0L, 1L });

            // turn off group 1
            env.Runtime.MetricsService.SetMetricsReportingInterval("GroupOneStatements", -1);
            SendTimer(env, 24000);
            env.AssertListenerNotInvoked("StmtMetrics");

            // turn on group 1
            env.Runtime.MetricsService.SetMetricsReportingInterval("GroupOneStatements", 1000);
            SendTimer(env, 25000);
            env.AssertPropsNew("StmtMetrics", fields, new object[] { "GroupOne", 0L, 0L });

            env.UndeployAll();
        }

        private void SendTimer(
            RegressionEnvironment env,
            long currentTime)
        {
            env.AdvanceTime(currentTime);
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.RUNTIMEOPS);
        }
    }
} // end of namespace