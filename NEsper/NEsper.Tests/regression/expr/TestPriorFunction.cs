///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestPriorFunction 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestPriorTimewindowStats() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        var epl = "SELECT prior(1, average) as value FROM SupportBean().win:time(5 minutes).stat:uni(IntPrimitive)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
	        Assert.AreEqual(1.0, _listener.AssertOneGetNewAndReset().Get("value"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
	        Assert.AreEqual(2.5, _listener.AssertOneGetNewAndReset().Get("value"));
	    }

        [Test]
	    public void TestPriorStreamAndVariable()
	    {
	        RunAssertionPriorStreamAndVariable("1");

	        // try variable
	        _epService.EPAdministrator.CreateEPL("create constant variable int NUM_PRIOR = 1");
	        RunAssertionPriorStreamAndVariable("NUM_PRIOR");

	        // must be a constant-value expression
	        _epService.EPAdministrator.CreateEPL("create variable int NUM_PRIOR_NONCONST = 1");
	        try {
	            RunAssertionPriorStreamAndVariable("NUM_PRIOR_NONCONST");
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'prior(NUM_PRIOR_NONCONST,s0)': Prior function requires a constant-value integer-typed index expression as the first parameter");
	        }
	    }

	    private void RunAssertionPriorStreamAndVariable(string priorIndex) {
	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	        var text = "select prior(" + priorIndex + ", s0) as result from S0.win:length(2) as s0";
	        var stmt = _epService.EPAdministrator.CreateEPL(text);
	        stmt.AddListener(_listener);

	        var e1 = new SupportBean_S0(3);
	        _epService.EPRuntime.SendEvent(e1);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("result"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
	        Assert.AreEqual(e1, _listener.AssertOneGetNewAndReset().Get("result"));
	        Assert.AreEqual(typeof(SupportBean_S0), stmt.EventType.GetPropertyType("result"));

	        stmt.Dispose();
	    }

        [Test]
	    public void TestPriorTimeWindow()
	    {
	        var viewExpr = "select irstream Symbol as currSymbol, " +
	                          " prior(2, Symbol) as priorSymbol, " +
	                          " prior(2, Price) as priorPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:time(1 min) ";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("priorPrice"));

	        SendTimer(0);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D1", 1);
	        AssertNewEvents("D1", null, null);

	        SendTimer(1000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D2", 2);
	        AssertNewEvents("D2", null, null);

	        SendTimer(2000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D3", 3);
	        AssertNewEvents("D3", "D1", 1d);

	        SendTimer(3000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D4", 4);
	        AssertNewEvents("D4", "D2", 2d);

	        SendTimer(4000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D5", 5);
	        AssertNewEvents("D5", "D3", 3d);

	        SendTimer(30000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("D6", 6);
	        AssertNewEvents("D6", "D4", 4d);

	        SendTimer(60000);
	        AssertOldEvents("D1", null, null);
	        SendTimer(61000);
	        AssertOldEvents("D2", null, null);
	        SendTimer(62000);
	        AssertOldEvents("D3", "D1", 1d);
	        SendTimer(63000);
	        AssertOldEvents("D4", "D2", 2d);
	        SendTimer(64000);
	        AssertOldEvents("D5", "D3", 3d);
	        SendTimer(90000);
	        AssertOldEvents("D6", "D4", 4d);

	        SendMarketEvent("D7", 7);
	        AssertNewEvents("D7", "D5", 5d);
	        SendMarketEvent("D8", 8);
	        SendMarketEvent("D9", 9);
	        SendMarketEvent("D10", 10);
	        SendMarketEvent("D11", 11);
	        _listener.Reset();

	        // release batch
	        SendTimer(150000);
	        var oldData = _listener.LastOldData;
	        Assert.IsNull(_listener.LastNewData);
	        Assert.AreEqual(5, oldData.Length);
	        AssertEvent(oldData[0], "D7", "D5", 5d);
	        AssertEvent(oldData[1], "D8", "D6", 6d);
	        AssertEvent(oldData[2], "D9", "D7", 7d);
	        AssertEvent(oldData[3], "D10", "D8", 8d);
	        AssertEvent(oldData[4], "D11", "D9", 9d);
	    }

        [Test]
	    public void TestPriorExtTimedWindow()
	    {
	        var viewExpr = "select irstream Symbol as currSymbol, " +
	                          " prior(2, Symbol) as priorSymbol, " +
	                          " prior(3, Price) as priorPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:ext_timed(Volume, 1 min) ";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("priorPrice"));

	        SendMarketEvent("D1", 1, 0);
	        AssertNewEvents("D1", null, null);

	        SendMarketEvent("D2", 2, 1000);
	        AssertNewEvents("D2", null, null);

	        SendMarketEvent("D3", 3, 3000);
	        AssertNewEvents("D3", "D1", null);

	        SendMarketEvent("D4", 4, 4000);
	        AssertNewEvents("D4", "D2", 1d);

	        SendMarketEvent("D5", 5, 5000);
	        AssertNewEvents("D5", "D3", 2d);

	        SendMarketEvent("D6", 6, 30000);
	        AssertNewEvents("D6", "D4", 3d);

	        SendMarketEvent("D7", 7, 60000);
	        AssertEvent(_listener.LastNewData[0], "D7", "D5", 4d);
	        AssertEvent(_listener.LastOldData[0], "D1", null, null);
	        _listener.Reset();

	        SendMarketEvent("D8", 8, 61000);
	        AssertEvent(_listener.LastNewData[0], "D8", "D6", 5d);
	        AssertEvent(_listener.LastOldData[0], "D2", null, null);
	        _listener.Reset();

	        SendMarketEvent("D9", 9, 63000);
	        AssertEvent(_listener.LastNewData[0], "D9", "D7", 6d);
	        AssertEvent(_listener.LastOldData[0], "D3", "D1", null);
	        _listener.Reset();

	        SendMarketEvent("D10", 10, 64000);
	        AssertEvent(_listener.LastNewData[0], "D10", "D8", 7d);
	        AssertEvent(_listener.LastOldData[0], "D4", "D2", 1d);
	        _listener.Reset();

	        SendMarketEvent("D10", 10, 150000);
	        var oldData = _listener.LastOldData;
	        Assert.AreEqual(6, oldData.Length);
	        AssertEvent(oldData[0], "D5", "D3", 2d);
	    }

        [Test]
	    public void TestPriorTimeBatchWindow()
	    {
	        var viewExpr = "select irstream Symbol as currSymbol, " +
	                          " prior(3, Symbol) as priorSymbol, " +
	                          " prior(2, Price) as priorPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 min) ";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("priorSymbol"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("priorPrice"));

	        SendTimer(0);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("A", 1);
	        SendMarketEvent("B", 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(60000);
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        AssertEvent(_listener.LastNewData[0], "A", null, null);
	        AssertEvent(_listener.LastNewData[1], "B", null, null);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendTimer(80000);
	        SendMarketEvent("C", 3);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(120000);
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        AssertEvent(_listener.LastNewData[0], "C", null, 1d);
	        Assert.AreEqual(2, _listener.LastOldData.Length);
	        AssertEvent(_listener.LastOldData[0], "A", null, null);
	        _listener.Reset();

	        SendTimer(300000);
	        SendMarketEvent("D", 4);
	        SendMarketEvent("E", 5);
	        SendMarketEvent("F", 6);
	        SendMarketEvent("G", 7);
	        SendTimer(360000);
	        Assert.AreEqual(4, _listener.LastNewData.Length);
	        AssertEvent(_listener.LastNewData[0], "D", "A", 2d);
	        AssertEvent(_listener.LastNewData[1], "E", "B", 3d);
	        AssertEvent(_listener.LastNewData[2], "F", "C", 4d);
	        AssertEvent(_listener.LastNewData[3], "G", "D", 5d);
	    }

        [Test]
	    public void TestPriorUnbound()
	    {
	        var viewExpr = "select Symbol as currSymbol, " +
	                          " prior(3, Symbol) as priorSymbol, " +
	                          " prior(2, Price) as priorPrice " +
	                          "from " + typeof(SupportMarketDataBean).FullName;

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("priorPrice"));

	        SendMarketEvent("A", 1);
	        AssertNewEvents("A", null, null);

	        SendMarketEvent("B", 2);
	        AssertNewEvents("B", null, null);

	        SendMarketEvent("C", 3);
	        AssertNewEvents("C", null, 1d);

	        SendMarketEvent("D", 4);
	        AssertNewEvents("D", "A", 2d);

	        SendMarketEvent("E", 5);
	        AssertNewEvents("E", "B", 3d);
	    }

        [Test]
	    public void TestPriorNoDataWindowWhere()
	    {
	        var text = "select * from " + typeof(SupportMarketDataBean).FullName +
	                      " where prior(1, Price) = 100";
	        var stmt = _epService.EPAdministrator.CreateEPL(text);
	        stmt.AddListener(_listener);

	        SendMarketEvent("IBM", 75);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("IBM", 100);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("IBM", 120);
	        Assert.IsTrue(_listener.IsInvoked);
	    }

        [Test]
	    public void TestLongRunningSingle()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // excluded from instrumentation, too much data

	        var viewExpr = "select Symbol as currSymbol, " +
	                          " prior(3, Symbol) as prior0Symbol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(3, Symbol)";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        var random = new Random();
	        // 200000 is a better number for a memory test, however for short unit tests this is 2000
	        for (var i = 0; i < 2000; i++)
	        {
	            if (i % 10000 == 0)
	            {
	                //System.out.println(i);
	            }

	            SendMarketEvent(Convert.ToString(random.Next()), 4);

	            if (i % 1000 == 0)
	            {
	                _listener.Reset();
	            }
	        }
	    }

        [Test]
	    public void TestLongRunningUnbound()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // excluded from instrumentation, too much data

	        var viewExpr = "select Symbol as currSymbol, " +
	                          " prior(3, Symbol) as prior0Symbol " +
	                          "from " + typeof(SupportMarketDataBean).FullName;

	        var selectTestView = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);
	        Assert.IsFalse(selectTestView.StatementContext.IsStatelessSelect);

	        var random = new Random();
	        // 200000 is a better number for a memory test, however for short unit tests this is 2000
	        for (var i = 0; i < 2000; i++)
	        {
	            if (i % 10000 == 0)
	            {
	                //System.out.println(i);
	            }

                SendMarketEvent(Convert.ToString(random.Next()), 4);

	            if (i % 1000 == 0)
	            {
	                _listener.Reset();
	            }
	        }
	    }

        [Test]
	    public void TestLongRunningMultiple()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // excluded from instrumentation, too much data

	        var viewExpr = "select Symbol as currSymbol, " +
	                          " prior(3, Symbol) as prior0Symbol, " +
	                          " prior(2, Symbol) as prior1Symbol, " +
	                          " prior(1, Symbol) as prior2Symbol, " +
	                          " prior(0, Symbol) as prior3Symbol, " +
	                          " prior(0, Price) as prior0Price, " +
	                          " prior(1, Price) as prior1Price, " +
	                          " prior(2, Price) as prior2Price, " +
	                          " prior(3, Price) as prior3Price " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(3, Symbol)";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        var random = new Random();
	        // 200000 is a better number for a memory test, however for short unit tests this is 2000
	        for (var i = 0; i < 2000; i++)
	        {
	            if (i % 10000 == 0)
	            {
	                //System.out.println(i);
	            }

                SendMarketEvent(Convert.ToString(random.Next()), 4);

	            if (i % 1000 == 0)
	            {
	                _listener.Reset();
	            }
	        }
	    }

        [Test]
	    public void TestPriorLengthWindow()
	    {
	        var viewExpr =   "select irstream Symbol as currSymbol, " +
	                            "prior(0, Symbol) as prior0Symbol, " +
	                            "prior(1, Symbol) as prior1Symbol, " +
	                            "prior(2, Symbol) as prior2Symbol, " +
	                            "prior(3, Symbol) as prior3Symbol, " +
	                            "prior(0, Price) as prior0Price, " +
	                            "prior(1, Price) as prior1Price, " +
	                            "prior(2, Price) as prior2Price, " +
	                            "prior(3, Price) as prior3Price " +
	                            "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) ";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prior0Symbol"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prior0Price"));

	        SendMarketEvent("A", 1);
	        AssertNewEvents("A", "A", 1d, null, null, null, null, null, null);
	        SendMarketEvent("B", 2);
	        AssertNewEvents("B", "B", 2d, "A", 1d, null, null, null, null);
	        SendMarketEvent("C", 3);
	        AssertNewEvents("C", "C", 3d, "B", 2d, "A", 1d, null, null);

	        SendMarketEvent("D", 4);
	        var newEvent = _listener.LastNewData[0];
	        var oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
	        AssertEventProps(oldEvent, "A", "A", 1d, null, null, null, null, null, null);

	        SendMarketEvent("E", 5);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
	        AssertEventProps(oldEvent, "B", "B", 2d, "A", 1d, null, null, null, null);

	        SendMarketEvent("F", 6);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "F", "F", 6d, "E", 5d, "D", 4d, "C", 3d);
	        AssertEventProps(oldEvent, "C", "C", 3d, "B", 2d, "A", 1d, null, null);

	        SendMarketEvent("G", 7);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "G", "G", 7d, "F", 6d, "E", 5d, "D", 4d);
	        AssertEventProps(oldEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);

	        SendMarketEvent("G", 8);
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(oldEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
	    }

        [Test]
	    public void TestPriorLengthWindowWhere()
	    {
	        var viewExpr =   "select prior(2, Symbol) as currSymbol " +
	                            "from " + typeof(SupportMarketDataBean).FullName + ".win:length(1) " +
	                            "where prior(2, Price) > 100";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendMarketEvent("A", 1);
	        SendMarketEvent("B", 130);
	        SendMarketEvent("C", 10);
	        Assert.IsFalse(_listener.IsInvoked);
	        SendMarketEvent("D", 5);
	        Assert.AreEqual("B", _listener.AssertOneGetNewAndReset().Get("currSymbol"));
	    }

        [Test]
	    public void TestPriorSortWindow()
	    {
	        var viewExpr = "select irstream Symbol as currSymbol, " +
	                          " prior(0, Symbol) as prior0Symbol, " +
	                          " prior(1, Symbol) as prior1Symbol, " +
	                          " prior(2, Symbol) as prior2Symbol, " +
	                          " prior(3, Symbol) as prior3Symbol, " +
	                          " prior(0, Price) as prior0Price, " +
	                          " prior(1, Price) as prior1Price, " +
	                          " prior(2, Price) as prior2Price, " +
	                          " prior(3, Price) as prior3Price " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(3, Symbol)";
	        TryPriorSortWindow(viewExpr);

	        viewExpr = "select irstream Symbol as currSymbol, " +
	                          " prior(3, Symbol) as prior3Symbol, " +
	                          " prior(1, Symbol) as prior1Symbol, " +
	                          " prior(2, Symbol) as prior2Symbol, " +
	                          " prior(0, Symbol) as prior0Symbol, " +
	                          " prior(2, Price) as prior2Price, " +
	                          " prior(1, Price) as prior1Price, " +
	                          " prior(0, Price) as prior0Price, " +
	                          " prior(3, Price) as prior3Price " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(3, Symbol)";
	        TryPriorSortWindow(viewExpr);
	    }

        [Test]
	    public void TestPriorTimeBatchWindowJoin()
	    {
	        var viewExpr = "select TheString as currSymbol, " +
	                          "prior(2, Symbol) as priorSymbol, " +
	                          "prior(1, Price) as priorPrice " +
	                          "from " + typeof(SupportBean).FullName + ".win:keepall(), " +
	                          typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 min)";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("priorPrice"));

	        SendTimer(0);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendMarketEvent("A", 1);
	        SendMarketEvent("B", 2);
	        SendBeanEvent("X1");
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(60000);
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        AssertEvent(_listener.LastNewData[0], "X1", null, null);
	        AssertEvent(_listener.LastNewData[1], "X1", null, 1d);
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendMarketEvent("C1", 11);
	        SendMarketEvent("C2", 12);
	        SendMarketEvent("C3", 13);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(120000);
	        Assert.AreEqual(3, _listener.LastNewData.Length);
	        AssertEvent(_listener.LastNewData[0], "X1", "A", 2d);
	        AssertEvent(_listener.LastNewData[1], "X1", "B", 11d);
	        AssertEvent(_listener.LastNewData[2], "X1", "C1", 12d);
	    }

	    private void TryPriorSortWindow(string viewExpr)
	    {
	        var statement = _epService.EPAdministrator.CreateEPL(viewExpr);
	        statement.AddListener(_listener);

	        SendMarketEvent("COX", 30);
	        AssertNewEvents("COX", "COX", 30d, null, null, null, null, null, null);

	        SendMarketEvent("IBM", 45);
	        AssertNewEvents("IBM", "IBM", 45d, "COX", 30d, null, null, null, null);

	        SendMarketEvent("MSFT", 33);
	        AssertNewEvents("MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);

	        SendMarketEvent("XXX", 55);
	        var newEvent = _listener.LastNewData[0];
	        var oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
	        AssertEventProps(oldEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);

	        SendMarketEvent("BOO", 20);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "BOO", "BOO", 20d, "XXX", 55d, "MSFT", 33d, "IBM", 45d);
	        AssertEventProps(oldEvent, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);

	        SendMarketEvent("DOR", 1);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
	        AssertEventProps(oldEvent, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);

	        SendMarketEvent("AAA", 2);
	        newEvent = _listener.LastNewData[0];
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(newEvent, "AAA", "AAA", 2d, "DOR", 1d, "BOO", 20d, "XXX", 55d);
	        AssertEventProps(oldEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);

	        SendMarketEvent("AAB", 2);
	        oldEvent = _listener.LastOldData[0];
	        AssertEventProps(oldEvent, "COX", "COX", 30d, null, null, null, null, null, null);
	        _listener.Reset();

	        statement.Stop();
	    }

	    private void AssertNewEvents(string currSymbol, string priorSymbol, double? priorPrice)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);

	        AssertEvent(newData[0], currSymbol, priorSymbol, priorPrice);

	        _listener.Reset();
	    }

	    private void AssertEvent(EventBean eventBean, string currSymbol, string priorSymbol, double? priorPrice)
	    {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(priorSymbol, eventBean.Get("priorSymbol"));
	        Assert.AreEqual(priorPrice, eventBean.Get("priorPrice"));
	    }

	    private void AssertNewEvents(string currSymbol, string prior0Symbol, double? prior0Price, string prior1Symbol, double? prior1Price, string prior2Symbol, double? prior2Price, string prior3Symbol, double? prior3Price)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);
	        AssertEventProps(newData[0], currSymbol, prior0Symbol, prior0Price, prior1Symbol, prior1Price, prior2Symbol, prior2Price, prior3Symbol, prior3Price);

	        _listener.Reset();
	    }

	    private void AssertEventProps(EventBean eventBean, string currSymbol, string prior0Symbol, double? prior0Price, string prior1Symbol, double? prior1Price, string prior2Symbol, double? prior2Price, string prior3Symbol, double? prior3Price)
	    {
	        Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
	        Assert.AreEqual(prior0Symbol, eventBean.Get("prior0Symbol"));
	        Assert.AreEqual(prior0Price, eventBean.Get("prior0Price"));
	        Assert.AreEqual(prior1Symbol, eventBean.Get("prior1Symbol"));
	        Assert.AreEqual(prior1Price, eventBean.Get("prior1Price"));
	        Assert.AreEqual(prior2Symbol, eventBean.Get("prior2Symbol"));
	        Assert.AreEqual(prior2Price, eventBean.Get("prior2Price"));
	        Assert.AreEqual(prior3Symbol, eventBean.Get("prior3Symbol"));
	        Assert.AreEqual(prior3Price, eventBean.Get("prior3Price"));

	        _listener.Reset();
	    }

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        _epService.EPRuntime.SendEvent(theEvent);
	    }

	    private void SendMarketEvent(string symbol, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendMarketEvent(string symbol, double price, long volume)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendBeanEvent(string theString)
	    {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void AssertOldEvents(string currSymbol, string priorSymbol, double? priorPrice)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.IsNull(newData);
	        Assert.AreEqual(1, oldData.Length);

	        Assert.AreEqual(currSymbol, oldData[0].Get("currSymbol"));
	        Assert.AreEqual(priorSymbol, oldData[0].Get("priorSymbol"));
	        Assert.AreEqual(priorPrice, oldData[0].Get("priorPrice"));

	        _listener.Reset();
	    }
	}
} // end of namespace
