///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeBatchMean 
    {
        private const String SYMBOL = "CSCO.O";

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            var config = new Configuration();
            config.AddEventType<SupportBean>();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            Assert.IsFalse(_epService.EPRuntime.IsExternalClockingEnabled);
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestTimeBatchMean()
        {
            // Set up a 2 second time window
            var timeBatchMean = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                    "(Symbol='" + SYMBOL + "').win:time_batch(2).stat:uni(Volume)");
            timeBatchMean.Events += _testListener.Update;
    
            _testListener.Reset();
            CheckMeanIterator(timeBatchMean, Double.NaN);
            Assert.IsFalse(_testListener.IsInvoked);
    
            // Send a couple of events, check mean
            SendEvent(SYMBOL, 500);
            SendEvent(SYMBOL, 1000);
            CheckMeanIterator(timeBatchMean, Double.NaN);              // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(_testListener.IsInvoked);      // No new data posted to the iterator, yet
    
            // Sleep for 1 seconds
            Sleep(1000);
    
            // Send more events
            SendEvent(SYMBOL, 1000);
            SendEvent(SYMBOL, 1200);
            CheckMeanIterator(timeBatchMean, Double.NaN);              // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(_testListener.IsInvoked);
    
            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(timeBatchMean, 925);                 // Now the statistics view received the first batch
            Assert.IsTrue(_testListener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(925);
    
            // Send more events
            SendEvent(SYMBOL, 500);
            SendEvent(SYMBOL, 600);
            SendEvent(SYMBOL, 1000);
            CheckMeanIterator(timeBatchMean, 925);              // The iterator is still showing the old result as next batch not released
            Assert.IsFalse(_testListener.IsInvoked);
    
            // Sleep for 1 seconds
            Sleep(1000);
    
            // Send more events
            SendEvent(SYMBOL, 200);
            CheckMeanIterator(timeBatchMean, 925);
            Assert.IsFalse(_testListener.IsInvoked);
    
            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(timeBatchMean, 2300d / 4d); // Now the statistics view received the second batch, the mean now is over all events
            Assert.IsTrue(_testListener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(2300d / 4d);
    
            // Send more events
            SendEvent(SYMBOL, 1200);
            CheckMeanIterator(timeBatchMean, 2300d / 4d);
            Assert.IsFalse(_testListener.IsInvoked);
    
            // Sleep for 2 seconds, no events received anymore
            Sleep(2000);
            CheckMeanIterator(timeBatchMean, 1200); // statistics view received the third batch
            Assert.IsTrue(_testListener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(1200);
    
            // try to compile with flow control, these are tested elsewhere
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(10 sec, 'FORCE_UPDATE, START_EAGER')");
        }
    
        private void SendEvent(String symbol, long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void CheckMeanListener(double meanExpected)
        {
            Assert.IsTrue(_testListener.LastNewData.Length == 1);
            var listenerValues = _testListener.LastNewData[0];
            CheckValue(listenerValues, meanExpected);
            _testListener.Reset();
        }
    
        private void CheckMeanIterator(EPStatement timeBatchMean, double meanExpected)
        {
            var en = timeBatchMean.GetEnumerator();
            Assert.IsTrue(en.MoveNext());
            CheckValue(en.Current, meanExpected);
            Assert.IsFalse(en.MoveNext());
        }
    
        private void CheckValue(EventBean values, double avgE)
        {
            var avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg,  avgE, 6));
        }
    
        private double GetDoubleValue(ViewFieldEnum field, EventBean theEvent)
        {
            return theEvent.Get(field.GetName()).AsDouble();
        }
    
        private void Sleep(int msec)
        {
            try
            {
                Thread.Sleep(msec);
            }
            catch (ThreadInterruptedException)
            {
            }
        }
    }
}
