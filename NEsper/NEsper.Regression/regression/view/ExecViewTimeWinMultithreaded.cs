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
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    /// <summary>
    ///     Test for N threads feeding events that affect M statements which employ a small time window.
    ///     Each of the M statements is associated with a symbol and each event send hits exactly one
    ///     statement only.
    ///     <para>
    ///         Thus the timer is fairly busy when active, competing with N application threads.
    ///         Created for ESPER-59 Internal Threading Bugs Found.
    ///     </para>
    ///     <para>
    ///         Exceptions can occur in
    ///         (1) an application thread during SendEvent() outside of the listener, causes the test to fail
    ///         (2) an application thread during SendEvent() inside of the listener, causes assertion to fail
    ///         (3) the timer thread, causes an exception to be logged and assertion *may* fail
    ///     </para>
    /// </summary>
    public class ExecViewTimeWinMultithreaded : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ResultUpdateListener[] _listeners;

        private Thread[] _threads;

        public override void Run(EPServiceProvider epService)
        {
            var numSymbols = 1;
            var numThreads = 4;
            var numEventsPerThread = 50000;
            var timeWindowSize = 0.2;

            // Set up threads, statements and listeners
            SetUp(epService, numSymbols, numThreads, numEventsPerThread, timeWindowSize);

            // Start threads
            var startTime = DateTimeHelper.CurrentTimeMillis;
            foreach (var thread in _threads)
            {
                thread.Start();
            }

            // Wait for completion
            foreach (var thread in _threads)
            {
                thread.Join();
            }

            var endTime = DateTimeHelper.CurrentTimeMillis;

            // Check listener results
            long totalReceived = 0;
            foreach (var listener in _listeners)
            {
                totalReceived += listener.NumReceived;
                Assert.IsFalse(listener.IsCaughtRuntimeException);
            }

            var numTimeWindowAdvancements = (endTime - startTime) / 1000 / timeWindowSize;

            Log.Info(
                "Completed, expected=" + numEventsPerThread * numThreads +
                " numTimeWindowAdvancements=" + numTimeWindowAdvancements +
                " totalReceived=" + totalReceived);
            Assert.IsTrue(totalReceived < numEventsPerThread * numThreads + numTimeWindowAdvancements + 1);
            Assert.IsTrue(totalReceived >= numEventsPerThread * numThreads);

            _listeners = null;
            _threads = null;
        }

        private void SetUp(
            EPServiceProvider epService, int numSymbols, int numThreads, int numEvents, double timeWindowSize)
        {
            _threads = new Thread[numThreads];
            _listeners = new ResultUpdateListener[numSymbols];

            // Create a statement for N number of symbols, each it's own listener
            var symbols = new string[numSymbols];
            _listeners = new ResultUpdateListener[symbols.Length];
            for (var i = 0; i < symbols.Length; i++)
            {
                symbols[i] = "S" + i;
                var epl = "select symbol, sum(volume) as sumVol " +
                          "from " + typeof(SupportMarketDataBean).FullName +
                          "(symbol='" + symbols[i] + "')#time(" + timeWindowSize + ")";
                var testStmt = epService.EPAdministrator.CreateEPL(epl);
                _listeners[i] = new ResultUpdateListener();
                testStmt.Events += _listeners[i].Update;
            }

            // Create threads to send events
            var runnables = new TimeWinRunnable[_threads.Length];
            var rlock = SupportContainer.Instance.LockManager().CreateDefaultLock();
            for (var i = 0; i < _threads.Length; i++)
            {
                runnables[i] = new TimeWinRunnable(i, epService.EPRuntime, rlock, symbols, numEvents);
                _threads[i] = new Thread(runnables[i].Run);
            }
        }

        public class TimeWinRunnable
        {
            private readonly EPRuntime _epRuntime;
            private readonly int _numberOfEvents;
            private readonly ILockable _sharedLock;
            private readonly string[] _symbols;
            private readonly int _threadNum;

            public TimeWinRunnable(
                int threadNum, 
                EPRuntime epRuntime, 
                ILockable sharedLock, 
                string[] symbols, 
                int numberOfEvents)
            {
                _threadNum = threadNum;
                _epRuntime = epRuntime;
                _sharedLock = sharedLock;
                _symbols = symbols;
                _numberOfEvents = numberOfEvents;
            }

            public void Run()
            {
                for (var i = 0; i < _numberOfEvents; i++)
                {
                    var symbolNum = (_threadNum + _numberOfEvents) % _symbols.Length;
                    var symbol = _symbols[symbolNum];
                    long volume = 1;

                    var theEvent = new SupportMarketDataBean(symbol, -1, volume, null);

                    using (_sharedLock.Acquire())
                    {
                        _epRuntime.SendEvent(theEvent);
                    }
                }
            }
        }

        public class ResultUpdateListener
        {
            private string _lastSymbol;

            public int NumReceived { get; private set; }

            public bool IsCaughtRuntimeException { get; private set; }

            public void Update(object sender, UpdateEventArgs args)
            {
                Update(args.NewEvents, args.OldEvents);
            }

            public void Update(EventBean[] newEvents, EventBean[] oldEvents)
            {
                if (newEvents == null || newEvents.Length == 0)
                {
                    return;
                }

                try
                {
                    NumReceived += newEvents.Length;

                    var symbol = (string) newEvents[0].Get("symbol");
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
                    IsCaughtRuntimeException = true;
                    throw;
                }
            }
        }
    }
} // end of namespace