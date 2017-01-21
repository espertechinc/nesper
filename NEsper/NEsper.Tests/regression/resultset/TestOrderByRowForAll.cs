///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOrderByRowForAll 
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestIteratorAggregateRowForAll()
		{
	        var fields = new string[] {"sumPrice"};
	        var statementString = "select sum(Price) as sumPrice from " +
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
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{ new object[] {214d}});

	        SendEvent("KGB", 75);
	        EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{ new object[] {289d}});

	        // JIRA ESPER-644 Infinite loop when restarting a statement
	        _epService.EPAdministrator.Configuration.AddEventType("FB", Collections.SingletonMap<string, object>("timeTaken", typeof(double)));
	        var stmt = _epService.EPAdministrator.CreateEPL("select avg(timeTaken) as timeTaken from FB order by timeTaken desc");
	        stmt.Stop();
	        stmt.Start();
	    }

	    private void SendEvent(string symbol, double price)
		{
		    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
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
	}
} // end of namespace
