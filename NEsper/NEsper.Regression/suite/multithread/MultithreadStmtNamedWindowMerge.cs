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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and named window updates.
    /// </summary>
    public class MultithreadStmtNamedWindowMerge : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 3, 100);
            TrySend(env, 2, 1000);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numEventsPerThread)
        {
            // setup statements
            var path = new RegressionPath();
            env.CompileDeploy("create window MyWindow#keepall as select * from SupportBean", path);
            env.CompileDeploy(
                "on SupportBean sb " +
                "merge MyWindow nw where nw.TheString = sb.TheString " +
                " when not matched then insert select * " +
                " when matched then update set IntPrimitive = nw.IntPrimitive + 1",
                path);

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowMerge)).ThreadFactory);
            var future = new IFuture<bool?>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new StmtNamedWindowMergeCallable(env.Runtime, numEventsPerThread));
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // compare
            var rows = env.CompileExecuteFAF("select * from MyWindow", path).Array;
            Assert.AreEqual(numEventsPerThread, rows.Length);
            foreach (var row in rows) {
                Assert.AreEqual(numThreads - 1, row.Get("IntPrimitive"));
            }

            //long deltaTime = endTime - startTime;
            //System.out.println("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);
            env.UndeployAll();
        }
    }
} // end of namespace