///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeWindowUnique 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestMonthScoped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            _epService.EPAdministrator.CreateEPL("select rstream * from SupportBean.win:time(1 month)").
                Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            SendCurrentTime("2002-02-15T9:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-03-01T9:00:00.000");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[]{"E1"});

            SendCurrentTimeWithMinus("2002-03-15T9:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-03-15T9:00:00.000");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[] {"E2"});
        }

        // Make sure the timer and dispatch works for externally timed events and views
        [Test]
        public void TestWindowUnique()
        {
            // Set up a time window with a unique view attached
            EPStatement windowUniqueView = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                    ".win:time(3.0).std:unique(symbol)");
            windowUniqueView.Events += _listener.Update;
    
            SendTimer(0);
    
            SendEvent("IBM");
    
            Assert.IsNull(_listener.LastOldData);
            SendTimer(4000);
            Assert.AreEqual(1, _listener.LastOldData.Length);
        }
    
        // Make sure the timer and dispatch works for externally timed events and views
        [Test]
        public void TestWindowUniqueMultiKey()
        {
            SendTimer(0);
    
            // Set up a time window with a unique view attached
            EPStatement windowUniqueView = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                    ".win:time(3.0).std:unique(symbol, Price)");
            windowUniqueView.Events += _listener.Update;
            String[] fields = new[] {"Symbol", "Price", "Volume"};
    
            SendEvent("IBM", 10, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"IBM", 10.0, 1L});
    
            SendEvent("IBM", 11, 2L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"IBM", 11.0, 2L});
    
            SendEvent("IBM", 10, 3L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] {"IBM", 10.0, 3L});
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {"IBM", 10.0, 1L});
            _listener.Reset();
    
            SendEvent("IBM", 11, 4L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] {"IBM", 11.0, 4L});
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {"IBM", 11.0, 2L});
            _listener.Reset();
    
            SendTimer(2000);
            SendEvent(null, 11, 5L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, 11.0, 5L});
    
            SendTimer(3000);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {"IBM", 10.0, 3L});
            EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new Object[] {"IBM", 11.0, 4L});
            _listener.Reset();
    
            SendEvent(null, 11, 6L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] {null, 11.0, 6L});
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {null, 11.0, 5L});
            _listener.Reset();
    
            SendTimer(6000);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {null, 11.0, 6L});
            _listener.Reset();
        }
    
        private void SendEvent(String symbol)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, 0, 0L, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvent(String symbol, double price, long? volume)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendTimer(long time)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendCurrentTime(String time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(String time, long minus)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
}
