///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety of statements that are very similar that is share the same filter and views. &lt;p&gt; The engine shares locks between statements that share filters and views. </summary>
    [TestFixture]
    public class TestMTStmtSharedView 
    {
        private static readonly IList<string> SYMBOLS = new[]{"IBM", "MSFT", "GE"};

        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.Threading.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtSharedView", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestSharedViews()
        {
            TrySend(4, 500, 100);
            //trySend(2, 1000, 100);
            //trySend(3, 2000, 20);
        }
    
        private void TrySend(int numThreads, int numRepeats, int numStatements)
        {
            // Create same statement X times
            var stmts = new EPStatement[numStatements];
            var listeners = new SupportMTUpdateListener[stmts.Length];
            for (int i = 0; i < stmts.Length; i++)
            {
                stmts[i] = _engine.EPAdministrator.CreateEPL(
                    " select * " +
                    " from " + typeof(SupportMarketDataBean).FullName + "#groupwin(Symbol)#uni(Price)");
                listeners[i] = new SupportMTUpdateListener();
                stmts[i].Events += listeners[i].Update;
            }
    
            // Start send threads
            // Each threads sends each symbol with price = 0 to numRepeats
            var startTime = PerformanceObserver.MilliTime;
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtSharedViewCallable(numRepeats, _engine, SYMBOLS);
                future[i] = threadPool.Submit(callable);
            }
    
            // Shut down
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.IsTrue(delta < 5000, "delta=" + delta + " not less then 5 sec");   // should take less then 5 seconds even for 100 statements as they need to share resources thread-safely
    
            // Assert results
            foreach (SupportMTUpdateListener listener in listeners)
            {
                Assert.AreEqual(numRepeats * numThreads * SYMBOLS.Count, listener.GetNewDataList().Count);
                EventBean[] newDataLast = listener.GetNewDataList()[listener.GetNewDataList().Count - 1];
                Assert.AreEqual(1, newDataLast.Length);
                EventBean result = newDataLast[0];
                Assert.AreEqual(numRepeats * numThreads, result.Get("datapoints").AsLong());
                Assert.IsTrue(SYMBOLS.Contains((string) result.Get("Symbol")));
                Assert.AreEqual(SumToN(numRepeats) * numThreads, result.Get("total"));
                listener.Reset();
            }
    
            foreach (var stmt in stmts)
            {
                stmt.Stop();
            }
        }
    
        private static double SumToN(int N)
        {
            double sum = 0;
            for (int i = 0; i < N; i++)
            {
                sum += i;
            }
            return sum;
        }
    }
}
