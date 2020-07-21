///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of context.
    /// </summary>
    public class MultithreadContextCountSimple : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@name('ctx') create context HashByUserCtx as coalesce by consistent_hash_crc32(P00) from SupportBean_S0 granularity 10000000",
                path);
            env.CompileDeploy("@name('select') context HashByUserCtx select P01 from SupportBean_S0", path);

            TrySendContextCountSimple(env, 4, 5);

            env.UndeployAll();
        }

        private static void TrySendContextCountSimple(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            var listener = new SupportMTUpdateListener();
            env.Statement("select").AddListener(listener);

            IList<object>[] eventsPerThread = new List<object>[numThreads];
            for (var t = 0; t < numThreads; t++) {
                eventsPerThread[t] = new List<object>();
                for (var i = 0; i < numRepeats; i++) {
                    eventsPerThread[t].Add(new SupportBean_S0(-1, "E" + i, i + "_" + t));
                }
            }

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadContextCountSimple)).ThreadFactory);
            var future = new IFuture<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, env.Runtime, eventsPerThread[i].GetEnumerator());
                future[i] = threadPool.Submit(callable);
            }

            SupportCompileDeployUtil.ThreadSleep(2000);
            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            var result = listener.GetNewDataListFlattened();
            ISet<string> received = new HashSet<string>();
            foreach (var @event in result) {
                var key = (string) @event.Get("P01");
                if (received.Contains(key)) {
                    Assert.Fail("key " + key + " received multiple times");
                }

                received.Add(key);
            }

            if (received.Count != numRepeats * numThreads) {
                log.Info("Received are " + received.Count + " entries");
                Assert.Fail();
            }
        }
    }
} // end of namespace