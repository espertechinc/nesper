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
using com.espertech.esper.compat;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
	public class TestMTContextTerminated
    {
        [Test]
	    public void TestMTTerminateFault()
        {
            var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = true;
	        config.AddEventType(typeof(StartContextEvent));
	        config.AddEventType(typeof(PayloadEvent));
	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();

	        var eplStatement = "create context StartThenTwoSeconds start StartContextEvent end after 2 seconds";
	        epService.EPAdministrator.CreateEPL(eplStatement);

	        var aggStatement = "@Name('select') context StartThenTwoSeconds " +
	                "select Account, count(*) as totalCount " +
	                "from PayloadEvent " +
	                "group by Account " +
	                "output snapshot when terminated";
	        var epAggStatement = epService.EPAdministrator.CreateEPL(aggStatement);
	        epAggStatement.Events += (sender, args) => {
	                // no action, still listening to make sure select-clause evaluates
	        };

	        // start context
	        epService.EPRuntime.SendEvent(new StartContextEvent());

	        // start threads
	        IList<Thread> threads = new List<Thread>();
	        IList<MyRunnable> runnables = new List<MyRunnable>();
	        for (var i = 0; i < 8; i++) {
	            var myRunnable = new MyRunnable(epService);
	            runnables.Add(myRunnable);
	            var thread = new Thread(myRunnable.Run) { Name = "Thread" + i };
	            thread.Start();
	            threads.Add(thread);
	        }

	        // join
	        foreach (var thread in threads) {
	            thread.Join();
	        }

	        // assert
	        foreach (var runnable in runnables) {
	            Assert.IsNull(runnable.Exception);
	        }
	    }

	    public class StartContextEvent {}

	    public class PayloadEvent
        {
            public string Account { get; set; }

	        public PayloadEvent(string account) {
	            Account = account;
	        }
	    }

	    public class MyRunnable : IRunnable
        {
	        internal readonly EPServiceProvider Engine;
	        internal Exception Exception;

	        public MyRunnable(EPServiceProvider engine) {
	            Engine = engine;
	        }

	        public void Run() {
	            try {
	                for (var i = 0; i < 2000000; i++) {
	                    var payloadEvent = new PayloadEvent("A1");
	                    Engine.EPRuntime.SendEvent(payloadEvent);
	                }
	            }
	            catch (Exception ex) {
	                Exception = ex;
	            }
	        }
	    }
	}
} // end of namespace
