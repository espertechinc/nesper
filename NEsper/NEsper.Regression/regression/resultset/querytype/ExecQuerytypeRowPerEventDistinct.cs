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
    public class ExecQuerytypeRowPerEventDistinct : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select irstream symbol, sum(distinct volume) as volSum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("volSum"));
    
            SendEvent(epService, SYMBOL_DELL, 10000);
            AssertEvents(listener, SYMBOL_DELL, 10000);
    
            SendEvent(epService, SYMBOL_DELL, 10000);
            AssertEvents(listener, SYMBOL_DELL, 10000);       // still 10k since summing distinct volumes
    
            SendEvent(epService, SYMBOL_DELL, 20000);
            AssertEvents(listener, SYMBOL_DELL, 30000);
    
            SendEvent(epService, SYMBOL_IBM, 1000);
            AssertEvents(listener, SYMBOL_DELL, 31000, SYMBOL_IBM, 31000);
    
            SendEvent(epService, SYMBOL_IBM, 1000);
            AssertEvents(listener, SYMBOL_DELL, 21000, SYMBOL_IBM, 21000);
    
            SendEvent(epService, SYMBOL_IBM, 1000);
            AssertEvents(listener, SYMBOL_DELL, 1000, SYMBOL_IBM, 1000);
    
            stmt.Dispose();
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbol, long volSum) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(volSum, newData[0].Get("volSum"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbolOld, long volSumOld,
                                  string symbolNew, long volSumNew) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbolOld, oldData[0].Get("symbol"));
            Assert.AreEqual(volSumOld, oldData[0].Get("volSum"));
    
            Assert.AreEqual(symbolNew, newData[0].Get("symbol"));
            Assert.AreEqual(volSumNew, newData[0].Get("volSum"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
