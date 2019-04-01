///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of statements that are very similar that is share the same filter and views.
    /// <para>
    /// The engine shares locks between statements that share filters and views.
    /// </para>
    /// </summary>
    public class ExecMTStmtSharedView : RegressionExecution {
        private static readonly string[] SYMBOLS = {"IBM", "MSFT", "GE"};
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = false;
            configuration.EngineDefaults.ViewResources.IsShareViews = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 4, 500, 100);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numRepeats, int numStatements) {
            // Create same statement X times
            var stmt = new EPStatement[numStatements];
            var listeners = new SupportMTUpdateListener[stmt.Length];
            for (int i = 0; i < stmt.Length; i++) {
                stmt[i] = epService.EPAdministrator.CreateEPL(
                        " select * " +
                                " from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#uni(price)");
                listeners[i] = new SupportMTUpdateListener();
                stmt[i].Events += listeners[i].Update;
            }

            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];

            // Start send threads
            // Each threads sends each symbol with price = 0 to numRepeats
            long delta = PerformanceObserver.TimeMillis(
                () => {
                    for (int i = 0; i < numThreads; i++) {
                        var callable = new StmtSharedViewCallable(numRepeats, epService, SYMBOLS);
                        future[i] = threadPool.Submit(callable);
                    }

                    // Shut down
                    threadPool.Shutdown();
                    threadPool.AwaitTermination(10, TimeUnit.SECONDS);
                });

            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
            
            Assert.That(delta, Is.LessThan(5000));   // should take less then 5 seconds even for 100 statements as they need to share resources thread-safely
    
            // Assert results
            foreach (SupportMTUpdateListener listener in listeners) {
                Assert.AreEqual(numRepeats * numThreads * SYMBOLS.Length, listener.NewDataList.Count);
                EventBean[] newDataLast = listener.NewDataList[listener.NewDataList.Count - 1];
                Assert.AreEqual(1, newDataLast.Length);
                var result = newDataLast[0];
                Assert.AreEqual(numRepeats * numThreads, result.Get("datapoints").AsLong());
                Assert.IsTrue(Collections.List(SYMBOLS).Contains(result.Get("symbol")));
                Assert.AreEqual(SumToN(numRepeats) * numThreads, result.Get("total"));
                listener.Reset();
            }
    
            for (int i = 0; i < stmt.Length; i++) {
                stmt[i].Stop();
            }
        }
    
        private double SumToN(int n) {
            double sum = 0;
            for (int i = 0; i < n; i++) {
                sum += i;
            }
            return sum;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
