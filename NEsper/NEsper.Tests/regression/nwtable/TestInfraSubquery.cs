///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraSubquery 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listenerWindow;
        private SupportUpdateListener listenerStmtOne;
        private SupportUpdateListener listenerStmtTwo;
        private SupportUpdateListener listenerStmtDelete;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
            listenerWindow = new SupportUpdateListener();
            listenerStmtOne = new SupportUpdateListener();
            listenerStmtTwo = new SupportUpdateListener();
            listenerStmtDelete = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listenerWindow = null;
            listenerStmtOne = null;
            listenerStmtTwo = null;
            listenerStmtDelete = null;
        }
    
        [Test]
        public void TestSubquerySelfCheck() {
            RunAssertionSubquerySelfCheck(true);
            RunAssertionSubquerySelfCheck(false);
        }
    
        [Test]
        public void TestSubqueryDeleteInsertReplace() {
            RunAssertionSubqueryDeleteInsertReplace(true);
            RunAssertionSubqueryDeleteInsertReplace(false);
        }
    
        [Test]
        public void TestInvalidSubquery() {
            RunAssertionInvalidSubquery(true);
            RunAssertionInvalidSubquery(false);
        }
    
        [Test]
        public void TestAssertionUncorrelatedSubqueryAggregation() {
            RunAssertionUncorrelatedSubqueryAggregation(true);
            RunAssertionUncorrelatedSubqueryAggregation(false);
        }
    
        private void RunAssertionUncorrelatedSubqueryAggregation(bool namedWindow)
        {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b long)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select irstream (select sum(b) from MyInfra) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(listenerStmtOne);
    
            SendMarketBean("M1");
            string[] fieldsStmt = new string[] {"value", "symbol"};
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M1"});
    
            SendSupportBean("S1", 5L, -1L);
            SendMarketBean("M2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{5L, "M2"});
    
            SendSupportBean("S2", 10L, -1L);
            SendMarketBean("M3");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{15L, "M3"});
    
            // create 2nd consumer
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectOne); // same stmt
            stmtSelectTwo.AddListener(listenerStmtTwo);
    
            SendSupportBean("S3", 8L, -1L);
            SendMarketBean("M4");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{23L, "M4"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{23L, "M4"});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        public void RunAssertionInvalidSubquery(bool namedWindow)
        {
            string eplCreate = namedWindow ?
                "create window MyInfra.win:keepall() as " + typeof(SupportBean).FullName :
                "create table MyInfra(TheString string)";
            epService.EPAdministrator.CreateEPL(eplCreate);
    
            try
            {
                epService.EPAdministrator.CreateEPL("select (select TheString from MyInfra.std:lastevent()) from MyInfra");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                if (namedWindow) {
                    Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying MyInfra: Consuming statements to a named window cannot declare a data window view onto the named window [select (select TheString from MyInfra.std:lastevent()) from MyInfra]", ex.Message);
                }
                else {
                    SupportMessageAssertUtil.AssertMessage(ex, "Views are not supported with tables");
                }
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSubqueryDeleteInsertReplace(bool namedWindow)
        {
            string[] fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName :
                    "create table MyInfra(key string primary key, value int primary key)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            // delete
            string stmtTextDelete = "on " + typeof(SupportBean).FullName + " delete from MyInfra where key = TheString";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(listenerStmtDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName + " as s0";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[]{"E1", 1}});
    
            SendSupportBean("E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            }
            else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            }
    
            SendSupportBean("E1", 3);
            if (namedWindow) {
                Assert.AreEqual(2, listenerWindow.NewDataList.Count);
                EPAssertionUtil.AssertProps(listenerWindow.OldDataList[0][0], fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerWindow.NewDataList[1][0], fields, new object[]{"E1", 3});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 }, new object[] { "E1", 3 } });
    
            listenerWindow.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSubquerySelfCheck(bool namedWindow)
        {
            string[] fields = new string[] {"key", "value"};
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName :
                    "create table MyInfra (key string primary key, value int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            // create insert into (not does insert if key already exists)
            string stmtTextInsertOne = "insert into MyInfra select TheString as key, IntBoxed as value from " + typeof(SupportBean).FullName + " as s0" +
                                        " where not exists (select * from MyInfra as win where win.key = s0.TheString)";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            else {
                Assert.IsFalse(listenerWindow.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            SendSupportBean("E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
    
            SendSupportBean("E1", 3);
            Assert.IsFalse(listenerWindow.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
    
            SendSupportBean("E3", 4);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 4});
                EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 4 } });
            }
            else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 4 } });
            }
    
            // Add delete
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyInfra where key = id";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(listenerStmtDelete);
    
            // delete E2
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 4 } });
    
            SendSupportBean("E2", 5);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 5});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 4 }, new object[] { "E2", 5 } });
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean SendSupportBean(string theString, long longPrimitive, long? longBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendMarketBean(string symbol)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, 0l, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
}
