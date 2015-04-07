///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestThreadedConfigTimer 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        [Test]
        public void TestOp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            config.EngineDefaults.ExpressionConfig.IsUdfCache = false;
            config.EngineDefaults.ThreadingConfig.IsThreadPoolTimerExec = true;
            config.EngineDefaults.ThreadingConfig.ThreadPoolTimerExecNumThreads = 5;
            config.AddEventType("MyMap", new Dictionary<String, Object>());
            config.AddImport(typeof(SupportStaticMethodLib).FullName);
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            SendTimer(0, epService);
    
            Log.Debug("Creating statements");
            int countStatements = 100;
            SupportListenerTimerHRes listener = new SupportListenerTimerHRes();
            for (int i = 0; i < countStatements; i++)
            {
                EPStatement stmt = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(10) from pattern[every MyMap -> timer:interval(1)]");
                stmt.Events += listener.Update;
            }
            
            Log.Info("Sending trigger event");
            epService.EPRuntime.SendEvent(new Dictionary<String, Object>(), "MyMap");

            long delta = PerformanceObserver.TimeMillis(() => SendTimer(1000, epService));
            Assert.LessOrEqual(delta, 100, "Delta is " + delta);
            
            // wait for delivery
            while(true)
            {
                int countDelivered = listener.NewEvents.Count;
                if (countDelivered == countStatements)
                {
                    break;
                }
    
                Log.Info("Delivered " + countDelivered + ", waiting for more");
                Thread.Sleep(200);
            }
    
            Assert.AreEqual(100, listener.NewEvents.Count);
            // analyze result
            //List<Pair<Long, EventBean[]>> events = listener.NewEvents;
            //OccuranceResult result = OccuranceAnalyzer.Analyze(events, new long[] {100 * 1000 * 1000L, 10*1000 * 1000L});
            //Log.Info(result);
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
