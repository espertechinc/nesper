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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectInKeywordPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportBean));
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
            configuration.AddEventType("S3", typeof(SupportBean_S3));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerformanceInKeywordAsPartOfSubquery(epService);
            RunAssertionPerformanceWhereClauseCoercion(epService);
            RunAssertionPerformanceWhereClause(epService);
        }
    
        private void RunAssertionPerformanceInKeywordAsPartOfSubquery(EPServiceProvider epService) {
            var eplSingleIndex = "select (select p00 from S0#keepall as s0 where s0.p01 in (s1.p10, s1.p11)) as c0 from S1 as s1";
            var stmtSingleIdx = epService.EPAdministrator.CreateEPL(eplSingleIndex);
            var listener = new SupportUpdateListener();
            stmtSingleIdx.Events += listener.Update;
    
            TryAssertionPerformanceInKeywordAsPartOfSubquery(epService, listener);
            stmtSingleIdx.Dispose();
    
            var eplMultiIdx = "select (select p00 from S0#keepall as s0 where s1.p11 in (s0.p00, s0.p01)) as c0 from S1 as s1";
            var stmtMultiIdx = epService.EPAdministrator.CreateEPL(eplMultiIdx);
            stmtMultiIdx.Events += listener.Update;
    
            TryAssertionPerformanceInKeywordAsPartOfSubquery(epService, listener);
    
            stmtMultiIdx.Dispose();
        }
    
        private void TryAssertionPerformanceInKeywordAsPartOfSubquery(EPServiceProvider epService, SupportUpdateListener listener) {
            for (var i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "v" + i, "p00_" + i));
            }
    
            var startTime = DateTimeHelper.CurrentTimeMillis;
            for (var i = 0; i < 2000; i++) {
                var index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(new SupportBean_S1(index, "x", "p00_" + index));
                Assert.AreEqual("v" + index, listener.AssertOneGetNewAndReset().Get("c0"));
            }
            var endTime = DateTimeHelper.CurrentTimeMillis;
            var delta = endTime - startTime;
    
            Assert.IsTrue(delta < 500, "Failed perf test, delta=" + delta);
        }
    
        private void RunAssertionPerformanceWhereClauseCoercion(EPServiceProvider epService) {
            var stmtText = "select IntPrimitive from MyEvent(TheString='A') as s0 where IntPrimitive in (" +
                    "select LongBoxed from MyEvent(TheString='B')#length(10000) where s0.IntPrimitive = LongBoxed)";
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload with 10k events
            for (var i = 0; i < 10000; i++) {
                var bean = new SupportBean();
                bean.TheString = "B";
                bean.LongBoxed = (long) i;
                epService.EPRuntime.SendEvent(bean);
            }
    
            var startTime = DateTimeHelper.CurrentTimeMillis;
            for (var i = 0; i < 10000; i++) {
                var index = 5000 + i % 1000;
                var bean = new SupportBean();
                bean.TheString = "A";
                bean.IntPrimitive = index;
                epService.EPRuntime.SendEvent(bean);
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
            }
            var endTime = DateTimeHelper.CurrentTimeMillis;
            var delta = endTime - startTime;
    
            Assert.IsTrue(delta < 2000, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerformanceWhereClause(EPServiceProvider epService) {
            var stmtText = "select id from S0 as s0 where p00 in (" +
                    "select p10 from S1#length(10000) where s0.p00 = p10)";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload with 10k events
            for (var i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }
    
            var startTime = DateTimeHelper.CurrentTimeMillis;
            for (var i = 0; i < 10000; i++) {
                var index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("id"));
            }
            var endTime = DateTimeHelper.CurrentTimeMillis;
            var delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    }
} // end of namespace
