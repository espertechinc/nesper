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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitSimple 
	{
	    private const string JOIN_KEY = "KEY";
	    private const string CATEGORY = "Un-aggregated and Un-grouped";

	    private EPServiceProvider _epService;
	    private long _currentTime;
	    private SupportUpdateListener _listener;

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
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec)";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test2NoneNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test3NoneHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            " having Price > 10";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test4NoneHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            " having Price > 10";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test5DefaultNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test6DefaultNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test7DefaultHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) \n" +
	                            "having Price > 10" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test8DefaultHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "having Price > 10" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec) " +
	                "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec), " +
	                "SupportBean#keepall where TheString=Symbol " +
	                "output all every 1 seconds";
	        RunAssertion56(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            "having Price > 10" +
	                            "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec) " +
	                "having Price > 10" +
	                "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "having Price > 10" +
	                            "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec), " +
	                "SupportBean#keepall where TheString=Symbol " +
	                "having Price > 10" +
	                "output all every 1 seconds";
	        RunAssertion78(stmtText, "all");
	    }

        [Test]
	    public void Test13LastNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec)" +
	                            "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test14LastNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "output last every 1 seconds";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec)" +
	                            "having Price > 10 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "having Price > 10 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoinIStream()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            "output first every 1 seconds";
	        RunAssertion17IStream(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingJoinIStream()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec)," +
	                "SupportBean#keepall where TheString=Symbol " +
	                "output first every 1 seconds";
	        RunAssertion17IStream(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoinIRStream()
	    {
	        var stmtText = "select irstream Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec) " +
	                "output first every 1 seconds";
	        RunAssertion17IRStream(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingJoinIRStream()
	    {
	        var stmtText = "select irstream Symbol, Volume, Price " +
	                "from MarketData#time(5.5 sec), " +
	                "SupportBean#keepall where TheString=Symbol " +
	                "output first every 1 seconds";
	        RunAssertion17IRStream(stmtText, "first");
	    }

        [Test]
	    public void Test18SnapshotNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, Price " +
	                            "from MarketData#time(5.5 sec) " +
	                            "output snapshot every 1 seconds";
	        RunAssertion18(stmtText, "first");
	    }

        [Test]
	    public void TestOutputFirstUnidirectionalJoinNamedWindow() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

	        var fields = "c0,c1".Split(',');
	        var epl =
	                "create window MyWindow#keepall as SupportBean_S0;\n" +
	                "insert into MyWindow select * from SupportBean_S0;\n" +
                    "@Name('join') select myWindow.id as c0, s1.id as c1\n" +
	                "from SupportBean_S1 as s1 unidirectional, MyWindow as myWindow\n" +
	                "where myWindow.p00 = s1.p10\n" +
	                "output first every 1 minutes;";
	        _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("join").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "b"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1000, "b"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {20, 1000});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1001, "b"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1002, "a"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(60*1000));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1003, "a"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1003});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1004, "a"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(120*1000));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1005, "a"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1005});
	    }

        [Test]
	    public void TestOutputEveryTimePeriod()
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

	        var stmtText = "select Symbol from MarketData#keepall output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        SendMDEvent("E1", 0);

	        long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
	        var deltaMSec = deltaSec * 1000 + 5 + 2000;
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
	        Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("Symbol"));
	    }

        [Test]
	    public void TestOutputEveryTimePeriodVariable()
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        _epService.EPAdministrator.Configuration.AddVariable("D", typeof(int), 1);
	        _epService.EPAdministrator.Configuration.AddVariable("H", typeof(int), 2);
	        _epService.EPAdministrator.Configuration.AddVariable("M", typeof(int), 3);
	        _epService.EPAdministrator.Configuration.AddVariable("S", typeof(int), 4);
	        _epService.EPAdministrator.Configuration.AddVariable("MS", typeof(int), 5);

	        var stmtText = "select Symbol from MarketData#keepall output snapshot every D days H hours M minutes S seconds MS milliseconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        SendMDEvent("E1", 0);

	        long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
	        var deltaMSec = deltaSec * 1000 + 5 + 2000;
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
	        Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("Symbol"));

	        // test statement model
	        var model = _epService.EPAdministrator.CompileEPL(stmtText);
	        Assert.AreEqual(stmtText, model.ToEPL());
	    }

	    private void RunAssertion34(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        var fields = new string[] {"Symbol", "Volume", "Price"};

	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 24d}});
	        expected.AddResultInsert(2100, 1, new object[][] { new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(7000, 0, new object[][] { new object[] {"IBM", 150L, 24d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion15_16(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"IBM", 150L, 24d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(800, 1, new object[][] { new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 24d}});
	        expected.AddResultInsert(1500, 2, new object[][] { new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsert(2100, 1, new object[][] { new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultInsert(4900, 1, new object[][] { new object[] {"YAH", 11500L, 3d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(5900, 1, new object[][] { new object[] {"YAH", 10500L, 1d}});
	        expected.AddResultRemove(6300, 0, new object[][] { new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultRemove(7000, 0, new object[][] { new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion13_14(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"YAH", 11500L, 3d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 10500L, 1d}}, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"YAH", 10000L, 1d}, });

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion78(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 150L, 24d},  new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"IBM", 150L, 24d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion56(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 22d},  new object[] {"YAH", 11500L, 3d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 10500L, 1d}}, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d}, });

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion17IStream(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 24d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultInsert(5900, 1, new object[][]{ new object[] {"YAH", 10500L, 1.0d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
	        execution.Execute(false);
	    }

	    private void RunAssertion17IRStream(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 24d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 22d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultRemove(6300, 0, new object[][] { new object[] {"MSFT", 5000L, 9d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
	        execution.Execute(false);
	    }

	    private void RunAssertion18(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "Price"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d},  new object[] {"YAH", 11000L, 2d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d},  new object[] {"YAH", 11000L, 2d},  new object[] {"IBM", 150L, 22d},  new object[] {"YAH", 11500L, 3d}});
	        expected.AddResultInsert(6200, 0, new object[][] { new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 24d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 26d},  new object[] {"YAH", 11000L, 2d},  new object[] {"IBM", 150L, 22d},  new object[] {"YAH", 11500L, 3d},  new object[] {"YAH", 10500L, 1d}});
	        expected.AddResultInsert(7200, 0, new object[][] { new object[] {"IBM", 155L, 26d},  new object[] {"YAH", 11000L, 2d},  new object[] {"IBM", 150L, 22d},  new object[] {"YAH", 11500L, 3d},  new object[] {"YAH", 10500L, 1d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void TestAggAllHaving()
	    {
	        var stmtText = "select Symbol, Volume " +
	                            "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as two " +
	                            "having Volume > 0 " +
	                            "output every 5 events";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);
	        var fields = new string[] {"Symbol", "Volume"};

	        SendMDEvent("S0", 20);
	        SendMDEvent("IBM", -1);
	        SendMDEvent("MSFT", -2);
	        SendMDEvent("YAH", 10);
	        Assert.IsFalse(listener.IsInvoked);

	        SendMDEvent("IBM", 0);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{ new object[] {"S0", 20L},  new object[] {"YAH", 10L}});
	        listener.Reset();
	    }

        [Test]
	    public void TestAggAllHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume " +
	                            "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as one," +
	                            typeof(SupportBean).FullName + "#length(10) as two " +
	                            "where one.Symbol=two.TheString " +
	                            "having Volume > 0 " +
	                            "output every 5 events";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);
	        var fields = new string[] {"Symbol", "Volume"};
	        _epService.EPRuntime.SendEvent(new SupportBean("S0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("MSFT", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("YAH", 0));

	        SendMDEvent("S0", 20);
	        SendMDEvent("IBM", -1);
	        SendMDEvent("MSFT", -2);
	        SendMDEvent("YAH", 10);
	        Assert.IsFalse(listener.IsInvoked);

	        SendMDEvent("IBM", 0);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{ new object[] {"S0", 20L},  new object[] {"YAH", 10L}});
	        listener.Reset();
	    }

        [Test]
	    public void TestIterator()
		{
	        var fields = new string[] {"Symbol", "Price"};
	        var statementString = "select Symbol, TheString, Price from " +
	    	            typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
	    	            typeof(SupportBeanString).FullName + "#length(100) as two " +
	                    "where one.Symbol = two.TheString " +
	                    "output every 3 events";
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));

	        // Output limit clause ignored when iterating, for both joins and no-join
	        SendEvent("CAT", 50);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{ new object[] {"CAT", 50d}});

	        SendEvent("CAT", 60);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, new object[][]{ new object[] {"CAT", 50d},  new object[] {"CAT", 60d}});

	        SendEvent("IBM", 70);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, new object[][]{ new object[] {"CAT", 50d},  new object[] {"CAT", 60d},  new object[] {"IBM", 70d}});

	        SendEvent("IBM", 90);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, new object[][]{ new object[] {"CAT", 50d},  new object[] {"CAT", 60d},  new object[] {"IBM", 70d},  new object[] {"IBM", 90d}});
	    }

        [Test]
	    public void TestLimitEventJoin()
		{
			var eventName1 = typeof(SupportBean).FullName;
			var eventName2 = typeof(SupportBean_A).FullName;
			var joinStatement =
				"select * from " +
					eventName1 + "#length(5) as event1," +
					eventName2 + "#length(5) as event2" +
				" where event1.TheString = event2.id";
			var outputStmt1 = joinStatement + " output every 1 events";
		   	var outputStmt3 = joinStatement + " output every 3 events";

		   	var fireEvery1 = _epService.EPAdministrator.CreateEPL(outputStmt1);
			var fireEvery3 = _epService.EPAdministrator.CreateEPL(outputStmt3);

		   	var updateListener1 = new SupportUpdateListener();
			fireEvery1.AddListener(updateListener1);
			var updateListener3 = new SupportUpdateListener();
			fireEvery3.AddListener(updateListener3);

			// send event 1
			SendJoinEvents("IBM");

			Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
			Assert.AreEqual(1, updateListener1.LastNewData.Length);
			Assert.IsNull(updateListener1.LastOldData);

			Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
			Assert.IsNull(updateListener3.LastNewData);
			Assert.IsNull(updateListener3.LastOldData);

			// send event 2
			SendJoinEvents("MSFT");

			Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
			Assert.AreEqual(1, updateListener1.LastNewData.Length);
			Assert.IsNull(updateListener1.LastOldData);

		   	Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
			Assert.IsNull(updateListener3.LastNewData);
			Assert.IsNull(updateListener3.LastOldData);

			// send event 3
			SendJoinEvents("YAH");

			Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
			Assert.AreEqual(1, updateListener1.LastNewData.Length);
			Assert.IsNull(updateListener1.LastOldData);

			Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
			Assert.AreEqual(3, updateListener3.LastNewData.Length);
			Assert.IsNull(updateListener3.LastOldData);
		}

        [Test]
	    public void TestLimitTime(){
	    	var eventName = typeof(SupportBean).FullName;
	    	var selectStatement = "select * from " + eventName + "#length(5)";

	    	// test integer seconds
	    	var statementString1 = selectStatement +
	    		" output every 3 seconds";
	    	TimeCallback(statementString1, 3000);

	    	// test fractional seconds
	    	var statementString2 = selectStatement +
	    	" output every 3.3 seconds";
	    	TimeCallback(statementString2, 3300);

	    	// test integer minutes
	    	var statementString3 = selectStatement +
	    	" output every 2 minutes";
	    	TimeCallback(statementString3, 120000);

	    	// test fractional minutes
	    	var statementString4 =
	    		"select * from " +
	    			eventName + "#length(5)" +
	    		" output every .05 minutes";
	    	TimeCallback(statementString4, 3000);
	    }

        [Test]
	    public void TestTimeBatchOutputEvents()
	    {
	        var stmtText = "select * from " + typeof(SupportBean).FullName + "#time_batch(10 seconds) output every 10 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        SendTimer(0);
	        SendTimer(10000);
	        Assert.IsFalse(listener.IsInvoked);
	        SendTimer(20000);
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent("e1");
	        SendTimer(30000);
	        Assert.IsFalse(listener.IsInvoked);
	        SendTimer(40000);
	        var newEvents = listener.GetAndResetLastNewData();
	        Assert.AreEqual(1, newEvents.Length);
	        Assert.AreEqual("e1", newEvents[0].Get("TheString"));
	        listener.Reset();

	        SendTimer(50000);
	        Assert.IsTrue(listener.IsInvoked);
	        listener.Reset();

	        SendTimer(60000);
	        Assert.IsTrue(listener.IsInvoked);
	        listener.Reset();

	        SendTimer(70000);
	        Assert.IsTrue(listener.IsInvoked);
	        listener.Reset();

	        SendEvent("e2");
	        SendEvent("e3");
	        SendTimer(80000);
	        newEvents = listener.GetAndResetLastNewData();
	        Assert.AreEqual(2, newEvents.Length);
	        Assert.AreEqual("e2", newEvents[0].Get("TheString"));
	        Assert.AreEqual("e3", newEvents[1].Get("TheString"));

	        SendTimer(90000);
	        Assert.IsTrue(listener.IsInvoked);
	        listener.Reset();
	    }

        [Test]
	    public void TestSimpleNoJoinAll() {
	        RunAssertionSimpleNoJoinAll(false);
	        RunAssertionSimpleNoJoinAll(true);
	    }

	    public void RunAssertionSimpleNoJoinAll(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt')" : "";
		    var viewExpr = hint + "select LongBoxed " +
	                          "from " + typeof(SupportBean).FullName + "#length(3) " +
	                          "output all every 2 events";

		    RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));

		    viewExpr =  hint + "select LongBoxed " +
		                "from " + typeof(SupportBean).FullName + "#length(3) " +
		                "output every 2 events";

		    RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));

		    viewExpr =  hint + "select * " +
		                "from " + typeof(SupportBean).FullName + "#length(3) " +
		                "output every 2 events";

		    RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));
		}

        [Test]
		public void TestSimpleNoJoinLast()
	    {
	        var viewExpr = "select LongBoxed " +
	        "from " + typeof(SupportBean).FullName + "#length(3) " +
	        "output last every 2 events";

	        RunAssertLast(CreateStmtAndListenerNoJoin(viewExpr));

	        viewExpr = "select * " +
	        "from " + typeof(SupportBean).FullName + "#length(3) " +
	        "output last every 2 events";

	        RunAssertLast(CreateStmtAndListenerNoJoin(viewExpr));
	    }

        [Test]
	    public void TestSimpleJoinAll()
		{
	        RunAssertionSimpleJoinAll(false);
	        RunAssertionSimpleJoinAll(true);
	    }

	    private void RunAssertionSimpleJoinAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt')" : "";
	        var viewExpr = hint + "select LongBoxed  " +
	                "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
	                typeof(SupportBean).FullName + "#length(3) as two " +
	                "output all every 2 events";

	        RunAssertAll(CreateStmtAndListenerJoin(viewExpr));
	    }

	    private SupportUpdateListener CreateStmtAndListenerNoJoin(string viewExpr) {
			_epService.Initialize();
			var updateListener = new SupportUpdateListener();
			var view = _epService.EPAdministrator.CreateEPL(viewExpr);
		    view.AddListener(updateListener);

		    return updateListener;
		}

		private void RunAssertAll(SupportUpdateListener updateListener)
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
		    Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
		    Assert.IsNull(updateListener.LastOldData);
		}

	    private void SendEvent(long LongBoxed, int IntBoxed, short shortBoxed)
		{
		    var bean = new SupportBean();
		    bean.TheString = JOIN_KEY;
		    bean.LongBoxed = LongBoxed;
		    bean.IntBoxed = IntBoxed;
		    bean.ShortBoxed = shortBoxed;
		    _epService.EPRuntime.SendEvent(bean);
		}

		private void SendEvent(long LongBoxed)
		{
		    SendEvent(LongBoxed, 0, (short)0);
		}

        [Test]
		public void TestSimpleJoinLast()
		{
		    var viewExpr = "select LongBoxed " +
		    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
		    typeof(SupportBean).FullName + "#length(3) as two " +
		    "output last every 2 events";

			RunAssertLast(CreateStmtAndListenerJoin(viewExpr));
		}

        [Test]
	    public void TestLimitEventSimple()
	    {
	        var updateListener1 = new SupportUpdateListener();
	        var updateListener2 = new SupportUpdateListener();
	        var updateListener3 = new SupportUpdateListener();

	        var eventName = typeof(SupportBean).FullName;
	        var selectStmt = "select * from " + eventName + "#length(5)";
	        var statement1 = selectStmt +
	            " output every 1 events";
	        var statement2 = selectStmt +
	            " output every 2 events";
	        var statement3 = selectStmt +
	            " output every 3 events";

	        var rateLimitStmt1 = _epService.EPAdministrator.CreateEPL(statement1);
	        rateLimitStmt1.AddListener(updateListener1);
	        var rateLimitStmt2 = _epService.EPAdministrator.CreateEPL(statement2);
	        rateLimitStmt2.AddListener(updateListener2);
	        var rateLimitStmt3 = _epService.EPAdministrator.CreateEPL(statement3);
	        rateLimitStmt3.AddListener(updateListener3);

	        // send event 1
	        SendEvent("IBM");

	        Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
	        Assert.AreEqual(1,updateListener1.LastNewData.Length);
	        Assert.IsNull(updateListener1.LastOldData);

	        Assert.IsFalse(updateListener2.GetAndClearIsInvoked());
	        Assert.IsNull(updateListener2.LastNewData);
	        Assert.IsNull(updateListener2.LastOldData);

	        Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
	        Assert.IsNull(updateListener3.LastNewData);
	        Assert.IsNull(updateListener3.LastOldData);

	        // send event 2
	        SendEvent("MSFT");

	        Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
	        Assert.AreEqual(1,updateListener1.LastNewData.Length);
	        Assert.IsNull(updateListener1.LastOldData);

	        Assert.IsTrue(updateListener2.GetAndClearIsInvoked());
	        Assert.AreEqual(2,updateListener2.LastNewData.Length);
	        Assert.IsNull(updateListener2.LastOldData);

	        Assert.IsFalse(updateListener3.GetAndClearIsInvoked());

	        // send event 3
	        SendEvent("YAH");

	        Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
	        Assert.AreEqual(1,updateListener1.LastNewData.Length);
	        Assert.IsNull(updateListener1.LastOldData);

	        Assert.IsFalse(updateListener2.GetAndClearIsInvoked());

	        Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
	        Assert.AreEqual(3,updateListener3.LastNewData.Length);
	        Assert.IsNull(updateListener3.LastOldData);
	    }

        [Test]
	    public void TestLimitSnapshot()
	    {
	        var listener = new SupportUpdateListener();

	        SendTimer(0);
	        var selectStmt = "select * from " + typeof(SupportBean).FullName + "#time(10) output snapshot every 3 events";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(listener);

	        SendTimer(1000);
	        SendEvent("IBM");
	        SendEvent("MSFT");
	        Assert.IsFalse(listener.GetAndClearIsInvoked());

	        SendTimer(2000);
	        SendEvent("YAH");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"IBM"},  new object[] {"MSFT"},  new object[] {"YAH"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(3000);
	        SendEvent("s4");
	        SendEvent("s5");
	        Assert.IsFalse(listener.GetAndClearIsInvoked());

	        SendTimer(10000);
	        SendEvent("s6");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"IBM"},  new object[] {"MSFT"},  new object[] {"YAH"},  new object[] {"s4"},  new object[] {"s5"},  new object[] {"s6"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(11000);
	        SendEvent("s7");
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent("s8");
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent("s9");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"YAH"},  new object[] {"s4"},  new object[] {"s5"},  new object[] {"s6"},  new object[] {"s7"},  new object[] {"s8"},  new object[] {"s9"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(14000);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s6"},  new object[] {"s7"},  new object[] {"s8"},  new object[] {"s9"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendEvent("s10");
	        SendEvent("s11");
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimer(23000);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s10"},  new object[] {"s11"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendEvent("s12");
	        Assert.IsFalse(listener.IsInvoked);
	    }

        [Test]
	    public void TestFirstSimpleHavingAndNoHaving() {
	        RunAssertionFirstSimpleHavingAndNoHaving("");
	        RunAssertionFirstSimpleHavingAndNoHaving("having IntPrimitive != 0");
	    }

	    private void RunAssertionFirstSimpleHavingAndNoHaving(string having) {
	        var epl = "select TheString from SupportBean " + having + " output first every 3 events";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E4"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();
	    }

        [Test]
	    public void TestLimitSnapshotJoin()
	    {
	        var listener = new SupportUpdateListener();

	        SendTimer(0);
	        var selectStmt = "select TheString from " + typeof(SupportBean).FullName + "#time(10) as s," +
	                typeof(SupportMarketDataBean).FullName + "#keepall as m where s.TheString = m.Symbol output snapshot every 3 events order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(listener);

	        foreach (var symbol in "s0,s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11".Split(','))
	        {
	            _epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, 0, 0L, ""));
	        }

	        SendTimer(1000);
	        SendEvent("s0");
	        SendEvent("s1");
	        Assert.IsFalse(listener.GetAndClearIsInvoked());

	        SendTimer(2000);
	        SendEvent("s2");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s0"},  new object[] {"s1"},  new object[] {"s2"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(3000);
	        SendEvent("s4");
	        SendEvent("s5");
	        Assert.IsFalse(listener.GetAndClearIsInvoked());

	        SendTimer(10000);
	        SendEvent("s6");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s0"},  new object[] {"s1"},  new object[] {"s2"},  new object[] {"s4"},  new object[] {"s5"},  new object[] {"s6"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(11000);
	        SendEvent("s7");
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent("s8");
	        Assert.IsFalse(listener.IsInvoked);

	        SendEvent("s9");
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s2"},  new object[] {"s4"},  new object[] {"s5"},  new object[] {"s6"},  new object[] {"s7"},  new object[] {"s8"},  new object[] {"s9"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendTimer(14000);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s6"},  new object[] {"s7"},  new object[] {"s8"},  new object[] {"s9"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendEvent("s10");
	        SendEvent("s11");
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimer(23000);
	        EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[] {"TheString"}, new object[][]{ new object[] {"s10"},  new object[] {"s11"}});
	        Assert.IsNull(listener.LastOldData);
	        listener.Reset();

	        SendEvent("s12");
	        Assert.IsFalse(listener.IsInvoked);
	    }

        [Test]
	    public void TestSnapshotMonthScoped() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        SendCurrentTime("2002-02-01T09:00:00.000");
	        _epService.EPAdministrator.CreateEPL("select * from SupportBean#lastevent output snapshot every 1 month").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        SendCurrentTimeWithMinus("2002-03-01T09:00:00.000", 1);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendCurrentTime("2002-03-01T09:00:00.000");
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new object[][] { new object[] {"E1"}});
	    }

        [Test]
	    public void TestFirstMonthScoped() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        SendCurrentTime("2002-02-01T09:00:00.000");
	        _epService.EPAdministrator.CreateEPL("select * from SupportBean#lastevent output first every 1 month").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        SendCurrentTimeWithMinus("2002-03-01T09:00:00.000", 1);
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendCurrentTime("2002-03-01T09:00:00.000");
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new object[][] { new object[] {"E4"}});
	    }

	    private SupportUpdateListener CreateStmtAndListenerJoin(string viewExpr) {
			_epService.Initialize();

			var updateListener = new SupportUpdateListener();
			var view = _epService.EPAdministrator.CreateEPL(viewExpr);
		    view.AddListener(updateListener);

		    _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

		    return updateListener;
		}

		private void RunAssertLast(SupportUpdateListener updateListener)
		{
			// send an event
		    SendEvent(1);

		    // check no update
		    Assert.IsFalse(updateListener.GetAndClearIsInvoked());

		    // send another event
		    SendEvent(2);

		    // check update, only the last event present
		    Assert.IsTrue(updateListener.GetAndClearIsInvoked());
		    Assert.AreEqual(1, updateListener.LastNewData.Length);
		    Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
		    Assert.IsNull(updateListener.LastOldData);
		}

	    private void SendTimer(long time)
	    {
	        var theEvent = new CurrentTimeEvent(time);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }

	    private void SendEvent(string s)
		{
		    var bean = new SupportBean();
		    bean.TheString = s;
		    bean.DoubleBoxed = 0.0;
		    bean.IntPrimitive = 0;
		    bean.IntBoxed = 0;
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void TimeCallback(string statementString, int timeToCallback) {
	    	// clear any old events
	        _epService.Initialize();

	    	// set the clock to 0
	    	_currentTime = 0;
	    	SendTimeEvent(0);

	    	// create the EPL statement and add a listener
	    	var statement = _epService.EPAdministrator.CreateEPL(statementString);
	    	var updateListener = new SupportUpdateListener();
	    	statement.AddListener(updateListener);
	    	updateListener.Reset();

	    	// send an event
	    	SendEvent("IBM");

	    	// check that the listener hasn't been updated
	        SendTimeEvent(timeToCallback - 1);
	    	Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	    	// update the clock
	    	SendTimeEvent(timeToCallback);

	    	// check that the listener has been updated
	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	Assert.AreEqual(1, updateListener.LastNewData.Length);
	    	Assert.IsNull(updateListener.LastOldData);

	    	// send another event
	    	SendEvent("MSFT");

	    	// check that the listener hasn't been updated
	    	Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	    	// update the clock
	    	SendTimeEvent(timeToCallback);

	    	// check that the listener has been updated
	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	Assert.AreEqual(1, updateListener.LastNewData.Length);
	    	Assert.IsNull(updateListener.LastOldData);

	    	// don't send an event
	    	// check that the listener hasn't been updated
	    	Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	    	// update the clock
	    	SendTimeEvent(timeToCallback);

	    	// check that the listener has been updated
	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	Assert.IsNull(updateListener.LastNewData);
	    	Assert.IsNull(updateListener.LastOldData);

	    	// don't send an event
	    	// check that the listener hasn't been updated
	    	Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	    	// update the clock
	    	SendTimeEvent(timeToCallback);

	    	// check that the listener has been updated
	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	Assert.IsNull(updateListener.LastNewData);
	    	Assert.IsNull(updateListener.LastOldData);

	    	// send several events
	    	SendEvent("YAH");
	    	SendEvent("s4");
	    	SendEvent("s5");

	    	// check that the listener hasn't been updated
	    	Assert.IsFalse(updateListener.GetAndClearIsInvoked());

	    	// update the clock
	    	SendTimeEvent(timeToCallback);

	    	// check that the listener has been updated
	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	Assert.AreEqual(3, updateListener.LastNewData.Length);
	    	Assert.IsNull(updateListener.LastOldData);
	    }

	    private void SendTimeEvent(int timeIncrement){
	    	_currentTime += timeIncrement;
	        var theEvent = new CurrentTimeEvent(_currentTime);
	        _epService.EPRuntime.SendEvent(theEvent);
	    }

	    private void SendJoinEvents(string s)
		{
		    var event1 = new SupportBean();
		    event1.TheString = s;
		    event1.DoubleBoxed = 0.0;
		    event1.IntPrimitive = 0;
		    event1.IntBoxed = 0;

		    var event2 = new SupportBean_A(s);

		    _epService.EPRuntime.SendEvent(event1);
		    _epService.EPRuntime.SendEvent(event2);
		}

	    private void SendMDEvent(string symbol, long volume)
		{
		    var bean = new SupportMarketDataBean(symbol, 0, volume, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void SendEvent(string symbol, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendCurrentTime(string time) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
	    }

	    private void SendCurrentTimeWithMinus(string time, long minus) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
	    }
	}
} // end of namespace
