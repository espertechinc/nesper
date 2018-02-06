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
using com.espertech.esper.client.soda;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectIn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
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
        public void TestInSelect()
        {
            String stmtText = "select id in (select id from S1#length(1000)) as value from S0";
    
            EPStatementSPI stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            RunTestInSelect();
        }
    
        [Test]
        public void TestInSelectOM()
        {
            EPStatementObjectModel subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create("id");
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView(View.Create("length", Expressions.Constant(1000))));
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryIn("id", subquery), "value");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String stmtText = "select id in (select id from S1#length(1000)) as value from S0";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            RunTestInSelect();
        }
    
        [Test]
        public void TestInSelectCompile()
        {
            String stmtText = "select id in (select id from S1#length(1000)) as value from S0";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            RunTestInSelect();
        }
    
        private void RunTestInSelect()
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestInSelectWhere()
        {
            String stmtText = "select id in (select id from S1#length(1000) where id > 0) as value from S0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestInSelectWhereExpressions()
        {
            String stmtText = "select 3*id in (select 2*id from S1#length(1000)) as value from S0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(6));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestInWildcard()
        {
            _epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            String stmtText = "select s0.anyObject in (select * from S1#length(1000)) as value from ArrayBean s0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SupportBean_S1 s1 = new SupportBean_S1(100);
            SupportBeanArrayCollMap arrayBean = new SupportBeanArrayCollMap(s1);
            _epService.EPRuntime.SendEvent(s1);
            _epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SupportBean_S2 s2 = new SupportBean_S2(100);
            arrayBean.AnyObject = s2;
            _epService.EPRuntime.SendEvent(s2);
            _epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestInNullable()
        {
            String stmtText = "select id from S0 as s0 where p00 in (select p10 from S1#length(1000))";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, null));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, null));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A"));
            Assert.AreEqual(4, _listener.AssertOneGetNewAndReset().Get("id"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-2, null));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, null));
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestInNullableCoercion()
        {
            String stmtText = "select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                              "where LongBoxed in " +
                              "(select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("A", 0, 0L);
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("B", null, null);
    
            SendBean("A", 0, 0L);
            Assert.IsFalse(_listener.IsInvoked);
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("B", 99, null);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
            SendBean("A", null, 99L);
            Assert.AreEqual(99L, _listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean("B", 98, null);
    
            SendBean("A", null, 98L);
            Assert.AreEqual(98L, _listener.AssertOneGetNewAndReset().Get("LongBoxed"));
        }
    
        [Test]
        public void TestInNullRow()
        {
            String stmtText = "select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                              "where IntBoxed in " +
                              "(select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("B", 1, 1L);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("A", 1, 1L);
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("IntBoxed"));
    
            SendBean("B", null, null);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("A", 1, 1L);
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("IntBoxed"));
        }
    
        [Test]
        public void TestNotInNullRow()
        {
            String stmtText = "select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                              "where IntBoxed not in " +
                              "(select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("B", 1, 1L);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("A", 1, 1L);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("B", null, null);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("A", 1, 1L);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestNotInSelect()
        {
            String stmtText = "select not id in (select id from S1#length(1000)) as value from S0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(false, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestNotInNullableCoercion()
        {
            String stmtText = "select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                              "where LongBoxed not in " +
                              "(select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendBean("A", 0, 0L);
            Assert.AreEqual(0L, _listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean("A", null, null);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean("B", null, null);
    
            SendBean("A", 1, 1L);
            Assert.IsFalse(_listener.IsInvoked);
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("B", 99, null);
    
            SendBean("A", null, null);
            Assert.IsFalse(_listener.IsInvoked);
            SendBean("A", null, 99L);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("B", 98, null);
    
            SendBean("A", null, 98L);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBean("A", null, 97L);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            try
            {
                String stmtText = "select " +
                              "intArr in (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr in (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        private void SendBean(string stringValue, int? intBoxed, long? longBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
