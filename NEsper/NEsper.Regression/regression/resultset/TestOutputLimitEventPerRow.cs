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
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitEventPerRow 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private const string CATEGORY = "Aggregated and Grouped";

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
	    public void TestUnaggregatedOutputFirst() {
	        SendTimer(0);

	        var fields = "TheString,IntPrimitive".Split(',');
	        var epl = "select * from SupportBean\n" +
	                "     group by TheString\n" +
	                "     output first every 10 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        Assert.IsFalse(listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 3});

	        SendTimer(5000);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", 4});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
	        Assert.IsFalse(listener.IsInvoked);

	        SendTimer(10000);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
	        Assert.IsFalse(listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 7));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 7});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 8));
	        Assert.IsFalse(listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 9));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 9});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        Assert.IsFalse(listener.IsInvoked);
	    }

        [Test]
	    public void TestOutputFirstHavingJoinNoJoin() {

	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();

	        var stmtText = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
	        TryOutputFirstHaving(stmtText);

	        var stmtTextJoin = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
	                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
	        TryOutputFirstHaving(stmtTextJoin);

	        var stmtTextOrder = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
	        TryOutputFirstHaving(stmtTextOrder);

	        var stmtTextOrderJoin = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
	                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
	        TryOutputFirstHaving(stmtTextOrderJoin);
	    }

	    private void TryOutputFirstHaving(string statementText) {
	        var fields = "TheString,LongPrimitive,value".Split(',');
	        var fieldsLimited = "TheString,value".Split(',');
	        _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.Price");
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

	        SendBeanEvent("E1", 101, 10);
	        SendBeanEvent("E2", 102, 15);
	        SendBeanEvent("E1", 103, 10);
	        SendBeanEvent("E2", 104, 5);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 105, 5);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 105L, 25});

	        SendBeanEvent("E2", 106, -6);    // to 19, does not count toward condition
	        SendBeanEvent("E2", 107, 2);    // to 21, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 108, 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 108L, 22});

	        SendBeanEvent("E2", 109, 1);    // to 23, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 110, 1);     // to 24
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 110L, 24});

	        SendBeanEvent("E2", 111, -10);    // to 14
	        SendBeanEvent("E2", 112, 10);    // to 24, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 113, 0);    // to 24, counts toward condition
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 113L, 24});

	        SendBeanEvent("E2", 114, -10);    // to 14
	        SendBeanEvent("E2", 115, 1);     // to 15
	        SendBeanEvent("E2", 116, 5);     // to 20
	        SendBeanEvent("E2", 117, 0);     // to 20
	        SendBeanEvent("E2", 118, 1);     // to 21    // counts
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 119, 0);    // to 21
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 119L, 21});

	        // remove events
	        SendMDEvent("E2", 0);   // remove 113, 117, 119 (any order of delete!)
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLimited, new object[] {"E2", 21});

	        // remove events
	        SendMDEvent("E2", -10); // remove 111, 114
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLimited, new object[] {"E2", 41});

	        // remove events
	        SendMDEvent("E2", -6);  // since there is 3*0 we output the next one
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLimited, new object[] {"E2", 47});

	        SendMDEvent("E2", 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void Test1NoneNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                          "from MarketData#time(5.5 sec)" +
	                          "group by Symbol";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test2NoneNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                          "group by Symbol";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test3NoneHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            " having sum(Price) > 50";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test4NoneHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test5DefaultNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test6DefaultNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output every 1 seconds";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test7DefaultHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) \n"  +
	                            "group by Symbol " +
	                            "having sum(Price) > 50" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test8DefaultHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output all every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion9_10(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output all every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion9_10(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, sum(Price) " +
	                "from MarketData#time(5.5 sec) " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, sum(Price) " +
	                "from MarketData#time(5.5 sec), " +
	                "SupportBean#keepall where TheString=Symbol " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test13LastNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec)" +
	                            "group by Symbol " +
	                            "output last every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test14LastNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output last every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec)" +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, sum(Price) " +
	                "from MarketData#time(5.5 sec)" +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, Volume, sum(Price) " +
	                "from MarketData#time(5.5 sec), " +
	                "SupportBean#keepall where TheString=Symbol " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output first every 1 seconds";
	        RunAssertion17(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec), " +
	                            "SupportBean#keepall where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output first every 1 seconds";
	        RunAssertion17(stmtText, "first");
	    }

        [Test]
	    public void Test18SnapshotNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, Volume, sum(Price) " +
	                            "from MarketData#time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output snapshot every 1 seconds";
	        RunAssertion18(stmtText, "snapshot");
	    }

	    private void RunAssertion12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(800, 1, new object[][] { new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 49d}});
	        expected.AddResultInsert(1500, 2, new object[][] { new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsert(2100, 1, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultInsert(4900, 1, new object[][] { new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsert(5900, 1, new object[][] { new object[] {"YAH", 10500L, 7d}});
	        expected.AddResultRemove(6300, 0, new object[][] { new object[] {"MSFT", 5000L, null}});
	        expected.AddResultRemove(7000, 0, new object[][] { new object[] {"IBM", 150L, 48d},  new object[] {"YAH", 10000L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion34(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(2100, 1, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultRemove(5700, 0, new object[][] { new object[] {"IBM", 100L, 72d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion13_14(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 75d},  new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d},  new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 10500L, 7d}}, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"IBM", 150L, 48d},  new object[] {"MSFT", 5000L, null},  new object[] {"YAH", 10000L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion15_16(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsRem(7200, 0, null, null);

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion78(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsRem(7200, 0, null, null);

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion56(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 150L, 49d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d},  new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"YAH", 10500L, 7d}}, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultRemove(7200, 0, new object[][] { new object[] {"MSFT", 5000L, null},  new object[] {"IBM", 150L, 48d},  new object[] {"YAH", 10000L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion9_10(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 150L, 49d},  new object[] {"IBM", 155L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 155L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 155L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 150L, 72d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"YAH", 10500L, 7d}}, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsRem(7200, 0, new object[][] { new object[] {"IBM", 150L, 48d},  new object[] {"MSFT", 5000L, null},  new object[] {"YAH", 10500L, 6d}}, new object[][] { new object[] {"IBM", 150L, 48d},  new object[] {"MSFT", 5000L, null},  new object[] {"YAH", 10000L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion11_12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 150L, 72d}}, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsRem(7200, 0, null, null);

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion17(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(200, 1, new object[][] { new object[] {"IBM", 100L, 25d}});
	        expected.AddResultInsert(800, 1, new object[][] { new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(1500, 1, new object[][] { new object[] {"IBM", 150L, 49d}});
	        expected.AddResultInsert(1500, 2, new object[][] { new object[] {"YAH", 10000L, 1d}});
	        expected.AddResultInsert(3500, 1, new object[][] { new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(4300, 1, new object[][] { new object[] {"IBM", 150L, 97d}});
	        expected.AddResultInsert(4900, 1, new object[][] { new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultInsert(5700, 0, new object[][] { new object[] {"IBM", 100L, 72d}});
	        expected.AddResultInsert(5900, 1, new object[][] { new object[] {"YAH", 10500L, 7d}});
	        expected.AddResultInsert(6300, 0, new object[][] { new object[] {"MSFT", 5000L, null}});
	        expected.AddResultInsert(7000, 0, new object[][] { new object[] {"IBM", 150L, 48d},  new object[] {"YAH", 10000L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion18(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "Volume", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 100L, 25d},  new object[] {"MSFT", 5000L, 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 100L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 75d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 100L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 75d},  new object[] {"YAH", 10000L, 1d},  new object[] {"IBM", 155L, 75d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 100L, 75d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 75d},  new object[] {"YAH", 10000L, 3d},  new object[] {"IBM", 155L, 75d},  new object[] {"YAH", 11000L, 3d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 100L, 97d},  new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 97d},  new object[] {"YAH", 10000L, 6d},  new object[] {"IBM", 155L, 97d},  new object[] {"YAH", 11000L, 6d},  new object[] {"IBM", 150L, 97d},  new object[] {"YAH", 11500L, 6d}});
	        expected.AddResultInsert(6200, 0, new object[][] { new object[] {"MSFT", 5000L, 9d},  new object[] {"IBM", 150L, 72d},  new object[] {"YAH", 10000L, 7d},  new object[] {"IBM", 155L, 72d},  new object[] {"YAH", 11000L, 7d},  new object[] {"IBM", 150L, 72d},  new object[] {"YAH", 11500L, 7d},  new object[] {"YAH", 10500L, 7d}});
	        expected.AddResultInsert(7200, 0, new object[][] { new object[] {"IBM", 155L, 48d},  new object[] {"YAH", 11000L, 6d},  new object[] {"IBM", 150L, 48d},  new object[] {"YAH", 11500L, 6d},  new object[] {"YAH", 10500L, 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void TestHaving()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Symbol, Volume, sum(Price) as sumPrice" +
	                          " from " + typeof(SupportMarketDataBean).FullName + "#time(10 sec) " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 10 " +
	                          "output every 3 events";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        RunAssertionHavingDefault();
	    }

        [Test]
	    public void TestHavingJoin()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Symbol, Volume, sum(Price) as sumPrice" +
	                          " from " + typeof(SupportMarketDataBean).FullName + "#time(10 sec) as s0," +
	                          typeof(SupportBean).FullName + "#keepall as s1 " +
	                          "where s0.Symbol = s1.TheString " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 10 " +
	                          "output every 3 events";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 0));

	        RunAssertionHavingDefault();
	    }

        [Test]
	    public void TestJoinSortWindow()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Symbol, Volume, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + "#sort(1, Volume) as s0," +
	                          typeof(SupportBean).FullName + "#keepall as s1 where s1.TheString = s0.Symbol " +
	                          "group by Symbol output every 1 seconds";
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
	    public void TestLimitSnapshot()
	    {
	        SendTimer(0);
	        var selectStmt = "select Symbol, Volume, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
	                "#time(10 seconds) group by Symbol output snapshot every 1 seconds";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);
	        SendEvent("s0", 1, 20);

	        SendTimer(500);
	        SendEvent("IBM", 2, 16);
	        SendEvent("s0", 3, 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "Volume", "sumPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"s0", 1L, 34d},  new object[] {"IBM", 2L, 16d},  new object[] {"s0", 3L, 34d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendEvent("MSFT", 4, 18);
	        SendEvent("IBM", 5, 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{ new object[] {"s0", 1L, 34d},  new object[] {"IBM", 2L, 46d},  new object[] {"s0", 3L, 34d},  new object[] {"MSFT", 4L, 18d},  new object[] {"IBM", 5L, 46d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"MSFT", 4L, 18d},  new object[] {"IBM", 5L, 30d}});
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
	        var selectStmt = "select Symbol, Volume, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
	                "#time(10 seconds) as m, " + typeof(SupportBean).FullName +
	                "#keepall as s where s.TheString = m.Symbol group by Symbol output snapshot every 1 seconds order by Symbol, Volume asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("ABC", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("MSFT", 3));

	        SendEvent("ABC", 1, 20);

	        SendTimer(500);
	        SendEvent("IBM", 2, 16);
	        SendEvent("ABC", 3, 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "Volume", "sumPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 1L, 34d},  new object[] {"ABC", 3L, 34d},  new object[] {"IBM", 2L, 16d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendEvent("MSFT", 4, 18);
	        SendEvent("IBM", 5, 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{ new object[] {"ABC", 1L, 34d},  new object[] {"ABC", 3L, 34d},  new object[] {"IBM", 2L, 46d},  new object[] {"IBM", 5L, 46d},  new object[] {"MSFT", 4L, 18d},});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(10500);
	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"IBM", 5L, 30d},  new object[] {"MSFT", 4L, 18d}});
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
	    public void TestMaxTimeWindow()
	    {
	        SendTimer(0);

	        var viewExpr = "select irstream Symbol, " +
	                                  "Volume, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + "#time(1 sec) " +
	                          "group by Symbol output every 1 seconds";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

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
	    public void TestNoJoinLast() {
	        RunAssertionNoJoinLast(true);
	        RunAssertionNoJoinLast(false);
	    }

	    private void RunAssertionNoJoinLast(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
		    var viewExpr = hint +
	                          "select Symbol, Volume, sum(Price) as mySum " +
		                      "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
		                      "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
		                      "group by Symbol " +
		                      "output last every 2 events";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);

		    RunAssertionLast();

	        selectTestView.Dispose();
	        _listener.Reset();
		}

	    private void AssertEvent(string symbol, double? mySum, long? volume)
		{
		    var newData = _listener.LastNewData;

		    Assert.AreEqual(1, newData.Length);

		    Assert.AreEqual(symbol, newData[0].Get("Symbol"));
		    Assert.AreEqual(mySum, newData[0].Get("mySum"));
		    Assert.AreEqual(volume, newData[0].Get("Volume"));

		    _listener.Reset();
		    Assert.IsFalse(_listener.IsInvoked);
		}

		private void RunAssertionSingle(EPStatement selectTestView)
		{
		    // assert select result type
		    Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
		    Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
		    Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));

		    SendEvent(SYMBOL_DELL, 10, 100);
		    Assert.IsTrue(_listener.IsInvoked);
		    AssertEvent(SYMBOL_DELL, 100d, 10L);

		    SendEvent(SYMBOL_IBM, 15, 50);
		    AssertEvent(SYMBOL_IBM, 50d, 15L);
		}

        [Test]
		public void TestNoOutputClauseView()
		{
		    var viewExpr = "select Symbol, Volume, sum(Price) as mySum " +
		                      "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
		                      "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
		                      "group by Symbol ";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);

		    RunAssertionSingle(selectTestView);
		}

        [Test]
		public void TestNoJoinDefault()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        var viewExpr = "select Symbol, Volume, sum(Price) as mySum " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol " +
	                          "output every 2 events";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertionDefault(selectTestView);
	    }

        [Test]
	    public void TestJoinDefault()
		{
		    // Every event generates a new row, this time we sum the Price by Symbol and output Volume
		    var viewExpr = "select Symbol, Volume, sum(Price) as mySum " +
		                      "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
		                                typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
		                      "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
		                      "  and one.TheString = two.Symbol " +
		                      "group by Symbol " +
		                      "output every 2 events";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);

		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

		    RunAssertionDefault(selectTestView);
		}

        [Test]
	    public void TestNoJoinAll() {
	        RunAssertionNoJoinAll(false);
	        RunAssertionNoJoinAll(true);
	    }

	    private void RunAssertionNoJoinAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        var viewExpr = hint + "select Symbol, Volume, sum(Price) as mySum " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol " +
	                          "output all every 2 events";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertionAll(selectTestView);

	        selectTestView.Dispose();
	        _listener.Reset();
	    }

        [Test]
	    public void TestJoinAll()
	    {
	        RunAssertionJoinAll(false);
	        RunAssertionJoinAll(true);
	    }

	    private void RunAssertionJoinAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        var viewExpr = hint + "select Symbol, Volume, sum(Price) as mySum " +
	                          "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
	                                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	                          "  and one.TheString = two.Symbol " +
	                          "group by Symbol " +
	                          "output all every 2 events";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

	        RunAssertionAll(selectTestView);

	        selectTestView.Dispose();
	        _listener.Reset();
	    }

        [Test]
	    public void TestJoinLast() {
	        RunAssertionJoinLast(true);
	        RunAssertionJoinLast(false);
	    }

		private void RunAssertionJoinLast(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
		    var viewExpr = hint +
	                          "select Symbol, Volume, sum(Price) as mySum " +
		                      "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
		                                typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
		                      "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
		                      "  and one.TheString = two.Symbol " +
		                      "group by Symbol " +
		                      "output last every 2 events";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);

		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

		    RunAssertionLast();

	        _listener.Reset();
	        selectTestView.Dispose();
		}

	    private void RunAssertionHavingDefault()
	    {
	        SendEvent("IBM", 1, 5);
	        SendEvent("IBM", 2, 6);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("IBM", 3, -3);
	        var fields = "Symbol,Volume,sumPrice".Split(',');
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"IBM", 2L, 11.0});

	        SendTimer(5000);
	        SendEvent("IBM", 4, 10);
	        SendEvent("IBM", 5, 0);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("IBM", 6, 1);
	        Assert.AreEqual(3, _listener.LastNewData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[] {"IBM", 4L, 18.0});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields, new object[] {"IBM", 5L, 18.0});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[2], fields, new object[] {"IBM", 6L, 19.0});
	        _listener.Reset();

	        SendTimer(11000);
	        Assert.AreEqual(3, _listener.LastOldData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[] {"IBM", 1L, 11.0});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new object[] {"IBM", 2L, 11.0});
	        _listener.Reset();
	    }

	    private void RunAssertionDefault(EPStatement selectTestView)
	    {
	    	// assert select result type
	    	Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	    	Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
	    	Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));

	    	SendEvent(SYMBOL_IBM, 500, 20);
	    	Assert.IsFalse(_listener.GetAndClearIsInvoked());

	    	SendEvent(SYMBOL_DELL, 10000, 51);
	        var fields = "Symbol,Volume,mySum".Split(',');
	        UniformPair<EventBean[]> events = _listener.GetDataListsFlattened();
	        if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM))
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_IBM, 500L, 20.0 }, new object[] { SYMBOL_DELL, 10000L, 51.0 } });
	        }
	        else
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_DELL, 10000L, 51.0 }, new object[] { SYMBOL_IBM, 500L, 20.0 } });
	        }
	        Assert.IsNull(_listener.LastOldData);

	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 20000, 52);
	    	Assert.IsFalse(_listener.GetAndClearIsInvoked());

	    	SendEvent(SYMBOL_DELL, 40000, 45);
	        events = _listener.GetDataListsFlattened();
	        EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                    new object[][] { new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 }, new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 } });
	        Assert.IsNull(_listener.LastOldData);
	    }

	    private void RunAssertionAll(EPStatement selectTestView)
	    {
	    	// assert select result type
	    	Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	    	Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
	    	Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));

	    	SendEvent(SYMBOL_IBM, 500, 20);
	    	Assert.IsFalse(_listener.GetAndClearIsInvoked());

	    	SendEvent(SYMBOL_DELL, 10000, 51);
	        var fields = "Symbol,Volume,mySum".Split(',');
	        UniformPair<EventBean[]> events = _listener.GetDataListsFlattened();
	        if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM))
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_IBM, 500L, 20.0 }, new object[] { SYMBOL_DELL, 10000L, 51.0 } });
	        }
	        else
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_DELL, 10000L, 51.0 }, new object[] { SYMBOL_IBM, 500L, 20.0 } });
	        }
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 20000, 52);
	    	Assert.IsFalse(_listener.GetAndClearIsInvoked());

	    	SendEvent(SYMBOL_DELL, 40000, 45);
	        events = _listener.GetDataListsFlattened();
	        if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM))
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_IBM, 500L, 20.0 }, new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 }, new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 } });
	        }
	        else
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 }, new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 }, new object[] { SYMBOL_IBM, 500L, 20.0 } });
	        }
	        Assert.IsNull(_listener.LastOldData);
	    }

		private void RunAssertionLast()
	    {
	        var fields = "Symbol,Volume,mySum".Split(',');
	        SendEvent(SYMBOL_DELL, 10000, 51);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendEvent(SYMBOL_DELL, 20000, 52);
	        UniformPair<EventBean[]> events = _listener.GetDataListsFlattened();
	        EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                    new object[][] { new object[] { SYMBOL_DELL, 20000L, 103.0 } });
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 30000, 70);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendEvent(SYMBOL_IBM, 10000, 20);
	        events = _listener.GetDataListsFlattened();
	        if (events.First[0].Get("Symbol").Equals(SYMBOL_DELL))
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_DELL, 30000L, 173.0 }, new object[] { SYMBOL_IBM, 10000L, 20.0 } });
	        }
	        else
	        {
	            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][] { new object[] { SYMBOL_IBM, 10000L, 20.0 }, new object[] { SYMBOL_DELL, 30000L, 173.0 } });
	        }
	        Assert.IsNull(_listener.LastOldData);
	    }

	    private void SendEvent(string symbol, long volume, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }

	    private void SendBeanEvent(string theString, long longPrimitive, int intPrimitive)
		{
	        var b = new SupportBean();
	        b.TheString = theString;
	        b.LongPrimitive = longPrimitive;
	        b.IntPrimitive = intPrimitive;
		    _epService.EPRuntime.SendEvent(b);
		}

	    private void SendMDEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
