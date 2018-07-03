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
	public class TestGroupByWithNull 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportMarketDataBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestCountWithNull()
        {
            var statementText =
                "select Symbol, count(*) as Value " +
                " from SupportMarketDataBean " +
                " group by Symbol " +
                " order by Symbol ";
            var fields = new string[]{ "Symbol", "Value" };

            using (var statement = _epService.EPAdministrator.CreateEPL(statementText))
            {
                statement.Events += _listener.Update;
                SendEvent(SYMBOL_DELL, 100);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { "DELL", 1L });
                SendEvent(SYMBOL_DELL, 150);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { "DELL", 2L });

                SendEvent(SYMBOL_IBM, 200);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { "IBM", 1L });
                SendEvent(SYMBOL_IBM, 250);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { "IBM", 2L });
                SendEvent(SYMBOL_IBM, 300);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { "IBM", 3L });

                SendEvent(null, 1000);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { null, 1L });
                SendEvent(null, 2000);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { null, 2L });
                SendEvent(null, 3000);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { null, 3L });
                SendEvent(null, 4000);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] { null, 4L });
            }
	    }

	    private void SendEvent(string symbol, long? volume)
	    {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
