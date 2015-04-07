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
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextNested
    {
        private EPServiceProvider _epService;
        private EPServiceProviderSPI _spi;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.AddEventType("SupportBean_S2", typeof(SupportBean_S2));
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _spi = (EPServiceProviderSPI) _epService;
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestNestedContextWithFilterUDF()
        {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "customEnabled", typeof(TestContextNested).FullName, "CustomMatch", FilterOptimizable.ENABLED);
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    "customDisabled", typeof(TestContextNested).FullName, "CustomMatch", FilterOptimizable.DISABLED);
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated after 24 hours, " +
                    "context BCtx initiated by SupportBean_S1 as s1 terminated after 1 hour");
            var stmt = _epService.EPAdministrator.CreateEPL("context NestedContext select * " +
                    "from SupportBean(" +
                    "customEnabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id)" +
                    " and " +
                    "customDisabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id))");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "S1"));
            _epService.EPRuntime.SendEvent(new SupportBean("X", -1));
            Assert.IsTrue(listener.IsInvoked);
        }
    
        public static bool CustomMatch(String theString, String p00, int intPrimitive, int s1id)
        {
            Assert.AreEqual("X", theString);
            Assert.AreEqual("S0", p00);
            Assert.AreEqual(-1, intPrimitive);
            Assert.AreEqual(2, s1id);
            return true;
        }
    
        [Test]
        public void TestIterateTargetedCP()
        {
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean");
    
            var fields = "c0,c1,c2,c3".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
                    "select context.ACtx.s0.p00 as c0, context.BCtx.label as c1, TheString as c2, Sum(IntPrimitive) as c3 from SupportBean.win:length(5) group by TheString");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            var expectedAll = new Object[][]{new Object[] {"S0_1", "grp1", "E2", -1}, new Object[] {"S0_1", "grp3", "E3", 5}, new Object[] {"S0_1", "grp3", "E1", 3}, new Object[] {"S0_2", "grp3", "E1", 2}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expectedAll);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(ContextPartitionSelectorAll.INSTANCE), stmt.GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE), fields, expectedAll);
            var allIds = new SupportSelectorById(new HashSet<int>{ 0, 1, 2, 3, 4, 5 });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(allIds), stmt.GetSafeEnumerator(allIds), fields, expectedAll);
    
            // test iterator targeted
            ContextPartitionSelector firstOne = new SupportSelectorFilteredInitTerm("S0_2");
            ContextPartitionSelector secondOne = new SupportSelectorCategory(Collections.SingletonList("grp3"));
            var nestedSelector = new SupportSelectorNested(new ContextPartitionSelector[] {firstOne, secondOne});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelector), stmt.GetSafeEnumerator(nestedSelector), fields, new Object[][]{new Object[] {"S0_2", "grp3", "E1", 2}});
    
            ContextPartitionSelector firstTwo = new SupportSelectorFilteredInitTerm("S0_1");
            ContextPartitionSelector secondTwo = new SupportSelectorCategory(Collections.SingletonList("grp1"));
            var nestedSelectorTwo = new SupportSelectorNested(Collections.List(new ContextPartitionSelector[] {firstOne, secondOne}, new ContextPartitionSelector[] {firstTwo, secondTwo}));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelectorTwo), stmt.GetSafeEnumerator(nestedSelectorTwo), fields, new Object[][]{new Object[] {"S0_2", "grp3", "E1", 2}, new Object[] {"S0_1", "grp1", "E2", -1}});
    
            // test iterator filtered : not supported for nested
            try {
                var filtered = new MySelectorFilteredNested(new Object[] {"S0_2", "grp3"});
                stmt.GetEnumerator(filtered);
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorById, ContextPartitionSelectorNested] interfaces but received com."), "message: " + ex.Message);
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test 3 nesting levels and targeted
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx group by IntPrimitive < 0 as i1, group by IntPrimitive = 0 as i2, group by IntPrimitive > 0 as i3 from SupportBean," +
                    "context BCtx group by LongPrimitive < 0 as l1, group by LongPrimitive = 0 as l2, group by LongPrimitive > 0 as l3 from SupportBean," +
                    "context CCtx group by BoolPrimitive = true as b1, group by BoolPrimitive = false as b2 from SupportBean");
    
            var fieldsSelect = "c0,c1,c2,c3".Split(',');
            var stmtSelect = _epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
                    "select context.ACtx.label as c0, context.BCtx.label as c1, context.CCtx.label as c2, Count(*) as c3 from SupportBean.win:length(5) having Count(*) > 0");
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 10L, true));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 2, -10L, false));
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 1, 11L, false));
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 0, 0L, true));
            _epService.EPRuntime.SendEvent(MakeEvent("E5", -1, 10L, false));
            _epService.EPRuntime.SendEvent(MakeEvent("E6", -1, 10L, true));
    
            var expectedRows = new Object[][] {
                    new Object[] { "i1", "l3", "b1", 2L},
                    new Object[] { "i3", "l1", "b2", 1L},
                    new Object[] { "i1", "l3", "b2", 1L},
                    new Object[] { "i2", "l2", "b1", 1L},
                    new Object[] { "i3", "l3", "b2", 1L},
                    };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(), stmtSelect.GetSafeEnumerator(), fieldsSelect, expectedRows);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(ContextPartitionSelectorAll.INSTANCE), stmtSelect.GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE), fields, expectedRows);
    
            // test iterator targeted
            var selectors = new ContextPartitionSelector[] {
                    new SupportSelectorCategory(Collections.SingletonList("i3")),
                    new SupportSelectorCategory(Collections.SingletonList("l1")),
                    new SupportSelectorCategory(Collections.SingletonList("b2"))
            };
            var nestedSelectorSelect = new SupportSelectorNested(Collections.SingletonList(selectors));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(nestedSelectorSelect), stmtSelect.GetSafeEnumerator(nestedSelectorSelect), fieldsSelect, new Object[][] {new Object[] {"i3", "l1", "b2", 1L}});
        }
    
        [Test]
        public void TestInvalid() {
            String epl;
    
            // invalid same sub-context name twice
            epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)";
            TryInvalid(epl, "Error starting statement: Context by name 'EightToNine' has already been declared within nested context 'ABC' [");
    
            // validate statement added to nested context
            epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context PartCtx as partition by TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(epl);
            epl = "context ABC select * from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: Segmented context 'PartCtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");
        }
    
        private void TryInvalid(String epl, String expected) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                if (!ex.Message.StartsWith(expected)) {
                    throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
                }
                Assert.IsTrue(expected.Trim().Length != 0);
            }
        }
    
        [Test]
        public void TestIterator()
        {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegByString partition by TheString from SupportBean");
    
            var listener = new SupportUpdateListener();
            var fields = "c0,c1,c2".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.EightToNine.startTime as c0, context.SegByString.key1 as c1, IntPrimitive as c2 from SupportBean.win:keepall()");
            stmtUser.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var expected = new Object[][]{new Object[] {DateTimeHelper.ParseDefaultMSec("2002-05-01 08:00:00.000"), "E1", 1}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            expected = new Object[][]{new Object[] {DateTimeHelper.ParseDefaultMSec("2002-05-01 08:00:00.000"), "E1", 1}, new Object[] {DateTimeHelper.ParseDefaultMSec("2002-05-01 08:00:00.000"), "E1", 2}};
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);

            // extract path
            GetSpi(_epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
        }
    
        [Test]
        public void TestPartitionedWithFilter()
        {
            RunAssertionPartitionedNonOverlap();
            RunAssertionPartitionOverlap();
        }
    
        [Test]
        public void TestNestingFilterCorrectness() {
            String eplContext;
            var eplSelect = "context TheContext select Count(*) from SupportBean";
            EPStatementSPI spiCtx;
            EPStatementSPI spiStmt;
            SupportBean bean;
    
            // category over partition
            eplContext = "create context TheContext " +
                    "context CtxCategory as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxPartition as partition by TheString from SupportBean";
            spiCtx = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters("SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            AssertFilters("SupportBean(TheString=E1,IntPrimitive<0)", spiStmt);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // category over partition over category
            eplContext = "create context TheContext " +
                    "context CtxCategoryOne as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxPartition as partition by TheString from SupportBean," +
                    "context CtxCategoryTwo as group LongPrimitive < 0 as negative, group LongPrimitive > 0 as positive from SupportBean";
            spiCtx = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters("SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            bean = new SupportBean("E1", -1);
            bean.LongPrimitive = 1;
            _epService.EPRuntime.SendEvent(bean);
            AssertFilters("SupportBean(LongPrimitive<0,TheString=E1,IntPrimitive<0),SupportBean(LongPrimitive>0,TheString=E1,IntPrimitive<0)", spiStmt);
            AssertFilters("SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // partition over partition over partition
            eplContext = "create context TheContext " +
                    "context CtxOne as partition by TheString from SupportBean, " +
                    "context CtxTwo as partition by IntPrimitive from SupportBean," +
                    "context CtxThree as partition by LongPrimitive from SupportBean";
            spiCtx = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters("SupportBean()", spiCtx);
            bean = new SupportBean("E1", 2);
            bean.LongPrimitive = 3;
            _epService.EPRuntime.SendEvent(bean);
            AssertFilters("SupportBean(LongPrimitive=3,IntPrimitive=2,TheString=E1)", spiStmt);
            AssertFilters("SupportBean(),SupportBean(TheString=E1),SupportBean(TheString=E1,IntPrimitive=2)", spiCtx);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // category over hash
            eplContext = "create context TheContext " +
                    "context CtxCategoryOne as group IntPrimitive < 0 as negative, group IntPrimitive > 0 as positive from SupportBean, " +
                    "context CtxTwo as coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 100";
            spiCtx = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters("SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            bean = new SupportBean("E1", 2);
            bean.LongPrimitive = 3;
            _epService.EPRuntime.SendEvent(bean);
            AssertFilters("SupportBean(consistent_hash_crc32(unresolvedPropertyName=TheString streamOrPropertyName=null resolvedPropertyName=TheString)=33,IntPrimitive>0)", spiStmt);
            AssertFilters("SupportBean(IntPrimitive<0),SupportBean(IntPrimitive>0)", spiCtx);
            _epService.EPAdministrator.DestroyAllStatements();
    
            eplContext = "create context TheContext " +
                    "context CtxOne as partition by TheString from SupportBean, " +
                    "context CtxTwo as start pattern [SupportBean_S0] end pattern[SupportBean_S1]";
            spiCtx = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplContext);
            spiStmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplSelect);
    
            AssertFilters("SupportBean()", spiCtx);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            AssertFilters("", spiStmt);
            AssertFilters("SupportBean(),SupportBean_S0()", spiCtx);
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertFilters(String expected, EPStatementSPI spiStmt)
        {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            var set = filterSPI.Take(Collections.SingletonList(spiStmt.StatementId));
            Assert.AreEqual(expected, set.ToString());
            filterSPI.Apply(set);
        }
    
        private void RunAssertionPartitionOverlap()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(EndEvent));
            _epService.EPAdministrator.CreateEPL("@Audit('pattern-instances') create context TheContext"
                    + " context CtxSession partition by id from TestEvent, "
                    + " context CtxStartEnd start TestEvent as te end EndEvent(id=te.id)");
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "context TheContext select firstEvent from TestEvent.std:firstevent() as firstEvent"
                            + " inner join TestEvent.std:lastevent() as lastEvent");
            var supportSubscriber = new SupportSubscriber();
            stmt.Subscriber = supportSubscriber;
    
            for (var i = 0; i < 2; i++) {
                _epService.EPRuntime.SendEvent(new TestEvent(1, 5));
                _epService.EPRuntime.SendEvent(new TestEvent(2, 10));
                _epService.EPRuntime.SendEvent(new EndEvent(1));
    
                supportSubscriber.Reset();
                _epService.EPRuntime.SendEvent(new TestEvent(2, 15));
                Assert.AreEqual(10, (((TestEvent) supportSubscriber.AssertOneGetNewAndReset()) .Time));
    
                _epService.EPRuntime.SendEvent(new EndEvent(1));
                _epService.EPRuntime.SendEvent(new EndEvent(2));
            }
        }
    
        private void RunAssertionPartitionedNonOverlap() {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            var eplCtx = "create context NestedContext as " +
                    "context SegByString as partition by TheString from SupportBean(IntPrimitive > 0), " +
                    "context InitCtx initiated by SupportBean_S0 as s0 terminated after 60 seconds";
            var stmtCtx = _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            var fields = "c0,c1,c2".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx.s0.p00 as c0, TheString as c1, Sum(IntPrimitive) as c2 from SupportBean group by TheString");
            stmtUser.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -5));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_1", "E1", 2}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "E2", 4}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_1", "E1", 8}, new Object[] {"S0_2", "E1", 6}});
        }
    
        [Test]
        public void TestCategoryOverPatternInitiated()
        {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            var eplCtx = "create context NestedContext as " +
                    "context ByCat as group IntPrimitive < 0 as g1, group IntPrimitive > 0 as g2, group IntPrimitive = 0 as g3 from SupportBean, " +
                    "context InitCtx as initiated by pattern [every a=SupportBean_S0 -> b=SupportBean_S1(id = a.id)] terminated after 10 sec";
            _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            var fields = "c0,c1,c2,c3".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c0, context.InitCtx.a.p00 as c1, context.InitCtx.b.p10 as c2, Sum(IntPrimitive) as c3 from SupportBean group by TheString");
            stmtUser.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g2", "S0_1", "S1_2", 3}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g1", "S0_1", "S1_2", -2}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g3", "S0_1", "S1_2", 0}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g2", "S0_1", "S1_2", 8}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g2", "S0_1", "S1_2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "S0_3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "S1_3"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g2", "S0_1", "S1_2", 15}, new Object[] {"g2", "S0_3", "S1_3", 7}});
    
            SendTimeEvent("2002-05-01 08:00:10.000");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(104, "S0_4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(104, "S1_4"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"g2", "S0_4", "S1_4", 9}});

            GetSpi(_epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
        }
    
        [Test]
        public void TestSingleEventTriggerNested() {
            // Test partitioned context
            //
            var eplCtxOne = "create context NestedContext as " +
                    "context SegByString as partition by TheString from SupportBean, " +
                    "context SegByInt as partition by IntPrimitive from SupportBean, " +
                    "context SegByLong as partition by LongPrimitive from SupportBean ";
            var stmtCtxOne = _epService.EPAdministrator.CreateEPL(eplCtxOne);
    
            var listenerOne = new SupportUpdateListener();
            var fieldsOne = "c0,c1,c2,c3".Split(',');
            var stmtUserOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.SegByString.key1 as c0, context.SegByInt.key1 as c1, context.SegByLong.key1 as c2, Count(*) as c3 from SupportBean");
            stmtUserOne.Events += listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new Object[][]{new Object[] {"E1", 10, 100L, 1L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new Object[][]{new Object[] {"E2", 10, 100L, 1L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new Object[][]{new Object[] {"E1", 11, 100L, 1L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 101));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new Object[][]{new Object[] {"E1", 10, 101L, 1L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new Object[][]{new Object[] {"E1", 10, 100L, 2L}});
    
            stmtCtxOne.Dispose();
            stmtUserOne.Dispose();
    
            // Test partitioned context
            //
            var eplCtxTwo = "create context NestedContext as " +
                    "context HashOne coalesce by Hash_code(TheString) from SupportBean granularity 10, " +
                    "context HashTwo coalesce by Hash_code(IntPrimitive) from SupportBean granularity 10";
            var stmtCtxTwo = _epService.EPAdministrator.CreateEPL(eplCtxTwo);
    
            var listenerTwo = new SupportUpdateListener();
            var fieldsTwo = "c1,c2".Split(',');
            var stmtUserTwo = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, Count(*) as c2 from SupportBean");
            stmtUserTwo.Events += listenerTwo.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new Object[][]{new Object[] {"E1", 1L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new Object[][]{new Object[] {"E2", 1L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new Object[][]{new Object[] {"E1", 2L}});
    
            stmtCtxTwo.Dispose();
            stmtUserTwo.Dispose();
    
            // Test partitioned context
            //
            var eplCtxThree = "create context NestedContext as " +
                    "context InitOne initiated by SupportBean(TheString like 'I%') as sb0 terminated after 10 sec, " +
                    "context InitTwo initiated by SupportBean(IntPrimitive > 0) as sb1 terminated after 10 sec";
            var stmtCtxThree = _epService.EPAdministrator.CreateEPL(eplCtxThree);
    
            var listenerThree = new SupportUpdateListener();
            var fieldsThree = "c1,c2".Split(',');
            var stmtUserThree = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, Count(*) as c2 from SupportBean");
            stmtUserThree.Events += listenerThree.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EPAssertionUtil.AssertPropsPerRow(listenerThree.GetAndResetLastNewData(), fieldsThree, new Object[][]{new Object[] {"I1", 1L}});
    
            stmtCtxThree.Dispose();
            stmtUserThree.Dispose();
        }
    
        [Test]
        public void Test4ContextsNested() {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            SendTimeEvent("2002-05-01 07:00:00.000");
    
            var eplCtx = "create context NestedContext as " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context InitCtx0 initiated by SupportBean_S0 as s0 terminated after 60 seconds, " +
                    "context InitCtx1 initiated by SupportBean_S1 as s1 terminated after 30 seconds, " +
                    "context InitCtx2 initiated by SupportBean_S2 as s2 terminated after 10 seconds";
            var stmtCtx = _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            var fields = "c1,c2,c3,c4".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx0.s0.p00 as c1, context.InitCtx1.s1.p10 as c2, context.InitCtx2.s2.p20 as c3, Sum(IntPrimitive) as c4 from SupportBean");
            stmtUser.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_2", 2}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_2", 5}});
    
            SendTimeEvent("2002-05-01 08:00:05.000");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_3"));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_2", 9}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_3"));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_2", 14}, new Object[] {"S0_2", "S1_2", "S2_3", 5}, new Object[] {"S0_2", "S1_3", "S2_3", 5}});
    
            SendTimeEvent("2002-05-01 08:00:10.000"); // terminate S2_2 leaf
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_3", 11}, new Object[] {"S0_2", "S1_3", "S2_3", 11}});
    
            SendTimeEvent("2002-05-01 08:00:15.000"); // terminate S0_2/S1_2/S2_3 and S0_2/S1_3/S2_3 leafs
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_4"));
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_2", "S2_4", 8}, new Object[] {"S0_2", "S1_3", "S2_4", 8}});
    
            SendTimeEvent("2002-05-01 08:00:30.000"); // terminate S1_2 branch
    
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(105, "S1_5"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(205, "S2_5"));
            _epService.EPRuntime.SendEvent(new SupportBean("E10", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "S1_3", "S2_5", 10}, new Object[] {"S0_2", "S1_5", "S2_5", 10}});
    
            SendTimeEvent("2002-05-01 08:01:00.000"); // terminate S0_2 branch, only the "8to9" is left
    
            _epService.EPRuntime.SendEvent(new SupportBean("E11", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(6, "S0_6"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(106, "S1_6"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(206, "S2_6"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_6", "S1_6", "S2_6", 12}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(7, "S0_7"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(107, "S1_7"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(207, "S2_7"));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 13));
            Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);
    
            SendTimeEvent("2002-05-01 10:00:00.000"); // terminate all
    
            _epService.EPRuntime.SendEvent(new SupportBean("E14", 14));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent("2002-05-02 08:00:00.000"); // start next day
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(8, "S0_8"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(108, "S1_8"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(208, "S2_8"));
            _epService.EPRuntime.SendEvent(new SupportBean("E15", 15));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_8", "S1_8", "S2_8", 15}});
            
            stmtUser.Stop();
            _epService.EPRuntime.SendEvent(new SupportBean("E16", 16));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
            AgentInstanceAssertionUtil.AssertInstanceCounts(stmtUser.StatementContext, 0);
        }
    
        [Test]
        public void TestTemporalOverlapOverPartition() {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            var eplCtx = "create context NestedContext as " +
                    "context InitCtx initiated by SupportBean_S0(id > 0) as s0 terminated after 10 seconds, " +
                    "context SegmCtx as partition by TheString from SupportBean(IntPrimitive > 0)";
            var stmtCtx = _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            var fields = "c1,c2,c3".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.InitCtx.s0.p00 as c1, context.SegmCtx.key1 as c2, Sum(IntPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_2"));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_2", "E3", 3});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_2", "E4", 4});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_2", "E3", 8});
    
            SendTimeEvent("2002-05-01 08:00:05.000");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0_3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_4"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "E3", 14}, new Object[] {"S0_4", "E3", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"S0_2", "E4", 11}, new Object[] {"S0_4", "E4", 7}});
    
            SendTimeEvent("2002-05-01 08:00:10.000"); // expires first context
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_4", "E3", 14});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_4", "E4", 16});
    
            SendTimeEvent("2002-05-01 08:00:15.000"); // expires second context
    
            _epService.EPRuntime.SendEvent(new SupportBean("Ex", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_5"));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", -10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"S0_5", "E4", 10});
    
            SendTimeEvent("2002-05-01 08:00:25.000"); // expires second context
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void Test3ContextsTemporalOverCategoryOverPartition()
        {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            var eplCtx = "create context NestedContext as " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context ByCat as group IntPrimitive<0 as g1, group IntPrimitive=0 as g2, group IntPrimitive>0 as g3 from SupportBean, " +
                    "context SegmentedByString as partition by TheString from SupportBean";
            var stmtCtx = _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var listener = new SupportUpdateListener();
            var fields = "c1,c2,c3".Split(',');
            var stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, Sum(LongPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            RunAssertion3Contexts(listener, fields, "2002-05-01 09:00:00.000");
    
            stmtCtx.Dispose();
            stmtUser.Dispose();
            
            SendTimeEvent("2002-05-02 08:00:00.000");
    
            // test SODA
            var model = _epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, model.ToEPL());
            var stmtCtxTwo = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplCtx, stmtCtxTwo.Text);
    
            stmtUser = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, Sum(LongPrimitive) as c3 from SupportBean");
            stmtUser.Events += listener.Update;
    
            RunAssertion3Contexts(listener, fields, "2002-05-02 09:00:00.000");
        }
    
        /// <summary>Root: Temporal Sub: Hash </summary>
        [Test]
        public void TestTemporalFixedOverHash() {
            SendTimeEvent("2002-05-01 07:00:00.000");
    
            var stmtCtx = _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context HashedCtx coalesce Hash_code(IntPrimitive) from SupportBean granularity 10 preallocate");
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            var fields = "c1,c2".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "TheString as c1, Count(*) as c2 from SupportBean group by TheString");
            statement.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent("2002-05-01 08:00:00.000"); // start context
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 2L});
    
            SendTimeEvent("2002-05-01 09:00:00.000"); // terminate
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimeEvent("2002-05-02 08:00:00.000"); // start context
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 1L});
        }
    
        /// <summary>Root: Category Sub: Initiated </summary>
        [Test]
        public void TestCategoryOverTemporalOverlapping() {
            SendTimeEvent("2002-05-01 08:00:00.000");
    
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ByCat " +
                    "  group IntPrimitive < 0 and IntPrimitive != -9999 as g1, " +
                    "  group IntPrimitive = 0 as g2, " +
                    "  group IntPrimitive > 0 as g3 from SupportBean, " +
                    "context InitGrd initiated by SupportBean(TheString like 'init%') as sb terminated after 10 seconds");
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            var fields = "c1,c2,c3".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.ByCat.label as c1, context.InitGrd.sb.TheString as c2, Count(*) as c3 from SupportBean");
            statement.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("init_1", -9999));
            _epService.EPRuntime.SendEvent(new SupportBean("X100", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("X101", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("X102", -10));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("init_2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g2", "init_2", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g2", "init_2", 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("init_3", -2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g1", "init_3", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g1", "init_3", 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g1", "init_3", 3L});
    
            SendTimeEvent("2002-05-01 08:11:00.000"); // terminates all
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        /// <summary>Root: Fixed temporal Sub: Partition by string - Root starts deactivated. - With context destroy before statement destroy </summary>
        [Test]
        public void TestFixedTemporalOverPartitioned() {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            SendTimeEvent("2002-05-01 07:00:00.000");
    
            var stmtCtx = _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            var fields = "c1".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select Count(*) as c1 from SupportBean");
            statement.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent("2002-05-01 08:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
    
            // ends EightToNine context
            SendTimeEvent("2002-05-01 09:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            // starts EightToNine context
            SendTimeEvent("2002-05-02 08:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
            
            stmtCtx.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
    
            statement.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
        }
    
        /// <summary>Root: Partition by string Sub: Fixed temporal - Sub starts deactivated. - With statement destroy before context destroy </summary>
        [Test]
        public void TestPartitionedOverFixedTemporal() {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            SendTimeEvent("2002-05-01 07:00:00.000");
    
            var stmtCtx = _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context SegmentedByAString partition by TheString from SupportBean, " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            var listener = new SupportUpdateListener();
            var fields = "c1".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select Count(*) as c1 from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent("2002-05-01 08:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            // ends EightToNine context
            SendTimeEvent("2002-05-01 09:00:00.000");
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);
    
            // starts EightToNine context
            SendTimeEvent("2002-05-02 08:00:00.000");
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{1L});
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
            Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);
    
            statement.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
    
            stmtCtx.Dispose();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
        }

        /// <summary>
        /// Test nested context properties.
        /// Root: Fixed temporal
        ///  Sub: Partition by string
        ///  - fixed temportal starts active
        ///  - starting and stopping statement
        /// </summary>
        [Test]
        public void TestContextProps()
        {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            SendTimeEvent("2002-05-01 08:30:00.000");
    
            var stmtCtx = _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
    
            var listener = new SupportUpdateListener();
            var fields = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select " +
                    "context.EightToNine.name as c0, " +
                    "context.EightToNine.startTime as c1, " +
                    "context.SegmentedByAString.name as c2, " +
                    "context.SegmentedByAString.key1 as c3, " +
                    "context.name as c4, " +
                    "context.id as c5, " +
                    "IntPrimitive as c6," +
                    "count(*) as c7 " +
                    "from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"EightToNine", DateTimeHelper.ParseDefaultMSec("2002-05-01 08:30:00.000"),
                    "SegmentedByAString", "E1",
                    "NestedContext", 0,
                    10, 1L});
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"EightToNine", DateTimeHelper.ParseDefaultMSec("2002-05-01 08:30:00.000"),
                    "SegmentedByAString", "E2",
                    "NestedContext", 1,
                    20, 1L});
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2, 0, 0, 0);
    
            statement.Stop();
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            statement.Start();
            
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"EightToNine", DateTimeHelper.ParseDefaultMSec("2002-05-01 08:30:00.000"),
                    "SegmentedByAString", "E2",
                    "NestedContext", 2,
                    30, 1L});
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(2, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 0, 0, 0);
    
            statement.Dispose();
            stmtCtx.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            Assert.IsFalse(listener.IsInvoked);
            Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
        }
    
        /// <summary>Test late-coming statement. Root: Fixed temporal Sub: Partition by string </summary>
        [Test]
        public void TestLateComingStatement() {
            SendTimeEvent("2002-05-01 08:30:00.000");
    
            var stmtCtx = _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by TheString from SupportBean");
    
            var listenerOne = new SupportUpdateListener();
            var fields = "c0,c1".Split(',');
            var statementOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, Count(*) as c1 from SupportBean");
            statementOne.Events += listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 1L});
    
            var listenerTwo = new SupportUpdateListener();
            var statementTwo = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, sum(IntPrimitive) as c1 from SupportBean");
            statementTwo.Events += listenerTwo.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 2L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 20});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 1L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 30});
    
            var listenerThree = new SupportUpdateListener();
            var statementThree = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, min(IntPrimitive) as c1 from SupportBean");
            statementThree.Events += listenerThree.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 40));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 3L});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 60});
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 40});
    
            statementTwo.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 4L});
            Assert.IsFalse(listenerTwo.IsInvoked);
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 40});
    
            statementOne.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -60));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
            EPAssertionUtil.AssertProps(listenerThree.AssertOneGetNewAndReset(), fields, new Object[]{"E1", -60});
            
            statementThree.Dispose();
        }
    
        private void RunAssertion3Contexts(SupportUpdateListener listener, String[] fields, String subsequentTime) {
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g2", "E1", 10L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 0, 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g2", "E2", 11L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g2", "E1", 22L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g3", "E1", 13L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 14));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g1", "E1", 14L});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 15));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"g1", "E2", 15L});
    
            SendTimeEvent(subsequentTime);
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", -1, 15));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestPartitionWithMultiPropsAndTerm() {
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by TheString, IntPrimitive from SupportBean, " +
                    "context InitiateAndTerm start SupportBean as e1 " +
                    "end SupportBean_S0(id=e1.IntPrimitive and p00=e1.TheString)");
    
            var listenerOne = new SupportUpdateListener();
            var fields = "c0,c1,c2".Split(',');
            var statementOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select TheString as c0, IntPrimitive as c1, Count(LongPrimitive) as c2 from SupportBean \n" +
                    "output last when terminated");
            statementOne.Events += listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 1));
            Assert.IsFalse(listenerOne.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 0, 2L});
        }
    
        private Object MakeEvent(String theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private Object MakeEvent(String theString, int intPrimitive, long longPrimitive, bool boolPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.BoolPrimitive = boolPrimitive;
            return bean;
        }
    
        private void SendTimeEvent(String time) {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService)
        {
            return ((EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin);
        }

        public class MySelectorFilteredNested : ContextPartitionSelectorFiltered
        {
            private readonly Object[] _pathMatch;
    
            private readonly IList<Object[]> _paths = new List<Object[]>();
            private readonly LinkedHashSet<int> _cpids = new LinkedHashSet<int>();
    
            public MySelectorFilteredNested(Object[] pathMatch) {
                _pathMatch = pathMatch;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var nested = (ContextPartitionIdentifierNested) contextPartitionIdentifier;
                if (_pathMatch == null && _cpids.Contains(nested.ContextPartitionId.Value)) {
                    throw new Exception("Already exists context id: " + nested.ContextPartitionId);
                }
                _cpids.Add(nested.ContextPartitionId.Value);
    
                var first = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0];
                var second = (ContextPartitionIdentifierCategory) nested.Identifiers[1];
    
                var extract = new Object[2];
                extract[0] = ((EventBean) first.Properties.Get("s0")).Get("p00");
                extract[1] = second.Label;
                _paths.Add(extract);
    
                return _paths != null && Collections.AreEqual(_pathMatch, extract);
            }
        }

        public class TestEvent
        {
            public TestEvent(int id, int time)
            {
                Id = id;
                Time = time;
            }

            public int Time { get; private set; }

            public int Id { get; private set; }
        }

        public class EndEvent
        {
            public EndEvent(int id)
            {
                Id = id;
            }

            public int Id { get; private set; }
        }
    }
}
