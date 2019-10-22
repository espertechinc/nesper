///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
    ///     Test for multithread-safety of a lookup statement.
    /// </summary>
    public class MultithreadStmtSubquery : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 4, 10000);
            TrySend(env, 3, 10000);
            TrySend(env, 2, 10000);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            env.CompileDeploy(
                "@Name('s0') select (select Id from SupportBean_S0#length(1000000) where Id = S1.Id) as value from SupportBean_S1 as S1");
            var listener = new SupportMTUpdateListener();
            env.Statement("s0").AddListener(listener);

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtSubquery)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtSubqueryCallable(i, env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // Assert results
            var totalExpected = numThreads * numRepeats;

            // assert new data
            var resultNewData = listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, resultNewData.Length);

            ISet<int> values = new HashSet<int>();
            foreach (var theEvent in resultNewData) {
                values.Add(theEvent.Get("value").AsInt());
            }

            Assert.AreEqual(totalExpected, values.Count, "Unexpected duplicates");

            listener.Reset();
            env.UndeployAll();
        }
    }
} // end of namespace