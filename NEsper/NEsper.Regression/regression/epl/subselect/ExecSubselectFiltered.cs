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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectFiltered : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Sensor", typeof(SupportSensorEvent));
            configuration.AddEventType("MyEvent", typeof(SupportBean));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
            configuration.AddEventType("S3", typeof(SupportBean_S3));
            configuration.AddEventType("S4", typeof(SupportBean_S4));
            configuration.AddEventType("S5", typeof(SupportBean_S5));
            configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("ST1", typeof(SupportBean_ST1));
            epService.EPAdministrator.Configuration.AddEventType("ST2", typeof(SupportBean_ST2));
    
            RunAssertionHavingClauseNoAggregation(epService);
            RunAssertion3StreamKeyRangeCoercion(epService);
            RunAssertion2StreamRangeCoercion(epService);
            RunAssertionSameEventCompile(epService);
            RunAssertionSameEventOM(epService);
            RunAssertionSameEvent(epService);
            RunAssertionSelectWildcard(epService);
            RunAssertionSelectWildcardNoName(epService);
            RunAssertionWhereConstant(epService);
            RunAssertionWherePrevious(epService);
            RunAssertionWherePreviousOM(epService);
            RunAssertionWherePreviousCompile(epService);
            RunAssertionSelectWithWhereJoined(epService);
            RunAssertionSelectWhereJoined2Streams(epService);
            RunAssertionSelectWhereJoined3Streams(epService);
            RunAssertionSelectWhereJoined3SceneTwo(epService);
            RunAssertionSelectWhereJoined4Coercion(epService);
            RunAssertionSelectWhereJoined4BackCoercion(epService);
            RunAssertionSelectWithWhere2Subqery(epService);
            RunAssertionJoinFilteredOne(epService);
            RunAssertionJoinFilteredTwo(epService);
            RunAssertionSubselectPrior(epService);
            RunAssertionSubselectMixMax(epService);
        }
    
        private void RunAssertionHavingClauseNoAggregation(EPServiceProvider epService) {
            TryAssertionHavingNoAggNoFilterNoWhere(epService);
            TryAssertionHavingNoAggWWhere(epService);
            TryAssertionHavingNoAggWFilterWWhere(epService);
        }
    
        private void RunAssertion3StreamKeyRangeCoercion(EPServiceProvider epService) {
            string epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.key2 and IntPrimitive between s0.p01Long and s1.p11Long) " +
                    "from ST2#lastevent st2, ST0#lastevent s0, ST1#lastevent s1";
            TryAssertion3StreamKeyRangeCoercion(epService, epl, true);
    
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.key2 and s1.p11Long >= IntPrimitive and s0.p01Long <= IntPrimitive) " +
                    "from ST2#lastevent st2, ST0#lastevent s0, ST1#lastevent s1";
            TryAssertion3StreamKeyRangeCoercion(epService, epl, false);
    
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.key2 and s1.p11Long > IntPrimitive) " +
                    "from ST2#lastevent st2, ST0#lastevent s0, ST1#lastevent s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST2", "G", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", -1L));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20L));
            Assert.AreEqual(13, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            stmt.Dispose();
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.key2 and s1.p11Long < IntPrimitive) " +
                    "from ST2#lastevent st2, ST0#lastevent s0, ST1#lastevent s1";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST2", "G", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", -1L));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20L));
            Assert.AreEqual(21, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            stmt.Dispose();
        }
    
        private void TryAssertion3StreamKeyRangeCoercion(EPServiceProvider epService, string epl, bool isHasRangeReversal) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G", -1));
            epService.EPRuntime.SendEvent(new SupportBean("G", 9));
            epService.EPRuntime.SendEvent(new SupportBean("G", 21));
            epService.EPRuntime.SendEvent(new SupportBean("G", 13));
            epService.EPRuntime.SendEvent(new SupportBean("G", 17));
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", "X", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 20L));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20
    
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST22", "G", 0));
            Assert.AreEqual(30, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 0L));    // range 0 to 20
            Assert.AreEqual(39, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", null, 0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST2("ST21", "G", 0));
            Assert.AreEqual(39, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 100L));   // range 0 to 100
            Assert.AreEqual(60, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", null));   // range 0 to null
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", null));    // range null to null
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", -1L));   // range null to -1
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));    // range 10 to -1
            if (isHasRangeReversal) {
                Assert.AreEqual(8, listener.AssertOneGetNewAndReset().Get("sumi"));
            } else {
                Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertion2StreamRangeCoercion(EPServiceProvider epService) {
    
            // between and 'in' automatically revert the range (20 to 10 is the same as 10 to 20)
            string epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive between s0.p01Long and s1.p11Long) " +
                    "from ST0#lastevent s0, ST1#lastevent s1";
            TryAssertion2StreamRangeCoercion(epService, epl, true);
    
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive between s1.p11Long and s0.p01Long) " +
                    "from ST1#lastevent s1, ST0#lastevent s0";
            TryAssertion2StreamRangeCoercion(epService, epl, true);
    
            // >= and <= should not automatically revert the range
            epl = "select (" +
                    "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive >= s0.p01Long and IntPrimitive <= s1.p11Long) " +
                    "from ST0#lastevent s0, ST1#lastevent s1";
            TryAssertion2StreamRangeCoercion(epService, epl, false);
        }
    
        private void TryAssertion2StreamRangeCoercion(EPServiceProvider epService, string epl, bool isHasRangeReversal) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST01", 10L));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST11", 20L));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 21));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_1", 10L));  // range 10 to 20
            Assert.AreEqual(13, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_1", 13L));  // range 10 to 13
            Assert.AreEqual(13, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_2", 13L));  // range 13 to 13
            Assert.AreEqual(13, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 14));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_3", 13L));  // range 13 to 13
            Assert.AreEqual(13, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_4", 20L));  // range 13 to 20
            Assert.AreEqual(27, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_3", 11L));  // range 11 to 20
            Assert.AreEqual(39, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_4", null));  // range null to 16
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_5", null));  // range null to null
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_5", 20L));  // range 20 to null
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_6", 13L));  // range 20 to 13
            if (isHasRangeReversal) {
                Assert.AreEqual(27, listener.AssertOneGetNewAndReset().Get("sumi"));
            } else {
                Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("sumi"));
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionSameEventCompile(EPServiceProvider epService) {
            string stmtText = "select (select * from S1#length(1000)) as events1 from S1";
            EPStatementObjectModel subquery = epService.EPAdministrator.CompileEPL(stmtText);
            subquery = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, subquery);
    
            EPStatement stmt = epService.EPAdministrator.Create(subquery);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            var theEvent = new SupportBean_S1(-1, "Y");
            epService.EPRuntime.SendEvent(theEvent);
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSameEventOM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.CreateWildcard();
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1")
                .AddView(View.Create("length", Expressions.Constant(1000))));
    
            var model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S1"));
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "events1");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select (select * from S1#length(1000)) as events1 from S1";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            var theEvent = new SupportBean_S1(-1, "Y");
            epService.EPRuntime.SendEvent(theEvent);
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSameEvent(EPServiceProvider epService) {
            string stmtText = "select (select * from S1#length(1000)) as events1 from S1";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            var theEvent = new SupportBean_S1(-1, "Y");
            epService.EPRuntime.SendEvent(theEvent);
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWildcard(EPServiceProvider epService) {
            string stmtText = "select (select * from S1#length(1000)) as events1 from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));
    
            var theEvent = new SupportBean_S1(-1, "Y");
            epService.EPRuntime.SendEvent(theEvent);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("events1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWildcardNoName(EPServiceProvider epService) {
            string stmtText = "select (select * from S1#length(1000)) from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("subselect_1"));
    
            var theEvent = new SupportBean_S1(-1, "Y");
            epService.EPRuntime.SendEvent(theEvent);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, result.Get("subselect_1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhereConstant(EPServiceProvider epService) {
            // single-column constant
            string stmtText = "select (select id from S1#length(1000) where p10='X') as ids1 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "Y"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "Y"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "Z"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("ids1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("ids1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids1"));
            stmt.Dispose();
    
            // two-column constant
            stmtText = "select (select id from S1#length(1000) where p10='X' and p11='Y') as ids1 from S0";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X", "Y"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("ids1"));
            stmt.Dispose();
    
            // single range
            stmtText = "select (select TheString from SupportBean#lastevent where IntPrimitive between 10 and 20) as ids1 from S0";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 15));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("ids1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWherePrevious(EPServiceProvider epService) {
            string stmtText = "select (select prev(1, id) from S1#length(1000) where id=s0.id) as value from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            RunWherePrevious(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionWherePreviousOM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create().Add(Expressions.Previous(1, "id"));
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView(View.Create("length", Expressions.Constant(1000))));
            subquery.WhereClause = Expressions.EqProperty("id", "s0.id");
    
            var model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S0", "s0"));
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "value");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select (select prev(1,id) from S1#length(1000) where id=s0.id) as value from S0 as s0";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            RunWherePrevious(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionWherePreviousCompile(EPServiceProvider epService) {
            string stmtText = "select (select prev(1,id) from S1#length(1000) where id=s0.id) as value from S0 as s0";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            RunWherePrevious(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunWherePrevious(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void RunAssertionSelectWithWhereJoined(EPServiceProvider epService) {
            string stmtText = "select (select id from S1#length(1000) where p10=s0.p00) as ids1 from S0 as s0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "Y"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "Z"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "X"));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("ids1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Y"));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("ids1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Z"));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("ids1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWhereJoined2Streams(EPServiceProvider epService) {
            string stmtText = "select (select id from S0#length(1000) where p00=s1.p10 and p00=s2.p20) as ids0 from S1#keepall as s1, S2#keepall as s2 where s1.id = s2.id";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(11, "s0_1"));
            Assert.AreEqual(99, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWhereJoined3Streams(EPServiceProvider epService) {
            string stmtText = "select (select id from S0#length(1000) where p00=s1.p10 and p00=s3.p30) as ids0 " +
                    "from S1#keepall as s1, S2#keepall as s2, S3#keepall as s3 where s1.id = s2.id and s2.id = s3.id";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(10, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(11, "xxx"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(11, "s0_1"));
            Assert.AreEqual(99, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(98, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "s0_x"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(12, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(12, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(13, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(13, "s0_x"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(14, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(14, "xx"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(14, "s0_2"));
            Assert.AreEqual(98, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWhereJoined3SceneTwo(EPServiceProvider epService) {
            string stmtText = "select (select id from S0#length(1000) where p00=s1.p10 and p00=s3.p30 and p00=s2.p20) as ids0 " +
                    "from S1#keepall as s1, S2#keepall as s2, S3#keepall as s3 where s1.id = s2.id and s2.id = s3.id";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(10, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(10, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(99, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "s0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(11, "xxx"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(11, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(98, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "s0_x"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(12, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(12, "s0_1"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(13, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(13, "s0_x"));
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(14, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(14, "s0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S3(14, "s0_2"));
            Assert.AreEqual(98, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectWhereJoined4Coercion(EPServiceProvider epService) {
            string stmtText = "select " +
                    "(select IntPrimitive from MyEvent(TheString='S')#length(1000) " +
                    "  where IntBoxed=s1.LongBoxed and " +
                    "IntBoxed=s2.DoubleBoxed and " +
                    "DoubleBoxed=s3.IntBoxed" +
                    ") as ids0 from " +
                    "MyEvent(TheString='A')#keepall as s1, " +
                    "MyEvent(TheString='B')#keepall as s2, " +
                    "MyEvent(TheString='C')#keepall as s3 " +
                    "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(epService, stmtText);
    
            stmtText = "select " +
                    "(select IntPrimitive from MyEvent(TheString='S')#length(1000) " +
                    "  where DoubleBoxed=s3.IntBoxed and " +
                    "IntBoxed=s2.DoubleBoxed and " +
                    "IntBoxed=s1.LongBoxed" +
                    ") as ids0 from " +
                    "MyEvent(TheString='A')#keepall as s1, " +
                    "MyEvent(TheString='B')#keepall as s2, " +
                    "MyEvent(TheString='C')#keepall as s3 " +
                    "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(epService, stmtText);
    
            stmtText = "select " +
                    "(select IntPrimitive from MyEvent(TheString='S')#length(1000) " +
                    "  where DoubleBoxed=s3.IntBoxed and " +
                    "IntBoxed=s1.LongBoxed and " +
                    "IntBoxed=s2.DoubleBoxed" +
                    ") as ids0 from " +
                    "MyEvent(TheString='A')#keepall as s1, " +
                    "MyEvent(TheString='B')#keepall as s2, " +
                    "MyEvent(TheString='C')#keepall as s3 " +
                    "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4Coercion(epService, stmtText);
        }
    
        private void RunAssertionSelectWhereJoined4BackCoercion(EPServiceProvider epService) {
            string stmtText = "select " +
                    "(select IntPrimitive from MyEvent(TheString='S')#length(1000) " +
                    "  where LongBoxed=s1.IntBoxed and " +
                    "LongBoxed=s2.DoubleBoxed and " +
                    "IntBoxed=s3.LongBoxed" +
                    ") as ids0 from " +
                    "MyEvent(TheString='A')#keepall as s1, " +
                    "MyEvent(TheString='B')#keepall as s2, " +
                    "MyEvent(TheString='C')#keepall as s3 " +
                    "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4CoercionBack(epService, stmtText);
    
            stmtText = "select " +
                    "(select IntPrimitive from MyEvent(TheString='S')#length(1000) " +
                    "  where LongBoxed=s2.DoubleBoxed and " +
                    "IntBoxed=s3.LongBoxed and " +
                    "LongBoxed=s1.IntBoxed " +
                    ") as ids0 from " +
                    "MyEvent(TheString='A')#keepall as s1, " +
                    "MyEvent(TheString='B')#keepall as s2, " +
                    "MyEvent(TheString='C')#keepall as s3 " +
                    "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
            TrySelectWhereJoined4CoercionBack(epService, stmtText);
        }
    
        private void TrySelectWhereJoined4CoercionBack(EPServiceProvider epService, string stmtText) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "A", 1, 10, 200, 3000);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 1, 10, 200, 3000);
            SendBean(epService, "C", 1, 10, 200, 3000);
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -1, 11, 201, 0);     // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "A", 2, 201, 0, 0);
            SendBean(epService, "B", 2, 0, 0, 201);
            SendBean(epService, "C", 2, 0, 11, 0);
            Assert.AreEqual(-1, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -2, 12, 202, 0);     // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "A", 3, 202, 0, 0);
            SendBean(epService, "B", 3, 0, 0, 202);
            SendBean(epService, "C", 3, 0, -1, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -3, 13, 203, 0);     // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "A", 4, 203, 0, 0);
            SendBean(epService, "B", 4, 0, 0, 203.0001);
            SendBean(epService, "C", 4, 0, 13, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -4, 14, 204, 0);     // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "A", 5, 205, 0, 0);
            SendBean(epService, "B", 5, 0, 0, 204);
            SendBean(epService, "C", 5, 0, 14, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Stop();
        }
    
        private void TrySelectWhereJoined4Coercion(EPServiceProvider epService, string stmtText) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "A", 1, 10, 200, 3000);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 1, 10, 200, 3000);
            SendBean(epService, "C", 1, 10, 200, 3000);
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -2, 11, 0, 3001);
            SendBean(epService, "A", 2, 0, 11, 0);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 2, 0, 0, 11);
            SendBean(epService, "C", 2, 3001, 0, 0);
            Assert.AreEqual(-2, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -3, 12, 0, 3002);
            SendBean(epService, "A", 3, 0, 12, 0);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 3, 0, 0, 12);
            SendBean(epService, "C", 3, 3003, 0, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -4, 11, 0, 3003);
            SendBean(epService, "A", 4, 0, 0, 0);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 4, 0, 0, 11);
            SendBean(epService, "C", 4, 3003, 0, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            SendBean(epService, "S", -5, 14, 0, 3004);
            SendBean(epService, "A", 5, 0, 14, 0);        // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(epService, "B", 5, 0, 0, 11);
            SendBean(epService, "C", 5, 3004, 0, 0);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("ids0"));
    
            stmt.Stop();
        }
    
        private void RunAssertionSelectWithWhere2Subqery(EPServiceProvider epService) {
            string stmtText = "select id from S0 as s0 where " +
                    " id = (select id from S1#length(1000) where s0.id = id) or id = (select id from S2#length(1000) where s0.id = id)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinFilteredOne(EPServiceProvider epService) {
            string stmtText = "select s0.id as s0id, s1.id as s1id, " +
                    "(select p20 from S2#length(1000) where id=s0.id) as s2p20, " +
                    "(select prior(1, p20) from S2#length(1000) where id=s0.id) as s2p20Prior, " +
                    "(select prev(1, p20) from S2#length(10) where id=s0.id) as s2p20Prev " +
                    "from S0#keepall as s0, S1#keepall as s1 " +
                    "where s0.id = s1.id and p00||p10 = (select p20 from S2#length(1000) where id=s0.id)";
            TryJoinFiltered(epService, stmtText);
        }
    
        private void RunAssertionJoinFilteredTwo(EPServiceProvider epService) {
            string stmtText = "select s0.id as s0id, s1.id as s1id, " +
                    "(select p20 from S2#length(1000) where id=s0.id) as s2p20, " +
                    "(select prior(1, p20) from S2#length(1000) where id=s0.id) as s2p20Prior, " +
                    "(select prev(1, p20) from S2#length(10) where id=s0.id) as s2p20Prev " +
                    "from S0#keepall as s0, S1#keepall as s1 " +
                    "where s0.id = s1.id and (select s0.p00||s1.p10 = p20 from S2#length(1000) where id=s0.id)";
            TryJoinFiltered(epService, stmtText);
        }
    
        private void RunAssertionSubselectPrior(EPServiceProvider epService) {
            string stmtTextOne = "insert into Pair " +
                    "select * from Sensor(device='A')#lastevent as a, Sensor(device='B')#lastevent as b " +
                    "where a.type = b.type";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            epService.EPAdministrator.CreateEPL("insert into PairDuplicatesRemoved select * from Pair(1=2)");
    
            string stmtTextTwo = "insert into PairDuplicatesRemoved " +
                    "select * from Pair " +
                    "where a.id != coalesce((select a.id from PairDuplicatesRemoved#lastevent), -1)" +
                    "  and b.id != coalesce((select b.id from PairDuplicatesRemoved#lastevent), -1)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(1, "Temperature", "A", 51, 94.5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(2, "Temperature", "A", 57, 95.5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(3, "Humidity", "B", 29, 67.5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(4, "Temperature", "B", 55, 88.0));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, theEvent.Get("a.id"));
            Assert.AreEqual(4, theEvent.Get("b.id"));
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(5, "Temperature", "B", 65, 85.0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(6, "Temperature", "B", 49, 87.0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(7, "Temperature", "A", 51, 99.5));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(7, theEvent.Get("a.id"));
            Assert.AreEqual(6, theEvent.Get("b.id"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubselectMixMax(EPServiceProvider epService) {
            string stmtTextOne =
                    "select " +
                            " (select * from Sensor#sort(1, measurement desc)) as high, " +
                            " (select * from Sensor#sort(1, measurement asc)) as low " +
                            " from Sensor";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextOne);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(1, "Temp", "Dev1", 68.0, 96.5));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(2, "Temp", "Dev2", 70.0, 98.5));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
    
            epService.EPRuntime.SendEvent(new SupportSensorEvent(3, "Temp", "Dev2", 65.0, 99.5));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
            Assert.AreEqual(65.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);
    
            stmt.Dispose();
        }
    
        private void TryJoinFiltered(EPServiceProvider epService, string stmtText) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "Y"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(1, "ab"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "b"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, theEvent.Get("s0id"));
            Assert.AreEqual(1, theEvent.Get("s1id"));
            Assert.AreEqual("ab", theEvent.Get("s2p20"));
            Assert.AreEqual(null, theEvent.Get("s2p20Prior"));
            Assert.AreEqual(null, theEvent.Get("s2p20Prev"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(2, "qx"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "q"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "x"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, theEvent.Get("s0id"));
            Assert.AreEqual(2, theEvent.Get("s1id"));
            Assert.AreEqual("qx", theEvent.Get("s2p20"));
            Assert.AreEqual("ab", theEvent.Get("s2p20Prior"));
            Assert.AreEqual("ab", theEvent.Get("s2p20Prev"));
    
            stmt.Dispose();
        }
    
        private void TryAssertionHavingNoAggWFilterWWhere(EPServiceProvider epService) {
            string epl = "select (select IntPrimitive from SupportBean(IntPrimitive < 20) #keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendS0AndAssert(epService, listener, null);
            SendSBAndS0Assert(epService, listener, "ID2", 10, null);
            SendSBAndS0Assert(epService, listener, "ID1", 11, null);
            SendSBAndS0Assert(epService, listener, "ID1", 20, null);
            SendSBAndS0Assert(epService, listener, "ID1", 19, 19);
    
            stmt.Dispose();
        }
    
        private void TryAssertionHavingNoAggWWhere(EPServiceProvider epService) {
            string epl = "select (select IntPrimitive from SupportBean#keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendS0AndAssert(epService, listener, null);
            SendSBAndS0Assert(epService, listener, "ID2", 10, null);
            SendSBAndS0Assert(epService, listener, "ID1", 11, null);
            SendSBAndS0Assert(epService, listener, "ID1", 20, 20);
    
            stmt.Dispose();
        }
    
        private void TryAssertionHavingNoAggNoFilterNoWhere(EPServiceProvider epService) {
            string epl = "select (select IntPrimitive from SupportBean#keepall having TheString = 'ID1') as c0 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendS0AndAssert(epService, listener, null);
            SendSBAndS0Assert(epService, listener, "ID2", 10, null);
            SendSBAndS0Assert(epService, listener, "ID1", 11, 11);
    
            stmt.Dispose();
        }
    
        private void SendBean(EPServiceProvider epService, string theString, int intPrimitive, int intBoxed, long longBoxed, double doubleBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSBAndS0Assert(EPServiceProvider epService, SupportUpdateListener listener, string theString, int intPrimitive, int? expected) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
            SendS0AndAssert(epService, listener, expected);
        }
    
        private void SendS0AndAssert(EPServiceProvider epService, SupportUpdateListener listener, int? expected) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    }
} // end of namespace
