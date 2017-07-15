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
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnDelete 
    {
        private EPServiceProviderSPI epService;
        private SupportUpdateListener listenerInfra;
        private SupportUpdateListener listenerDelete;
        private SupportUpdateListener listenerSelect;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            listenerInfra = new SupportUpdateListener();
            listenerDelete = new SupportUpdateListener();
            listenerSelect = new SupportUpdateListener();        
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_A), typeof(SupportBean_B)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listenerInfra = null;
            listenerDelete = null;
            listenerSelect = null;
        }
        
        [Test]
        public void TestDeleteCondition() {
            RunAssertionDeleteCondition(true);
            RunAssertionDeleteCondition(false);
        }
    
        [Test]
        public void TestDeletePattern() {
            RunAssertionDeletePattern(true);
            RunAssertionDeletePattern(false);
        }
    
        [Test]
        public void TestDeleteAll() {
            RunAssertionDeleteAll(true);
            RunAssertionDeleteAll(false);
        }
    
        private void RunAssertionDeleteAll(bool namedWindow) 
        {
            // create window
            string stmtTextCreate = namedWindow ?
                    "@Name('CreateInfra') create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "@Name('CreateInfra') create table MyInfra (a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerInfra);
    
            // create delete stmt
            string stmtTextDelete = "@Name('OnDelete') on " + typeof(SupportBean_A).FullName + " delete from MyInfra";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(listenerDelete);
            EPAssertionUtil.AssertEqualsAnyOrder(stmtDelete.EventType.PropertyNames, new string[]{"a", "b"});
    
            // create insert into
            string stmtTextInsertOne = "@Name('Insert') insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string[] fields = new string[] {"a", "b"};
            string stmtTextSelect = "@Name('Select') select irstream MyInfra.a as a, b from MyInfra as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(listenerSelect);
    
            // Delete all events, no result expected
            SendSupportBean_A("A1");
            Assert.IsFalse(listenerInfra.IsInvoked);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerDelete.IsInvoked);
            Assert.AreEqual(0, GetCount("MyInfra"));
    
            // send 1 event
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            else {
                Assert.IsFalse(listenerInfra.IsInvoked);
                Assert.IsFalse(listenerSelect.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, null);
            Assert.AreEqual(1, GetCount("MyInfra"));
    
            // Delete all events, 1 row expected
            SendSupportBean_A("A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            Assert.AreEqual(0, GetCount("MyInfra"));
    
            // send 2 events
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            listenerInfra.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            Assert.IsFalse(listenerDelete.IsInvoked);
            Assert.AreEqual(2, GetCount("MyInfra"));
    
            // Delete all events, 2 rows expected
            SendSupportBean_A("A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[1], fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            Assert.AreEqual(2, listenerDelete.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerDelete.LastNewData[0], fields, new object[]{"E2", 2});
            EPAssertionUtil.AssertProps(listenerDelete.LastNewData[1], fields, new object[]{"E3", 3});
            Assert.AreEqual(0, GetCount("MyInfra"));
    
            listenerInfra.Reset();
            listenerDelete.Reset();
            listenerSelect.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionDeletePattern(bool isNamedWindow) 
        {
            // create infra
            string stmtTextCreate = isNamedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from SupportBean" :
                    "create table MyInfra(a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerInfra);
    
            // create delete stmt
            string stmtTextDelete = "on pattern [every ea=" + typeof(SupportBean_A).FullName + " or every eb=" + typeof(SupportBean_B).FullName + "] " + " delete from MyInfra";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            stmtDelete.AddListener(listenerDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 1 event
            string[] fields = new string[] {"a", "b"};
            SendSupportBean("E1", 1);
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, null);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            Assert.AreEqual(1, GetCount("MyInfra"));
    
            // Delete all events using A, 1 row expected
            SendSupportBean_A("A1");
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            Assert.AreEqual(0, GetCount("MyInfra"));
    
            // send 1 event
            SendSupportBean("E2", 2);
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
            Assert.AreEqual(1, GetCount("MyInfra"));
    
            // Delete all events using B, 1 row expected
            SendSupportBean_B("B1");
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            Assert.AreEqual(0, GetCount("MyInfra"));
    
            stmtDelete.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionDeleteCondition(bool isNamedWindow) {    
        
            // create infra
            string stmtTextCreate = isNamedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from SupportBean" :
                    "create table MyInfra (a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(listenerInfra);
    
            // create delete stmt
            string stmtTextDelete = "on SupportBean_A delete from MyInfra where 'X' || a || 'X' = id";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create delete stmt
            stmtTextDelete = "on SupportBean_B delete from MyInfra where b < 5";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.AreEqual(3, GetCount("MyInfra"));
            listenerInfra.Reset();
            string[] fields = new string[] {"a", "b"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
    
            // delete E2
            SendSupportBean_A("XE2X");
            if (isNamedWindow) {
                Assert.AreEqual(1, listenerInfra.LastOldData.Length);
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E2", 2});
            }
            listenerInfra.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
            Assert.AreEqual(2, GetCount("MyInfra"));
    
            SendSupportBean("E7", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E7", 7 } });
            Assert.AreEqual(3, GetCount("MyInfra"));
    
            // delete all under 5
            SendSupportBean_B("B1");
            if (isNamedWindow) {
                Assert.AreEqual(2, listenerInfra.LastOldData.Length);
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[1], fields, new object[]{"E3", 3});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E7", 7 } });
            Assert.AreEqual(1, GetCount("MyInfra"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean_A SendSupportBean_A(string id)
        {
            SupportBean_A bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean_B SendSupportBean_B(string id)
        {
            SupportBean_B bean = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private long GetCount(string windowOrTableName) 
        {
            return epService.EPRuntime
                .ExecuteQuery("select count(*) as c0 from " + windowOrTableName)
                .Array[0].Get("c0")
                .AsLong();
        }
    }
}
