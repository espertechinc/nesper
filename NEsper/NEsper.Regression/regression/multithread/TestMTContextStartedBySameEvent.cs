///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    [TestFixture]
	public class TestMTContextStartedBySameEvent
    {
        [Test]
	    public void TestMT()
        {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Threading.IsInternalTimerEnabled = true;
	        config.AddEventType(typeof(PayloadEvent));
	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();

	        var eplStatement = "create context MyContext start PayloadEvent end after 0.5 seconds";
	        epService.EPAdministrator.CreateEPL(eplStatement);

	        var aggStatement = "@Name('select') context MyContext " +
	                "select count(*) as theCount " +
	                "from PayloadEvent " +
	                "output snapshot when terminated";
	        var epAggStatement = epService.EPAdministrator.CreateEPL(aggStatement);
	        var listener = new MyListener();
            epAggStatement.Events += listener.Update;

	        // start thread
	        long numEvents = 10000000;
	        var myRunnable = new MyRunnable(epService, numEvents);
	        var thread = new Thread(myRunnable.Run);
	        thread.Start();
	        thread.Join();

	        Thread.Sleep(1000);

	        // assert
	        Assert.IsNull(myRunnable.Exception);
	        Assert.AreEqual(numEvents, listener.Total);
	    }

	    public class PayloadEvent
        {
	    }

	    public class MyRunnable : IRunnable
        {
	        internal readonly EPServiceProvider Engine;
            internal readonly long NumEvents;
	        internal Exception Exception;

	        public MyRunnable(EPServiceProvider engine, long numEvents)
            {
	            Engine = engine;
	            NumEvents = numEvents;
	        }

	        public void Run()
            {
	            try {
	                for (var i = 0; i < NumEvents; i++) {
	                    var payloadEvent = new PayloadEvent();
	                    Engine.EPRuntime.SendEvent(payloadEvent);
	                    if (i > 0 && i % 1000000 == 0) {
	                        Debug.WriteLine("sent " + i + " events");
	                    }
	                }
	                Debug.WriteLine("sent " + NumEvents + " events");
	            }
	            catch (Exception ex)
	            {
	                Debug.WriteLine(ex.StackTrace);
	                Exception = ex;
	            }
	        }
	    }

	    public class MyListener
        {
	        internal long Total;

            public void Update(object sender, UpdateEventArgs args)
            {
	            var theCount = args.NewEvents[0].Get("theCount").AsLong();
	            Total += theCount;
	            Debug.WriteLine("count " + theCount + " total " + Total);
	        }
	    }
	}
} // end of namespace
