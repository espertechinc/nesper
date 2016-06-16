///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnSelect : IndexBackingTableInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerSelect;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerSelect = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerSelect = null;
        }
    
        [Test]
        public void TestOnSelectIndexChoice() {
            RunAssertionOnSelectIndexChoice(true);
            RunAssertionOnSelectIndexChoice(false);
        }
    
        [Test]
        public void TestWindowAgg() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
    
            RunAssertionWindowAgg(true);
            RunAssertionWindowAgg(false);
        }
    
        [Test]
        public void TestSelectAggregationHavingStreamWildcard() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            RunAssertionSelectAggregationHavingStreamWildcard(true);
            RunAssertionSelectAggregationHavingStreamWildcard(false);
        }
    
        [Test]
        public void TestPatternTimedSelectNW() {
            RunAssertionPatternTimedSelect(true);
        }
    
        [Test]
        public void TestPatternTimedSelectTable() {
            RunAssertionPatternTimedSelect(false);
        }
    
        [Test]
        public void TestInvalid() {
            RunAssertionInvalid(true);
            RunAssertionInvalid(false);
        }
    
        [Test]
        public void TestSelectCondition() {
            RunAssertionSelectCondition(true);
            RunAssertionSelectCondition(false);
        }
    
        [Test]
        public void TestSelectJoinColumnsLimit() {
            RunAssertionSelectJoinColumnsLimit(true);
            RunAssertionSelectJoinColumnsLimit(false);
        }
    
        [Test]
        public void TestSelectAggregation() {
            RunAssertionSelectAggregation(true);
            RunAssertionSelectAggregation(false);
        }
    
        [Test]
        public void TestSelectAggregationCorrelated() {
            RunAssertionSelectAggregationCorrelated(true);
            RunAssertionSelectAggregationCorrelated(false);
        }
    
        [Test]
        public void TestSelectAggregationGrouping() {
            RunAssertionSelectAggregationGrouping(true);
            RunAssertionSelectAggregationGrouping(false);
        }
    
        [Test]
        public void TestSelectCorrelationDelete() {
            RunAssertionSelectCorrelationDelete(true);
            RunAssertionSelectCorrelationDelete(false);
        }
    
        [Test]
        public void TestPatternCorrelation() {
            RunAssertionPatternCorrelation(true);
            RunAssertionPatternCorrelation(false);
        }
    
        private void RunAssertionPatternCorrelation(bool namedWindow)
        {
            var fields = new string[] {"a", "b"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on pattern [every ea=" + typeof(SupportBean_A).FullName +
                    " or every eb=" + typeof(SupportBean_B).FullName + "] select mywin.* from MyInfra as mywin where a = coalesce(ea.id, eb.id)";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("X1");
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }
    
            SendSupportBean_B("E2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
            }
    
            SendSupportBean_A("E1");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            }
    
            SendSupportBean_B("E3");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
    
            stmtCreate.Dispose();
            stmtSelect.Dispose();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectCorrelationDelete(bool namedWindow)
        {
            var fields = new string[] {"a", "b"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select mywin.* from MyInfra as mywin where id = a";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfra where a = id";
            var stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("X1");
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }
    
            SendSupportBean_A("E2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
            }
    
            SendSupportBean_A("E1");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
    
            // delete event
            SendSupportBean_B("E1");
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            SendSupportBean_A("E1");
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, null);
            }
    
            SendSupportBean_A("E2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
            }
    
            stmtSelect.Dispose();
            stmtDelete.Dispose();
            stmtCreate.Dispose();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectAggregationGrouping(bool namedWindow)
        {
            var fields = new string[] {"a", "sumb"};
            var listenerSelectTwo = new SupportUpdateListener();
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select a, sum(b) as sumb from MyInfra group by a order by a desc";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create select stmt
            var stmtTextSelectTwo = "on " + typeof(SupportBean_A).FullName + " select a, sum(b) as sumb from MyInfra group by a having sum(b) > 5 order by a desc";
            var stmtSelectTwo = _epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            stmtSelectTwo.AddListener(listenerSelectTwo);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // fire trigger
            SendSupportBean_A("A1");
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(listenerSelectTwo.IsInvoked);
    
            // send 3 events
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E1", 5);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(listenerSelectTwo.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("A1");
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.LastNewData, fields, new object[][]{new object[]{"E2", 2}, new object[]{"E1", 6}});
            Assert.IsNull(_listenerSelect.LastOldData);
            _listenerSelect.Reset();
            EPAssertionUtil.AssertPropsPerRow(listenerSelectTwo.LastNewData, fields, new object[][]{new object[]{"E1", 6}});
            Assert.IsNull(_listenerSelect.LastOldData);
            _listenerSelect.Reset();
    
            // send 3 events
            SendSupportBean("E4", -1);
            SendSupportBean("E2", 10);
            SendSupportBean("E1", 100);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            SendSupportBean_A("A2");
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.LastNewData, fields, new object[][]{new object[]{"E4", -1}, new object[]{"E2", 12}, new object[]{"E1", 106}});
    
            // create delete stmt, delete E2
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfra where id = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            SendSupportBean_B("E2");
    
            SendSupportBean_A("A3");
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.LastNewData, fields, new object[][]{new object[]{"E4", -1}, new object[]{"E1", 106}});
            Assert.IsNull(_listenerSelect.LastOldData);
            _listenerSelect.Reset();
            EPAssertionUtil.AssertPropsPerRow(listenerSelectTwo.LastNewData, fields, new object[][]{new object[]{"E1", 106}});
            Assert.IsNull(listenerSelectTwo.LastOldData);
            listenerSelectTwo.Reset();
    
            var resultType = stmtSelect.EventType;
            Assert.AreEqual(2, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(string), resultType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            stmtSelectTwo.Dispose();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectAggregationCorrelated(bool namedWindow)
        {
            var fields = new string[] {"sumb"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select sum(b) as sumb from MyInfra where a = id";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("A1");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{null});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{null}});
            }
    
            // fire trigger
            SendSupportBean_A("E2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{2}});
            }
    
            SendSupportBean("E2", 10);
            SendSupportBean_A("E2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{12});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{12}});
            }
    
            var resultType = stmtSelect.EventType;
            Assert.AreEqual(1, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectAggregation(bool namedWindow)
        {
            var fields = new string[] {"sumb"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra (a string primary key, b int primary key)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select sum(b) as sumb from MyInfra";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("A1");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{6});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{6}});
            }
    
            // create delete stmt
            var stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyInfra where id = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // Delete E2
            SendSupportBean_B("E2");
    
            // fire trigger
            SendSupportBean_A("A2");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{4});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{4}});
            }
    
            SendSupportBean("E4", 10);
            SendSupportBean_A("A3");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{14});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[]{14}});
            }
    
            var resultType = stmtSelect.EventType;
            Assert.AreEqual(1, resultType.PropertyNames.Length);
            Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectJoinColumnsLimit(bool namedWindow)
        {
            var fields = new string[] {"triggerid", "wina", "b"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra (a string primary key, b int)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " as trigger select trigger.id as triggerid, win.a as wina, b from MyInfra as win order by wina";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("A1");
            Assert.AreEqual(2, _listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fields, new object[]{"A1", "E1", 1});
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[1], fields, new object[]{"A1", "E2", 2});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "A1", "E1", 1 }, new object[] { "A1", "E2", 2 } });
            }
    
            // try limit clause
            stmtSelect.Dispose();
            stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " as trigger select trigger.id as triggerid, win.a as wina, b from MyInfra as win order by wina limit 1";
            stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            SendSupportBean_A("A1");
            Assert.AreEqual(1, _listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fields, new object[]{"A1", "E1", 1});
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "A1", "E1", 1 } });
            }
    
            stmtCreate.Dispose();
            _listenerSelect.Reset();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectCondition(bool namedWindow)
        {
            var fieldsCreate = new string[] {"a", "b"};
            var fieldsOnSelect = new string[] {"a", "b", "id"};
    
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra (a string primary key, b int)";
            var stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create select stmt
            var stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " select mywin.*, id from MyInfra as mywin where MyInfra.b < 3 order by a asc";
            var stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
            Assert.AreEqual(StatementType.ON_SELECT, ((EPStatementSPI) stmtSelect).StatementMetadata.StatementType);
    
            // create insert into
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean("E1", 1);
            SendSupportBean("E2", 2);
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // fire trigger
            SendSupportBean_A("A1");
            Assert.AreEqual(2, _listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fieldsCreate, new object[]{"E1", 1});
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[1], fieldsCreate, new object[]{"E2", 2});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fieldsCreate, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsOnSelect, new object[][] { new object[] { "E1", 1, "A1" }, new object[] { "E2", 2, "A1" } });
            } else {
                Assert.IsFalse(stmtSelect.HasFirst());
            }
    
            SendSupportBean("E4", 0);
            SendSupportBean_A("A2");
            Assert.AreEqual(3, _listenerSelect.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fieldsOnSelect, new object[]{"E1", 1, "A2"});
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[1], fieldsOnSelect, new object[]{"E2", 2, "A2"});
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[2], fieldsOnSelect, new object[]{"E4", 0, "A2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fieldsCreate, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 }, new object[] { "E4", 0 } });
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsCreate, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E4", 0 } });
            }
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            _listenerSelect.Reset();
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionInvalid(bool namedWindow) {
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select * from " + typeof(SupportBean).FullName :
                    "create table MyInfra (TheString string, IntPrimitive int)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            SupportMessageAssertUtil.TryInvalid(_epService, "on " + typeof(SupportBean_A).FullName + " select * from MyInfra where sum(IntPrimitive) > 100",
                    "Error validating expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [on com.espertech.esper.support.bean.SupportBean_A select * from MyInfra where sum(IntPrimitive) > 100]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "on " + typeof(SupportBean_A).FullName + " insert into MyStream select * from DUMMY",
                    "Named window or table 'DUMMY' has not been declared [on com.espertech.esper.support.bean.SupportBean_A insert into MyStream select * from DUMMY]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "on " + typeof(SupportBean_A).FullName + " select prev(1, TheString) from MyInfra",
                    "Error starting statement: Failed to validate select-clause expression 'prev(1,TheString)': Previous function cannot be used in this context [on com.espertech.esper.support.bean.SupportBean_A select prev(1, TheString) from MyInfra]");
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionPatternTimedSelect(bool namedWindow)
        {
            // test for JIRA ESPER-332
            SendTimer(0, _epService);
    
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as select * from " + typeof(SupportBean).FullName :
                    "create table MyInfra as (TheString string)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            var stmtCount = "on pattern[every timer:interval(10 sec)] select count(eve), eve from MyInfra as eve";
            _epService.EPAdministrator.CreateEPL(stmtCount);
    
            var stmtTextOnSelect = "on pattern [ every timer:interval(10 sec)] select TheString from MyInfra having count(TheString) > 0";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtTextOnSelect);
            stmt.AddListener(_listenerSelect);
    
            var stmtTextInsertOne = namedWindow ?
                    "insert into MyInfra select * from " + typeof(SupportBean).FullName :
                    "insert into MyInfra select TheString from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendTimer(11000, _epService);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            SendTimer(21000, _epService);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            SendSupportBean("E1", 1);
            SendTimer(31000, _epService);
            Assert.AreEqual("E1", _listenerSelect.AssertOneGetNewAndReset().Get("TheString"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSelectAggregationHavingStreamWildcard(bool namedWindow)
        {
            // create window
            var stmtTextCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as (a string, b int)" :
                    "create table MyInfra as (a string primary key, b int primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            var stmtTextSelect = "on SupportBean_A select mwc.* as mwcwin from MyInfra mwc where id = a group by a having sum(b) = 20";
            var select = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            Assert.IsFalse(select.StatementContext.IsStatelessSelect);
            select.AddListener(_listenerSelect);
    
            // send 3 event
            SendSupportBean("E1", 16);
            SendSupportBean("E2", 2);
            SendSupportBean("E1", 4);
    
            // fire trigger
            SendSupportBean_A("E1");
            var events = _listenerSelect.LastNewData;
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("E1", events[0].Get("mwcwin.a"));
            Assert.AreEqual("E1", events[1].Get("mwcwin.a"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionWindowAgg(bool namedWindow) {
            var eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            var eplInsert = namedWindow ?
                    "insert into MyInfra select * from SupportBean" :
                    "insert into MyInfra select TheString, IntPrimitive from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInsert);
            _epService.EPAdministrator.CreateEPL("on S1 as s1 delete from MyInfra where s1.p10 = TheString");
    
            var stmt = _epService.EPAdministrator.CreateEPL("on S0 as s0 " +
                    "select window(win.*) as c0," +
                    "window(win.*).where(v => v.IntPrimitive < 2) as c1, " +
                    "window(win.*).toMap(k=>k.TheString,v=>v.IntPrimitive) as c2 " +
                    "from MyInfra as win");
            stmt.AddListener(_listenerSelect);
    
            var beans = new SupportBean[3];
            for (var i = 0; i < beans.Length; i++) {
                beans[i] = new SupportBean("E" + i, i);
            }
    
            _epService.EPRuntime.SendEvent(beans[0]);
            _epService.EPRuntime.SendEvent(beans[1]);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(namedWindow, beans, new int[]{0, 1}, new int[]{0, 1}, "E0,E1".Split(','), new object[] {0,1});
    
            // add bean
            _epService.EPRuntime.SendEvent(beans[2]);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertReceived(namedWindow, beans, new int[]{0, 1, 2}, new int[]{0, 1}, "E0,E1,E2".Split(','), new object[] {0,1, 2});
    
            // delete bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(12));
            AssertReceived(namedWindow, beans, new int[]{0, 2}, new int[]{0}, "E0,E2".Split(','), new object[] {0,2});
    
            // delete another bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "E0"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(14));
            AssertReceived(namedWindow, beans, new int[]{2}, new int[0], "E2".Split(','), new object[] {2});
    
            // delete last bean
            _epService.EPRuntime.SendEvent(new SupportBean_S1(15, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(16));
            AssertReceived(namedWindow, beans, null, null, null, null);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void AssertReceived(bool namedWindow, SupportBean[] beans, int[] indexesAll, int[] indexesWhere, string[] mapKeys, object[] mapValues) {
            var received = _listenerSelect.AssertOneGetNewAndReset();
            object[] expectedAll;
            object[] expectedWhere;
            if (!namedWindow) {
                expectedAll = SupportBean.GetOAStringAndIntPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetOAStringAndIntPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsAnyOrder(expectedAll, (object[]) received.Get("c0"));
                var receivedColl = received.Get("c1").Unwrap<object>();
                EPAssertionUtil.AssertEqualsAnyOrder(expectedWhere, receivedColl == null ? null : receivedColl.ToArray());
            }
            else {
                expectedAll = SupportBean.GetBeansPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetBeansPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsExactOrder(expectedAll, (object[]) received.Get("c0"));
                EPAssertionUtil.AssertEqualsExactOrder(expectedWhere, received.Get("c1").Unwrap<object>());
            }
            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }
    
        public void RunAssertionOnSelectIndexChoice(bool isNamedWindow) {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
    
            var backingUniqueS1 = "unique hash={s1(string)} btree={}";
            var backingUniqueS1L1 = "unique hash={s1(string),l1(long)} btree={}";
            var backingNonUniqueS1 = "non-unique hash={s1(string)} btree={}";
            var backingUniqueS1D1 = "unique hash={s1(string),d1(double)} btree={}";
            var backingBtreeI1 = "non-unique hash={} btree={i1(int)}";
            var backingBtreeD1 = "non-unique hash={} btree={d1(double)}";
            var expectedIdxNameS1 = isNamedWindow ? null : "MyInfra";
    
            var preloadedEventsOne = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
            IndexAssertionEventSend eventSendAssertion = () =>  {
                var fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 20});
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E1", 10});
            };
    
            // single index one field (std:unique(s1))
            AssertIndexChoice(isNamedWindow, new string[0], preloadedEventsOne, "std:unique(s1)",
                new IndexAssertion[] {
                        new IndexAssertion(null, "s1 = s2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                        new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                        new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"),// busted
                });
    
            // single index one field (std:unique(s1))
            if (isNamedWindow) {
                var indexOneField = new string[] {"create unique index One on MyInfra (s1)"};
                AssertIndexChoice(isNamedWindow, indexOneField, preloadedEventsOne, "std:unique(s1)",
                        new IndexAssertion[] {
                                new IndexAssertion(null, "s1 = s2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1, eventSendAssertion),
                                new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"),// busted
                        });
            }
    
            // single index two field  (std:unique(s1))
            var indexTwoField = new string[] {"create unique index One on MyInfra (s1, l1)"};
            AssertIndexChoice(isNamedWindow, indexTwoField, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1L1, eventSendAssertion),
                    });
            AssertIndexChoice(isNamedWindow, indexTwoField, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", expectedIdxNameS1, isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingUniqueS1L1, eventSendAssertion),
                    });
    
            // two index one unique  (std:unique(s1))
            var indexSetTwo = new string[] {
                    "create index One on MyInfra (s1)",
                    "create unique index Two on MyInfra (s1, d1)"};
            AssertIndexChoice(isNamedWindow, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", isNamedWindow ? "Two" : "MyInfra", isNamedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2"), // busted
                    });
    
            // two index one unique  (win:keepall)
            AssertIndexChoice(isNamedWindow, indexSetTwo, preloadedEventsOne, "win:keepall()",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,One)')", "s1 = ssb2.s2 and l1 = ssb2.l2", "One", backingNonUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                            new IndexAssertion("@Hint('index(explicit,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2", isNamedWindow ? "One" : "MyInfra", isNamedWindow ? backingNonUniqueS1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(null, "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2", isNamedWindow ? "Two" : "MyInfra", isNamedWindow ? backingUniqueS1D1 : backingUniqueS1, eventSendAssertion),
                            new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2"), // busted
                    });
    
            // range  (std:unique(s1))
            IndexAssertionEventSend noAssertion = () => { };
            var indexSetThree = new string[] {
                    "create index One on MyInfra (i1 btree)",
                    "create index Two on MyInfra (d1 btree)"};
            AssertIndexChoice(isNamedWindow, indexSetThree, preloadedEventsOne, "std:unique(s1)",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "i1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                            new IndexAssertion(null, "d1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                            new IndexAssertion("@Hint('index(One, bust)')", "d1 between 1 and 10"),// busted
                    });
    
            // rel ops
            var preloadedEventsRelOp = new object[] {new SupportSimpleBeanOne("E1", 10, 11, 12)};
            IndexAssertionEventSend relOpAssertion = () =>  {
                var fields = "ssb2.s2,ssb1.s1,ssb1.i1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("EX", 0, 0, 0));
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"EX", "E1", 10});
            };
            AssertIndexChoice(isNamedWindow, new string[0], preloadedEventsRelOp, "win:keepall()",
                    new IndexAssertion[] {
                            new IndexAssertion(null, "9 < i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                            new IndexAssertion(null, "10 <= i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                            new IndexAssertion(null, "i1 <= 10", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                            new IndexAssertion(null, "i1 < 11", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                            new IndexAssertion(null, "11 > i1", null, isNamedWindow ? backingBtreeI1 : null, relOpAssertion),
                    });
        }
    
        private void AssertIndexChoice(bool isNamedWindow, string[] indexes, object[] preloadedEvents, string datawindow, IndexAssertion[] assertions)
        {
            var eplCreate = isNamedWindow ?
                    "@Name('create-window') create window MyInfra." + datawindow + " as SSB1" :
                    "@Name('create-table') create table MyInfra(s1 string primary key, i1 int, d1 double, l1 long)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select s1,i1,d1,l1 from SSB1");
            foreach (var index in indexes) {
                _epService.EPAdministrator.CreateEPL(index, "create-index '" + index + "'");
            }
            foreach (var @event in preloadedEvents) {
                _epService.EPRuntime.SendEvent(@event);
            }
    
            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var consumeEpl = INDEX_CALLBACK_HOOK +
                        (assertion.Hint == null ? "" : assertion.Hint) +
                        "@Name('on-select') on SSB2 as ssb2 " +
                        "select * " +
                        "from MyInfra as ssb1 where " + assertion.WhereClause;
    
                EPStatement consumeStmt;
                try {
                    consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
                }
                catch (EPStatementException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }
                    throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                }
    
                // assert index and access
                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(assertion.ExpectedIndexName, assertion.IndexBackingClass);
                consumeStmt.AddListener(_listenerSelect);
                assertion.EventSendAssertion.Invoke();
                consumeStmt.Dispose();
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean_A SendSupportBean_A(string id)
        {
            var bean = new SupportBean_A(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean_B SendSupportBean_B(string id)
        {
            var bean = new SupportBean_B(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
