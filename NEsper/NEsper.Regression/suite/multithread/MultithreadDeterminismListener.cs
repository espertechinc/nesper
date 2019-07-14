///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    public class MultithreadDeterminismListener
    {
        public void Run(Configuration configuration)
        {
            TrySend(4, 10000, true, Locking.SUSPEND, configuration);
            TrySend(4, 10000, true, Locking.SPIN, configuration);
        }

        public void ManualTestOrderedDeliveryFail()
        {
            // Commented out as this is a manual test -- it should fail since the disable preserve order.
            // trySend(3, 1000, false, null, configuration);
        }

        private static void TrySend(
            int numThreads,
            int numEvents,
            bool isPreserveOrder,
            Locking locking,
            Configuration configuration)
        {
            configuration.Runtime.Threading.IsListenerDispatchPreserveOrder = isPreserveOrder;
            configuration.Runtime.Threading.ListenerDispatchLocking = locking;
            configuration.Common.AddEventType(typeof(SupportBean));

            var runtime = EPRuntimeProvider.GetRuntime(typeof(MultithreadDeterminismListener).Name, configuration);
            runtime.Initialize();

            // setup statements
            var deployed = SupportCompileDeployUtil.CompileDeploy(
                "@Name('s0') select count(*) as cnt from SupportBean",
                runtime,
                configuration);
            var listener = new SupportMTUpdateListener();
            deployed.Statements[0].AddListener(listener);

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadDeterminismListener)).ThreadFactory);
            var future = new IFuture<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new SendEventCallable(i, runtime, new GeneratorEnumerator(numEvents)));
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            var events = listener.GetNewDataListFlattened();
            var result = new long[events.Length];
            for (var i = 0; i < events.Length; i++) {
                result[i] = events[i].Get("cnt").AsLong();
            }
            //log.info(".trySend result=" + Arrays.toString(result));

            // assert result
            Assert.AreEqual(numEvents * numThreads, events.Length);
            for (var i = 0; i < numEvents * numThreads; i++) {
                Assert.AreEqual(result[i], (long) i + 1);
            }

            runtime.Destroy();
        }
    }
} // end of namespace