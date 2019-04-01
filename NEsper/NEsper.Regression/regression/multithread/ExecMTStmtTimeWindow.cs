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
using com.espertech.esper.client.time;
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
    /// Test for multithread-safety of a time window -based statement.
    /// </summary>
    public class ExecMTStmtTimeWindow : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var stmt = epService.EPAdministrator.CreateEPL(
                    " select irstream IntPrimitive, TheString as key " +
                            " from " + typeof(SupportBean).FullName + "#time(1 sec)");
    
            var listener = new SupportMTUpdateListener();
            stmt.Events += listener.Update;
    
            TrySend(epService, listener, 10, 5000);
            TrySend(epService, listener, 6, 2000);
            TrySend(epService, listener, 2, 10000);
            TrySend(epService, listener, 3, 5000);
            TrySend(epService, listener, 5, 2500);
        }
    
        private void TrySend(EPServiceProvider epService, SupportMTUpdateListener listener, int numThreads, int numRepeats) {
            // set time to 0
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, epService, new GeneratorIterator(numRepeats));
                future[i] = threadPool.Submit(callable);
            }
    
            // Advance time window every 100 milliseconds for 1 second
            for (var i = 0; i < 10; i++) {
                epService.EPRuntime.SendEvent(new CurrentTimeEvent(i * 1000));
                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (var i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // set time to a large value
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000000000L));
    
            // Assert results
            var totalExpected = numThreads * numRepeats;
    
            // assert new data
            var resultNewData = listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, resultNewData.Length);
            var resultsNewData = SortPerIntKey(resultNewData);
            AssertResult(numRepeats, numThreads, resultsNewData);
    
            // assert old data
            var resultOldData = listener.GetOldDataListFlattened();
            Assert.AreEqual(totalExpected, resultOldData.Length);
            var resultsOldData = SortPerIntKey(resultOldData);
            AssertResult(numRepeats, numThreads, resultsOldData);
    
            listener.Reset();
        }
    
        private IDictionary<int, IList<string>> SortPerIntKey(EventBean[] result) {
            var results = new LinkedHashMap<int, IList<string>>();
            foreach (var theEvent in result) {
                var count = theEvent.Get("IntPrimitive").AsInt();
                var key = (string) theEvent.Get("key");
    
                var entries = results.Get(count);
                if (entries == null) {
                    entries = new List<string>();
                    results.Put(count, entries);
                }
                entries.Add(key);
            }
            return results;
        }
    
        // Each integer value must be there with 2 entries of the same value
        private void AssertResult(int numRepeats, int numThreads, IDictionary<int, IList<string>> results) {
            for (var i = 0; i < numRepeats; i++) {
                var values = results.Get(i);
                Assert.AreEqual(numThreads, values.Count);
                foreach (var value in values) {
                    Assert.AreEqual(Convert.ToString(i), value);
                }
            }
        }
    }
} // end of namespace
