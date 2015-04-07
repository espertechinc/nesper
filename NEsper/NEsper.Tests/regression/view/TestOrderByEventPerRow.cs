///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOrderByEventPerRow 
    {
        private EPServiceProvider _epService;
    	private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestAliasesAggregationCompile()
        {
            String statementString = "select Symbol, Volume, sum(Price) as mySum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by sum(Price), Symbol";
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(statementString);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(statementString, model.ToEPL());
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.Create(model);
            statement.Events += _testListener.Update;
    
            RunAssertionDefault();
        }
    
        [Test]
        public void TestAliasesAggregationOM()
        {
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("Symbol", "Volume").Add(Expressions.Sum("Price"), "mySum");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView(View.Create("win", "length", Expressions.Constant(20))));
            model.GroupByClause = GroupByClause.Create("Symbol");
            model.OutputLimitClause = OutputLimitClause.Create(6);
            model.OrderByClause = OrderByClause.Create(Expressions.Sum("Price")).Add("Symbol", false);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            String statementString = "select Symbol, Volume, sum(Price) as mySum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by sum(Price), Symbol";
    
            Assert.AreEqual(statementString, model.ToEPL());
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.Create(model);
            statement.Events += _testListener.Update;
    
            RunAssertionDefault();
        }
    
        [Test]
        public void TestAliases()
    	{
    		String statementString = "select Symbol, Volume, sum(Price) as mySum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by mySum, Symbol";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
    
            RunAssertionDefault();
    	}
    
        [Test]
        public void TestGroupBySwitch()
    	{
    		// Instead of the row-per-group behavior, these should
    		// get row-per-event behavior since there are properties
    		// in the order-by that are not in the select expression.
    		String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by sum(Price), Symbol, Volume";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
    
            RunAssertionDefaultNoVolume();
        }
    
        [Test]
        public void TestGroupBySwitchJoin()
    	{
            String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "group by Symbol " +
                                    "output every 6 events " +
                                    "order by sum(Price), Symbol, Volume";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
    
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
        	String statementString = "select Symbol, Volume, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) " +
                                    "group by Symbol " +
                                    "output last every 6 events " +
                                    "order by sum(Price)";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
    
            RunAssertionLast();
        }
    
        [Test]
        public void TestLastJoin()
        {
            String statementString = "select Symbol, Volume, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(20) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "group by Symbol " +
                                    "output last every 6 events " +
                                    "order by sum(Price)";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            _testListener = new SupportUpdateListener();
            statement.Events += _testListener.Update;
    
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
    
            String[] fields = "Symbol,Volume,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] {"CMU", 104L, 3.0}, new Object[] {"IBM", 102L, 7.0}, new Object[] {"CAT", 106L, 11.0}});
            Assert.IsNull(_testListener.LastOldData);
    
            SendEvent("IBM", 201, 3);
            SendEvent("IBM", 202, 4);
            SendEvent("CMU", 203, 5);
            SendEvent("CMU", 204, 5);
            SendEvent("DOG", 205, 0);
            SendEvent("DOG", 206, 1);
    
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] {"DOG", 206L, 1.0}, new Object[] {"CMU", 204L, 13.0}, new Object[] {"IBM", 202L, 14.0}});
            Assert.IsNull(_testListener.LastOldData);
        }
    
    
        [Test]
        public void TestIteratorGroupByEventPerRow()
    	{
            String[] fields = new String[] { "Symbol", "TheString", "sumPrice" };
            String statementString = "select Symbol, TheString, sum(Price) as sumPrice from " +
        	            typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
        	            typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                        "where one.Symbol = two.TheString " +
                        "group by Symbol " +
                        "order by Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            SendJoinEvents();
            SendEvent("CAT", 50);
            SendEvent("IBM", 49);
            SendEvent("CAT", 15);
            SendEvent("IBM", 100);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[] {"CAT", "CAT", 65d},
                                    new Object[] {"CAT", "CAT", 65d},
                                    new Object[] {"IBM", "IBM", 149d},
                                    new Object[] {"IBM", "IBM", 149d},
                                    });
    
            SendEvent("KGB", 75);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[] {"CAT", "CAT", 65d},
                                    new Object[] {"CAT", "CAT", 65d},
                                    new Object[] {"IBM", "IBM", 149d},
                                    new Object[] {"IBM", "IBM", 149d},
                                    new Object[] {"KGB", "KGB", 75d},
                                    });
        }
    
        private void SendEvent(String symbol, double price)
    	{
    	    SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
    	    _epService.EPRuntime.SendEvent(bean);
    	}
    
        private void SendEvent(String symbol, long volume, double price)
    	{
    	    SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, null);
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
    
            String[] fields = "Symbol,Volume,mySum".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] {"CMU", 130L, 1.0}, new Object[] {"CMU", 140L, 3.0}, new Object[] {"IBM", 110L, 3.0},
                            new Object[] {"CAT", 150L, 5.0}, new Object[] {"IBM", 120L, 7.0}, new Object[] {"CAT", 160L, 11.0} });
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
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields,
                    new Object[][] { new Object[] {"CMU", 1.0}, new Object[] {"CMU", 3.0}, new Object[] {"IBM", 3.0},
                            new Object[] {"CAT", 5.0}, new Object[] {"IBM", 7.0}, new Object[] {"CAT", 11.0} });
            Assert.IsNull(_testListener.LastOldData);
        }
    }
}
