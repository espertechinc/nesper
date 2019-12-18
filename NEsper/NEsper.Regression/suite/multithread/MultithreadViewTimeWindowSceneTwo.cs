///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for N threads feeding events that affect M statements which employ a small time window.
    ///     Each of the M statements is associated with a symbol and each event send hits exactly one
    ///     statement only.
    ///     <para />
    ///     Thus the timer is fairly busy when active, competing with N application threads.
    ///     Created for ESPER-59 Internal Threading Bugs Found.
    ///     <para />
    ///     Exceptions can occur in
    ///     (1) an application thread during sendEvent() outside of the listener, causes the test to fail
    ///     (2) an application thread during sendEvent() inside of the listener, causes assertion to fail
    ///     (3) the timer thread, causes an exception to be logged and assertion *may* fail
    /// </summary>
    public class MultithreadViewTimeWindowSceneTwo : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ResultUpdateListener[] listeners;
        private Thread[] threads;

        public void Run(RegressionEnvironment env)
        {
            var numSymbols = 1;
            var numThreads = 4;
            var numEventsPerThread = 50000;
            var timeWindowSize = 0.2;

            // Set up threads, statements and listeners
            SetUp(env, numSymbols, numThreads, numEventsPerThread, timeWindowSize);

            // Start threads
            var startTime = PerformanceObserver.MilliTime;
            foreach (var thread in threads) {
                thread.Start();
            }

            // Wait for completion
            foreach (var thread in threads) {
                thread.Join();
            }

            var endTime = PerformanceObserver.MilliTime;

            // Check listener results
            long totalReceived = 0;
            foreach (var listener in listeners) {
                totalReceived += listener.NumReceived;
                Assert.IsFalse(listener.IsCaughtException);
            }

            var numTimeWindowAdvancements = (endTime - startTime) / 1000 / timeWindowSize;

            log.Info(
                "Completed, expected=" +
                numEventsPerThread * numThreads +
                " numTimeWindowAdvancements=" +
                numTimeWindowAdvancements +
                " totalReceived=" +
                totalReceived);
            Assert.That(totalReceived, Is.LessThan(numEventsPerThread * numThreads + numTimeWindowAdvancements + 1));
            Assert.That(totalReceived, Is.GreaterThanOrEqualTo(numEventsPerThread * numThreads));

            listeners = null;
            threads = null;

            env.UndeployAll();
        }

        private void SetUp(
            RegressionEnvironment env,
            int numSymbols,
            int numThreads,
            int numEvents,
            double timeWindowSize)
        {
            threads = new Thread[numThreads];
            listeners = new ResultUpdateListener[numSymbols];

            // Create a statement for N number of symbols, each it's own listener
            var symbols = new string[numSymbols];
            listeners = new ResultUpdateListener[symbols.Length];
            for (var i = 0; i < symbols.Length; i++) {
                var annotation = $"@Name('stmt_{i}')";
                symbols[i] = "S" + i;
                var epl = annotation +
                          "select Symbol, sum(Volume) as sumVol " + 
                          " from SupportMarketDataBean" +
                          "(Symbol='" + symbols[i] + "')" +
                          "#time(" + timeWindowSize + ")";
                env.CompileDeploy(epl);
                var testStmt = env.Statement("stmt_" + i);
                listeners[i] = new ResultUpdateListener();
                testStmt.AddListener(listeners[i]);
            }

            // Create threads to send events
            var runnables = new TimeWinRunnable[threads.Length];
            var @lock = new MonitorSpinLock();
            for (var i = 0; i < threads.Length; i++) {
                runnables[i] = new TimeWinRunnable(i, env, @lock, symbols, numEvents);
                threads[i] = new Thread(runnables[i].Run) {
                    Name = typeof(MultithreadViewTimeWindowSceneTwo).Name
                };
            }
        }

        public class TimeWinRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly int numberOfEvents;
            private readonly ILockable sharedLock;
            private readonly string[] symbols;
            private readonly int threadNum;

            public TimeWinRunnable(
                int threadNum,
                RegressionEnvironment env,
                ILockable sharedLock,
                string[] symbols,
                int numberOfEvents)
            {
                this.threadNum = threadNum;
                this.env = env;
                this.sharedLock = sharedLock;
                this.symbols = symbols;
                this.numberOfEvents = numberOfEvents;
            }

            public void Run()
            {
                try {
                    for (var i = 0; i < numberOfEvents; i++) {
                        var symbolNum = (threadNum + numberOfEvents) % symbols.Length;
                        var symbol = symbols[symbolNum];
                        long volume = 1;

                        object theEvent = new SupportMarketDataBean(symbol, -1, volume, null);

                        using (sharedLock.Acquire()) {
                            env.SendEventBean(theEvent);
                        }
                    }
                }
                catch (Exception e) {
                    while (e != null) {
                        Console.WriteLine($"Exception: {e.GetType().Name}");
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine("----------------------------------------");
                        e = e.InnerException;
                    }
                }
            }
        }

        public class ResultUpdateListener : UpdateListener
        {
            private string lastSymbol;

            public int NumReceived { get; private set; }

            public bool IsCaughtException { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var newEvents = eventArgs.NewEvents;
                if (newEvents == null || newEvents.Length == 0) {
                    return;
                }

                try {
                    NumReceived += newEvents.Length;

                    var symbol = (string) newEvents[0].Get("Symbol");
                    if (lastSymbol != null) {
                        Assert.AreEqual(lastSymbol, symbol);
                    }
                    else {
                        lastSymbol = symbol;
                    }
                }
                catch (Exception ex) {
                    log.Error("Unexpected exception querying results", ex);
                    IsCaughtException = true;
                    throw;
                }
            }
        }
    }
} // end of namespace