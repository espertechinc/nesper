///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestIterator 
	{
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestPatternNoWindow()
	    {
	        // Test for Esper-115
	        var cepStatementString =	"@IterableUnbound select * from pattern " +
										"[every ( addressInfo = " + typeof(SupportBean).FullName + "(TheString='address') " +
										"-> txnWD = " + typeof(SupportBean).FullName + "(TheString='txn') ) ] " +
										"where addressInfo.IntBoxed = txnWD.IntBoxed";
			var epStatement = _epService.EPAdministrator.CreateEPL(cepStatementString);

	        var myEventBean1 = new SupportBean();
			myEventBean1.TheString = "address";
			myEventBean1.IntBoxed = 9001;
			_epService.EPRuntime.SendEvent(myEventBean1);
	        Assert.IsFalse(epStatement.GetEnumerator().MoveNext());

	        var myEventBean2 = new SupportBean();
	        myEventBean2.TheString = "txn";
	        myEventBean2.IntBoxed = 9001;
	        _epService.EPRuntime.SendEvent(myEventBean2);
	        Assert.IsTrue(epStatement.GetEnumerator().MoveNext());

	        var itr = epStatement.GetEnumerator();
            itr.MoveNext();
            var theEvent = itr.Current;
	        Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
	        Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
	    }

        [Test]
	    public void TestPatternWithWindow()
	    {
			var cepStatementString =	"select * from pattern " +
										"[every ( addressInfo = " + typeof(SupportBean).FullName + "(TheString='address') " +
										"-> txnWD = " + typeof(SupportBean).FullName + "(TheString='txn') ) ]#lastevent " +
										"where addressInfo.IntBoxed = txnWD.IntBoxed";
			var epStatement = _epService.EPAdministrator.CreateEPL(cepStatementString);

			var myEventBean1 = new SupportBean();
			myEventBean1.TheString = "address";
			myEventBean1.IntBoxed = 9001;
			_epService.EPRuntime.SendEvent(myEventBean1);

	        var myEventBean2 = new SupportBean();
	        myEventBean2.TheString = "txn";
	        myEventBean2.IntBoxed = 9001;
	        _epService.EPRuntime.SendEvent(myEventBean2);

	        var itr = epStatement.GetEnumerator();
            itr.MoveNext();
            EventBean theEvent = itr.Current;
	        Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
	        Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
	    }

        [Test]
	    public void TestOrderByWildcard()
	    {
	        var stmtText = "select * from " + typeof(SupportMarketDataBean).FullName + "#length(5) order by Symbol, Volume";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        object eventOne = SendEvent("SYM", 1);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventOne}, stmt.GetEnumerator());

	        object eventTwo = SendEvent("OCC", 2);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventOne}, stmt.GetEnumerator());

	        object eventThree = SendEvent("TOC", 3);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventOne, eventThree}, stmt.GetEnumerator());

	        object eventFour = SendEvent("SYM", 0);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventOne, eventThree}, stmt.GetEnumerator());

	        object eventFive = SendEvent("SYM", 10);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventOne, eventFive, eventThree}, stmt.GetEnumerator());

	        object eventSix = SendEvent("SYM", 4);
	        EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventSix, eventFive, eventThree}, stmt.GetEnumerator());
	    }

        [Test]
	    public void TestOrderByProps()
	    {
	        var fields = new string[] {"Symbol", "Volume"};
	        var stmtText = "select Symbol, Volume from " + typeof(SupportMarketDataBean).FullName + "#length(3) order by Symbol, Volume";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 1L}});

	        SendEvent("OCC", 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 2L},  new object[] {"SYM", 1L}});

	        SendEvent("SYM", 0);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 2L},  new object[] {"SYM", 0L},  new object[] {"SYM", 1L}});

	        SendEvent("OCC", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 2L},  new object[] {"OCC", 3L},  new object[] {"SYM", 0L}});
	    }

        [Test]
	    public void TestFilter()
	    {
	        var fields = new string[] {"Symbol", "vol"};
	        var stmtText = "select Symbol, Volume * 10 as vol from " + typeof(SupportMarketDataBean).FullName + "#length(5)" +
	                      " where Volume < 0";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("SYM", -1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -10L}});

	        SendEvent("SYM", -6);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -10L},  new object[] {"SYM", -60L}});

	        SendEvent("SYM", 1);
	        SendEvent("SYM", 16);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -10L},  new object[] {"SYM", -60L}});

	        SendEvent("SYM", -9);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -10L},  new object[] {"SYM", -60L},  new object[] {"SYM", -90L}});

	        SendEvent("SYM", 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -60L},  new object[] {"SYM", -90L}});

	        SendEvent("SYM", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -90L}});

	        SendEvent("SYM", 4);
	        SendEvent("SYM", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -90L}});
	        SendEvent("SYM", 6);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());
	    }

        [Test]
	    public void TestGroupByRowPerGroupOrdered()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol " +
	                          "order by Symbol";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 100L}});

	        SendEvent("OCC", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 5L},  new object[] {"SYM", 100L}});

	        SendEvent("SYM", 10);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 5L},  new object[] {"SYM", 110L}});

	        SendEvent("OCC", 6);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"OCC", 11L},  new object[] {"SYM", 110L}});

	        SendEvent("ATB", 8);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"ATB", 8L},  new object[] {"OCC", 11L},  new object[] {"SYM", 110L}});

	        SendEvent("ATB", 7);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"ATB", 15L},  new object[] {"OCC", 11L},  new object[] {"SYM", 10L}});
	    }

        [Test]
	    public void TestGroupByRowPerGroup()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 100L}});

	        SendEvent("SYM", 10);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 110L}});

	        SendEvent("TAC", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 110L},  new object[] {"TAC", 1L}});

	        SendEvent("SYM", 11);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 121L},  new object[] {"TAC", 1L}});

	        SendEvent("TAC", 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 121L},  new object[] {"TAC", 3L}});

	        SendEvent("OCC", 55);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 21L},  new object[] {"TAC", 3L},  new object[] {"OCC", 55L}});

	        SendEvent("OCC", 4);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"TAC", 3L},  new object[] {"SYM", 11L},  new object[] {"OCC", 59L}});

	        SendEvent("OCC", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 11L},  new object[] {"TAC", 2L},  new object[] {"OCC", 62L}});
	    }

        [Test]
	    public void TestGroupByRowPerGroupHaving()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol having sum(Volume) > 10";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 100L}});

	        SendEvent("SYM", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 105L}});

	        SendEvent("TAC", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 105L}});

	        SendEvent("SYM", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 108L}});

	        SendEvent("TAC", 12);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 108L},  new object[] {"TAC", 13L}});

	        SendEvent("OCC", 55);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"TAC", 13L},  new object[] {"OCC", 55L}});

	        SendEvent("OCC", 4);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"TAC", 13L},  new object[] {"OCC", 59L}});

	        SendEvent("OCC", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"TAC", 12L},  new object[] {"OCC", 62L}});
	    }

        [Test]
	    public void TestGroupByComplex()
	    {
	        var fields = new string[] {"Symbol", "msg"};
	        var stmtText = "insert into Cutoff " +
	                          "select Symbol, (Convert.ToString(count(*)) || 'x1000.0') as msg " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(Symbol)#length(1) " +
	                          "where Price - Volume >= 1000.0 group by Symbol having count(*) = 1";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", -1, -1L, null));
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 100000d, 0L, null));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", "1x1000.0"}});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 1d, 1L, null));
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());
	    }

        [Test]
	    public void TestGroupByRowPerEventOrdered()
	    {
	        var fields = new string[] {"Symbol", "Price", "sumVol"};
	        var stmtText = "select Symbol, Price, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol " +
	                          "order by Symbol";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", -1, 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -1d, 100L}});

	        SendEvent("TAC", -2, 12);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L},  new object[] {"TAC", -2d, 12L}});

	        SendEvent("TAC", -3, 13);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L}});

	        SendEvent("SYM", -4, 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 101L},  new object[] {"SYM", -4d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L}});

	        SendEvent("OCC", -5, 99);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"OCC", -5d, 99L},  new object[] {"SYM", -1d, 101L},  new object[] {"SYM", -4d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L}});

	        SendEvent("TAC", -6, 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"OCC", -5d, 99L},  new object[] {"SYM", -4d, 1L},  new object[] {"TAC", -2d, 27L},  new object[] {"TAC", -3d, 27L},  new object[] {"TAC", -6d, 27L}});
	    }

        [Test]
	    public void TestGroupByRowPerEvent()
	    {
	        var fields = new string[] {"Symbol", "Price", "sumVol"};
	        var stmtText = "select Symbol, Price, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", -1, 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -1d, 100L}});

	        SendEvent("TAC", -2, 12);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L},  new object[] {"TAC", -2d, 12L}});

	        SendEvent("TAC", -3, 13);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L}});

	        SendEvent("SYM", -4, 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L},  new object[] {"SYM", -4d, 101L}});

	        SendEvent("OCC", -5, 99);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L},  new object[] {"SYM", -4d, 101L},  new object[] {"OCC", -5d, 99L}});

	        SendEvent("TAC", -6, 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"TAC", -2d, 27L},  new object[] {"TAC", -3d, 27L},  new object[] {"SYM", -4d, 1L},  new object[] {"OCC", -5d, 99L},  new object[] {"TAC", -6d, 27L}});
	    }

        [Test]
	    public void TestGroupByRowPerEventHaving()
	    {
	        var fields = new string[] {"Symbol", "Price", "sumVol"};
	        var stmtText = "select Symbol, Price, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
	                          "group by Symbol having sum(Volume) > 20";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", -1, 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", -1d, 100L}});

	        SendEvent("TAC", -2, 12);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L}});

	        SendEvent("TAC", -3, 13);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 100L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L}});

	        SendEvent("SYM", -4, 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L},  new object[] {"SYM", -4d, 101L}});

	        SendEvent("OCC", -5, 99);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"SYM", -1d, 101L},  new object[] {"TAC", -2d, 25L},  new object[] {"TAC", -3d, 25L},  new object[] {"SYM", -4d, 101L},  new object[] {"OCC", -5d, 99L}});

	        SendEvent("TAC", -6, 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
	                new object[][]{ new object[] {"TAC", -2d, 27L},  new object[] {"TAC", -3d, 27L},  new object[] {"OCC", -5d, 99L},  new object[] {"TAC", -6d, 27L}});
	    }

        [Test]
	    public void TestAggregateAll()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 100L}});

	        SendEvent("TAC", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 101L},  new object[] {"TAC", 101L}});

	        SendEvent("MOV", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 104L},  new object[] {"TAC", 104L},  new object[] {"MOV", 104L}});

	        SendEvent("SYM", 10);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"TAC", 14L},  new object[] {"MOV", 14L},  new object[] {"SYM", 14L}});
	    }

        [Test]
	    public void TestAggregateAllOrdered()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
	                          " order by Symbol asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 100L}});

	        SendEvent("TAC", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 101L},  new object[] {"TAC", 101L}});

	        SendEvent("MOV", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"MOV", 104L},  new object[] {"SYM", 104L},  new object[] {"TAC", 104L}});

	        SendEvent("SYM", 10);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"MOV", 14L},  new object[] {"SYM", 14L},  new object[] {"TAC", 14L}});
	    }

        [Test]
	    public void TestAggregateAllHaving()
	    {
	        var fields = new string[] {"Symbol", "sumVol"};
	        var stmtText = "select Symbol, sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(3) having sum(Volume) > 100";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("SYM", 100);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent("TAC", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 101L},  new object[] {"TAC", 101L}});

	        SendEvent("MOV", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"SYM", 104L},  new object[] {"TAC", 104L},  new object[] {"MOV", 104L}});

	        SendEvent("SYM", 10);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());
	    }

        [Test]
	    public void TestRowForAll()
	    {
	        var fields = new string[] {"sumVol"};
	        var stmtText = "select sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {null}});

	        SendEvent(100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 100L } });

	        SendEvent(50);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 150L } });

	        SendEvent(25);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 175L } });

	        SendEvent(10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 85L } });
	    }

        [Test]
	    public void TestRowForAllHaving()
	    {
	        var fields = new string[] {"sumVol"};
	        var stmtText = "select sum(Volume) as sumVol " +
	                          "from " + typeof(SupportMarketDataBean).FullName + "#length(3) having sum(Volume) > 100";

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent(100);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());

	        SendEvent(50);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 150L } });

	        SendEvent(25);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { 175L } });

	        SendEvent(10);
	        Assert.IsFalse(stmt.GetEnumerator().MoveNext());
	    }

	    private void SendEvent(string symbol, double price, long volume)
	    {
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, null));
	    }

	    private SupportMarketDataBean SendEvent(string symbol, long volume)
	    {
	        var theEvent = new SupportMarketDataBean(symbol, 0, volume, null);
	        _epService.EPRuntime.SendEvent(theEvent);
	        return theEvent;
	    }

	    private void SendEvent(long volume)
	    {
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 0, volume, null));
	    }
	}
} // end of namespace
