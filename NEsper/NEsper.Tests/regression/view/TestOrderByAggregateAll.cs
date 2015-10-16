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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOrderByAggregateAll 
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
        public void TestIteratorAggregateRowPerEvent()
    	{
            String[] fields = new String[] {"Symbol", "sumPrice"};
            String statementString = "select Symbol, sum(Price) as sumPrice from " +
        	            typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
        	            typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                        "where one.Symbol = two.TheString " +
                        "order by Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
    
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
    
            SendEvent("CAT", 50);
            SendEvent("IBM", 49);
            SendEvent("CAT", 15);
            SendEvent("IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[] {"CAT", 214d},
                                    new Object[] {"CAT", 214d},
                                    new Object[] {"IBM", 214d},
                                    new Object[] {"IBM", 214d},
                                    });
    
            SendEvent("KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                    new Object[][] {
                                    new Object[] {"CAT", 289d},
                                    new Object[] {"CAT", 289d},
                                    new Object[] {"IBM", 289d},
                                    new Object[] {"IBM", 289d},
                                    new Object[] {"KGB", 289d},
                                    });
        }
    
        [Test]
        public void TestAliases()
        {
            String statementString = "select Symbol as mySymbol, sum(Price) as mySum from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                    "output every 6 events " +
                                    "order by mySymbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            String[] fields = "mySymbol,mySum".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 15.0}, new Object[] {"CAT", 21.0}, new Object[] {"CMU", 8.0}, new Object[] {"CMU", 10.0}, new Object[] {"IBM", 3.0}, new Object[] {"IBM", 7.0}});
        }
        
        [Test]
        public void TestAggregateAllJoinOrderFunction()
        {
        	String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "output every 6 events "  +
                                    "order by Volume*sum(Price), Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
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
    
            String[] fields = "Symbol".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT"}, new Object[] {"CAT"}, new Object[] {"CMU"}, new Object[] {"IBM"}, new Object[] {"IBM"}, new Object[] {"KGB"}});
        }
    
        [Test]
        public void TestAggregateAllOrderFunction()
        {
            String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                    "output every 6 events "  +
                                    "order by Volume*sum(Price), Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 2);
            SendEvent("KGB", 1);
            SendEvent("CMU", 3);
            SendEvent("IBM", 6);
            SendEvent("CAT", 6);
            SendEvent("CAT", 5);
    
            String[] fields = "Symbol".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT"}, new Object[] {"CAT"}, new Object[] {"CMU"}, new Object[] {"IBM"}, new Object[] {"IBM"}, new Object[] {"KGB"}});
    	}
    
        [Test]
        public void TestAggregateAllSum()
    	{
    		String statementString = "select Symbol, sum(Price) from " +
    		                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                "output every 6 events " +
                                "order by Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 15.0}, new Object[] {"CAT", 21.0}, new Object[] {"CMU", 8.0}, new Object[] {"CMU", 10.0}, new Object[] {"IBM", 3.0}, new Object[] {"IBM", 7.0}});
        }
    
        [Test]
        public void TestAggregateAllMaxSum()
        {
            String statementString = "select Symbol, max(sum(Price)) from " +
                                typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                "output every 6 events " +
                                "order by Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            String[] fields = "Symbol,max(sum(Price))".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 15.0}, new Object[] {"CAT", 21.0}, new Object[] {"CMU", 8.0}, new Object[] {"CMU", 10.0}, new Object[] {"IBM", 3.0}, new Object[] {"IBM", 7.0}});
        }
    
        [Test]
        public void TestAggregateAllSumHaving()
        {
            String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                    "having sum(Price) > 0 " +
                                    "output every 6 events " +
                                    "order by Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 15.0}, new Object[] {"CAT", 21.0}, new Object[] {"CMU", 8.0}, new Object[] {"CMU", 10.0}, new Object[] {"IBM", 3.0}, new Object[] {"IBM", 7.0}});
        }
    
        [Test]
        public void TestAggOrderWithSum()
        {
            String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) " +
                                    "output every 6 events "  +
                                    "order by Symbol, sum(Price)";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 15.0}, new Object[] {"CAT", 21.0}, new Object[] {"CMU", 8.0}, new Object[] {"CMU", 10.0}, new Object[] {"IBM", 3.0}, new Object[] {"IBM", 7.0}});
        }
    
        [Test]
    	public void TestAggregateAllJoin()
        {
        	String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "output every 6 events " +
                                    "order by Symbol, sum(Price)";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 11.0}, new Object[] {"CAT", 11.0}, new Object[] {"CMU", 21.0}, new Object[] {"CMU", 21.0}, new Object[] {"IBM", 18.0}, new Object[] {"IBM", 18.0}});
        }
    
        [Test]
        public void TestAggregateAllJoinMax()
        {
        	String statementString = "select Symbol, max(sum(Price)) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "output every 6 events " +
                                    "order by Symbol";
    
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            String[] fields = "Symbol,max(sum(Price))".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 11.0}, new Object[] {"CAT", 11.0}, new Object[] {"CMU", 21.0}, new Object[] {"CMU", 21.0}, new Object[] {"IBM", 18.0}, new Object[] {"IBM", 18.0}});
        }
    
        [Test]
        public void TestAggHaving()
        {
            String statementString = "select Symbol, sum(Price) from " +
                                    typeof(SupportMarketDataBean).FullName + ".win:length(10) as one, " +
                                    typeof(SupportBeanString).FullName + ".win:length(100) as two " +
                                    "where one.Symbol = two.TheString " +
                                    "having sum(Price) > 0 " +
                                    "output every 6 events " +
                                    "order by Symbol";
            _testListener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += _testListener.Update;
    
            SendEvent("IBM", 3);
            SendEvent("IBM", 4);
            SendEvent("CMU", 1);
            SendEvent("CMU", 2);
            SendEvent("CAT", 5);
            SendEvent("CAT", 6);
    
            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            String[] fields = "Symbol,sum(Price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(_testListener.LastNewData, fields, new Object[][] {
                    new Object[] {"CAT", 11.0}, new Object[] {"CAT", 11.0}, new Object[] {"CMU", 21.0}, new Object[] {"CMU", 21.0}, new Object[] {"IBM", 18.0}, new Object[] {"IBM", 18.0}});
        }
        
    	private void SendEvent(String symbol, double price)
    	{
    	    SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
    	    _epService.EPRuntime.SendEvent(bean);
    	}
    }
}
