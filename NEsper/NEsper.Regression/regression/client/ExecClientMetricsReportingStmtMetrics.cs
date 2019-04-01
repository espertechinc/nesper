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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.client.ExecClientMetricsReportingNW;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientMetricsReportingStmtMetrics : RegressionExecution {
        private static readonly long CPU_GOAL_ONE_NANO = 80 * 1000 * 1000;
        private static readonly long CPU_GOAL_TWO_NANO = 50 * 1000 * 1000;
        private static readonly long WALL_GOAL_ONE_MSEC = 200;
        private static readonly long WALL_GOAL_TWO_MSEC = 400;
    
        public override void Configure(Configuration configuration) {
            ApplyMetricsConfig(configuration, -1, -1, true);
    
            var configOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configOne.Interval = 10000;
            configOne.AddIncludeLike("%cpuStmt%");
            configOne.AddIncludeLike("%wallStmt%");
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("nonmetrics", configOne);
    
            // exclude metrics themselves from reporting
            var configTwo = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configTwo.Interval = -1;
            configOne.AddExcludeLike("%metrics%");
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("metrics", configTwo);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            SendTimer(epService, 1000);
    
            var statements = new EPStatement[5];
            var listener = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();

            statements[0] = epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "stmt_metrics");
            statements[0].Events += listener.Update;

            statements[1] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1)#keepall where MyMetricFunctions.TakeCPUTime(LongPrimitive)", "cpuStmtOne");
            statements[1].Events += listenerTwo.Update;
            statements[2] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=2)#keepall where MyMetricFunctions.TakeCPUTime(LongPrimitive)", "cpuStmtTwo");
            statements[2].Events += listenerTwo.Update;
            statements[3] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=3)#keepall where MyMetricFunctions.TakeWallTime(LongPrimitive)", "wallStmtThree");
            statements[3].Events += listenerTwo.Update;
            statements[4] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=4)#keepall where MyMetricFunctions.TakeWallTime(LongPrimitive)", "wallStmtFour");
            statements[4].Events += listenerTwo.Update;
    
            SendEvent(epService, "E1", 1, CPU_GOAL_ONE_NANO);
            SendEvent(epService, "E2", 2, CPU_GOAL_TWO_NANO);
            SendEvent(epService, "E3", 3, WALL_GOAL_ONE_MSEC);
            SendEvent(epService, "E4", 4, WALL_GOAL_TWO_MSEC);
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 11000);
            TryAssertion(epService, listener, 11000);
    
            SendEvent(epService, "E1", 1, CPU_GOAL_ONE_NANO);
            SendEvent(epService, "E2", 2, CPU_GOAL_TWO_NANO);
            SendEvent(epService, "E3", 3, WALL_GOAL_ONE_MSEC);
            SendEvent(epService, "E4", 4, WALL_GOAL_TWO_MSEC);
    
            SendTimer(epService, 21000);
            TryAssertion(epService, listener, 21000);
    
            // destroy all application stmts
            for (int i = 1; i < 5; i++) {
                statements[i].Dispose();
            }
            SendTimer(epService, 31000);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener, long timestamp) {
            string[] fields = "engineURI,statementName".Split(',');
    
            Assert.AreEqual(4, listener.NewDataList.Count);
            EventBean[] received = listener.GetNewDataListFlattened();
    
            EPAssertionUtil.AssertProps(received[0], fields, new object[]{"default", "cpuStmtOne"});
            EPAssertionUtil.AssertProps(received[1], fields, new object[]{"default", "cpuStmtTwo"});
            EPAssertionUtil.AssertProps(received[2], fields, new object[]{"default", "wallStmtThree"});
            EPAssertionUtil.AssertProps(received[3], fields, new object[]{"default", "wallStmtFour"});

            long cpuOne = received[0].Get("cpuTime").AsLong();
            long cpuTwo = received[1].Get("cpuTime").AsLong();
            long wallOne = received[2].Get("wallTime").AsLong();
            long wallTwo = received[3].Get("wallTime").AsLong();

            Assert.IsTrue(cpuOne > CPU_GOAL_ONE_NANO, "cpuOne=" + cpuOne);
            Assert.IsTrue(cpuTwo > CPU_GOAL_TWO_NANO, "cpuTwo=" + cpuTwo);
            Assert.IsTrue((wallOne + 50) > WALL_GOAL_ONE_MSEC, "wallOne=" + wallOne);
            Assert.IsTrue((wallTwo + 50) > WALL_GOAL_TWO_MSEC, "wallTwo=" + wallTwo);
    
            for (int i = 0; i < 4; i++) {
                Assert.AreEqual(1L, received[i].Get("OutputIStreamCount"));
                Assert.AreEqual(0L, received[i].Get("OutputRStreamCount"));
                Assert.AreEqual(timestamp, received[i].Get("timestamp"));
            }
    
            listener.Reset();
        }
    
        private void SendTimer(EPServiceProvider epService, long currentTime) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
        }
    
        private void SendEvent(EPServiceProvider epService, string id, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
