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

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectFilteredPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
            configuration.AddEventType("S3", typeof(SupportBean_S3));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerformanceOneCriteria(epService);
            RunAssertionPerformanceTwoCriteria(epService);
            RunAssertionPerformanceJoin3CriteriaSceneOne(epService);
            RunAssertionPerformanceJoin3CriteriaSceneTwo(epService);
        }
    
        private void RunAssertionPerformanceOneCriteria(EPServiceProvider epService) {
            string stmtText = "select (select p10 from S1#length(100000) where id = s0.id) as value from S0 as s0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                int index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                Assert.AreEqual(Convert.ToString(index), listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerformanceTwoCriteria(EPServiceProvider epService) {
            string stmtText = "select (select p10 from S1#length(100000) where s0.id = id and p10 = s0.p00) as value from S0 as s0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                int index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                Assert.AreEqual(Convert.ToString(index), listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerformanceJoin3CriteriaSceneOne(EPServiceProvider epService) {
            string stmtText = "select (select p00 from S0#length(100000) where p00 = s1.p10 and p01 = s2.p20 and p02 = s3.p30) as value " +
                    "from S1#length(100000) as s1, S2#length(100000) as s2, S3#length(100000) as s3 where s1.id = s2.id and s2.id = s3.id";
            TryPerfJoin3Criteria(epService, stmtText);
        }
    
        private void RunAssertionPerformanceJoin3CriteriaSceneTwo(EPServiceProvider epService) {
            string stmtText = "select (select p00 from S0#length(100000) where p01 = s2.p20 and p00 = s1.p10 and p02 = s3.p30 and id >= 0) as value " +
                    "from S3#length(100000) as s3, S1#length(100000) as s1, S2#length(100000) as s2 where s2.id = s3.id and s1.id = s2.id";
            TryPerfJoin3Criteria(epService, stmtText);
        }
    
        private void TryPerfJoin3Criteria(EPServiceProvider epService, string stmtText) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, Convert.ToString(i), Convert.ToString(i + 1), Convert.ToString(i + 2)));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = i;
                epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(index)));
                epService.EPRuntime.SendEvent(new SupportBean_S2(i, Convert.ToString(index + 1)));
                epService.EPRuntime.SendEvent(new SupportBean_S3(i, Convert.ToString(index + 2)));
                Assert.AreEqual(Convert.ToString(index), listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    }
} // end of namespace
