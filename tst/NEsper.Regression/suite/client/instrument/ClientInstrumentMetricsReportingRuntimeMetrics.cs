///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentMetricsReportingRuntimeMetrics : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = Collections.Array("RuntimeURI", "Timestamp", "InputCount", "InputCountDelta", "ScheduleDepth");
            SendTimer(env, 1000);

            var text = "@name('s0') select * from " + typeof(RuntimeMetric).FullName;
            env.CompileDeploy(text).AddListener("s0");

            env.SendEventBean(new SupportBean());

            SendTimer(env, 10999);
            ClassicAssert.IsFalse(env.Listener("s0").IsInvoked);

            env.CompileDeploy("select * from pattern[timer:interval(5 sec)]");

            SendTimer(env, 11000);
            env.AssertPropsNew("s0", fields, new object[] { env.RuntimeURI, 11000L, 1L, 1L, 1L });

            env.SendEventBean(new SupportBean());
            env.SendEventBean(new SupportBean());

            SendTimer(env, 20000);
            SendTimer(env, 21000);
            env.AssertPropsNew("s0", fields, new object[] { env.RuntimeURI, 21000L, 4L, 3L, 0L });

            var cpuGoal = 10.0d; // milliseconds of execution time

#if !NETCORE
            var before = PerformanceMetricsHelper.GetCurrentMetricResult();
            MyMetricFunctions.TakeMillis(cpuGoal);
            var after = PerformanceMetricsHelper.GetCurrentMetricResult();
            ClassicAssert.IsTrue((after.UserTime - before.UserTime).TotalMilliseconds > cpuGoal);
#endif

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