///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOrderByEventPerRow 
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private EPServiceProvider _epService;
		private SupportUpdateListener _testListener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	    }

        [Test]
	    public void TestAliasesAggregationCompile()
	    {
	        var statementString = "select Symbol, Volume, sum(Price) as mySum from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
	                                "group by Symbol " +
	                                "output every 6 events " +
	                                "order by sum(Price), Symbol";

	        var model = _epService.EPAdministrator.CompileEPL(statementString);
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
	        Assert.AreEqual(statementString, model.ToEPL());

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.Create(model);
	        statement.AddListener(_testListener);

	        RunAssertionDefault();
	    }

        [Test]
	    public void TestAliasesAggregationOM()
	    {
	        var model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create("Symbol", "Volume").Add(Expressions.Sum("Price"), "mySum");
	        model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView(View.Create("win", "length", Expressions.Constant(20))));
	        model.GroupByClause = GroupByClause.Create("Symbol");
	        model.OutputLimitClause = OutputLimitClause.Create(6);
	        model.OrderByClause = OrderByClause.Create(Expressions.Sum("Price")).Add("Symbol", false);
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

	        var statementString = "select Symbol, Volume, sum(Price) as mySum from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
	                                "group by Symbol " +
	                                "output every 6 events " +
	                                "order by sum(Price), Symbol";

	        Assert.AreEqual(statementString, model.ToEPL());

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.Create(model);
	        statement.AddListener(_testListener);

	        RunAssertionDefault();
	    }

        [Test]
	    public void TestAliases()
		{
			var statementString = "select Symbol, Volume, sum(Price) as mySum from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
	                                "group by Symbol " +
	                                "output every 6 events " +
	                                "order by mySum, Symbol";

	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _testListener = new SupportUpdateListener();
	        statement.AddListener(_testListener);

	        RunAssertionDefault();
		}

        [Test]
	    public void TestGroupBySwitch()
		{
			// Instead of the row-per-group behavior, these should
			// get row-per-event behavior since there are properties
			// in the order-by that are not in the select expression.
			var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
	                                "group by Symbol " +
	                                "output every 6 events " +
	                                "order by sum(Price), Symbol, Volume";

	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _testListener = new SupportUpdateListener();
	        statement.AddListener(_testListener);

	        RunAssertionDefaultNoVolume();
	    }

        [Test]
	    public void TestGroupBySwitchJoin()
		{
	        var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "group by Symbol " +
	                                "output every 6 events " +
	                                "order by sum(Price), Symbol, Volume";

	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _testListener = new SupportUpdateListener();
	        statement.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

	        RunAssertionDefaultNoVolume();
		}

        [Test]
		public void TestLast()
		{
	    	var statementString = "select Symbol, Volume, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
	                                "group by Symbol " +
	                                "output last every 6 events " +
	                                "order by sum(Price)";

	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _testListener = new SupportUpdateListener();
	        statement.AddListener(_testListener);

	        RunAssertionLast();
	    }

        [Test]
	    public void TestLastJoin()
	    {
	        var statementString = "select Symbol, Volume, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "group by Symbol " +
	                                "output last every 6 events " +
	                                "order by sum(Price)";

	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        _testListener = new SupportUpdateListener();
	        statement.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

	        RunAssertionLast();
	    }

	    private void RunAssertionLast()
	    {
	        SendEvent("IBM", 101, 3);
	        SendEvent("IBM", 102, 4);
	        SendEvent("CMU", 103, 1);
	        SendEvent("CMU", 104, 2);
	        SendEvent("CAT", 105, 5);
	        SendEvent("CAT", 106, 6);

	        var fields = "Symbol,Volume,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
	                new object[][]{ new object[] {"CMU", 104L, 3.0},  new object[] {"IBM", 102L, 7.0},  new object[] {"CAT", 106L, 11.0}});
	        Assert.IsNull(_testListener.LastOldData);

	        SendEvent("IBM", 201, 3);
	        SendEvent("IBM", 202, 4);
	        SendEvent("CMU", 203, 5);
	        SendEvent("CMU", 204, 5);
	        SendEvent("DOG", 205, 0);
	        SendEvent("DOG", 206, 1);

	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
	                new object[][]{ new object[] {"DOG", 206L, 1.0},  new object[] {"CMU", 204L, 13.0},  new object[] {"IBM", 202L, 14.0}});
	        Assert.IsNull(_testListener.LastOldData);
	    }

        [Test]
	    public void TestIteratorGroupByEventPerRow()
		{
	        var fields = new string[]  {"Symbol", "TheString", "sumPrice"};
	        var statementString = "select Symbol, TheString, sum(Price) as sumPrice from " +
	    	            typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	            typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                    "where one.Symbol = two.TheString " +
	                    "group by Symbol " +
	                    "order by Symbol";
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        SendJoinEvents();
	        SendEvent("CAT", 50);
	        SendEvent("IBM", 49);
	        SendEvent("CAT", 15);
	        SendEvent("IBM", 100);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
	                new object[][]{
	                         new object[] {"CAT", "CAT", 65d},
	                         new object[] {"CAT", "CAT", 65d},
	                         new object[] {"IBM", "IBM", 149d},
	                         new object[] {"IBM", "IBM", 149d}
	                });

	        SendEvent("KGB", 75);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
	                new object[][]{
	                         new object[] {"CAT", "CAT", 65d},
	                         new object[] {"CAT", "CAT", 65d},
	                         new object[] {"IBM", "IBM", 149d},
	                         new object[] {"IBM", "IBM", 149d},
	                         new object[] {"KGB", "KGB", 75d}
	                });
	    }

	    private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

	    private void SendEvent(string symbol, long volume, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, volume, null);
		    _epService.EPRuntime.SendEvent(bean);
		}

		private void SendJoinEvents()
		{
			_epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
			_epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
		}

	    private void RunAssertionDefault()
	    {
	        SendEvent("IBM", 110, 3);
	        SendEvent("IBM", 120, 4);
	        SendEvent("CMU", 130, 1);
	        SendEvent("CMU", 140, 2);
	        SendEvent("CAT", 150, 5);
	        SendEvent("CAT", 160, 6);

	        var fields = "Symbol,Volume,mySum".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
	                new object[][]{ new object[] {"CMU", 130L, 1.0},  new object[] {"CMU", 140L, 3.0},  new object[] {"IBM", 110L, 3.0},
	                         new object[] {"CAT", 150L, 5.0},  new object[] {"IBM", 120L, 7.0},  new object[] {"CAT", 160L, 11.0}});
	        Assert.IsNull(_testListener.LastOldData);
	    }

	    private void RunAssertionDefaultNoVolume()
	    {
	        SendEvent("IBM", 110, 3);
	        SendEvent("IBM", 120, 4);
	        SendEvent("CMU", 130, 1);
	        SendEvent("CMU", 140, 2);
	        SendEvent("CAT", 150, 5);
	        SendEvent("CAT", 160, 6);

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
	                new object[][]{ new object[] {"CMU", 1.0},  new object[] {"CMU", 3.0},  new object[] {"IBM", 3.0},
	                         new object[] {"CAT", 5.0},  new object[] {"IBM", 7.0},  new object[] {"CAT", 11.0}});
	        Assert.IsNull(_testListener.LastOldData);
	    }
	}
} // end of namespace
