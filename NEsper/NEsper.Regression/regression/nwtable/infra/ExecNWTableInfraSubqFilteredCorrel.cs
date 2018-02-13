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

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubqFilteredCorrel : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
    
            // named window tests
            RunAssertion(epService, true, false, false, false);  // no-share
            RunAssertion(epService, true, false, false, true);   // no-share create
            RunAssertion(epService, true, true, false, false);   // share no-create
            RunAssertion(epService, true, true, true, false);    // disable share no-create
            RunAssertion(epService, true, true, true, true);     // disable share create
    
            // table tests
            RunAssertion(epService, false, false, false, false);  // table no-create
            RunAssertion(epService, false, false, false, true);  // table create
        }
    
        private void RunAssertion(EPServiceProvider epService, bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex) {
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
    
            string createEpl = namedWindow ?
                    "create window MyInfra#keepall as select * from SupportBean" :
                    "create table MyInfra (theString string primary key, intPrimitive int primary key)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            epService.EPAdministrator.CreateEPL(createEpl);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select theString, intPrimitive from SupportBean");
    
            EPStatement indexStmt = null;
            if (createExplicitIndex) {
                indexStmt = epService.EPAdministrator.CreateEPL("create index MyIndex on MyInfra(theString)");
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -2));
    
            string consumeEpl = "select (select intPrimitive from MyInfra(intPrimitive<0) sw where s0.p00=sw.theString) as val from S0 s0";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            EPStatement consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "E2"));
            Assert.AreEqual(-2, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-3, "E3"));
            Assert.AreEqual(-3, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "E4"));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val"));
    
            consumeStmt.Stop();
            if (indexStmt != null) {
                indexStmt.Stop();
            }
            consumeStmt.Destroy();
            if (indexStmt != null) {
                indexStmt.Destroy();
            }
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
} // end of namespace
