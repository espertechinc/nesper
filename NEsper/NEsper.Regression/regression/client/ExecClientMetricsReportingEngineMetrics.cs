///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.metric;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.client.ExecClientMetricsReportingNW;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientMetricsReportingEngineMetrics : RegressionExecution {
        public override void Configure(Configuration configuration) {
            ApplyMetricsConfig(configuration, 10000, -1, true);
        }
    
        public override void Run(EPServiceProvider epService) {
            string[] engineFields = "engineURI,timestamp,inputCount,inputCountDelta,scheduleDepth".Split(',');
            SendTimer(epService, 1000);
    
            string text = "select * from " + typeof(EngineMetric).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(5 sec)]");
    
            SendTimer(epService, 11000);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, engineFields, new object[]{"default", 11000L, 1L, 1L, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            epService.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(epService, 20000);
            SendTimer(epService, 21000);
            theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, engineFields, new object[]{"default", 21000L, 4L, 3L, 0L});

#if false
            // Try MBean
            ThreadMXBean mbean = ManagementFactory.ThreadMXBean;
            if (!mbean.IsThreadCpuTimeEnabled) {
                Assert.Fail("ThreadMXBean CPU time reporting is not enabled");
            }
    
            long msecMultiplier = 1000 * 1000;
            long msecGoal = 10;
            long cpuGoal = msecGoal * msecMultiplier;
    
            long beforeCPU = mbean.CurrentThreadCpuTime;
            MyMetricFunctions.TakeCPUTime(cpuGoal);
            long afterCPU = mbean.CurrentThreadCpuTime;
            Assert.IsTrue((afterCPU - beforeCPU) > cpuGoal);
#endif
        }
    
        private void SendTimer(EPServiceProvider epService, long currentTime) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
        }
    }
} // end of namespace
