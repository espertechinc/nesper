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

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRowForAllHaving : RegressionExecution {
        private const string JOIN_KEY = "KEY";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
            RunAssertionAvgGroupWindow(epService);
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            string epl = "select irstream sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBean).FullName + "#time(10 seconds) " +
                    "having sum(LongBoxed) > 10";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssert(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            string epl = "select irstream sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#time(10 seconds) as one, " +
                    typeof(SupportBean).FullName + "#time(10 seconds) as two " +
                    "where one.TheString = two.TheString " +
                    "having sum(LongBoxed) > 10";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            TryAssert(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssert(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("mySum"));
    
            SendTimerEvent(epService, 0);
            SendEvent(epService, 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerEvent(epService, 5000);
            SendEvent(epService, 15);
            Assert.AreEqual(25L, listener.GetAndResetLastNewData()[0].Get("mySum"));
    
            SendTimerEvent(epService, 8000);
            SendEvent(epService, -5);
            Assert.AreEqual(20L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(listener.LastOldData);
    
            SendTimerEvent(epService, 10000);
            Assert.AreEqual(20L, listener.LastOldData[0].Get("mySum"));
            Assert.IsNull(listener.GetAndResetLastNewData());
        }
    
        private void RunAssertionAvgGroupWindow(EPServiceProvider epService) {
            //string stmtText = "select istream avg(price) as aprice from "+ typeof(SupportMarketDataBean).FullName
            //        +"#groupwin(symbol)#length(1) having avg(price) <= 0";
            string stmtText = "select istream avg(price) as aprice from " + typeof(SupportMarketDataBean).FullName
                    + "#unique(symbol) having avg(price) <= 0";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", -1);
            Assert.AreEqual(-1.0d, listener.LastNewData[0].Get("aprice"));
            listener.Reset();
    
            SendEvent(epService, "A", 5);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "B", -6);
            Assert.AreEqual(-.5d, listener.LastNewData[0].Get("aprice"));
            listener.Reset();
    
            SendEvent(epService, "C", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "C", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "C", -2);
            Assert.AreEqual(-1d, listener.LastNewData[0].Get("aprice"));
            listener.Reset();
    
            statement.Dispose();
        }
    
        private Object SendEvent(EPServiceProvider epService, string symbol, double price) {
            var theEvent = new SupportMarketDataBean(symbol, price, null, null);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed) {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed) {
            SendEvent(epService, longBoxed, 0, (short) 0);
        }
    
        private void SendTimerEvent(EPServiceProvider epService, long msec) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
        }
    }
} // end of namespace
