///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    [TestFixture]
	public class TestMTContextInitatedTerminatedWithNowParallel
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        _listener = null;
	    }

        [Test]
	    public void TestStartNowCountReliably()
        {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("create context MyCtx start @now end after 1 second");
	        var stmt = _epService.EPAdministrator.CreateEPL("context MyCtx select count(*) as cnt from SupportBean output last when terminated");
	        stmt.AddListener(_listener);

	        var latch = new AtomicBoolean(true);
	        // With 0-sleep or 1-sleep the counts start to drop because the event is chasing the context partition.
            var runnable = new MyTimeAdvancingRunnable(_epService, latch, 10, -1);
	        var t = new Thread(runnable.Run);
	        t.Start();

	        var numEvents = 10000;
	        for (var i = 0; i < numEvents; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean());
	        }
	        latch.Set(false);
	        t.Join();
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(int.MaxValue));

            var deliveries = _listener.GetNewDataListFlattened();
            var total = deliveries.Sum(@event => @event.Get("cnt").AsLong());
            Assert.AreEqual(numEvents, total);
	    }

	    public class MyTimeAdvancingRunnable
        {
	        private readonly EPServiceProvider _epService;
	        private readonly AtomicBoolean _latch;
	        private readonly int _threadSleepTime;
	        private readonly long _maxNumAdvances;

            public MyTimeAdvancingRunnable(EPServiceProvider epService, AtomicBoolean latch, int threadSleepTime, long maxNumAdvances)
            {
	            _epService = epService;
	            _latch = latch;
	            _threadSleepTime = threadSleepTime;
	            _maxNumAdvances = maxNumAdvances;
	        }

	        public void Run() {
	            long time = 1000;
	            long numAdvances = 0;

	            while(_latch.Get() && (_maxNumAdvances == -1 || numAdvances < _maxNumAdvances)) {
	                _epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
	                numAdvances++;
	                time += 1000;
	                Thread.Sleep(_threadSleepTime);
	            }
	        }
	    }
	}
} // end of namespace
