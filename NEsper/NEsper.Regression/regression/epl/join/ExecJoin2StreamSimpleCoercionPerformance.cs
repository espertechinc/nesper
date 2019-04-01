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
    public class ExecJoin2StreamSimpleCoercionPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerformanceCoercionForward(epService);
            RunAssertionPerformanceCoercionBack(epService);
        }
    
        private void RunAssertionPerformanceCoercionForward(EPServiceProvider epService) {
            string stmt = "select A.LongBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) as A," +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) as B" +
                    " where A.LongBoxed=B.IntPrimitive";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(MakeSupportEvent("A", 0, i));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(MakeSupportEvent("B", index, 0));
                Assert.AreEqual((long) index, listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            statement.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        private void RunAssertionPerformanceCoercionBack(EPServiceProvider epService) {
            string stmt = "select A.IntPrimitive as value from " +
                    typeof(SupportBean).FullName + "(TheString='A')#length(1000000) as A," +
                    typeof(SupportBean).FullName + "(TheString='B')#length(1000000) as B" +
                    " where A.IntPrimitive=B.LongBoxed";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(MakeSupportEvent("A", i, 0));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++) {
                int index = 5000 + i % 1000;
                epService.EPRuntime.SendEvent(MakeSupportEvent("B", 0, index));
                Assert.AreEqual(index, listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            statement.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        private Object MakeSupportEvent(string theString, int intPrimitive, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.LongBoxed = longBoxed;
            return bean;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
