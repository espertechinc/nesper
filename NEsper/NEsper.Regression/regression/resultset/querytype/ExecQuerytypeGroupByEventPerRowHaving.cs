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
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeGroupByEventPerRowHaving : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionGroupByHaving(epService, false);
            RunAssertionGroupByHaving(epService, true);
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
        }
    
        private void RunAssertionGroupByHaving(EPServiceProvider epService, bool join) {
            string epl = !join ?
                    "select * from SupportBean#length_batch(3) group by TheString having count(*) > 1" :
                    "select TheString, IntPrimitive from SupportBean_S0#lastevent, SupportBean#length_batch(3) group by TheString having count(*) > 1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            EventBean[] received = listener.GetNewDataListFlattened();
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive".Split(','),
                    new object[][]{new object[] {"E2", 20}, new object[] {"E2", 21}});
            listener.Reset();
            stmt.Dispose();
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select irstream symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol " +
                    "having sum(price) >= 50";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSum(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select irstream symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol " +
                    "having sum(price) >= 50";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionSum(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertionSum(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("mySum").GetBoxedType());
    
            string[] fields = "symbol,volume,mySum".Split(',');
            SendEvent(epService, SYMBOL_DELL, 10000, 49);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 20000, 54);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{SYMBOL_DELL, 20000L, 103d});
    
            SendEvent(epService, SYMBOL_IBM, 1000, 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_IBM, 5000, 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{SYMBOL_DELL, 10000L, 54d});
    
            SendEvent(epService, SYMBOL_IBM, 6000, 5);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
