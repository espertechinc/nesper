///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateCount : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCountPlusStar(epService);
            RunAssertionCount(epService);
            RunAssertionCountHaving(epService);
            RunAssertionSumHaving(epService);
        }
    
        private void RunAssertionCountPlusStar(EPServiceProvider epService) {
            // Test for ESPER-118
            string statementText = "select *, count(*) as cnt from " + typeof(SupportMarketDataBean).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "S0", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(1L, listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S0", listener.LastNewData[0].Get("symbol"));
    
            SendEvent(epService, "S1", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(2L, listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S1", listener.LastNewData[0].Get("symbol"));
    
            SendEvent(epService, "S2", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(3L, listener.LastNewData[0].Get("cnt"));
            Assert.AreEqual("S2", listener.LastNewData[0].Get("symbol"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCount(EPServiceProvider epService) {
            string statementText = "select count(*) as cnt from " + typeof(SupportMarketDataBean).FullName + "#time(1)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "DELL", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(1L, listener.LastNewData[0].Get("cnt"));
    
            SendEvent(epService, "DELL", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(2L, listener.LastNewData[0].Get("cnt"));
    
            SendEvent(epService, "DELL", 1L);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(3L, listener.LastNewData[0].Get("cnt"));
    
            // test invalid distinct
            SupportMessageAssertUtil.TryInvalid(epService, "select count(distinct *) from " + typeof(SupportMarketDataBean).FullName,
                    "Error starting statement: Failed to validate select-clause expression 'count(distinct *)': Invalid use of the 'distinct' keyword with count and wildcard");
    
            stmt.Dispose();
        }
    
        private void RunAssertionCountHaving(EPServiceProvider epService) {
            string theEvent = typeof(SupportBean).FullName;
            string statementText = "select irstream sum(IntPrimitive) as mysum from " + theEvent + " having sum(IntPrimitive) = 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendEvent(epService);
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("mysum"));
            SendEvent(epService);
            Assert.AreEqual(2, listener.AssertOneGetOldAndReset().Get("mysum"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumHaving(EPServiceProvider epService) {
            string theEvent = typeof(SupportBean).FullName;
            string statementText = "select irstream count(*) as mysum from " + theEvent + " having count(*) = 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendEvent(epService);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("mysum"));
            SendEvent(epService);
            Assert.AreEqual(2L, listener.AssertOneGetOldAndReset().Get("mysum"));
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService) {
            var bean = new SupportBean("", 1);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
