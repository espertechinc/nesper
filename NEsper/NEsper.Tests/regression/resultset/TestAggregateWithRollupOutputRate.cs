///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateWithRollupOutputRate 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestOutputLastNonJoin() {
	        RunAssertionOutputLast(false, false);
	    }

        [Test]
	    public void TestOutputLastJoin() {
	        RunAssertionOutputLast(false, true);
	    }

        [Test]
	    public void TestOutputLastNonJoinHinted() {
	        RunAssertionOutputLast(true, false);
	    }

        [Test]
	    public void TestOutputLastJoinHinted() {
	        RunAssertionOutputLast(true, true);
	    }

        [Test]
	    public void TestOutputLastSortedNonJoin() {
	        RunAssertionOutputLastSorted(false);
	    }

        [Test]
	    public void TestOutputLastSortedJoin() {
	        RunAssertionOutputLastSorted(true);
	    }

        [Test]
	    public void TestOutputAllNonJoin() {
	        RunAssertionOutputAll(false, false);
	    }

        [Test]
	    public void TestOutputAllNonJoinHinted() {
	        RunAssertionOutputAll(false, true);
	    }

        [Test]
	    public void TestOutputAllJoin() {
	        RunAssertionOutputAll(true, false);
	    }

        [Test]
	    public void TestOutputAllJoinHinted() {
	        RunAssertionOutputAll(true, true);
	    }

        [Test]
	    public void TestOutputAllSortedNonJoin() {
	        RunAssertionOutputAllSorted(false);
	    }

        [Test]
	    public void TestOutputAllSortedJoin() {
	        RunAssertionOutputAllSorted(true);
	    }

        [Test]
	    public void TestOutputDefaultNonJoin() {
	        RunAssertionOutputDefault(false);
	    }

        [Test]
	    public void TestOutputDefaultJoin() {
	        RunAssertionOutputDefault(true);
	    }

        [Test]
	    public void TestOutputDefaultSortedNonJoin() {
	        RunAssertionOutputDefaultSorted(false);
	    }

        [Test]
	    public void TestOutputDefaultSortedJoin() {
	        RunAssertionOutputDefaultSorted(true);
	    }

        [Test]
	    public void TestOutputFirstHavingNonJoin() {
	        RunAssertionOutputFirstHaving(false);
	    }

        [Test]
	    public void TestOutputFirstHavingJoin() {
	        RunAssertionOutputFirstHaving(true);
	    }

        [Test]
	    public void TestOutputFirstSortedNonJoin() {
	        RunAssertionOutputFirstSorted(false);
	    }

        [Test]
	    public void TestOutputFirstSortedJoin() {
	        RunAssertionOutputFirstSorted(true);
	    }

        [Test]
	    public void TestOutputFirstNonJoin() {
	        RunAssertionOutputFirst(false);
	    }

        [Test]
	    public void TestOutputFirstJoin() {
	        RunAssertionOutputFirst(true);
	    }

        [Test]
	    public void Test1NoOutputLimit()
	    {
	        var stmtText = "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("NoOutputLimit", null, fields);
	        expected.AddResultInsRem(200, 1, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 25d}}, new object[][] {new object[] {"IBM", null}, new object[] {null, null}});
	        expected.AddResultInsRem(800, 1, new object[][] {new object[] {"MSFT", 9d}, new object[] {null, 34d}}, new object[][] {new object[] {"MSFT", null}, new object[] {null, 25d}});
	        expected.AddResultInsRem(1500, 1, new object[][] {new object[] {"IBM", 49d}, new object[] {null, 58d}}, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 34d}});
	        expected.AddResultInsRem(1500, 2, new object[][] {new object[] {"YAH", 1d}, new object[] {null, 59d}}, new object[][] {new object[] {"YAH", null}, new object[] {null, 58d}});
	        expected.AddResultInsRem(2100, 1, new object[][] {new object[] {"IBM", 75d}, new object[] {null, 85d}}, new object[][] {new object[] {"IBM", 49d}, new object[] {null, 59d}});
	        expected.AddResultInsRem(3500, 1, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 87d}}, new object[][] {new object[] {"YAH", 1d}, new object[] {null, 85d}});
	        expected.AddResultInsRem(4300, 1, new object[][] {new object[] {"IBM", 97d}, new object[] {null, 109d}}, new object[][] {new object[] {"IBM", 75d}, new object[] {null, 87d}});
	        expected.AddResultInsRem(4900, 1, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 112d}}, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 109d}});
	        expected.AddResultInsRem(5700, 0, new object[][] {new object[] {"IBM", 72d}, new object[] {null, 87d}}, new object[][] {new object[] {"IBM", 97d}, new object[] {null, 112d}});
	        expected.AddResultInsRem(5900, 1, new object[][] {new object[] {"YAH", 7d}, new object[] {null, 88d}}, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 87d}});
	        expected.AddResultInsRem(6300, 0, new object[][] {new object[] {"MSFT", null}, new object[] {null, 79d}}, new object[][] {new object[] {"MSFT", 9d}, new object[] {null, 88d}});
	        expected.AddResultInsRem(7000, 0, new object[][] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}}, new object[][] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 79d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void Test2OutputLimitDefault()
	    {
	        var stmtText = "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)" +
	                "output every 1 seconds";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("DefaultOutputLimit", null, fields);
	        expected.AddResultInsRem(1200, 0,
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 25d }, new object[] { "MSFT", 9d }, new object[] { null, 34d } },
                    new object[][] { new object[] { "IBM", null }, new object[] { null, null }, new object[] { "MSFT", null }, new object[] { null, 25d } });
	        expected.AddResultInsRem(2200, 0,
                    new object[][] { new object[] { "IBM", 49d }, new object[] { null, 58d }, new object[] { "YAH", 1d }, new object[] { null, 59d }, new object[] { "IBM", 75d }, new object[] { null, 85d } },
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 34d }, new object[] { "YAH", null }, new object[] { null, 58d }, new object[] { "IBM", 49d }, new object[] { null, 59d } });
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0,
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 87d } },
                    new object[][] { new object[] { "YAH", 1d }, new object[] { null, 85d } });
	        expected.AddResultInsRem(5200, 0,
                    new object[][] { new object[] { "IBM", 97d }, new object[] { null, 109d }, new object[] { "YAH", 6d }, new object[] { null, 112d } },
                    new object[][] { new object[] { "IBM", 75d }, new object[] { null, 87d }, new object[] { "YAH", 3d }, new object[] { null, 109d } });
	        expected.AddResultInsRem(6200, 0,
                    new object[][] { new object[] { "IBM", 72d }, new object[] { null, 87d }, new object[] { "YAH", 7d }, new object[] { null, 88d } },
                    new object[][] { new object[] { "IBM", 97d }, new object[] { null, 112d }, new object[] { "YAH", 6d }, new object[] { null, 87d } });
	        expected.AddResultInsRem(7200, 0,
                    new object[][] { new object[] { "MSFT", null }, new object[] { null, 79d }, new object[] { "IBM", 48d }, new object[] { "YAH", 6d }, new object[] { null, 54d } },
                    new object[][] { new object[] { "MSFT", 9d }, new object[] { null, 88d }, new object[] { "IBM", 72d }, new object[] { "YAH", 7d }, new object[] { null, 79d } });

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void Test3OutputLimitAll() {
	        RunAssertion3OutputLimitAll(false);
	    }

        [Test]
	    public void Test3OutputLimitAllHinted() {
	        RunAssertion3OutputLimitAll(true);
	    }

	    private void RunAssertion3OutputLimitAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var stmtText = hint + "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)" +
	                "output all every 1 seconds";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
	        expected.AddResultInsRem(1200, 0,
	                new object[][] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34d}},
	                new object[][] {new object[] {"IBM", null}, new object[] {"MSFT", null}, new object[] {null, null}});
	        expected.AddResultInsRem(2200, 0,
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
	                new object[][] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {"YAH", null}, new object[] {null, 34d}});
	        expected.AddResultInsRem(3200, 0,
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}});
	        expected.AddResultInsRem(4200, 0,
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87d}},
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}});
	        expected.AddResultInsRem(5200, 0,
	                new object[][] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112d}},
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87d}});
	        expected.AddResultInsRem(6200, 0,
	                new object[][] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}, new object[] {null, 88d}},
	                new object[][] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112d}});
	        expected.AddResultInsRem(7200, 0,
	                new object[][] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}, new object[] {null, 54d}},
	                new object[][] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}, new object[] {null, 88d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(true);
	    }

        [Test]
	    public void Test4OutputLimitLast() {
	        RunAssertion4OutputLimitLast(false);
	    }

        [Test]
	    public void Test4OutputLimitLastHinted() {
	        RunAssertion4OutputLimitLast(true);
	    }

	    private void RunAssertion4OutputLimitLast(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var stmtText = hint + "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)" +
	                "output last every 1 seconds";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
	        expected.AddResultInsRem(1200, 0,
	                new object[][] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34d}},
	                new object[][] {new object[] {"IBM", null}, new object[] {"MSFT", null}, new object[] {null, null}});
	        expected.AddResultInsRem(2200, 0,
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
	                new object[][] {new object[] {"IBM", 25d}, new object[] {"YAH", null}, new object[] {null, 34d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0,
	                new object[][] {new object[] {"YAH", 3d}, new object[] {null, 87d}},
	                new object[][] {new object[] {"YAH", 1d}, new object[] {null, 85d}});
	        expected.AddResultInsRem(5200, 0,
	                new object[][] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}, new object[] {null, 112d}},
	                new object[][] {new object[] {"IBM", 75d}, new object[] {"YAH", 3d}, new object[] {null, 87d}});
	        expected.AddResultInsRem(6200, 0,
	                new object[][] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88d}},
	                new object[][] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}, new object[] {null, 112d}});
	        expected.AddResultInsRem(7200, 0,
	                new object[][] {new object[] {"MSFT", null}, new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}},
	                new object[][] {new object[] {"MSFT", 9d}, new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(true);
	    }

        [Test]
	    public void Test5OutputLimitFirst()
	    {
	        var stmtText = "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)" +
	                "output first every 1 seconds";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
	        expected.AddResultInsRem(200, 1, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 25d}}, new object[][] {new object[] {"IBM", null}, new object[] {null, null}});
	        expected.AddResultInsRem(800, 1, new object[][] {new object[] {"MSFT", 9d}}, new object[][] {new object[] {"MSFT", null}});
	        expected.AddResultInsRem(1500, 1, new object[][] {new object[] {"IBM", 49d}, new object[] {null, 58d}}, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 34d}});
	        expected.AddResultInsRem(1500, 2, new object[][] {new object[] {"YAH", 1d}}, new object[][] {new object[] {"YAH", null}});
	        expected.AddResultInsRem(3500, 1, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 87d}}, new object[][] {new object[] {"YAH", 1d}, new object[] {null, 85d}});
	        expected.AddResultInsRem(4300, 1, new object[][] {new object[] {"IBM", 97d}}, new object[][] {new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(4900, 1, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 112d}}, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 109d}});
	        expected.AddResultInsRem(5700, 0, new object[][] {new object[] {"IBM", 72d}}, new object[][] {new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(5900, 1, new object[][] {new object[] {"YAH", 7d}, new object[] {null, 88d}}, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 87d}});
	        expected.AddResultInsRem(6300, 0, new object[][] {new object[] {"MSFT", null}}, new object[][] {new object[] {"MSFT", 9d}});
	        expected.AddResultInsRem(7000, 0, new object[][] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}}, new object[][] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 79d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void Test6OutputLimitSnapshot()
	    {
	        var stmtText = "select symbol, sum(price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by rollup(symbol)" +
	                "output snapshot every 1 seconds";
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"symbol", "sum(price)"};
	        var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
	        expected.AddResultInsert(1200, 0, new object[][] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34.0}});
	        expected.AddResultInsert(2200, 0, new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85.0}});
	        expected.AddResultInsert(3200, 0, new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85.0}});
	        expected.AddResultInsert(4200, 0, new object[][] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87.0}});
	        expected.AddResultInsert(5200, 0, new object[][] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112.0}});
	        expected.AddResultInsert(6200, 0, new object[][] {new object[] {"MSFT", 9d}, new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88.0}});
	        expected.AddResultInsert(7200, 0, new object[][] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54.0}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertionOutputFirstHaving(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "having sum(longPrimitive) > 100 " +
	                "output first every 1 second").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", null, 110L}, new object[] {null, null, 150L}},
	                new object[][]{new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
	                new object[][]{new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}},
	                new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 130L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
	                new object[][] {new object[] {"E1", 1, 130L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", null, 210L}, new object[] {null, null, 210L}},
	                new object[][] {new object[] {"E1", null, 210L}, new object[] {null, null, 210L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 300L}},
	                new object[][] {new object[] {"E1", 1, 300L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
	                new object[][] {new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}});
	    }

	    private void RunAssertionOutputFirst(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output first every 1 second").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L}},
	                new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 2, 20L}},
	                new object[][]{new object[] {"E1", 2, null}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E2", 1, 40L}, new object[] {"E2", null, 40L}, new object[] {null, null, 100L}},
	                new object[][]{new object[] {"E2", 1, null}, new object[] {"E2", null, null}, new object[] {null, null, 60L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 2, 70L}, new object[] {"E1", null, 110L}},
	                new object[][]{new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}},
	                new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
	                new object[][] {new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	            new object[][] {
	                    new object[] {"E2", 1, null}, new object[] {"E1", 2, null}, new object[] {"E2", null, null},
	                    new object[] {"E1", null, 210L}, new object[] {null, null, 210L}},
	            new object[][] {
	                    new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L},
	                    new object[] {"E1", null, 260L}, new object[] {null, null, 300L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 300L}},
	                new object[][] {new object[] {"E1", 1, 210L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	            new object[][] {new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
	            new object[][] {new object[] {"E1", 1, 300L}, new object[] {"E1", null, 300L}, new object[] {null, null, 300L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionOutputFirstSorted(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output first every 1 second " +
	                "order by theString, intPrimitive").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 10L}, new object[] {"E1", null, 10L}, new object[] {"E1", 1, 10L}},
	                new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 2, 20L}},
	                new object[][]{new object[] {"E1", 2, null}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 100L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
	                new object[][]{new object[] {null, null, 60L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", null, 110L}, new object[] {"E1", 2, 70L}},
	                new object[][]{new object[] {"E1", null, 60L}, new object[] {"E1", 2, 20L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}},
	                new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}});

	        // pass 1 second
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 280L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 170L}},
	                new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}},
	                new object[][] {new object[] {null, null, 280L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 2, null},
	                        new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
	                new object[][] {new object[] {null, null, 300L}, new object[] {"E1", null, 260L}, new object[] {"E1", 2, 50L},
	                        new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 300L}},
	                new object[][] {new object[] {"E1", 1, 210L}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}},
	                new object[][] {new object[] {null, null, 300L}, new object[] {"E1", null, 300L}, new object[] {"E1", 1, 300L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionOutputDefault(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) "  + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output every 1 second").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L},
	                        new object[] {"E1", 2, 20L}, new object[] {"E1", null, 30L}, new object[] {null, null, 30L},
	                        new object[] {"E1", 1, 40L}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}},
	                new object[][]{
	                        new object[] {"E1", 1, null}, new object[] {"E1", null, null}, new object[] {null, null, null},
	                        new object[] {"E1", 2, null}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L},
	                        new object[] {"E1", 1, 10L},  new object[] {"E1", null, 30L}, new object[] {null, null, 30L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {"E2", 1, 40L}, new object[] {"E2", null, 40L}, new object[] {null, null, 100L},
	                        new object[] {"E1", 2, 70L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}},
	                new object[][] {
	                        new object[] {"E2", 1, null}, new object[] {"E2", null, null}, new object[] {null, null, 60L},
	                        new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}, new object[] {null, null, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
	                new object[][] {
	                        new object[] {"E1", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L},
	                        new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L},
	                },
	                new object[][] {
	                        new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L},
	                        new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L},
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {"E1", 1, 210L}, new object[] {"E1", null, 260L}, new object[] {null, null, 300L},
	                        new object[] {"E2", 1, null}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E1", null, 210L}, new object[] {null, null, 210L},
	                },
	                new object[][] {
	                        new object[] {"E1", 1, 130L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L},
	                        new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E1", null, 260L}, new object[] {null, null, 300L},
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {"E1", 1, 300L}, new object[] {"E1", null, 300L}, new object[] {null, null, 300L},
	                        new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
	        new object[][] {
	                        new object[] {"E1", 1, 210L}, new object[] {"E1", null, 210L}, new object[] {null, null, 210L},
	                        new object[] {"E1", 1, 300L}, new object[] {"E1", null, 300L}, new object[] {null, null, 300L}});
	    }

	    private void RunAssertionOutputDefaultSorted(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output every 1 second " +
	                "order by theString, intPrimitive").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {null, null, 10L}, new object[] {null, null, 30L}, new object[] {null, null, 60L},
	                        new object[] {"E1", null, 10L}, new object[] {"E1", null, 30L}, new object[] {"E1", null, 60L},
	                        new object[] {"E1", 1, 10L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}},
	                new object[][]{
	                        new object[] {null, null, null}, new object[] {null, null, 10L}, new object[] {null, null, 30L},
	                        new object[] {"E1", null, null}, new object[] {"E1", null, 10L}, new object[] {"E1", null, 30L},
	                        new object[] {"E1", 1, null}, new object[] {"E1", 1, 10L}, new object[] {"E1", 2, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {null, null, 100L}, new object[] {null, null, 150L},
	                        new object[] {"E1", null, 110L}, new object[] {"E1", 2, 70L},
	                        new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
	                new object[][] {
	                        new object[] {null, null, 60L}, new object[] {null, null, 100L},
	                        new object[] {"E1", null, 60L}, new object[] {"E1", 2, 20L},
	                        new object[] {"E2", null, null}, new object[] {"E2", 1, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}},
	                new object[][] {
	                        new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {null, null, 280L}, new object[] {null, null, 220L},
	                        new object[] {"E1", null, 240L}, new object[] {"E1", null, 180L},
	                        new object[] {"E1", 1, 170L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}},
	                new object[][] {
	                        new object[] {null, null, 210L}, new object[] {null, null, 280L},
	                        new object[] {"E1", null, 170L}, new object[] {"E1", null, 240L},
	                        new object[] {"E1", 1, 100L}, new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {null, null, 300L}, new object[] {null, null, 210L},
	                        new object[] {"E1", null, 260L}, new object[] {"E1", null, 210L},
	                        new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
	                new object[][] {
	                        new object[] {null, null, 220L}, new object[] {null, null, 300L},
	                        new object[] {"E1", null, 180L}, new object[] {"E1", null, 260L},
	                        new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {
	                        new object[] {null, null, 300L}, new object[] {null, null, 240L},
	                        new object[] {"E1", null, 300L}, new object[] {"E1", null, 240L},
	                        new object[] {"E1", 1, 300L}, new object[] {"E1", 1, 240L}},
	                new object[][] {
	                        new object[] {null, null, 210L}, new object[] {null, null, 300L},
	                        new object[] {"E1", null, 210L}, new object[] {"E1", null, 300L},
	                        new object[] {"E1", 1, 210L}, new object[] {"E1", 1, 300L}});
	    }

	    private void RunAssertionOutputAll(bool join, bool hinted) {

	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL(hint + "@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output all every 1 second").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}},
	                new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {"E2", null, 40L}, new object[] {null, null, 150L}},
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E2", 1, null}, new object[] {"E1", null, 60L}, new object[] {"E2", null, null}, new object[] {null, null, 60L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 170L}, new object[] {"E2", null, 40L}, new object[] {null, null, 210L}},
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {"E2", null, 40L}, new object[] {null, null, 150L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}},
	                new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 170L}, new object[] {"E2", null, 40L}, new object[] {null, null, 210L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}},
	                new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 240L}, new object[] {"E2", null, null}, new object[] {null, null, 240L}},
	                new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}});
	    }

	    private void RunAssertionOutputAllSorted(bool join) {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output all every 1 second " +
	                "order by theString, intPrimitive").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}},
	                new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}, new object[] {"E1", 2, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L} },
	                new object[][] {new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] { new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
	                new object[][] { new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L},  new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
	                new object[][] {new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L},  new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});
	    }

	    private void RunAssertionOutputLast(bool hinted, bool join) {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL(hint + "@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output last every 1 second").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}},
	                new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}},
	                new object[][]{new object[] {"E2", 1, null}, new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
	                new object[][] {new object[] {"E1", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000)); // removes the first 3 events
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
	                new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000)); // removes the second 2 events
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 210L}, new object[] {"E2", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}},
	                new object[][] {new object[] {"E1", 1, 130L}, new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 50L},  new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000)); // removes the third 1 event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
	                new object[][] {new object[] {"E1", 1, 210L}, new object[] {"E1", null, 210L}, new object[] {null, null, 210L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionOutputLastSorted(bool join) {

	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
	                "from SupportBean.win:time(3.5 sec) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(theString, intPrimitive) " +
	                "output last every 1 second " +
	                "order by theString, intPrimitive").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}},
	                new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}, new object[] {"E1", 2, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L},  new object[] {"E2", 1, 40L}},
	                new object[][] {new object[] {null, null, 60L},  new object[] {"E1", null, 60L},  new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}},
	                new object[][] {new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] {new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}},
	                new object[][] {new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] { new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
	                new object[][] { new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
	        _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][] { new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}},
	                new object[][] { new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}});
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        return sb;
	    }

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }
	}
} // end of namespace
