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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprPrior : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPriorTimewindowStats(epService);
            RunAssertionPriorStreamAndVariable(epService);
            RunAssertionPriorTimeWindow(epService);
            RunAssertionPriorExtTimedWindow(epService);
            RunAssertionPriorTimeBatchWindow(epService);
            RunAssertionPriorUnbound(epService);
            RunAssertionPriorNoDataWindowWhere(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionLongRunningSingle(epService);
                RunAssertionLongRunningUnbound(epService);
                RunAssertionLongRunningMultiple(epService);
            }
            RunAssertionPriorLengthWindow(epService);
            RunAssertionPriorLengthWindowWhere(epService);
            RunAssertionPriorSortWindow(epService);
            RunAssertionPriorTimeBatchWindowJoin(epService);
        }
    
        private void RunAssertionPriorTimewindowStats(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "SELECT prior(1, average) as value FROM SupportBean()#time(5 minutes)#uni(IntPrimitive)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            Assert.AreEqual(1.0, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.AreEqual(2.5, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorStreamAndVariable(EPServiceProvider epService) {
            TryAssertionPriorStreamAndVariable(epService, "1");
    
            // try variable
            epService.EPAdministrator.CreateEPL("create constant variable int NUM_PRIOR = 1");
            TryAssertionPriorStreamAndVariable(epService, "NUM_PRIOR");
    
            // must be a constant-value expression
            epService.EPAdministrator.CreateEPL("create variable int NUM_PRIOR_NONCONST = 1");
            try {
                TryAssertionPriorStreamAndVariable(epService, "NUM_PRIOR_NONCONST");
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'prior(NUM_PRIOR_NONCONST,s0)': Prior function requires a constant-value integer-typed index expression as the first parameter");
            }
        }
    
        private void TryAssertionPriorStreamAndVariable(EPServiceProvider epService, string priorIndex) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            string text = "select prior(" + priorIndex + ", s0) as result from S0#length(2) as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var e1 = new SupportBean_S0(3);
            epService.EPRuntime.SendEvent(e1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(e1, listener.AssertOneGetNewAndReset().Get("result"));
            Assert.AreEqual(typeof(SupportBean_S0), stmt.EventType.GetPropertyType("result"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorTimeWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prior(2, symbol) as priorSymbol, " +
                    " prior(2, price) as priorPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("priorPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D1", 1);
            AssertNewEvents(listener, "D1", null, null);
    
            SendTimer(epService, 1000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D2", 2);
            AssertNewEvents(listener, "D2", null, null);
    
            SendTimer(epService, 2000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D3", 3);
            AssertNewEvents(listener, "D3", "D1", 1d);
    
            SendTimer(epService, 3000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D4", 4);
            AssertNewEvents(listener, "D4", "D2", 2d);
    
            SendTimer(epService, 4000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D5", 5);
            AssertNewEvents(listener, "D5", "D3", 3d);
    
            SendTimer(epService, 30000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D6", 6);
            AssertNewEvents(listener, "D6", "D4", 4d);
    
            SendTimer(epService, 60000);
            AssertOldEvents(listener, "D1", null, null);
            SendTimer(epService, 61000);
            AssertOldEvents(listener, "D2", null, null);
            SendTimer(epService, 62000);
            AssertOldEvents(listener, "D3", "D1", 1d);
            SendTimer(epService, 63000);
            AssertOldEvents(listener, "D4", "D2", 2d);
            SendTimer(epService, 64000);
            AssertOldEvents(listener, "D5", "D3", 3d);
            SendTimer(epService, 90000);
            AssertOldEvents(listener, "D6", "D4", 4d);
    
            SendMarketEvent(epService, "D7", 7);
            AssertNewEvents(listener, "D7", "D5", 5d);
            SendMarketEvent(epService, "D8", 8);
            SendMarketEvent(epService, "D9", 9);
            SendMarketEvent(epService, "D10", 10);
            SendMarketEvent(epService, "D11", 11);
            listener.Reset();
    
            // release batch
            SendTimer(epService, 150000);
            EventBean[] oldData = listener.LastOldData;
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(5, oldData.Length);
            AssertEvent(oldData[0], "D7", "D5", 5d);
            AssertEvent(oldData[1], "D8", "D6", 6d);
            AssertEvent(oldData[2], "D9", "D7", 7d);
            AssertEvent(oldData[3], "D10", "D8", 8d);
            AssertEvent(oldData[4], "D11", "D9", 9d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorExtTimedWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prior(2, symbol) as priorSymbol, " +
                    " prior(3, price) as priorPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#ext_timed(volume, 1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("priorPrice"));
    
            SendMarketEvent(epService, "D1", 1, 0);
            AssertNewEvents(listener, "D1", null, null);
    
            SendMarketEvent(epService, "D2", 2, 1000);
            AssertNewEvents(listener, "D2", null, null);
    
            SendMarketEvent(epService, "D3", 3, 3000);
            AssertNewEvents(listener, "D3", "D1", null);
    
            SendMarketEvent(epService, "D4", 4, 4000);
            AssertNewEvents(listener, "D4", "D2", 1d);
    
            SendMarketEvent(epService, "D5", 5, 5000);
            AssertNewEvents(listener, "D5", "D3", 2d);
    
            SendMarketEvent(epService, "D6", 6, 30000);
            AssertNewEvents(listener, "D6", "D4", 3d);
    
            SendMarketEvent(epService, "D7", 7, 60000);
            AssertEvent(listener.LastNewData[0], "D7", "D5", 4d);
            AssertEvent(listener.LastOldData[0], "D1", null, null);
            listener.Reset();
    
            SendMarketEvent(epService, "D8", 8, 61000);
            AssertEvent(listener.LastNewData[0], "D8", "D6", 5d);
            AssertEvent(listener.LastOldData[0], "D2", null, null);
            listener.Reset();
    
            SendMarketEvent(epService, "D9", 9, 63000);
            AssertEvent(listener.LastNewData[0], "D9", "D7", 6d);
            AssertEvent(listener.LastOldData[0], "D3", "D1", null);
            listener.Reset();
    
            SendMarketEvent(epService, "D10", 10, 64000);
            AssertEvent(listener.LastNewData[0], "D10", "D8", 7d);
            AssertEvent(listener.LastOldData[0], "D4", "D2", 1d);
            listener.Reset();
    
            SendMarketEvent(epService, "D10", 10, 150000);
            EventBean[] oldData = listener.LastOldData;
            Assert.AreEqual(6, oldData.Length);
            AssertEvent(oldData[0], "D5", "D3", 2d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorTimeBatchWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prior(3, symbol) as priorSymbol, " +
                    " prior(2, price) as priorPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time_batch(1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("priorPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 60000);
            Assert.AreEqual(2, listener.LastNewData.Length);
            AssertEvent(listener.LastNewData[0], "A", null, null);
            AssertEvent(listener.LastNewData[1], "B", null, null);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 80000);
            SendMarketEvent(epService, "C", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 120000);
            Assert.AreEqual(1, listener.LastNewData.Length);
            AssertEvent(listener.LastNewData[0], "C", null, 1d);
            Assert.AreEqual(2, listener.LastOldData.Length);
            AssertEvent(listener.LastOldData[0], "A", null, null);
            listener.Reset();
    
            SendTimer(epService, 300000);
            SendMarketEvent(epService, "D", 4);
            SendMarketEvent(epService, "E", 5);
            SendMarketEvent(epService, "F", 6);
            SendMarketEvent(epService, "G", 7);
            SendTimer(epService, 360000);
            Assert.AreEqual(4, listener.LastNewData.Length);
            AssertEvent(listener.LastNewData[0], "D", "A", 2d);
            AssertEvent(listener.LastNewData[1], "E", "B", 3d);
            AssertEvent(listener.LastNewData[2], "F", "C", 4d);
            AssertEvent(listener.LastNewData[3], "G", "D", 5d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorUnbound(EPServiceProvider epService) {
            string epl = "select symbol as currSymbol, " +
                    " prior(3, symbol) as priorSymbol, " +
                    " prior(2, price) as priorPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("priorPrice"));
    
            SendMarketEvent(epService, "A", 1);
            AssertNewEvents(listener, "A", null, null);
    
            SendMarketEvent(epService, "B", 2);
            AssertNewEvents(listener, "B", null, null);
    
            SendMarketEvent(epService, "C", 3);
            AssertNewEvents(listener, "C", null, 1d);
    
            SendMarketEvent(epService, "D", 4);
            AssertNewEvents(listener, "D", "A", 2d);
    
            SendMarketEvent(epService, "E", 5);
            AssertNewEvents(listener, "E", "B", 3d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorNoDataWindowWhere(EPServiceProvider epService) {
            string text = "select * from " + typeof(SupportMarketDataBean).FullName +
                    " where prior(1, price) = 100";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketEvent(epService, "IBM", 75);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "IBM", 100);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "IBM", 120);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionLongRunningSingle(EPServiceProvider epService) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            } // excluded from instrumentation, too much data
    
            string epl = "select symbol as currSymbol, " +
                    " prior(3, symbol) as prior0Symbol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#sort(3, symbol)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var random = new Random();
            // 200000 is a better number for a memory test, however for short unit tests this is 2000
            for (int i = 0; i < 2000; i++) {
                if (i % 10000 == 0) {
                    //Log.Info(i);
                }
    
                SendMarketEvent(epService, Convert.ToString(random.Next()), 4);
    
                if (i % 1000 == 0) {
                    listener.Reset();
                }
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionLongRunningUnbound(EPServiceProvider epService) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            } // excluded from instrumentation, too much data
    
            string epl = "select symbol as currSymbol, " +
                    " prior(3, symbol) as prior0Symbol " +
                    "from " + typeof(SupportMarketDataBean).FullName;
    
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            var random = new Random();
            // 200000 is a better number for a memory test, however for short unit tests this is 2000
            for (int i = 0; i < 2000; i++) {
                if (i % 10000 == 0) {
                    //Log.Info(i);
                }
    
                SendMarketEvent(epService, Convert.ToString(random.Next()), 4);
    
                if (i % 1000 == 0) {
                    listener.Reset();
                }
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionLongRunningMultiple(EPServiceProvider epService) {
    
            string epl = "select symbol as currSymbol, " +
                    " prior(3, symbol) as prior0Symbol, " +
                    " prior(2, symbol) as prior1Symbol, " +
                    " prior(1, symbol) as prior2Symbol, " +
                    " prior(0, symbol) as prior3Symbol, " +
                    " prior(0, price) as prior0Price, " +
                    " prior(1, price) as prior1Price, " +
                    " prior(2, price) as prior2Price, " +
                    " prior(3, price) as prior3Price " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#sort(3, symbol)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var random = new Random();
            // 200000 is a better number for a memory test, however for short unit tests this is 2000
            for (int i = 0; i < 2000; i++) {
                if (i % 10000 == 0) {
                    //Log.Info(i);
                }
    
                SendMarketEvent(epService, Convert.ToString(random.Next()), 4);
    
                if (i % 1000 == 0) {
                    listener.Reset();
                }
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorLengthWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    "prior(0, symbol) as prior0Symbol, " +
                    "prior(1, symbol) as prior1Symbol, " +
                    "prior(2, symbol) as prior2Symbol, " +
                    "prior(3, symbol) as prior3Symbol, " +
                    "prior(0, price) as prior0Price, " +
                    "prior(1, price) as prior1Price, " +
                    "prior(2, price) as prior2Price, " +
                    "prior(3, price) as prior3Price " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prior0Symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("prior0Price"));
    
            SendMarketEvent(epService, "A", 1);
            AssertNewEvents(listener, "A", "A", 1d, null, null, null, null, null, null);
            SendMarketEvent(epService, "B", 2);
            AssertNewEvents(listener, "B", "B", 2d, "A", 1d, null, null, null, null);
            SendMarketEvent(epService, "C", 3);
            AssertNewEvents(listener, "C", "C", 3d, "B", 2d, "A", 1d, null, null);
    
            SendMarketEvent(epService, "D", 4);
            EventBean newEvent = listener.LastNewData[0];
            EventBean oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
            AssertEventProps(listener, oldEvent, "A", "A", 1d, null, null, null, null, null, null);
    
            SendMarketEvent(epService, "E", 5);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
            AssertEventProps(listener, oldEvent, "B", "B", 2d, "A", 1d, null, null, null, null);
    
            SendMarketEvent(epService, "F", 6);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "F", "F", 6d, "E", 5d, "D", 4d, "C", 3d);
            AssertEventProps(listener, oldEvent, "C", "C", 3d, "B", 2d, "A", 1d, null, null);
    
            SendMarketEvent(epService, "G", 7);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "G", "G", 7d, "F", 6d, "E", 5d, "D", 4d);
            AssertEventProps(listener, oldEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
    
            SendMarketEvent(epService, "G", 8);
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, oldEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorLengthWindowWhere(EPServiceProvider epService) {
            string epl = "select prior(2, symbol) as currSymbol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(1) " +
                    "where prior(2, price) > 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 130);
            SendMarketEvent(epService, "C", 10);
            Assert.IsFalse(listener.IsInvoked);
            SendMarketEvent(epService, "D", 5);
            Assert.AreEqual("B", listener.AssertOneGetNewAndReset().Get("currSymbol"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPriorSortWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prior(0, symbol) as prior0Symbol, " +
                    " prior(1, symbol) as prior1Symbol, " +
                    " prior(2, symbol) as prior2Symbol, " +
                    " prior(3, symbol) as prior3Symbol, " +
                    " prior(0, price) as prior0Price, " +
                    " prior(1, price) as prior1Price, " +
                    " prior(2, price) as prior2Price, " +
                    " prior(3, price) as prior3Price " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#sort(3, symbol)";
            TryPriorSortWindow(epService, epl);
    
            epl = "select irstream symbol as currSymbol, " +
                    " prior(3, symbol) as prior3Symbol, " +
                    " prior(1, symbol) as prior1Symbol, " +
                    " prior(2, symbol) as prior2Symbol, " +
                    " prior(0, symbol) as prior0Symbol, " +
                    " prior(2, price) as prior2Price, " +
                    " prior(1, price) as prior1Price, " +
                    " prior(0, price) as prior0Price, " +
                    " prior(3, price) as prior3Price " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#sort(3, symbol)";
            TryPriorSortWindow(epService, epl);
        }
    
        private void RunAssertionPriorTimeBatchWindowJoin(EPServiceProvider epService) {
            string epl = "select TheString as currSymbol, " +
                    "prior(2, symbol) as priorSymbol, " +
                    "prior(1, price) as priorPrice " +
                    "from " + typeof(SupportBean).FullName + "#keepall, " +
                    typeof(SupportMarketDataBean).FullName + "#time_batch(1 min)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("priorSymbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("priorPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 2);
            SendBeanEvent(epService, "X1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 60000);
            Assert.AreEqual(2, listener.LastNewData.Length);
            AssertEvent(listener.LastNewData[0], "X1", null, null);
            AssertEvent(listener.LastNewData[1], "X1", null, 1d);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendMarketEvent(epService, "C1", 11);
            SendMarketEvent(epService, "C2", 12);
            SendMarketEvent(epService, "C3", 13);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 120000);
            Assert.AreEqual(3, listener.LastNewData.Length);
            AssertEvent(listener.LastNewData[0], "X1", "A", 2d);
            AssertEvent(listener.LastNewData[1], "X1", "B", 11d);
            AssertEvent(listener.LastNewData[2], "X1", "C1", 12d);
    
            stmt.Dispose();
        }
    
        private void TryPriorSortWindow(EPServiceProvider epService, string epl) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendMarketEvent(epService, "COX", 30);
            AssertNewEvents(listener, "COX", "COX", 30d, null, null, null, null, null, null);
    
            SendMarketEvent(epService, "IBM", 45);
            AssertNewEvents(listener, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);
    
            SendMarketEvent(epService, "MSFT", 33);
            AssertNewEvents(listener, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);
    
            SendMarketEvent(epService, "XXX", 55);
            EventBean newEvent = listener.LastNewData[0];
            EventBean oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
            AssertEventProps(listener, oldEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
    
            SendMarketEvent(epService, "BOO", 20);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "BOO", "BOO", 20d, "XXX", 55d, "MSFT", 33d, "IBM", 45d);
            AssertEventProps(listener, oldEvent, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);
    
            SendMarketEvent(epService, "DOR", 1);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
            AssertEventProps(listener, oldEvent, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);
    
            SendMarketEvent(epService, "AAA", 2);
            newEvent = listener.LastNewData[0];
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "AAA", "AAA", 2d, "DOR", 1d, "BOO", 20d, "XXX", 55d);
            AssertEventProps(listener, oldEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
    
            SendMarketEvent(epService, "AAB", 2);
            oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, oldEvent, "COX", "COX", 30d, null, null, null, null, null, null);
            listener.Reset();
    
            statement.Stop();
        }
    
        private void AssertNewEvents(SupportUpdateListener listener, string currSymbol,
                                     string priorSymbol,
                                     double? priorPrice) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            AssertEvent(newData[0], currSymbol, priorSymbol, priorPrice);
    
            listener.Reset();
        }
    
        private void AssertEvent(EventBean eventBean,
                                 string currSymbol,
                                 string priorSymbol,
                                 double? priorPrice) {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(priorSymbol, eventBean.Get("priorSymbol"));
            Assert.AreEqual(priorPrice, eventBean.Get("priorPrice"));
        }
    
        private void AssertNewEvents(SupportUpdateListener listener, string currSymbol,
                                     string prior0Symbol,
                                     double? prior0Price,
                                     string prior1Symbol,
                                     double? prior1Price,
                                     string prior2Symbol,
                                     double? prior2Price,
                                     string prior3Symbol,
                                     double? prior3Price) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
            AssertEventProps(listener, newData[0], currSymbol, prior0Symbol, prior0Price, prior1Symbol, prior1Price, prior2Symbol, prior2Price, prior3Symbol, prior3Price);
    
            listener.Reset();
        }
    
        private void AssertEventProps(SupportUpdateListener listener, EventBean eventBean,
                                      string currSymbol,
                                      string prior0Symbol,
                                      double? prior0Price,
                                      string prior1Symbol,
                                      double? prior1Price,
                                      string prior2Symbol,
                                      double? prior2Price,
                                      string prior3Symbol,
                                      double? prior3Price) {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prior0Symbol, eventBean.Get("prior0Symbol"));
            Assert.AreEqual(prior0Price, eventBean.Get("prior0Price"));
            Assert.AreEqual(prior1Symbol, eventBean.Get("prior1Symbol"));
            Assert.AreEqual(prior1Price, eventBean.Get("prior1Price"));
            Assert.AreEqual(prior2Symbol, eventBean.Get("prior2Symbol"));
            Assert.AreEqual(prior2Price, eventBean.Get("prior2Price"));
            Assert.AreEqual(prior3Symbol, eventBean.Get("prior3Symbol"));
            Assert.AreEqual(prior3Price, eventBean.Get("prior3Price"));
    
            listener.Reset();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendMarketEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void AssertOldEvents(SupportUpdateListener listener, string currSymbol,
                                     string priorSymbol,
                                     double? priorPrice) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);
    
            Assert.AreEqual(currSymbol, oldData[0].Get("currSymbol"));
            Assert.AreEqual(priorSymbol, oldData[0].Get("priorSymbol"));
            Assert.AreEqual(priorPrice, oldData[0].Get("priorPrice"));
    
            listener.Reset();
        }
    }
} // end of namespace
