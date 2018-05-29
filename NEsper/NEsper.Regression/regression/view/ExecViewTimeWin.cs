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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeWin : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionWinTimeSum(epService);
            RunAssertionWinTimeSumGroupBy(epService);
            RunAssertionWinTimeSumSingle(epService);
        }
    
        private void RunAssertionWinTimeSum(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string sumTimeExpr = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(30)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(sumTimeExpr);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            TryAssertion(epService, testListener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionWinTimeSumGroupBy(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string sumTimeUniExpr = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName +
                    "#time(30) group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            TryGroupByAssertions(epService, testListener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionWinTimeSumSingle(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string sumTimeUniExpr = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName +
                    "(symbol = 'IBM')#time(30)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            TrySingleAssertion(epService, testListener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener testListener, EPStatement stmt) {
            AssertSelectResultType(stmt);
    
            var currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            AssertEvents(testListener, SYMBOL_DELL, 10000, 51, false);
    
            SendEvent(epService, SYMBOL_IBM, 20000, 52);
            AssertEvents(testListener, SYMBOL_IBM, 20000, 103, false);
    
            SendEvent(epService, SYMBOL_DELL, 40000, 45);
            AssertEvents(testListener, SYMBOL_DELL, 40000, 148, false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
    
            SendEvent(epService, SYMBOL_IBM, 30000, 70);
            AssertEvents(testListener, SYMBOL_IBM, 30000, 70, false);
    
            SendEvent(epService, SYMBOL_DELL, 10000, 20);
            AssertEvents(testListener, SYMBOL_DELL, 10000, 90, false);
        }
    
        private void TryGroupByAssertions(EPServiceProvider epService, SupportUpdateListener testListener, EPStatement stmt) {
            AssertSelectResultType(stmt);
    
            var currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            AssertEvents(testListener, SYMBOL_DELL, 10000, 51, false);
    
            SendEvent(epService, SYMBOL_IBM, 30000, 70);
            AssertEvents(testListener, SYMBOL_IBM, 30000, 70, false);
    
            SendEvent(epService, SYMBOL_DELL, 20000, 52);
            AssertEvents(testListener, SYMBOL_DELL, 20000, 103, false);
    
            SendEvent(epService, SYMBOL_IBM, 30000, 70);
            AssertEvents(testListener, SYMBOL_IBM, 30000, 140, false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
            SendEvent(epService, SYMBOL_DELL, 10000, 90);
            AssertEvents(testListener, SYMBOL_DELL, 10000, 90, false);
    
            SendEvent(epService, SYMBOL_IBM, 30000, 120);
            AssertEvents(testListener, SYMBOL_IBM, 30000, 120, false);
    
            SendEvent(epService, SYMBOL_DELL, 20000, 90);
            AssertEvents(testListener, SYMBOL_DELL, 20000, 180, false);
    
            SendEvent(epService, SYMBOL_IBM, 30000, 120);
            AssertEvents(testListener, SYMBOL_IBM, 30000, 240, false);
        }
    
        private void TrySingleAssertion(EPServiceProvider epService, SupportUpdateListener testListener, EPStatement stmt) {
            AssertSelectResultType(stmt);
    
            var currentTime = new CurrentTimeEvent(0);
            epService.EPRuntime.SendEvent(currentTime);
    
            SendEvent(epService, SYMBOL_IBM, 20000, 52);
            AssertEvents(testListener, SYMBOL_IBM, 20000, 52, false);
    
            SendEvent(epService, SYMBOL_IBM, 20000, 100);
            AssertEvents(testListener, SYMBOL_IBM, 20000, 152, false);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));
    
            //These events are out of the window and new sums are generated
            SendEvent(epService, SYMBOL_IBM, 20000, 252);
            AssertEvents(testListener, SYMBOL_IBM, 20000, 252, false);
    
            SendEvent(epService, SYMBOL_IBM, 20000, 100);
            AssertEvents(testListener, SYMBOL_IBM, 20000, 352, false);
        }
    
        private void AssertEvents(SupportUpdateListener testListener, string symbol, long volume, double sum, bool unique) {
            EventBean[] oldData = testListener.LastOldData;
            EventBean[] newData = testListener.LastNewData;
    
            if (!unique)
                Assert.IsNull(oldData);
    
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(volume, newData[0].Get("volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));
    
            testListener.Reset();
            Assert.IsFalse(testListener.IsInvoked);
        }
    
        private void AssertSelectResultType(EPStatement stmt) {
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
