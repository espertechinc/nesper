///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOrderBySimple
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private EPServiceProvider _epService;
	    private IList<double> _prices;
	    private IList<string> _symbols;
	    private SupportUpdateListener _testListener;
		private IList<long> _volumes;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _symbols = new List<string>();
	        _prices = new List<double>();
	        _volumes = new List<long>();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	        _prices = null;
	        _symbols = null;
	        _volumes = null;
	    }

        [Test]
	    public void TestOrderByMultiDelivery() {
	        // test for QWY-933597 or ESPER-409
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        // try pattern
	        var listener = new SupportUpdateListener();
	        var stmtText = "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] order by a.TheString desc";
	        var stmtOne = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmtOne.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 3));

	        var received = listener.GetNewDataListFlattened();
	        Assert.AreEqual(2, received.Length);
	        EPAssertionUtil.AssertPropsPerRow(received, "a.TheString".Split(','), new object[][]{ new object[] {"A2"},  new object[] {"A1"}});

	        // try pattern with output limit
	        var listenerThree = new SupportUpdateListener();
	        var stmtTextThree = "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] " +
	                "output every 2 events order by a.TheString desc";
	        var stmtThree = _epService.EPAdministrator.CreateEPL(stmtTextThree);
	        stmtThree.AddListener(listenerThree);

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 3));

	        var receivedThree = listenerThree.GetNewDataListFlattened();
	        Assert.AreEqual(2, receivedThree.Length);
	        EPAssertionUtil.AssertPropsPerRow(receivedThree, "a.TheString".Split(','), new object[][]{ new object[] {"A2"},  new object[] {"A1"}});

	        // try grouped time window
	        var stmtTextTwo = "select rstream TheString from SupportBean.std:groupwin(TheString).win:time(10) order by TheString desc";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
	        var listenerTwo = new SupportUpdateListener();
	        stmtTwo.AddListener(listenerTwo);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("A2", 1));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
	        var receivedTwo = listenerTwo.GetNewDataListFlattened();
	        Assert.AreEqual(2, receivedTwo.Length);
	        EPAssertionUtil.AssertPropsPerRow(receivedTwo, "TheString".Split(','), new object[][]{ new object[] {"A2"},  new object[] {"A1"}});
	    }

#if false
        [Test]
	    public void TestCollatorSortLocale()
	    {
	        var frenchForSin = "p\u00E9ch\u00E9";
	        var frenchForFruit = "p\u00EAche";

	        var sortedFrench = (frenchForFruit + "," + frenchForSin).Split(',');

	        Assert.AreEqual(1, frenchForFruit.CompareTo(frenchForSin));
	        Assert.AreEqual(-1, frenchForSin.CompareTo(frenchForFruit));
	        Locale.Default = Locale.FRENCH;
	        Assert.AreEqual(1, frenchForFruit.CompareTo(frenchForSin));
	        Assert.AreEqual(-1, Collator.Instance.Compare(frenchForFruit, frenchForSin));
	        Assert.AreEqual(-1, frenchForSin.CompareTo(frenchForFruit));
	        Assert.AreEqual(1, Collator.Instance.Compare(frenchForSin, frenchForFruit));
	        Assert.IsFalse(frenchForSin.Equals(frenchForFruit));

	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Language.SortUsingCollator = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean).FullName);

	        // test order by
	        var stmtText = "select TheString from SupportBean.win:keepall() order by TheString asc";
	        var stmtOne = _epService.EPAdministrator.CreateEPL(stmtText);
	        _epService.EPRuntime.SendEvent(new SupportBean(frenchForSin, 1));
	        _epService.EPRuntime.SendEvent(new SupportBean(frenchForFruit, 1));
	        EPAssertionUtil.AssertPropsPerRow(stmtOne.GetEnumerator(), "TheString".Split(','), new object[][]{{sortedFrench[0]}, {sortedFrench[1]}});

	        // test sort view
	        var listener = new SupportUpdateListener();
	        stmtText = "select irstream TheString from SupportBean.ext:sort(2, TheString asc)";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmtTwo.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportBean(frenchForSin, 1));
	        _epService.EPRuntime.SendEvent(new SupportBean(frenchForFruit, 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("abc", 1));

	        Assert.AreEqual(frenchForSin, listener.LastOldData[0].Get("TheString"));
	        Locale.Default = Locale.US;
	    }
#endif

        [Test]
	    public void TestIterator()
		{
	    	var statementString = "select Symbol, TheString, Price from " +
	    	            typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	            typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                    "where one.Symbol = two.TheString " +
	                    "order by Price";
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        SendJoinEvents();
	        SendEvent("CAT", 50);
	        SendEvent("IBM", 49);
	        SendEvent("CAT", 15);
	        SendEvent("IBM", 100);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[] {"Symbol", "TheString", "Price"},
	                new object[][]{
	                         new object[] {"CAT", "CAT", 15d},
	                         new object[] {"IBM", "IBM", 49d},
	                         new object[] {"CAT", "CAT", 50d},
	                         new object[] {"IBM", "IBM", 100d}
	                });

	        SendEvent("KGB", 75);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[] {"Symbol", "TheString", "Price"},
	                new object[][]{
	                         new object[] {"CAT", "CAT", 15d},
	                         new object[] {"IBM", "IBM", 49d},
	                         new object[] {"CAT", "CAT", 50d},
	                         new object[] {"KGB", "KGB", 75d},
	                         new object[] {"IBM", "IBM", 100d}
	                });
	    }

        [Test]
	    public void TestAcrossJoin()
		{
	    	var statementString = "select Symbol, TheString from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertValues(_symbols, "Symbol");
	    	AssertValues(_symbols, "TheString");
	       	AssertOnlyProperties(new string[] {"Symbol", "TheString"});
	        ClearValues();

	    	statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by TheString, Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesBySymbolPrice();
	    	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	    	ClearValues();
		}

        [Test]
	    public void TestDescending_OM()
		{
	        var stmtText = "select Symbol from " +
	                typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                "output every 6 events "  +
	                "order by Price desc";

	        var model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create("Symbol");
	        model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("win", "length", Expressions.Constant(5)));
	        model.OutputLimitClause = OutputLimitClause.Create(6);
	        model.OrderByClause = OrderByClause.Create().Add("Price", true);
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
	        Assert.AreEqual(stmtText, model.ToEPL());

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.Create(model);
	        statement.AddListener(_testListener);
	        SendEvent("IBM", 2);
	        SendEvent("KGB", 1);
	        SendEvent("CMU", 3);
	        SendEvent("IBM", 6);
	        SendEvent("CAT", 6);
	        SendEvent("CAT", 5);

			OrderValuesByPriceDesc();
			AssertValues(_symbols, "Symbol");
			ClearValues();
	    }

        [Test]
	    public void TestDescending()
		{
			var statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price desc";
			CreateAndSend(statementString);
			OrderValuesByPriceDesc();
			AssertValues(_symbols, "Symbol");
			ClearValues();

			statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price desc, Symbol asc";
			CreateAndSend(statementString);
			OrderValuesByPrice();
            _symbols.Reverse();
			AssertValues(_symbols, "Symbol");
			ClearValues();

			statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price asc";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
			ClearValues();

			statementString = "select Symbol, Volume from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol desc";
			CreateAndSend(statementString);
			OrderValuesBySymbol();
            _symbols.Reverse();
			AssertValues(_symbols, "Symbol");
			AssertValues(_volumes, "Volume");
			ClearValues();

			statementString = "select Symbol, Price from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol desc, Price desc";
			CreateAndSend(statementString);
			OrderValuesBySymbolPrice();
            _symbols.Reverse();
            _prices.Reverse();
			AssertValues(_symbols, "Symbol");
			AssertValues(_prices, "Price");
			ClearValues();

			statementString = "select Symbol, Price from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol, Price";
			CreateAndSend(statementString);
			OrderValuesBySymbolPrice();
			AssertValues(_symbols, "Symbol");
			AssertValues(_prices, "Price");
			ClearValues();
		}

        [Test]
	    public void TestExpressions()
		{
			var statementString = "select Symbol from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by (Price * 6) + 5";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
			AssertOnlyProperties(new string[] {"Symbol"});
		 	ClearValues();

		 	_epService.Initialize();

			statementString = "select Symbol, Price from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by (Price * 6) + 5, Price";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol", "Price"});
		 	ClearValues();

		 	_epService.Initialize();

			statementString = "select Symbol, 1+Volume*23 from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by (Price * 6) + 5, Price, Volume";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol", "1+Volume*23"});
		 	ClearValues();

		 	_epService.Initialize();

			statementString = "select Symbol from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by Volume*Price, Symbol";
		 	CreateAndSend(statementString);
		 	OrderValuesBySymbol();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol"});
		 	ClearValues();
		}

        [Test]
	    public void TestAliasesSimple()
	    {
	        var statementString = "select Symbol as mySymbol from " +
	        typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	        "output every 6 events "  +
	        "order by mySymbol";
	        CreateAndSend(statementString);
	        OrderValuesBySymbol();
	        AssertValues(_symbols, "mySymbol");
	           AssertOnlyProperties(new string[] {"mySymbol"});
	        ClearValues();

	        statementString = "select Symbol as mySymbol, Price as myPrice from " +
	        typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	        "output every 6 events "  +
	        "order by myPrice";
	        CreateAndSend(statementString);
	        OrderValuesByPrice();
	        AssertValues(_symbols, "mySymbol");
	        AssertValues(_prices, "myPrice");
	           AssertOnlyProperties(new string[] {"mySymbol", "myPrice"});
	        ClearValues();

	        statementString = "select Symbol, Price as myPrice from " +
	         typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	         "output every 6 events "  +
	         "order by (myPrice * 6) + 5, Price";
	         CreateAndSend(statementString);
	         OrderValuesByPrice();
	         AssertValues(_symbols, "Symbol");
	           AssertOnlyProperties(new string[] {"Symbol", "myPrice"});
	         ClearValues();

	        statementString = "select Symbol, 1+Volume*23 as myVol from " +
	         typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	         "output every 6 events "  +
	         "order by (Price * 6) + 5, Price, myVol";
	         CreateAndSend(statementString);
	         OrderValuesByPrice();
	         AssertValues(_symbols, "Symbol");
	           AssertOnlyProperties(new string[] {"Symbol", "myVol"});
	         ClearValues();
	    }

        [Test]
	    public void TestExpressionsJoin()
	    {
	    	var statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by (Price * 6) + 5";
	     	CreateAndSend(statementString);
	     	SendJoinEvents();
	     	OrderValuesByPriceJoin();
	     	AssertValues(_symbols, "Symbol");
	    	AssertOnlyProperties(new string[] {"Symbol"});
	     	ClearValues();

	     	_epService.Initialize();

	    	statementString = "select Symbol, Price from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by (Price * 6) + 5, Price";
	     	CreateAndSend(statementString);
	     	SendJoinEvents();
	     	OrderValuesByPriceJoin();
	     	AssertValues(_prices, "Price");
	       	AssertOnlyProperties(new string[] {"Symbol", "Price"});
	     	ClearValues();

	     	_epService.Initialize();

	    	statementString = "select Symbol, 1+Volume*23 from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by (Price * 6) + 5, Price, Volume";
	     	CreateAndSend(statementString);
	     	SendJoinEvents();
	     	OrderValuesByPriceJoin();
	     	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol", "1+Volume*23"});
	     	ClearValues();

	     	_epService.Initialize();

	    	statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by Volume*Price, Symbol";
	     	CreateAndSend(statementString);
	     	SendJoinEvents();
	     	OrderValuesBySymbol();
	     	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	     	ClearValues();
	    }

        [Test]
	    public void TestInvalid()
		{
			var statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by sum(Price)";
			try
			{
				CreateAndSend(statementString);
				Assert.Fail();
			}
			catch(EPStatementException)
			{
				// expected
			}

			statementString = "select sum(Price) from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by sum(Price + 6)";
			try
			{
				CreateAndSend(statementString);
                Assert.Fail();
			}
			catch(EPStatementException)
			{
				// expected
			}

			statementString = "select sum(Price + 6) from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by sum(Price)";
			try
			{
				CreateAndSend(statementString);
				Assert.Fail();
			}
			catch(EPStatementException)
			{
				// expected
			}
		}

        [Test]
	    public void TestInvalidJoin()
	    {
	    	var statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by sum(Price)";
	    	try
	    	{
	    		CreateAndSend(statementString);
	    		Assert.Fail();
	    	}
	    	catch(EPStatementException)
	    	{
	    		// expected
	    	}

	    	statementString = "select sum(Price) from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by sum(Price + 6)";
	    	try
	    	{
	    		CreateAndSend(statementString);
	    		Assert.Fail();
	    	}
	    	catch(EPStatementException)
	    	{
	    		// expected
	    	}

	    	statementString = "select sum(Price + 6) from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by sum(Price)";
	    	try
	    	{
	    		CreateAndSend(statementString);
	    		Assert.Fail();
	    	}
	    	catch(EPStatementException)
	    	{
	    		// expected
	    	}
	    }

        [Test]
	    public void TestMultipleKeys()
		{
			var statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
			"output every 6 events "  +
			"order by Symbol, Price";
			CreateAndSend(statementString);
			OrderValuesBySymbolPrice();
			AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol"});
			ClearValues();

			statementString = "select Symbol from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by Price, Symbol, Volume";
		 	CreateAndSend(statementString);
		 	OrderValuesByPriceSymbol();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol"});
		 	ClearValues();

			statementString = "select Symbol, Volume*2 from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by Price, Volume";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol", "Volume*2"});
		 	ClearValues();
		}

        [Test]
		public void TestAliases()
		{
			var statementString = "select Symbol as mySymbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by mySymbol";
			CreateAndSend(statementString);
			OrderValuesBySymbol();
			AssertValues(_symbols, "mySymbol");
		   	AssertOnlyProperties(new string[] {"mySymbol"});
			ClearValues();

			statementString = "select Symbol as mySymbol, Price as myPrice from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by myPrice";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "mySymbol");
			AssertValues(_prices, "myPrice");
		   	AssertOnlyProperties(new string[] {"mySymbol", "myPrice"});
			ClearValues();

			statementString = "select Symbol, Price as myPrice from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by (myPrice * 6) + 5, Price";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol", "myPrice"});
		 	ClearValues();

			statementString = "select Symbol, 1+Volume*23 as myVol from " +
		 	typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
		 	"output every 6 events "  +
		 	"order by (Price * 6) + 5, Price, myVol";
		 	CreateAndSend(statementString);
		 	OrderValuesByPrice();
		 	AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol", "myVol"});
		 	ClearValues();

			statementString = "select Symbol as mySymbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"order by Price, mySymbol";
			CreateAndSend(statementString);
			_symbols.Add("CAT");
			AssertValues(_symbols, "mySymbol");
			ClearValues();
			SendEvent("FOX", 10);
			_symbols.Add("FOX");
			AssertValues(_symbols, "mySymbol");
			ClearValues();
		}

        [Test]
	    public void TestMultipleKeysJoin()
	    {
	    	var statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Symbol, Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesBySymbolPrice();
	    	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	    	ClearValues();

	    	statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by Price, Symbol, Volume";
	     	CreateAndSend(statementString);
	    	SendJoinEvents();
	     	OrderValuesByPriceSymbol();
	     	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	     	ClearValues();

	    	statementString = "select Symbol, Volume*2 from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	     	"output every 6 events "  +
	     	"order by Price, Volume";
	     	CreateAndSend(statementString);
	    	SendJoinEvents();
	     	OrderValuesByPriceJoin();
	     	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol", "Volume*2"});
	     	ClearValues();
	    }

        [Test]
	    public void TestSimple()
		{
			var statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol"});
			ClearValues();

			statementString = "select Symbol, Price from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
			AssertValues(_prices, "Price");
		   	AssertOnlyProperties(new string[] {"Symbol", "Price"});
			ClearValues();

			statementString = "select Symbol, Volume from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
			AssertValues(_volumes, "Volume");
		   	AssertOnlyProperties(new string[] {"Symbol", "Volume"});
			ClearValues();

			statementString = "select Symbol, Volume*2 from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
			AssertValues(_volumes, "Volume*2");
		   	AssertOnlyProperties(new string[] {"Symbol", "Volume*2"});
			ClearValues();

			statementString = "select Symbol, Volume from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol";
			CreateAndSend(statementString);
			OrderValuesBySymbol();
			AssertValues(_symbols, "Symbol");
			AssertValues(_volumes, "Volume");
		   	AssertOnlyProperties(new string[] {"Symbol", "Volume"});
			ClearValues();

			statementString = "select Price from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol";
			CreateAndSend(statementString);
			OrderValuesBySymbol();
			AssertValues(_prices, "Price");
		   	AssertOnlyProperties(new string[] {"Price"});
			ClearValues();
		}

        [Test]
	    public void TestSimpleJoin()
	    {
	    	var statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	    	ClearValues();

	    	statementString = "select Symbol, Price from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertValues(_symbols, "Symbol");
	    	AssertValues(_prices, "Price");
	       	AssertOnlyProperties(new string[] {"Symbol", "Price"});
	    	ClearValues();

	    	statementString = "select Symbol, Volume from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertValues(_symbols, "Symbol");
	    	AssertValues(_volumes, "Volume");
	       	AssertOnlyProperties(new string[] {"Symbol", "Volume"});
	    	ClearValues();

	    	statementString = "select Symbol, Volume*2 from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertValues(_symbols, "Symbol");
	    	AssertValues(_volumes, "Volume*2");
	       	AssertOnlyProperties(new string[] {"Symbol", "Volume*2"});
	    	ClearValues();

	    	statementString = "select Symbol, Volume from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Symbol";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesBySymbol();
	    	AssertValues(_symbols, "Symbol");
	    	AssertValues(_volumes, "Volume");
	       	AssertOnlyProperties(new string[] {"Symbol", "Volume"});
	    	ClearValues();

	    	statementString = "select Price from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Symbol, Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesBySymbolJoin();
	    	AssertValues(_prices, "Price");
	       	AssertOnlyProperties(new string[] {"Price"});
	    	ClearValues();
	    }

        [Test]
	    public void TestWildcard()
		{
			var statementString = "select * from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			AssertValues(_symbols, "Symbol");
			AssertValues(_prices, "Price");
			AssertValues(_volumes, "Volume");
		   	AssertOnlyProperties(new string[] {"Symbol", "Id", "Volume", "Price", "Feed"});
			ClearValues();

			statementString = "select * from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"output every 6 events "  +
			"order by Symbol";
			CreateAndSend(statementString);
			OrderValuesBySymbol();
			AssertValues(_symbols, "Symbol");
			AssertValues(_prices, "Price");
			AssertValues(_volumes, "Volume");
            AssertOnlyProperties(new string[] { "Symbol", "Volume", "Price", "Feed", "Id" });
			ClearValues();
		}

        [Test]
	    public void TestWildcardJoin()
	    {
	    	var statementString = "select * from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events " +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
	    	AssertSymbolsJoinWildCard();
	    	ClearValues();

	    	_epService.Initialize();

	    	statementString = "select * from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"output every 6 events "  +
	    	"order by Symbol, Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesBySymbolJoin();
	    	AssertSymbolsJoinWildCard();
	    	ClearValues();
	    }

        [Test]
	    public void TestNoOutputClauseView()
	    {
			var statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
			"order by Price";
			CreateAndSend(statementString);
			_symbols.Add("CAT");
			AssertValues(_symbols, "Symbol");
			ClearValues();
			SendEvent("FOX", 10);
			_symbols.Add("FOX");
			AssertValues(_symbols, "Symbol");
			ClearValues();

			_epService.Initialize();

			// Set start time
			SendTimeEvent(0);

			statementString = "select Symbol from " +
			typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 sec) " +
			"order by Price";
			CreateAndSend(statementString);
			OrderValuesByPrice();
			SendTimeEvent(1000);
			AssertValues(_symbols, "Symbol");
		   	AssertOnlyProperties(new string[] {"Symbol"});
			ClearValues();
	    }

        [Test]
	    public void TestNoOutputClauseJoin()
	    {
	    	var statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"order by Price";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
			_symbols.Add("KGB");
			AssertValues(_symbols, "Symbol");
			ClearValues();
			SendEvent("DOG", 10);
			_symbols.Add("DOG");
			AssertValues(_symbols, "Symbol");
			ClearValues();

			_epService.Initialize();

			// Set start time
			SendTimeEvent(0);

	    	statementString = "select Symbol from " +
	    	typeof(SupportMarketDataBean).FullName + ".win:time_batch(1) as one, " +
	    	typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	    	"where one.Symbol = two.TheString " +
	    	"order by Price, Symbol";
	    	CreateAndSend(statementString);
	    	SendJoinEvents();
	    	OrderValuesByPriceJoin();
			SendTimeEvent(1000);
	    	AssertValues(_symbols, "Symbol");
	       	AssertOnlyProperties(new string[] {"Symbol"});
	    	ClearValues();
	    }

		private void AssertOnlyProperties(IList<string> requiredProperties)
	    {
	    	var events = _testListener.LastNewData;
	    	if(events == null || events.Length == 0)
	    	{
	    		return;
	    	}
	    	var type = events[0].EventType;
	    	IList<string> actualProperties = new List<string>(type.PropertyNames);
	    	Log.Debug(".assertOnlyProperties actualProperties=="+actualProperties);
	    	Assert.IsTrue(actualProperties.ContainsAll(requiredProperties));
	    	actualProperties.RemoveAll(requiredProperties);
	    	Assert.IsTrue(actualProperties.IsEmpty());
	    }

		private void AssertSymbolsJoinWildCard()
	    {
	    	var events = _testListener.LastNewData;
	    	Log.Debug(".assertValues event type = " + events[0].EventType);
	    	Log.Debug(".assertValues values: " + _symbols);
	    	Log.Debug(".assertValues events.length==" + events.Length);
	    	for(var i = 0; i < events.Length; i++)
	    	{
	    		var theEvent = (SupportMarketDataBean)events[i].Get("one");
	    		Assert.AreEqual(_symbols[i], theEvent.Symbol);
	    	}
	    }

	    private void AssertValues<T>(IList<T> values, string valueName)
	    {
	    	var events = _testListener.LastNewData;
	    	Assert.AreEqual(values.Count, events.Length);
	    	Log.Debug(".assertValues values: " + values);
	    	for(var i = 0; i < events.Length; i++)
	    	{
	    		Log.Debug(".assertValues events["+i+"]=="+events[i].Get(valueName));
	    		Assert.AreEqual(values[i], events[i].Get(valueName));
	    	}
	    }

		private void ClearValues()
	    {
	    	_prices.Clear();
	    	_volumes.Clear();
	    	_symbols.Clear();
	    }

		private void CreateAndSend(string statementString)
        {
			_testListener = new SupportUpdateListener();
			var statement = _epService.EPAdministrator.CreateEPL(statementString);
	    	statement.AddListener(_testListener);
	    	SendEvent("IBM", 2);
	    	SendEvent("KGB", 1);
	    	SendEvent("CMU", 3);
	    	SendEvent("IBM", 6);
	    	SendEvent("CAT", 6);
	    	SendEvent("CAT", 5);
		}

		private void OrderValuesByPrice()
	    {
	    	_symbols.Insert(0, "KGB");
	    	_symbols.Insert(1, "IBM");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "CAT");
	    	_symbols.Insert(4, "IBM");
	    	_symbols.Insert(5, "CAT");
	    	_prices.Insert(0, 1d);
	    	_prices.Insert(1, 2d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 5d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 6d);
	    	_volumes.Insert(0, 0L);
	    	_volumes.Insert(1, 0L);
	    	_volumes.Insert(2, 0L);
	    	_volumes.Insert(3, 0L);
	    	_volumes.Insert(4, 0L);
	    	_volumes.Insert(5, 0L);
	    }

	    private void OrderValuesByPriceDesc()
	    {
	    	_symbols.Insert(0, "IBM");
	    	_symbols.Insert(1, "CAT");
	    	_symbols.Insert(2, "CAT");
	    	_symbols.Insert(3, "CMU");
	    	_symbols.Insert(4, "IBM");
	    	_symbols.Insert(5, "KGB");
	    	_prices.Insert(0, 6d);
	    	_prices.Insert(1, 6d);
	    	_prices.Insert(2, 5d);
	    	_prices.Insert(3, 3d);
	    	_prices.Insert(4, 2d);
	    	_prices.Insert(5, 1d);
	    	_volumes.Insert(0, 0L);
            _volumes.Insert(1, 0L);
            _volumes.Insert(2, 0L);
            _volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

		private void OrderValuesByPriceJoin()
	    {
	    	_symbols.Insert(0, "KGB");
	    	_symbols.Insert(1, "IBM");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "CAT");
	    	_symbols.Insert(4, "CAT");
	    	_symbols.Insert(5, "IBM");
	    	_prices.Insert(0, 1d);
	    	_prices.Insert(1, 2d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 5d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 6d);
            _volumes.Insert(0, 0L);
	    	_volumes.Insert(1, 0L);
	    	_volumes.Insert(2, 0L);
	    	_volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

	    private void OrderValuesByPriceSymbol()
	    {
	    	_symbols.Insert(0, "KGB");
	    	_symbols.Insert(1, "IBM");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "CAT");
	    	_symbols.Insert(4, "CAT");
	    	_symbols.Insert(5, "IBM");
	    	_prices.Insert(0, 1d);
	    	_prices.Insert(1, 2d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 5d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 6d);
            _volumes.Insert(0, 0L);
            _volumes.Insert(1, 0L);
            _volumes.Insert(2, 0L);
            _volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

		private void OrderValuesBySymbol()
	    {
	    	_symbols.Insert(0, "CAT");
	    	_symbols.Insert(1, "CAT");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "IBM");
	    	_symbols.Insert(4, "IBM");
	    	_symbols.Insert(5, "KGB");
	    	_prices.Insert(0, 6d);
	    	_prices.Insert(1, 5d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 2d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 1d);
            _volumes.Insert(0, 0L);
            _volumes.Insert(1, 0L);
            _volumes.Insert(2, 0L);
            _volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

	    private void OrderValuesBySymbolJoin()
	    {
	    	_symbols.Insert(0, "CAT");
	    	_symbols.Insert(1, "CAT");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "IBM");
	    	_symbols.Insert(4, "IBM");
	    	_symbols.Insert(5, "KGB");
	    	_prices.Insert(0, 5d);
	    	_prices.Insert(1, 6d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 2d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 1d);
            _volumes.Insert(0, 0L);
            _volumes.Insert(1, 0L);
            _volumes.Insert(2, 0L);
            _volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

		private void OrderValuesBySymbolPrice()
	    {
	    	_symbols.Insert(0, "CAT");
	    	_symbols.Insert(1, "CAT");
	    	_symbols.Insert(2, "CMU");
	    	_symbols.Insert(3, "IBM");
	    	_symbols.Insert(4, "IBM");
	    	_symbols.Insert(5, "KGB");
	    	_prices.Insert(0, 5d);
	    	_prices.Insert(1, 6d);
	    	_prices.Insert(2, 3d);
	    	_prices.Insert(3, 2d);
	    	_prices.Insert(4, 6d);
	    	_prices.Insert(5, 1d);
            _volumes.Insert(0, 0L);
            _volumes.Insert(1, 0L);
            _volumes.Insert(2, 0L);
            _volumes.Insert(3, 0L);
            _volumes.Insert(4, 0L);
            _volumes.Insert(5, 0L);
	    }

		private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

		private void SendTimeEvent(int millis)
		{
	        var theEvent = new CurrentTimeEvent(millis);
	        _epService.EPRuntime.SendEvent(theEvent);
		}

		private void SendJoinEvents()
		{
			_epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
		}

	}
} // end of namespace
