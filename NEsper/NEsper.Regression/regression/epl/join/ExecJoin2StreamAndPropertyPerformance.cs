///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoin2StreamAndPropertyPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerfRemoveStream(epService);
            RunAssertionPerf2Properties(epService);
            RunAssertionPerf3Properties(epService);
        }
    
        private void RunAssertionPerfRemoveStream(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myStaticEvaluator", typeof(MyStaticEval), "MyStaticEvaluator");
    
            MyStaticEval.CountCalled = 0;
            MyStaticEval.WaitTimeMSec = 0;
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            string epl = "select * from SupportBean#time(1) as sb, " +
                    " SupportBean_S0#keepall as s0 " +
                    " where MyStaticEvaluator(sb.TheString, s0.p00)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x"));
            Assert.AreEqual(0, MyStaticEval.CountCalled);
    
            epService.EPRuntime.SendEvent(new SupportBean("y", 10));
            Assert.AreEqual(1, MyStaticEval.CountCalled);
            Assert.IsTrue(updateListener.IsInvoked);
    
            // this would be observed as hanging if there was remove-stream evaluation
            MyStaticEval.WaitTimeMSec = 10000000;
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPerf2Properties(EPServiceProvider epService) {
            string methodName = ".testPerformanceJoinNoResults";
    
            string epl = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(1000000)," +
                    typeof(SupportBean).FullName + "#length(1000000)" +
                    " where symbol=TheString and volume=LongBoxed";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                SendEvent(epService, MakeMarketEvent("IBM_" + i, 1));
                SendEvent(epService, MakeSupportEvent("CSCO_" + i, 2));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay at 250, belwo 500ms
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }
    
        private void RunAssertionPerf3Properties(EPServiceProvider epService) {
            string methodName = ".testPerformanceJoinNoResults";
    
            string epl = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "()#length(1000000)," +
                    typeof(SupportBean).FullName + "#length(1000000)" +
                    " where symbol=TheString and volume=LongBoxed and DoublePrimitive=price";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                SendEvent(epService, MakeMarketEvent("IBM_" + i, 1));
                SendEvent(epService, MakeSupportEvent("CSCO_" + i, 2));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay at 250, belwo 500ms
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private Object MakeSupportEvent(string id, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = id;
            bean.LongBoxed = longBoxed;
            return bean;
        }
    
        private Object MakeMarketEvent(string id, long volume) {
            return new SupportMarketDataBean(id, 0, (long) volume, "");
        }

        public class MyStaticEval {
            private static int countCalled = 0;
            private static long waitTimeMSec;

            public static int CountCalled {
                get => countCalled;
                set => countCalled = value;
            }

            public static long WaitTimeMSec {
                get => waitTimeMSec;
                set => waitTimeMSec = value;
            }

            public static bool MyStaticEvaluator(string a, string b) {
                try {
                    Thread.Sleep((int) waitTimeMSec);
                    countCalled++;
                }
                catch (ThreadInterruptedException) {
                    return false;
                }

                return true;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
