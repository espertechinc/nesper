///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabase3StreamOuterJoin : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;
    
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.EngineDefaults.Logging.IsEnableADO = true;
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBeanTwo>();
    
            RunAssertionInnerJoinLeftS0(epService);
            RunAssertionOuterJoinLeftS0(epService);
        }
    
        private void RunAssertionInnerJoinLeftS0(EPServiceProvider epService) {
            string stmtText = "select * from SupportBean#lastevent sb" +
                    " inner join " +
                    " SupportBeanTwo#lastevent sbt" +
                    " on sb.TheString = sbt.stringTwo " +
                    " inner join " +
                    " sql:MyDB ['select myint from mytesttable'] as s1 " +
                    "  on s1.myint = sbt.IntPrimitiveTwo";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("T1", -1));
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            epService.EPRuntime.SendEvent(new SupportBean("T2", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T2", "T2", 30});
    
            epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T3", "T3", 40});
    
            statement.Dispose();
        }
    
        private void RunAssertionOuterJoinLeftS0(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBeanTwo>();
            string stmtText = "select * from SupportBean#lastevent sb" +
                    " left outer join " +
                    " SupportBeanTwo#lastevent sbt" +
                    " on sb.TheString = sbt.stringTwo " +
                    " left outer join " +
                    " sql:MyDB ['select myint from mytesttable'] as s1 " +
                    "  on s1.myint = sbt.IntPrimitiveTwo";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("T1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T1", "T1", null});
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            epService.EPRuntime.SendEvent(new SupportBean("T2", -2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T2", "T2", 30});
    
            epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T3", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T3", "T3", 40});
    
            statement.Dispose();
        }
    }
} // end of namespace
