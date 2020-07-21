///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class MultithreadStmtFilter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var plainFilter = "@name('s0') select count(*) as mycount from SupportBean";
            tryCount(env, 2, 1000, plainFilter, GeneratorEnumerator.DEFAULT_SUPPORTEBEAN_CB);
            tryCount(env, 4, 1000, plainFilter, GeneratorEnumerator.DEFAULT_SUPPORTEBEAN_CB);

            ICollection<string> vals = Arrays.AsList("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");

            GeneratorEnumeratorCallback enumCallback = numEvent => {
                var bean = new SupportCollection();
                bean.Strvals = vals;
                return bean;
            };

            var enumFilter =
                "@name('s0') select count(*) as mycount from SupportCollection(Strvals.anyOf(v -> v = 'j'))";
            tryCount(env, 4, 1000, enumFilter, enumCallback);
        }

        private void tryCount(
            RegressionEnvironment env,
            int numThreads,
            int numMessages,
            string epl,
            GeneratorEnumeratorCallback generatorEnumeratorCallback)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtFilter)).ThreadFactory);
            env.CompileDeploy(epl);
            var listener = new MTListener("mycount");
            env.Statement("s0").AddListener(listener);

            var future = new IFuture<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(
                    new SendEventCallable(
                        i,
                        env.Runtime,
                        new GeneratorEnumerator(numMessages, generatorEnumeratorCallback)));
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // verify results
            Assert.AreEqual(numMessages * numThreads, listener.Values.Count);
            var result = new SortedSet<int>();
            foreach (var row in listener.Values) {
                result.Add(row.AsInt32());
            }

            Assert.AreEqual(numMessages * numThreads, result.Count);
            Assert.AreEqual(1, result.First());
            Assert.AreEqual(numMessages * numThreads, result.Last());

            env.UndeployAll();
        }
    }
}