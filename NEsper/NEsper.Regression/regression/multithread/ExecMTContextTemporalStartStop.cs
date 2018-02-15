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
using com.espertech.esper.client.time;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected behavior
    /// </summary>
    public class ExecMTContextTemporalStartStop : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context EverySecond as start (*, *, *, *, *, *) end (*, *, *, *, *, *)");
            epService.EPAdministrator.CreateEPL("context EverySecond select * from SupportBean");
    
            var timerRunnable = new TimerRunnable(epService, 0, 24 * 60 * 60 * 1000, 1000);
            var timerThread = new Thread(timerRunnable.Run);
            timerThread.Name = "timer";

            var eventRunnable = new EventRunnable(epService, 1000000);
            var eventThread = new Thread(eventRunnable.Run);
            eventThread.Name = "event";
    
            timerThread.Start();
            eventThread.Start();
    
            timerThread.Join();
            eventThread.Join();
            Assert.IsNull(eventRunnable.Exception);
            Assert.IsNull(timerRunnable.Exception);
        }
    
        public class TimerRunnable
        {
            private readonly EPServiceProvider epService;
            private readonly long start;
            private readonly long end;
            private readonly long increment;
    
            private Exception exception;

            public long Start => start;

            public long End => end;

            public long Increment => increment;

            public Exception Exception => exception;

            public TimerRunnable(EPServiceProvider epService, long start, long end, long increment) {
                this.epService = epService;
                this.start = start;
                this.end = end;
                this.increment = increment;
            }
    
            public void Run() {
                Log.Info("Started time drive");
                try {
                    long current = start;
                    long stepCount = 0;
                    long expectedSteps = (end - start) / increment;
                    while (current < end) {
                        epService.EPRuntime.SendEvent(new CurrentTimeEvent(current));
                        current += increment;
                        stepCount++;
    
                        if (stepCount % 10000 == 0) {
                            Log.Info("Sending step #" + stepCount + " of " + expectedSteps);
                        }
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }
                Log.Info("Completed time drive");
            }
        }
    
        public class EventRunnable
        {
            private readonly EPServiceProvider epService;
            private readonly long numEvents;
    
            private Exception exception;

            public long NumEvents => numEvents;

            public Exception Exception => exception;

            public EventRunnable(EPServiceProvider epService, long numEvents) {
                this.epService = epService;
                this.numEvents = numEvents;
            }
    
            public void Run() {
                Log.Info("Started event send");
                try {
                    long count = 0;
                    while (count < numEvents) {
                        epService.EPRuntime.SendEvent(new SupportBean());
                        count++;
    
                        if (count % 10000 == 0) {
                            Log.Info("Sending event #" + count);
                        }
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }
                Log.Info("Completed event send");
            }
        }
    
    }
} // end of namespace
