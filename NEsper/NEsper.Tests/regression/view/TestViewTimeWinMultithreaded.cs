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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
	/// <summary>
	/// Test for N threads feeding events that affect M statements which employ a small time window.
	/// Each of the M statements is associated with a Symbol and each event send hits exactly one
	/// statement only.
	/// <para />Thus the timer is fairly busy when active, competing with N application threads.
	/// Created for ESPER-59 Internal Threading Bugs Found.
	/// <para />Exceptions can occur in
	/// (1) an application thread during sendEvent() outside of the listener, causes the test to fail
	/// (2) an application thread during sendEvent() inside of the listener, causes assertion to fail
	/// (3) the timer thread, causes an exception to be logged and assertion *may* fail
	/// </summary>
    [TestFixture]
	public class TestViewTimeWinMultithreaded
    {
	    private Thread[] _threads;
	    private ResultUpdateListener[] _listeners;

        [TearDown]
	    public void TearDown()
        {
	        _listeners = null;
	    }

        [Test]
	    public void TestMultithreaded()
	    {
	        const int numSymbols = 1;
	        const int numThreads = 4;
	        const int numEventsPerThread = 50000;
	        const double timeWindowSize = 0.2;

	        // Set up threads, statements and listeners
	        SetUp(numSymbols, numThreads, numEventsPerThread, timeWindowSize);

	        // Start threads
            long startTime = DateTimeHelper.CurrentTimeMillis;
	        foreach (var thread in _threads) {
	            thread.Start();
	        }

	        // Wait for completion
	        foreach (var thread in _threads) {
	            thread.Join();
	        }
            long endTime = DateTimeHelper.CurrentTimeMillis;

	        // Check listener results
	        long totalReceived = 0;
	        foreach (var listener in _listeners) {
	            totalReceived += listener.NumReceived;
	            Assert.IsFalse(listener.IsCaughtRuntimeException);
	        }
	        var numTimeWindowAdvancements = (endTime - startTime) / 1000 / timeWindowSize;

	        log.Info("Completed, expected=" + numEventsPerThread * numThreads +
	            " numTimeWindowAdvancements=" + numTimeWindowAdvancements +
	            " totalReceived=" + totalReceived);
	        Assert.IsTrue(totalReceived < numEventsPerThread * numThreads + numTimeWindowAdvancements + 1);
	        Assert.IsTrue(totalReceived >= numEventsPerThread * numThreads);
	    }

	    private void SetUp(int numSymbols, int numThreads, int numEvents, double timeWindowSize)
	    {
	        var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        epService.Initialize();

	        // Create a statement for N number of Symbols, each it's own listener
	        var symbols = new string[numSymbols];
	        _listeners = new ResultUpdateListener[symbols.Length];
	        for (var i = 0; i < symbols.Length; i++)
	        {
	            symbols[i] = "S" + i;
	            var viewExpr = "select Symbol, sum(Volume) as sumVol " +
	                              "from " + typeof(SupportMarketDataBean).FullName +
	                              "(Symbol='" + symbols[i] + "').win:time(" + timeWindowSize + ")";
	            var testStmt = epService.EPAdministrator.CreateEPL(viewExpr);
	            _listeners[i] = new ResultUpdateListener();
	            testStmt.Events += _listeners[i].Update;
	        }

	        // Create threads to send events
	        _threads = new Thread[numThreads];
	        var runnables = new TimeWinRunnable[_threads.Length];
	        var rlock = LockManager.CreateDefaultLock();
	        for (var i = 0; i < _threads.Length; i++)
	        {
	            runnables[i] = new TimeWinRunnable(i, epService.EPRuntime, rlock, symbols, numEvents);
	            _threads[i] = new Thread(runnables[i].Run);
	        }
	    }

	    public class TimeWinRunnable
	    {
	        private readonly int _threadNum;
	        private readonly EPRuntime _epRuntime;
	        private readonly ILockable _sharedLock;
	        private readonly string[] _symbols;
	        private readonly int _numberOfEvents;

            public TimeWinRunnable(int threadNum, EPRuntime epRuntime, ILockable sharedLock, string[] symbols, int numberOfEvents)
            {
	            _threadNum = threadNum;
	            _epRuntime = epRuntime;
	            _sharedLock = sharedLock;
	            _symbols = symbols;
	            _numberOfEvents = numberOfEvents;
	        }

	        public void Run() {

	            for (var i = 0; i < _numberOfEvents; i++)
	            {
	                var symbolNum = (_threadNum + _numberOfEvents) % _symbols.Length;
	                var symbol = _symbols[symbolNum];
	                const long volume = 1;

	                object theEvent = new SupportMarketDataBean(symbol, -1, volume, null);

	                using(_sharedLock.Acquire())
	                {
	                    _epRuntime.SendEvent(theEvent);
	                }
	            }
	        }
	    }

	    public class ResultUpdateListener
	    {
	        private bool _isCaughtRuntimeException;
	        private int _numReceived = 0;
	        private string _lastSymbol = null;

	        public void Update(object sender, UpdateEventArgs args)
	        {
	            Update(args.NewEvents, args.OldEvents);
	        }

	        public void Update(EventBean[] newEvents, EventBean[] oldEvents)
            {
	            if ((newEvents == null) || (newEvents.Length == 0))
	            {
	                return;
	            }

	            try {
	                _numReceived += newEvents.Length;

	                var symbol = (string) newEvents[0].Get("Symbol");
	                if (_lastSymbol != null)
	                {
	                    Assert.AreEqual(_lastSymbol, symbol);
	                }
	                else
	                {
	                    _lastSymbol = symbol;
	                }
	            }
	            catch (Exception ex)
	            {
	                log.Error("Unexpected exception querying results", ex);
	                _isCaughtRuntimeException = true;
	                throw;
	            }
	        }

	        public int NumReceived
	        {
	            get { return _numReceived; }
	        }

	        public bool IsCaughtRuntimeException
	        {
	            get { return _isCaughtRuntimeException; }
	        }
	    }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
