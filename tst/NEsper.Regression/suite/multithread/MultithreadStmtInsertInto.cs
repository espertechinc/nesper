///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class MultithreadStmtInsertInto : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public insert into XStream " +
                " select TheString as key, count(*) as mycount\n" +
                " from SupportBean#time(5 min)" +
                " group by TheString",
                path);
            env.CompileDeploy(
                "@public insert into XStream " +
                " select Symbol as key, count(*) as mycount\n" +
                " from SupportMarketDataBean#time(5 min)" +
                " group by Symbol",
                path);

            env.CompileDeploy("@name('s0') select key, mycount from XStream", path);
            var listener = new SupportMTUpdateListener();
            env.Statement("s0").AddListener(listener);

            TrySend(env, listener, 10, 5000);
            TrySend(env, listener, 4, 10000);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            SupportMTUpdateListener listener,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtInsertInto)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtInsertIntoCallable(Convert.ToString(i), env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // Assert results
            var totalExpected = numThreads * numRepeats * 2;
            var result = listener.NewDataListFlattened;
            ClassicAssert.AreEqual(totalExpected, result.Length);
            IDictionary<long, ICollection<string>> results = new Dictionary<long, ICollection<string>>();
            foreach (var theEvent in result) {
                var count = theEvent.Get("mycount").AsInt64();
                var key = (string)theEvent.Get("key");

                var entries = results.Get(count);
                if (entries == null) {
                    entries = new HashSet<string>();
                    results.Put(count, entries);
                }

                entries.Add(key);
            }

            ClassicAssert.AreEqual(numRepeats, results.Count);
            foreach (var value in results.Values) {
                ClassicAssert.AreEqual(2 * numThreads, value.Count);
                for (var i = 0; i < numThreads; i++) {
                    ClassicAssert.IsTrue(value.Contains("E1_" + i));
                    ClassicAssert.IsTrue(value.Contains("E2_" + i));
                }
            }

            listener.Reset();
        }
    }
} // end of namespace