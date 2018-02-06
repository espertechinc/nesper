///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    [TestFixture]
	public class TestMTContextSegmented 
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        _listener = null;
	    }

        [Test]
	    public void TestSegmentedContext()
	    {
	        var choices = "A,B,C,D".Split(',');
	        TrySend(4, 1000, choices);
	    }

	    private void TrySend(int numThreads, int numEvents, string[] choices)
	    {
	        if (numEvents < choices.Length) {
	            throw new ArgumentException("Number of events must at least match number of choices");
	        }

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.CreateEPL("create variable boolean myvar = false");
	        _epService.EPAdministrator.CreateEPL("create context SegmentedByString as partition by theString from SupportBean");
	        var stmt = _epService.EPAdministrator.CreateEPL("context SegmentedByString select theString, count(*) - 1 as cnt from SupportBean output snapshot when myvar = true");
	        stmt.AddListener(_listener);

	        // preload - since concurrently sending same-category events an event can be dropped
	        for (var i = 0; i < choices.Length; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean(choices[i], 0));
	        }

	        var runnables = new EventRunnable[numThreads];
	        for (var i = 0; i < runnables.Length; i++) {
	            runnables[i] = new EventRunnable(_epService, numEvents, choices);
	        }

	        // start
	        var threads = new Thread[runnables.Length];
	        for (var i = 0; i < runnables.Length; i++) {
	            threads[i] = new Thread(runnables[i].Run);
	            threads[i].Start();
	        }

	        // join
	        Log.Info("Waiting for completion");
	        for (var i = 0; i < runnables.Length; i++) {
	            threads[i].Join();
	        }

	        IDictionary<string, long?> totals = new Dictionary<string, long?>();
	        foreach (var choice in choices) {
	            totals.Put(choice, 0L);
	        }

	        // verify
	        var sum = 0;
	        for (var i = 0; i < runnables.Length; i++) {
	            Assert.IsNull(runnables[i].Exception);
	            foreach (var entry in runnables[i].Totals) {
	                var current = totals.Get(entry.Key);
	                current += entry.Value;
	                sum += entry.Value;
	                totals.Put(entry.Key, current);
	                //System.out.println("Thread " + i + " key " + entry.getKey() + " count " + entry.getValue());
	            }
	        }

	        Assert.AreEqual(numThreads * numEvents, sum);

	        _epService.EPRuntime.SetVariableValue("myvar", true);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
	        var result = _listener.LastNewData;
	        Assert.AreEqual(choices.Length, result.Length);
	        foreach (var item in result) {
	            var theString = (string) item.Get("theString");
	            var count = (long?) item.Get("cnt");
	            //System.out.println("String " + string + " count " + count);
	            Assert.AreEqual(count, totals.Get(theString));
	        }
	    }

	    public class EventRunnable : IRunnable
        {
	        private readonly EPServiceProvider _epService;
	        private readonly int _numEvents;
	        private readonly string[] _choices;

	        public EventRunnable(EPServiceProvider epService, int numEvents, string[] choices)
            {
	            Totals = new Dictionary<string, int>();
	            _epService = epService;
	            _numEvents = numEvents;
	            _choices = choices;
	        }

	        public void Run()
            {
	            Log.Info("Started event send");

	            try {
	                for (var i = 0; i < _numEvents; i++) {
	                    var chosen = _choices[i % _choices.Length];
	                    _epService.EPRuntime.SendEvent(new SupportBean(chosen, 1));

	                    var current = Totals.Get(chosen, 0) + 1;
	                    Totals.Put(chosen, current);
	                }
	            }
	            catch (Exception ex) {
	                Log.Error("Exception encountered: " + ex.Message, ex);
	                Exception = ex;
	            }

	            Log.Info("Completed event send");
	        }

	        public Exception Exception { get; private set; }

	        public IDictionary<string, int> Totals { get; private set; }
        }
	}
} // end of namespace
