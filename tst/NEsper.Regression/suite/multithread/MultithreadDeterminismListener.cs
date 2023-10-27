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
using com.espertech.esper.regressionlib.framework;
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
    public class MultithreadDeterminismListener : RegressionExecutionPreConfigured
    {
        private readonly Configuration _configuration;

        public MultithreadDeterminismListener(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            var runtimeProvider = new EPRuntimeProvider();
            TrySend(runtimeProvider, 4, 10000, true, Locking.SUSPEND, _configuration);
            TrySend(runtimeProvider, 4, 10000, true, Locking.SPIN, _configuration);
        }

        public void ManualTestOrderedDeliveryFail()
        {
            // Commented out as this is a manual test -- it should fail since the disable preserve order.
            // trySend(3, 1000, false, null, configuration);
        }

        private static void TrySend(
            EPRuntimeProvider runtimeProvider,
            int numThreads,
            int numEvents,
            bool isPreserveOrder,
            Locking locking,
            Configuration configuration)
        {
            configuration.Runtime.Threading.IsListenerDispatchPreserveOrder = isPreserveOrder;
            configuration.Runtime.Threading.ListenerDispatchLocking = locking;
            configuration.Common.AddEventType(typeof(SupportBean));

            var runtime = runtimeProvider.GetRuntimeInstance(nameof(MultithreadDeterminismListener), configuration);
            runtime.Initialize();

            // setup statements
            var deployed = SupportCompileDeployUtil.CompileDeploy(
                "@name('s0') select count(*) as cnt from SupportBean",
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
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            var events = listener.NewDataListFlattened;
            var result = new long[events.Length];
            for (var i = 0; i < events.Length; i++) {
                result[i] = events[i].Get("cnt").AsInt64();
            }
            //log.info(".trySend result=" + Arrays.toString(result));

            // assert result
            Assert.AreEqual(numEvents * numThreads, events.Length);
            for (var i = 0; i < numEvents * numThreads; i++) {
                Assert.AreEqual(result[i], (long)i + 1);
            }

            runtime.Destroy();
        }
    }
} // end of namespace