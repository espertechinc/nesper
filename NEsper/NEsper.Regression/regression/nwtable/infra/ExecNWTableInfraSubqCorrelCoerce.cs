///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubqCorrelCoerce : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0Bean", typeof(SupportBean_S0));
    
            // named window tests
            RunAssertion(epService, true, false, false, false); // no share
            RunAssertion(epService, true, false, false, true); // no share create index
            RunAssertion(epService, true, true, false, false); // share
            RunAssertion(epService, true, true, false, true); // share create index
            RunAssertion(epService, true, true, true, false); // disable share
            RunAssertion(epService, true, true, true, true); // disable share create index
    
            // table tests
            RunAssertion(epService, false, false, false, false); // table
            RunAssertion(epService, false, false, false, true); // table + create index
        }
    
        private void RunAssertion(EPServiceProvider epService, bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex) {
            epService.EPAdministrator.CreateEPL("create schema EventSchema(e0 string, e1 int, e2 string)");
            epService.EPAdministrator.CreateEPL("create schema WindowSchema(col0 string, col1 long, col2 string)");
    
            string createEpl = namedWindow ?
                    "create window MyInfra#keepall as WindowSchema" :
                    "create table MyInfra (col0 string primary key, col1 long, col2 string)";
            if (enableIndexShareCreate) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            epService.EPAdministrator.CreateEPL(createEpl);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select * from WindowSchema");
    
            EPStatement stmtIndex = null;
            if (createExplicitIndex) {
                stmtIndex = epService.EPAdministrator.CreateEPL("create index MyIndex on MyInfra (col2, col1)");
            }
    
            string[] fields = "e0,val".Split(',');
            string consumeEpl = "select e0, (select col0 from MyInfra where col2 = es.e2 and col1 = es.e1) as val from EventSchema es";
            if (disableIndexShareConsumer) {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            EPStatement consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            var listener = new SupportUpdateListener();
            consumeStmt.Events += listener.Update;
    
            SendWindow(epService, "W1", 10L, "c31");
            SendEvent(epService, "E1", 10, "c31");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "W1"});
    
            SendEvent(epService, "E2", 11, "c32");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", null});
    
            SendWindow(epService, "W2", 11L, "c32");
            SendEvent(epService, "E3", 11, "c32");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", "W2"});
    
            SendWindow(epService, "W3", 11L, "c31");
            SendWindow(epService, "W4", 10L, "c32");
    
            SendEvent(epService, "E4", 11, "c31");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "W3"});
    
            SendEvent(epService, "E5", 10, "c31");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5", "W1"});
    
            SendEvent(epService, "E6", 10, "c32");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6", "W4"});
    
            // test late start
            consumeStmt.Dispose();
            consumeStmt = epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.Events += listener.Update;
    
            SendEvent(epService, "E6", 10, "c32");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6", "W4"});
    
            if (stmtIndex != null) {
                stmtIndex.Dispose();
            }
            consumeStmt.Dispose();
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void SendWindow(EPServiceProvider epService, string col0, long col1, string col2) {
            var theEvent = new LinkedHashMap<string, Object>();
            theEvent.Put("col0", col0);
            theEvent.Put("col1", col1);
            theEvent.Put("col2", col2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "WindowSchema");
            } else {
                epService.EPRuntime.SendEvent(theEvent, "WindowSchema");
            }
        }
    
        private void SendEvent(EPServiceProvider epService, string e0, int e1, string e2) {
            var theEvent = new LinkedHashMap<string, Object>();
            theEvent.Put("e0", e0);
            theEvent.Put("e1", e1);
            theEvent.Put("e2", e2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "EventSchema");
            } else {
                epService.EPRuntime.SendEvent(theEvent, "EventSchema");
            }
        }
    }
} // end of namespace
