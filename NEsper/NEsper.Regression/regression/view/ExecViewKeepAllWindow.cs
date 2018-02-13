///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertFalse;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewKeepAllWindow : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionIterator(epService);
            RunAssertionWindowStats(epService);
        }
    
        private void RunAssertionIterator(EPServiceProvider epService) {
            string epl = "select symbol, price from " + typeof(SupportMarketDataBean).FullName + "#keepall";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            SendEvent(epService, "ABC", 20);
            SendEvent(epService, "DEF", 100);
    
            // check iterator results
            IEnumerator<EventBean> events = statement.GetEnumerator();
            EventBean theEvent = events.Next();
            Assert.AreEqual("ABC", theEvent.Get("symbol"));
            Assert.AreEqual(20d, theEvent.Get("price"));
    
            theEvent = events.Next();
            Assert.AreEqual("DEF", theEvent.Get("symbol"));
            Assert.AreEqual(100d, theEvent.Get("price"));
            Assert.IsFalse(events.HasNext());
    
            SendEvent(epService, "EFG", 50);
    
            // check iterator results
            events = statement.GetEnumerator();
            theEvent = events.Next();
            Assert.AreEqual("ABC", theEvent.Get("symbol"));
            Assert.AreEqual(20d, theEvent.Get("price"));
    
            theEvent = events.Next();
            Assert.AreEqual("DEF", theEvent.Get("symbol"));
            Assert.AreEqual(100d, theEvent.Get("price"));
    
            theEvent = events.Next();
            Assert.AreEqual("EFG", theEvent.Get("symbol"));
            Assert.AreEqual(50d, theEvent.Get("price"));
    
            statement.Destroy();
        }
    
        private void RunAssertionWindowStats(EPServiceProvider epService) {
            string epl = "select irstream symbol, count(*) as cnt, sum(price) as mysum from " + typeof(SupportMarketDataBean).FullName +
                    "#keepall group by symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
            listener.Reset();
    
            SendEvent(epService, "S1", 100);
            var fields = new string[]{"symbol", "cnt", "mysum"};
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"S1", 1L, 100d});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"S1", 0L, null});
            listener.Reset();
    
            SendEvent(epService, "S2", 50);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"S2", 1L, 50d});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"S2", 0L, null});
            listener.Reset();
    
            SendEvent(epService, "S1", 5);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"S1", 2L, 105d});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"S1", 1L, 100d});
            listener.Reset();
    
            SendEvent(epService, "S2", -1);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[]{"S2", 2L, 49d});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[]{"S2", 1L, 50d});
            listener.Reset();
    
            statement.Destroy();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
