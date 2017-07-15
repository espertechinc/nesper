///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
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
    public class TestNamedWindowOnSelect : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerSelect;
        private SupportUpdateListener _listenerConsumer;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerSelect = new SupportUpdateListener();
            _listenerConsumer = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerSelect = null;
            _listenerConsumer = null;
        }
    
        [Test]
        public void TestInsertIntoWildcardUndType()
        {
            string[] fields = new string[] {"TheString", "IntPrimitive"};
    
            // create window
            string stmtTextCreate = "create window MyWindow.win:keepall() as select * from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from " + typeof(SupportBean).FullName + "(TheString like 'E%')";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create on-select stmt
            string stmtTextSelect = "on " + typeof(SupportBean_A).FullName + " insert into MyStream select mywin.* from MyWindow as mywin order by TheString asc";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
            Assert.AreEqual(StatementType.ON_INSERT, ((EPStatementSPI) stmtSelect).StatementMetadata.StatementType);
    
            // create consuming statement
            string stmtTextConsumer = "select * from default.MyStream";
            EPStatement stmtConsumer = _epService.EPAdministrator.CreateEPL(stmtTextConsumer);
            stmtConsumer.AddListener(_listenerConsumer);
    
            // create second inserting statement
            string stmtTextInsertTwo = "insert into MyStream select * from " + typeof(SupportBean).FullName + "(TheString like 'I%')";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertTwo);
    
            // send event
            SendSupportBean("E1", 1);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(_listenerConsumer.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // fire trigger
            SendSupportBean_A("A1");
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertProps(_listenerConsumer.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            // insert via 2nd insert into
            SendSupportBean("I2", 2);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertProps(_listenerConsumer.AssertOneGetNewAndReset(), fields, new object[]{"I2", 2});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // send event
            SendSupportBean("E3", 3);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(_listenerConsumer.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
    
            // fire trigger
            SendSupportBean_A("A2");
            Assert.AreEqual(1, _listenerSelect.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.LastNewData, fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
            _listenerSelect.Reset();
            Assert.AreEqual(2, _listenerConsumer.NewDataList.Count);
            EPAssertionUtil.AssertPropsPerRow(_listenerConsumer.GetNewDataListFlattened(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
            _listenerConsumer.Reset();
    
            // check type
            EventType consumerType = stmtConsumer.EventType;
            Assert.AreEqual(typeof(string), consumerType.GetPropertyType("TheString"));
            Assert.IsTrue(consumerType.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), consumerType.UnderlyingType);
    
            // check type
            EventType onSelectType = stmtSelect.EventType;
            Assert.AreEqual(typeof(string), onSelectType.GetPropertyType("TheString"));
            Assert.IsTrue(onSelectType.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), onSelectType.UnderlyingType);
    
            // delete all from named window
            string stmtTextDelete = "on " + typeof(SupportBean_B).FullName + " delete from MyWindow";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            SendSupportBean_B("B1");
    
            // fire trigger - nothing to insert
            SendSupportBean_A("A3");
    
            stmtConsumer.Dispose();
            stmtSelect.Dispose();
            stmtCreate.Dispose();
        }
    
        private SupportBean_A SendSupportBean_A(string id)
        {
            SupportBean_A bean = new SupportBean_A(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean_B SendSupportBean_B(string id)
        {
            SupportBean_B bean = new SupportBean_B(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void AssertCountS0Window(long expected)
        {
            Assert.AreEqual(expected, _epService.EPRuntime.ExecuteQuery("select count(*) as c0 from S0Window").Array[0].Get("c0"));
        }
    }
}
