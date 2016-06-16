///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectAggregationGroupBy 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();        
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestNamedWindowSubqueryIndexShared()
        {
            // test uncorrelated
            _epService.EPAdministrator.CreateEPL("@Hint('enable_window_subquery_indexshare')" +
                    "create window SBWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into SBWindow select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
    
            var stmtUncorrelated = _epService.EPAdministrator.CreateEPL("select " +
                    "(select TheString as c0, Sum(IntPrimitive) as c1 from SBWindow group by TheString).Take(10) as e1 from S0");
            stmtUncorrelated.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][] { new Object[] {"E1", 30}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][] { new Object[] {"E1", 30}, new Object[] {"E2", 200}});
            stmtUncorrelated.Dispose();
    
            // test correlated
            var stmtCorrelated = _epService.EPAdministrator.CreateEPL("select " +
                    "(select TheString as c0, Sum(IntPrimitive) as c1 from SBWindow where TheString = s0.p00 group by TheString).Take(10) as e1 from S0 as s0");
            stmtCorrelated.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][] { new Object[] {"E1", 30}});
    
            stmtCorrelated.Dispose();
        }
    
        [Test]
        public void TestUncorrelatedIteratorAndExpressionDef()
        {
            var fields = "c0,c1".Split(',');
            const string epl = "expression getGroups {" +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    "  from SupportBean.win:keepall() group by TheString)" +
                    "}" +
                    "select getGroups() as e1, getGroups().Take(10) as e2 from S0.std:lastevent()";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            SendSBEventAndTrigger("E1", 20);
            foreach (var @event in new EventBean[] {_listener.AssertOneGetNew(), stmt.First()})
            {
                AssertMapField("e1", @event, fields, new Object[] {"E1", 20});
                AssertMapMultiRow("e2", @event, "c0", fields, new Object[][] { new Object[] {"E1", 20}});
            }
            _listener.Reset();
    
            SendSBEventAndTrigger("E2", 30);
            foreach (var @event in new EventBean[] {_listener.AssertOneGetNew(), stmt.First()})
            {
                AssertMapField("e1", @event, fields, null);
                AssertMapMultiRow("e2", @event, "c0", fields, new Object[][] { new Object[] {"E1", 20}, new Object[] {"E2", 30}});
            }
            _listener.Reset();
        }
    
        [Test]
        public void TestCorrelatedWithEnumMethod()
        {
            var fieldName = "subq";
            var fields = "c0,c1".Split(',');
    
            var eplEnumCorrelated = "select " +
                    "(select TheString as c0, Sum(IntPrimitive) as c1 " +
                    " from SupportBean.win:keepall() " +
                    " where IntPrimitive = s0.id " +
                    " group by TheString).Take(100) as subq " +
                    "from S0 as s0";
            var stmtEnumUnfiltered = _epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
            stmtEnumUnfiltered.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 10}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 20}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E2", 100}});
        }
    
        [Test]
        public void TestUncorrelatedWithEnumerationMethod()
        {
            const string fieldName = "subq";
            var fields = "c0,c1".Split(',');
    
            // test unfiltered
            const string eplEnumUnfiltered = "select " +
                                             "(select TheString as c0, Sum(IntPrimitive) as c1 " +
                                             " from SupportBean.win:keepall() " +
                                             " group by TheString).Take(100) as subq " +
                                             "from S0 as s0";
            var stmtEnumUnfiltered = _epService.EPAdministrator.CreateEPL(eplEnumUnfiltered);
            stmtEnumUnfiltered.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);
    
            SendSBEventAndTrigger("E1", 10);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 10}});
    
            SendSBEventAndTrigger("E1", 20);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 30}});
    
            SendSBEventAndTrigger("E2", 100);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 30}, new Object[] {"E2", 100}});
    
            SendSBEventAndTrigger("E3", 2000);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 30}, new Object[] {"E2", 100}, new Object[] {"E3", 2000}});
            stmtEnumUnfiltered.Dispose();
    
            // test filtered
            const string eplEnumFiltered = "select " +
                                           "(select TheString as c0, Sum(IntPrimitive) as c1 " +
                                           " from SupportBean.win:keepall() " +
                                           " where IntPrimitive > 100 " +
                                           " group by TheString).Take(100) as subq " +
                                           "from S0 as s0";
            var stmtEnumFiltered = _epService.EPAdministrator.CreateEPL(eplEnumFiltered);
            stmtEnumFiltered.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);
    
            SendSBEventAndTrigger("E1", 10);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);
    
            SendSBEventAndTrigger("E1", 200);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 200}});
    
            SendSBEventAndTrigger("E1", 11);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 200}});
    
            SendSBEventAndTrigger("E1", 201);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 401}});
    
            SendSBEventAndTrigger("E2", 300);
            AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new Object[][]{new Object[] {"E1", 401}, new Object[] {"E2", 300}});
    
            stmtEnumFiltered.Dispose();
        }
    
        [Test]
        public void TestUncorrelatedUnfiltered()
        {
            const string fieldName = "subq";
            var fields = "c0,c1".Split(',');
            const string eplNoDelete = "select " +
                                       "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                       "from SupportBean.win:keepall() " +
                                       "group by TheString) as subq " +
                                       "from S0 as s0";
            var stmtNoDelete = _epService.EPAdministrator.CreateEPL(eplNoDelete);
            stmtNoDelete.Events += _listener.Update;
            RunAssertionNoDelete(fieldName, fields);
            stmtNoDelete.Dispose();
    
            // try SODA
            var model = _epService.EPAdministrator.CompileEPL(eplNoDelete);
            Assert.AreEqual(eplNoDelete, model.ToEPL());
            stmtNoDelete = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtNoDelete.Text, eplNoDelete);
            stmtNoDelete.Events += _listener.Update;
            RunAssertionNoDelete(fieldName, fields);
            stmtNoDelete.Dispose();
    
            // test named window with delete/remove
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on S1 delete from MyWindow where id = IntPrimitive");
            var stmtDelete = _epService.EPAdministrator.CreateEPL("@Hint('disable_reclaim_group') select (select TheString as c0, Sum(IntPrimitive) as c1 " +
                    " from MyWindow group by TheString) as subq from S0 as s0");
            stmtDelete.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            SendSBEventAndTrigger("E1", 10);
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E1", 10});
    
            SendS1EventAndTrigger(10);     // delete 10
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            SendSBEventAndTrigger("E2", 20);
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E2", 20});
    
            SendSBEventAndTrigger("E2", 21);
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E2", 41});
    
            SendSBEventAndTrigger("E1", 30);
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            SendS1EventAndTrigger(30);     // delete 30
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E2", 41});
    
            SendS1EventAndTrigger(20);     // delete 20
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E2", 21});
    
            SendSBEventAndTrigger("E1", 31);    // two groups
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            SendS1EventAndTrigger(21);     // delete 21
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E1", 31});
            stmtDelete.Dispose();
    
            // test multiple group-by criteria
            var fieldsMultiGroup = "c0,c1,c2,c3,c4".Split(',');
            const string eplMultiGroup = "select " +
                                         "(select TheString as c0, IntPrimitive as c1, TheString||'x' as c2, " +
                                         "    IntPrimitive * 1000 as c3, Sum(LongPrimitive) as c4 " +
                                         " from SupportBean.win:keepall() " +
                                         " group by TheString, IntPrimitive) as subq " +
                                         "from S0 as s0";
            var stmtMultiGroup = _epService.EPAdministrator.CreateEPL(eplMultiGroup);
            stmtMultiGroup.Events += _listener.Update;
    
            SendSBEventAndTrigger("G1", 1, 100L);
            AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, new Object[]{"G1", 1, "G1x", 1000, 100L});
    
            SendSBEventAndTrigger("G1", 1, 101L);
            AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, new Object[]{"G1", 1, "G1x", 1000, 201L});
    
            SendSBEventAndTrigger("G2", 1, 200L);
            AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, null);
        }
    
        [Test]
        public void TestContextPartitioned()
        {
            const string fieldName = "subq";
            var fields = "c0,c1".Split(',');
    
            _epService.EPAdministrator.CreateEPL(
                    "create context MyCtx partition by TheString from SupportBean, p00 from S0");
    
            const string stmtText = "context MyCtx select " +
                                    "(select TheString as c0, Sum(IntPrimitive) as c1 " +
                                    " from SupportBean.win:keepall() " +
                                    " group by TheString) as subq " +
                                    "from S0 as s0";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P1"));
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"P1", 100});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "P2"));
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "P2"));
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"P2", 200});
    
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 205));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "P2"));
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"P2", 405});
        }
    
        [Test]
        public void TestInvalid()
        {
            String epl;
    
            // not fully aggregated
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean.win:keepall() group by IntPrimitive) from S0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause [select (select TheString, sum(LongPrimitive) from SupportBean.win:keepall() group by IntPrimitive) from S0]");
    
            // correlated group-by not allowed
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean.win:keepall() group by TheString, s0.Id) from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (property 'Id' is not) [select (select TheString, sum(LongPrimitive) from SupportBean.win:keepall() group by TheString, s0.Id) from S0 as s0]");
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean.win:keepall() group by TheString, s0.get_P00()) from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (expression 's0.get_P00()' against stream 1 is not)");
    
            // aggregations not allowed in group-by
            epl = "select (select IntPrimitive, sum(LongPrimitive) from SupportBean.win:keepall() group by sum(IntPrimitive)) from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select IntPrimitive, sum(LongPrimitive) from SupportBean.win:keepall() group by sum(IntPrimitive)) from S0 as s0]");
    
            // "prev" not allowed in group-by
            epl = "select (select IntPrimitive, sum(LongPrimitive) from SupportBean.win:keepall() group by Prev(1, IntPrimitive)) from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select IntPrimitive, sum(LongPrimitive) from SupportBean.win:keepall() group by Prev(1, IntPrimitive)) from S0 as s0]");
        }
    
        private void RunAssertionNoDelete(String fieldName, String[] fields)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
    
            SendSBEventAndTrigger("E1", 10);
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E1", 10});
    
            SendSBEventAndTrigger("E1", 20);
            AssertMapFieldAndReset(fieldName, _listener, fields, new Object[]{"E1", 30});
    
            // second group - this returns null as subquerys cannot return multiple rows (unless enumerated) (sql standard)
            SendSBEventAndTrigger("E2", 5);
            AssertMapFieldAndReset(fieldName, _listener, fields, null);
        }
    
        private void SendSBEventAndTrigger(String theString, int intPrimitive)
        {
            SendSBEventAndTrigger(theString, intPrimitive, 0);
        }
    
        private void SendSBEventAndTrigger(String theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
        }
    
        private void SendS1EventAndTrigger(int id)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S1(id, "x"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
        }
    
        private void AssertMapFieldAndReset(String fieldName, SupportUpdateListener listener, String[] names, Object[] values)
        {
            AssertMapField(fieldName, listener.AssertOneGetNew(), names, values);
            listener.Reset();
        }
    
        private void AssertMapMultiRowAndReset(String fieldName, SupportUpdateListener listener, String sortKey, String[] names, Object[][] values)
        {
            AssertMapMultiRow(fieldName, listener.AssertOneGetNew(), sortKey, names, values);
            listener.Reset();
        }
    
        private void AssertMapField(String fieldName, EventBean @event, String[] names, Object[] values)
        {
            var subq = (IDictionary<string, object>) @event.Get(fieldName);
            if (values == null && subq == null)
            {
                return;
            }
            EPAssertionUtil.AssertPropsMap(subq, names, values);
        }

        internal static void AssertMapMultiRow(String fieldName, EventBean @event, String sortKey, String[] names, Object[][] values)
        {
            var subq = @event.Get(fieldName).Unwrap<IDictionary<string, object>>();
            if (values == null && subq == null)
            {
                return;
            }
            
            var maps = subq.ToArray();
            maps.SortInPlace((o1, o2) => ((IComparable) o1.Get(sortKey)).CompareTo(o2.Get(sortKey)));

            EPAssertionUtil.AssertPropsPerRow(maps, names, values);
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }
    }
}
