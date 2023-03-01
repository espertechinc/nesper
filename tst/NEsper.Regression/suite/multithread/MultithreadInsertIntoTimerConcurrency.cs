///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadInsertIntoTimerConcurrency
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private EPRuntimeProvider _runtimeProvider;
        private EPEventService _runtime;
        private IExecutorService _executorService;

        private AtomicLong _idCounter;
        private NoActionUpdateListener _noActionUpdateListener;

        public void Run(Configuration configuration)
        {
            _idCounter = new AtomicLong(0);
            _executorService = Executors.NewCachedThreadPool();
            _noActionUpdateListener = new NoActionUpdateListener();

            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Runtime.Threading.InsertIntoDispatchLocking = Locking.SUSPEND;

            _runtimeProvider = new EPRuntimeProvider();
            
            var runtime = _runtimeProvider.GetRuntime(GetType().Name, configuration);
            runtime.Initialize();
            _runtime = runtime.EventService;

            var path = new RegressionPath();
            var epl = "insert into Stream1 select count(*) as cnt from SupportBean#time(7 sec)";
            var compiled = SupportCompileDeployUtil.Compile(epl, configuration, path);
            path.Add(compiled);
            SupportCompileDeployUtil.Deploy(compiled, runtime);

            epl += " output every 10 seconds";
            compiled = SupportCompileDeployUtil.Compile(epl, configuration, path);
            SupportCompileDeployUtil.DeployAddListener(compiled, "insert", _noActionUpdateListener, runtime);

            var sendTickEventRunnable = new SendEventRunnable(this, 10000);
            Start(sendTickEventRunnable, 4);

            // Adjust here for long-running test
            SupportCompileDeployUtil.ThreadSleep(3000);
            sendTickEventRunnable.Shutdown = true;

            _executorService.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(_executorService, 1, TimeUnit.SECONDS);
            runtime.Destroy();
        }

        private void Start<T>(
            ICallable<T> task,
            int numInstances)
        {
            for (var i = 0; i < numInstances; i++) {
                Start(task);
            }
        }

        private IFuture<T> Start<T>(ICallable<T> task)
        {
            var future = _executorService.Submit(task);
            return future;
        }

        public void SendEvent()
        {
            var id = _idCounter.GetAndIncrement();
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = id;
            _runtime.SendEventBean(theEvent, "SupportBean");
        }

        private class SendEventRunnable : ICallable<object>
        {
            private readonly int maxSent;
            private readonly MultithreadInsertIntoTimerConcurrency insertIntoTimer;
            private bool shutdown;

            public SendEventRunnable(
                MultithreadInsertIntoTimerConcurrency insertIntoTimer,
                int maxSent)
            {
                this.maxSent = maxSent;
                this.insertIntoTimer = insertIntoTimer;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public object Call()
            {
                var count = 0;
                while (true) {
                    insertIntoTimer.SendEvent();
                    SupportCompileDeployUtil.ThreadSleep(1);
                    count++;

                    if (count % 1000 == 0) {
                        log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " send " + count + " events");
                    }

                    if (count > maxSent) {
                        break;
                    }

                    if (shutdown) {
                        break;
                    }
                }

                return null;
            }
        }
    }
} // end of namespace