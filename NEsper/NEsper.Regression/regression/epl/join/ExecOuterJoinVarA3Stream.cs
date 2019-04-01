///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.type;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterJoinVarA3Stream : RegressionExecution {
        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("P1", typeof(SupportBean_S1));
            configuration.AddEventType("P2", typeof(SupportBean_S2));
            configuration.AddEventType("P3", typeof(SupportBean_S3));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMapLeftJoinUnsortedProps(epService);
            RunAssertionLeftJoin_2sides_multicolumn(epService);
            RunAssertionLeftOuterJoin_root_s0_OM(epService);
            RunAssertionLeftOuterJoin_root_s0_Compiled(epService);
            RunAssertionLeftOuterJoin_root_s0(epService);
            RunAssertionRightOuterJoin_S2_root_s2(epService);
            RunAssertionRightOuterJoin_S1_root_s1(epService);
            RunAssertionInvalidMulticolumn(epService);
        }
    
        private void RunAssertionMapLeftJoinUnsortedProps(EPServiceProvider epService) {
            string stmtText = "select t1.col1, t1.col2, t2.col1, t2.col2, t3.col1, t3.col2 from type1#keepall as t1" +
                    " left outer join type2#keepall as t2" +
                    " on t1.col2 = t2.col2 and t1.col1 = t2.col1" +
                    " left outer join type3#keepall as t3" +
                    " on t1.col1 = t3.col1";
    
            var mapType = new Dictionary<string, object>();
            mapType.Put("col1", typeof(string));
            mapType.Put("col2", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("type1", mapType);
            epService.EPAdministrator.Configuration.AddEventType("type2", mapType);
            epService.EPAdministrator.Configuration.AddEventType("type3", mapType);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"t1.col1", "t1.col2", "t2.col1", "t2.col2", "t3.col1", "t3.col2"};
    
            SendMapEvent(epService, "type2", "a1", "b1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendMapEvent(epService, "type1", "b1", "a1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"b1", "a1", null, null, null, null});
    
            SendMapEvent(epService, "type1", "a1", "a1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"a1", "a1", null, null, null, null});
    
            SendMapEvent(epService, "type1", "b1", "b1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"b1", "b1", null, null, null, null});
    
            SendMapEvent(epService, "type1", "a1", "b1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"a1", "b1", "a1", "b1", null, null});
    
            SendMapEvent(epService, "type3", "c1", "b1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendMapEvent(epService, "type1", "d1", "b1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"d1", "b1", null, null, null, null});
    
            SendMapEvent(epService, "type3", "d1", "bx");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"d1", "b1", null, null, "d1", "bx"});
    
            Assert.IsFalse(listener.IsInvoked);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLeftJoin_2sides_multicolumn(EPServiceProvider epService) {
            string[] fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".Split(',');
    
            string epl = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
                    " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 and s0.p01 = s2.p21";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "A_1", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A_2", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A_1", "B_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "A_2", "B_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(20, "A_1", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(21, "A_2", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(22, "A_1", "B_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(23, "A_2", "B_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_3", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A_3", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "A_1", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "A_3", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "A_3", "B_1", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionLeftOuterJoin_root_s0_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            FromClause fromClause = FromClause.Create(
                    FilterStream.Create(EVENT_S0, "s0").AddView("keepall"),
                    FilterStream.Create(EVENT_S1, "s1").AddView("keepall"),
                    FilterStream.Create(EVENT_S2, "s2").AddView("keepall"));
            fromClause.Add(OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s1.p10"));
            fromClause.Add(OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s2.p20"));
            model.FromClause = fromClause;
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual("select * from " + typeof(SupportBean_S0).FullName + "#keepall as s0 left outer join " + typeof(SupportBean_S1).FullName + "#keepall as s1 on s0.p00 = s1.p10 left outer join " + typeof(SupportBean_S2).FullName + "#keepall as s2 on s0.p00 = s2.p20", model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener);
        }
    
        private void RunAssertionLeftOuterJoin_root_s0_Compiled(EPServiceProvider epService) {
            string epl = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    "left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
                    "left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(epl, model.ToEPL());
    
            TryAssertion(epService, listener);
        }
    
        private void RunAssertionLeftOuterJoin_root_s0(EPServiceProvider epService) {
            /// <summary>
            /// Query:
            /// s0
            /// s1 <-      -> s2
            /// </summary>
            string epl = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener);
        }
    
        private void RunAssertionRightOuterJoin_S2_root_s2(EPServiceProvider epService) {
            /// <summary>
            /// Query: right other join is eliminated/translated
            /// s0
            /// s1 <-      -> s2
            /// </summary>
            string epl = "select * from " +
                    EVENT_S2 + "#length(1000) as s2 " +
                    " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
                    " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener);
        }
    
        private void RunAssertionRightOuterJoin_S1_root_s1(EPServiceProvider epService) {
            /// <summary>
            /// Query: right other join is eliminated/translated
            /// s0
            /// s1 <-      -> s2
            /// </summary>
            string epl = "select * from " +
                    EVENT_S1 + "#length(1000) as s1 " +
                    " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener);
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener) {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            object[] s1Events = SupportBean_S1.MakeS1("A", new string[]{"A-s1-1", "A-s1-2"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            object[] s2Events = SupportBean_S2.MakeS2("A", new string[]{"A-s2-1", "A-s2-2"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            object[] s0Events = SupportBean_S0.MakeS0("A", new string[]{"A-s0-1"});
            SendEvent(epService, s0Events);
            var expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new object[] {s0Events[0], s1Events[1], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new string[]{"B-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s0Events = SupportBean_S0.MakeS0("B", new string[]{"B-s0-2"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[]{"C-s1-1"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("C", new string[]{"C-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[]{"D-s1-1", "D-s1-2"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("D", new string[]{"D-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{
                    new object[] {s0Events[0], s1Events[0], null},
                    new object[] {s0Events[0], s1Events[1], null}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new string[]{"E-s2-1"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("E", new string[]{"E-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new string[]{"F-s2-1", "F-s2-2"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("F", new string[]{"F-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{
                    new object[] {s0Events[0], null, s2Events[0]},
                    new object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[]{"G-s1-1"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("G", new string[]{"G-s2-1", "G-s2-2"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("G", new string[]{"G-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[]{"H-s1-1", "H-s1-2"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("H", new string[]{"H-s2-1"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("H", new string[]{"H-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new string[]{"I-s1-1"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("I", new string[]{"I-s2-1"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("I", new string[]{"I-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new string[]{"Q-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s2Events = SupportBean_S2.MakeS2("Q", new string[]{"Q-s2-1", "Q-s2-2"});
            SendEvent(epService, s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
            SendEvent(epService, s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents(listener));
    
            s1Events = SupportBean_S1.MakeS1("Q", new string[]{"Q-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new string[]{"R-s2-1", "R-s2-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("R", new string[]{"R-s1-1"});
            SendEvent(epService, s1Events);
            Assert.IsFalse(listener.IsInvoked);
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[]{"S-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s1Events = SupportBean_S1.MakeS1("S", new string[]{"S-s1-1"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new string[]{"T-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s2Events = SupportBean_S2.MakeS2("T", new string[]{"T-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("T", new string[]{"T-s1-1"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[]{"U-s0-1", "U-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new string[]{"U-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], null},
                    new object[] {s0Events[1], s1Events[0], null},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[]{"V-s0-1", "V-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new string[]{"V-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new string[]{"V-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[]{"W-s0-1", "W-s0-2"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new string[]{"W-s2-1", "W-s2-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new string[]{"W-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new object[] {s0Events[1], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new string[]{"J-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("J", new string[]{"J-s1-1", "J-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new string[]{"J-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new string[]{"K-s1-1", "K-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("K", new string[]{"K-s2-1"});
            SendEvent(epService, s2Events);
            Assert.IsFalse(listener.IsInvoked);
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new string[]{"L-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("L", new string[]{"L-s2-1"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[]{"M-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new string[]{"M-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new string[]{"M-s2-1"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[]{"N-s0-1", "N-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("N", new string[]{"N-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], null, s2Events[0]},
                    new object[] {s0Events[1], null, s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new string[]{"O-s0-1", "O-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new string[]{"O-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("O", new string[]{"O-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new string[]{"P-s0-1", "P-s0-2"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new string[]{"P-s1-1", "P-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new string[]{"P-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new object[] {s0Events[1], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidMulticolumn(EPServiceProvider epService) {
            try {
                string epl = "select * from " +
                        EVENT_S0 + "#length(1000) as s0 " +
                        " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
                        " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 and s1.p11 = s2.p21";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause");
            }
    
            try {
                string epl = "select * from " +
                        EVENT_S0 + "#length(1000) as s0 " +
                        " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
                        " left outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s0.p00 and s2.p20 = s1.p11";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [");
            }
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventsAndReset(EPServiceProvider epService, SupportUpdateListener listener, object[] events) {
            SendEvent(epService, events);
            listener.Reset();
        }
    
        private void SendEvent(EPServiceProvider epService, object[] events) {
            for (int i = 0; i < events.Length; i++) {
                epService.EPRuntime.SendEvent(events[i]);
            }
        }
    
        private void SendMapEvent(EPServiceProvider epService, string type, string col1, string col2) {
            var mapEvent = new Dictionary<string, Object>();
            mapEvent.Put("col1", col1);
            mapEvent.Put("col2", col2);
            epService.EPRuntime.SendEvent(mapEvent, type);
        }
    
        private object[][] GetAndResetNewEvents(SupportUpdateListener listener) {
            EventBean[] newEvents = listener.LastNewData;
            listener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new string[]{"s0", "s1", "s2"});
        }
    }
} // end of namespace
