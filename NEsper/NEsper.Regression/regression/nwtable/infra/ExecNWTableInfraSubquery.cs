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
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraSubquery : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
    
            RunAssertionSubquerySelfCheck(epService, true);
            RunAssertionSubquerySelfCheck(epService, false);
    
            RunAssertionSubqueryDeleteInsertReplace(epService, true);
            RunAssertionSubqueryDeleteInsertReplace(epService, false);
    
            RunAssertionInvalidSubquery(epService, true);
            RunAssertionInvalidSubquery(epService, false);
    
            RunAssertionUncorrelatedSubqueryAggregation(epService, true);
            RunAssertionUncorrelatedSubqueryAggregation(epService, false);
        }
    
        private void RunAssertionUncorrelatedSubqueryAggregation(EPServiceProvider epService, bool namedWindow) {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfraUCS#keepall as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfraUCS(a string primary key, b long)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfraUCS select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select irstream (select sum(b) from MyInfraUCS) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendMarketBean(epService, "M1");
            var fieldsStmt = new string[]{"value", "symbol"};
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M1"});
    
            SendSupportBean(epService, "S1", 5L, -1L);
            SendMarketBean(epService, "M2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{5L, "M2"});
    
            SendSupportBean(epService, "S2", 10L, -1L);
            SendMarketBean(epService, "M3");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{15L, "M3"});
    
            // create 2nd consumer
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectOne); // same stmt
            var listenerStmtTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerStmtTwo.Update;
    
            SendSupportBean(epService, "S3", 8L, -1L);
            SendMarketBean(epService, "M4");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{23L, "M4"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{23L, "M4"});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraUCS", false);
        }
    
        private void RunAssertionInvalidSubquery(EPServiceProvider epService, bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfraIS#keepall as " + typeof(SupportBean).FullName :
                    "create table MyInfraIS(TheString string)";
            epService.EPAdministrator.CreateEPL(eplCreate);
    
            try {
                epService.EPAdministrator.CreateEPL("select (select TheString from MyInfraIS#lastevent) from MyInfraIS");
                Assert.Fail();
            } catch (EPException ex) {
                if (namedWindow) {
                    Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying MyInfraIS: Consuming statements to a named window cannot declare a data window view onto the named window [select (select TheString from MyInfraIS#lastevent) from MyInfraIS]", ex.Message);
                } else {
                    SupportMessageAssertUtil.AssertMessage(ex, "Views are not supported with tables");
                }
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIS", false);
        }
    
        private void RunAssertionSubqueryDeleteInsertReplace(EPServiceProvider epService, bool namedWindow) {
            var fields = new string[]{"key", "value"};
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName :
                    "create table MyInfra(key string primary key, value int primary key)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // delete
            string stmtTextDelete = "on " + typeof(SupportBean).FullName + " delete from MyInfra where key = TheString";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName + " as s0";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            SendSupportBean(epService, "E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            } else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            }
    
            SendSupportBean(epService, "E1", 3);
            if (namedWindow) {
                Assert.AreEqual(2, listenerWindow.NewDataList.Count);
                EPAssertionUtil.AssertProps(listenerWindow.OldDataList[0][0], fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerWindow.NewDataList[1][0], fields, new object[]{"E1", 3});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}, new object[] {"E1", 3}});
    
            listenerWindow.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSubquerySelfCheck(EPServiceProvider epService, bool namedWindow) {
            var fields = new string[]{"key", "value"};
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfraSSS#keepall as select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName :
                    "create table MyInfraSSS (key string primary key, value int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into (not does insert if key already exists)
            string stmtTextInsertOne = "insert into MyInfraSSS select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName + " as s0" +
                    " where not exists (select * from MyInfraSSS as win where win.key = s0.TheString)";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            } else {
                Assert.IsFalse(listenerWindow.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            SendSupportBean(epService, "E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            SendSupportBean(epService, "E1", 3);
            Assert.IsFalse(listenerWindow.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            SendSupportBean(epService, "E3", 4);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 4});
                EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
            } else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
            }
    
            // Add delete
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyInfraSSS where key = id";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            // delete E2
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 4}});
    
            SendSupportBean(epService, "E2", 5);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 5});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 4}, new object[] {"E2", 5}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSSS", false);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, long longPrimitive, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
