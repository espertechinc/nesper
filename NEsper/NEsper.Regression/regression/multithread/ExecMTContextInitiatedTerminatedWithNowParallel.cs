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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextInitiatedTerminatedWithNowParallel : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create context MyCtx start @now end after 1 second");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyCtx select count(*) as cnt from SupportBean output last when terminated");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var latch = new AtomicBoolean(true);
            // With 0-sleep or 1-sleep the counts start to drop because the event is chasing the context partition.
            var t = new Thread(new MyTimeAdvancingRunnable(epService, latch, 10, -1).Run);
            t.Start();
    
            int numEvents = 10000;
            for (int i = 0; i < numEvents; i++) {
                epService.EPRuntime.SendEvent(new SupportBean());
            }
            latch.Set(false);
            t.Join();
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(int.MaxValue));
    
            long total = 0;
            EventBean[] deliveries = listener.GetNewDataListFlattened();
            foreach (EventBean @event in deliveries) {
                long count = (long) @event.Get("cnt");
                total += count;
            }
            Assert.AreEqual(numEvents, total);
        }
    
        public class MyTimeAdvancingRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly AtomicBoolean _latch;
            private readonly long _threadSleepTime;
            private readonly long _maxNumAdvances;
    
            public MyTimeAdvancingRunnable(EPServiceProvider epService, AtomicBoolean latch, long threadSleepTime, long maxNumAdvances) {
                _epService = epService;
                _latch = latch;
                _threadSleepTime = threadSleepTime;
                _maxNumAdvances = maxNumAdvances;
            }
    
            public void Run() {
                long time = 1000;
                long numAdvances = 0;
                try {
                    while (_latch.Get() && (_maxNumAdvances == -1 || numAdvances < _maxNumAdvances)) {
                        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
                        numAdvances++;
                        time += 1000;
                        try {
                            Thread.Sleep((int) _threadSleepTime);
                        } catch (ThreadInterruptedException) {
                        }
                    }
                } catch (Exception) {
                }
            }
        }
    }
} // end of namespace
