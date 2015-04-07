///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
	public class TestSubscriberNamedMethod 
	{
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().Name);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestSubscriberNamedUpdateMethod()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        var stmt = _epService.EPAdministrator.CreateEPL("select theString from SupportBean");
	        var subscriber = new MyNamedUpdateSubscriber();
            stmt.Subscriber = new EPSubscriber(subscriber, "SomeNewDataMayHaveArrived");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.AreEqual("E1", subscriber.GetLastValue());
	    }

	    private class MyNamedUpdateSubscriber
        {
	        private string _lastValue;

	        public void SomeNewDataMayHaveArrived(object[] newData, object[] oldData) {
	            _lastValue = newData[0].ToString();
	        }

	        public string GetLastValue() {
	            return _lastValue;
	        }
	    }
	}
} // end of namespace
