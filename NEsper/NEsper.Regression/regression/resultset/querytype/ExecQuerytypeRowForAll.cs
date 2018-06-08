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
    public class ExecQuerytypeRowForAll : RegressionExecution {
        private const string JOIN_KEY = "KEY";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
            RunAssertionAvgPerSym(epService);
            RunAssertionSelectStarStdGroupBy(epService);
            RunAssertionSelectExprStdGroupBy(epService);
            RunAssertionSelectAvgExprStdGroupBy(epService);
            RunAssertionSelectAvgStdGroupByUni(epService);
            RunAssertionSelectAvgExprGroupBy(epService);
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            string epl = "select irstream sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBean).FullName + "#time(10 sec)";
    
            SendTimerEvent(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssert(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            string epl = "select irstream sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#keepall as one, " +
                    typeof(SupportBean).FullName + "#time(10 sec) as two " +
                    "where one.TheString = two.TheString";
    
            SendTimerEvent(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            TryAssert(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssert(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            // assert select result type
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {null}});
    
            SendTimerEvent(epService, 0);
            SendEvent(epService, 10);
            Assert.AreEqual(10L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {10L}});
    
            SendTimerEvent(epService, 5000);
            SendEvent(epService, 15);
            Assert.AreEqual(25L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {25L}});
    
            SendTimerEvent(epService, 8000);
            SendEvent(epService, -5);
            Assert.AreEqual(20L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {20L}});
    
            SendTimerEvent(epService, 10000);
            Assert.AreEqual(20L, listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(10L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {10L}});
    
            SendTimerEvent(epService, 15000);
            Assert.AreEqual(10L, listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(-5L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {-5L}});
    
            SendTimerEvent(epService, 18000);
            Assert.AreEqual(-5L, listener.LastOldData[0].Get("mySum"));
            Assert.IsNull(listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"mySum"}, new object[][]{new object[] {null}});
        }
    
        private void RunAssertionAvgPerSym(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream avg(price) as avgp, sym from " + typeof(SupportPriceEvent).FullName + "#groupwin(sym)#length(2)"
            );
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportPriceEvent(1, "A"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual(1.0, theEvent.Get("avgp"));
    
            epService.EPRuntime.SendEvent(new SupportPriceEvent(2, "B"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sym"));
            Assert.AreEqual(1.5, theEvent.Get("avgp"));
    
            epService.EPRuntime.SendEvent(new SupportPriceEvent(9, "A"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((1 + 2 + 9) / 3.0, theEvent.Get("avgp"));
    
            epService.EPRuntime.SendEvent(new SupportPriceEvent(18, "B"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sym"));
            Assert.AreEqual((1 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));
    
            epService.EPRuntime.SendEvent(new SupportPriceEvent(5, "A"));
            theEvent = listener.LastNewData[0];
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((2 + 9 + 18 + 5) / 4.0, theEvent.Get("avgp"));
            theEvent = listener.LastOldData[0];
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((5 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSelectStarStdGroupBy(EPServiceProvider epService) {
            string stmtText = "select istream * from " + typeof(SupportMarketDataBean).FullName
                    + "#groupwin(symbol)#length(2)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, listener.LastNewData[0].Get("price"));
            Assert.IsTrue(listener.LastNewData[0].Underlying is SupportMarketDataBean);
    
            statement.Dispose();
        }
    
        private void RunAssertionSelectExprStdGroupBy(EPServiceProvider epService) {
            string stmtText = "select istream price from " + typeof(SupportMarketDataBean).FullName
                    + "#groupwin(symbol)#length(2)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, listener.LastNewData[0].Get("price"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSelectAvgExprStdGroupBy(EPServiceProvider epService) {
            string stmtText = "select istream avg(price) as aprice from " + typeof(SupportMarketDataBean).FullName
                    + "#groupwin(symbol)#length(2)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, listener.LastNewData[0].Get("aprice"));
            SendEvent(epService, "B", 3);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(2.0, listener.LastNewData[0].Get("aprice"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSelectAvgStdGroupByUni(EPServiceProvider epService) {
            string stmtText = "select istream average as aprice from " + typeof(SupportMarketDataBean).FullName
                    + "#groupwin(symbol)#length(2)#uni(price)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(1.0, listener.LastNewData[0].Get("aprice"));
            SendEvent(epService, "B", 3);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(3.0, listener.LastNewData[0].Get("aprice"));
            SendEvent(epService, "A", 3);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(2.0, listener.LastNewData[0].Get("aprice"));
            SendEvent(epService, "A", 10);
            SendEvent(epService, "A", 20);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(15.0, listener.LastNewData[0].Get("aprice"));
    
            statement.Dispose();
        }
    
        private void RunAssertionSelectAvgExprGroupBy(EPServiceProvider epService) {
            string stmtText = "select istream avg(price) as aprice, symbol from " + typeof(SupportMarketDataBean).FullName
                    + "#length(2) group by symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, listener.LastNewData[0].Get("aprice"));
            Assert.AreEqual("A", listener.LastNewData[0].Get("symbol"));
            SendEvent(epService, "B", 3);
            //there is no A->1 as we already got it out
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(3.0, listener.LastNewData[0].Get("aprice"));
            Assert.AreEqual("B", listener.LastNewData[0].Get("symbol"));
            SendEvent(epService, "B", 5);
            // there is NOW a A->null entry
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(2, listener.LastNewData.Length);
            Assert.AreEqual(null, listener.LastNewData[1].Get("aprice"));
            Assert.AreEqual(4.0, listener.LastNewData[0].Get("aprice"));
            Assert.AreEqual("B", listener.LastNewData[0].Get("symbol"));
            SendEvent(epService, "A", 10);
            SendEvent(epService, "A", 20);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(2, listener.LastNewData.Length);
            Assert.AreEqual(15.0, listener.LastNewData[0].Get("aprice")); //A
            Assert.AreEqual(null, listener.LastNewData[1].Get("aprice")); //B
    
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
