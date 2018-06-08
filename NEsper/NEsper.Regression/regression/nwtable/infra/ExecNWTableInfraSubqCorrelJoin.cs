///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubqCorrelJoin : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0Bean", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1Bean", typeof(SupportBean_S1));
    
            // named window
            RunAssertion(epService, true, false); // disable index-share
            RunAssertion(epService, true, true); // enable-index-share
    
            // table
            RunAssertion(epService, false, false);
        }
    
        private void RunAssertion(EPServiceProvider epService, bool namedWindow, bool enableIndexShareCreate) {
            string createEpl = namedWindow ?
                    "create window MyInfra#unique(TheString) as select * from SupportBean" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            epService.EPAdministrator.CreateEPL(createEpl);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
    
            string consumeEpl = "select (select IntPrimitive from MyInfra where TheString = s1.p10) as val from S0Bean#lastevent as s0, S1Bean#lastevent as s1";
            EPStatement consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            var listener = new SupportUpdateListener();
            consumeStmt.Events += listener.Update;
    
            string[] fields = "val".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{30});
    
            consumeStmt.Stop();
            consumeStmt.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
} // end of namespace
