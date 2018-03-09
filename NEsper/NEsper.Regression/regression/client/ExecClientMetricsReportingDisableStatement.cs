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

namespace com.espertech.esper.regression.client
{
    public class ExecClientMetricsReportingDisableStatement : RegressionExecution {
        private static readonly long CPUGOALONENANO = 80 * 1000 * 1000;
    
        public override void Configure(Configuration configuration) {
            ApplyMetricsConfig(configuration, -1, 10000, true);
    
            var configOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            configOne.Interval = -1;
            configOne.AddIncludeLike("%@METRIC%");
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("metrics", configOne);
        }
    
        public override void Run(EPServiceProvider epService) {
            var fields = new string[]{"statementName"};
            var statements = new EPStatement[5];
    
            SendTimer(epService, 1000);
    
            statements[0] = epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "MyStatement@METRIC");
            var listenerStmtMetric = new SupportUpdateListener();
            statements[0].Events += listenerStmtMetric.Update;
    
            statements[1] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1)#keepall where 2=2", "stmtone");
            SendEvent(epService, "E1", 1, CPUGOALONENANO);
            statements[2] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive>0)#lastevent where 1=1", "stmttwo");
            SendEvent(epService, "E2", 1, CPUGOALONENANO);
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtMetric.GetNewDataListFlattened(), fields, new object[][]{new object[] {"stmtone"}, new object[] {"stmttwo"}});
            listenerStmtMetric.Reset();
    
            SendEvent(epService, "E1", 1, CPUGOALONENANO);
            SendTimer(epService, 21000);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtMetric.GetNewDataListFlattened(), fields, new object[][]{new object[] {"stmtone"}, new object[] {"stmttwo"}});
            listenerStmtMetric.Reset();
    
            epService.EPAdministrator.Configuration.SetMetricsReportingStmtDisabled("stmtone");
    
            SendEvent(epService, "E1", 1, CPUGOALONENANO);
            SendTimer(epService, 31000);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtMetric.GetNewDataListFlattened(), fields, new object[][]{new object[] {"stmttwo"}});
            listenerStmtMetric.Reset();

            epService.EPAdministrator.Configuration.SetMetricsReportingStmtEnabled("stmtone");
            epService.EPAdministrator.Configuration.SetMetricsReportingStmtDisabled("stmttwo");
    
            SendEvent(epService, "E1", 1, CPUGOALONENANO);
            SendTimer(epService, 41000);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtMetric.GetNewDataListFlattened(), fields, new object[][]{new object[] {"stmtone"}});
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
