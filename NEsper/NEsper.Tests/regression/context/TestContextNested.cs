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
        [Test]
	    public void TestNestedContextWithFilterUDF()
        {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
	                "customEnabled", typeof(TestContextNested).FullName, "CustomMatch", FilterOptimizable.ENABLED);
	        epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
	                "customDisabled", typeof(TestContextNested).FullName, "CustomMatch", FilterOptimizable.DISABLED);
	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context ACtx initiated by SupportBean_S0 as s0 terminated after 24 hours, " +
	                "context BCtx initiated by SupportBean_S1 as s1 terminated after 1 hour");
	        var stmt = epService.EPAdministrator.CreateEPL("context NestedContext select * " +
	                "from SupportBean(" +
	                "customEnabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id)" +
	                " and " +
	                "customDisabled(TheString, context.ACtx.s0.p00, IntPrimitive, context.BCtx.s1.id))");
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(2, "S1"));
	        epService.EPRuntime.SendEvent(new SupportBean("X", -1));
	        Assert.IsTrue(listener.IsInvoked);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    public static bool CustomMatch(string theString, string p00, int intPrimitive, int s1id) {
	        Assert.AreEqual("X", theString);
	        Assert.AreEqual("S0", p00);
	        Assert.AreEqual(-1, intPrimitive);
	        Assert.AreEqual(2, s1id);
	        return true;
	    }

        [Test]
	    public void TestIterateTargetedCP() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
	                "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean");

	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmt = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
	                "select context.ACtx.s0.p00 as c0, context.BCtx.label as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean.win:length(5) group by TheString");

	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
	        epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
	        epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
	        epService.EPRuntime.SendEvent(new SupportBean("E1", 2));

	        var expectedAll = new object[][]
	        {
	            new object[] {"S0_1", "grp1", "E2", -1}, 
                new object[] {"S0_1", "grp3", "E3", 5}, 
                new object[] {"S0_1", "grp3", "E1", 3}, 
                new object[] {"S0_2", "grp3", "E1", 2}
	        };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expectedAll);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(ContextPartitionSelectorAll.INSTANCE), stmt.GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE), fields, expectedAll);
            var allIds = new SupportSelectorById(CompatExtensions.AsHashSet<int>(0, 1, 2, 3, 4, 5));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(allIds), stmt.GetSafeEnumerator(allIds), fields, expectedAll);

	        // test iterator targeted
	        ContextPartitionSelector firstOne = new SupportSelectorFilteredInitTerm("S0_2");
	        ContextPartitionSelector secondOne = new SupportSelectorCategory(Collections.SingletonList("grp3"));
	        var nestedSelector = new SupportSelectorNested(Collections.SingletonList(new ContextPartitionSelector[] {firstOne, secondOne}));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelector), stmt.GetSafeEnumerator(nestedSelector), fields, new object[][] { new object[] { "S0_2", "grp3", "E1", 2 } });

	        ContextPartitionSelector firstTwo = new SupportSelectorFilteredInitTerm("S0_1");
	        ContextPartitionSelector secondTwo = new SupportSelectorCategory(Collections.SingletonList("grp1"));
	        var nestedSelectorTwo = new SupportSelectorNested(Collections.List(
                new ContextPartitionSelector[] {firstOne, secondOne},
                new ContextPartitionSelector[] {firstTwo, secondTwo}
            ));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(nestedSelectorTwo), stmt.GetSafeEnumerator(nestedSelectorTwo), fields, new object[][] { new object[] { "S0_2", "grp3", "E1", 2 }, new object[] { "S0_1", "grp1", "E2", -1 } });

	        // test iterator filtered : not supported for nested
	        try {
	            var filtered = new MySelectorFilteredNested(new object[] {"S0_2", "grp3"});
	            stmt.GetEnumerator(filtered);
	            Assert.Fail();
	        }
	        catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorById, ContextPartitionSelectorNested] interfaces but received com."), "message: " + ex.Message);
	        }

	        epService.EPAdministrator.DestroyAllStatements();

	        // test 3 nesting levels and targeted
	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context ACtx group by IntPrimitive < 0 as i1, group by IntPrimitive = 0 as i2, group by IntPrimitive > 0 as i3 from SupportBean," +
	                "context BCtx group by LongPrimitive < 0 as l1, group by LongPrimitive = 0 as l2, group by LongPrimitive > 0 as l3 from SupportBean," +
	                "context CCtx group by BoolPrimitive = true as b1, group by BoolPrimitive = false as b2 from SupportBean");

	        var fieldsSelect = "c0,c1,c2,c3".Split(',');
	        var stmtSelect = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context NestedContext " +
	                "select context.ACtx.label as c0, context.BCtx.label as c1, context.CCtx.label as c2, count(*) as c3 from SupportBean.win:length(5) having count(*) > 0");

	        epService.EPRuntime.SendEvent(MakeEvent("E1", -1, 10L, true));
	        epService.EPRuntime.SendEvent(MakeEvent("E2", 2, -10L, false));
	        epService.EPRuntime.SendEvent(MakeEvent("E3", 1, 11L, false));
	        epService.EPRuntime.SendEvent(MakeEvent("E4", 0, 0L, true));
	        epService.EPRuntime.SendEvent(MakeEvent("E5", -1, 10L, false));
	        epService.EPRuntime.SendEvent(MakeEvent("E6", -1, 10L, true));

	        var expectedRows = new object[][] {
	                new object[] { "i1", "l3", "b1", 2L},
	                new object[] { "i3", "l1", "b2", 1L},
	                new object[] { "i1", "l3", "b2", 1L},
	                new object[] { "i2", "l2", "b1", 1L},
	                new object[] { "i3", "l3", "b2", 1L},
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
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(nestedSelectorSelect), stmtSelect.GetSafeEnumerator(nestedSelectorSelect), fieldsSelect, new object[][] { new object[] { "i3", "l1", "b2", 1L } });

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestInvalid() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        string epl;

	        // invalid same sub-context name twice
	        epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)";
	        TryInvalid(epService, epl, "Error starting statement: Context by name 'EightToNine' has already been declared within nested context 'ABC' [");

	        // validate statement added to nested context
	        epl = "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context PartCtx as partition by TheString from SupportBean";
	        epService.EPAdministrator.CreateEPL(epl);
	        epl = "context ABC select * from SupportBean_S0";
	        TryInvalid(epService, epl, "Error starting statement: Segmented context 'PartCtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestIterator() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context SegByString partition by TheString from SupportBean");

	        var listener = new SupportUpdateListener();
	        var fields = "c0,c1,c2".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.EightToNine.startTime as c0, context.SegByString.key1 as c1, IntPrimitive as c2 from SupportBean.win:keepall()");
	        stmtUser.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        var expected = new object[][]
	        {
	            new object[] { DateTimeParser.ParseDefaultMSec("2002-05-1T8:00:00.000"), "E1", 1 }
	        };
	        EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            expected = new object[][]
            {
                new object[] { DateTimeParser.ParseDefaultMSec("2002-05-1T8:00:00.000"), "E1", 1 },
                new object[] { DateTimeParser.ParseDefaultMSec("2002-05-1T8:00:00.000"), "E1", 2 }
            };
	        EPAssertionUtil.AssertPropsPerRow(stmtUser.GetEnumerator(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(stmtUser.GetSafeEnumerator(), fields, expected);

	        // extract path
            if (GetSpi(epService).IsSupportsExtract)
            {
                GetSpi(epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestPartitionedWithFilter() {

	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        RunAssertionPartitionedNonOverlap(epService);
	        RunAssertionPartitionOverlap(epService);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestNestingFilterCorrectness() {

	        var epServiceNoIsolation = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epServiceNoIsolation, this.GetType(), GetType().FullName); }
	        RunAssertionNestingFilterCorrectness(epServiceNoIsolation, false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }

	        var epServiceWithIsolation = AllocateEngine(true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epServiceWithIsolation, this.GetType(), GetType().FullName); }
	        RunAssertionNestingFilterCorrectness(epServiceWithIsolation, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    private void RunAssertionNestingFilterCorrectness(EPServiceProvider epService, bool isolationAllowed) {
	        string eplContext;
	        var eplSelect = "context TheContext select count(*) from SupportBean";
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

	    private void AssertFilters(EPServiceProvider epService, bool allowIsolation, string expected, EPStatementSPI spiStmt) {
	        if (!allowIsolation) {
	            return;
	        }
	        var spi = (EPServiceProviderSPI) epService;
	        var filterSPI = (FilterServiceSPI) spi.FilterService;
            if (!filterSPI.IsSupportsTakeApply)
            {
                return;
            }

	        var set = filterSPI.Take(Collections.SingletonList(spiStmt.StatementId));
	        Assert.AreEqual(expected, set.ToString());
	        filterSPI.Apply(set);
	    }

	    private void RunAssertionPartitionOverlap(EPServiceProvider epService) {
	        epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
	        epService.EPAdministrator.Configuration.AddEventType(typeof(EndEvent));
	        epService.EPAdministrator.CreateEPL("@Audit('pattern-instances') create context TheContext"
	                + " context CtxSession partition by id from TestEvent, "
	                + " context CtxStartEnd start TestEvent as te end EndEvent(id=te.id)");
	        var stmt = epService.EPAdministrator.CreateEPL(
	                "context TheContext select firstEvent from TestEvent.std:firstevent() as firstEvent"
	                        + " inner join TestEvent.std:lastevent() as lastEvent");
	        var supportSubscriber = new SupportSubscriber();
	        stmt.Subscriber = supportSubscriber;

	        for (var i = 0; i < 2; i++) {
	            epService.EPRuntime.SendEvent(new TestEvent(1, 5));
	            epService.EPRuntime.SendEvent(new TestEvent(2, 10));
	            epService.EPRuntime.SendEvent(new EndEvent(1));

	            supportSubscriber.Reset();
	            epService.EPRuntime.SendEvent(new TestEvent(2, 15));
	            Assert.AreEqual(10, (((TestEvent) supportSubscriber.AssertOneGetNewAndReset()) .Time));

	            epService.EPRuntime.SendEvent(new EndEvent(1));
	            epService.EPRuntime.SendEvent(new EndEvent(2));
	        }
	    }

	    private void RunAssertionPartitionedNonOverlap(EPServiceProvider epService) {
	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        var eplCtx = "create context NestedContext as " +
	                "context SegByString as partition by TheString from SupportBean(IntPrimitive > 0), " +
	                "context InitCtx initiated by SupportBean_S0 as s0 terminated after 60 seconds";
	        var stmtCtx = epService.EPAdministrator.CreateEPL(eplCtx);

	        var listener = new SupportUpdateListener();
	        var fields = "c0,c1,c2".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.InitCtx.s0.p00 as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString");
	        stmtUser.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
	        epService.EPRuntime.SendEvent(new SupportBean("E1", -5));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_1", "E1", 2 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "E2", 4 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_1", "E1", 8 }, new object[] { "S0_2", "E1", 6 } });
	    }

        [Test]
	    public void TestCategoryOverPatternInitiated() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        var eplCtx = "create context NestedContext as " +
	                "context ByCat as group IntPrimitive < 0 as g1, group IntPrimitive > 0 as g2, group IntPrimitive = 0 as g3 from SupportBean, " +
	                "context InitCtx as initiated by pattern [every a=SupportBean_S0 -> b=SupportBean_S1(id = a.id)] terminated after 10 sec";
	        epService.EPAdministrator.CreateEPL(eplCtx);

	        var listener = new SupportUpdateListener();
	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.ByCat.label as c0, context.InitCtx.a.p00 as c1, context.InitCtx.b.p10 as c2, sum(IntPrimitive) as c3 from SupportBean group by TheString");
	        stmtUser.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_1"));
	        epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

	        epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g2", "S0_1", "S1_2", 3 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E4", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g1", "S0_1", "S1_2", -2 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g3", "S0_1", "S1_2", 0 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g2", "S0_1", "S1_2", 8 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g2", "S0_1", "S1_2", 6 } });

	        epService.EPRuntime.SendEvent(new SupportBean_S0(102, "S0_3"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(102, "S1_3"));

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g2", "S0_1", "S1_2", 15 }, new object[] { "g2", "S0_3", "S1_3", 7 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:10.000");

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(104, "S0_4"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(104, "S1_4"));

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "g2", "S0_4", "S1_4", 9 } });

            if (GetSpi(epService).IsSupportsExtract)
            {
                GetSpi(epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestSingleEventTriggerNested() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        // Test partitioned context
	        //
	        var eplCtxOne = "create context NestedContext as " +
	                "context SegByString as partition by TheString from SupportBean, " +
	                "context SegByInt as partition by IntPrimitive from SupportBean, " +
	                "context SegByLong as partition by LongPrimitive from SupportBean ";
	        var stmtCtxOne = epService.EPAdministrator.CreateEPL(eplCtxOne);

	        var listenerOne = new SupportUpdateListener();
	        var fieldsOne = "c0,c1,c2,c3".Split(',');
	        var stmtUserOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.SegByString.key1 as c0, context.SegByInt.key1 as c1, context.SegByLong.key1 as c2, count(*) as c3 from SupportBean");
	        stmtUserOne.AddListener(listenerOne);

	        epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][] { new object[] { "E1", 10, 100L, 1L } });

	        epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][] { new object[] { "E2", 10, 100L, 1L } });

	        epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][] { new object[] { "E1", 11, 100L, 1L } });

	        epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 101));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][] { new object[] { "E1", 10, 101L, 1L } });

	        epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetAndResetLastNewData(), fieldsOne, new object[][] { new object[] { "E1", 10, 100L, 2L } });

	        stmtCtxOne.Dispose();
	        stmtUserOne.Dispose();

	        // Test partitioned context
	        //
	        var eplCtxTwo = "create context NestedContext as " +
	                "context HashOne coalesce by hash_code(TheString) from SupportBean granularity 10, " +
	                "context HashTwo coalesce by hash_code(IntPrimitive) from SupportBean granularity 10";
	        var stmtCtxTwo = epService.EPAdministrator.CreateEPL(eplCtxTwo);

	        var listenerTwo = new SupportUpdateListener();
	        var fieldsTwo = "c1,c2".Split(',');
	        var stmtUserTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "TheString as c1, count(*) as c2 from SupportBean");
	        stmtUserTwo.AddListener(listenerTwo);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][] { new object[] { "E1", 1L } });

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][] { new object[] { "E2", 1L } });

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listenerTwo.GetAndResetLastNewData(), fieldsTwo, new object[][] { new object[] { "E1", 2L } });

	        stmtCtxTwo.Dispose();
	        stmtUserTwo.Dispose();

	        // Test partitioned context
	        //
	        var eplCtxThree = "create context NestedContext as " +
	                "context InitOne initiated by SupportBean(TheString like 'I%') as sb0 terminated after 10 sec, " +
	                "context InitTwo initiated by SupportBean(IntPrimitive > 0) as sb1 terminated after 10 sec";
	        var stmtCtxThree = epService.EPAdministrator.CreateEPL(eplCtxThree);

	        var listenerThree = new SupportUpdateListener();
	        var fieldsThree = "c1,c2".Split(',');
	        var stmtUserThree = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "TheString as c1, count(*) as c2 from SupportBean");
	        stmtUserThree.AddListener(listenerThree);

	        epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            EPAssertionUtil.AssertPropsPerRow(listenerThree.GetAndResetLastNewData(), fieldsThree, new object[][] { new object[] { "I1", 1L } });

	        stmtCtxThree.Dispose();
	        stmtUserThree.Dispose();

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void Test4ContextsNested() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        var spi = (EPServiceProviderSPI) epService;
	        var filterSPI = (FilterServiceSPI) spi.FilterService;
	        SendTimeEvent(epService, "2002-05-1T7:00:00.000");

	        var eplCtx = "create context NestedContext as " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context InitCtx0 initiated by SupportBean_S0 as s0 terminated after 60 seconds, " +
	                "context InitCtx1 initiated by SupportBean_S1 as s1 terminated after 30 seconds, " +
	                "context InitCtx2 initiated by SupportBean_S2 as s2 terminated after 10 seconds";
	        var stmtCtx = epService.EPAdministrator.CreateEPL(eplCtx);

	        var listener = new SupportUpdateListener();
	        var fields = "c1,c2,c3,c4".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.InitCtx0.s0.p00 as c1, context.InitCtx1.s1.p10 as c2, context.InitCtx2.s2.p20 as c3, sum(IntPrimitive) as c4 from SupportBean");
	        stmtUser.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_1"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_1"));
	        epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_2"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(100, "S1_2"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(200, "S2_2"));

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_2", 2 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_2", 5 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:05.000");

	        epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_3"));
	        epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_2", 9 } });

	        epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_3"));
	        epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_2", 14 }, new object[] { "S0_2", "S1_2", "S2_3", 5 }, new object[] { "S0_2", "S1_3", "S2_3", 5 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:10.000"); // terminate S2_2 leaf

	        epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_3", 11 }, new object[] { "S0_2", "S1_3", "S2_3", 11 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:15.000"); // terminate S0_2/S1_2/S2_3 and S0_2/S1_3/S2_3 leafs

	        epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S2(201, "S2_4"));
	        epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_2", "S2_4", 8 }, new object[] { "S0_2", "S1_3", "S2_4", 8 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:30.000"); // terminate S1_2 branch

	        epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S1(105, "S1_5"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(205, "S2_5"));
	        epService.EPRuntime.SendEvent(new SupportBean("E10", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "S1_3", "S2_5", 10 }, new object[] { "S0_2", "S1_5", "S2_5", 10 } });

	        SendTimeEvent(epService, "2002-05-1T8:01:00.000"); // terminate S0_2 branch, only the "8to9" is left

	        epService.EPRuntime.SendEvent(new SupportBean("E11", 11));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(6, "S0_6"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(106, "S1_6"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(206, "S2_6"));
	        epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_6", "S1_6", "S2_6", 12 } });

	        epService.EPRuntime.SendEvent(new SupportBean_S0(7, "S0_7"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(107, "S1_7"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(207, "S2_7"));
	        epService.EPRuntime.SendEvent(new SupportBean("E3", 13));
	        Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);

	        SendTimeEvent(epService, "2002-05-1T10:00:00.000"); // terminate all

	        epService.EPRuntime.SendEvent(new SupportBean("E14", 14));
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimeEvent(epService, "2002-05-2T8:00:00.000"); // start next day

	        epService.EPRuntime.SendEvent(new SupportBean_S0(8, "S0_8"));
	        epService.EPRuntime.SendEvent(new SupportBean_S1(108, "S1_8"));
	        epService.EPRuntime.SendEvent(new SupportBean_S2(208, "S2_8"));
	        epService.EPRuntime.SendEvent(new SupportBean("E15", 15));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_8", "S1_8", "S2_8", 15 } });

	        stmtUser.Stop();
	        epService.EPRuntime.SendEvent(new SupportBean("E16", 16));
	        Assert.IsFalse(listener.IsInvoked);
	        Assert.AreEqual(0, filterSPI.FilterCountApprox);
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(stmtUser.StatementContext, 0);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestTemporalOverlapOverPartition() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        var eplCtx = "create context NestedContext as " +
	                "context InitCtx initiated by SupportBean_S0(id > 0) as s0 terminated after 10 seconds, " +
	                "context SegmCtx as partition by TheString from SupportBean(IntPrimitive > 0)";
	        var stmtCtx = epService.EPAdministrator.CreateEPL(eplCtx);

	        var listener = new SupportUpdateListener();
	        var fields = "c1,c2,c3".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.InitCtx.s0.p00 as c1, context.SegmCtx.key1 as c2, sum(IntPrimitive) as c3 from SupportBean");
	        stmtUser.AddListener(listener);

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

	        SendTimeEvent(epService, "2002-05-1T8:00:05.000");

	        epService.EPRuntime.SendEvent(new SupportBean_S0(-2, "S0_3"));
	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_4"));

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "E3", 14 }, new object[] { "S0_4", "E3", 6 } });

	        epService.EPRuntime.SendEvent(new SupportBean("E4", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "S0_2", "E4", 11 }, new object[] { "S0_4", "E4", 7 } });

	        SendTimeEvent(epService, "2002-05-1T8:00:10.000"); // expires first context

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_4", "E3", 14});

	        epService.EPRuntime.SendEvent(new SupportBean("E4", 9));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_4", "E4", 16});

	        SendTimeEvent(epService, "2002-05-1T8:00:15.000"); // expires second context

	        epService.EPRuntime.SendEvent(new SupportBean("Ex", 1));
	        epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_5"));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
	        epService.EPRuntime.SendEvent(new SupportBean("E4", -10));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S0_5", "E4", 10});

	        SendTimeEvent(epService, "2002-05-1T8:00:25.000"); // expires second context

	        epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
	        Assert.IsFalse(listener.IsInvoked);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void Test3ContextsTermporalOverCategoryOverPartition() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        var eplCtx = "create context NestedContext as " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context ByCat as group IntPrimitive<0 as g1, group IntPrimitive=0 as g2, group IntPrimitive>0 as g3 from SupportBean, " +
	                "context SegmentedByString as partition by TheString from SupportBean";
	        var stmtCtx = epService.EPAdministrator.CreateEPL(eplCtx);

	        var listener = new SupportUpdateListener();
	        var fields = "c1,c2,c3".Split(',');
	        var stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(LongPrimitive) as c3 from SupportBean");
	        stmtUser.AddListener(listener);

	        RunAssertion3Contexts(epService, listener, fields, "2002-05-1T9:00:00.000");

	        stmtCtx.Dispose();
            stmtUser.Dispose();

	        SendTimeEvent(epService, "2002-05-2T8:00:00.000");

	        // test SODA
	        var model = epService.EPAdministrator.CompileEPL(eplCtx);
	        Assert.AreEqual(eplCtx, model.ToEPL());
	        var stmtCtxTwo = epService.EPAdministrator.Create(model);
	        Assert.AreEqual(eplCtx, stmtCtxTwo.Text);

	        stmtUser = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(LongPrimitive) as c3 from SupportBean");
	        stmtUser.AddListener(listener);

	        RunAssertion3Contexts(epService, listener, fields, "2002-05-2T9:00:00.000");

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Root: Temporal
	    /// Sub: Hash
	    /// </summary>
        [Test]
	    public void TestTemporalFixedOverHash() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }
	        var spi = (EPServiceProviderSPI) epService;

	        SendTimeEvent(epService, "2002-05-1T7:00:00.000");

	        var stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context HashedCtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10 preallocate");
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);

	        var listener = new SupportUpdateListener();
	        var fields = "c1,c2".Split(',');
	        var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "TheString as c1, count(*) as c2 from SupportBean group by TheString");
	        statement.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000"); // start context

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});

	        SendTimeEvent(epService, "2002-05-1T9:00:00.000"); // terminate

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimeEvent(epService, "2002-05-2T8:00:00.000"); // start context

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Root: Category
	    /// Sub: Initiated
	    /// </summary>
        [Test]
	    public void TestCategoryOverTemporalOverlapping() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }
	        var spi = (EPServiceProviderSPI) epService;

	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");

	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context ByCat " +
	                "  group IntPrimitive < 0 and IntPrimitive != -9999 as g1, " +
	                "  group IntPrimitive = 0 as g2, " +
	                "  group IntPrimitive > 0 as g3 from SupportBean, " +
	                "context InitGrd initiated by SupportBean(TheString like 'init%') as sb terminated after 10 seconds");
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);

	        var listener = new SupportUpdateListener();
	        var fields = "c1,c2,c3".Split(',');
	        var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.ByCat.label as c1, context.InitGrd.sb.TheString as c2, count(*) as c3 from SupportBean");
	        statement.AddListener(listener);

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

	        SendTimeEvent(epService, "2002-05-1T8:11:00.000"); // terminates all

	        epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
	        Assert.IsFalse(listener.IsInvoked);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Root: Fixed temporal
	    /// Sub: Partition by string
	    /// - Root starts deactivated.
	    /// - With context destroy before statement destroy
	    /// </summary>
        [Test]
	    public void TestFixedTemporalOverPartitioned() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }
	        var spi = (EPServiceProviderSPI) epService;

	        var filterSPI = (FilterServiceSPI) spi.FilterService;
	        SendTimeEvent(epService, "2002-05-1T7:00:00.000");

	        var stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context SegmentedByAString partition by TheString from SupportBean");
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);

	        var listener = new SupportUpdateListener();
	        var fields = "c1".Split(',');
	        var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select count(*) as c1 from SupportBean");
	        statement.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.IsFalse(listener.IsInvoked);
	        Assert.AreEqual(0, filterSPI.FilterCountApprox);
	        Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);

	        // starts EightToNine context
	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");
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
	        SendTimeEvent(epService, "2002-05-1T9:00:00.000");
	        Assert.AreEqual(0, filterSPI.FilterCountApprox);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        Assert.IsFalse(listener.IsInvoked);

	        // starts EightToNine context
	        SendTimeEvent(epService, "2002-05-2T8:00:00.000");
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Root: Partition by string
	    /// Sub: Fixed temporal
	    /// - Sub starts deactivated.
	    /// - With statement destroy before context destroy
	    /// </summary>
        [Test]
	    public void TestPartitionedOverFixedTemporal() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }
	        var spi = (EPServiceProviderSPI) epService;

	        var filterSPI = (FilterServiceSPI) spi.FilterService;
	        SendTimeEvent(epService, "2002-05-1T7:00:00.000");

	        var stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context SegmentedByAString partition by TheString from SupportBean, " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)");
	        Assert.AreEqual(0, filterSPI.FilterCountApprox);
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);

	        var listener = new SupportUpdateListener();
	        var fields = "c1".Split(',');
	        var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select count(*) as c1 from SupportBean");
	        statement.AddListener(listener);
	        Assert.AreEqual(1, filterSPI.FilterCountApprox);
	        Assert.AreEqual(0, spi.SchedulingService.ScheduleHandleCount);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        Assert.IsFalse(listener.IsInvoked);
	        Assert.AreEqual(1, filterSPI.FilterCountApprox);
	        Assert.AreEqual(1, spi.SchedulingService.ScheduleHandleCount);

	        // starts EightToNine context
	        SendTimeEvent(epService, "2002-05-1T8:00:00.000");
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
	        SendTimeEvent(epService, "2002-05-1T9:00:00.000");
	        Assert.AreEqual(1, filterSPI.FilterCountApprox);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        Assert.IsFalse(listener.IsInvoked);
	        Assert.AreEqual(2, spi.SchedulingService.ScheduleHandleCount);

	        // starts EightToNine context
	        SendTimeEvent(epService, "2002-05-2T8:00:00.000");
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

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Test nested context properties.
	    /// Root: Fixed temporal
	    /// Sub: Partition by string
	    /// - fixed temportal starts active
	    /// - starting and stopping statement
	    /// </summary>
        [Test]
	    public void TestContextProps() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }
	        var spi = (EPServiceProviderSPI) epService;

	        var filterSPI = (FilterServiceSPI) spi.FilterService;
	        SendTimeEvent(epService, "2002-05-1T8:30:00.000");

	        var stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context SegmentedByAString partition by TheString from SupportBean");

	        var listener = new SupportUpdateListener();
	        var fields = "c0,c1,c2,c3,c4,c5,c6".Split(',');
	        var statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select " +
	                "context.EightToNine.name as c0, " +
	                "context.EightToNine.startTime as c1, " +
	                "context.SegmentedByAString.name as c2, " +
	                "context.SegmentedByAString.key1 as c3, " +
	                "context.name as c4, " +
	                "IntPrimitive as c5," +
	                "count(*) as c6 " +
	                "from SupportBean");
	        statement.AddListener(listener);
	        Assert.AreEqual(1, filterSPI.FilterCountApprox);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T8:30:00.000"),
	                "SegmentedByAString", "E1",
	                "NestedContext",
	                10, 1L});
	        Assert.AreEqual(2, filterSPI.FilterCountApprox);

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T8:30:00.000"),
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
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EightToNine", DateTimeParser.ParseDefaultMSec("2002-05-1T8:30:00.000"),
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

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    /// <summary>
	    /// Test late-coming statement.
	    /// Root: Fixed temporal
	    /// Sub: Partition by string
	    /// </summary>
        [Test]
	    public void TestLateComingStatement() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        SendTimeEvent(epService, "2002-05-1T8:30:00.000");

	        var stmtCtx = epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
	                "context SegmentedByAString partition by TheString from SupportBean");

	        var listenerOne = new SupportUpdateListener();
	        var fields = "c0,c1".Split(',');
	        var statementOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, count(*) as c1 from SupportBean");
	        statementOne.AddListener(listenerOne);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});

	        var listenerTwo = new SupportUpdateListener();
	        var statementTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, sum(IntPrimitive) as c1 from SupportBean");
	        statementTwo.AddListener(listenerTwo);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
	        EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2L});
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20});

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 30));
	        EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"E2", 30});

	        var listenerThree = new SupportUpdateListener();
	        var statementThree = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext select TheString as c0, min(IntPrimitive) as c1 from SupportBean");
	        statementThree.AddListener(listenerThree);

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

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    private void RunAssertion3Contexts(EPServiceProvider epService, SupportUpdateListener listener, string[] fields, string subsequentTime) {

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

        [Test]
	    public void TestPartitionWithMultiPropsAndTerm() {
	        var epService = AllocateEngine(false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName); }

	        epService.EPAdministrator.CreateEPL("create context NestedContext " +
	                "context PartitionedByKeys partition by TheString, IntPrimitive from SupportBean, " +
	                "context InitiateAndTerm start SupportBean as e1 " +
	                "end SupportBean_S0(id=e1.IntPrimitive and p00=e1.TheString)");

	        var listenerOne = new SupportUpdateListener();
	        var fields = "c0,c1,c2".Split(',');
	        var statementOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context NestedContext " +
	                "select TheString as c0, IntPrimitive as c1, count(LongPrimitive) as c2 from SupportBean \n" +
	                "output last when terminated");
	        statementOne.AddListener(listenerOne);

	        epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
	        epService.EPRuntime.SendEvent(MakeEvent("E1", 0, 10));
	        epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 1));
	        Assert.IsFalse(listenerOne.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
	        EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 0, 2L});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
        public void TestNestedOverlappingAndPattern()
        {
            EPServiceProvider epService = AllocateEngine(false);
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by theString from SupportBean, " +
                    "context TimedImmediate initiated @now and pattern[every timer:interval(10)] terminated after 10 seconds");
            RunAssertion(epService);
        }

        [Test]
        public void TestNestedNonOverlapping()
        {
            EPServiceProvider epService = AllocateEngine(false);
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context PartitionedByKeys partition by theString from SupportBean, " +
                    "context TimedImmediate start @now end after 10 seconds");
            RunAssertion(epService);
        }

        private void RunAssertion(EPServiceProvider epService)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            SupportUpdateListener listenerOne = new SupportUpdateListener();
            String[] fields = "c0,c1".SplitCsv();
            EPStatement statementOne = epService.EPAdministrator.CreateEPL("context NestedContext " +
                    "select theString as c0, sum(intPrimitive) as c1 from SupportBean \n" +
                    "output last when terminated");
            statementOne.AddListener(listenerOne);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E1", 1 }, new Object[] { "E2", 2 } }, null);
            listenerOne.Reset();

            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
            EPAssertionUtil.AssertPropsPerRow(listenerOne.GetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E1", 3 }, new Object[] { "E3", 4 } }, null);
        }

	    private object MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        return bean;
	    }

	    private object MakeEvent(string theString, int intPrimitive, long longPrimitive, bool boolPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        bean.BoolPrimitive = boolPrimitive;
	        return bean;
	    }

	    private void SendTimeEvent(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
	    }

	    private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
	        return ((EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin);
	    }

	    public class MySelectorFilteredNested : ContextPartitionSelectorFiltered
        {
	        private readonly object[] _pathMatch;

	        private readonly IList<object[]> _paths = new List<object[]>();
	        private readonly LinkedHashSet<int?> _cpids = new LinkedHashSet<int?>();

	        public MySelectorFilteredNested(object[] pathMatch) {
	            _pathMatch = pathMatch;
	        }

	        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
	            var nested = (ContextPartitionIdentifierNested) contextPartitionIdentifier;
	            if (_pathMatch == null && _cpids.Contains(nested.ContextPartitionId)) {
	                throw new Exception("Already exists context id: " + nested.ContextPartitionId);
	            }
	            _cpids.Add(nested.ContextPartitionId);

	            var first = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0];
	            var second = (ContextPartitionIdentifierCategory) nested.Identifiers[1];

	            var extract = new object[2];
	            extract[0] = ((EventBean) first.Properties.Get("s0")).Get("p00");
	            extract[1] = second.Label;
	            _paths.Add(extract);

	            return _paths != null && Collections.AreEqual(_pathMatch, extract);
	        }
	    }

        [Serializable]
	    public class TestEvent {
            public int Time { get; private set; }
            public int Id { get; private set; }
            public TestEvent(int id, int time)
            {
	            Id = id;
	            Time = time;
	        }
	    }

        [Serializable]
	    public class EndEvent {
            public int Id { get; private set; }
            public EndEvent(int id)
            {
	            Id = id;
	        }
	    }

	    private EPServiceProvider AllocateEngine(bool allowIsolated)
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        configuration.AddEventType<SupportBean_S0>();
	        configuration.AddEventType<SupportBean_S1>();
	        configuration.AddEventType("SupportBean_S2", typeof(SupportBean_S2));
	        configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
	        configuration.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
	        var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        return epService;
	    }

	    private void TryInvalid(EPServiceProvider epService, string epl, string expected) {
	        try {
	            epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            if (!ex.Message.StartsWith(expected)) {
	                throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
	            }
	            Assert.IsTrue(expected.Trim().Length != 0);
	        }
	    }
	}
} // end of namespace
