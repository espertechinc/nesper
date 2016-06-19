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
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitEventPerGroup 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private const string CATEGORY = "Fully-Aggregated and Grouped";

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("MarketData", typeof(SupportMarketDataBean));
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestLastNoDataWindow() {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var epl = "select TheString, IntPrimitive as intp from SupportBean group by TheString output last every 1 seconds order by TheString asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), new string[] {"TheString", "intp"}, new object[][]{ new object[] {"E1", 3},  new object[] {"E2", 21},  new object[] {"E3", 31}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 33));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), new string[] {"TheString", "intp"}, new object[][]{ new object[] {"E1", 5},  new object[] {"E3", 33}});
	    }

        [Test]
	    public void TestOutputFirstHavingJoinNoJoin()
        {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();

	        var stmtText = "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
	        TryOutputFirstHaving(stmtText);

	        var stmtTextJoin = "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A.win:keepall() a where a.id = mv.TheString " +
	                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
	        TryOutputFirstHaving(stmtTextJoin);

	        var stmtTextOrder = "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
	        TryOutputFirstHaving(stmtTextOrder);

	        var stmtTextOrderJoin = "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A.win:keepall() a where a.id = mv.TheString " +
	                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
	        TryOutputFirstHaving(stmtTextOrderJoin);
	    }

	    private void TryOutputFirstHaving(string statementText) {
	        var fields = "TheString,value".Split(',');
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.Price");
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

	        SendBeanEvent("E1", 10);
	        SendBeanEvent("E2", 15);
	        SendBeanEvent("E1", 10);
	        SendBeanEvent("E2", 5);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 5);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 25});

	        SendBeanEvent("E2", -6);    // to 19, does not count toward condition
	        SendBeanEvent("E2", 2);    // to 21, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 22});

	        SendBeanEvent("E2", 1);    // to 23, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 1);     // to 24
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 24});

	        SendBeanEvent("E2", -10);    // to 14
	        SendBeanEvent("E2", 10);    // to 24, counts toward condition
	        Assert.IsFalse(_listener.IsInvoked);
	        SendBeanEvent("E2", 0);    // to 24, counts toward condition
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 24});

	        SendBeanEvent("E2", -10);    // to 14
	        SendBeanEvent("E2", 1);     // to 15
	        SendBeanEvent("E2", 5);     // to 20
	        SendBeanEvent("E2", 0);     // to 20
	        SendBeanEvent("E2", 1);     // to 21    // counts
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 0);    // to 21
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 21});

	        // remove events
	        SendMDEvent("E2", 0);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 21});

	        // remove events
	        SendMDEvent("E2", -10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 41});

	        // remove events
	        SendMDEvent("E2", -6);  // since there is 3*-10 we output the next one
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 47});

	        SendMDEvent("E2", 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestOutputFirstCrontab() {
	        SendTimer(0);
	        var fields = "TheString,value".Split(',');
	        _epService.EPAdministrator.Configuration.AddVariable("varout", typeof(bool), false);
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.Price");
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first at (*/2, *, *, *, *)");
	        stmt.AddListener(_listener);

	        SendBeanEvent("E1", 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 10});

	        SendTimer(2 * 60 * 1000 - 1);
	        SendBeanEvent("E1", 11);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(2 * 60 * 1000);
	        SendBeanEvent("E1", 12);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 33});

	        SendBeanEvent("E2", 20);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});

	        SendBeanEvent("E2", 21);
	        SendTimer(4 * 60 * 1000 - 1);
	        SendBeanEvent("E2", 22);
	        SendBeanEvent("E1", 13);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(4 * 60 * 1000);
	        SendBeanEvent("E2", 23);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 86});
	        SendBeanEvent("E1", 14);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 60});
	    }

        [Test]
	    public void TestOutputFirstWhenThen() {
	        var fields = "TheString,value".Split(',');
	        _epService.EPAdministrator.Configuration.AddVariable("varout", typeof(bool), false);
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.Price");
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first when varout then set varout = false");
	        stmt.AddListener(_listener);

	        SendBeanEvent("E1", 10);
	        SendBeanEvent("E1", 11);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SetVariableValue("varout", true);
	        SendBeanEvent("E1", 12);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 33});
	        Assert.AreEqual(false, _epService.EPRuntime.GetVariableValue("varout"));

	        _epService.EPRuntime.SetVariableValue("varout", true);
	        SendBeanEvent("E2", 20);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});
	        Assert.AreEqual(false, _epService.EPRuntime.GetVariableValue("varout"));

	        SendBeanEvent("E1", 13);
	        SendBeanEvent("E2", 21);
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestOutputFirstEveryNEvents() {
	        var fields = "TheString,value".Split(',');
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.Price");
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every 3 events");
	        stmt.AddListener(_listener);

	        SendBeanEvent("E1", 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 10});

	        SendBeanEvent("E1", 12);
	        SendBeanEvent("E1", 11);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E1", 13);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 46});

	        SendMDEvent("S1", 12);
	        SendMDEvent("S1", 11);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMDEvent("S1", 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 13});

	        SendBeanEvent("E1", 14);
	        SendBeanEvent("E1", 15);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 20);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});

	        // test variable
	        _epService.EPAdministrator.CreateEPL("create variable int myvar = 1");
	        stmt.Dispose();
	        stmt = _epService.EPAdministrator.CreateEPL("select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every myvar events");
	        stmt.AddListener(_listener);

	        SendBeanEvent("E3", 10);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E3", 10}});

	        SendBeanEvent("E1", 5);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 47}});

	        _epService.EPRuntime.SetVariableValue("myvar", 2);

	        SendBeanEvent("E1", 6);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E1", 7);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 60}});

	        SendBeanEvent("E1", 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E1", 1);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 62}});
	    }

        [Test]
	    public void TestWildcardEventPerGroup()
        {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean group by TheString output last every 3 events order by TheString asc");
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

	        var events = listener.GetNewDataListFlattened();
	        listener.Reset();
	        Assert.AreEqual(2, events.Length);
	        Assert.AreEqual("ATT", events[0].Get("TheString"));
	        Assert.AreEqual(11, events[0].Get("IntPrimitive"));
	        Assert.AreEqual("IBM", events[1].Get("TheString"));
	        Assert.AreEqual(100, events[1].Get("IntPrimitive"));
	        stmt.Dispose();

	        // All means each event
	        stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean group by TheString output all every 3 events");
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

	        events = listener.GetNewDataListFlattened();
	        Assert.AreEqual(3, events.Length);
	        Assert.AreEqual("IBM", events[0].Get("TheString"));
	        Assert.AreEqual(10, events[0].Get("IntPrimitive"));
	        Assert.AreEqual("ATT", events[1].Get("TheString"));
	        Assert.AreEqual(11, events[1].Get("IntPrimitive"));
	        Assert.AreEqual("IBM", events[2].Get("TheString"));
	        Assert.AreEqual(100, events[2].Get("IntPrimitive"));
	    }

        [Test]
	    public void Test1NoneNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                          "from MarketData.win:time(5.5 sec)" +
	                          "group by Symbol " +
	                          "order by Symbol asc";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test2NoneNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "order by Symbol asc";
	        RunAssertion12(stmtText, "none");
	    }

        [Test]
	    public void Test3NoneHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            " having sum(Price) > 50";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test4NoneHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50";
	        RunAssertion34(stmtText, "none");
	    }

        [Test]
	    public void Test5DefaultNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output every 1 seconds order by Symbol asc";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test6DefaultNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output every 1 seconds order by Symbol asc";
	        RunAssertion56(stmtText, "default");
	    }

        [Test]
	    public void Test7DefaultHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) \n"  +
	                            "group by Symbol " +
	                            "having sum(Price) > 50" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test8DefaultHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50" +
	                            "output every 1 seconds";
	        RunAssertion78(stmtText, "default");
	    }

        [Test]
	    public void Test9AllNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output all every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion9_10(stmtText, "all");
	    }

        [Test]
	    public void Test10AllNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output all every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion9_10(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test11AllHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec) " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test12AllHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output all every 1 seconds";
	        RunAssertion11_12(stmtText, "all");
	    }

        [Test]
	    public void Test13LastNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec)" +
	                            "group by Symbol " +
	                            "output last every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test14LastNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output last every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion13_14(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec)" +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test15LastHavingNoJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec)" +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "having sum(Price) > 50 " +
	                            "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test16LastHavingJoinHinted()
	    {
	        var stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
	                "from MarketData.win:time(5.5 sec), " +
	                "SupportBean.win:keepall() where TheString=Symbol " +
	                "group by Symbol " +
	                "having sum(Price) > 50 " +
	                "output last every 1 seconds";
	        RunAssertion15_16(stmtText, "last");
	    }

        [Test]
	    public void Test17FirstNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output first every 1 seconds";
	        RunAssertion17(stmtText, "first");
	    }

        [Test]
	    public void Test17FirstNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output first every 1 seconds";
	        RunAssertion17(stmtText, "first");
	    }

        [Test]
	    public void Test18SnapshotNoHavingNoJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec) " +
	                            "group by Symbol " +
	                            "output snapshot every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion18(stmtText, "snapshot");
	    }

        [Test]
	    public void Test18SnapshotNoHavingJoin()
	    {
	        var stmtText = "select Symbol, sum(Price) " +
	                            "from MarketData.win:time(5.5 sec), " +
	                            "SupportBean.win:keepall() where TheString=Symbol " +
	                            "group by Symbol " +
	                            "output snapshot every 1 seconds " +
	                            "order by Symbol";
	        RunAssertion18(stmtText, "snapshot");
	    }

	    private void RunAssertion12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(200, 1, new object[][] { new object[] {"IBM", 25d}}, new object[][] { new object[] {"IBM", null}});
	        expected.AddResultInsRem(800, 1, new object[][] { new object[] {"MSFT", 9d}}, new object[][] { new object[] {"MSFT", null}});
	        expected.AddResultInsRem(1500, 1, new object[][] { new object[] {"IBM", 49d}}, new object[][] { new object[] {"IBM", 25d}});
	        expected.AddResultInsRem(1500, 2, new object[][] { new object[] {"YAH", 1d}}, new object[][] { new object[] {"YAH", null}});
	        expected.AddResultInsRem(2100, 1, new object[][] { new object[] {"IBM", 75d}}, new object[][] { new object[] {"IBM", 49d}});
	        expected.AddResultInsRem(3500, 1, new object[][] { new object[] {"YAH", 3d}}, new object[][] { new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(4300, 1, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(4900, 1, new object[][] { new object[] {"YAH", 6d}}, new object[][] { new object[] {"YAH", 3d}});
	        expected.AddResultInsRem(5700, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(5900, 1, new object[][] { new object[] {"YAH", 7d}}, new object[][] { new object[] {"YAH", 6d}});
	        expected.AddResultInsRem(6300, 0, new object[][] { new object[] {"MSFT", null}}, new object[][] { new object[] {"MSFT", 9d}});
	        expected.AddResultInsRem(7000, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 72d},  new object[] {"YAH", 7d}});

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
	        expected.AddResultInsRem(2100, 1, new object[][] { new object[] {"IBM", 75d}}, null);
	        expected.AddResultInsRem(4300, 1, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(5700, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(7000, 0, null, new object[][] { new object[] {"IBM", 72d}});

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
	        expected.AddResultInsRem(1200, 0, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 9d}}, new object[][] { new object[] {"IBM", null},  new object[] {"MSFT", null}});
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"YAH", 1d}}, new object[][] { new object[] {"IBM", 25d},  new object[] {"YAH", null}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, new object[][] { new object[] {"YAH", 3d}}, new object[][] { new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 75d},  new object[] {"YAH", 3d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d},  new object[] {"YAH", 7d}}, new object[][] { new object[] {"IBM", 97d},  new object[] {"YAH", 6d}});
	        expected.AddResultInsRem(7200, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"MSFT", null},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 72d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 7d}});

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
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 75d}}, null);
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] {"IBM", 72d}});

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
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 75d}}, null);
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, null, null);
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] {"IBM", 72d}});

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
	        expected.AddResultInsRem(1200, 0, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 9d}}, new object[][] { new object[] {"IBM", null},  new object[] {"MSFT", null}});
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 49d},  new object[] {"IBM", 75d},  new object[] {"YAH", 1d}}, new object[][] { new object[] {"IBM", 25d},  new object[] {"IBM", 49d},  new object[] {"YAH", null}});
	        expected.AddResultInsRem(3200, 0, null, null);
	        expected.AddResultInsRem(4200, 0, new object[][] { new object[] {"YAH", 3d}}, new object[][] { new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 75d},  new object[] {"YAH", 3d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d},  new object[] {"YAH", 7d}}, new object[][] { new object[] {"IBM", 97d},  new object[] {"YAH", 6d}});
	        expected.AddResultInsRem(7200, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"MSFT", null},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 72d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 7d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion9_10(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 9d}}, new object[][] { new object[] {"IBM", null},  new object[] {"MSFT", null}});
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}}, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 9d},  new object[] {"YAH", null}});
	        expected.AddResultInsRem(3200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}}, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(4200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 3d}}, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 3d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 7d}}, new object[][] { new object[] {"IBM", 97d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 6d}});
	        expected.AddResultInsRem(7200, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"MSFT", null},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 72d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 7d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion11_12(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(1200, 0, null, null);
	        expected.AddResultInsRem(2200, 0, new object[][] { new object[] {"IBM", 75d}}, null);
	        expected.AddResultInsRem(3200, 0, new object[][] { new object[] {"IBM", 75d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(4200, 0, new object[][] { new object[] {"IBM", 75d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(5200, 0, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(6200, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] {"IBM", 72d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion17(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsRem(200, 1, new object[][] { new object[] {"IBM", 25d}}, new object[][] { new object[] {"IBM", null}});
	        expected.AddResultInsRem(800, 1, new object[][] { new object[] {"MSFT", 9d}}, new object[][] { new object[] {"MSFT", null}});
	        expected.AddResultInsRem(1500, 1, new object[][] { new object[] {"IBM", 49d}}, new object[][] { new object[] {"IBM", 25d}});
	        expected.AddResultInsRem(1500, 2, new object[][] { new object[] {"YAH", 1d}}, new object[][] { new object[] {"YAH", null}});
	        expected.AddResultInsRem(3500, 1, new object[][] { new object[] {"YAH", 3d}}, new object[][] { new object[] {"YAH", 1d}});
	        expected.AddResultInsRem(4300, 1, new object[][] { new object[] {"IBM", 97d}}, new object[][] { new object[] {"IBM", 75d}});
	        expected.AddResultInsRem(4900, 1, new object[][] { new object[] {"YAH", 6d}}, new object[][] { new object[] {"YAH", 3d}});
	        expected.AddResultInsRem(5700, 0, new object[][] { new object[] {"IBM", 72d}}, new object[][] { new object[] {"IBM", 97d}});
	        expected.AddResultInsRem(5900, 1, new object[][] { new object[] {"YAH", 7d}}, new object[][] { new object[] {"YAH", 6d}});
	        expected.AddResultInsRem(6300, 0, new object[][] { new object[] {"MSFT", null}}, new object[][] { new object[] {"MSFT", 9d}});
	        expected.AddResultInsRem(7000, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"YAH", 6d}}, new object[][] { new object[] {"IBM", 72d},  new object[] {"YAH", 7d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

	    private void RunAssertion18(string stmtText, string outputLimit)
	    {
	        SendTimer(0);
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        var fields = new string[] {"Symbol", "sum(Price)"};
	        var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
	        expected.AddResultInsert(1200, 0, new object[][] { new object[] {"IBM", 25d},  new object[] {"MSFT", 9d}});
	        expected.AddResultInsert(2200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}});
	        expected.AddResultInsert(3200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 1d}});
	        expected.AddResultInsert(4200, 0, new object[][] { new object[] {"IBM", 75d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 3d}});
	        expected.AddResultInsert(5200, 0, new object[][] { new object[] {"IBM", 97d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 6d}});
	        expected.AddResultInsert(6200, 0, new object[][] { new object[] {"IBM", 72d},  new object[] {"MSFT", 9d},  new object[] {"YAH", 7d}});
	        expected.AddResultInsert(7200, 0, new object[][] { new object[] {"IBM", 48d},  new object[] {"YAH", 6d}});

	        var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
	        execution.Execute(false);
	    }

        [Test]
	    public void TestJoinSortWindow()
	    {
	        SendTimer(0);

	        var fields = "Symbol,maxVol".Split(',');
	        var viewExpr = "select irstream Symbol, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(1, Volume desc) as s0," +
	                          typeof(SupportBean).FullName + ".win:keepall() as s1 " +
	                          "group by Symbol output every 1 seconds";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));

	        SendMDEvent("JOIN_KEY", 1d);
	        SendMDEvent("JOIN_KEY", 2d);
	        _listener.Reset();

	        // moves all events out of the window,
	        SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
	        UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
	        Assert.AreEqual(2, result.First.Length);
	        EPAssertionUtil.AssertPropsPerRow(result.First, fields, new object[][]{ new object[] {"JOIN_KEY", 1.0},  new object[] {"JOIN_KEY", 2.0}});
	        Assert.AreEqual(2, result.Second.Length);
	        EPAssertionUtil.AssertPropsPerRow(result.Second, fields, new object[][]{ new object[] {"JOIN_KEY", null},  new object[] {"JOIN_KEY", 1.0}});
	    }

        [Test]
	    public void TestLimitSnapshot()
	    {
	        SendTimer(0);
	        var selectStmt = "select Symbol, min(Price) as minPrice from " + typeof(SupportMarketDataBean).FullName +
	                ".win:time(10 seconds) group by Symbol output snapshot every 1 seconds order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);
	        SendMDEvent("ABC", 20);

	        SendTimer(500);
	        SendMDEvent("IBM", 16);
	        SendMDEvent("ABC", 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "minPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 14d},  new object[] {"IBM", 16d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendMDEvent("IBM", 18);
	        SendMDEvent("MSFT", 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 14d},  new object[] {"IBM", 16d},  new object[] {"MSFT", 30d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"IBM", 18d},  new object[] {"MSFT", 30d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(12000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();
	    }

        [Test]
	    public void TestLimitSnapshotLimit()
	    {
	        SendTimer(0);
	        var selectStmt = "select Symbol, min(Price) as minPrice from " + typeof(SupportMarketDataBean).FullName +
	                ".win:time(10 seconds) as m, " +
	                typeof(SupportBean).FullName + ".win:keepall() as s where s.TheString = m.Symbol " +
	                "group by Symbol output snapshot every 1 seconds order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
	        stmt.AddListener(_listener);

	        foreach (var TheString in "ABC,IBM,MSFT".Split(','))
	        {
	            _epService.EPRuntime.SendEvent(new SupportBean(TheString, 1));
	        }

	        SendMDEvent("ABC", 20);

	        SendTimer(500);
	        SendMDEvent("IBM", 16);
	        SendMDEvent("ABC", 14);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendTimer(1000);
	        var fields = new string[] {"Symbol", "minPrice"};
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 14d},  new object[] {"IBM", 16d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(1500);
	        SendMDEvent("IBM", 18);
	        SendMDEvent("MSFT", 30);

	        SendTimer(10000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"ABC", 14d},  new object[] {"IBM", 16d},  new object[] {"MSFT", 30d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(10500);
	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"IBM", 18d},  new object[] {"MSFT", 30d}});
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(11500);
	        SendTimer(12000);
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.IsNull(_listener.LastNewData);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();
	    }

        [Test]
	    public void TestGroupBy_All()
	    {
	        var fields = "Symbol,sum(Price)".Split(',');
	    	var eventName = typeof(SupportMarketDataBean).FullName;
	    	var statementString = "select irstream Symbol, sum(Price) from " + eventName + ".win:length(5) group by Symbol output all every 5 events";
	    	var statement = _epService.EPAdministrator.CreateEPL(statementString);
	    	var updateListener = new SupportUpdateListener();
	    	statement.AddListener(updateListener);

	    	// send some events and check that only the most recent
	    	// ones are kept
	    	SendMDEvent("IBM", 1D);
	    	SendMDEvent("IBM", 2D);
	    	SendMDEvent("HP", 1D);
	    	SendMDEvent("IBM", 3D);
	    	SendMDEvent("MAC", 1D);

	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	var newData = updateListener.LastNewData;
	    	Assert.AreEqual(3, newData.Length);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(newData, fields, new object[][]{
	                 new object[] {"IBM", 6d},  new object[] {"HP", 1d},  new object[] {"MAC", 1d}});
	    	var oldData = updateListener.LastOldData;
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(oldData, fields, new object[][]{
	                 new object[] {"IBM", null},  new object[] {"HP", null},  new object[] {"MAC", null}});
	    }

        [Test]
	    public void TestGroupBy_Default()
	    {
	        var fields = "Symbol,sum(Price)".Split(',');
	    	var eventName = typeof(SupportMarketDataBean).FullName;
	    	var statementString = "select irstream Symbol, sum(Price) from " + eventName + ".win:length(5) group by Symbol output every 5 events";
	    	var statement = _epService.EPAdministrator.CreateEPL(statementString);
	    	var updateListener = new SupportUpdateListener();
	    	statement.AddListener(updateListener);

	    	// send some events and check that only the most recent
	    	// ones are kept
	    	SendMDEvent("IBM", 1D);
	    	SendMDEvent("IBM", 2D);
	    	SendMDEvent("HP", 1D);
	    	SendMDEvent("IBM", 3D);
	    	SendMDEvent("MAC", 1D);

	    	Assert.IsTrue(updateListener.GetAndClearIsInvoked());
	    	var newData = updateListener.LastNewData;
	        var oldData = updateListener.LastOldData;
	    	Assert.AreEqual(5, newData.Length);
	        Assert.AreEqual(5, oldData.Length);
	        EPAssertionUtil.AssertPropsPerRow(newData, fields, new object[][]{
	                 new object[] {"IBM", 1d},  new object[] {"IBM", 3d},  new object[] {"HP", 1d},  new object[] {"IBM", 6d},  new object[] {"MAC", 1d}});
	        EPAssertionUtil.AssertPropsPerRow(oldData, fields, new object[][]{
	                 new object[] {"IBM", null},  new object[] {"IBM", 1d},  new object[] {"HP", null},  new object[] {"IBM", 3d},  new object[] {"MAC", null}});
	    }

        [Test]
	    public void TestMaxTimeWindow()
	    {
	        SendTimer(0);

	        var fields = "Symbol,maxVol".Split(',');
	        var viewExpr = "select irstream Symbol, max(Price) as maxVol" +
	                          " from " + typeof(SupportMarketDataBean).FullName + ".win:time(1 sec) " +
	                          "group by Symbol output every 1 seconds";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendMDEvent("SYM1", 1d);
	        SendMDEvent("SYM1", 2d);
	        _listener.Reset();

	        // moves all events out of the window,
	        SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
	        UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
	        Assert.AreEqual(3, result.First.Length);
	        EPAssertionUtil.AssertPropsPerRow(result.First, fields, new object[][]{ new object[] {"SYM1", 1.0},  new object[] {"SYM1", 2.0},  new object[] {"SYM1", null}});
	        Assert.AreEqual(3, result.Second.Length);
	        EPAssertionUtil.AssertPropsPerRow(result.Second, fields, new object[][]{ new object[] {"SYM1", null},  new object[] {"SYM1", 1.0},  new object[] {"SYM1", 2.0}});
	    }

        [Test]
	    public void TestNoJoinLast() {
	        RunAssertionNoJoinLast(true);
	        RunAssertionNoJoinLast(false);
	    }

	    private void RunAssertionNoJoinLast(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
		    var viewExpr = hint + "select irstream Symbol," +
		                             "sum(Price) as mySum," +
		                             "avg(Price) as myAvg " +
		                      "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
		                      "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
		                      "group by Symbol " +
		                      "output last every 2 events";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);
		    RunAssertionLast(selectTestView);
            selectTestView.Dispose();
		}

        [Test]
	    public void TestNoOutputClauseView()
	    {
	    	var viewExpr = "select irstream Symbol," +
	    	"sum(Price) as mySum," +
	    	"avg(Price) as myAvg " +
	    	"from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	    	"where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	    	"group by Symbol";

	    	var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	    	selectTestView.AddListener(_listener);

	    	RunAssertionSingle(selectTestView);
	    }

        [Test]
	    public void TestNoOutputClauseJoin()
	    {
	    	var viewExpr = "select irstream Symbol," +
	    	"sum(Price) as mySum," +
	    	"avg(Price) as myAvg " +
	    	"from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
	    	"where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	    	"       and one.TheString = two.Symbol " +
	    	"group by Symbol";

	    	var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	    	selectTestView.AddListener(_listener);

	    	_epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	    	_epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
	    	_epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

	    	RunAssertionSingle(selectTestView);
	    }

        [Test]
	    public void TestNoJoinAll()
        {
	        RunAssertionNoJoinAll(false);
	        RunAssertionNoJoinAll(true);
	    }

		private void RunAssertionNoJoinAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var viewExpr = hint + "select irstream Symbol," +
	                                 "sum(Price) as mySum," +
	                                 "avg(Price) as myAvg " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol " +
	                          "output all every 2 events";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertionAll(selectTestView);

            selectTestView.Dispose();
	    }

        [Test]
	    public void TestJoinLast() {
	        RunAssertionJoinLast(true);
	        RunAssertionJoinLast(false);
	    }

	    public void RunAssertionJoinLast(bool hinted)
		{
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var viewExpr = hint + "select irstream Symbol," +
		                             "sum(Price) as mySum," +
		                             "avg(Price) as myAvg " +
		                      "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
		                                typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
		                      "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
		                      "       and one.TheString = two.Symbol " +
		                      "group by Symbol " +
		                      "output last every 2 events";

		    var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
		    selectTestView.AddListener(_listener);

		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
		    _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
		    _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

		    RunAssertionLast(selectTestView);

	        selectTestView.Dispose();
		}

        [Test]
	    public void TestJoinAll() {
	        RunAssertionJoinAll(false);
	        RunAssertionJoinAll(true);
	    }

		private void RunAssertionJoinAll(bool hinted)
	    {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
	        var viewExpr = hint + "select irstream Symbol," +
	                                 "sum(Price) as mySum," +
	                                 "avg(Price) as myAvg " +
	                          "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
	                                    typeof(SupportMarketDataBean).FullName + ".win:length(5) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	                          "       and one.TheString = two.Symbol " +
	                          "group by Symbol " +
	                          "output all every 2 events";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

	        RunAssertionAll(selectTestView);

	        selectTestView.Dispose();
	    }

	    private void RunAssertionLast(EPStatement selectTestView)
		{
		    // assert select result type
		    Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
		    Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
		    Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("myAvg"));

		    SendMDEvent(SYMBOL_DELL, 10);
		    Assert.IsFalse(_listener.IsInvoked);

		    SendMDEvent(SYMBOL_DELL, 20);
		    AssertEvent(SYMBOL_DELL,
		            null, null,
		            30d, 15d);
		    _listener.Reset();

		    SendMDEvent(SYMBOL_DELL, 100);
		    Assert.IsFalse(_listener.IsInvoked);

		    SendMDEvent(SYMBOL_DELL, 50);
		    AssertEvent(SYMBOL_DELL,
		    		30d, 15d,
		            170d, 170/3d);
		}

	    private void RunAssertionSingle(EPStatement selectTestView)
		{
		    // assert select result type
		    Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
		    Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
		    Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("myAvg"));

		    SendMDEvent(SYMBOL_DELL, 10);
		    Assert.IsTrue(_listener.IsInvoked);
		    AssertEvent(SYMBOL_DELL,
	            	null, null,
	            	10d, 10d);

		    SendMDEvent(SYMBOL_IBM, 20);
		    Assert.IsTrue(_listener.IsInvoked);
		    AssertEvent(SYMBOL_IBM,
		            	null, null,
		            	20d, 20d);
		}

		private void RunAssertionAll(EPStatement selectTestView)
	    {
	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("myAvg"));

	        SendMDEvent(SYMBOL_IBM, 70);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMDEvent(SYMBOL_DELL, 10);
	        AssertEvents(SYMBOL_IBM,
	        		null, null,
	        		70d, 70d,
	        		SYMBOL_DELL,
	                null, null,
	                10d, 10d);
		    _listener.Reset();

	        SendMDEvent(SYMBOL_DELL, 20);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMDEvent(SYMBOL_DELL, 100);
	        AssertEvents(SYMBOL_IBM,
	        		70d, 70d,
	        		70d, 70d,
	        		SYMBOL_DELL,
	                10d, 10d,
	                130d, 130d/3d);
	    }

	    private void AssertEvent(string symbol, double? oldSum, double? oldAvg, double? newSum, double? newAvg)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.AreEqual(1, oldData.Length);
	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(symbol, oldData[0].Get("Symbol"));
	        Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
	        Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

	        Assert.AreEqual(symbol, newData[0].Get("Symbol"));
	        Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

	        _listener.Reset();
	        Assert.IsFalse(_listener.IsInvoked);
	    }

	    private void AssertEvents(string symbolOne, double? oldSumOne, double? oldAvgOne, double newSumOne, double newAvgOne,
	                              string symbolTwo, double? oldSumTwo, double? oldAvgTwo, double newSumTwo, double newAvgTwo)
	    {
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetDataListsFlattened(),
	                "mySum,myAvg".Split(','),
                    new object[][] { new object[] { newSumOne, newAvgOne }, new object[] { newSumTwo, newAvgTwo } },
                    new object[][] { new object[] { oldSumOne, oldAvgOne }, new object[] { oldSumTwo, oldAvgTwo } });
	    }

	    private void SendMDEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void SendBeanEvent(string theString, int intPrimitive)
		{
		    _epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
		}

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }
	}
} // end of namespace
