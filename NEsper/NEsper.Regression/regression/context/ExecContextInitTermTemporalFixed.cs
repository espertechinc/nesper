///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextInitTermTemporalFixed : RegressionExecution {
    
        public override void Configure(Configuration configuration)
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();

            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.AddEventType<SupportBean>("SupportBean");
            configuration.AddEventType<SupportBean_S0>("SupportBean_S0");
            configuration.AddEventType<SupportBean_S1>("SupportBean_S1");
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionContextPartitionSelection(epService);
            RunAssertionFilterStartedFilterEndedCorrelatedOutputSnapshot(epService);
            RunAssertionFilterStartedPatternEndedCorrelated(epService);
            RunAssertionStartAfterEndAfter(epService);
            RunAssertionFilterStartedFilterEndedOutputSnapshot(epService);
            RunAssertionPatternStartedPatternEnded(epService);
            RunAssertionContextCreateDestroy(epService);
            RunAssertionDBHistorical(epService);
            RunAssertionPrevPriorAndAggregation(epService);
            RunAssertionJoin(epService);
            RunAssertionPatternWithTime(epService);
            RunAssertionSubselect(epService);
            RunAssertionNWSameContextOnExpr(epService);
            RunAssertionNWFireAndForget(epService);
            RunAssertionStartTurnedOff(epService);
            RunAssertionStartTurnedOn(epService);
        }
    
        private void RunAssertionContextPartitionSelection(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create context MyCtx as start SupportBean_S0 s0 end SupportBean_S1(id=s0.id)");
            var stmt = epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.s0.p00 as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean#keepall group by TheString");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            var expected = new object[][]{new object[] {0, "S0_1", "E1", 6}, new object[] {0, "S0_1", "E2", 10}, new object[] {0, "S0_1", "E3", 201}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expected);
    
            // test iterator targeted by context partition id
            var selectorById = new SupportSelectorById(Collections.SingletonSet(0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, expected);
    
            // test iterator targeted by property on triggering event
            var filtered = new SupportSelectorFilteredInitTerm("S0_1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, expected);
            filtered = new SupportSelectorFilteredInitTerm("S0_2");
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
    
            // test always-false filter - compare context partition info
            filtered = new SupportSelectorFilteredInitTerm(null);
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{1000L}, filtered.ContextsStartTimes);
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{"S0_1"}, filtered.P00PropertyValues);
    
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented() {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById] interfaces but received com."),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterStartedFilterEndedCorrelatedOutputSnapshot(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start SupportBean_S0 as s0 " +
                    "end SupportBean_S1(p10 = s0.p00) as s1");
    
            var fields = "c1,c2,c3".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.id as c1, context.s1.id as c2, sum(IntPrimitive) as c3 " +
                    "from SupportBean#keepall output snapshot when terminated");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // terminate
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 200, 5});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts new one
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // ignored
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(201, "G2"));  // terminate
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{101, 201, 15});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterStartedPatternEndedCorrelated(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start SupportBean_S0 as s0 " +
                    "end pattern [SupportBean_S1(p10 = s0.p00)]");
    
            var fields = "c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean#keepall");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));  // false terminate
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 5});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // actual terminate
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts second
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 6});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, null));    // false terminate
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "GY"));    // false terminate
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 13});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(300, "G2"));  // actual terminate
            epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // starts third
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G3"));    // terminate third
    
            epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartAfterEndAfter(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as start after 5 sec end after 10 sec");
    
            var fields = "c1,c2,c3".Split(',');
            var fieldsShort = "c3".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.startTime as c1, context.endTime as c2, sum(IntPrimitive) as c3 " +
                    "from SupportBean#keepall");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimeEvent(epService, "2002-05-1T08:00:05.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:05.000"), DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:15.000"), 2});
    
            SendTimeEvent(epService, "2002-05-1T08:00:14.999");
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsShort, new object[]{5});
    
            SendTimeEvent(epService, "2002-05-1T08:00:15.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-1T08:00:20.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:20.000"), DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:30.000"), 5});
    
            SendTimeEvent(epService, "2002-05-1T08:00:30.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(listener.IsInvoked);
    
            // try variable
            epService.EPAdministrator.CreateEPL("create variable int var_start = 10");
            epService.EPAdministrator.CreateEPL("create variable int var_end = 20");
            epService.EPAdministrator.CreateEPL("create context FrequentlyContext as start after var_start sec end after var_end sec");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterStartedFilterEndedOutputSnapshot(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as start SupportBean_S0 as s0 end SupportBean_S1 as s1");
    
            var fields = "c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean#keepall output snapshot when terminated");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // terminate
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "S1_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_1", 5});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_2"));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "S0_2"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_3"));    // ends it
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_2", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(103, "S0_3"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean("E5", 6));           // some more data
            epService.EPRuntime.SendEvent(new SupportBean_S0(104, "S0_4"));    // ignored
            epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_3"));    // ends it
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_3", 6});
    
            statement.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternStartedPatternEnded(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start pattern [s0=SupportBean_S0 -> timer:interval(1 sec)] " +
                    "end pattern [s1=SupportBean_S1 -> timer:interval(1 sec)]");
    
            var fields = "c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean#keepall");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimeEvent(epService, "2002-05-1T08:00:01.000"); // 1 second passes
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_1", 4});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_1", 9});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "S0_2"));    // ignored
            SendTimeEvent(epService, "2002-05-1T08:00:03.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_1", 15});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_1"));    // ignored
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_1", 22});
    
            SendTimeEvent(epService, "2002-05-1T08:00:04.000"); // terminates
    
            epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "S1_2"));    // ignored
            SendTimeEvent(epService, "2002-05-1T08:00:10.000");
            epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(103, "S0_3"));    // new instance
            SendTimeEvent(epService, "2002-05-1T08:00:11.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E10", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_3", 10});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextCreateDestroy(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EverySecond as start (*, *, *, *, *, *) end (*, *, *, *, *, *)");
    
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EverySecond select * from SupportBean");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.999");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendTimeEvent(epService, "2002-05-1T08:00:01.000");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            var start = DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:01.999");
            for (var i = 0; i < 10; i++) {
                SendTimeEvent(epService, start);
    
                SendEventAndAssert(epService, listener, false);
    
                start += 1;
                SendTimeEvent(epService, start);
    
                SendEventAndAssert(epService, listener, true);
    
                start += 999;
                SendTimeEvent(epService, start);
    
                SendEventAndAssert(epService, listener, true);
    
                start += 1;
                SendTimeEvent(epService, start);
    
                SendEventAndAssert(epService, listener, false);
    
                start += 999;
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDBHistorical(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            var fields = "s1.mychar".Split(',');
            var listener = new SupportUpdateListener();
            var stmtText = "context NineToFive select * from SupportBean_S0 as s0, sql:MyDB ['select * from mytesttable where ${id} = mytesttable.mybigint'] as s1";
            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Y"});
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"X"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPrevPriorAndAggregation(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            var fields = "col1,col2,col3,col4,col5".Split(',');
            var listener = new SupportUpdateListener();
            var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "select prev(TheString) as col1, prevwindow(sb) as col2, prevtail(TheString) as col3, prior(1, TheString) as col4, sum(IntPrimitive) as col5 " +
                    "from SupportBean#keepall as sb");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            var event1 = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(event1);
            var expected = new object[][]{new object[] {null, new SupportBean[]{event1}, "E1", null, 1}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            var event2 = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(event2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", new SupportBean[]{event2, event1}, "E1", "E1", 3});
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            // now started
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
    
            var event3 = new SupportBean("E3", 9);
            epService.EPRuntime.SendEvent(event3);
            expected = new object[][]{new object[] {null, new SupportBean[]{event3}, "E3", null, 9}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 0, 3, 1);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoin(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            var fields = "col1,col2,col3,col4".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "select sb.TheString as col1, sb.IntPrimitive as col2, s0.id as col3, s0.p00 as col4 " +
                    "from SupportBean#keepall as sb full outer join SupportBean_S0#keepall as s0 on p00 = TheString");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 1, "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 5, 1, "E1"});
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
    
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 4, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 4, 2, "E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternWithTime(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context NineToFive select * from pattern[every timer:interval(10 sec)]");
            statement.Events += listener.Update;
            var spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);   // from the context
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);   // context + pattern
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-1T09:00:10.000");
            Assert.IsTrue(listener.IsInvoked);
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
            listener.Reset();   // it is not well defined whether the listener does get fired or not
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);   // from the context
    
            // now started
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);   // context + pattern
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-2T09:00:10.000");
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubselect(EPServiceProvider epService) {
            var filterSPI = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            var fields = "TheString,col".Split(',');
            var listener = new SupportUpdateListener();
            var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NineToFive select TheString, (select p00 from SupportBean_S0#lastevent) as col from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);   // from the context
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "S01"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "S01"});
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context
    
            epService.EPRuntime.SendEvent(new SupportBean("Ex", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);   // from the context
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(12, "S02"));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "S02"});
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 1, 0, 0);
    
            // now gone
            SendTimeEvent(epService, "2002-05-2T17:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context
    
            epService.EPRuntime.SendEvent(new SupportBean("Ey", 0));
            Assert.IsFalse(listener.IsInvoked);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNWSameContextOnExpr(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("makeBean", GetType(), "SingleRowPluginMakeBean");
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            // no started yet
            var fields = "TheString,IntPrimitive".Split(',');
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow#keepall as SupportBean");
            stmt.Events += listener.Update;
    
            epService.EPAdministrator.CreateEPL("context NineToFive insert into MyWindow select * from SupportBean");
    
            epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "on SupportBean_S0 s0 merge MyWindow mw where mw.TheString = s0.p00 " +
                    "when matched then update set IntPrimitive = s0.id " +
                    "when not matched then insert select MakeBean(id, p00)");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E1"));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E1", 3});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E1", 1});
            listener.Reset();
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
    
            // no longer updated
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            // now started again but empty
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNWFireAndForget(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
    
            // no started yet
            epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("context NineToFive insert into MyWindow select * from SupportBean");
    
            // not queryable
            TryInvalidNWQuery(epService);
    
            // now started
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            TryNWQuery(epService, 0);
    
            // now not empty
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(1, epService.EPRuntime.ExecuteQuery("select * from MyWindow").Array.Length);
    
            // now gone
            SendTimeEvent(epService, "2002-05-1T17:00:00.000");
    
            // no longer queryable
            TryInvalidNWQuery(epService);
            epService.EPRuntime.SendEvent(new SupportBean());
    
            // now started again but empty
            SendTimeEvent(epService, "2002-05-2T09:00:00.000");
            TryNWQuery(epService, 0);
    
            // fill some data
            epService.EPRuntime.SendEvent(new SupportBean());
            epService.EPRuntime.SendEvent(new SupportBean());
            SendTimeEvent(epService, "2002-05-2T09:10:00.000");
            TryNWQuery(epService, 2);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalidNWQuery(EPServiceProvider epService) {
            try {
                epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            } catch (EPException ex) {
                var expected = "Error executing statement: Named window 'MyWindow' is associated to context 'NineToFive' that is not available for querying without context partition selector, use the ExecuteQuery(epl, selector) method instead [select * from MyWindow]";
                Assert.AreEqual(expected, ex.Message);
            }
        }
    
        private void TryNWQuery(EPServiceProvider epService, int numRows) {
            var result = epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            Assert.AreEqual(numRows, result.Array.Length);
        }
    
        private void RunAssertionStartTurnedOff(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            var contextEPL = "@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            var stmtContext = epService.EPAdministrator.CreateEPL("@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            AssertContextEventType(stmtContext.EventType);
            var contextListener = new SupportUpdateListener();
            stmtContext.Events += contextListener.Update;
            stmtContext.Subscriber = new MiniSubscriber();

            var stmtOneListener = new SupportUpdateListener();
            var stmtOne = epService.EPAdministrator.CreateEPL(
                "@Name('A') context NineToFive select * from SupportBean", stmtOneListener);
            stmtOne.Events += stmtOneListener.Update;
    
            SendTimeAndAssert(epService, "2002-05-1T08:59:30.000", false, 1);
            SendTimeAndAssert(epService, "2002-05-1T08:59:59.999", false, 1);
            SendTimeAndAssert(epService, "2002-05-1T09:00:00.000", true, 1);

            var stmtTwoListener = new SupportUpdateListener();
            var stmtTwo = epService.EPAdministrator.CreateEPL(
                "@Name('B') context NineToFive select * from SupportBean", stmtTwoListener);
            stmtTwo.Events += stmtTwoListener.Update;
    
            SendTimeAndAssert(epService, "2002-05-1T16:59:59.000", true, 2);
            SendTimeAndAssert(epService, "2002-05-1T17:00:00.000", false, 2);

            var stmtThreeListener = new SupportUpdateListener();
            var stmtThree = epService.EPAdministrator.CreateEPL(
                "@Name('C') context NineToFive select * from SupportBean", stmtThreeListener);
            stmtThree.Events += stmtThreeListener.Update;
    
            SendTimeAndAssert(epService, "2002-05-2T08:59:59.999", false, 3);
            SendTimeAndAssert(epService, "2002-05-2T09:00:00.000", true, 3);
            SendTimeAndAssert(epService, "2002-05-2T16:59:59.000", true, 3);
            SendTimeAndAssert(epService, "2002-05-2T17:00:00.000", false, 3);
    
            Assert.IsFalse(contextListener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test SODA
            SendTimeEvent(epService, "2002-05-3T16:59:59.000");
            var model = epService.EPAdministrator.CompileEPL(contextEPL);
            Assert.AreEqual(contextEPL, model.ToEPL());
            var stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(contextEPL, stmt.Text);

            // test built-in properties
            var listener = new SupportUpdateListener();
            var stmtLast = epService.EPAdministrator.CreateEPL(
                "@Name('A') context NineToFive " +
                "select context.name as c1, context.startTime as c2, context.endTime as c3, TheString as c4 from SupportBean",
                listener);
            stmtLast.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("NineToFive", theEvent.Get("c1"));
            Assert.AreEqual("2002-05-03 16:59:59.000", DateTimeHelper.Print(theEvent.Get("c2").AsDateTime()));
            Assert.AreEqual("2002-05-03 17:00:00.000", DateTimeHelper.Print(theEvent.Get("c3").AsDateTime()));
            Assert.AreEqual("E1", theEvent.Get("c4"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartTurnedOn(EPServiceProvider epService) {
    
            var ctxMgmtService = ((EPServiceProviderSPI) epService).ContextManagementService;
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
    
            SendTimeEvent(epService, "2002-05-1T09:15:00.000");
            var stmtContext = epService.EPAdministrator.CreateEPL("@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Assert.AreEqual(1, ctxMgmtService.ContextCount);

            var stmtOneListener = new SupportUpdateListener();
            var stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') context NineToFive select * from SupportBean", stmtOneListener);
            stmtOne.Events += stmtOneListener.Update;
    
            SendTimeAndAssert(epService, "2002-05-1T09:16:00.000", true, 1);
            SendTimeAndAssert(epService, "2002-05-1T16:59:59.000", true, 1);
            SendTimeAndAssert(epService, "2002-05-1T17:00:00.000", false, 1);

            var stmtTwoListener = new SupportUpdateListener();
            var stmtTwo = epService.EPAdministrator.CreateEPL("@Name('B') context NineToFive select * from SupportBean", stmtTwoListener);
            stmtTwo.Events += stmtTwoListener.Update;
    
            SendTimeAndAssert(epService, "2002-05-2T08:59:59.999", false, 2);
            SendTimeAndAssert(epService, "2002-05-2T09:15:00.000", true, 2);
            SendTimeAndAssert(epService, "2002-05-2T16:59:59.000", true, 2);
            SendTimeAndAssert(epService, "2002-05-2T17:00:00.000", false, 2);
    
            // destroy context before stmts
            stmtContext.Dispose();
            Assert.AreEqual(1, ctxMgmtService.ContextCount);
    
            stmtTwo.Dispose();
            stmtOne.Dispose();
    
            // context gone too
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
        }
    
        private void AssertContextEventType(EventType eventType) {
            Assert.AreEqual(0, eventType.PropertyNames.Length);
            Assert.AreEqual("anonymous_EventType_Context_NineToFive", eventType.Name);
        }
    
        private void SendTimeAndAssert(EPServiceProvider epService, string time, bool isInvoked, int countStatements) {
            SendTimeEvent(epService, time);
            epService.EPRuntime.SendEvent(new SupportBean());
    
            var statements = epService.EPAdministrator.StatementNames;
            Assert.AreEqual(countStatements + 1, statements.Count);
    
            foreach (var statement in statements) {
                var stmt = epService.EPAdministrator.GetStatement(statement);
                if (stmt.Name == "context") {
                    continue;
                }

                if (stmt.UserObject is SupportUpdateListener listener)
                {
                    Assert.AreEqual(isInvoked, listener.GetAndClearIsInvoked(), "Failed for statement " + stmt.Name);
                }
                else
                {
                    throw new IllegalStateException("value not a SupportUpdateListener");
                }
            }
        }
    
        private void SendTimeEvent(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendTimeEvent(EPServiceProvider epService, long time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
        }
    
        private void SendEventAndAssert(EPServiceProvider epService, SupportUpdateListener listener, bool expected) {
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(expected, listener.IsInvoked);
            listener.Reset();
        }
    
        public static SupportBean SingleRowPluginMakeBean(int id, string p00) {
            return new SupportBean(p00, id);
        }
    
        public class MiniSubscriber {
            public static void Update() {
                // no action
            }
        }
    }
} // end of namespace
