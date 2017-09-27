///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableFAFExecuteQuery : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            config.AddEventType<SupportBean>("SupportBean");
            config.AddEventType<SupportBean_A>("SupportBean_A");
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestFAFInsert() {
            string[] propertyNames = "p0,p1".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create table MyTable as (p0 string, p1 int)");
    
            string eplInsertInto = "insert into MyTable (p0, p1) select 'a', 1";
            EPOnDemandQueryResult resultOne = _epService.EPRuntime.ExecuteQuery(eplInsertInto);
            AssertFAFInsertResult(resultOne, propertyNames, stmt);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), propertyNames, new object[][] { new object[] { "a", 1 } });
        }
    
        [Test]
        public void TestFAFDelete() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create table MyTable as (p0 string primary key, thesum sum(int))");
            _epService.EPAdministrator.CreateEPL("into table MyTable select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString");
            for (int i = 0; i < 10; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("G" + i, i));
            }
            Assert.AreEqual(10L, GetTableCount(stmt));
            _epService.EPRuntime.ExecuteQuery("delete from MyTable");
            Assert.AreEqual(0L, GetTableCount(stmt));
        }
    
        [Test]
        public void TestFAFUpdate() {
            string[] fields = "p0,p1".Split(',');
            _epService.EPAdministrator.CreateEPL("@Name('TheTable') create table MyTable as (p0 string primary key, p1 string, thesum sum(int))");
            _epService.EPAdministrator.CreateEPL("into table MyTable select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("update MyTable set p1 = 'ABC'");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("TheTable").GetEnumerator(), fields, new object[][] { new object[] { "E1", "ABC" }, new object[] { "E2", "ABC" } });
        }
    
        [Test]
        public void TestFAFSelect() {
            string[] fields = "p0".Split(',');
            _epService.EPAdministrator.CreateEPL("@Name('TheTable') create table MyTable as (p0 string primary key, thesum sum(int))");
            _epService.EPAdministrator.CreateEPL("into table MyTable select TheString, sum(IntPrimitive) as thesum from SupportBean group by TheString");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("select * from MyTable");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields, new object[][] { new object[] { "E1" }, new object[] { "E2" } });
        }
    
        private long GetTableCount(EPStatement stmt) {
            return EPAssertionUtil.EnumeratorCount(stmt.GetEnumerator());
        }
    
        private void AssertFAFInsertResult(EPOnDemandQueryResult resultOne, string[] propertyNames, EPStatement stmt) {
            Assert.AreEqual(0, resultOne.Array.Length);
            Assert.AreSame(resultOne.EventType, stmt.EventType);
        }
    }
}
