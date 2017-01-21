///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
	/// <summary>
	/// Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected behavior
	/// </summary>
    [TestFixture]
	public class TestMTContextTemporalStartStop 
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	    }

        [Test]
	    public void TestMT()
	    {
	        TrySend();
	    }

	    private void TrySend()
	    {
	        _epService.EPAdministrator.CreateEPL("create context EverySecond as start (*, *, *, *, *, *) end (*, *, *, *, *, *)");
	        _epService.EPAdministrator.CreateEPL("context EverySecond select * from SupportBean");

	        var timerRunnable = new TimerRunnable(_epService, 0, 24*60*60*1000, 1000);
	        var timerThread = new Thread(timerRunnable.Run) { Name = "timer" };

	        var eventRunnable = new EventRunnable(_epService, 1000000);
	        var eventThread = new Thread(eventRunnable.Run) { Name = "event" };

	        timerThread.Start();
	        eventThread.Start();

	        timerThread.Join();
	        eventThread.Join();
	        Assert.IsNull(eventRunnable.Exception);
	        Assert.IsNull(timerRunnable.Exception);
	    }

	    public class TimerRunnable : IRunnable
        {
            internal readonly EPServiceProvider EpService;
	        internal readonly long Start;
            internal readonly long End;
            internal readonly long Increment;

            internal Exception Exception;

	        public TimerRunnable(EPServiceProvider epService, long start, long end, long increment) {
	            EpService = epService;
	            Start = start;
	            End = end;
	            Increment = increment;
	        }

	        public void Run() {
	            Log.Info("Started time drive");
	            try {
	                var current = Start;
	                long stepCount = 0;
	                var expectedSteps = (End - Start) / Increment;
	                while (current < End) {
	                    EpService.EPRuntime.SendEvent(new CurrentTimeEvent(current));
	                    current += Increment;
	                    stepCount++;

	                    if (stepCount % 10000 == 0) {
	                        Log.Info("Sending step #" + stepCount + " of " + expectedSteps);
	                    }
	                }
	            }
	            catch (Exception ex) {
	                Log.Error("Exception encountered: " + ex.Message, ex);
	                Exception = ex;
	            }
	            Log.Info("Completed time drive");
	        }
	    }

	    public class EventRunnable : IRunnable
        {
            internal readonly EPServiceProvider EpService;
            internal readonly long NumEvents;

	        internal Exception Exception;

	        public EventRunnable(EPServiceProvider epService, long numEvents) {
	            EpService = epService;
	            NumEvents = numEvents;
	        }

	        public void Run() {
	            Log.Info("Started event send");
	            try {
	                long count = 0;
	                while (count < NumEvents) {
	                    EpService.EPRuntime.SendEvent(new SupportBean());
	                    count++;

	                    if (count % 10000 == 0) {
	                        Log.Info("Sending event #" + count);
	                    }
	                }
	            }
	            catch (Exception ex) {
	                Log.Error("Exception encountered: " + ex.Message, ex);
	                Exception = ex;
	            }
	            Log.Info("Completed event send");
	        }
	    }
	}
} // end of namespace
