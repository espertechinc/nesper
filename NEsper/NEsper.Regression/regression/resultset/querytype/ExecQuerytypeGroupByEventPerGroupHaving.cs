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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeGroupByEventPerGroupHaving : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionHavingCount(epService);
            RunAssertionSumJoin(epService);
            RunAssertionSumOneView(epService);
        }
    
        private void RunAssertionHavingCount(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string text = "select * from SupportBean(IntPrimitive = 3)#length(10) as e1 group by TheString having count(*) > 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    " " + typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE')" +
                    "       and one.TheString = two.symbol " +
                    "group by symbol " +
                    "having sum(price) >= 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));
    
            TryAssertion(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            string epl = "select irstream symbol, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol " +
                    "having sum(price) >= 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, SYMBOL_DELL, 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 60);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 30);
            AssertNewEvent(listener, SYMBOL_DELL, 100);
    
            SendEvent(epService, SYMBOL_IBM, 30);
            AssertOldEvent(listener, SYMBOL_DELL, 100);
    
            SendEvent(epService, SYMBOL_IBM, 80);
            AssertNewEvent(listener, SYMBOL_IBM, 110);
        }
    
        private void AssertNewEvent(SupportUpdateListener listener, string symbol, double newSum) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertOldEvent(SupportUpdateListener listener, string symbol, double newSum) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);
    
            Assert.AreEqual(newSum, oldData[0].Get("mySum"));
            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
