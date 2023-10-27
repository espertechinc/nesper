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

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of named windows and fire-and-forget queries.
    /// </summary>
    public class MultithreadStmtNamedWindowFAF : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public create window MyWindow#keepall as select TheString, LongPrimitive from SupportBean",
                path);
            env.CompileDeploy(
                "insert into MyWindow(TheString, LongPrimitive) select Symbol, Volume from SupportMarketDataBean",
                path);
            TryIterate(env, path, 2, 500);
            env.UndeployAll();
        }

        private static void TryIterate(
            RegressionEnvironment env,
            RegressionPath path,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowFAF)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowQueryCallable(env, path, numRepeats, Convert.ToString(i));
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            SupportCompileDeployUtil.ThreadSleep(100);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace