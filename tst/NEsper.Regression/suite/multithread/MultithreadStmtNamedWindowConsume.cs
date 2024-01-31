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
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class MultithreadStmtNamedWindowConsume : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@name('window') @public create window MyWindow#keepall as select TheString, LongPrimitive from SupportBean",
                path);
            var listenerWindow = new SupportMTUpdateListener();
            env.Statement("window").AddListener(listenerWindow);

            env.CompileDeploy(
                "insert into MyWindow(TheString, LongPrimitive) " +
                " select Symbol, Volume \n" +
                " from SupportMarketDataBean",
                path);

            var stmtTextDelete = "on SupportBean_A as S0 delete from MyWindow as win where win.TheString = S0.Id";
            env.CompileDeploy(stmtTextDelete, path);

            TrySend(env, path, listenerWindow, 4, 1000, 8);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            RegressionPath path,
            SupportMTUpdateListener listenerWindow,
            int numThreads,
            int numRepeats,
            int numConsumers)
        {
            var listenerConsumers = new SupportMTUpdateListener[numConsumers];
            var compiled = env.Compile("select TheString, LongPrimitive from MyWindow", path);
            for (var i = 0; i < listenerConsumers.Length; i++) {
                var stmtName = "c" + i;
                try {
                    env.Deployment.Deploy(compiled, new DeploymentOptions().WithStatementNameRuntime(ctx => stmtName));
                }
                catch (EPDeployException e) {
                    throw new EPException(e);
                }

                var stmtConsumer = env.Statement(stmtName);
                listenerConsumers[i] = new SupportMTUpdateListener();
                stmtConsumer.AddListener(listenerConsumers[i]);
            }

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowConsume)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowConsumeCallable(Convert.ToString(i), env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            // compute list of expected
            IList<string> expectedIdsList = new List<string>();
            for (var i = 0; i < numThreads; i++) {
                try {
                    expectedIdsList.AddAll((IList<string>)future[i].Get());
                }
                catch (Exception t) {
                    throw new EPException(t);
                }
            }

            var expectedIds = expectedIdsList.ToArray();

            ClassicAssert.AreEqual(numThreads * numRepeats, listenerWindow.NewDataList.Count); // old and new each

            // compute list of received
            for (var i = 0; i < listenerConsumers.Length; i++) {
                var newEvents = listenerConsumers[i].NewDataListFlattened;
                var receivedIds = new string[newEvents.Length];
                for (var j = 0; j < newEvents.Length; j++) {
                    receivedIds[j] = (string)newEvents[j].Get("TheString");
                }

                ClassicAssert.AreEqual(receivedIds.Length, expectedIds.Length);

                Array.Sort(receivedIds);
                Array.Sort(expectedIds);
                CompatExtensions.DeepEqualsWithType(expectedIds, receivedIds);
            }
        }
    }
} // end of namespace