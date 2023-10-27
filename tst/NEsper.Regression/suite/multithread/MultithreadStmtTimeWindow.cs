///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of a time window -based statement.
    /// </summary>
    public class MultithreadStmtTimeWindow : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 10, 5000);
            TrySend(env, 6, 2000);
            TrySend(env, 2, 10000);
            TrySend(env, 3, 5000);
            TrySend(env, 5, 2500);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            // set time to 0
            env.AdvanceTime(0);

            var listener = new SupportMTUpdateListener();
            env.CompileDeploy(
                "@name('s0') select irstream IntPrimitive, TheString as key from SupportBean#time(1 sec)");
            env.Statement("s0").AddListener(listener);

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtTimeWindow)).ThreadFactory);
            var futures = new List<IFuture<bool>>();
            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, env.Runtime, new GeneratorEnumerator(numRepeats));
                futures.Add(threadPool.Submit(callable));
            }

            // Advance time window every 100 milliseconds for 1 second
            for (var i = 0; i < 10; i++) {
                env.AdvanceTime(i * 1000);
                SupportCompileDeployUtil.ThreadSleep(100);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(futures);

            // set time to a large value
            env.AdvanceTime(10000000000L);

            // Assert results
            var totalExpected = numThreads * numRepeats;

            // assert new data
            var resultNewData = listener.NewDataListFlattened;
            Assert.AreEqual(totalExpected, resultNewData.Length);
            var resultsNewData = SortPerIntKey(resultNewData);
            AssertResult(numRepeats, numThreads, resultsNewData);

            // assert old data
            var resultOldData = listener.OldDataListFlattened;
            Assert.AreEqual(totalExpected, resultOldData.Length);
            var resultsOldData = SortPerIntKey(resultOldData);
            AssertResult(numRepeats, numThreads, resultsOldData);

            env.UndeployAll();
        }

        private static IDictionary<int, IList<string>> SortPerIntKey(EventBean[] result)
        {
            IDictionary<int, IList<string>> results = new Dictionary<int, IList<string>>();
            foreach (var theEvent in result) {
                var count = theEvent.Get("IntPrimitive").AsInt32();
                var key = (string)theEvent.Get("key");

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
        private static void AssertResult(
            int numRepeats,
            int numThreads,
            IDictionary<int, IList<string>> results)
        {
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