///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
	public class TestRowPatternMaxStatesEngineWide : SupportBeanConstants
	{
	    private EPServiceProvider _epService;
	    private SupportConditionHandlerFactory.SupportConditionHandler _handler;
	    private SupportUpdateListener _listenerOne;
	    private SupportUpdateListener _listenerTwo;

        [SetUp]
	    public void SetUp()
        {
	        _listenerOne = new SupportUpdateListener();
	        _listenerTwo = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        _handler = null;
	    }

        [Test]
	    public void TestReportDontPreventandRuntimeConfig()
        {
            var fields = "c0".Split(',');
	        InitService(3L, false);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }

	        var epl = "@Name('S1') select * from SupportBean " +
	                "match_recognize (" +
	                "  partition by theString " +
	                "  measures P1.theString as c0" +
	                "  pattern (P1 P2) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 1," +
	                "    P2 as P2.intPrimitive = 2" +
	                ")";

	        var listener = new SupportUpdateListener();
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("C", 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(new SupportBean("D", 1));
	        AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E", 1));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 4));

	        _epService.EPRuntime.SendEvent(new SupportBean("D", 2));    // D gone
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"D"});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));    // A gone
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A"});

	        _epService.EPRuntime.SendEvent(new SupportBean("C", 2));    // C gone
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"C"});

	        _epService.EPRuntime.SendEvent(new SupportBean("F", 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        _epService.EPRuntime.SendEvent(new SupportBean("G", 1));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        _epService.EPAdministrator.Configuration.MatchRecognizeMaxStates = 4L;

	        _epService.EPRuntime.SendEvent(new SupportBean("G", 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        _epService.EPRuntime.SendEvent(new SupportBean("H", 1));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 4));

	        _epService.EPAdministrator.Configuration.MatchRecognizeMaxStates = null;

	        _epService.EPRuntime.SendEvent(new SupportBean("I", 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestTwoStatementNoDelete()
	    {
	        var fields = "c0".Split(',');
	        InitService(3L, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }

	        var eplOne = "@Name('S1') select * from SupportBean(theString='A') " +
	                "match_recognize (" +
	                "  measures P1.longPrimitive as c0" +
	                "  pattern (P1 P2 P3) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 1," +
	                "    P2 as P2.intPrimitive = 1," +
	                "    P3 as P3.intPrimitive = 2 and P3.longPrimitive = P1.longPrimitive" +
	                ")";
	        var stmtOne = _epService.EPAdministrator.CreateEPL(eplOne);
	        stmtOne.AddListener(_listenerOne);

	        var eplTwo = "@Name('S2') select * from SupportBean(theString='B') " +
	                "match_recognize (" +
	                "  measures P1.longPrimitive as c0" +
	                "  pattern (P1 P2 P3) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 1," +
	                "    P2 as P2.intPrimitive = 1," +
	                "    P3 as P3.intPrimitive = 2 and P3.longPrimitive = P1.longPrimitive" +
	                ")";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(eplTwo);
	        stmtTwo.AddListener(_listenerTwo);

	        _epService.EPRuntime.SendEvent(MakeBean("A", 1, 10)); // A(10):P1->P2
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 11)); // A(10):P1->P2, B(11):P1->P2
	        _epService.EPRuntime.SendEvent(MakeBean("A", 1, 12)); // A(10):P2->P3, A(12):P1->P2, B(11):P1->P2
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 13)); // would be: A(10):P2->P3, A(12):P1->P2, B(11):P2->P3, B(13):P1->P2
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 2, "S2", 1));

	        // terminate B
	        _epService.EPRuntime.SendEvent(MakeBean("B", 2, 11)); // we have no more B-state
	        EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), fields, new object[] {11L});

	        // should not overflow
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 15));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 16));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 2, "S2", 1));

	        // terminate A
	        _epService.EPRuntime.SendEvent(MakeBean("A", 2, 10)); // we have no more A-state
	        EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new object[] {10L});

	        // should not overflow
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 17));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 18));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 1, 19));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("A", 1, 20));
            AssertContextEnginePool(_epService, stmtOne, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 1, "S2", 2));

	        // terminate B
	        _epService.EPRuntime.SendEvent(MakeBean("B", 2, 17));
	        EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), fields, new object[] {17L});

	        // terminate A
	        _epService.EPRuntime.SendEvent(MakeBean("A", 2, 19));
	        EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new object[] {19L});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestDataWindowAndStmtStop()
	    {
	        var fields = "c0".Split(',');
	        InitService(4L, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}

	        var eplOne = "@Name('S1') select * from SupportBean(theString = 'A') " +
	                "match_recognize (" +
	                "  partition by intPrimitive " +
	                "  measures P2.intPrimitive as c0" +
	                "  pattern (P1 P2) " +
	                "  define " +
	                "    P1 as P1.longPrimitive = 1," +
	                "    P2 as P2.longPrimitive = 2" +
	                ")";
	        var stmtOne = _epService.EPAdministrator.CreateEPL(eplOne);
	        stmtOne.AddListener(_listenerOne);

	        var eplTwo = "@Name('S2') select * from SupportBean(theString = 'B').win:length(2) " +
	                "match_recognize (" +
	                "  partition by intPrimitive " +
	                "  measures P2.intPrimitive as c0" +
	                "  pattern (P1 P2) " +
	                "  define " +
	                "    P1 as P1.longPrimitive = 1," +
	                "    P2 as P2.longPrimitive = 2" +
	                ")";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(eplTwo);
	        stmtTwo.AddListener(_listenerTwo);

	        _epService.EPRuntime.SendEvent(MakeBean("A", 100, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 200, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 100, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 200, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 300, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 400, 1));
	        EPAssertionUtil.EnumeratorToArray(stmtTwo.GetEnumerator());
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("A", 300, 1));
            AssertContextEnginePool(_epService, stmtOne, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 2, "S2", 2));

	        // terminate B
	        _epService.EPRuntime.SendEvent(MakeBean("B", 400, 2));
	        EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), fields, new object[] {400});

	        // terminate one of A
	        _epService.EPRuntime.SendEvent(MakeBean("A", 100, 2));
	        EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new object[] {100});

	        // fill up A
	        _epService.EPRuntime.SendEvent(MakeBean("A", 300, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 400, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 500, 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("B", 500, 1));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 4, "S2", 0));

	        // destroy statement-1 freeing up all "A"
	        stmtOne.Dispose();

	        // any number of B doesn't trigger overflow because of data window
	        _epService.EPRuntime.SendEvent(MakeBean("B", 600, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 700, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 800, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 900, 1));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestContextPartitionAndOverflow()
	    {
	        var fields = "c0".Split('.');
	        InitService(3L, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }

	        var eplCtx = "create context MyCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(p10 = s0.p00)";
	        _epService.EPAdministrator.CreateEPL(eplCtx);

	        var epl = "@Name('S1') context MyCtx select * from SupportBean(theString = context.s0.p00) " +
	                "match_recognize (" +
	                "  measures P2.theString as c0" +
	                "  pattern (P1 P2) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 1," +
	                "    P2 as P2.intPrimitive = 2" +
	                ")";
	        var listener = new SupportUpdateListener();
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "B"));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "C"));
	        _epService.EPRuntime.SendEvent(new SupportBean("C", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "D"));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        _epService.EPRuntime.SendEvent(new SupportBean("D", 1));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        // terminate a context partition
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "D"));
	        _epService.EPRuntime.SendEvent(new SupportBean("D", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E"));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        _epService.EPRuntime.SendEvent(new SupportBean("E", 1));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A"});

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestNamedWindowInSequenceRemoveEvent()
	    {
	        var fields = "c0,c1".Split(',');
	        InitService(3L, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }

	        var namedWindow = "create window MyWindow.win:keepall() as SupportBean";
	        _epService.EPAdministrator.CreateEPL(namedWindow);
	        var insert = "insert into MyWindow select * from SupportBean";
	        _epService.EPAdministrator.CreateEPL(insert);
	        var delete = "on SupportBean_S0 delete from MyWindow where theString = p00";
	        _epService.EPAdministrator.CreateEPL(delete);

	        var epl = "@Name('S1') select * from MyWindow " +
	                "match_recognize (" +
	                "  partition by theString " +
	                "  measures P1.longPrimitive as c0, P2.longPrimitive as c1" +
	                "  pattern (P1 P2) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 0," +
	                "    P2 as P2.intPrimitive = 1" +
	                ")";
	        var listener = new SupportUpdateListener();
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(MakeBean("A", 0, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 0, 2));
	        _epService.EPRuntime.SendEvent(MakeBean("C", 0, 3));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("D", 0, 4));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        // delete A (in-sequence remove)
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
	        _epService.EPRuntime.SendEvent(MakeBean("D", 0, 5)); // now 3 states: B, C, D
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // test matching
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 6)); // now 2 states: C, D
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {2L, 6L});

	        // no overflows
	        _epService.EPRuntime.SendEvent(MakeBean("E", 0, 7));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("F", 0, 9));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        // no match expected
	        _epService.EPRuntime.SendEvent(MakeBean("F", 1, 10));
	        Assert.IsFalse(listener.IsInvoked);

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

        [Test]
	    public void TestNamedWindowOutOfSequenceRemoveEvent()
	    {
	        var fields = "c0,c1,c2".Split(',');
	        InitService(3L, true);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }

	        var namedWindow = "create window MyWindow.win:keepall() as SupportBean";
	        _epService.EPAdministrator.CreateEPL(namedWindow);
	        var insert = "insert into MyWindow select * from SupportBean";
	        _epService.EPAdministrator.CreateEPL(insert);
	        var delete = "on SupportBean_S0 delete from MyWindow where theString = p00 and intPrimitive = id";
	        _epService.EPAdministrator.CreateEPL(delete);

	        var epl = "@Name('S1') select * from MyWindow " +
	                "match_recognize (" +
	                "  partition by theString " +
	                "  measures P1.longPrimitive as c0, P2.longPrimitive as c1, P3.longPrimitive as c2" +
	                "  pattern (P1 P2 P3) " +
	                "  define " +
	                "    P1 as P1.intPrimitive = 0," +
	                "    P2 as P2.intPrimitive = 1," +
	                "    P3 as P3.intPrimitive = 2" +
	                ")";
	        var listener = new SupportUpdateListener();
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(MakeBean("A", 0, 1));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 1, 2));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 0, 3));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // delete A-1 (out-of-sequence remove)
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
	        _epService.EPRuntime.SendEvent(MakeBean("A", 2, 4));
	        Assert.IsFalse(listener.IsInvoked);
	        Assert.IsTrue(_handler.Contexts.IsEmpty()); // states: B

	        // test overflow
	        _epService.EPRuntime.SendEvent(MakeBean("C", 0, 5));
	        _epService.EPRuntime.SendEvent(MakeBean("D", 0, 6));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("E", 0, 7));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        // assert nothing matches for overflowed and deleted
	        _epService.EPRuntime.SendEvent(MakeBean("E", 1, 8));
	        _epService.EPRuntime.SendEvent(MakeBean("E", 2, 9));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "C")); // delete c
	        _epService.EPRuntime.SendEvent(MakeBean("C", 1, 10));
	        _epService.EPRuntime.SendEvent(MakeBean("C", 2, 11));
	        Assert.IsFalse(listener.IsInvoked);

	        // assert match found for B
	        _epService.EPRuntime.SendEvent(MakeBean("B", 1, 12));
	        _epService.EPRuntime.SendEvent(MakeBean("B", 2, 13));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {3L, 12L, 13L});

	        // no overflow
	        _epService.EPRuntime.SendEvent(MakeBean("F", 0, 14));
	        _epService.EPRuntime.SendEvent(MakeBean("G", 0, 15));
	        Assert.IsTrue(_handler.Contexts.IsEmpty());

	        // overflow
	        _epService.EPRuntime.SendEvent(MakeBean("H", 0, 16));
            AssertContextEnginePool(_epService, stmt, _handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
	    }

	    private void InitService(long max, bool preventStart)
        {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType(typeof(SupportBean));
	        config.AddEventType(typeof(SupportBean_S0));
	        config.AddEventType(typeof(SupportBean_S1));
	        config.EngineDefaults.ConditionHandlingConfig.AddClass<SupportConditionHandlerFactory>();
	        config.EngineDefaults.MatchRecognizeConfig.MaxStates = max;
	        config.EngineDefaults.MatchRecognizeConfig.IsMaxStatesPreventStart = preventStart;
	        config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();

	        var context = SupportConditionHandlerFactory.FactoryContexts[0];
	        Assert.AreEqual(_epService.URI, context.EngineURI);
	        _handler = SupportConditionHandlerFactory.LastHandler;
	    }

	    private static void AssertContextEnginePool(EPServiceProvider epService, EPStatement stmt, IList<ConditionHandlerContext> contexts, int max, IDictionary<string, long> counts)
        {
	        Assert.AreEqual(1, contexts.Count);
	        var context = contexts[0];
	        Assert.AreEqual(epService.URI, context.EngineURI);
	        Assert.AreEqual(stmt.Text, context.Epl);
	        Assert.AreEqual(stmt.Name, context.StatementName);
	        var condition = (ConditionMatchRecognizeStatesMax) context.EngineCondition;
	        Assert.AreEqual(max, condition.Max);
	        Assert.AreEqual(counts.Count, condition.Counts.Count);
	        foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
	        }
	        contexts.Clear();
	    }

	    private static IDictionary<string, long> GetExpectedCountMap(string stmtOne, long countOne, string stmtTwo, long countTwo)
        {
	        IDictionary<string, long> result = new Dictionary<string, long>();
	        result.Put(stmtOne, countOne);
	        result.Put(stmtTwo, countTwo);
	        return result;
	    }

	    private static IDictionary<string, long> GetExpectedCountMap(string stmtOne, long countOne)
        {
	        IDictionary<string, long> result = new Dictionary<string, long>();
	        result.Put(stmtOne, countOne);
	        return result;
	    }

	    private static SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive)
        {
	        var supportBean = new SupportBean(theString, intPrimitive);
	        supportBean.LongPrimitive = longPrimitive;
	        return supportBean;
	    }
	}
} // end of namespace
