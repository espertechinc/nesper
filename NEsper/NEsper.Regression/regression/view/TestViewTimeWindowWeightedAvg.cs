///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeWindowWeightedAvg
    {
        private const String SYMBOL = "CSCO.O";
        private const String FEED = "feed1";

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Threading.IsInternalTimerEnabled = true;
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
        public void TestWindowStats()
        {
            // Set up a 1 second time window
            EPStatement weightedAvgView = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                    "(Symbol='" + SYMBOL + "')#time(3.0)#weighted_avg(Price, Volume, Symbol, Feed)");
            weightedAvgView.Events += _testListener.Update;

            Assert.AreEqual(typeof(double?), weightedAvgView.EventType.GetPropertyType("average"));
            _testListener.Reset();

            // Send 2 events, E1 and E2 at +0sec
            _epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10, 500));
            CheckValue(weightedAvgView, 10);

            _epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 11, 500));
            CheckValue(weightedAvgView, 10.5);

            // Sleep for 1.5 seconds
            Sleep(1500);

            // Send 2 more events, E3 and E4 at +1.5sec
            _epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10, 1000));
            CheckValue(weightedAvgView, 10.25);
            _epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10.5, 2000));
            CheckValue(weightedAvgView, 10.375);

            // Sleep for 2 seconds, E1 and E2 should have left the window
            Sleep(2000);
            CheckValue(weightedAvgView, 10.333333333);

            // Send another event, E5 at +3.5sec
            _epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10.2, 1000));
            CheckValue(weightedAvgView, 10.3);

            // Sleep for 2.5 seconds, E3 and E4 should expire
            Sleep(2500);
            CheckValue(weightedAvgView, 10.2);

            // Sleep for 1 seconds, E5 should have expired
            Sleep(1000);
            CheckValue(weightedAvgView, Double.NaN);
        }

        private SupportMarketDataBean MakeBean(String symbol, double price, long volume)
        {
            return new SupportMarketDataBean(symbol, price, volume, FEED);
        }

        private void CheckValue(EPStatement weightedAvgView, double avgE)
        {
            IEnumerator<EventBean> iterator = weightedAvgView.GetEnumerator();
            CheckValue(iterator.Advance(), avgE);
            Assert.IsTrue(iterator.MoveNext() == false);

            Assert.IsTrue(_testListener.LastNewData.Length == 1);
            EventBean listenerValues = _testListener.LastNewData[0];
            CheckValue(listenerValues, avgE);

            _testListener.Reset();
        }

        private void CheckValue(EventBean values, double avgE)
        {
            double avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
            Assert.AreEqual(FEED, values.Get("Feed"));
            Assert.AreEqual(SYMBOL, values.Get("Symbol"));
        }

        private double GetDoubleValue(ViewFieldEnum field, EventBean theEvent)
        {
            return theEvent.Get(field.GetName()).AsDouble();
        }

        private void Sleep(int msec)
        {
            Thread.Sleep(msec);
        }
    }
}
