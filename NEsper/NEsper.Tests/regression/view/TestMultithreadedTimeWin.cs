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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    /// <summary>Test for Count threads feeding events that affect M statements which employ a small time window. Each of the M statements is associated with a symbol and each event send hits exactly one statement only. &lt;p&gt; Thus the timer is fairly busy when active, competing with Count application threads. Created for ESPER-59 Internal Threading Bugs Found. &lt;p&gt; Exceptions can occur in (1) an application thread during SendEvent() outside of the listener, causes the test to fail (2) an application thread during SendEvent() inside of the listener, causes assertion to fail (3) the timer thread, causes an exception to be logged and assertion *may* fail </summary>
    [TestFixture]
    public class TestMultithreadedTimeWin  {
    
        private List<Thread> _threads;
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
            double delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    _threads.ForEach(t => t.Start());
                    _threads.ForEach(t => t.Join());
                });
    
            // Check listener results
            long totalReceived = 0;
            foreach (ResultUpdateListener listener in _listeners) {
                totalReceived += listener.NumReceived;
                Assert.IsFalse(listener.IsCaughtRuntimeException);
            }
            double numTimeWindowAdvancements = delta / 1000 / timeWindowSize;
    
            Log.Info("Completed, expected=" + numEventsPerThread * numThreads +
                " numTimeWindowAdvancements=" + numTimeWindowAdvancements +
                " totalReceived=" + totalReceived);
            Assert.IsTrue(totalReceived < numEventsPerThread * numThreads + numTimeWindowAdvancements + 1);
            Assert.IsTrue(totalReceived >= numEventsPerThread * numThreads);
        }
    
        private void SetUp(int numSymbols, int numThreads, int numEvents, double timeWindowSize)
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
    
            // Create a statement for Count number of symbols, each it's own listener
            var symbols = new String[numSymbols];
            _listeners = new ResultUpdateListener[symbols.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                var index = i;
                symbols[i] = "S" + i;
                String viewExpr = "select Symbol, sum(Volume) as sumVol " +
                                  "from " + typeof(SupportMarketDataBean).FullName +
                                  "(Symbol='" + symbols[i] + "').win:time(" + timeWindowSize + ")";
                EPStatement testStmt = epService.EPAdministrator.CreateEPL(viewExpr);
                _listeners[i] = new ResultUpdateListener();
                testStmt.Events += (s, e) => _listeners[index].Update(e.NewEvents, e.OldEvents);
            }
    
            // Create threads to send events
            _threads = new List<Thread>();
            var @lock = LockManager.CreateDefaultLock();
            for (int i = 0; i < numThreads; i++)
            {
                var runnable = new TimeWinRunnable(i, epService.EPRuntime, @lock, symbols, numEvents);
                _threads.Add(new Thread(runnable.Run));
            }
        }
    
        public class TimeWinRunnable
        {
            private readonly int _threadNum;
            private readonly EPRuntime _epRuntime;
            private readonly ILockable _sharedLock;
            private readonly String[] _symbols;
            private readonly int _numberOfEvents;
    
            public TimeWinRunnable(int threadNum, EPRuntime epRuntime, ILockable sharedLock, String[] symbols, int numberOfEvents) {
                _threadNum = threadNum;
                _epRuntime = epRuntime;
                _sharedLock = sharedLock;
                _symbols = symbols;
                _numberOfEvents = numberOfEvents;
            }
    
            public void Run() {
    
                for (int i = 0; i < _numberOfEvents; i++)
                {
                    int symbolNum = (_threadNum + _numberOfEvents) % _symbols.Length;
                    String symbol = _symbols[symbolNum];
                    const long volume = 1;
    
                    Object theEvent = new SupportMarketDataBean(symbol, -1, volume, null);
    
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
            private int _numReceived;
            private String _lastSymbol;
    
            public void Update(EventBean[] newEvents, EventBean[] oldEvents) {
    
                if ((newEvents == null) || (newEvents.Length == 0))
                {
                    return;
                }
    
                try {
                    _numReceived += newEvents.Length;
    
                    var symbol = (String) newEvents[0].Get("Symbol");
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
                    Log.Error("Unexpected exception querying results", ex);
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
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
