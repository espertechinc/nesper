///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    public class MultithreadDeterminismInsertIntoLockConfig
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(Configuration configuration)
        {
            TrySendCountFollowedBy(4, 100, Locking.SUSPEND, configuration);
            TrySendCountFollowedBy(4, 100, Locking.SPIN, configuration);
        }

        private static void TrySendCountFollowedBy(
            int numThreads,
            int numEvents,
            Locking locking,
            Configuration configuration)
        {
            configuration.Runtime.Threading.InsertIntoDispatchLocking = locking;
            configuration.Runtime.Threading.InsertIntoDispatchTimeout = 5000; // 5 second timeout
            configuration.Common.AddEventType(typeof(SupportBean));

            // This should fail all test in this class
            // config.getEngineDefaults().getThreading().setInsertIntoDispatchPreserveOrder(false);
            var runtime = EPRuntimeProvider.GetRuntime(
                typeof(MultithreadDeterminismInsertIntoLockConfig).Name,
                configuration);
            runtime.Initialize();

            // setup statements
            var path = new RegressionPath();
            var eplInsert = "insert into MyStream select count(*) as cnt from SupportBean";
            var compiledInsert = Compile(eplInsert, configuration, path);
            path.Add(compiledInsert);
            var deployedInsert = Deploy(compiledInsert, runtime);
            deployedInsert.Statements[0].Events += (
                sender,
                updateEventArgs) => {
                log.Debug(".update cnt=" + updateEventArgs.NewEvents[0].Get("cnt"));
            };

            var listeners = new SupportUpdateListener[numEvents];
            for (var i = 0; i < numEvents; i++) {
                var text = "select * from pattern [MyStream(cnt=" + (i + 1) + ") => MyStream(cnt=" + (i + 2) + ")]";
                var compiled = Compile(text, configuration, path);
                var deployedPattern = Deploy(compiled, runtime);
                listeners[i] = new SupportUpdateListener();
                deployedPattern.Statements[0].AddListener(listeners[i]);
            }

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadDeterminismInsertIntoLockConfig)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            var sharedStartLock = new SlimReaderWriterLock();
            using (sharedStartLock.WriteLock.Acquire()) {
                for (var i = 0; i < numThreads; i++) {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(i, sharedStartLock, runtime, new GeneratorEnumerator(numEvents)));
                }

                ThreadSleep(100);
            }

            threadPool.Shutdown();
            ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            AssertFutures(future);

            // assert result
            for (var i = 0; i < numEvents - 1; i++) {
                Assert.AreEqual(1, listeners[i].NewDataList.Count, "Listener not invoked: #" + i);
            }

            runtime.Destroy();
        }
    }
} // end of namespace