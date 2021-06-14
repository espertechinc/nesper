///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class MultithreadStmtNamedWindowDelete : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('window') create window MyWindow#keepall as select TheString, LongPrimitive from SupportBean",
                path);
            var listenerWindow = new SupportMTUpdateListener();
            env.Statement("window").AddListener(listenerWindow);

            env.CompileDeploy(
                "insert into MyWindow(TheString, LongPrimitive) select Symbol, Volume from SupportMarketDataBean",
                path);
            env.CompileDeploy("on SupportBean_A as S0 delete from MyWindow as win where win.TheString = S0.Id", path);

            env.CompileDeploy("@Name('consumer') select irstream TheString, LongPrimitive from MyWindow", path);
            var listenerConsumer = new SupportMTUpdateListener();
            env.Statement("consumer").AddListener(listenerConsumer);

            TrySend(env, listenerWindow, listenerConsumer, 4, 1000);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            SupportMTUpdateListener listenerWindow,
            SupportMTUpdateListener listenerConsumer,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowDelete)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowDeleteCallable(Convert.ToString(i), env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            // compute list of expected
            IList<string> expectedIdsList = new List<string>();
            for (var i = 0; i < numThreads; i++) {
                try {
                    expectedIdsList.AddAll((IList<string>) future[i].Get());
                }
                catch (Exception t) {
                    throw new EPException(t);
                }
            }

            var expectedIds = expectedIdsList.ToArray();

            Assert.AreEqual(2 * numThreads * numRepeats, listenerWindow.NewDataList.Count); // old and new each
            Assert.AreEqual(2 * numThreads * numRepeats, listenerConsumer.NewDataList.Count); // old and new each

            // compute list of received
            var newEvents = listenerWindow.GetNewDataListFlattened();
            var receivedIds = new string[newEvents.Length];
            for (var i = 0; i < newEvents.Length; i++) {
                receivedIds[i] = (string) newEvents[i].Get("TheString");
            }

            Assert.AreEqual(receivedIds.Length, expectedIds.Length);

            Array.Sort(receivedIds);
            Array.Sort(expectedIds);
            CompatExtensions.DeepEqualsWithType(expectedIds, receivedIds);
        }
    }
} // end of namespace