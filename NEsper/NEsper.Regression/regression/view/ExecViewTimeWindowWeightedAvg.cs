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
    public class ExecViewTimeWindowWeightedAvg : RegressionExecution {
        private const string SYMBOL = "CSCO.O";
        private const string FEED = "feed1";
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            // Set up a 1 second time window
            EPStatement weightedAvgView = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                            "(symbol='" + SYMBOL + "')#time(3.0)#weighted_avg(price, volume, symbol, feed)");
            var testListener = new SupportUpdateListener();
            weightedAvgView.Events += testListener.Update;
    
            Assert.AreEqual(typeof(double?), weightedAvgView.EventType.GetPropertyType("average"));
            testListener.Reset();
    
            // Send 2 events, E1 and E2 at +0sec
            epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10, 500));
            CheckValue(epService, testListener, weightedAvgView, 10);
    
            epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 11, 500));
            CheckValue(epService, testListener, weightedAvgView, 10.5);
    
            // Sleep for 1.5 seconds
            Sleep(1500);
    
            // Send 2 more events, E3 and E4 at +1.5sec
            epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10, 1000));
            CheckValue(epService, testListener, weightedAvgView, 10.25);
            epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10.5, 2000));
            CheckValue(epService, testListener, weightedAvgView, 10.375);
    
            // Sleep for 2 seconds, E1 and E2 should have left the window
            Sleep(2000);
            CheckValue(epService, testListener, weightedAvgView, 10.333333333);
    
            // Send another event, E5 at +3.5sec
            epService.EPRuntime.SendEvent(MakeBean(SYMBOL, 10.2, 1000));
            CheckValue(epService, testListener, weightedAvgView, 10.3);
    
            // Sleep for 2.5 seconds, E3 and E4 should expire
            Sleep(2500);
            CheckValue(epService, testListener, weightedAvgView, 10.2);
    
            // Sleep for 1 seconds, E5 should have expired
            Sleep(1000);
            CheckValue(epService, testListener, weightedAvgView, Double.NaN);
        }
    
        private SupportMarketDataBean MakeBean(string symbol, double price, long volume) {
            return new SupportMarketDataBean(symbol, price, volume, FEED);
        }
    
        private void CheckValue(EPServiceProvider epService, SupportUpdateListener testListener, EPStatement weightedAvgView, double avgE) {
            IEnumerator<EventBean> iterator = weightedAvgView.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            CheckValue(iterator.Current, avgE);
            Assert.IsTrue(!iterator.MoveNext());
    
            Assert.IsTrue(testListener.LastNewData.Length == 1);
            EventBean listenerValues = testListener.LastNewData[0];
            CheckValue(listenerValues, avgE);
    
            testListener.Reset();
        }
    
        private void CheckValue(EventBean values, double avgE) {
            double avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
            Assert.AreEqual(FEED, values.Get("feed"));
            Assert.AreEqual(SYMBOL, values.Get("symbol"));
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
