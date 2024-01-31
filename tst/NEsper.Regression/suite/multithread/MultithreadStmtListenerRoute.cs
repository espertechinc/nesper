///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for update listeners that route events.
    /// </summary>
    public class MultithreadStmtListenerRoute : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            TryListener(env, 4, 500);
        }

        private static void TryListener(
            RegressionEnvironment env,
            int numThreads,
            int numRoutes)
        {
            env.CompileDeploy("@name('trigger') select * from SupportBean");
            env.CompileDeploy("@name('s0') select * from SupportMarketDataBean");
            var listener = new SupportMTUpdateListener();
            env.Statement("s0").AddListener(listener);

            // Set of events routed by each listener
            ISet<SupportMarketDataBean> routed = new HashSet<SupportMarketDataBean>().AsSyncSet();

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtListenerRoute)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtListenerCreateStmtCallable(
                    i,
                    env.Runtime,
                    env.Statement("trigger"),
                    numRoutes,
                    routed);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // assert
            var results = listener.NewDataListFlattened;
            ClassicAssert.IsTrue(results.Length >= numThreads * numRoutes);

            foreach (var routedEvent in routed) {
                var found = false;
                for (var i = 0; i < results.Length; i++) {
                    if (results[i].Underlying == routedEvent) {
                        found = true;
                    }
                }

                ClassicAssert.IsTrue(found);
            }

            env.UndeployAll();
        }
    }
} // end of namespace