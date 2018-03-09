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
    public class ExecJoin3StreamCoercionPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerfCoercion3waySceneOne(epService);
            RunAssertionPerfCoercion3waySceneTwo(epService);
            RunAssertionPerfCoercion3waySceneThree(epService);
        }
    
        private void RunAssertionPerfCoercion3waySceneOne(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3" +
                    " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
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
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertionPerfCoercion3waySceneTwo(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3" +
                    " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, "A", i, 0, 0);
                SendEvent(epService, "B", 0, i, 0);
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                SendEvent(epService, "C", 0, 0, index);
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            stmt.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        private void RunAssertionPerfCoercion3waySceneThree(EPServiceProvider epService) {
            string stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C')#length(1000000) s3" +
                    " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, "A", i, 0, 0);
                SendEvent(epService, "C", 0, 0, i);
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                SendEvent(epService, "B", 0, index, 0);
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            stmt.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
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
