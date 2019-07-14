///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for update listeners that create and stop statements.
    /// </summary>
    public class MultithreadStmtListenerCreateStmt : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('s0') select * from SupportBean");
            TryListener(env, 2, 100, env.Statement("s0"));
            env.UndeployAll();
        }

        private static void TryListener(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats,
            EPStatement stmt)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtListenerCreateStmt)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtListenerRouteCallable(i, env, stmt, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace