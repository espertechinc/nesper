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
using com.espertech.esper.type;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterJoin2Stream : RegressionExecution {
        private static readonly string[] FIELDS = new string[]{"s0.id", "s0.p00", "s1.id", "s1.p10"};
    
        private static readonly SupportBean_S0[] EVENTS_S0;
        private static readonly SupportBean_S1[] EVENTS_S1;
    
        static ExecOuterJoin2Stream() {
            EVENTS_S0 = new SupportBean_S0[15];
            EVENTS_S1 = new SupportBean_S1[15];
            int count = 100;
            for (int i = 0; i < EVENTS_S0.Length; i++) {
                EVENTS_S0[i] = new SupportBean_S0(count++, Convert.ToString(i));
            }
            count = 200;
            for (int i = 0; i < EVENTS_S1.Length; i++) {
                EVENTS_S1[i] = new SupportBean_S1(count++, Convert.ToString(i));
            }
        }
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanRange));
    
            RunAssertionRangeOuterJoin(epService);
            RunAssertionFullOuterIteratorGroupBy(epService);
            RunAssertionFullOuterJoin(epService);
            RunAssertionMultiColumnLeft_OM(epService);
            RunAssertionMultiColumnLeft(epService);
            RunAssertionMultiColumnRight(epService);
            RunAssertionMultiColumnRightCoercion(epService);
            RunAssertionRightOuterJoin(epService);
            RunAssertionLeftOuterJoin(epService);
            RunAssertionEventType(epService);
        }
    
        private void RunAssertionRangeOuterJoin(EPServiceProvider epService) {
    
            string stmtOne = "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBean#keepall sb " +
                    "full outer join " +
                    "SupportBeanRange#keepall sbr " +
                    "on TheString = key " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
            TryAssertion(epService, stmtOne);
    
            string stmtTwo = "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on TheString = key " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
            TryAssertion(epService, stmtTwo);
    
            string stmtThree = "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on TheString = key " +
                    "where IntPrimitive >= rangeStart and IntPrimitive <= rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
            TryAssertion(epService, stmtThree);
        }
    
        private void TryAssertion(EPServiceProvider epService, string epl) {
    
            string[] fields = "sbstr,sbint,sbrk,sbrs,sbre".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("K1", 10));
            epService.EPRuntime.SendEvent(new SupportBeanRange("R1", "K1", 20, 30));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("K1", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"K1", 30, "K1", 20, 30}});
    
            epService.EPRuntime.SendEvent(new SupportBean("K1", 40));
            epService.EPRuntime.SendEvent(new SupportBean("K1", 31));
            epService.EPRuntime.SendEvent(new SupportBean("K1", 19));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 39, 41));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"K1", 40, "K1", 39, 41}});
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 38, 40));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"K1", 40, "K1", 38, 40}});
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 40, 42));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"K1", 40, "K1", 40, 42}});
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 41, 42));
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 38, 39));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("K1", 41));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{
                new object[]{"K1", 41, "K1", 39, 41}, new object[]{"K1", 41, "K1", 40, 42}, new object[]{"K1", 41, "K1", 41, 42}});
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 35, 42));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]
            {
                new object[]{"K1", 40, "K1", 35, 42}, new object[]{"K1", 41, "K1", 35, 42}
            });
    
            stmt.Dispose();
        }
    
        private void RunAssertionFullOuterIteratorGroupBy(EPServiceProvider epService) {
            string epl = "select TheString, IntPrimitive, symbol, volume " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#keepall " +
                    "full outer join " +
                    typeof(SupportBean).FullName + "#groupwin(TheString, IntPrimitive)#length(2) " +
                    "on TheString = symbol " +
                    "group by TheString, IntPrimitive, symbol " +
                    "order by TheString, IntPrimitive, symbol, volume";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventMD(epService, "c0", 200L);
            SendEventMD(epService, "c3", 400L);
    
            SendEvent(epService, "c0", 0);
            SendEvent(epService, "c0", 1);
            SendEvent(epService, "c0", 2);
            SendEvent(epService, "c1", 0);
            SendEvent(epService, "c1", 1);
            SendEvent(epService, "c1", 2);
            SendEvent(epService, "c2", 0);
            SendEvent(epService, "c2", 1);
            SendEvent(epService, "c2", 2);
    
            var enumerator = stmt.GetSafeEnumerator();
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(enumerator);
            Assert.AreEqual(10, events.Length);
    
            /* For debugging, comment in
            for (int i = 0; i < events.Length; i++)
            {
                Log.Info(
                       "string=" + events[i].Get("string") +
                       "  int=" + events[i].Get("IntPrimitive") +
                       "  symbol=" + events[i].Get("symbol") +
                       "  volume="  + events[i].Get("volume")
                    );
            }
            */
    
            EPAssertionUtil.AssertPropsPerRow(events, "TheString,IntPrimitive,symbol,volume".Split(','),
                    new object[][]{
                            new object[] {null, null, "c3", 400L},
                            new object[] {"c0", 0, "c0", 200L},
                            new object[] {"c0", 1, "c0", 200L},
                            new object[] {"c0", 2, "c0", 200L},
                            new object[] {"c1", 0, null, null},
                            new object[] {"c1", 1, null, null},
                            new object[] {"c1", 2, null, null},
                            new object[] {"c2", 0, null, null},
                            new object[] {"c2", 1, null, null},
                            new object[] {"c2", 2, null, null}
                    });
    
            stmt.Dispose();
        }
    
        private void RunAssertionFullOuterJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = SetupStatement(epService, listener, "full");
    
            // Send S0[0]
            SendEvent(EVENTS_S0[0], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), 100, "0", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null}});
    
            // Send S1[1]
            SendEvent(EVENTS_S1[1], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 201, "1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null},
                            new object[] {null, null, 201, "1"}});
    
            // Send S1[2] and S0[2]
            SendEvent(EVENTS_S1[2], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null},
                            new object[] {null, null, 201, "1"},
                            new object[] {null, null, 202, "2"}});
    
            SendEvent(EVENTS_S0[2], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null},
                            new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"}});
    
            // Send S0[3] and S1[3]
            SendEvent(EVENTS_S0[3], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), 103, "3", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null},
                            new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", null, null}});
            SendEvent(EVENTS_S1[3], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), 103, "3", 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {100, "0", null, null},
                            new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"}});
    
            // Send S0[4], pushes S0[0] out of window
            SendEvent(EVENTS_S0[4], epService);
            EventBean oldEvent = listener.LastOldData[0];
            EventBean newEvent = listener.LastNewData[0];
            CompareEvent(oldEvent, 100, "0", null, null);
            CompareEvent(newEvent, 104, "4", null, null);
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"},
                            new object[] {104, "4", null, null}});
    
            // Send S1[4]
            SendEvent(EVENTS_S1[4], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), 104, "4", 204, "4");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"},
                            new object[] {104, "4", 204, "4"}});
    
            // Send S1[5]
            SendEvent(EVENTS_S1[5], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 201, "1"},
                            new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"},
                            new object[] {104, "4", 204, "4"},
                            new object[] {null, null, 205, "5"}});
    
            // Send S1[6], pushes S1[1] out of window
            SendEvent(EVENTS_S1[5], epService);
            oldEvent = listener.LastOldData[0];
            newEvent = listener.LastNewData[0];
            CompareEvent(oldEvent, null, null, 201, "1");
            CompareEvent(newEvent, null, null, 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"},
                            new object[] {104, "4", 204, "4"},
                            new object[] {null, null, 205, "5"},
                            new object[] {null, null, 205, "5"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiColumnLeft_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".Split(','));
            FromClause fromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBean_S0).FullName, "s0").AddView("keepall"),
                    FilterStream.Create(typeof(SupportBean_S1).FullName, "s1").AddView("keepall"));
            fromClause.Add(OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s1.p10").Add("s1.p11", "s0.p01"));
            model.FromClause = fromClause;
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from " + typeof(SupportBean_S0).FullName + "#keepall as s0 left outer join " + typeof(SupportBean_S1).FullName + "#keepall as s1 on s0.p00 = s1.p10 and s1.p11 = s0.p01";
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertMultiColumnLeft(epService, listener);
    
            EPStatementObjectModel modelReverse = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, modelReverse.ToEPL());
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiColumnLeft(EPServiceProvider epService) {
            string epl = "select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                    typeof(SupportBean_S0).FullName + "#length(3) as s0 " +
                    "left outer join " +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " on s0.p00 = s1.p10 and s0.p01 = s1.p11";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertMultiColumnLeft(epService, listener);
    
            stmt.Dispose();
        }
    
        private void AssertMultiColumnLeft(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A_1", "B_1", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A_1", "B_1", 2, "A_1", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A_2", "B_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "A_1", "B_2"));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertionMultiColumnRight(EPServiceProvider epService) {
            string[] fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".Split(',');
            string epl = "select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                    typeof(SupportBean_S0).FullName + "#length(3) as s0 " +
                    "right outer join " +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " on s0.p00 = s1.p10 and s1.p11 = s0.p01";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_1", "B_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A_1", "B_1", 2, "A_1", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 3, "A_2", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(4, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 4, "A_1", "B_2"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiColumnRightCoercion(EPServiceProvider epService) {
            string[] fields = "s0.TheString, s1.TheString".Split(',');
            string epl = "select s0.TheString, s1.TheString from " +
                    typeof(SupportBean).FullName + "(TheString like 'S0%')#keepall as s0 " +
                    "right outer join " +
                    typeof(SupportBean).FullName + "(TheString like 'S1%')#keepall as s1" +
                    " on s0.IntPrimitive = s1.DoublePrimitive and s1.IntPrimitive = s0.DoublePrimitive";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "S1_1", 10, 20d);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, "S1_1"});
    
            SendEvent(epService, "S0_2", 11, 22d);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "S0_3", 11, 21d);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "S0_4", 12, 21d);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "S1_2", 11, 22d);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, "S1_2"});
    
            SendEvent(epService, "S1_3", 22, 11d);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_2", "S1_3"});
    
            SendEvent(epService, "S0_5", 22, 11d);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_5", "S1_2"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRightOuterJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = SetupStatement(epService, listener, "right");
    
            // Send S0 events, no events expected
            SendEvent(EVENTS_S0[0], epService);
            SendEvent(EVENTS_S0[1], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS, null);
    
            // Send S1[2]
            SendEvent(EVENTS_S1[2], epService);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, null, null, 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 202, "2"}});
    
            // Send S0[2] events, joined event expected
            SendEvent(EVENTS_S0[2], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"}});
    
            // Send S1[3]
            SendEvent(EVENTS_S1[3], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, null, null, 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"},
                            new object[] {null, null, 203, "3"}});
    
            // Send some more S0 events
            SendEvent(EVENTS_S0[3], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 103, "3", 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"}});
    
            // Send some more S0 events
            SendEvent(EVENTS_S0[4], epService);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"},
                            new object[] {103, "3", 203, "3"}});
    
            // Push S0[2] out of the window
            SendEvent(EVENTS_S0[5], epService);
            theEvent = listener.AssertOneGetOldAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 202, "2"},
                            new object[] {103, "3", 203, "3"}});
    
            // Some more S1 events
            SendEvent(EVENTS_S1[6], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 206, "6");
            SendEvent(EVENTS_S1[7], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 207, "7");
            SendEvent(EVENTS_S1[8], epService);
            CompareEvent(listener.AssertOneGetNewAndReset(), null, null, 208, "8");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, 202, "2"},
                            new object[] {103, "3", 203, "3"},
                            new object[] {null, null, 206, "6"},
                            new object[] {null, null, 207, "7"},
                            new object[] {null, null, 208, "8"}});
    
            // Push S1[2] out of the window
            SendEvent(EVENTS_S1[9], epService);
            EventBean oldEvent = listener.LastOldData[0];
            EventBean newEvent = listener.LastNewData[0];
            CompareEvent(oldEvent, null, null, 202, "2");
            CompareEvent(newEvent, null, null, 209, "9");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {103, "3", 203, "3"},
                            new object[] {null, null, 206, "6"},
                            new object[] {null, null, 207, "7"},
                            new object[] {null, null, 208, "8"},
                            new object[] {null, null, 209, "9"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionLeftOuterJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = SetupStatement(epService, listener, "left");
    
            // Send S1 events, no events expected
            SendEvent(EVENTS_S1[0], epService);
            SendEvent(EVENTS_S1[1], epService);
            SendEvent(EVENTS_S1[3], epService);
            Assert.IsNull(listener.LastNewData);    // No events expected
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS, null);
    
            // Send S0 event, expect event back from outer join
            SendEvent(EVENTS_S0[2], epService);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", null, null}});
    
            // Send S1 event matching S0, expect event back
            SendEvent(EVENTS_S1[2], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"}});
    
            // Send some more unmatched events
            SendEvent(EVENTS_S1[4], epService);
            SendEvent(EVENTS_S1[5], epService);
            SendEvent(EVENTS_S1[6], epService);
            Assert.IsNull(listener.LastNewData);    // No events expected
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"}});
    
            // Send event, expect a join result
            SendEvent(EVENTS_S0[5], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 105, "5", 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", 202, "2"},
                            new object[] {105, "5", 205, "5"}});
    
            // Let S1[2] go out of the window (lenght 5), expected old join event
            SendEvent(EVENTS_S1[7], epService);
            SendEvent(EVENTS_S1[8], epService);
            theEvent = listener.AssertOneGetOldAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", null, null},
                            new object[] {105, "5", 205, "5"}});
    
            // S0[9] should generate an outer join event
            SendEvent(EVENTS_S0[9], epService);
            theEvent = listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 109, "9", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {102, "2", null, null},
                            new object[] {109, "9", null, null},
                            new object[] {105, "5", 205, "5"}});
    
            // S0[2] Should leave the window (length 3), should get OLD and NEW event
            SendEvent(EVENTS_S0[10], epService);
            EventBean oldEvent = listener.LastOldData[0];
            EventBean newEvent = listener.LastNewData[0];
            CompareEvent(oldEvent, 102, "2", null, null);     // S1[2] has left the window already
            CompareEvent(newEvent, 110, "10", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {110, "10", null, null},
                            new object[] {109, "9", null, null},
                            new object[] {105, "5", 205, "5"}});
        }
    
        private void RunAssertionEventType(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = SetupStatement(epService, listener, "left");
    
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("s0.p00"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("s0.id"));
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("s1.p10"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("s1.id"));
            Assert.AreEqual(4, stmt.EventType.PropertyNames.Length);
        }
    
        private void CompareEvent(EventBean receivedEvent, int? idS0, string p00, int? idS1, string p10) {
            Assert.AreEqual(idS0, receivedEvent.Get("s0.id"));
            Assert.AreEqual(idS1, receivedEvent.Get("s1.id"));
            Assert.AreEqual(p00, receivedEvent.Get("s0.p00"));
            Assert.AreEqual(p10, receivedEvent.Get("s1.p10"));
        }
    
        private void SendEvent(EPServiceProvider epService, string s, int intPrimitive, double doublePrimitive) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string s, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEventMD(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private EPStatement SetupStatement(EPServiceProvider epService, SupportUpdateListener listener, string outerJoinType) {
            string joinStatement = "select irstream s0.id, s0.p00, s1.id, s1.p10 from " +
                    typeof(SupportBean_S0).FullName + "#length(3) as s0 " +
                    outerJoinType + " outer join " +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " on s0.p00 = s1.p10";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += listener.Update;
            return stmt;
        }
    
        private void SendEvent(Object theEvent, EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
