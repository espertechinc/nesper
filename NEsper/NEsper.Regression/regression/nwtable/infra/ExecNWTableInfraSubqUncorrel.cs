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
    public class ExecNWTableInfraSubqUncorrel : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
    
            // named window tests
            RunAssertion(epService, true, false, false); // testNoShare
            RunAssertion(epService, true, true, false); // testShare
            RunAssertion(epService, true, true, true); // testDisableShare
    
            // table tests
            RunAssertion(epService, false, false, false);
        }
    
        private void RunAssertion(EPServiceProvider epService, bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer) {
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b long, c long)";
            if (enableIndexShareCreate) {
                stmtTextCreate = "@Hint('enable_window_subquery_indexshare') " + stmtTextCreate;
            }
            // create window
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select irstream (select a from MyInfra) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            if (disableIndexShareConsumer) {
                stmtTextSelectOne = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectOne;
            }
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"value", "symbol"});
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("value"));
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("symbol"));
    
            SendMarketBean(epService, "M1");
            var fieldsStmt = new string[]{"value", "symbol"};
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M1"});
    
            SendSupportBean(epService, "S1", 1L, 2L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            var fieldsWin = new string[]{"a", "b", "c"};
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S1", 1L, 2L});
            } else {
                Assert.IsFalse(listenerWindow.IsInvoked);
            }
    
            // create consumer 2 -- note that this one should not start empty now
            string stmtTextSelectTwo = "select irstream (select a from MyInfra) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            if (disableIndexShareConsumer) {
                stmtTextSelectTwo = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectTwo;
            }
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            var listenerStmtTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerStmtTwo.Update;
    
            SendMarketBean(epService, "M1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S1", "M1"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S1", "M1"});
    
            SendSupportBean(epService, "S2", 10L, 20L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S2", 10L, 20L});
            }
    
            SendMarketBean(epService, "M2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M2"});
            Assert.IsFalse(listenerWindow.IsInvoked);
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M2"});
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyInfra where id = a";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            // delete S1
            epService.EPRuntime.SendEvent(new SupportBean_A("S1"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fieldsWin, new object[]{"S1", 1L, 2L});
            }
    
            SendMarketBean(epService, "M3");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S2", "M3"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S2", "M3"});
    
            // delete S2
            epService.EPRuntime.SendEvent(new SupportBean_A("S2"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fieldsWin, new object[]{"S2", 10L, 20L});
            }
    
            SendMarketBean(epService, "M4");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M4"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M4"});
    
            SendSupportBean(epService, "S3", 100L, 200L);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S3", 100L, 200L});
            }
    
            SendMarketBean(epService, "M5");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S3", "M5"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S3", "M5"});
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, long longPrimitive, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
