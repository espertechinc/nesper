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
    public class ExecJoin2StreamSimplePerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerformanceJoinNoResults(epService);
            RunAssertionJoinPerformanceStreamA(epService);
            RunAssertionJoinPerformanceStreamB(epService);
        }
    
        private void RunAssertionPerformanceJoinNoResults(EPServiceProvider epService) {
            SetupStatement(epService);
            string methodName = ".testPerformanceJoinNoResults";
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                SendEvent(epService, MakeMarketEvent("IBM_" + i));
                SendEvent(epService, MakeSupportEvent("CSCO_" + i));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay below 50 ms
            Assert.IsTrue((endTime - startTime) < 500);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinPerformanceStreamA(EPServiceProvider epService) {
            SupportUpdateListener updateListener = SetupStatement(epService);
            string methodName = ".testJoinPerformanceStreamA";
    
            // Send 100k events
            Log.Info(methodName + " Preloading events");
            for (int i = 0; i < 50000; i++) {
                SendEvent(epService, MakeMarketEvent("IBM_" + i));
            }
            Log.Info(methodName + " Done preloading");
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            SendEvent(epService, MakeSupportEvent("IBM_10"));
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            // Stay below 50 ms
            Assert.IsTrue((endTime - startTime) < 50);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoinPerformanceStreamB(EPServiceProvider epService) {
            string methodName = ".testJoinPerformanceStreamB";
            SupportUpdateListener updateListener = SetupStatement(epService);
    
            // Send 100k events
            Log.Info(methodName + " Preloading events");
            for (int i = 0; i < 50000; i++) {
                SendEvent(epService, MakeSupportEvent("IBM_" + i));
            }
            Log.Info(methodName + " Done preloading");
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
    
            updateListener.Reset();
            SendEvent(epService, MakeMarketEvent("IBM_" + 10));
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            // Stay below 50 ms
            Assert.IsTrue((endTime - startTime) < 25);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private Object MakeSupportEvent(string id) {
            var bean = new SupportBean();
            bean.TheString = id;
            return bean;
        }
    
        private Object MakeMarketEvent(string id) {
            return new SupportMarketDataBean(id, 0, (long) 0, "");
        }
    
        private SupportUpdateListener SetupStatement(EPServiceProvider epService) {
            var updateListener = new SupportUpdateListener();
    
            string epl = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(1000000)," +
                    typeof(SupportBean).FullName + "#length(1000000)" +
                    " where symbol=TheString";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += updateListener.Update;
            return updateListener;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
