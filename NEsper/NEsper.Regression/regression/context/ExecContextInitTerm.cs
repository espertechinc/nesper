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
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextInitTerm : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNoTerminationCondition(epService);
            RunAssertionStartZeroInitiatedNow(epService);
            RunAssertionEndSameEventAsAnalyzed(epService);
            RunAssertionContextPartitionSelection(epService);
            RunAssertionFilterInitiatedFilterAllTerminated(epService);
            RunAssertionFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot(epService);
            RunAssertionScheduleFilterResources(epService);
            RunAssertionPatternIntervalZeroInitiatedNow(epService);
            RunAssertionPatternInclusion(epService);
            RunAssertionPatternInitiatedStraightSelect(epService);
            RunAssertionFilterInitiatedStraightEquals(epService);
            RunAssertionFilterAllOperators(epService);
            RunAssertionFilterBooleanOperator(epService);
            RunAssertionTerminateTwoContextSameTime(epService);
            RunAssertionOutputSnapshotWhenTerminated(epService);
            RunAssertionOutputAllEvery2AndTerminated(epService);
            RunAssertionOutputWhenExprWhenTerminatedCondition(epService);
            RunAssertionOutputOnlyWhenTerminatedCondition(epService);
            RunAssertionOutputOnlyWhenSetAndWhenTerminatedSet(epService);
            RunAssertionOutputOnlyWhenTerminatedThenSet(epService);
            RunAssertionCrontab(epService);
            RunAssertionStartNowCalMonthScoped(epService);
        }
    
        private void RunAssertionNoTerminationCondition(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5));
    
            TryAssertionNoTerminationConditionOverlapping(epService, false);
            TryAssertionNoTerminationConditionOverlapping(epService, true);
    
            TryAssertionNoTerminationConditionNonoverlapping(epService, false);
            TryAssertionNoTerminationConditionNonoverlapping(epService, true);
    
            TryAssertionNoTerminationConditionNested(epService, false);
            TryAssertionNoTerminationConditionNested(epService, true);
        }
    
        private void RunAssertionStartZeroInitiatedNow(EPServiceProvider epService) {
            var fieldsOne = "c0,c1".Split(',');
    
            // test start-after with immediate start
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var contextExpr = "create context CtxPerId start after 0 sec end after 60 sec";
            epService.EPAdministrator.CreateEPL(contextExpr);
            var stream = epService.EPAdministrator.CreateEPL("context CtxPerId select TheString as c0, IntPrimitive as c1 from SupportBean");
            var listener = new SupportUpdateListener();
            stream.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(59999));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternIntervalZeroInitiatedNow(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecContextInitTerm))) {
                return;
            }
    
            var fieldsOne = "c0,c1".Split(',');
    
            // test initiated-by pattern with immediate start
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000));
            var contextExprTwo = "create context CtxPerId initiated by pattern [timer:interval(0) or every timer:interval(1 min)] terminated after 60 sec";
            epService.EPAdministrator.CreateEPL(contextExprTwo);
            var streamTwo = epService.EPAdministrator.CreateEPL("context CtxPerId select TheString as c0, sum(IntPrimitive) as c1 from SupportBean");
            var listener = new SupportUpdateListener();
            streamTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E1", 10});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000 + 59999));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E2", 30});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000 + 60000));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E3", 4});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternInclusion(EPServiceProvider epService) {
            var fields = "TheString,IntPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var contextExpr = "create context CtxPerId initiated by pattern [every-distinct (a.TheString, 10 sec) a=SupportBean]@Inclusive terminated after 10 sec ";
            epService.EPAdministrator.CreateEPL(contextExpr);
            var streamExpr = "context CtxPerId select * from SupportBean(TheString = context.a.TheString) output last when terminated";
            var stream = epService.EPAdministrator.CreateEPL(streamExpr);
            var listener = new SupportUpdateListener();
            stream.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10100));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 4});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(16100));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20099));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 5});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(26100 - 1));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(26100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 6});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test multiple pattern with multiple events
            var contextExprMulti = "create context CtxPerId initiated by pattern [every a=SupportBean_S0 -> b=SupportBean_S1]@Inclusive terminated after 10 sec ";
            epService.EPAdministrator.CreateEPL(contextExprMulti);
            var streamExprMulti = "context CtxPerId select * from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]";
            var streamMulti = epService.EPAdministrator.CreateEPL(streamExprMulti);
            streamMulti.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "S1_1"));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEndSameEventAsAnalyzed(EPServiceProvider epService) {
    
            // same event terminates - not included
            var fields = "c1,c2,c3,c4".Split(',');
            epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "start SupportBean " +
                    "end SupportBean(IntPrimitive=11)");
            var stmt = epService.EPAdministrator.CreateEPL("context MyCtx " +
                    "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
                    "output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10, 10, 10d});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // same event terminates - included
            fields = "c1,c2,c3,c4".Split(',');
            epService.EPAdministrator.CreateEPL("create schema MyCtxTerminate(TheString string)");
            epService.EPAdministrator.CreateEPL("create context MyCtx as start SupportBean end MyCtxTerminate");
            stmt = epService.EPAdministrator.CreateEPL("context MyCtx " +
                    "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
                    "output snapshot when terminated");
            stmt.Events += listener.Update;
            epService.EPAdministrator.CreateEPL("insert into MyCtxTerminate select TheString from SupportBean(IntPrimitive=11)");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11, 21, 10.5d});
    
            // test with audit
            var epl = "@Audit create context AdBreakCtx as initiated by SupportBean(IntPrimitive > 0) as ad " +
                    " terminated by SupportBean(TheString=ad.TheString, IntPrimitive < 0) as endAd";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPAdministrator.CreateEPL("context AdBreakCtx select count(*) from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", -10));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextPartitionSelection(EPServiceProvider epService) {
            var fields = "c0,c1,c2,c3".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create context MyCtx as initiated by SupportBean_S0 s0 terminated by SupportBean_S1(id=s0.id)");
            var stmt = epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.s0.p00 as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean#keepall group by TheString");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new[] {new object[] {0, "S0_1", "E1", 6}, new object[] {0, "S0_1", "E2", 10}, new object[] {0, "S0_1", "E3", 201}, new object[] {1, "S0_2", "E1", 3}, new object[] {1, "S0_2", "E3", 201}});
    
            // test iterator targeted by context partition id
            var selectorById = new SupportSelectorById(Collections.SingletonSet(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, new[] {new object[] {1, "S0_2", "E1", 3}, new object[] {1, "S0_2", "E3", 201}});
    
            // test iterator targeted by property on triggering event
            var filtered = new SupportSelectorFilteredInitTerm("S0_2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new[] {new object[] {1, "S0_2", "E1", 3}, new object[] {1, "S0_2", "E3", 201}});
    
            // test always-false filter - compare context partition info
            filtered = new SupportSelectorFilteredInitTerm(null);
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{1000L, 2000L}, filtered.ContextsStartTimes);
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{"S0_1", "S0_2"}, filtered.P00PropertyValues);
    
            try
            {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented(() => null));
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(
                    ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById] interfaces but received com."),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterInitiatedFilterAllTerminated(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create context MyContext as " +
                    "initiated by SupportBean_S0 " +
                    "terminated by SupportBean_S1");
    
            var fields = "c1".Split(',');
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context MyContext select sum(IntPrimitive) as c1 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1")); // initiate one
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "S0_2"));  // initiate another
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {5}, new object[] {3}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {9}, new object[] {7}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "S1_1"));  // terminate all
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "initiated by SupportBean_S0 as s0 " +
                    "terminated by SupportBean_S1(p10 = s0.p00)");
    
            var fields = "c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            var statement = epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean#keepall output snapshot when terminated");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // terminate
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 5});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts new one
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // also starts new one
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G2"));  // terminate G2
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 15});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G3"));  // terminate G3
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 11});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionScheduleFilterResources(EPServiceProvider epService) {
            // test no-context statement
            var stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#time(30)");
            var spi = (EPServiceProviderSPI) epService;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
    
            stmt.Dispose();
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            // test initiated
            var filterServiceSPI = (FilterServiceSPI) spi.FilterService;
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            var eplCtx = "create context EverySupportBean as " +
                    "initiated by SupportBean as sb " +
                    "terminated after 1 minutes";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            epService.EPAdministrator.CreateEPL("context EverySupportBean select * from SupportBean_S0#time(2 min) sb0");
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(1, filterServiceSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(2, filterServiceSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "S0_1"));
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(2, filterServiceSPI.FilterCountApprox);
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(1, filterServiceSPI.FilterCountApprox);
    
            epService.EPAdministrator.DestroyAllStatements();
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(0, filterServiceSPI.FilterCountApprox);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternInitiatedStraightSelect(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            var eplCtx = "create context EverySupportBean as " +
                    "initiated by pattern [every (a=SupportBean_S0 or b=SupportBean_S1)] " +
                    "terminated after 1 minutes";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var fields = "c1,c2,c3".Split(',');
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EverySupportBean " +
                    "select context.a.id as c1, context.b.id as c2, TheString as c3 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, 2, "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {null, 2, "E2"}, new object[] {3, null, "E2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test SODA
            AssertSODA(epService, eplCtx);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterInitiatedStraightEquals(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            var ctxEPL = "create context EverySupportBean as " +
                    "initiated by SupportBean(TheString like \"I%\") as sb " +
                    "terminated after 1 minutes";
            epService.EPAdministrator.CreateEPL(ctxEPL);
    
            var fields = "c1".Split(',');
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EverySupportBean " +
                    "select sum(LongPrimitive) as c1 from SupportBean(IntPrimitive = context.sb.IntPrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, -2L));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("I1", 2, 4L)); // counts towards stuff
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 3L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{7L});
    
            epService.EPRuntime.SendEvent(MakeEvent("I2", 3, 14L)); // counts towards stuff
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{14L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 2, 2L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{9L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E4", 3, 15L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{29L});
    
            SendTimeEvent(epService, "2002-05-1T08:01:30.000");
    
            epService.EPRuntime.SendEvent(MakeEvent("E", -1, -2L));
            Assert.IsFalse(listener.IsInvoked);
    
            // test SODA
            epService.EPAdministrator.DestroyAllStatements();
            var model = epService.EPAdministrator.CompileEPL(ctxEPL);
            Assert.AreEqual(ctxEPL, model.ToEPL());
            var stmtModel = epService.EPAdministrator.Create(model);
            Assert.AreEqual(ctxEPL, stmtModel.Text);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilterAllOperators(EPServiceProvider epService) {
    
            // test plain
            epService.EPAdministrator.CreateEPL("create context EverySupportBean as " +
                    "initiated by SupportBean_S0 as sb " +
                    "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds");
    
            TryOperator(epService, "context.sb.id = IntBoxed", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
            TryOperator(epService, "IntBoxed = context.sb.id", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
    
            TryOperator(epService, "context.sb.id > IntBoxed", new[] {new object[] {11, false}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "context.sb.id >= IntBoxed", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "context.sb.id < IntBoxed", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "context.sb.id <= IntBoxed", new[] {new object[] {11, true}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "IntBoxed < context.sb.id", new[] {new object[] {11, false}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed <= context.sb.id", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed > context.sb.id", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "IntBoxed >= context.sb.id", new[] {new object[] {11, true}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "IntBoxed in (context.sb.id)", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "IntBoxed between context.sb.id and context.sb.id", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "context.sb.id != IntBoxed", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
            TryOperator(epService, "IntBoxed != context.sb.id", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
    
            TryOperator(epService, "IntBoxed not in (context.sb.id)", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed not between context.sb.id and context.sb.id", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
    
            TryOperator(epService, "context.sb.id is IntBoxed", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
            TryOperator(epService, "IntBoxed is context.sb.id", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
    
            TryOperator(epService, "context.sb.id is not IntBoxed", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
            TryOperator(epService, "IntBoxed is not context.sb.id", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
    
            // try coercion
            TryOperator(epService, "context.sb.id = ShortBoxed", new[] {new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}});
            TryOperator(epService, "ShortBoxed = context.sb.id", new[] {new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}});
    
            TryOperator(epService, "context.sb.id > ShortBoxed", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, false}, new object[] {(short) 9, true}, new object[] {(short) 8, true}});
            TryOperator(epService, "ShortBoxed < context.sb.id", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, false}, new object[] {(short) 9, true}, new object[] {(short) 8, true}});
    
            TryOperator(epService, "ShortBoxed in (context.sb.id)", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {(short) 8, false}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOperator(EPServiceProvider epService, string @operator, object[][] testdata) {
            var filterSpi = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
    
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EverySupportBean " +
                    "select TheString as c0,IntPrimitive as c1,context.sb.p00 as c2 " +
                    "from SupportBean(" + @operator + ")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // initiate
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S01"));
    
            for (var i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean();
                var testValue = testdata[i][0];
                if (testValue is int?) {
                    bean.IntBoxed = (int?) testValue;
                } else {
                    bean.ShortBoxed = (short?) testValue;
                }
                var expected = (bool) testdata[i][1];
    
                epService.EPRuntime.SendEvent(bean);
                Assert.AreEqual(expected, listener.GetAndClearIsInvoked(), "Failed at " + i);
            }
    
            // assert type of expression
            if (filterSpi.IsSupportsTakeApply) {
                var set = filterSpi.Take(Collections.SingletonList(stmt.StatementId));
                Assert.AreEqual(1, set.Filters.Count);
                var valueSet = set.Filters[0].FilterValueSet;
                Assert.AreEqual(1, valueSet.Parameters.Length);
                var para = valueSet.Parameters[0][0];
                Assert.IsTrue(para.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionFilterBooleanOperator(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EverySupportBean as " +
                    "initiated by SupportBean_S0 as sb " +
                    "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds");
    
            var fields = "c0,c1,c2".Split(',');
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EverySupportBean " +
                    "select TheString as c0,IntPrimitive as c1,context.sb.p00 as c2 " +
                    "from SupportBean(IntPrimitive + context.sb.id = 5)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S01"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2, "S01"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S02"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E3", 2, "S01"}, new object[] {"E3", 2, "S02"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "S03"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4", 2, "S01"}, new object[] {"E4", 2, "S02"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E5", 1, "S03"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTerminateTwoContextSameTime(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            var eplContext = "@Name('CTX') create context CtxInitiated " +
                    "initiated by SupportBean_S0 as sb0 " +
                    "terminated after 1 minute";
            epService.EPAdministrator.CreateEPL(eplContext);
            var listener = new SupportUpdateListener();
    
            var fields = "c1,c2,c3".Split(',');
            var eplGrouped = "@Name('S1') context CtxInitiated select TheString as c1, sum(IntPrimitive) as c2, context.sb0.p00 as c3 from SupportBean";
            epService.EPAdministrator.CreateEPL(eplGrouped).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "SB01"));
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G2", 2, "SB01"});
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G3", 5, "SB01"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "SB02"));
    
            epService.EPRuntime.SendEvent(new SupportBean("G4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"G4", 9, "SB01"}, new object[] {"G4", 4, "SB02"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("G5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"G5", 14, "SB01"}, new object[] {"G5", 9, "SB02"}});
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("G6", 6));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // clean up
            epService.EPAdministrator.GetStatement("S1").Dispose();
            epService.EPAdministrator.GetStatement("CTX").Dispose();
        }
    
        private void RunAssertionOutputSnapshotWhenTerminated(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min");
    
            // test when-terminated and snapshot
            var fields = "c1".Split(',');
            var epl = "context EveryMinute select sum(IntPrimitive) as c1 from SupportBean output snapshot when terminated";
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendTimeEvent(epService, "2002-05-1T08:01:10.000");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendTimeEvent(epService, "2002-05-1T08:01:59.999");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // terminate
            SendTimeEvent(epService, "2002-05-1T08:02:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1 + 2 + 3});
    
            SendTimeEvent(epService, "2002-05-1T08:02:01.000");
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            AssertSODA(epService, epl);
    
            // terminate
            SendTimeEvent(epService, "2002-05-1T08:03:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4 + 5 + 6});
    
            stmt.Dispose();
    
            // test late-coming statement without "terminated"
            var stmtTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EveryMinute " +
                    "select context.id as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 2 events");
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E10", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E11", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-1T08:04:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E12", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E13", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{7});
    
            // terminate
            SendTimeEvent(epService, "2002-05-1T08:05:00.000");
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputAllEvery2AndTerminated(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min");
    
            // test when-terminated and every 2 events output all with group by
            var fields = "c1,c2".Split(',');
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context EveryMinute " +
                    "select TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString output all every 2 events and when terminated order by TheString asc");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendTimeEvent(epService, "2002-05-1T08:01:10.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", 1 + 2}});
    
            SendTimeEvent(epService, "2002-05-1T08:01:59.999");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // terminate
            SendTimeEvent(epService, "2002-05-1T08:02:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", 1 + 2}, new object[] {"E2", 3}});
    
            SendTimeEvent(epService, "2002-05-1T08:02:01.000");
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4", 4}, new object[] {"E5", 5}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4", 14}, new object[] {"E5", 5}, new object[] {"E6", 6}});
    
            // terminate
            SendTimeEvent(epService, "2002-05-1T08:03:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4", 14}, new object[] {"E5", 5}, new object[] {"E6", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E6", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", -1}, new object[] {"E6", -2}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputWhenExprWhenTerminatedCondition(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min");
    
            // test when-terminated and every 2 events output all with group by
            var fields = "c0".Split(',');
            var epl = "context EveryMinute " +
                    "select TheString as c0 from SupportBean output when count_insert>1 and when terminated and count_insert>0";
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1"}, new object[] {"E2"}});
    
            SendTimeEvent(epService, "2002-05-1T08:01:59.999");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // terminate, new context partition
            SendTimeEvent(epService, "2002-05-1T08:02:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E3"}});
    
            SendTimeEvent(epService, "2002-05-1T08:02:10.000");
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4"}, new object[] {"E5"}});
    
            SendTimeEvent(epService, "2002-05-1T08:03:00.000");
            Assert.IsFalse(listener.IsInvoked);
    
            AssertSODA(epService, epl);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputOnlyWhenTerminatedCondition(EPServiceProvider epService) {
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min");
    
            // test when-terminated and every 2 events output all with group by
            var fields = "c0".Split(',');
            var epl = "context EveryMinute " +
                    "select TheString as c0 from SupportBean output when terminated and count_insert > 0";
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            // terminate, new context partition
            SendTimeEvent(epService, "2002-05-1T08:02:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1"}, new object[] {"E2"}});
    
            // terminate, new context partition
            SendTimeEvent(epService, "2002-05-1T08:03:00.000");
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputOnlyWhenSetAndWhenTerminatedSet(EPServiceProvider epService) {
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min");
    
            // include then-set and both real-time and terminated output
            epService.EPAdministrator.CreateEPL("create variable int myvar = 0");
            var eplOne = "context EveryMinute select TheString as c0 from SupportBean " +
                    "output when true " +
                    "then set myvar=1 " +
                    "and when terminated " +
                    "then set myvar=2";
            var stmtOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("myvar"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendTimeEvent(epService, "2002-05-1T08:02:00.000"); // terminate, new context partition
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(2, epService.EPRuntime.GetVariableValue("myvar"));
    
            AssertSODA(epService, eplOne);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputOnlyWhenTerminatedThenSet(EPServiceProvider epService) {
    
            var fields = "c0".Split(',');
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create variable int myvar = 0");
            epService.EPAdministrator.CreateEPL("create context EverySupportBeanS0 as " +
                    "initiated by SupportBean_S0 as s0 " +
                    "terminated after 1 min");
    
            // include only-terminated output with set
            epService.EPRuntime.SetVariableValue("myvar", 0);
            var eplTwo = "context EverySupportBeanS0 select TheString as c0 from SupportBean " +
                    "output when terminated " +
                    "then set myvar=10";
            var stmtTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplTwo);
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            // terminate, new context partition
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E4"}});
            Assert.AreEqual(10, epService.EPRuntime.GetVariableValue("myvar"));
    
            AssertSODA(epService, eplTwo);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCrontab(EPServiceProvider epService) {
            var filterSPI = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 3 min");
    
            var fields = "c1,c2".Split(',');
            var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("@IterableUnbound context EveryMinute select TheString as c1, sum(IntPrimitive) as c2 from SupportBean");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, null);
    
            SendTimeEvent(epService, "2002-05-1T08:01:00.000");
    
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            var expected = new[] {new object[] {"E2", 5}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:01:59.999");
    
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1);
            epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
            expected = new[] {new object[] {"E3", 11}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:02:00.000");
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2);
            epService.EPRuntime.SendEvent(new SupportBean("E4", 7));
            expected = new[] {new object[] {"E4", 18}, new object[] {"E4", 7}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:02:59.999");
    
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2);
            epService.EPRuntime.SendEvent(new SupportBean("E5", 8));
            expected = new[] {new object[] {"E5", 26}, new object[] {"E5", 15}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:03:00.000");
    
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
            epService.EPRuntime.SendEvent(new SupportBean("E6", 9));
            expected = new[] {new object[] {"E6", 35}, new object[] {"E6", 24}, new object[] {"E6", 9}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:04:00.000");
    
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
            epService.EPRuntime.SendEvent(new SupportBean("E7", 10));
            expected = new[] {new object[] {"E7", 34}, new object[] {"E7", 19}, new object[] {"E7", 10}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            SendTimeEvent(epService, "2002-05-1T08:05:00.000");
    
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
            epService.EPRuntime.SendEvent(new SupportBean("E8", 11));
            expected = new[] {new object[] {"E8", 30}, new object[] {"E8", 21}, new object[] {"E8", 11}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
    
            // assert certain keywords are valid: last keyword, timezone
            epService.EPAdministrator.CreateEPL("create context CtxMonthly1 start (0, 0, 1, *, *, 0) End(59, 23, last, *, *, 59)");
            epService.EPAdministrator.CreateEPL("create context CtxMonthly2 start (0, 0, 1, *, *) End(59, 23, last, *, *)");
            epService.EPAdministrator.CreateEPL("create context CtxMonthly3 start (0, 0, 1, *, *, 0, 'GMT-5') End(59, 23, last, *, *, 59, 'GMT-8')");
            TryInvalid(epService, "create context CtxMonthly4 start (0) End(*,*,*,*,*)",
                    "Error starting statement: Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 1 [create context CtxMonthly4 start (0) End(*,*,*,*,*)]");
            TryInvalid(epService, "create context CtxMonthly4 start (*,*,*,*,*) End(*,*,*,*,*,*,*,*)",
                    "Error starting statement: Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 8 [create context CtxMonthly4 start (*,*,*,*,*) End(*,*,*,*,*,*,*,*)]");
    
            // test invalid -after
            TryInvalid(epService, "create context CtxMonthly4 start after 1 second end after -1 seconds",
                    "Error starting statement: Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after 1 second end after -1 seconds]");
            TryInvalid(epService, "create context CtxMonthly4 start after -1 second end after 1 seconds",
                    "Error starting statement: Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after -1 second end after 1 seconds]");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartNowCalMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            epService.EPAdministrator.CreateEPL("create context MyCtx start SupportBean_S1 end after 1 month");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context MyCtx select * from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionNoTerminationConditionOverlapping(EPServiceProvider epService, bool soda) {
    
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create context SupportBeanInstanceCtx as initiated by SupportBean as sb");
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, "context SupportBeanInstanceCtx " +
                    "select id, context.sb.IntPrimitive as sbint, context.startTime as starttime, context.endTime as endtime from SupportBean_S0(p00=context.sb.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = "id,sbint,starttime,endtime".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "P2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 200, 5L, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "P1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 100, 5L, null});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionNoTerminationConditionNonoverlapping(EPServiceProvider epService, bool soda) {
    
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create context SupportBeanInstanceCtx as start SupportBean as sb");
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, "context SupportBeanInstanceCtx " +
                    "select id, context.sb.IntPrimitive as sbint, context.startTime as starttime, context.endTime as endtime from SupportBean_S0(p00=context.sb.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = "id,sbint,starttime,endtime".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "P2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "P1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20, 100, 5L, null});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionNoTerminationConditionNested(EPServiceProvider epService, bool soda) {
    
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create context MyCtx as " +
                    "context Lvl1Ctx as start SupportBean_S0 as s0, " +
                    "context Lvl2Ctx as start SupportBean_S1 as s1");
    
            var stmt = epService.EPAdministrator.CreateEPL("context MyCtx " +
                    "select TheString, context.Lvl1Ctx.s0.p00 as p00, context.Lvl2Ctx.s1.p10 as p10 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = "TheString,p00,p10".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "A", "B"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendTimeEvent(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private void AssertSODA(EPServiceProvider epService, string epl) {
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            var stmtModel = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmtModel.Text);
            stmtModel.Dispose();
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    
        [Serializable]
        public class Event
        {
            public string ProductID { get; }

            public Event(string productId)
            {
                ProductID = productId;
            }
        }
    }
} // end of namespace
