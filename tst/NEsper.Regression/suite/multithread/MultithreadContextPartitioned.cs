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
using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
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
    ///     Test for multithread-safety of context with database access.
    /// </summary>
    public class MultithreadContextPartitioned : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create context CtxEachString partition by TheString from SupportBean", path);
            env.CompileDeploy("@name('select') context CtxEachString select * from SupportBean", path);

            TryPerformanceDispatch(env, 8, 100);

            env.UndeployAll();
        }

        private static void TryPerformanceDispatch(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            var listener = new MyListener();
            env.Statement("select").AddListener(listener);

            var random = new Random();
            var events = new IList<object>[numThreads];
            var eventId = 0;
            for (var threadNum = 0; threadNum < numThreads; threadNum++) {
                events[threadNum] = new List<object>();
                for (var eventNum = 0; eventNum < numRepeats; eventNum++) {
                    // range: 1 to 1000
                    var partition = random.Next(0, 50);
                    eventId++;
                    events[threadNum].Add(new SupportBean(new int?(partition).ToString(), eventId));
                }
            }

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadContextPartitioned)).ThreadFactory);
            var futures = new IFuture<bool>[numThreads];
            var startTime = PerformanceObserver.MilliTime;

            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, env.Runtime, events[i].GetEnumerator());
                futures[i] = threadPool.Submit(callable);
            }

            SupportCompileDeployUtil.AssertFutures(futures);
            var delta = PerformanceObserver.MilliTime - startTime;

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            // print those events not received
            foreach (var eventList in events) {
                foreach (var @event in eventList) {
                    if (!listener.Beans.Contains(@event)) {
                        Log.Info("Expected event was not received, event " + @event);
                    }
                }
            }

            ClassicAssert.AreEqual(numRepeats * numThreads, listener.Beans.Count);
            Assert.That(delta, Is.LessThan(500));
        }

        public class MyListener : UpdateListener
        {
            public IList<SupportBean> Beans { get; } = new List<SupportBean>();

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var newEvents = eventArgs.NewEvents;
                lock (this) {
                    if (newEvents.Length > 1) {
                        ClassicAssert.AreEqual(1, newEvents.Length);
                    }

                    Beans.Add((SupportBean)newEvents[0].Underlying);
                }
            }
        }
    }
} // end of namespace