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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeWindowUnique : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.AllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMonthScoped(epService);
            RunAssertionWindowUnique(epService);
            RunAssertionWindowUniqueMultiKey(epService);
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select rstream * from SupportBean#Time(1 month)").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendCurrentTime(epService, "2002-02-15T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "theString".Split(','), new Object[]{"E1"});
    
            SendCurrentTimeWithMinus(epService, "2002-03-15T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-15T09:00:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "theString".Split(','), new Object[]{"E2"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        // Make sure the timer and dispatch works for externally timed events and views
        private void RunAssertionWindowUnique(EPServiceProvider epService) {
            // Set up a time window with a unique view attached
            EPStatement windowUniqueView = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#Time(3.0)#unique(symbol)");
            var listener = new SupportUpdateListener();
            windowUniqueView.AddListener(listener);
    
            SendTimer(epService, 0);
    
            SendEvent(epService, "IBM");
    
            Assert.IsNull(listener.LastOldData);
            SendTimer(epService, 4000);
            Assert.AreEqual(1, listener.LastOldData.Length);
        }
    
        // Make sure the timer and dispatch works for externally timed events and views
        private void RunAssertionWindowUniqueMultiKey(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            // Set up a time window with a unique view attached
            EPStatement windowUniqueView = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#Time(3.0)#unique(symbol, price)");
            var listener = new SupportUpdateListener();
            windowUniqueView.AddListener(listener);
            var fields = new string[]{"symbol", "price", "volume"};
    
            SendEvent(epService, "IBM", 10, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"IBM", 10.0, 1L});
    
            SendEvent(epService, "IBM", 11, 2L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"IBM", 11.0, 2L});
    
            SendEvent(epService, "IBM", 10, 3L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"IBM", 10.0, 3L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"IBM", 10.0, 1L});
            listener.Reset();
    
            SendEvent(epService, "IBM", 11, 4L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"IBM", 11.0, 4L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"IBM", 11.0, 2L});
            listener.Reset();
    
            SendTimer(epService, 2000);
            SendEvent(epService, null, 11, 5L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{null, 11.0, 5L});
    
            SendTimer(epService, 3000);
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"IBM", 10.0, 3L});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new Object[]{"IBM", 11.0, 4L});
            listener.Reset();
    
            SendEvent(epService, null, 11, 6L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{null, 11.0, 6L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{null, 11.0, 5L});
            listener.Reset();
    
            SendTimer(epService, 6000);
            Assert.AreEqual(1, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{null, 11.0, 6L});
            listener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol) {
            var theEvent = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendTimer(EPServiceProvider epService, long time) {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
