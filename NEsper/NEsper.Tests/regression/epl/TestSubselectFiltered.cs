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
using com.espertech.esper.client.soda;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectFiltered 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Sensor", typeof(SupportSensorEvent));
            config.AddEventType("MyEvent", typeof(SupportBean));
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            config.AddEventType("S3", typeof(SupportBean_S3));
            config.AddEventType("S4", typeof(SupportBean_S4));
            config.AddEventType("S5", typeof(SupportBean_S5));
            config.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
        
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void Test3StreamKeyRangeCoercion() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("ST1", typeof(SupportBean_ST1));
            _epService.EPAdministrator.Configuration.AddEventType("ST2", typeof(SupportBean_ST2));
    
            String epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where TheString = st2.key2 and IntPrimitive between s0.p01Long and s1.p11Long) " +
                    "from ST2.std:lastevent() st2, ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            RunAssertion3StreamKeyRangeCoercion(epl, true);
    
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where TheString = st2.key2 and s1.p11Long >= IntPrimitive and s0.p01Long <= IntPrimitive) " +
                    "from ST2.std:lastevent() st2, ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            RunAssertion3StreamKeyRangeCoercion(epl, false);
    
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where TheString = st2.key2 and s1.p11Long > IntPrimitive) " +
                    "from ST2.std:lastevent() st2, ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST2", "G", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", -1L));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20L));
            Assert.AreEqual(13, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            stmt.Dispose();
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where TheString = st2.key2 and s1.p11Long < IntPrimitive) " +
                    "from ST2.std:lastevent() st2, ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST2", "G", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", -1L));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20L));
            Assert.AreEqual(21, _listener.AssertOneGetNewAndReset().Get("sumi"));
        }
    
        private void RunAssertion3StreamKeyRangeCoercion(String epl, bool isHasRangeReversal) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 9));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 17));
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", "X", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 20L));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST22", "G", 0));
            Assert.AreEqual(30, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 0L));    // range 0 to 20
            Assert.AreEqual(39, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", null, 0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", "G", 0));
            Assert.AreEqual(39, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 100L));   // range 0 to 100
            Assert.AreEqual(60, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", null));   // range 0 to null
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", null));    // range null to null
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", -1L));   // range null to -1
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));    // range 10 to -1
            if (isHasRangeReversal) {
                Assert.AreEqual(8, _listener.AssertOneGetNewAndReset().Get("sumi"));
            }
            else {
                Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
            }
    
            stmt.Dispose();
        }
    
        [Test]
        public void Test2StreamRangeCoercion()
        {
            String epl;

            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("ST1", typeof(SupportBean_ST1));
    
            // between and 'in' automatically revert the range (20 to 10 is the same as 10 to 20)
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where IntPrimitive between s0.P01Long and s1.P11Long) " +
                    "from ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            RunAssertion2StreamRangeCoercion(epl, true);

            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where IntPrimitive between s1.P11Long and s0.P01Long) " +
                    "from ST1.std:lastevent() s1, ST0.std:lastevent() s0";
            RunAssertion2StreamRangeCoercion(epl, true);
    
            // >= and <= should not automatically revert the range
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean.win:keepall() where IntPrimitive >= s0.P01Long and IntPrimitive <= s1.P11Long) " +
                    "from ST0.std:lastevent() s0, ST1.std:lastevent() s1";
            RunAssertion2StreamRangeCoercion(epl, false);
        }
    
        private void RunAssertion2StreamRangeCoercion(String epl, bool isHasRangeReversal) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 20L));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 21));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_1", 10L));  // range 10 to 20
            Assert.AreEqual(13, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_1", 13L));  // range 10 to 13
            Assert.AreEqual(13, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_2", 13L));  // range 13 to 13
            Assert.AreEqual(13, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 14));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_3", 13L));  // range 13 to 13
            Assert.AreEqual(13, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_4", 20L));  // range 13 to 20
            Assert.AreEqual(27, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_3", 11L));  // range 11 to 20
            Assert.AreEqual(39, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_4", null));  // range null to 16
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_5", null));  // range null to null
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_5", 20L));  // range 20 to null
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_6", 13L));  // range 20 to 13
            if (isHasRangeReversal) {
                Assert.AreEqual(27, _listener.AssertOneGetNewAndReset().Get("sumi"));
            }
            else {
                Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("sumi"));
            }
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestSameEventCompile()
        {
            String stmtText = "select (select * from S1.win:length(1000)) as events1 from S1";
            EPStatementObjectModel subquery = _epService.EPAdministrator.CompileEPL(stmtText);
            subquery = (EPStatementObjectModel) SerializableObjectCopier.Copy(subquery);
    
            EPStatement stmt = _epService.EPAdministrator.Create(subquery);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            Object theEvent = new SupportBean_S1(-1, "Y");
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean result = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
        }
    
        [Test]
        public void TestSameEventOM()
        {
            EPStatementObjectModel subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.CreateWildcard();
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView(View.Create("win", "length", Expressions.Constant(1000))));
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S1"));
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "events1");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String stmtText = "select (select * from S1.win:length(1000)) as events1 from S1";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            Object theEvent = new SupportBean_S1(-1, "Y");
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean result = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
        }
    
        [Test]
        public void TestSameEvent()
        {
            String stmtText = "select (select * from S1.win:length(1000)) as events1 from S1";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            Object theEvent = new SupportBean_S1(-1, "Y");
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean result = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
        }
    
        [Test]
        public void TestSelectWildcard()
        {
            String stmtText = "select (select * from S1.win:length(1000)) as events1 from S0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            Object theEvent = new SupportBean_S1(-1, "Y");
            _epService.EPRuntime.SendEvent(theEvent);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EventBean result = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
        }
    
        [Test]
        public void TestSelectWildcardNoName()
        {
            String stmtText = "select (select * from S1.win:length(1000)) from S0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("subselect_1"));
    
            Object theEvent = new SupportBean_S1(-1, "Y");
            _epService.EPRuntime.SendEvent(theEvent);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EventBean result = _listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("subselect_1"));
        }
    
        [Test]
        public void TestWhereConstant()
        {
            // single-column constant
            String stmtText = "select (select id from S1.win:length(1000) where p10='X') as ids1 from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "Y"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "Y"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "Z"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("ids1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("ids1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids1"));
            stmt.Dispose();
    
            // two-column constant
            stmtText = "select (select id from S1.win:length(1000) where p10='X' and p11='Y') as ids1 from S0";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X", "Y"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("ids1"));
            stmt.Dispose();
    
            // single range
            stmtText = "select (select TheString from SupportBean.std:lastevent() where IntPrimitive between 10 and 20) as ids1 from S0";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 15));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("ids1"));
        }
    
        [Test]
        public void TestWherePrevious()
        {
            String stmtText = "select (select prev(1, id) from S1.win:length(1000) where id=s0.id) as value from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            RunWherePrevious();
        }
    
        [Test]
        public void TestWherePreviousOM()
        {
            EPStatementObjectModel subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create().Add(Expressions.Previous(1, "id"));
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView(View.Create("win", "length", Expressions.Constant(1000))));
            subquery.WhereClause = Expressions.EqProperty("id","s0.id");
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S0", "s0"));
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "value");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String stmtText = "select (select prev(1,id) from S1.win:length(1000) where id=s0.id) as value from S0 as s0";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            RunWherePrevious();
        }
    
        [Test]
        public void TestWherePreviousCompile()
        {
            String stmtText = "select (select prev(1,id) from S1.win:length(1000) where id=s0.id) as value from S0 as s0";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            RunWherePrevious();
        }
    
        private void RunWherePrevious()
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestSelectWithWhereJoined()
        {
            String stmtText = "select (select id from S1.win:length(1000) where p10=s0.P00) as ids1 from S0 as s0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "Y"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "Z"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "X"));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("ids1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Y"));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("ids1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Z"));
            Assert.AreEqual(3, _listener.AssertOneGetNewAndReset().Get("ids1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids1"));
        }
    
        [Test]
        public void TestSelectWhereJoined2Streams()
        {
            String stmtText = "select (select id from S0.win:length(1000) where p00=s1.p10 and p00=s2.p20) as ids0 from S1.win:keepall() as s1, S2.win:keepall() as s2 where s1.id = s2.id";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(11, "s0_1"));
            Assert.AreEqual(99, _listener.AssertOneGetNewAndReset().Get("ids0"));
        }
    
        [Test]
        public void TestSelectWhereJoined3Streams()
        {
            String stmtText = "select (select id from S0.win:length(1000) where p00=s1.p10 and p00=s3.p30) as ids0 " +
                                "from S1.win:keepall() as s1, S2.win:keepall() as s2, S3.win:keepall() as s3 where s1.id = s2.id and s2.id = s3.id";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(10, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(11, "xxx"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(11, "s0_1"));
            Assert.AreEqual(99, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(98, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "s0_x"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(12, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(12, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(13, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(13, "s0_x"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(14, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(14, "xx"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(14, "s0_2"));
            Assert.AreEqual(98, _listener.AssertOneGetNewAndReset().Get("ids0"));
        }
    
        [Test]
        public void TestSelectWhereJoined3SceneTwo()
        {
            String stmtText = "select (select id from S0.win:length(1000) where p00=s1.p10 and p00=s3.p30 and p00=s2.p20) as ids0 " +
                                "from S1.win:keepall() as s1, S2.win:keepall() as s2, S3.win:keepall() as s3 where s1.id = s2.id and s2.id = s3.id";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(10, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(11, "xxx"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(11, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(98, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "s0_x"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(12, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(12, "s0_1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(13, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(13, "s0_x"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(14, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(14, "s0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S3(14, "s0_2"));
            Assert.AreEqual(98, _listener.AssertOneGetNewAndReset().Get("ids0"));
        }
    
        [Test]
        public void TestSelectWhereJoined4Coercion()
        {
            String stmtText = "select " +
              "(select IntPrimitive from MyEvent(TheString='S').win:length(1000) " +
              "  where IntBoxed=s1.LongBoxed and " +
                       "IntBoxed=s2.DoubleBoxed and " +
                       "DoubleBoxed=s3.IntBoxed" +
              ") as ids0 from " +
              "MyEvent(TheString='A').win:keepall() as s1, " +
              "MyEvent(TheString='B').win:keepall() as s2, " +
              "MyEvent(TheString='C').win:keepall() as s3 " +
              "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(stmtText);
    
            stmtText = "select " +
              "(select IntPrimitive from MyEvent(TheString='S').win:length(1000) " +
              "  where DoubleBoxed=s3.IntBoxed and " +
                       "IntBoxed=s2.DoubleBoxed and " +
                       "IntBoxed=s1.LongBoxed" +
              ") as ids0 from " +
              "MyEvent(TheString='A').win:keepall() as s1, " +
              "MyEvent(TheString='B').win:keepall() as s2, " +
              "MyEvent(TheString='C').win:keepall() as s3 " +
              "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(stmtText);
    
            stmtText = "select " +
              "(select IntPrimitive from MyEvent(TheString='S').win:length(1000) " +
              "  where DoubleBoxed=s3.IntBoxed and " +
                       "IntBoxed=s1.LongBoxed and " +
                       "IntBoxed=s2.DoubleBoxed" +
              ") as ids0 from " +
              "MyEvent(TheString='A').win:keepall() as s1, " +
              "MyEvent(TheString='B').win:keepall() as s2, " +
              "MyEvent(TheString='C').win:keepall() as s3 " +
              "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(stmtText);
        }
    
        [Test]
        public void TestSelectWhereJoined4BackCoercion()
        {
            String stmtText = "select " +
              "(select IntPrimitive from MyEvent(TheString='S').win:length(1000) " +
              "  where LongBoxed=s1.IntBoxed and " +
                       "LongBoxed=s2.DoubleBoxed and " +
                       "IntBoxed=s3.LongBoxed" +
              ") as ids0 from " +
              "MyEvent(TheString='A').win:keepall() as s1, " +
              "MyEvent(TheString='B').win:keepall() as s2, " +
              "MyEvent(TheString='C').win:keepall() as s3 " +
              "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4CoercionBack(stmtText);
    
            stmtText = "select " +
              "(select IntPrimitive from MyEvent(TheString='S').win:length(1000) " +
              "  where LongBoxed=s2.DoubleBoxed and " +
                       "IntBoxed=s3.LongBoxed and " +
                       "LongBoxed=s1.IntBoxed " +
              ") as ids0 from " +
              "MyEvent(TheString='A').win:keepall() as s1, " +
              "MyEvent(TheString='B').win:keepall() as s2, " +
              "MyEvent(TheString='C').win:keepall() as s3 " +
              "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4CoercionBack(stmtText);
        }
    
        private void TrySelectWhereJoined4CoercionBack(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("A", 1, 10, 200, 3000);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 1, 10, 200, 3000);
            SendBean("C", 1, 10, 200, 3000);
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -1, 11, 201, 0);     // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("A", 2, 201, 0, 0);
            SendBean("B", 2, 0, 0, 201);
            SendBean("C", 2, 0, 11, 0);
            Assert.AreEqual(-1, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -2, 12, 202, 0);     // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("A", 3, 202, 0, 0);
            SendBean("B", 3, 0, 0, 202);
            SendBean("C", 3, 0, -1, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -3, 13, 203, 0);     // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("A", 4, 203, 0, 0);
            SendBean("B", 4, 0, 0, 203.0001);
            SendBean("C", 4, 0, 13, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -4, 14, 204, 0);     // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("A", 5, 205, 0, 0);
            SendBean("B", 5, 0, 0, 204);
            SendBean("C", 5, 0, 14, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Stop();
        }
    
        private void TrySelectWhereJoined4Coercion(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("A", 1, 10, 200, 3000);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 1, 10, 200, 3000);
            SendBean("C", 1, 10, 200, 3000);
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -2, 11, 0, 3001);
            SendBean("A", 2, 0, 11, 0);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 2, 0, 0, 11);
            SendBean("C", 2, 3001, 0, 0);
            Assert.AreEqual(-2, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -3, 12, 0, 3002);
            SendBean("A", 3, 0, 12, 0);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 3, 0, 0, 12);
            SendBean("C", 3, 3003, 0, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -4, 11, 0, 3003);
            SendBean("A", 4, 0, 0, 0);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 4, 0, 0, 11);
            SendBean("C", 4, 3003, 0, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean("S", -5, 14, 0, 3004);
            SendBean("A", 5, 0, 14, 0);        // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean("B", 5, 0, 0, 11);
            SendBean("C", 5, 3004, 0, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Stop();
        }
    
        [Test]
        public void TestSelectWithWhere2Subqery()
        {
            String stmtText = "select id from S0 as s0 where " +
                            " id = (select id from S1.win:length(1000) where s0.id = id) or id = (select id from S2.win:length(1000) where s0.id = id)";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("id"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("id"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestJoinFilteredOne()
        {
            String stmtText = "select s0.id as s0id, s1.id as s1id, " +
                              "(select p20 from S2.win:length(1000) where id=s0.id) as s2p20, " +
                              "(select prior(1, p20) from S2.win:length(1000) where id=s0.id) as s2p20Prior, " +
                              "(select prev(1, p20) from S2.win:length(10) where id=s0.id) as s2p20Prev " +
                              "from S0.win:keepall() as s0, S1.win:keepall() as s1 " +
                              "where s0.id = s1.id and p00||p10 = (select p20 from S2.win:length(1000) where id=s0.id)";
            TryJoinFiltered(stmtText);
        }
    
        [Test]
        public void TestJoinFilteredTwo()
        {
            String stmtText = "select s0.id as s0id, s1.id as s1id, " +
                              "(select p20 from S2.win:length(1000) where id=s0.id) as s2p20, " +
                              "(select prior(1, p20) from S2.win:length(1000) where id=s0.id) as s2p20Prior, " +
                              "(select prev(1, p20) from S2.win:length(10) where id=s0.id) as s2p20Prev " +
                              "from S0.win:keepall() as s0, S1.win:keepall() as s1 " +
                              "where s0.id = s1.id and (select s0.P00||s1.p10 = p20 from S2.win:length(1000) where id=s0.id)";
            TryJoinFiltered(stmtText);
        }
    
        [Test]
        public void TestSubselectPrior()
        {
            String stmtTextOne = "insert into Pair " +
                    "select * from Sensor(device='A').std:lastevent() as a, Sensor(device='B').std:lastevent() as b " +
                    "where a.type = b.type";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            _epService.EPAdministrator.CreateEPL("insert into PairDuplicatesRemoved select * from Pair(1=2)");
    
            String stmtTextTwo = "insert into PairDuplicatesRemoved " +
                    "select * from Pair " +
                    "where a.id != coalesce((select a.id from PairDuplicatesRemoved.std:lastevent()), -1)" +
                    "  and b.id != coalesce((select b.id from PairDuplicatesRemoved.std:lastevent()), -1)";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(1,"Temperature","A",51,94.5));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(2,"Temperature","A",57,95.5));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(3,"Humidity","B",29,67.5));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(4,"Temperature","B",55,88.0));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, theEvent.Get("a.id"));
            Assert.AreEqual(4, theEvent.Get("b.id"));
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(5,"Temperature","B",65,85.0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(6,"Temperature","B",49,87.0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(7,"Temperature","A",51,99.5));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(7, theEvent.Get("a.id"));
            Assert.AreEqual(6, theEvent.Get("b.id"));
        }
    
        [Test]
        public void TestSubselectMixMax()
        {
            String stmtTextOne =
                         "select " +
                         " (select * from Sensor.ext:sort(1, measurement desc)) as high, " +
                         " (select * from Sensor.ext:sort(1, measurement asc)) as low " +
                         " from Sensor";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtTextOne);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(1, "Temp", "Dev1", 68.0, 96.5));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(2, "Temp", "Dev2", 70.0, 98.5));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
    
            _epService.EPRuntime.SendEvent(new SupportSensorEvent(3, "Temp", "Dev2", 65.0, 99.5));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(65.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
        }
    
        private void TryJoinFiltered(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "Y"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(1, "ab"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "b"));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, theEvent.Get("s0id"));
            Assert.AreEqual(1, theEvent.Get("s1id"));
            Assert.AreEqual("ab", theEvent.Get("s2p20"));
            Assert.AreEqual(null, theEvent.Get("s2p20Prior"));
            Assert.AreEqual(null, theEvent.Get("s2p20Prev"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "qx"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "q"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "x"));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, theEvent.Get("s0id"));
            Assert.AreEqual(2, theEvent.Get("s1id"));
            Assert.AreEqual("qx", theEvent.Get("s2p20"));
            Assert.AreEqual("ab", theEvent.Get("s2p20Prior"));
            Assert.AreEqual("ab", theEvent.Get("s2p20Prev"));
        }
    
        private void SendBean(string stringValue, int intPrimitive, int intBoxed, long longBoxed, double doubleBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
