///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitAggregateAll 
	{
	    private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).FullName;
	    private const string JOIN_KEY = "KEY";

	    private SupportUpdateListener _listener;
		private EPServiceProvider _epService;
	    private long _currentTime;
	    private const string CATEGORY = "Aggregated and Un-grouped";

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("MarketData", typeof(SupportMarketDataBean));
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void Test1NoneNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec)";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test2NoneNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test3NoneHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            " having sum(Price) > 100";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test4NoneHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            " having sum(Price) > 100";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test5DefaultNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test6DefaultNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test7DefaultHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) \n" +
	                            "having sum(Price) > 100" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test8DefaultHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "having sum(Price) > 100" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec) " +
	                "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "having sum(Price) > 100" +
	                            "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec) " +
	                "having sum(Price) > 100" +
	                "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "having sum(Price) > 100" +
	                            "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "having sum(Price) > 100" +
	                "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test13LastNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec)" +
	                            "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test13LastNoHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test14LastNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test14LastNoHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec)" +
	                            "having sum(Price) > 100 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "having sum(Price) > 100 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "having sum(Price) > 100 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "having sum(Price) > 100 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoinIStreamOnly()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "output first every 1 seconds";
	        RunAssertion17IStreamOnly(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoinIRStream()
	    {
	        var stmtText = "select irstream Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec) " +
	                "output first every 1 seconds";
	        RunAssertion17IRStream(stmtText, "first");
	    }

        [Test]
	    public void Test18SnapshotNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "output snapshot every 1 seconds";
	        RunAssertion18(stmtText, "first");
	    }

	    private void RunAssertion12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 25d}});
	        expected.AddResultInsert(800, 1, new object[][] { new object[] {"MSFT", 34d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 58d}});
	        expected.AddResultInsert(1500, 2, new object[][] { new object[] {"YAH", 59d}});
	        expected.AddResultInsert(2100, 1, new object[][] { new object[] {"IBM", 85d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 87d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 109d}});
	        expected.AddResultInsert(4900, 1, new object[][] { new object[] {"YAH", 112d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 87d}});
	        expected.AddResultInsert(5900, 1, new object[][] { new object[] {"YAH", 88d}});
	        expected.AddResultRemove(6300, 0, new object[][] { new object[] {"MSFT", 79d}});
	        expected.AddResultRemove(7000, 0, new object[][] { new object[] {"IBM", 54d},  new object[] {"YAH", 54d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion34(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 109d}});
	        expected.AddResultInsert(4900, 1, new object[][] { new object[] {"YAH", 112d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion13_14(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"MSFT", 34d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 85d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 87d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"YAH", 112d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 88d}}, new object[][] { new object[] {"IBM", 87d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"YAH", 54d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion15_16(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsRem(2200, 0, null, null);
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"YAH", 112d}});
	        expected.AddResultInsRem(6200, 0, null, null);
	        expected.AddResultInsRem(7200, 0, null, null);

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion78(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsRem(2200, 0, null, null);
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 109d},  new object[] {"YAH", 112d}}, null);
	        expected.AddResultInsRem(6200, 0, null, null);
	        expected.AddResultInsRem(7200, 0, null, null);

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion56(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 34d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 58d},  new object[] {"YAH", 59d},  new object[] {"IBM", 85d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 87d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 109d},  new object[] {"YAH", 112d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 88d}}, new object[][] { new object[] {"IBM", 87d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"MSFT", 79d},  new object[] {"IBM", 54d},  new object[] {"YAH", 54d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion17IStreamOnly(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 25d}});
	        expected.AddResultInsert(1500, 1, new object[][]{ new object[] {"IBM", 58d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 87d}});
	        expected.AddResultInsert(4300, 1, new object[][]{ new object[] {"IBM", 109d}});
	        expected.AddResultInsert(5900, 1, new object[][]{ new object[] {"YAH", 88d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
	        execution.Execute(false);
	    }

	    private void RunAssertion17IRStream(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 25d}});
	        expected.AddResultInsert(1500, 1, new object[][]{ new object[] {"IBM", 58d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 87d}});
	        expected.AddResultInsert(4300, 1, new object[][]{ new object[] {"IBM", 109d}});
	        expected.AddResultRemove(5700, 0, new object[][]{ new object[] {"IBM", 87d}});
	        expected.AddResultRemove(6300, 0, new object[][]{ new object[] {"MSFT", 79d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
	        execution.Execute(false);
	    }

	    private void RunAssertion18(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 34d},  new object[] {"MSFT", 34d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 85d},  new object[] {"MSFT", 85d},  new object[] {"IBM", 85d},  new object[] {"YAH", 85d},  new object[] {"IBM", 85d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 85d},  new object[] {"MSFT", 85d},  new object[] {"IBM", 85d},  new object[] {"YAH", 85d},  new object[] {"IBM", 85d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 87d},  new object[] {"MSFT", 87d},  new object[] {"IBM", 87d},  new object[] {"YAH", 87d},  new object[] {"IBM", 87d},  new object[] {"YAH", 87d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 112d},  new object[] {"MSFT", 112d},  new object[] {"IBM", 112d},  new object[] {"YAH", 112d},  new object[] {"IBM", 112d},  new object[] {"YAH", 112d},  new object[] {"IBM", 112d},  new object[] {"YAH", 112d}});
	        expected.AddResultInsert(6200, 0, new object[][] { new object[] {"MSFT", 88d},  new object[] {"IBM", 88d},  new object[] {"YAH", 88d},  new object[] {"IBM", 88d},  new object[] {"YAH", 88d},  new object[] {"IBM", 88d},  new object[] {"YAH", 88d},  new object[] {"YAH", 88d}});
	        expected.AddResultInsert(7200, 0, new object[][] { new object[] {"IBM", 54d},  new object[] {"YAH", 54d},  new object[] {"IBM", 54d},  new object[] {"YAH", 54d},  new object[] {"YAH", 54d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void TestHaving()
	    {
	        SendTimer(0);

	        var viewExpr = "select Symbol, avg(Price) as avgPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:time(3 sec) " +
	                          "having avg(Price) > 10" +
	                          "output every 1 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        RunHavingAssertion();
	    }

        [Test]
	    public void TestHavingJoin()
	    {
	        SendTimer(0);

	        var viewExpr = "select Symbol, avg(Price) as avgPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:time(3 sec) as md, " +
	                          typeof(SupportBean).FullName + ".win:keepall() as s where s.TheString = md.Symbol " +
	                          "having avg(Price) > 10" +
	                          "output every 1 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("SYM1", -1));

	        RunHavingAssertion();
	    }

	    private void RunHavingAssertion()
	    {
	        SendEvent("SYM1", 10d);
	        SendEvent("SYM1", 11d);
	        SendEvent("SYM1", 9);

	        SendTimer(1000);
	        var fields = "Symbol,avgPrice".Split(',');
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"SYM1", 10.5});

	        SendEvent("SYM1", 13d);
	        SendEvent("SYM1", 10d);
	        SendEvent("SYM1", 9);
	        SendTimer(2000);

	        Assert.AreEqual(3, _listener.LastNewData.Length);
	        Assert.IsNull(_listener.LastOldData);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{ new object[] {"SYM1", 43 / 4.0},  new object[] {"SYM1", 53.0 / 5.0},  new object[] {"SYM1", 62 / 6.0}});
	    }

        [Test]
	    public void TestMaxTimeWindow()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Volume, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + ".win:time(1 sec) " +
	                          "output every 1 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        SendEvent("SYM1", 1d);
	        SendEvent("SYM1", 2d);
	        _listener.Reset();

	        // moves all events out of the window,
	        SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
	        UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
	        Assert.AreEqual(2, result.First.Length);
	        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
	        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
	        Assert.AreEqual(2, result.Second.Length);
	        Assert.AreEqual(null, result.Second[0].Get("maxVol"));
	        Assert.AreEqual(null, result.Second[1].Get("maxVol"));
	    }

        [Test]
	    public void TestLimitSnapshot()
	    {
	        SendTimer(0);
	        var selectStmt = "select Symbol, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
	                ".win:time(10 seconds) output snapshot every 1 seconds order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);
	        SendEvent("ABC", 20);

	        SendTimer(500);
	        SendEvent("IBM", 16);
	        SendEvent("MSFT", 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "sumPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 50d},  new object[] {"IBM", 50d},  new object[] {"MSFT", 50d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendEvent("YAH", 18);
	        SendEvent("s4", 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 98d},  new object[] {"IBM", 98d},  new object[] {"MSFT", 98d},  new object[] {"YAH", 98d},  new object[] {"s4", 98d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"YAH", 48d},  new object[] {"s4", 48d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(12000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(13000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();
	    }

        [Test]
	    public void TestLimitSnapshotJoin()
	    {
	        SendTimer(0);
	        var selectStmt = "select Symbol, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
	                ".win:time(10 seconds) as m, " + typeof(SupportBean).FullName +
	                ".win:keepall() as s where s.TheString = m.Symbol output snapshot every 1 seconds order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("ABC", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("MSFT", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("YAH", 4));
	        _epService.EPRuntime.SendEvent(new SupportBean("s4", 5));

	        SendEvent("ABC", 20);

	        SendTimer(500);
	        SendEvent("IBM", 16);
	        SendEvent("MSFT", 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "sumPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 50d},  new object[] {"IBM", 50d},  new object[] {"MSFT", 50d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendEvent("YAH", 18);
	        SendEvent("s4", 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 98d},  new object[] {"IBM", 98d},  new object[] {"MSFT", 98d},  new object[] {"YAH", 98d},  new object[] {"s4", 98d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(10500);
	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"YAH", 48d},  new object[] {"s4", 48d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(11500);
	        SendTimer(12000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(13000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();
	    }

        [Test]
	    public void TestJoinSortWindow()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Volume, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(1, Volume desc) as s0," +
	                          typeof(SupportBean).FullName + ".win:keepall() as s1 " +
	                          "output every 1 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));

	        SendEvent("JOIN_KEY", 1d);
	        SendEvent("JOIN_KEY", 2d);
	        _listener.Reset();

	        // moves all events out of the window,
	        SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
	        UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
	        Assert.AreEqual(2, result.First.Length);
	        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
	        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
	        Assert.AreEqual(1, result.Second.Length);
	        Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));
	    }

        [Test]
	    public void TestAggregateAllNoJoinLast() {
	        RunAssertionAggregateAllNoJoinLast(true);
	        RunAssertionAggregateAllNoJoinLast(false);
	    }

		private void RunAssertionAggregateAllNoJoinLast(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

		    var viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
	                        "from " + typeof(SupportBean).FullName + ".win:length(3) " +
	                        "having sum(LongBoxed) > 0 " +
	                        "output last every 2 events";

		    RunAssertLastSum(CreateStmtAndListenerNoJoin(viewExpr));

		    viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
	                    "from " + typeof(SupportBean).FullName + ".win:length(3) " +
	                    "output last every 2 events";
		    RunAssertLastSum(CreateStmtAndListenerNoJoin(viewExpr));
		}

        [Test]
	    public void TestAggregateAllJoinAll() {
	        RunAssertionAggregateAllJoinAll(true);
	        RunAssertionAggregateAllJoinAll(false);
	    }

		private void RunAssertionAggregateAllJoinAll(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

		    var viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
	                        "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
	                        typeof(SupportBean).FullName + ".win:length(3) as two " +
	                        "having sum(LongBoxed) > 0 " +
	                        "output all every 2 events";

		    RunAssertAllSum(CreateStmtAndListenerJoin(viewExpr));

		    viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
	                    "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
	                    typeof(SupportBean).FullName + ".win:length(3) as two " +
	                    "output every 2 events";

		    RunAssertAllSum(CreateStmtAndListenerJoin(viewExpr));
		}

        [Test]
		public void TestAggregateAllJoinLast()
	    {
	        var viewExpr = "select LongBoxed, sum(LongBoxed) as result " +
	        "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
	        typeof(SupportBean).FullName + ".win:length(3) as two " +
	        "having sum(LongBoxed) > 0 " +
	        "output last every 2 events";

	        RunAssertLastSum(CreateStmtAndListenerJoin(viewExpr));

	        viewExpr = "select LongBoxed, sum(LongBoxed) as result " +
	        "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
	        typeof(SupportBean).FullName + ".win:length(3) as two " +
	        "output last every 2 events";

	        RunAssertLastSum(CreateStmtAndListenerJoin(viewExpr));
	    }

        [Test]
	    public void TestTime()
	    {
	        // Set the clock to 0
	        _currentTime = 0;
	        SendTimeEventRelative(0);

	        // Create the EPL statement and add a listener
	        var statementText = "select Symbol, sum(Volume) from " + EVENT_NAME + ".win:length(5) output first every 3 seconds";
	        var statement = _epService.EPAdministrator.CreateEPL(statementText);
	        var updateListener = new SupportUpdateListener();
	        statement.AddListener(updateListener);
	        updateListener.Reset();

	        // Send the first event of the batch; should be output
	        SendMarketDataEvent(10L);
	        AssertEvent(updateListener, 10L);

	        // Send another event, not the first, for aggregation
	        // update only, no output
	        SendMarketDataEvent(20L);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Update time
	        SendTimeEventRelative(3000);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Send first event of the next batch, should be output.
	        // The aggregate value is computed over all events
	        // received: 10 + 20 + 30 = 60
	        SendMarketDataEvent(30L);
	        AssertEvent(updateListener, 60L);

	        // Send the next event of the batch, no output
	        SendMarketDataEvent(40L);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Update time
	        SendTimeEventRelative(3000);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Send first event of third batch
	        SendMarketDataEvent(1L);
	        AssertEvent(updateListener, 101L);

	        // Update time
	        SendTimeEventRelative(3000);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Update time: no first event this batch, so a callback
	        // is made at the end of the interval
	        SendTimeEventRelative(3000);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestCount()
	    {
	        // Create the EPL statement and add a listener
	        var statementText = "select Symbol, sum(Volume) from " + EVENT_NAME + ".win:length(5) output first every 3 events";
	        var statement = _epService.EPAdministrator.CreateEPL(statementText);
	        var updateListener = new SupportUpdateListener();
	        statement.AddListener(updateListener);
	        updateListener.Reset();

	        // Send the first event of the batch, should be output
	        SendEventLong(10L);
	        AssertEvent(updateListener, 10L);

	        // Send the second event of the batch, not output, used
	        // for updating the aggregate value only
	        SendEventLong(20L);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // Send the third event of the batch, still not output,
	        // but should reset the batch
	        SendEventLong(30L);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	        // First event, next batch, aggregate value should be
	        // 10 + 20 + 30 + 40 = 100
	        SendEventLong(40L);
	        AssertEvent(updateListener, 100L);

	        // Next event again not output
	        SendEventLong(50L);
	        Assert.IsFalse(updateListener.GetAndClearIsInvoked());
	    }

	    private void SendEventLong(long volume)
	    {
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DELL", 0.0, volume, null));
	    }

	    private SupportUpdateListener CreateStmtAndListenerNoJoin(string viewExpr)
        {
			_epService.Initialize();
			var updateListener = new SupportUpdateListener();
			var view = _epService.EPAdministrator.CreateEPL(viewExpr);
		    view.AddListener(updateListener);

		    return updateListener;
		}

		private void RunAssertAllSum(SupportUpdateListener updateListener)
		{
			// send an event
		    SendEvent(1);

		    // check no update
		    Assert.IsFalse(updateListener.GetAndClearIsInvoked());

		    // send another event
		    SendEvent(2);

		    // check update, all events present
		    Assert.IsTrue(updateListener.GetAndClearIsInvoked());
		    Assert.AreEqual(2, updateListener.LastNewData.Length);
		    Assert.AreEqual(1L, updateListener.LastNewData[0].Get("LongBoxed"));
		    Assert.AreEqual(1L, updateListener.LastNewData[0].Get("result"));
		    Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
		    Assert.AreEqual(3L, updateListener.LastNewData[1].Get("result"));
		    Assert.IsNull(updateListener.LastOldData);
		}

		private void RunAssertLastSum(SupportUpdateListener updateListener)
		{
			// send an event
		    SendEvent(1);

		    // check no update
		    Assert.IsFalse(updateListener.GetAndClearIsInvoked());

		    // send another event
		    SendEvent(2);

		    // check update, all events present
		    Assert.IsTrue(updateListener.GetAndClearIsInvoked());
		    Assert.AreEqual(1, updateListener.LastNewData.Length);
		    Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
		    Assert.AreEqual(3L, updateListener.LastNewData[0].Get("result"));
		    Assert.IsNull(updateListener.LastOldData);
		}

	    private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
		{
		    var bean = new SupportBean();
		    bean.TheString = JOIN_KEY;
		    bean.LongBoxed = longBoxed;
		    bean.IntBoxed = intBoxed;
		    bean.ShortBoxed = shortBoxed;
		    _epService.EPRuntime.SendEvent(bean);
		}

		private void SendEvent(long longBoxed)
		{
		    SendEvent(longBoxed, 0, (short)0);
		}

	    private void SendMarketDataEvent(long volume)
	    {
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM1", 0, volume, null));
	    }

	    private void SendTimeEventRelative(int timeIncrement){
	        _currentTime += timeIncrement;
	        var theEvent = new CurrentTimeEvent(_currentTime);
	        _epService.EPRuntime.SendEvent(theEvent);
	    }

		private SupportUpdateListener CreateStmtAndListenerJoin(string viewExpr)
        {
			_epService.Initialize();

			var updateListener = new SupportUpdateListener();
			var view = _epService.EPAdministrator.CreateEPL(viewExpr);
		    view.AddListener(updateListener);

		    _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

		    return updateListener;
		}

	    private void AssertEvent(SupportUpdateListener updateListener, long volume)
	    {
	        Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	        Assert.IsTrue(updateListener.LastNewData != null);
	        Assert.AreEqual(1, updateListener.LastNewData.Length);
	        Assert.AreEqual(volume, updateListener.LastNewData[0].Get("sum(Volume)"));
	    }

	    private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void SendTimer(long time)
	    {
	        var theEvent = new CurrentTimeEvent(time);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }
	}
} // end of namespace
