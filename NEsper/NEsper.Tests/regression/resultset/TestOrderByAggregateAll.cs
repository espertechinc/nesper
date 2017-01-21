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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOrderByAggregateAll 
	{
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
	    public void TestIteratorAggregateRowPerEvent()
		{
	        var fields = new string[] {"Symbol", "sumPrice"};
	        var statementString = "select Symbol, sum(Price) as sumPrice from " +
	    	            typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	    	            typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                    "where one.Symbol = two.TheString " +
	                    "order by Symbol";
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));

	        SendEvent("CAT", 50);
	        SendEvent("IBM", 49);
	        SendEvent("CAT", 15);
	        SendEvent("IBM", 100);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
	                new object[][]{
	                         new object[] {"CAT", 214d},
	                         new object[] {"CAT", 214d},
	                         new object[] {"IBM", 214d},
	                         new object[] {"IBM", 214d}
	                });

	        SendEvent("KGB", 75);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
	                new object[][]{
	                         new object[] {"CAT", 289d},
	                         new object[] {"CAT", 289d},
	                         new object[] {"IBM", 289d},
	                         new object[] {"IBM", 289d},
	                         new object[] {"KGB", 289d}
	                });
	    }

        [Test]
	    public void TestAliases()
	    {
	        var statementString = "select Symbol as mySymbol, sum(Price) as mySum from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                                "output every 6 events " +
	                                "order by mySymbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        var fields = "mySymbol,mySum".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 15.0},  new object[] {"CAT", 21.0},  new object[] {"CMU", 8.0},  new object[] {"CMU", 10.0},  new object[] {"IBM", 3.0},  new object[] {"IBM", 7.0}});
	    }

        [Test]
	    public void TestAggregateAllJoinOrderFunction()
	    {
	    	var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "output every 6 events "  +
	                                "order by Volume*sum(Price), Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);
	        SendEvent("IBM", 2);
	        SendEvent("KGB", 1);
	        SendEvent("CMU", 3);
	        SendEvent("IBM", 6);
	        SendEvent("CAT", 6);
	        SendEvent("CAT", 5);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));

	        var fields = "Symbol".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT"},  new object[] {"CAT"},  new object[] {"CMU"},  new object[] {"IBM"},  new object[] {"IBM"},  new object[] {"KGB"}});
	    }

        [Test]
	    public void TestAggregateAllOrderFunction()
	    {
	        var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                                "output every 6 events "  +
	                                "order by Volume*sum(Price), Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 2);
	        SendEvent("KGB", 1);
	        SendEvent("CMU", 3);
	        SendEvent("IBM", 6);
	        SendEvent("CAT", 6);
	        SendEvent("CAT", 5);

	        var fields = "Symbol".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT"},  new object[] {"CAT"},  new object[] {"CMU"},  new object[] {"IBM"},  new object[] {"IBM"},  new object[] {"KGB"}});
		}

        [Test]
	    public void TestAggregateAllSum()
		{
			var statementString = "select Symbol, sum(Price) from " +
			                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                            "output every 6 events " +
	                            "order by Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 15.0},  new object[] {"CAT", 21.0},  new object[] {"CMU", 8.0},  new object[] {"CMU", 10.0},  new object[] {"IBM", 3.0},  new object[] {"IBM", 7.0}});
	    }

        [Test]
	    public void TestAggregateAllMaxSum()
	    {
	        var statementString = "select Symbol, max(sum(Price)) from " +
	                            typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                            "output every 6 events " +
	                            "order by Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        var fields = "Symbol,max(sum(Price))".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 15.0},  new object[] {"CAT", 21.0},  new object[] {"CMU", 8.0},  new object[] {"CMU", 10.0},  new object[] {"IBM", 3.0},  new object[] {"IBM", 7.0}});
	    }

        [Test]
	    public void TestAggregateAllSumHaving()
	    {
	        var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                                "having sum(Price) > 0 " +
	                                "output every 6 events " +
	                                "order by Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 15.0},  new object[] {"CAT", 21.0},  new object[] {"CMU", 8.0},  new object[] {"CMU", 10.0},  new object[] {"IBM", 3.0},  new object[] {"IBM", 7.0}});
	    }

        [Test]
	    public void TestAggOrderWithSum()
	    {
	        var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
	                                "output every 6 events "  +
	                                "order by Symbol, sum(Price)";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 15.0},  new object[] {"CAT", 21.0},  new object[] {"CMU", 8.0},  new object[] {"CMU", 10.0},  new object[] {"IBM", 3.0},  new object[] {"IBM", 7.0}});
	    }

        [Test]
		public void TestAggregateAllJoin()
	    {
	    	var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "output every 6 events " +
	                                "order by Symbol, sum(Price)";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 11.0},  new object[] {"CAT", 11.0},  new object[] {"CMU", 21.0},  new object[] {"CMU", 21.0},  new object[] {"IBM", 18.0},  new object[] {"IBM", 18.0}});
	    }

        [Test]
	    public void TestAggregateAllJoinMax()
	    {
	    	var statementString = "select Symbol, max(sum(Price)) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "output every 6 events " +
	                                "order by Symbol";

	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));

	        var fields = "Symbol,max(sum(Price))".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 11.0},  new object[] {"CAT", 11.0},  new object[] {"CMU", 21.0},  new object[] {"CMU", 21.0},  new object[] {"IBM", 18.0},  new object[] {"IBM", 18.0}});
	    }

        [Test]
	    public void TestAggHaving()
	    {
	        var statementString = "select Symbol, sum(Price) from " +
	                                typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
	                                typeof(SupportBeanString).FullName + ".win:length(100) as two " +
	                                "where one.Symbol = two.TheString " +
	                                "having sum(Price) > 0 " +
	                                "output every 6 events " +
	                                "order by Symbol";
	        _testListener = new SupportUpdateListener();
	        var statement = _epService.EPAdministrator.CreateEPL(statementString);
	        statement.AddListener(_testListener);

	        SendEvent("IBM", 3);
	        SendEvent("IBM", 4);
	        SendEvent("CMU", 1);
	        SendEvent("CMU", 2);
	        SendEvent("CAT", 5);
	        SendEvent("CAT", 6);

	        _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));

	        var fields = "Symbol,sum(Price)".Split(',');
	        EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new object[][]{
	                 new object[] {"CAT", 11.0},  new object[] {"CAT", 11.0},  new object[] {"CMU", 21.0},  new object[] {"CMU", 21.0},  new object[] {"IBM", 18.0},  new object[] {"IBM", 18.0}});
	    }

		private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
		    _epService.EPRuntime.SendEvent(bean);
		}
	}
} // end of namespace
