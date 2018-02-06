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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.view;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeBatchMean : RegressionExecution {
        private const string SYMBOL = "CSCO.O";
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // Set up a 2 second time window
            EPStatement timeBatchMean = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                            "(symbol='" + SYMBOL + "')#time_batch(2)#uni(volume)");
            var listener = new SupportUpdateListener();
            timeBatchMean.Events += listener.Update;
    
            listener.Reset();
            CheckMeanIterator(timeBatchMean, Double.NaN);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send a couple of events, check mean
            SendEvent(epService, SYMBOL, 500);
            SendEvent(epService, SYMBOL, 1000);
            CheckMeanIterator(timeBatchMean, Double.NaN);              // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(listener.IsInvoked);      // No new data posted to the iterator, yet
    
            // Sleep for 1 seconds
            Sleep(1000);
    
            // Send more events
            SendEvent(epService, SYMBOL, 1000);
            SendEvent(epService, SYMBOL, 1200);
            CheckMeanIterator(timeBatchMean, Double.NaN);              // The iterator is still showing no result yet as no batch was released
            Assert.IsFalse(listener.IsInvoked);
    
            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(timeBatchMean, 925);                 // Now the statistics view received the first batch
            Assert.IsTrue(listener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(listener, 925);
    
            // Send more events
            SendEvent(epService, SYMBOL, 500);
            SendEvent(epService, SYMBOL, 600);
            SendEvent(epService, SYMBOL, 1000);
            CheckMeanIterator(timeBatchMean, 925);              // The iterator is still showing the old result as next batch not released
            Assert.IsFalse(listener.IsInvoked);
    
            // Sleep for 1 seconds
            Sleep(1000);
    
            // Send more events
            SendEvent(epService, SYMBOL, 200);
            CheckMeanIterator(timeBatchMean, 925);
            Assert.IsFalse(listener.IsInvoked);
    
            // Sleep for 1.5 seconds, thus triggering a new batch
            Sleep(1500);
            CheckMeanIterator(timeBatchMean, 2300d / 4d); // Now the statistics view received the second batch, the mean now is over all events
            Assert.IsTrue(listener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(listener, 2300d / 4d);
    
            // Send more events
            SendEvent(epService, SYMBOL, 1200);
            CheckMeanIterator(timeBatchMean, 2300d / 4d);
            Assert.IsFalse(listener.IsInvoked);
    
            // Sleep for 2 seconds, no events received anymore
            Sleep(2000);
            CheckMeanIterator(timeBatchMean, 1200); // statistics view received the third batch
            Assert.IsTrue(listener.IsInvoked);   // Listener has been invoked
            CheckMeanListener(listener, 1200);
    
            // try to compile with flow control, these are tested elsewhere
            epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(10 sec, 'FORCE_UPDATE, START_EAGER')");
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume) {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void CheckMeanListener(SupportUpdateListener listener, double meanExpected) {
            Assert.IsTrue(listener.LastNewData.Length == 1);
            EventBean listenerValues = listener.LastNewData[0];
            CheckValue(listenerValues, meanExpected);
            listener.Reset();
        }
    
        private void CheckMeanIterator(EPStatement timeBatchMean, double meanExpected) {
            IEnumerator<EventBean> iterator = timeBatchMean.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            CheckValue(iterator.Current, meanExpected);
            Assert.IsFalse(iterator.MoveNext());
        }
    
        private void CheckValue(EventBean values, double avgE) {
            double avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
        }
    
        private double GetDoubleValue(ViewFieldEnum field, EventBean theEvent) {
            return theEvent.Get(field.GetName()).AsDouble();
        }
    
        private void Sleep(int msec) {
            try {
                Thread.Sleep(msec);
            } catch (ThreadInterruptedException) {
            }
        }
    }
} // end of namespace
