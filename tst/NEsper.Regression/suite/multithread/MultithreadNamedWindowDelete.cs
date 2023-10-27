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
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadNamedWindowDelete : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@name('create') @public create window MyWindow#keepall() as select TheString, LongPrimitive from SupportBean",
                path);
            var listenerWindow = new SupportMTUpdateListener();
            env.Statement("create").AddListener(listenerWindow);

            env.CompileDeploy(
                "insert into MyWindow(TheString, LongPrimitive) select Symbol, Volume  from SupportMarketDataBean",
                path);

            var stmtTextDelete = "on SupportBean_A as S0 delete from MyWindow as win where win.TheString = S0.Id";
            env.CompileDeploy(stmtTextDelete, path);

            env.CompileDeploy("@name('s0') select irstream TheString, LongPrimitive from MyWindow", path);
            var listenerConsumer = new SupportMTUpdateListener();
            env.Statement("s0").AddListener(listenerConsumer);

            try {
                TrySend(4, 25000, listenerConsumer, listenerWindow, env);
            }
            catch (Exception ex) {
                throw new EPException("Failed: " + ex.Message, ex);
            }

            env.UndeployAll();
        }

        private static void TrySend(
            int numThreads,
            int numRepeats,
            SupportMTUpdateListener listenerConsumer,
            SupportMTUpdateListener listenerWindow,
            RegressionEnvironment env)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadNamedWindowDelete)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowDeleteCallable(Convert.ToString(i), env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeUnitHelper.ToTimeSpan(10, TimeUnit.SECONDS));

            // compute list of expected
            IList<string> expectedIdsList = new List<string>();
            for (var i = 0; i < numThreads; i++) {
                expectedIdsList.AddAll((IList<string>)future[i].Get());
            }

            var expectedIds = expectedIdsList.ToArray();

            Assert.AreEqual(2 * numThreads * numRepeats, listenerWindow.NewDataList.Count); // old and new each
            Assert.AreEqual(2 * numThreads * numRepeats, listenerConsumer.NewDataList.Count); // old and new each

            // compute list of received
            var newEvents = listenerWindow.NewDataListFlattened;
            var receivedIds = new string[newEvents.Length];
            for (var i = 0; i < newEvents.Length; i++) {
                receivedIds[i] = (string)newEvents[i].Get("TheString");
            }

            Assert.AreEqual(receivedIds.Length, expectedIds.Length);

            Array.Sort(receivedIds);
            Array.Sort(expectedIds);
            CompatExtensions.DeepEqualsWithType(expectedIds, receivedIds);
        }
    }
} // end of namespace