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
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextNested : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.AddEventType("SupportBean_S2", typeof(SupportBean_S2));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNestedContextWithFilterUDF(epService);
            RunAssertionIterateTargetedCP(epService);
            RunAssertionInvalid(epService);
            RunAssertionIterator(epService);
            RunAssertionPartitionedWithFilter(epService);
            RunAssertionNestingFilterCorrectness(epService, false);
            RunAssertionNestingFilterCorrectness(epService, true);
            RunAssertionPartitionOverlap(epService);
            RunAssertionPartitionedNonOverlap(epService);
            RunAssertionCategoryOverPatternInitiated(epService);
            RunAssertionSingleEventTriggerNested(epService);
            RunAssertion4ContextsNested(epService);
            RunAssertionTemporalOverlapOverPartition(epService);
            RunAssertion3ContextsTermporalOverCategoryOverPartition(epService);
            RunAssertionTemporalFixedOverHash(epService);
            RunAssertionCategoryOverTemporalOverlapping(epService);
            RunAssertionFixedTemporalOverPartitioned(epService);
            RunAssertionPartitionedOverFixedTemporal(epService);
            RunAssertionContextProps(epService);
            RunAssertionLateComingStatement(epService);
            RunAssertionPartitionWithMultiPropsAndTerm(epService);
            RunAssertionNestedOverlappingAndPattern(epService);
            RunAssertionNestedNonOverlapping(epService);
        }
    
        private void RunAssertionNestedContextWithFilterUDF(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "customEnabled", typeof(ExecContextNested), "CustomMatch", FilterOptimizableEnum.ENABLED);
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "customDisabled", typeof(ExecContextNested), "CustomMatch", FilterOptimizableEnum.DISABLED);
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated after 24 hours, " +
                    "context BCtx initiated by SupportBean_S1 as s1 terminated after 1 hour");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context NestedContext select * " +
                    "from SupportBean(" +
                    "CustomEnabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id)" +
                    " and " +
                    "CustomDisabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id))");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "S1"));
            epService.EPRuntime.SendEvent(new SupportBean("X", -1));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public static bool CustomMatch(string theString, string p00, int intPrimitive, int s1id) {
            Assert.AreEqual("X", theString);
            Assert.AreEqual("S0", p00);
            Assert.AreEqual(-1, intPrimitive);
            Assert.AreEqual(2, s1id);
            return true;
        }
    
        private void RunAssertionIterateTargetedCP(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean");
    
            string[] fields = "c0,c1,c2,c3".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
                    "select context.ACtx.s0.p00 as c0, context.BCtx.label as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean#length(5) group by TheString");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            var expectedAll = new object[][]{new object[] {"S0_1", "grp1", "E2", -1}, new object[] {"S0_1", "grp3", "E3", 5}, new object[] {"S0_1", "grp3", "E1", 3}, new object[] {"S0_2", "grp3", "E1", 2}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expectedAll);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(ContextPartitionSelectorAll.INSTANCE), stmt.GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE), fields, expectedAll);
            var allIds = new SupportSelectorById(Collections.Set(0, 1, 2, 3, 4, 5));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(allIds), stmt.GetSafeEnumerator(allIds), fields, expectedAll);
    
            // test iterator targeted
            var firstOne = new SupportSelectorFilteredInitTerm("S0_2");
            var secondOne = new SupportSelectorCategory(Collections.SingletonSet("grp3"));
            var nestedSelector = new SupportSelectorNested(Collections.SingletonList(new ContextPartitionSelector[]{firstOne, secondOne}));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelector), stmt.GetSafeEnumerator(nestedSelector), fields, new object[][]{new object[] {"S0_2", "grp3", "E1", 2}});
    
            var firstTwo = new SupportSelectorFilteredInitTerm("S0_1");
            var secondTwo = new SupportSelectorCategory(Collections.SingletonSet("grp1"));
            var nestedSelectorTwo = new SupportSelectorNested(Collections.List(new ContextPartitionSelector[]{firstOne, secondOne}, new ContextPartitionSelector[]{firstTwo, secondTwo}));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelectorTwo), stmt.GetSafeEnumerator(nestedSelectorTwo), fields, new object[][]{new object[] {"S0_2", "grp3", "E1", 2}, new object[] {"S0_1", "grp1", "E2", -1}});
    
            // test iterator filtered : not supported for nested
            try {
                var filtered = new MySelectorFilteredNested(new object[]{"S0_2", "grp3"});
                stmt.GetEnumerator(filtered);
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorById, ContextPartitionSelectorNested] interfaces but received com."), "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test 3 nesting levels and targeted
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx group by IntPrimitive < 0 as i1, group by IntPrimitive = 0 as i2, group by IntPrimitive > 0 as i3 from SupportBean," +
                    "context BCtx group by LongPrimitive < 0 as l1, group by LongPrimitive = 0 as l2, group by LongPrimitive > 0 as l3 from SupportBean," +
                    "context CCtx group by BoolPrimitive = true as b1, group by BoolPrimitive = false as b2 from SupportBean");
    
            string[] fieldsSelect = "c0,c1,c2,c3".Split(',');
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
                    "select context.ACtx.label as c0, context.BCtx.label as c1, context.CCtx.label as c2, count(*) as c3 from SupportBean#length(5) having count(*) > 0");
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 10L, true));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 2, -10L, false));
            epService.EPRuntime.SendEvent(MakeEvent("E3", 1, 11L, false));
            epService.EPRuntime.SendEvent(MakeEvent("E4", 0, 0L, true));
            epService.EPRuntime.SendEvent(MakeEvent("E5", -1, 10L, false));
            epService.EPRuntime.SendEvent(MakeEvent("E6", -1, 10L, true));
    
            var expectedRows = new object[][]{
                new object[]{"i1", "l3", "b1", 2L},
                new object[]{"i3", "l1", "b2", 1L},
                new object[]{"i1", "l3", "b2", 1L},
                new object[]{"i2", "l2", "b1", 1L},
                new object[]{"i3", "l3", "b2", 1L},
            };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(), stmtSelect.GetSafeEnumerator(), fieldsSelect, expectedRows);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(ContextPartitionSelectorAll.INSTANCE), stmtSelect.GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE), fields, expectedRows);
    
            // test iterator targeted
            var selectors = new ContextPartitionSelector[]{
                    new SupportSelectorCategory(Collections.SingletonSet("i3")),
                    new SupportSelectorCategory(Collections.SingletonSet("l1")),
                    new SupportSelectorCategory(Collections.SingletonSet("b2"))
            };
            var nestedSelectorSelect = new SupportSelectorNested(Collections.SingletonList(selectors));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(nestedSelectorSelect), stmtSelect.GetSafeEnumerator(nestedSelectorSelect), fieldsSelect, new object[][]{new object[] {"i3", "l1", "b2", 1L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            // invalid same sub-context name twice
            epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)";
            TryInvalid(epService, epl, "Error starting statement: Context by name 'EightToNine' has already been declared within nested context 'ABC' [");
    
            // validate statement added to nested context
            epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context PartCtx as partition by TheString from SupportBean";
            epService.EPAdministrator.CreateEPL(epl);
            epl = "context ABC select * from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: Segmented context 'PartCtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");
        }
    
        private void RunAssertionIterator(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegByString partition by TheString from SupportBean");
    
            var listener = new SupportUpdateListener();
            string[] fields = "c0,c1,c2".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.EightToNine.startTime as c0, context.SegByString.key1 as c1, IntPrimitive as c2 from SupportBean#keepall");
            stmtUser.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var expected = new object[][]{new object[] {DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 1}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            expected = new object[][]{new object[] {DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 1}, new object[] {DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 2}};
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);
    
            // extract path
            if (GetSpi(epService).IsSupportsExtract) {
                GetSpi(epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPartitionedWithFilter(EPServiceProvider epService) {
            RunAssertionPartitionedNonOverlap(epService);
            RunAssertionPartitionOverlap(epService);
        }
    
        internal static void RunAssertionNestingFilterCorrectness(EPServiceProvider epService, bool isolationAllowed) {
            string eplContext;
            string eplSelect = "context TheContext select count(*) from SupportBean";
            EPStatementSPI spiCtx;
            EPStatementSPI spiStmt;
            SupportBean bean;
    
            // category over partition
            eplContext = "create context TheContext " +
                    "context CtxCategory as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxPartition as partition by TheString from SupportBean";
            spiCtx = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters(epService, isolationAllowed, "SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            AssertFilters(epService, isolationAllowed, "SupportBean(TheString=E1,IntPrimitive<0)", spiStmt);
            epService.EPAdministrator.DestroyAllStatements();
    
            // category over partition over category
            eplContext = "create context TheContext " +
                    "context CtxCategoryOne as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxPartition as partition by TheString from SupportBean," +
                    "context CtxCategoryTwo as group LongPrimitive < 0 as negative, group LongPrimitive > 0 as positive from SupportBean";
            spiCtx = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters(epService, isolationAllowed, "SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            bean = new SupportBean("E1", -1);
            bean.LongPrimitive = 1;
            epService.EPRuntime.SendEvent(bean);
            AssertFilters(epService, isolationAllowed, "SupportBean(LongPrimitive<0,TheString=E1,IntPrimitive<0),SupportBean(LongPrimitive>0,TheString=E1,IntPrimitive<0)", spiStmt);
            AssertFilters(epService, isolationAllowed, "SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            epService.EPAdministrator.DestroyAllStatements();
    
            // partition over partition over partition
            eplContext = "create context TheContext " +
                    "context CtxOne as partition by TheString from SupportBean, " +
                    "context CtxTwo as partition by IntPrimitive from SupportBean," +
                    "context CtxThree as partition by LongPrimitive from SupportBean";
            spiCtx = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters(epService, isolationAllowed, "SupportBean()", spiCtx);
            bean = new SupportBean("E1", 2);
            bean.LongPrimitive = 3;
            epService.EPRuntime.SendEvent(bean);
            AssertFilters(epService, isolationAllowed, "SupportBean(LongPrimitive=3,IntPrimitive=2,TheString=E1)", spiStmt);
            AssertFilters(epService, isolationAllowed, "SupportBean(),SupportBean(TheString=E1),SupportBean(TheString=E1,IntPrimitive=2)", spiCtx);
            epService.EPAdministrator.DestroyAllStatements();
    
            // category over hash
            eplContext = "create context TheContext " +
                    "context CtxCategoryOne as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxTwo as coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 100";
            spiCtx = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters(epService, isolationAllowed, "SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            bean = new SupportBean("E1", 2);
            bean.LongPrimitive = 3;
            epService.EPRuntime.SendEvent(bean);
            AssertFilters(epService, isolationAllowed, "SupportBean(consistent_hash_crc32(TheString)=33,IntPrimitive>0)", spiStmt);
            AssertFilters(epService, isolationAllowed, "SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            epService.EPAdministrator.DestroyAllStatements();
    
            eplContext = "create context TheContext " +
                    "context CtxOne as partition by TheString from SupportBean, " +
                    "context CtxTwo as start pattern [SupportBean_S0] end pattern[SupportBean_S1]";
            spiCtx = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters(epService, isolationAllowed, "SupportBean()", spiCtx);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            AssertFilters(epService, isolationAllowed, "", spiStmt);
            AssertFilters(epService, isolationAllowed, "SupportBean(),SupportBean_S0()", spiCtx);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static void AssertFilters(EPServiceProvider epService, bool allowIsolation, string expected, EPStatementSPI spiStmt) {
            if (!allowIsolation) {
                return;
            }
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            FilterServiceSPI filterSPI = (FilterServiceSPI) spi.FilterService;
            if (!filterSPI.IsSupportsTakeApply) {
                return;
            }
            FilterSet set = filterSPI.Take(Collections.SingletonSet(spiStmt.StatementId));
            Assert.AreEqual(expected, set.ToString());
            filterSPI.Apply(set);
        }
    
        private void RunAssertionPartitionOverlap(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(EndEvent));
            epService.EPAdministrator.CreateEPL("@Audit('pattern-instances') create context TheContext"
                    + " context CtxSession partition by id from TestEvent, "
                    + " context CtxStartEnd start TestEvent as te end EndEvent(id=te.id)");
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "context TheContext select firstEvent from TestEvent#firstevent() as firstEvent"
                            + " inner join TestEvent#lastevent as lastEvent");
            var supportSubscriber = new SupportSubscriber();
            stmt.Subscriber = supportSubscriber;
    
            for (int i = 0; i < 2; i++) {
                epService.EPRuntime.SendEvent(new TestEvent(1, 5));
                epService.EPRuntime.SendEvent(new TestEvent(2, 10));
                epService.EPRuntime.SendEvent(new EndEvent(1));
    
                supportSubscriber.Reset();
                epService.EPRuntime.SendEvent(new TestEvent(2, 15));
                Assert.AreEqual(10, ((TestEvent) supportSubscriber.AssertOneGetNewAndReset()).Time);
    
                epService.EPRuntime.SendEvent(new EndEvent(1));
                epService.EPRuntime.SendEvent(new EndEvent(2));
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPartitionedNonOverlap(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            string eplCtx = "create context NestedContext as " +
                    "context SegByString as partition by TheString from SupportBean(IntPrimitive > 0), " +
                    "context InitCtx initiated by SupportBean_S0 as s0 terminated after 60 seconds";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c0,c1,c2".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx.s0.p00 as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString");
            stmtUser.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", -5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_1", "E1", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "E2", 4}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_1", "E1", 8}, new object[] {"S0_2", "E1", 6}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCategoryOverPatternInitiated(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            string eplCtx = "create context NestedContext as " +
                    "context ByCat as group IntPrimitive < 0 as g1, group IntPrimitive > 0 as g2, group IntPrimitive = 0 as g3 from SupportBean, " +
                    "context InitCtx as initiated by pattern [every a=SupportBean_S0 -> b=SupportBean_S1(id = a.id)] terminated after 10 sec";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c0,c1,c2,c3".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c0, context.InitCtx.a.p00 as c1, context.InitCtx.b.p10 as c2, sum(IntPrimitive) as c3 from SupportBean group by TheString");
            stmtUser.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g2", "S0_1", "S1_2", 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g1", "S0_1", "S1_2", -2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g3", "S0_1", "S1_2", 0}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g2", "S0_1", "S1_2", 8}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g2", "S0_1", "S1_2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(102, "S0_3"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "S1_3"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g2", "S0_1", "S1_2", 15}, new object[] {"g2", "S0_3", "S1_3", 7}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:10.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(104, "S0_4"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(104, "S1_4"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"g2", "S0_4", "S1_4", 9}});
    
            if (GetSpi(epService).IsSupportsExtract) {
                GetSpi(epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSingleEventTriggerNested(EPServiceProvider epService) {
            // Test partitioned context
            //
            string eplCtxOne = "create context NestedContext as " +
                    "context SegByString as partition by TheString from SupportBean, " +
                    "context SegByInt as partition by IntPrimitive from SupportBean, " +
                    "context SegByLong as partition by LongPrimitive from SupportBean ";
            EPStatement stmtCtxOne = epService.EPAdministrator.CreateEPL(eplCtxOne);
    
            var listenerOne = new SupportUpdateListener();
            string[] fieldsOne = "c0,c1,c2,c3".Split(',');
            EPStatementSPI stmtUserOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.SegByString.key1 as c0, context.SegByInt.key1 as c1, context.SegByLong.key1 as c2, count(*) as c3 from SupportBean");
            stmtUserOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][]{new object[] {"E1", 10, 100L, 1L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][]{new object[] {"E2", 10, 100L, 1L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][]{new object[] {"E1", 11, 100L, 1L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 101));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][]{new object[] {"E1", 10, 101L, 1L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][]{new object[] {"E1", 10, 100L, 2L}});
    
            stmtCtxOne.Dispose();
            stmtUserOne.Dispose();
    
            // Test partitioned context
            //
            string eplCtxTwo = "create context NestedContext as " +
                    "context HashOne coalesce by Hash_code(TheString) from SupportBean granularity 10, " +
                    "context HashTwo coalesce by Hash_code(IntPrimitive) from SupportBean granularity 10";
            EPStatement stmtCtxTwo = epService.EPAdministrator.CreateEPL(eplCtxTwo);
    
            var listenerTwo = new SupportUpdateListener();
            string[] fieldsTwo = "c1,c2".Split(',');
            EPStatementSPI stmtUserTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, count(*) as c2 from SupportBean");
            stmtUserTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][]{new object[] {"E1", 1L}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][]{new object[] {"E2", 1L}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][]{new object[] {"E1", 2L}});
    
            stmtCtxTwo.Dispose();
            stmtUserTwo.Dispose();
    
            // Test partitioned context
            //
            string eplCtxThree = "create context NestedContext as " +
                    "context InitOne initiated by SupportBean(TheString like 'I%') as sb0 terminated after 10 sec, " +
                    "context InitTwo initiated by SupportBean(IntPrimitive > 0) as sb1 terminated after 10 sec";
            EPStatement stmtCtxThree = epService.EPAdministrator.CreateEPL(eplCtxThree);
    
            var listenerThree = new SupportUpdateListener();
            string[] fieldsThree = "c1,c2".Split(',');
            EPStatementSPI stmtUserThree = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, count(*) as c2 from SupportBean");
            stmtUserThree.Events += listenerThree.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EPAssertionUtil.AssertPropsPerRow(listenerThree.GetAndResetLastNewData(), fieldsThree, new object[][]{new object[] {"I1", 1L}});
    
            stmtCtxThree.Dispose();
            stmtUserThree.Dispose();
        }
    
        private void RunAssertion4ContextsNested(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            FilterServiceSPI filterSPI = (FilterServiceSPI) spi.FilterService;
            SendTimeEvent(epService, "2002-05-1T07:00:00.000");
    
            string eplCtx = "create context NestedContext as " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context InitCtx0 initiated by SupportBean_S0 as s0 terminated after 60 seconds, " +
                    "context InitCtx1 initiated by SupportBean_S1 as s1 terminated after 30 seconds, " +
                    "context InitCtx2 initiated by SupportBean_S2 as s2 terminated after 10 seconds";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1,c2,c3,c4".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx0.s0.p00 as c1, context.InitCtx1.s1.p10 as c2, context.InitCtx2.s2.p20 as c3, sum(IntPrimitive) as c4 from SupportBean");
            stmtUser.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_2"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_2", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_2", 5}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:05.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_3"));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_2", 9}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_3"));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_2", 14}, new object[] {"S0_2", "S1_2", "S2_3", 5}, new object[] {"S0_2", "S1_3", "S2_3", 5}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:10.000"); // terminate S2_2 leaf
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_3", 11}, new object[] {"S0_2", "S1_3", "S2_3", 11}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:15.000"); // terminate S0_2/S1_2/S2_3 and S0_2/S1_3/S2_3 leafs
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_4"));
            epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_2", "S2_4", 8}, new object[] {"S0_2", "S1_3", "S2_4", 8}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:30.000"); // terminate S1_2 branch
    
            epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(105, "S1_5"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(205, "S2_5"));
            epService.EPRuntime.SendEvent(new SupportBean("E10", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "S1_3", "S2_5", 10}, new object[] {"S0_2", "S1_5", "S2_5", 10}});
    
            SendTimeEvent(epService, "2002-05-01T08:01:00.000"); // terminate S0_2 branch, only the "8to9" is left
    
            epService.EPRuntime.SendEvent(new SupportBean("E11", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "S0_6"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(106, "S1_6"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(206, "S2_6"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_6", "S1_6", "S2_6", 12}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "S0_7"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(107, "S1_7"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(207, "S2_7"));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 13));
            Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);
    
            SendTimeEvent(epService, "2002-05-1T10:00:00.000"); // terminate all
    
            epService.EPRuntime.SendEvent(new SupportBean("E14", 14));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-2T08:00:00.000"); // start next day
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(8, "S0_8"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(108, "S1_8"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(208, "S2_8"));
            epService.EPRuntime.SendEvent(new SupportBean("E15", 15));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_8", "S1_8", "S2_8", 15}});
    
            stmtUser.Stop();
            epService.EPRuntime.SendEvent(new SupportBean("E16", 16));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            AgentInstanceAssertionUtil.AssertInstanceCounts(stmtUser.StatementContext, 0);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTemporalOverlapOverPartition(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            string eplCtx = "create context NestedContext as " +
                    "context InitCtx initiated by SupportBean_S0(id > 0) as s0 terminated after 10 seconds, " +
                    "context SegmCtx as partition by TheString from SupportBean(IntPrimitive > 0)";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1,c2,c3".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx.s0.p00 as c1, context.SegmCtx.key1 as c2, sum(IntPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_2", "E3", 3});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_2", "E4", 4});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_2", "E3", 8});
    
            SendTimeEvent(epService, "2002-05-1T08:00:05.000");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0_3"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_4"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "E3", 14}, new object[] {"S0_4", "E3", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"S0_2", "E4", 11}, new object[] {"S0_4", "E4", 7}});
    
            SendTimeEvent(epService, "2002-05-1T08:00:10.000"); // expires first context
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_4", "E3", 14});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_4", "E4", 16});
    
            SendTimeEvent(epService, "2002-05-1T08:00:15.000"); // expires second context
    
            epService.EPRuntime.SendEvent(new SupportBean("Ex", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_5"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E4", -10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_5", "E4", 10});
    
            SendTimeEvent(epService, "2002-05-1T08:00:25.000"); // expires second context
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion3ContextsTermporalOverCategoryOverPartition(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            string eplCtx = "create context NestedContext as " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context ByCat as group IntPrimitive<0 as g1, group IntPrimitive=0 as g2, group IntPrimitive>0 as g3 from SupportBean, " +
                    "context SegmentedByString as partition by TheString from SupportBean";
            EPStatement stmtCtx = epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1,c2,c3".Split(',');
            EPStatementSPI stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(LongPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            TryAssertion3Contexts(epService, listener, fields, "2002-05-1T09:00:00.000");
    
            stmtCtx.Dispose();
            stmtUser.Dispose();
    
            SendTimeEvent(epService, "2002-05-2T08:00:00.000");
    
            // test SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, model.ToEPL());
            EPStatement stmtCtxTwo = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplCtx, stmtCtxTwo.Text);
    
            stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(LongPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            TryAssertion3Contexts(epService, listener, fields, "2002-05-2T09:00:00.000");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Root: Temporal
        /// Sub: Hash
        /// </summary>
        private void RunAssertionTemporalFixedOverHash(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            SendTimeEvent(epService, "2002-05-1T07:00:00.000");
    
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context HashedCtx coalesce Hash_code(IntPrimitive) from SupportBean granularity 10 preallocate");
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1,c2".Split(',');
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, count(*) as c2 from SupportBean group by TheString");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000"); // start context
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
    
            SendTimeEvent(epService, "2002-05-1T09:00:00.000"); // terminate
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent(epService, "2002-05-2T08:00:00.000"); // start context
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Root: Category
        /// Sub: Initiated
        /// </summary>
        private void RunAssertionCategoryOverTemporalOverlapping(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
    
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ByCat " +
                    "  group IntPrimitive < 0 and IntPrimitive != -9999 as g1, " +
                    "  group IntPrimitive = 0 as g2, " +
                    "  group IntPrimitive > 0 as g3 from SupportBean, " +
                    "context InitGrd initiated by SupportBean(TheString like 'init%') as sb terminated after 10 seconds");
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1,c2,c3".Split(',');
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.InitGrd.sb.TheString as c2, count(*) as c3 from SupportBean");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("init_1", -9999));
            epService.EPRuntime.SendEvent(new SupportBean("X100", 0));
            epService.EPRuntime.SendEvent(new SupportBean("X101", 10));
            epService.EPRuntime.SendEvent(new SupportBean("X102", -10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("init_2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g2", "init_2", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g2", "init_2", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("init_3", -2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g1", "init_3", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g1", "init_3", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g1", "init_3", 3L});
    
            SendTimeEvent(epService, "2002-05-1T08:11:00.000"); // terminates all
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Root: Fixed temporal
        /// Sub: Partition by string
        /// <para>
        /// - Root starts deactivated.
        /// - With context destroy before statement destroy
        /// </para>
        /// </summary>
        private void RunAssertionFixedTemporalOverPartitioned(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            FilterServiceSPI filterSPI = (FilterServiceSPI) spi.FilterService;
            SendTimeEvent(epService, "2002-05-1T07:00:00.000");
    
            EPStatement stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1".Split(',');
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select count(*) as c1 from SupportBean");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
    
            // ends EightToNine context
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            // starts EightToNine context
            SendTimeEvent(epService, "2002-05-2T08:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
    
            stmtCtx.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
    
            statement.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Root: Partition by string
        /// Sub: Fixed temporal
        /// <para>
        /// - Sub starts deactivated.
        /// - With statement destroy before context destroy
        /// </para>
        /// </summary>
        private void RunAssertionPartitionedOverFixedTemporal(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            FilterServiceSPI filterSPI = (FilterServiceSPI) spi.FilterService;
            SendTimeEvent(epService, "2002-05-1T07:00:00.000");
    
            EPStatement stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context SegmentedByAString partition by TheString from SupportBean, " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            string[] fields = "c1".Split(',');
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select count(*) as c1 from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent(epService, "2002-05-1T08:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            // ends EightToNine context
            SendTimeEvent(epService, "2002-05-1T09:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent(epService, "2002-05-2T08:00:00.000");
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L});
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
            Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);
    
            statement.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
    
            stmtCtx.Dispose();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Test nested context properties.
        /// <para>
        /// Root: Fixed temporal
        /// Sub: Partition by string
        /// </para>
        /// <para>
        /// - fixed temportal starts active
        /// - starting and stopping statement
        /// </para>
        /// </summary>
        private void RunAssertionContextProps(EPServiceProvider epService) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            FilterServiceSPI filterSPI = (FilterServiceSPI) spi.FilterService;
            SendTimeEvent(epService, "2002-05-1T08:30:00.000");
    
            EPStatement stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
    
            var listener = new SupportUpdateListener();
            string[] fields = "c0,c1,c2,c3,c4,c5,c6".Split(',');
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.EightToNine.name as c0, " +
                    "context.EightToNine.startTime as c1, " +
                    "context.SegmentedByAString.name as c2, " +
                    "context.SegmentedByAString.key1 as c3, " +
                    "context.name as c4, " +
                    "IntPrimitive as c5," +
                    "count(*) as c6 " +
                    "from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                    "SegmentedByAString", "E1",
                    "NestedContext",
                    10, 1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                    "SegmentedByAString", "E2",
                    "NestedContext",
                    20, 1L});
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
    
            statement.Stop();
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            statement.Start();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                    "SegmentedByAString", "E2",
                    "NestedContext",
                    30, 1L});
            Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 0, 0, 0);
    
            statement.Dispose();
            stmtCtx.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        /// <summary>
        /// Test late-coming statement.
        /// <para>
        /// Root: Fixed temporal
        /// Sub: Partition by string
        /// </para>
        /// </summary>
        private void RunAssertionLateComingStatement(EPServiceProvider epService) {
            SendTimeEvent(epService, "2002-05-1T08:30:00.000");
    
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
    
            var listenerOne = new SupportUpdateListener();
            string[] fields = "c0,c1".Split(',');
            EPStatementSPI statementOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, count(*) as c1 from SupportBean");
            statementOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            var listenerTwo = new SupportUpdateListener();
            EPStatementSPI statementTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, sum(IntPrimitive) as c1 from SupportBean");
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"E2", 30});
    
            var listenerThree = new SupportUpdateListener();
            EPStatementSPI statementThree = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, min(IntPrimitive) as c1 from SupportBean");
            statementThree.Events += listenerThree.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 40));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"E1", 60});
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new object[]{"E1", 40});
    
            statementTwo.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 4L});
            Assert.IsFalse(listenerTwo.IsInvoked);
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new object[]{"E1", 40});
    
            statementOne.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -60));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new object[]{"E1", -60});
    
            statementThree.Dispose();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertion3Contexts(EPServiceProvider epService, SupportUpdateListener listener, string[] fields, string subsequentTime) {
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g2", "E1", 10L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 0, 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g2", "E2", 11L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g2", "E1", 22L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g3", "E1", 13L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 14));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g1", "E1", 14L});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 15));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"g1", "E2", 15L});
    
            SendTimeEvent(epService, subsequentTime);
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 15));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertionPartitionWithMultiPropsAndTerm(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by TheString, IntPrimitive from SupportBean, " +
                    "context InitiateAndTerm start SupportBean as e1 " +
                    "end SupportBean_S0(id=e1.IntPrimitive and p00=e1.TheString)");
    
            var listenerOne = new SupportUpdateListener();
            string[] fields = "c0,c1,c2".Split(',');
            EPStatementSPI statementOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select TheString as c0, IntPrimitive as c1, count(LongPrimitive) as c2 from SupportBean \n" +
                    "output last when terminated");
            statementOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 1));
            Assert.IsFalse(listenerOne.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 0, 2L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedOverlappingAndPattern(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by TheString from SupportBean, " +
                    "context TimedImmediate initiated @now and pattern[every timer:interval(10)] terminated after 10 seconds");
            TryAssertion(epService);
        }
    
        private void RunAssertionNestedNonOverlapping(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by TheString from SupportBean, " +
                    "context TimedImmediate start @now end after 10 seconds");
            TryAssertion(epService);
        }
    
        private void TryAssertion(EPServiceProvider epService) {
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listenerOne = new SupportUpdateListener();
            string[] fields = "c0,c1".Split(',');
            EPStatement statementOne = epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select TheString as c0, sum(IntPrimitive) as c1 from SupportBean \n" +
                    "output last when terminated");
            statementOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}}, null);
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 3}, new object[] {"E3", 4}}, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private Object MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private Object MakeEvent(string theString, int intPrimitive, long longPrimitive, bool boolPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.BoolPrimitive = boolPrimitive;
            return bean;
        }
    
        private void SendTimeEvent(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
            return (EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin;
        }
    
        public class MySelectorFilteredNested : ContextPartitionSelectorFiltered {
    
            private readonly object[] pathMatch;
    
            private List<object[]> paths = new List<object[]>();
            private LinkedHashSet<int?> cpids = new LinkedHashSet<int?>();
    
            public MySelectorFilteredNested(object[] pathMatch) {
                this.pathMatch = pathMatch;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                ContextPartitionIdentifierNested nested = (ContextPartitionIdentifierNested) contextPartitionIdentifier;
                if (pathMatch == null && cpids.Contains(nested.ContextPartitionId)) {
                    throw new EPRuntimeException("Already exists context id: " + nested.ContextPartitionId);
                }
                cpids.Add(nested.ContextPartitionId);
    
                ContextPartitionIdentifierInitiatedTerminated first = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0];
                ContextPartitionIdentifierCategory second = (ContextPartitionIdentifierCategory) nested.Identifiers[1];
    
                var extract = new Object[2];
                extract[0] = ((EventBean) first.Properties.Get("s0")).Get("p00");
                extract[1] = second.Label;
                paths.Add(extract);
    
                return paths != null && Collections.AreEqual(pathMatch, extract);
            }
        }
    
        [Serializable]
        public class TestEvent  {
            private int time;
            private int id;
    
            public TestEvent(int id, int time) {
                this.id = id;
                this.time = time;
            }

            public int Time {
                get { return time; }
            }

            public int Id {
                get { return id; }
            }
        }
    
        [Serializable]
        public class EndEvent  {
            private int id;
    
            public EndEvent(int id) {
                this.id = id;
            }

            public int Id {
                get { return id; }
            }
        }
    }
} // end of namespace
