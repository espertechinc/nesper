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
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestGroupByEventPerGroup 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestCriteriaByDotMethod() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        var epl = "select sb.get_TheString() as c0, sum(IntPrimitive) as c1 from SupportBean.win:length_batch(2) as sb group by sb.get_TheString()";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[] {"E1", 30});
	    }

        [Test]
	    public void TestUnboundStreamIterate() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        // with output snapshot
	        var fields = "c0,c1".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
	                "output snapshot every 3 events");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 10}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 10},  new object[] {"E2", 20}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 21},  new object[] {"E2", 20}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 21},  new object[] {"E2", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 21},  new object[] {"E2", 20},  new object[] {"E0", 30}});
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();

	        // with order-by
	        stmt = _epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
	                "output snapshot every 3 events order by TheString asc");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 21},  new object[] {"E2", 20}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 21},  new object[] {"E2", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E0", 30},  new object[] {"E1", 21},  new object[] {"E2", 20}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 40));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { "E0", 30 }, new object[] { "E1", 21 }, new object[] { "E2", 20 }, new object[] { "E3", 40 } });
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();

	        // test un-grouped case
	        stmt = _epService.EPAdministrator.CreateEPL("select null as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 3 events");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {null, 10}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {null, 30}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {null, 41}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {null, 41}});

	        stmt.Dispose();

	        // test reclaim
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        stmt = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=1,reclaim_group_freq=1') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
	                "output snapshot every 3 events");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 11));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1800));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 10},  new object[] {"E0", 11},  new object[] {"E2", 12}});
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 10},  new object[] {"E0", 11},  new object[] {"E2", 12}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2200));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 13));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E0", 11},  new object[] {"E2", 25}});
	    }

        [Test]
	    public void TestNamedWindowDelete()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_B>();
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on SupportBean_A a delete from MyWindow w where w.TheString = a.id");
	        _epService.EPAdministrator.CreateEPL("on SupportBean_B delete from MyWindow");

	        var fields = "TheString,mysum".Split(',');
	        var viewExpr = "@Hint('DISABLE_RECLAIM_GROUP') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertion(selectTestView, fields);

	        selectTestView.Dispose();
	        _epService.EPRuntime.SendEvent(new SupportBean_B("delete"));

	        viewExpr = "select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
	        selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertion(selectTestView, fields);
	    }

        [Test]
	    public void TestUnboundStreamUnlimitedKey()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}

	        // ESPER-396 Unbound stream and aggregating/grouping by unlimited key (i.e. timestamp) configurable state drop
	        SendTimer(0);

	        // After the oldest group is 60 second old, reclaim group older then  30 seconds
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        var stmtOne = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
	        stmtOne.AddListener(_listener);

	        for (var i = 0; i < 1000; i++)
	        {
	            SendTimer(1000 + i * 1000); // reduce factor if sending more events
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);
	        }

	        _listener.Reset();

	        for (var i = 0; i < 964; i++)
	        {
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
	        }

	        for (var i = 965; i < 1000; i++)
	        {
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
	        }

	        // no frequency provided
	        _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
	        _epService.EPRuntime.SendEvent(new SupportBean());

	        _epService.EPAdministrator.CreateEPL("create variable int myAge = 10");
	        _epService.EPAdministrator.CreateEPL("create variable int myFreq = 10");

	        stmtOne.Dispose();
	        stmtOne = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=myAge,reclaim_group_freq=myFreq') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
	        stmtOne.AddListener(_listener);

	        for (var i = 0; i < 1000; i++)
	        {
	            SendTimer(2000000 + 1000 + i * 1000); // reduce factor if sending more events
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);

	            if (i == 500)
	            {
	                _epService.EPRuntime.SetVariableValue("myAge", 60);
	                _epService.EPRuntime.SetVariableValue("myFreq", 90);
	            }

	            /*
	            if (i % 100000 == 0)
	            {
	                System.out.println("Sending event number " + i);
	            }
	            */
	        }

	        _listener.Reset();

	        for (var i = 0; i < 900; i++)
	        {
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
	        }

	        for (var i = 900; i < 1000; i++)
	        {
	            var theEvent = new SupportBean();
	            theEvent.LongPrimitive = i * 1000;
	            _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
	        }

	        stmtOne.Dispose();

	        // invalid tests
	        TryInvalid("@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
	                   "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
	        TryInvalid("@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
	                   "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
	        _epService.EPAdministrator.Configuration.AddVariable("MyVar", typeof(string), "");
	        TryInvalid("@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
	                   "Error starting statement: Variable type of variable 'MyVar' is not numeric [@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
	        TryInvalid("@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
	                   "Error starting statement: Hint parameter value '-30' is an invalid value, expecting a double-typed seconds value or variable name [@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
	    }

	    private void RunAssertion(EPStatement selectTestView, string[] fields)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean("A", 100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 100});

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 20});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 101));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 201});

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 21));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 41});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "A", 201 }, new object[] { "B", 41 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", null});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "B", 41 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 102));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 102});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "A", 102 }, new object[] { "B", 41 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_A("B"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"B", null});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "A", 102 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 22));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 22});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "A", 102 }, new object[] { "B", 22 } });
	    }

        [Test]
	    public void TestAggregateGroupedProps()
	    {
	        // test for ESPER-185
	        var fields = "mycount".Split(',');
	        var viewExpr = "select irstream count(price) as mycount " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "group by price";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendEvent(SYMBOL_DELL, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 11);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1L }, new object[] { 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{2L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 2L }, new object[] { 1L } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestAggregateGroupedPropsPerGroup()
	    {
	        // test for ESPER-185
	        var fields = "mycount".Split(',');
	        var viewExpr = "select irstream count(price) as mycount " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "group by Symbol, price";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendEvent(SYMBOL_DELL, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 11);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1L }, new object[] { 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{2L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 2L }, new object[] { 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 2L }, new object[] { 1L }, new object[] { 1L } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestAggregationOverGroupedProps()
	    {
	        // test for ESPER-185
	        var fields = "Symbol,price,mycount".Split(',');
	        var viewExpr = "select irstream Symbol,price,count(price) as mycount " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "group by Symbol, price order by Symbol asc";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendEvent(SYMBOL_DELL, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 11);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"DELL", 11.0, 1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"DELL", 11.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 1L }, new object[] { "DELL", 11.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 2L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 5);
	        Assert.AreEqual(1, _listener.NewDataList.Count);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"IBM", 5.0, 1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"IBM", 5.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L }, new object[] { "IBM", 5.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 5);
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"IBM", 5.0, 2L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"IBM", 5.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 2L }, new object[] { "DELL", 11.0, 1L }, new object[] { "IBM", 5.0, 2L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 5);
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields, new object[]{"IBM", 5.0, 3L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new object[]{"IBM", 5.0, 2L});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"DELL", 10.0, 1L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"DELL", 10.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 11.0, 1L }, new object[] { "DELL", 10.0, 1L }, new object[] { "IBM", 5.0, 3L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 5);
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields, new object[]{"IBM", 5.0, 4L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new object[]{"IBM", 5.0, 3L});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{"DELL", 11.0, 0L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{"DELL", 11.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10.0, 1L }, new object[] { "IBM", 5.0, 4L } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestSumOneView()
	    {
	        var viewExpr = "select irstream Symbol," +
	                                 "sum(price) as mySum," +
	                                 "avg(price) as myAvg " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        var viewExpr = "select irstream Symbol," +
	                                 "sum(price) as mySum," +
	                                 "avg(price) as myAvg " +
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

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestUniqueInBatch()
	    {
	        var stmtOne = "insert into MyStream select Symbol, price from " +
	                typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 sec)";
	        _epService.EPAdministrator.CreateEPL(stmtOne);
	        SendTimer(0);

	        var viewExpr = "select Symbol " +
	                          "from MyStream.win:time_batch(1 sec).std:unique(Symbol) " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendEvent("IBM", 100);
	        SendEvent("IBM", 101);
	        SendEvent("IBM", 102);
	        SendTimer(1000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimer(2000);
	        UniformPair<EventBean[]> received = _listener.GetDataListsFlattened();
	        Assert.AreEqual("IBM", received.First[0].Get("Symbol"));
	    }

	    private void RunAssertion(EPStatement selectTestView)
	    {
	        var fields = new string[] {"Symbol", "mySum", "myAvg"};
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("myAvg"));

	        SendEvent(SYMBOL_DELL, 10);
	        AssertEvents(SYMBOL_DELL,
	                null, null,
	                10d, 10d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 10d, 10d } });

	        SendEvent(SYMBOL_DELL, 20);
	        AssertEvents(SYMBOL_DELL,
	                10d, 10d,
	                30d, 15d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 30d, 15d } });

	        SendEvent(SYMBOL_DELL, 100);
	        AssertEvents(SYMBOL_DELL,
	                30d, 15d,
	                130d, 130d/3d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 130d, 130d / 3d } });

	        SendEvent(SYMBOL_DELL, 50);
	        AssertEvents(SYMBOL_DELL,
	                130d, 130/3d,
	                170d, 170/3d);    // 20 + 100 + 50
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 170d, 170d / 3d } });

	        SendEvent(SYMBOL_DELL, 5);
	        AssertEvents(SYMBOL_DELL,
	                170d, 170/3d,
	                155d, 155/3d);    // 100 + 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 155d, 155d / 3d } });

	        SendEvent("AAA", 1000);
	        AssertEvents(SYMBOL_DELL,
	                155d, 155d/3,
	                55d, 55d/2);    // 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { "DELL", 55d, 55d / 2d } });

	        SendEvent(SYMBOL_IBM, 70);
	        AssertEvents(SYMBOL_DELL,
	                55d, 55/2d,
	                5, 5,
	                SYMBOL_IBM,
	                null, null,
	                70, 70);    // Dell:5
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[]{"DELL", 5d, 5d}, new object[]{"IBM", 70d, 70d}});

	        SendEvent("AAA", 2000);
	        AssertEvents(SYMBOL_DELL,
	                5d, 5d,
	                null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[]{"IBM", 70d, 70d}});

	        SendEvent("AAA", 3000);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("AAA", 4000);
	        AssertEvents(SYMBOL_IBM,
	                70d, 70d,
	                null, null);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);
	    }

        private void AssertEvents(string symbol, double? oldSum, double? oldAvg, double? newSum, double? newAvg)
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

        private void AssertEvents(
            string symbolOne,
            double? oldSumOne,
            double? oldAvgOne,
            double newSumOne,
            double newAvgOne,
            string symbolTwo,
            double? oldSumTwo,
            double? oldAvgTwo,
            double newSumTwo,
            double newAvgTwo)
	    {
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetDataListsFlattened(),
	                "mySum,myAvg".Split(','),
                    new object[][] { new object[] { newSumOne, newAvgOne }, new object[] { newSumTwo, newAvgTwo } },
                    new object[][] { new object[] { oldSumOne, oldAvgOne }, new object[] { oldSumTwo, oldAvgTwo } });
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

	    private void TryInvalid(string epl, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
