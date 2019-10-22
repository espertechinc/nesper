///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class MultithreadStmtNamedWindowIterate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = SetupStmts(env);
            TryIterate(env, path, 4, 250);
            env.UndeployAll();

            path = SetupStmts(env);
            TryIterate(env, path, 2, 500);
            env.UndeployAll();
        }

        private static RegressionPath SetupStmts(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "create window MyWindow#groupwin(TheString)#keepall as select TheString, LongPrimitive from SupportBean",
                path);
            env.CompileDeploy(
                "insert into MyWindow(TheString, LongPrimitive) select Symbol, Volume from SupportMarketDataBean",
                path);
            return path;
        }

        private static void TryIterate(
            RegressionEnvironment env,
            RegressionPath path,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowIterate)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowIterateCallable(Convert.ToString(i), env, path, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace