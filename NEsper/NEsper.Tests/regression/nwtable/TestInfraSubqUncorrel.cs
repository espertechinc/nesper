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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraSubqUncorrel 
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
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
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
        public void TestUncorrelated() {
            // named window tests
            RunAssertion(true, false, false); // testNoShare
            RunAssertion(true, true, false); // testShare
            RunAssertion(true, true, true); // testDisableShare
            
            // table tests
            RunAssertion(false, false, false);
        }
    
        private void RunAssertion(bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer)
        {
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b long, c long)";
            if (enableIndexShareCreate) {
                stmtTextCreate = "@Hint('enable_window_subquery_indexshare') " + stmtTextCreate;
            }
            // create window
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, LongPrimitive as b, LongBoxed as c from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select irstream (select a from MyInfra) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            if (disableIndexShareConsumer) {
                stmtTextSelectOne = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectOne;
            }
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(listenerStmtOne);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtSelectOne.EventType.PropertyNames, new string[]{"value", "symbol"});
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("value"));
            Assert.AreEqual(typeof(string), stmtSelectOne.EventType.GetPropertyType("symbol"));
    
            SendMarketBean("M1");
            var fieldsStmt = new string[] {"value", "symbol"};
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M1"});
    
            SendSupportBean("S1", 1L, 2L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            var fieldsWin = new string[] {"a", "b", "c"};
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S1", 1L, 2L});
            }
            else {
                Assert.IsFalse(listenerWindow.IsInvoked);
            }
    
            // create consumer 2 -- note that this one should not start empty now
            string stmtTextSelectTwo = "select irstream (select a from MyInfra) as value, symbol from " + typeof(SupportMarketDataBean).FullName;
            if (disableIndexShareConsumer) {
                stmtTextSelectTwo = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectTwo;
            }
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.AddListener(listenerStmtTwo);
    
            SendMarketBean("M1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S1", "M1"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S1", "M1"});
    
            SendSupportBean("S2", 10L, 20L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S2", 10L, 20L});
            }
    
            SendMarketBean("M2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M2"});
            Assert.IsFalse(listenerWindow.IsInvoked);
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M2"});
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyInfra where id = a";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(listenerStmtDelete);
    
            // delete S1
            epService.EPRuntime.SendEvent(new SupportBean_A("S1"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fieldsWin, new object[]{"S1", 1L, 2L});
            }
    
            SendMarketBean("M3");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S2", "M3"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S2", "M3"});
    
            // delete S2
            epService.EPRuntime.SendEvent(new SupportBean_A("S2"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fieldsWin, new object[]{"S2", 10L, 20L});
            }
    
            SendMarketBean("M4");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M4"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{null, "M4"});
    
            SendSupportBean("S3", 100L, 200L);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"S3", 100L, 200L});
            }
    
            SendMarketBean("M5");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S3", "M5"});
            EPAssertionUtil.AssertProps(listenerStmtTwo.AssertOneGetNewAndReset(), fieldsStmt, new object[]{"S3", "M5"});
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
    
        private void SendMarketBean(string symbol)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, 0l, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
}
