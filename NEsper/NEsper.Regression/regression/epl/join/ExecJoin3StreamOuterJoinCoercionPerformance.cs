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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoin3StreamOuterJoinCoercionPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerfCoercion3waySceneOne(epService);
            RunAssertionPerfCoercion3waySceneTwo(epService);
            RunAssertionPerfCoercion3waySceneThree(epService);
            RunAssertionPerfCoercion3wayRange(epService);
        }
    
        private void RunAssertionPerfCoercion3waySceneOne(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, "B", 0, i, 0);
                SendEvent(epService, "C", 0, 0, i);
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                SendEvent(epService, "A", index, 0, 0);
                EventBean theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long) index, theEvent.Get("v2"));
                Assert.AreEqual((double) index, theEvent.Get("v3"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerfCoercion3waySceneTwo(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, "B", 0, i, 0);
                SendEvent(epService, "A", i, 0, 0);
            }
    
            listener.Reset();
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                SendEvent(epService, "C", 0, 0, index);
                EventBean theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long) index, theEvent.Get("v2"));
                Assert.AreEqual((double) index, theEvent.Get("v3"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerfCoercion3waySceneThree(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, "A", i, 0, 0);
                SendEvent(epService, "C", 0, 0, i);
            }
    
            listener.Reset();
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                SendEvent(epService, "B", 0, index, 0);
                EventBean theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long) index, theEvent.Get("v2"));
                Assert.AreEqual((double) index, theEvent.Get("v3"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerfCoercion3wayRange(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            string stmtText = "select * from " +
                    "SupportBeanRange#keepall sbr " +
                    " left outer join " +
                    "SupportBean_ST0#keepall s0 on s0.key0=sbr.key" +
                    " left outer join " +
                    "SupportBean_ST1#keepall s1 on s1.key1=s0.key0" +
                    " where s0.p00 between sbr.rangeStartLong and sbr.rangeEndLong";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            Log.Info("Preload");
            for (int i = 0; i < 10; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_" + i, "K", i));
            }
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_" + i, "K", i));
            }
            Log.Info("Preload done");
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 100; i++) {
                long index = 5000 + i;
                epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "K", index, index + 2));
                Assert.AreEqual(30, listener.GetAndResetLastNewData().Length);
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0X", "K", 5000));
            Assert.AreEqual(10, listener.GetAndResetLastNewData().Length);
    
            epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1X", "K", 5004));
            Assert.AreEqual(301, listener.GetAndResetLastNewData().Length);
    
            Assert.IsTrue(delta < 500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intBoxed, long longBoxed, double doubleBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
