///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
	public class TestContextInitTerm
    {
	    private EPServiceProvider _epService;
	    private EPServiceProviderSPI _spi;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        configuration.AddEventType<SupportBean_S0>();
	        configuration.AddEventType<SupportBean_S1>();
	        configuration.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        _spi = (EPServiceProviderSPI) _epService;
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestStartZeroInitiatedNow() {
	        var fieldsOne = "c0,c1".Split(',');

	        // test start-after with immediate start
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var contextExpr = "create context CtxPerId start after 0 sec end after 60 sec";
	        _epService.EPAdministrator.CreateEPL(contextExpr);
	        var stream = _epService.EPAdministrator.CreateEPL("context CtxPerId select TheString as c0, IntPrimitive as c1 from SupportBean");
	        stream.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E1", 1});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(59999));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E2", 2});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestPatternIntervalZeroInitiatedNow()
        {
	        var fieldsOne = "c0,c1".Split(',');

	        // test initiated-by pattern with immediate start
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000));
	        var contextExprTwo =  "create context CtxPerId initiated by pattern [timer:interval(0) or every timer:interval(1 min)] terminated after 60 sec";
	        _epService.EPAdministrator.CreateEPL(contextExprTwo);
	        var streamTwo = _epService.EPAdministrator.CreateEPL("context CtxPerId select TheString as c0, sum(IntPrimitive) as c1 from SupportBean");
	        streamTwo.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E1", 10});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000+59999));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E2", 30});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000+60000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsOne, new object[] {"E3", 4});
	    }

        [Test]
	    public void TestPatternInclusion() {
	        var fields = "TheString,IntPrimitive".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var contextExpr =  "create context CtxPerId initiated by pattern [every-distinct (a.TheString, 10 sec) a=SupportBean]@Inclusive terminated after 10 sec ";
	        _epService.EPAdministrator.CreateEPL(contextExpr);
	        var streamExpr = "context CtxPerId select * from SupportBean(TheString = context.a.TheString) output last when terminated";
	        var stream = _epService.EPAdministrator.CreateEPL(streamExpr);
	        stream.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 3});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 4});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(16100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 6));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20099));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 5});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(26100-1));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(26100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 6});

	        _epService.EPAdministrator.DestroyAllStatements();

	        // test multiple pattern with multiple events
	        var contextExprMulti =  "create context CtxPerId initiated by pattern [every a=SupportBean_S0 -> b=SupportBean_S1]@Inclusive terminated after 10 sec ";
	        _epService.EPAdministrator.CreateEPL(contextExprMulti);
	        var streamExprMulti = "context CtxPerId select * from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]";
	        var streamMulti = _epService.EPAdministrator.CreateEPL(streamExprMulti);
	        streamMulti.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "S1_1"));
	        Assert.IsTrue(_listener.IsInvoked);
	    }

        [Test]
	    public void TestEndSameEventAsAnalyzed() {

	        // same event terminates - not included
	        var fields = "c1,c2,c3,c4".Split(',');
	        _epService.EPAdministrator.CreateEPL("create context MyCtx as " +
	                "start SupportBean " +
	                "end SupportBean(IntPrimitive=11)");
	        var stmt = _epService.EPAdministrator.CreateEPL("context MyCtx " +
	                "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
	                "output snapshot when terminated");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, 10, 10, 10d});

	        _epService.EPAdministrator.DestroyAllStatements();

	        // same event terminates - included
	        fields = "c1,c2,c3,c4".Split(',');
	        _epService.EPAdministrator.CreateEPL("create schema MyCtxTerminate(TheString string)");
	        _epService.EPAdministrator.CreateEPL("create context MyCtx as start SupportBean end MyCtxTerminate");
	        stmt = _epService.EPAdministrator.CreateEPL("context MyCtx " +
	                "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
	                "output snapshot when terminated");
	        stmt.AddListener(_listener);
	        _epService.EPAdministrator.CreateEPL("insert into MyCtxTerminate select TheString from SupportBean(IntPrimitive=11)");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, 11, 21, 10.5d});

	        // test with audit
	        var epl = "@Audit create context AdBreakCtx as initiated by SupportBean(IntPrimitive > 0) as ad " +
	            " terminated by SupportBean(TheString=ad.TheString, IntPrimitive < 0) as endAd";
	        _epService.EPAdministrator.CreateEPL(epl);
	        _epService.EPAdministrator.CreateEPL("context AdBreakCtx select count(*) from SupportBean");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", -10));
	    }

        [Test]
	    public void TestContextPartitionSelection() {
	        var fields = "c0,c1,c2,c3".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("create context MyCtx as initiated by SupportBean_S0 s0 terminated by SupportBean_S1(id=s0.id)");
	        var stmt = _epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.s0.p00 as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean.win:keepall() group by TheString");

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new object[][] { new object[] { 0, "S0_1", "E1", 6 }, new object[] { 0, "S0_1", "E2", 10 }, new object[] { 0, "S0_1", "E3", 201 }, new object[] { 1, "S0_2", "E1", 3 }, new object[] { 1, "S0_2", "E3", 201 } });

	        // test iterator targeted by context partition id
	        var selectorById = new SupportSelectorById(Collections.SingletonList(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, new object[][] { new object[] { 1, "S0_2", "E1", 3 }, new object[] { 1, "S0_2", "E3", 201 } });

	        // test iterator targeted by property on triggering event
	        var filtered = new SupportSelectorFilteredInitTerm("S0_2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new object[][] { new object[] { 1, "S0_2", "E1", 3 }, new object[] { 1, "S0_2", "E3", 201 } });

	        // test always-false filter - compare context partition info
	        filtered = new SupportSelectorFilteredInitTerm(null);
	        Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
	        EPAssertionUtil.AssertEqualsAnyOrder(new object[]{1000L, 2000L}, filtered.ContextsStartTimes);
	        EPAssertionUtil.AssertEqualsAnyOrder(new object[] {"S0_1", "S0_2"}, filtered.P00PropertyValues);

	        try {
	            stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented
                {
                    ProcPartitionKeys = () => null
	            });
	            Assert.Fail();
	        }
	        catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById] interfaces but received com."), "message: " + ex.Message);
	        }
	    }

        [Test]
	    public void TestFilterInitiatedFilterAllTerminated() {

	        _epService.EPAdministrator.CreateEPL("create context MyContext as " +
	                "initiated by SupportBean_S0 " +
	                "terminated by SupportBean_S1");

	        var fields = "c1".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context MyContext select sum(IntPrimitive) as c1 from SupportBean");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1")); // initiate one

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "S0_2"));  // initiate another

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 5 }, new object[] { 3 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 9 }, new object[] { 7 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "S1_1"));  // terminate all
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot() {
	        _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
	                "initiated by SupportBean_S0 as s0 " +
	                "terminated by SupportBean_S1(p10 = s0.p00)");

	        var fields = "c1,c2".Split(',');
	        var listener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
	                "from SupportBean.win:keepall() output snapshot when terminated");
	        statement.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));
	        Assert.IsFalse(listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // terminate
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 5});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts new one
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // also starts new one

	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G2"));  // terminate G2
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G2", 15});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G3"));  // terminate G3
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G3", 11});
	    }

        [Test]
	    public void TestScheduleFilterResources() {
	        // test no-context statement
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time(30)");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);

	        stmt.Dispose();
	        Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);

	        // test initiated
	        var filterServiceSPI = (FilterServiceSPI) _spi.FilterService;

	        SendTimeEvent("2002-05-1T8:00:00.000");
	        var eplCtx = "create context EverySupportBean as " +
	                "initiated by SupportBean as sb " +
	                "terminated after 1 minutes";
	        _epService.EPAdministrator.CreateEPL(eplCtx);

	        _epService.EPAdministrator.CreateEPL("context EverySupportBean select * from SupportBean_S0.win:time(2 min) sb0");
	        Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
	        Assert.AreEqual(1, filterServiceSPI.FilterCountApprox);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);
	        Assert.AreEqual(2, filterServiceSPI.FilterCountApprox);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "S0_1"));
	        Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);
	        Assert.AreEqual(2, filterServiceSPI.FilterCountApprox);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
	        Assert.AreEqual(1, filterServiceSPI.FilterCountApprox);

	        _epService.EPAdministrator.DestroyAllStatements();
	        Assert.AreEqual(0, _spi.SchedulingService.ScheduleHandleCount);
	        Assert.AreEqual(0, filterServiceSPI.FilterCountApprox);
	    }

        [Test]
	    public void TestPatternInitiatedStraightSelect() {
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        var eplCtx = "create context EverySupportBean as " +
	                "initiated by pattern [every (a=SupportBean_S0 or b=SupportBean_S1)] " +
	                "terminated after 1 minutes";
	        _epService.EPAdministrator.CreateEPL(eplCtx);

	        var fields = "c1,c2,c3".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EverySupportBean " +
	                "select context.a.id as c1, context.b.id as c2, TheString as c3 from SupportBean");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(2));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, 2, "E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3));

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { null, 2, "E2" }, new object[] { 3, null, "E2" } });

	        _epService.EPAdministrator.DestroyAllStatements();

	        // test SODA
	        AssertSODA(eplCtx);
	    }

        [Test]
	    public void TestFilterInitiatedStraightEquals() {
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        var ctxEPL = "create context EverySupportBean as " +
	                "initiated by SupportBean(TheString like \"I%\") as sb " +
	                "terminated after 1 minutes";
	        _epService.EPAdministrator.CreateEPL(ctxEPL);

	        var fields = "c1".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EverySupportBean " +
	                "select sum(LongPrimitive) as c1 from SupportBean(IntPrimitive = context.sb.IntPrimitive)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", -1, -2L));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("I1", 2, 4L)); // counts towards stuff
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{4L});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 3L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{7L});

	        _epService.EPRuntime.SendEvent(MakeEvent("I2", 3, 14L)); // counts towards stuff
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{14L});

	        _epService.EPRuntime.SendEvent(MakeEvent("E3", 2, 2L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{9L});

	        _epService.EPRuntime.SendEvent(MakeEvent("E4", 3, 15L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{29L});

	        SendTimeEvent("2002-05-1T8:01:30.000");

	        _epService.EPRuntime.SendEvent(MakeEvent("E", -1, -2L));
	        Assert.IsFalse(_listener.IsInvoked);

	        // test SODA
	        _epService.EPAdministrator.DestroyAllStatements();
	        var model = _epService.EPAdministrator.CompileEPL(ctxEPL);
	        Assert.AreEqual(ctxEPL, model.ToEPL());
	        var stmtModel = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(ctxEPL, stmtModel.Text);
	    }

        [Test]
	    public void TestFilterAllOperators()
        {
	        // test plain
	        _epService.EPAdministrator.CreateEPL("create context EverySupportBean as " +
	                "initiated by SupportBean_S0 as sb " +
	                "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds");

#if false
            TryOperator("context.sb.id = IntBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });
            TryOperator("IntBoxed = context.sb.id", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

            TryOperator("context.sb.id > IntBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("context.sb.id >= IntBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("context.sb.id < IntBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("context.sb.id <= IntBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("IntBoxed < context.sb.id", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed <= context.sb.id", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed > context.sb.id", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("IntBoxed >= context.sb.id", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("IntBoxed in (context.sb.id)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });
            TryOperator("IntBoxed between context.sb.id and context.sb.id", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

            TryOperator("context.sb.id != IntBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });
            TryOperator("IntBoxed != context.sb.id", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });

            TryOperator("IntBoxed not in (context.sb.id)", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
            TryOperator("IntBoxed not between context.sb.id and context.sb.id", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });

            TryOperator("context.sb.id is IntBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });
            TryOperator("IntBoxed is context.sb.id", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

            TryOperator("context.sb.id is not IntBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });
            TryOperator("IntBoxed is not context.sb.id", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });
#endif

	        // try coercion
            TryOperator("context.sb.id = shortBoxed", new object[][] { new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { null, false } });
            TryOperator("shortBoxed = context.sb.id", new object[][] { new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { null, false } });

            TryOperator("context.sb.id > shortBoxed", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, false }, new object[] { (short)9, true }, new object[] { (short)8, true } });
            TryOperator("shortBoxed < context.sb.id", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, false }, new object[] { (short)9, true }, new object[] { (short)8, true } });

            TryOperator("shortBoxed in (context.sb.id)", new object[][] { new object[] { (short)11, false }, new object[] { (short)10, true }, new object[] { (short)9, false }, new object[] { (short)8, false } });
	    }

	    private void TryOperator(string @operator, object[][] testdata) {
	        var filterSpi = (FilterServiceSPI) _spi.FilterService;

	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EverySupportBean " +
	                "select TheString as c0,IntPrimitive as c1,context.sb.p00 as c2 " +
	                "from SupportBean(" + @operator + ")");
	        stmt.AddListener(_listener);

	        // initiate
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S01"));

	        for (var i = 0; i < testdata.Length; i++) {
	            var bean = new SupportBean();
	            var testValue = testdata[i][0];

	            if (testValue is int) {
	                bean.IntBoxed = testValue.AsInt();
	            }
	            else {
	                bean.ShortBoxed = testValue.AsBoxedShort();
	            }
	            var expected = (Boolean) testdata[i][1];

	            _epService.EPRuntime.SendEvent(bean);
                Assert.AreEqual(expected, _listener.GetAndClearIsInvoked(), "Failed at " + i);
	        }

	        // assert type of expression
	        if (filterSpi.IsSupportsTakeApply) {
	            var set = filterSpi.Take(Collections.SingletonList(stmt.StatementId));
	            Assert.AreEqual(1, set.Filters.Count);
	            FilterValueSet valueSet = set.Filters[0].FilterValueSet;
	            Assert.AreEqual(1, valueSet.Parameters.Length);
	            var para = valueSet.Parameters[0][0];
	            Assert.IsTrue(para.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION);
	        }

	        stmt.Dispose();
	    }

        [Test]
	    public void TestFilterBooleanOperator() {
	        _epService.EPAdministrator.CreateEPL("create context EverySupportBean as " +
	                "initiated by SupportBean_S0 as sb " +
	                "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds");

	        var fields = "c0,c1,c2".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EverySupportBean " +
	                "select TheString as c0,IntPrimitive as c1,context.sb.p00 as c2 " +
	                "from SupportBean(IntPrimitive + context.sb.id = 5)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S01"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2, "S01"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S02"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E3", 2, "S01"},  new object[] {"E3", 2, "S02"}});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "S03"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4", 2, "S01"},  new object[] {"E4", 2, "S02"}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E5", 1, "S03"}});
	    }

        [Test]
	    public void TestTerminateTwoContextSameTime() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();

	        SendTimeEvent("2002-05-1T8:00:00.000");
	        var eplContext = "@Name('CTX') create context CtxInitiated " +
	                "initiated by SupportBean_S0 as sb0 " +
	                "terminated after 1 minute";
	        _epService.EPAdministrator.CreateEPL(eplContext);

	        var fields = "c1,c2,c3".Split(',');
	        var eplGrouped = "@Name('S1') context CtxInitiated select TheString as c1, sum(IntPrimitive) as c2, context.sb0.p00 as c3 from SupportBean";
	        _epService.EPAdministrator.CreateEPL(eplGrouped).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "SB01"));

	        _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"G2", 2, "SB01"});

	        _epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"G3", 5, "SB01"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "SB02"));

	        _epService.EPRuntime.SendEvent(new SupportBean("G4", 4));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"G4", 9, "SB01"},  new object[] {"G4", 4, "SB02"}});

	        _epService.EPRuntime.SendEvent(new SupportBean("G5", 5));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"G5", 14, "SB01"},  new object[] {"G5", 9, "SB02"}});

	        SendTimeEvent("2002-05-1T8:01:00.000");

	        _epService.EPRuntime.SendEvent(new SupportBean("G6", 6));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        // clean up
	        _epService.EPAdministrator.GetStatement("S1").Dispose();
	        _epService.EPAdministrator.GetStatement("CTX").Dispose();
	    }

        [Test]
	    public void TestOutputSnapshotWhenTerminated() {
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 1 min");

	        // test when-terminated and snapshot
	        var fields = "c1".Split(',');
	        var epl = "context EveryMinute select sum(IntPrimitive) as c1 from SupportBean output snapshot when terminated";
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        SendTimeEvent("2002-05-1T8:01:10.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

	        SendTimeEvent("2002-05-1T8:01:59.999");
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        // terminate
	        SendTimeEvent("2002-05-1T8:02:00.000");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1 + 2 + 3});

	        SendTimeEvent("2002-05-1T8:02:01.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        AssertSODA(epl);

	        // terminate
	        SendTimeEvent("2002-05-1T8:03:00.000");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{4 + 5 + 6});

	        stmt.Dispose();

	        // test late-coming statement without "terminated"
	        var stmtTwo = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EveryMinute " +
	                "select context.id as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 2 events");
	        stmtTwo.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E10", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E11", 2));
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimeEvent("2002-05-1T8:04:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E12", 3));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E13", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{7});

	        // terminate
	        SendTimeEvent("2002-05-1T8:05:00.000");
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestOutputAllEvery2AndTerminated() {
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 1 min");

	        // test when-terminated and every 2 events output all with group by
	        var fields = "c1,c2".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context EveryMinute " +
	                "select TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString output all every 2 events and when terminated order by TheString asc");
	        stmt.AddListener(_listener);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        SendTimeEvent("2002-05-1T8:01:10.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 1 + 2}});

	        SendTimeEvent("2002-05-1T8:01:59.999");
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        // terminate
	        SendTimeEvent("2002-05-1T8:02:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 1 + 2},  new object[] {"E2", 3}});

	        SendTimeEvent("2002-05-1T8:02:01.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4", 4},  new object[] {"E5", 5}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4", 14},  new object[] {"E5", 5},  new object[] {"E6", 6}});

	        // terminate
	        SendTimeEvent("2002-05-1T8:03:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4", 14},  new object[] {"E5", 5},  new object[] {"E6", 6}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E6", -2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", -1},  new object[] {"E6", -2}});
	    }

        [Test]
	    public void TestOutputWhenExprWhenTerminatedCondition() {
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 1 min");

	        // test when-terminated and every 2 events output all with group by
	        var fields = "c0".Split(',');
	        var epl = "context EveryMinute " +
	                "select TheString as c0 from SupportBean output when count_insert>1 and when terminated and count_insert>0";
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"}});

	        SendTimeEvent("2002-05-1T8:01:59.999");
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        // terminate, new context partition
	        SendTimeEvent("2002-05-1T8:02:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E3"}});

	        SendTimeEvent("2002-05-1T8:02:10.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4"},  new object[] {"E5"}});

	        SendTimeEvent("2002-05-1T8:03:00.000");
	        Assert.IsFalse(_listener.IsInvoked);

	        AssertSODA(epl);
	    }

        [Test]
	    public void TestOutputOnlyWhenTerminatedCondition() {

	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 1 min");

	        // test when-terminated and every 2 events output all with group by
	        var fields = "c0".Split(',');
	        var epl = "context EveryMinute " +
	                "select TheString as c0 from SupportBean output when terminated and count_insert > 0";
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        // terminate, new context partition
	        SendTimeEvent("2002-05-1T8:02:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"}});

	        // terminate, new context partition
	        SendTimeEvent("2002-05-1T8:03:00.000");
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestOutputOnlyWhenSetAndWhenTerminatedSet() {

	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 1 min");

	        // include then-set and both real-time and terminated output
	        _epService.EPAdministrator.CreateEPL("create variable int myvar = 0");
	        var eplOne = "context EveryMinute select TheString as c0 from SupportBean " +
	                "output when true " +
	                "then set myvar=1 " +
	                "and when terminated " +
	                "then set myvar=2";
	        var stmtOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplOne);
	        stmtOne.AddListener(_listener);

	        SendTimeEvent("2002-05-1T8:01:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("myvar"));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendTimeEvent("2002-05-1T8:02:00.000"); // terminate, new context partition
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(2, _epService.EPRuntime.GetVariableValue("myvar"));

	        AssertSODA(eplOne);
	    }

        [Test]
	    public void TestOutputOnlyWhenTerminatedThenSet() {

	        var fields = "c0".Split(',');
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create variable int myvar = 0");
	        _epService.EPAdministrator.CreateEPL("create context EverySupportBeanS0 as " +
	                "initiated by SupportBean_S0 as s0 " +
	                "terminated after 1 min");

	        // include only-terminated output with set
	        _epService.EPRuntime.SetVariableValue("myvar", 0);
	        var eplTwo = "context EverySupportBeanS0 select TheString as c0 from SupportBean " +
	                "output when terminated " +
	                "then set myvar=10";
	        var stmtTwo = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplTwo);
	        stmtTwo.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        Assert.IsFalse(_listener.IsInvoked);

	        // terminate, new context partition
	        SendTimeEvent("2002-05-1T8:01:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E4"}});
	        Assert.AreEqual(10, _epService.EPRuntime.GetVariableValue("myvar"));

	        AssertSODA(eplTwo);
	    }

        [Test]
	    public void TestCrontab() {
	        var filterSPI = (FilterServiceSPI) _spi.FilterService;
	        SendTimeEvent("2002-05-1T8:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context EveryMinute as " +
	                "initiated by pattern[every timer:at(*, *, *, *, *)] " +
	                "terminated after 3 min");

	        var fields = "c1,c2".Split(',');
	        var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("@IterableUnbound context EveryMinute select TheString as c1, sum(IntPrimitive) as c2 from SupportBean");
	        statement.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(0, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, null);

	        SendTimeEvent("2002-05-1T8:01:00.000");

	        Assert.AreEqual(1, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1);
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
	        var expected = new object[][]{ new object[] {"E2", 5}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:01:59.999");

	        Assert.AreEqual(1, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1);
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
	        expected = new object[][]{ new object[] {"E3", 11}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:02:00.000");

	        Assert.AreEqual(2, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2);
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 7));
	        expected = new object[][]{ new object[] {"E4", 18},  new object[] {"E4", 7}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:02:59.999");

	        Assert.AreEqual(2, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 2);
	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 8));
	        expected = new object[][]{ new object[] {"E5", 26},  new object[] {"E5", 15}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:03:00.000");

	        Assert.AreEqual(3, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 9));
	        expected = new object[][]{ new object[] {"E6", 35},  new object[] {"E6", 24},  new object[] {"E6", 9}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:04:00.000");

	        Assert.AreEqual(3, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
	        _epService.EPRuntime.SendEvent(new SupportBean("E7", 10));
	        expected = new object[][]{ new object[] {"E7", 34},  new object[] {"E7", 19},  new object[] {"E7", 10}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        SendTimeEvent("2002-05-1T8:05:00.000");

	        Assert.AreEqual(3, filterSPI.FilterCountApprox);
	        AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3);
	        _epService.EPRuntime.SendEvent(new SupportBean("E8", 11));
	        expected = new object[][]{ new object[] {"E8", 30},  new object[] {"E8", 21},  new object[] {"E8", 11}};
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, expected);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

	        // assert certain keywords are valid: last keyword, timezone
	        _epService.EPAdministrator.CreateEPL("create context CtxMonthly1 start (0, 0, 1, *, *, 0) end(59, 23, last, *, *, 59)");
	        _epService.EPAdministrator.CreateEPL("create context CtxMonthly2 start (0, 0, 1, *, *) end(59, 23, last, *, *)");
	        _epService.EPAdministrator.CreateEPL("create context CtxMonthly3 start (0, 0, 1, *, *, 0, 'GMT-5') end(59, 23, last, *, *, 59, 'GMT-8')");
	        TryInvalid("create context CtxMonthly4 start (0) end(*,*,*,*,*)",
	                "Error starting statement: Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 1 [create context CtxMonthly4 start (0) end(*,*,*,*,*)]");
	        TryInvalid("create context CtxMonthly4 start (*,*,*,*,*) end(*,*,*,*,*,*,*,*)",
	                "Error starting statement: Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 8 [create context CtxMonthly4 start (*,*,*,*,*) end(*,*,*,*,*,*,*,*)]");

	        // test invalid -after
	        TryInvalid("create context CtxMonthly4 start after 1 second end after -1 seconds",
	                "Error starting statement: Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after 1 second end after -1 seconds]");
	        TryInvalid("create context CtxMonthly4 start after -1 second end after 1 seconds",
	                "Error starting statement: Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after -1 second end after 1 seconds]");
	    }

        [Test]
	    public void TestStartNowCalMonthScoped() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
	        SendCurrentTime("2002-02-01T9:00:00.000");
	        _epService.EPAdministrator.CreateEPL("create context MyCtx start SupportBean_S1 end after 1 month");
	        _epService.EPAdministrator.CreateEPL("context MyCtx select * from SupportBean").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendCurrentTime("2002-03-01T9:00:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestNonOverlappingSubqueryAndInvalid()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // using a separate engine instance

	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.ExecutionConfig.IsPrioritized = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(Event));

	        SendTimeEvent("2002-05-1T10:00:00.000");

	        var epl =
	                "\n @Name('ctx') create context RuleActivityTime as start (0, 9, *, *, *) end (0, 17, *, *, *);" +
	                        "\n @Name('window') context RuleActivityTime create window EventsWindow.std:firstunique(productID) as Event;" +
	                        "\n @Name('variable') create variable boolean IsOutputTriggered_2 = false;" +
	                        "\n @Name('A') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
	                        "\n @Name('B') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
	                        "\n @Name('C') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
	                        "\n @Name('D') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
	                        "\n @Name('out') context RuleActivityTime select * from EventsWindow";

	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(new SupportUpdateListener());

	        _epService.EPRuntime.SendEvent(new Event("A1"));

	        // invalid - subquery not the same context
	        SupportMessageAssertUtil.TryInvalid(_epService, "insert into EventsWindow select * from Event(not exists (select * from EventsWindow))",
	                "Failed to validate subquery number 1 querying EventsWindow: Named window by name 'EventsWindow' has been declared for context 'RuleActivityTime' and can only be used within the same context");

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

	    private void SendTimeEvent(string time) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        return bean;
	    }

	    private void AssertSODA(string epl) {
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        var stmtModel = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(epl, stmtModel.Text);
	        stmtModel.Dispose();
	    }

	    private void TryInvalid(string epl, string message) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private void SendCurrentTime(string time) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
	    }

	    private void SendCurrentTimeWithMinus(string time, long minus) {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
	    }

        [Serializable]
	    public class Event
        {
            public string ProductID { get; private set; }

            public Event(string productId)
            {
	            ProductID = productId;
	        }
        }
	}
} // end of namespace
