///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
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
    ///     Test for multithread-safety of context with database access.
    /// </summary>
    public class MultithreadContextDBAccess : RegressionExecutionWithConfigure
    {
        public void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Common.Logging.IsEnableADO = true;
            configuration.Runtime.Threading.IsListenerDispatchPreserveOrder = false;

            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDB.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configuration.Common.AddDatabaseReference("MyDB", configDB);
        }
        
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create context CtxEachString partition by TheString from SupportBean", path);
            env.CompileDeploy(
                "@name('select') context CtxEachString " +
                "select * from SupportBean, " +
                "  sql:MyDB ['select mycol3 from mytesttable_large where ${TheString} = mycol1']",
                path);

            // up to 10 threads, up to 1000 combinations (1 to 1000)
            TryThreadSafetyHistoricalJoin(env, 8, 20);

            env.UndeployAll();
        }

        public bool EnableHATest => true;

        public bool HAWithCOnly => true;


        private static void TryThreadSafetyHistoricalJoin(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            var listener = new MyListener();
            env.Statement("select").AddListener(listener);

            var events = new IList<object>[numThreads];
            for (var threadNum = 0; threadNum < numThreads; threadNum++) {
                events[threadNum] = new List<object>();
                for (var eventNum = 0; eventNum < numRepeats; eventNum++) {
                    // range: 1 to 1000
                    var partition = eventNum + 1;
                    events[threadNum].Add(new SupportBean(new int?(partition).ToString(), 0));
                }
            }

            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadContextDBAccess)).ThreadFactory);
            var futures = new IFuture<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, env.Runtime, events[i].GetEnumerator());
                futures[i] = threadPool.Submit(callable);
            }

            SupportCompileDeployUtil.AssertFutures(futures);
            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            Assert.AreEqual(numRepeats * numThreads, listener.Count);
        }

        public class MyListener : UpdateListener
        {
            public int Count { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var newEvents = eventArgs.NewEvents;
                lock (this) {
                    if (newEvents.Length > 1) {
                        Assert.AreEqual(1, newEvents.Length);
                    }

                    Count += 1;
                }
            }
        }
    }
} // end of namespace