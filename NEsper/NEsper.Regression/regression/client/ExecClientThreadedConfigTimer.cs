///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientThreadedConfigTimer : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = false;
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.EngineDefaults.Threading.IsThreadPoolTimerExec = true;
            configuration.EngineDefaults.Threading.ThreadPoolTimerExecNumThreads = 5;
            configuration.AddEventType("MyMap", new Dictionary<string, object>());
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientThreadedConfigTimer))) {
                return;
            }
            SendTimer(0, epService);
    
            Log.Debug("Creating statements");
            int countStatements = 100;
            var listener = new SupportListenerTimerHRes();
            for (int i = 0; i < countStatements; i++) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(10) from pattern[every MyMap -> timer:interval(1)]");
                stmt.Events += listener.Update;
            }
    
            Log.Info("Sending trigger event");
            epService.EPRuntime.SendEvent(new Dictionary<string, Object>(), "MyMap");
    
            long start = PerformanceObserver.NanoTime;
            SendTimer(1000, epService);
            long end = PerformanceObserver.NanoTime;
            long delta = (end - start) / 1000000;
            Assert.IsTrue(delta < 100, "Delta is " + delta);
    
            // wait for delivery
            while (true) {
                int countDelivered = listener.NewEvents.Count;
                if (countDelivered == countStatements) {
                    break;
                }
    
                Log.Info("Delivered " + countDelivered + ", waiting for more");
                Thread.Sleep(200);
            }
    
            Assert.AreEqual(100, listener.NewEvents.Count);
            // analyze result
            //List<Pair<long, EventBean[]>> events = listener.NewEvents;
            //OccuranceResult result = OccuranceAnalyzer.Analyze(events, new long[] {100 * 1000 * 1000L, 10*1000 * 1000L});
            //Log.Info(result);
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
