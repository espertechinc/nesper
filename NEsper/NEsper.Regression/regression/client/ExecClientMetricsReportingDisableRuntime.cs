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
    public class ExecClientMetricsReportingDisableRuntime : RegressionExecution {
        private static readonly long CPUGOALONENANO = 80 * 1000 * 1000;
    
        public override void Configure(Configuration configuration) {
            ApplyMetricsConfig(configuration, 10000, 10000, true);
        }
    
        public override void Run(EPServiceProvider epService) {
            var statements = new EPStatement[5];
            SendTimer(epService, 1000);
    
            statements[0] = epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "stmtmetric");
            var listenerStmtMetric = new SupportUpdateListener();
            statements[0].Events += listenerStmtMetric.Update;
    
            statements[1] = epService.EPAdministrator.CreateEPL("select * from " + typeof(EngineMetric).FullName, "enginemetric");
            var listenerEngineMetric = new SupportUpdateListener();
            statements[1].Events += listenerEngineMetric.Update;
    
            statements[2] = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive=1)#keepall where MyMetricFunctions.TakeCPUTime(LongPrimitive)");
            SendEvent(epService, "E1", 1, CPUGOALONENANO);
    
            SendTimer(epService, 11000);
            Assert.IsTrue(listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsTrue(listenerEngineMetric.GetAndClearIsInvoked());
    
            epService.EPAdministrator.Configuration.SetMetricsReportingDisabled();
            SendEvent(epService, "E2", 2, CPUGOALONENANO);
            SendTimer(epService, 21000);
            Assert.IsFalse(listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsFalse(listenerEngineMetric.GetAndClearIsInvoked());
    
            SendTimer(epService, 31000);
            SendEvent(epService, "E3", 3, CPUGOALONENANO);
            Assert.IsFalse(listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsFalse(listenerEngineMetric.GetAndClearIsInvoked());
    
            epService.EPAdministrator.Configuration.SetMetricsReportingEnabled();
            SendEvent(epService, "E4", 4, CPUGOALONENANO);
            SendTimer(epService, 41000);
            Assert.IsTrue(listenerStmtMetric.GetAndClearIsInvoked());
            Assert.IsTrue(listenerEngineMetric.GetAndClearIsInvoked());
    
            statements[2].Dispose();
            SendTimer(epService, 51000);
            Assert.IsTrue(listenerStmtMetric.IsInvoked); // metrics statements reported themselves
            Assert.IsTrue(listenerEngineMetric.IsInvoked);
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
