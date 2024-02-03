///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class MultithreadStmtStatelessEnummethod : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            ICollection<string> vals = Arrays.AsList("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");

            GeneratorEnumeratorCallback enumCallback = numEvent => {
                var bean = new SupportCollection();
                bean.Strvals = vals;
                return bean;
            };

            var enumFilter = "@name('s0') select Strvals.anyOf(v -> v = 'j') from SupportCollection";
            TryCount(env, 4, 1000, enumFilter, enumCallback);
            env.UndeployAll();
        }

        private static void TryCount(
            RegressionEnvironment env,
            int numThreads,
            int numMessages,
            string epl,
            GeneratorEnumeratorCallback generatorEnumeratorCallback)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtStatelessEnummethod)).ThreadFactory);
            env.CompileDeploy(epl);
            var listener = new SupportMTUpdateListener();
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

            ClassicAssert.AreEqual(numMessages * numThreads, listener.NewDataListFlattened.Length);
        }
    }
}