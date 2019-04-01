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
    public class ExecClientMetricsReportingStmtGroups : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            ApplyMetricsConfig(configuration, -1, 7000, true);
    
            var groupOne = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupOne.Interval = 8000;
            groupOne.AddIncludeLike("%GroupOne%");
            groupOne.IsReportInactive = true;
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("GroupOneStatements", groupOne);
    
            var groupTwo = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupTwo.Interval = 6000;
            groupTwo.IsDefaultInclude = true;
            groupTwo.AddExcludeLike("%Default%");
            groupTwo.AddExcludeLike("%Metrics%");
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("GroupTwoNonDefaultStatements", groupTwo);
    
            var groupThree = new ConfigurationMetricsReporting.StmtGroupMetrics();
            groupThree.Interval = -1;
            groupThree.AddIncludeLike("%Metrics%");
            configuration.EngineDefaults.MetricsReporting.AddStmtGroup("MetricsStatements", groupThree);
        }
    
        public override void Run(EPServiceProvider epService)
        {
            SendTimer(epService, 0);
    
            epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 1)#keepall", "GroupOne");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 2)#keepall", "GroupTwo");
            stmt.Subscriber = new SupportSubscriber();
            epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 3)#keepall", "Default");   // no listener
    
            stmt = epService.EPAdministrator.CreateEPL("select * from " + typeof(StatementMetric).FullName, "StmtMetrics");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 6000);
            SendTimer(epService, 7000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 8000);
            string[] fields = "StatementName,OutputIStreamCount,NumInput".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"GroupOne", 0L, 0L});
    
            SendTimer(epService, 12000);
            SendTimer(epService, 14000);
            SendTimer(epService, 15999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 16000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"GroupOne", 0L, 0L});
    
            // should report as groupTwo
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            SendTimer(epService, 17999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 18000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"GroupTwo", 1L, 1L});
    
            // should report as groupTwo
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            SendTimer(epService, 20999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 21000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Default", 0L, 1L});
    
            // turn off group 1
            epService.EPAdministrator.Configuration.SetMetricsReportingInterval("GroupOneStatements", -1);
            SendTimer(epService, 24000);
            Assert.IsFalse(listener.IsInvoked);
    
            // turn on group 1
            epService.EPAdministrator.Configuration.SetMetricsReportingInterval("GroupOneStatements", 1000);
            SendTimer(epService, 25000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"GroupOne", 0L, 0L});
        }
    
        private void SendTimer(EPServiceProvider epService, long currentTime)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
        }
    }
} // end of namespace
