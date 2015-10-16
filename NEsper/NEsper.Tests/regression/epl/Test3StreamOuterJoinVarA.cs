///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.type;
using com.espertech.esper.util;

using NUnit.Framework;



namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test3StreamOuterJoinVarA  {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("P1", typeof(SupportBean_S1));
            config.AddEventType("P2", typeof(SupportBean_S2));
            config.AddEventType("P3", typeof(SupportBean_S3));
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            updateListener = null;
        }
    
        [Test]
        public void TestMapLeftJoinUnsortedProps() {
            String stmtText = "select t1.col1, t1.col2, t2.col1, t2.col2, t3.col1, t3.col2 from type1.win:keepall() as t1" +
                    " left outer join type2.win:keepall() as t2" +
                    " on t1.col2 = t2.col2 and t1.col1 = t2.col1" +
                    " left outer join type3.win:keepall() as t3" +
                    " on t1.col1 = t3.col1";
    
            IDictionary<String, Object> mapType = new Dictionary<String, Object>();
            mapType["col1"] = typeof(string);
            mapType["col2"] = typeof(string);
            epService.EPAdministrator.Configuration.AddEventType("type1", mapType);
            epService.EPAdministrator.Configuration.AddEventType("type2", mapType);
            epService.EPAdministrator.Configuration.AddEventType("type3", mapType);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += updateListener.Update;
    
            String[] fields = new String[]{"t1.col1", "t1.col2", "t2.col1", "t2.col2", "t3.col1", "t3.col2"};
    
            SendMapEvent("type2", "a1", "b1");
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendMapEvent("type1", "b1", "a1");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"b1", "a1", null, null, null, null});
    
            SendMapEvent("type1", "a1", "a1");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"a1", "a1", null, null, null, null});
    
            SendMapEvent("type1", "b1", "b1");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"b1", "b1", null, null, null, null});
    
            SendMapEvent("type1", "a1", "b1");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"a1", "b1", "a1", "b1", null, null});
    
            SendMapEvent("type3", "c1", "b1");
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendMapEvent("type1", "d1", "b1");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"d1", "b1", null, null, null, null});
    
            SendMapEvent("type3", "d1", "bx");
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{"d1", "b1", null, null, "d1", "bx"});
    
            Assert.IsFalse(updateListener.IsInvoked);
        }
    
        [Test]
        public void TestLeftJoin_2sides_multicolumn() {
            String[] fields = "s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".Split(',');
    
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11" +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 and s0.p01 = s2.p21";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "A_1", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A_2", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A_1", "B_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "A_2", "B_2"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(20, "A_1", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(21, "A_2", "B_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(22, "A_1", "B_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(23, "A_2", "B_2"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_3", "B_3"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{1, "A_3", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{2, "A_1", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "A_3", "B_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{3, "A_3", "B_1", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[]{5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});
        }
    
        [Test]
        public void TestLeftOuterJoin_root_s0_OM() {
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            FromClause fromClause = FromClause.Create(
                    FilterStream.Create(EVENT_S0, "s0").AddView("win", "keepall"),
                    FilterStream.Create(EVENT_S1, "s1").AddView("win", "keepall"),
                    FilterStream.Create(EVENT_S2, "s2").AddView("win", "keepall"));
            fromClause.Add(OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s1.p10"));
            fromClause.Add(OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s2.p20"));
            model.FromClause = fromClause;
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean_S0.win:keepall() as s0 left outer join com.espertech.esper.support.bean.SupportBean_S1.win:keepall() as s1 on s0.P00 = s1.p10 left outer join com.espertech.esper.support.bean.SupportBean_S2.win:keepall() as s2 on s0.P00 = s2.p20", model.ToEPL());
            EPStatement joinView = epService.EPAdministrator.Create(model);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestLeftOuterJoin_root_s0_Compiled() {
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    "left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    "left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(joinStatement);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            EPStatement joinView = epService.EPAdministrator.Create(model);
            joinView.Events += updateListener.Update;
    
            Assert.AreEqual(joinStatement, model.ToEPL());
    
            RunAsserts();
        }
    
        [Test]
        public void TestLeftOuterJoin_root_s0() {
            /// <summary>Query: s0 s1 &lt;-      -&gt; s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRightOuterJoin_S2_root_s2() {
            /// <summary>Query: right other join is eliminated/translated s0 s1 &lt;-      -&gt; s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s2.p20 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRightOuterJoin_S1_root_s1() {
            /// <summary>Query: right other join is eliminated/translated s0 s1 &lt;-      -&gt; s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:length(1000) as s1 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        private void RunAsserts() {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            Object[] s1Events = SupportBean_S1.MakeS1("A", new String[]{"A-s1-1", "A-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            Object[] s2Events = SupportBean_S2.MakeS2("A", new String[]{"A-s2-1", "A-s2-2"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            Object[] s0Events = SupportBean_S0.MakeS0("A", new String[]{"A-s0-1"});
            SendEvent(s0Events);
            Object[][] expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new String[]{"B-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s0Events = SupportBean_S0.MakeS0("B", new String[]{"B-s0-2"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new String[]{"C-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("C", new String[]{"C-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new String[]{"D-s1-1", "D-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("D", new String[]{"D-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], null},
                    new Object[] {s0Events[0], s1Events[1], null}
            }, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new String[]{"E-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("E", new String[]{"E-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new String[]{"F-s2-1", "F-s2-2"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("F", new String[]{"F-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[0], null, s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new String[]{"G-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("G", new String[]{"G-s2-1", "G-s2-2"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("G", new String[]{"G-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new String[]{"H-s1-1", "H-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("H", new String[]{"H-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("H", new String[]{"H-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new String[]{"I-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("I", new String[]{"I-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("I", new String[]{"I-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new String[]{"Q-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s2Events = SupportBean_S2.MakeS2("Q", new String[]{"Q-s2-1", "Q-s2-2"});
            SendEvent(s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
            SendEvent(s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents());
    
            s1Events = SupportBean_S1.MakeS1("Q", new String[]{"Q-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new String[]{"R-s2-1", "R-s2-2"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("R", new String[]{"R-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new String[]{"S-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s1Events = SupportBean_S1.MakeS1("S", new String[]{"S-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new String[]{"T-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s2Events = SupportBean_S2.MakeS2("T", new String[]{"T-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("T", new String[]{"T-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new String[]{"U-s0-1", "U-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new String[]{"U-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], null},
                    new Object[] {s0Events[1], s1Events[0], null},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new String[]{"V-s0-1", "V-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new String[]{"V-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new String[]{"V-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new String[]{"W-s0-1", "W-s0-2"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new String[]{"W-s2-1", "W-s2-2"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new String[]{"W-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new String[]{"J-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("J", new String[]{"J-s1-1", "J-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new String[]{"J-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new String[]{"K-s1-1", "K-s1-2"});
            SendEventsAndReset(s2Events);
    
            s2Events = SupportBean_S2.MakeS2("K", new String[]{"K-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new String[]{"L-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("L", new String[]{"L-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new String[]{"M-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new String[]{"M-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new String[]{"M-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("Count", new String[]{"Count-s0-1", "Count-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("Count", new String[]{"Count-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[1], null, s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new String[]{"O-s0-1", "O-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new String[]{"O-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("O", new String[]{"O-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new String[]{"P-s0-1", "P-s0-2"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new String[]{"P-s1-1", "P-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new String[]{"P-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
        }
    
        [Test]
        public void TestInvalidMulticolumn() {
            try {
                String joinStatement = "select * from " +
                        EVENT_S0 + ".win:length(1000) as s0 " +
                        " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11" +
                        " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 and s1.p11 = s2.p21";
                epService.EPAdministrator.CreateEPL(joinStatement);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [select * from com.espertech.esper.support.bean.SupportBean_S0.win:length(1000) as s0  left outer join com.espertech.esper.support.bean.SupportBean_S1.win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11 left outer join com.espertech.esper.support.bean.SupportBean_S2.win:length(1000) as s2 on s0.P00 = s2.p20 and s1.p11 = s2.p21]", ex.Message);
            }
    
            try {
                String joinStatement = "select * from " +
                        EVENT_S0 + ".win:length(1000) as s0 " +
                        " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11" +
                        " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s2.p20 = s0.P00 and s2.p20 = s1.p11";
                epService.EPAdministrator.CreateEPL(joinStatement);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [select * from com.espertech.esper.support.bean.SupportBean_S0.win:length(1000) as s0  left outer join com.espertech.esper.support.bean.SupportBean_S1.win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11 left outer join com.espertech.esper.support.bean.SupportBean_S2.win:length(1000) as s2 on s2.p20 = s0.P00 and s2.p20 = s1.p11]", ex.Message);
            }
        }
    
        private void SendEvent(Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventsAndReset(Object[] events) {
            SendEvent(events);
            updateListener.Reset();
        }
    
        private void SendEvent(Object[] events) {
            for (int i = 0; i < events.Length; i++) {
                epService.EPRuntime.SendEvent(events[i]);
            }
        }
    
        private void SendMapEvent(String type, String col1, String col2) {
            IDictionary<String, Object> mapEvent = new Dictionary<String, Object>();
            mapEvent["col1"] = col1;
            mapEvent["col2"] = col2;
            epService.EPRuntime.SendEvent(mapEvent, type);
        }
    
        private Object[][] GetAndResetNewEvents() {
            EventBean[] newEvents = updateListener.LastNewData;
            updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new String[]{"s0", "s1", "s2"});
        }
    }
}
