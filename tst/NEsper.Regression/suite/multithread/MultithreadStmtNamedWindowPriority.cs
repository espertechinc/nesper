///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
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
    ///     Test for multithread-safety of @priority and named windows.
    /// </summary>
    public class MultithreadStmtNamedWindowPriority : RegressionExecutionWithConfigure
    {
        public bool EnableHATest => true;
        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.Common.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.Runtime.Execution.IsPrioritized = true;
            configuration.Runtime.Threading.IsInsertIntoDispatchPreserveOrder = false;
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@Name('window') create window MyWindow#keepall as (c0 string, c1 string)", path);
            env.CompileDeploy("insert into MyWindow select P00 as c0, P01 as c1 from SupportBean_S0", path);
            env.CompileDeploy(
                "@Priority(1) on SupportBean_S1 S1 merge MyWindow S0 where S1.P10 = c0 " +
                "when matched then update set c1 = S1.P11",
                path);
            env.CompileDeploy(
                "@Priority(0) on SupportBean_S1 S1 merge MyWindow S0 where S1.P10 = c0 " +
                "when matched then update set c1 = S1.P12",
                path);

            TrySend(env, env.Statement("window"), 4, 1000);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            EPStatement stmtWindow,
            int numThreads,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowPriority)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowPriorityCallable(i, env.Runtime, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            SupportCompileDeployUtil.AssertFutures(future);

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            var events = EPAssertionUtil.EnumeratorToArray(stmtWindow.GetEnumerator());
            Assert.AreEqual(numThreads * numRepeats, events.Length);
            for (var i = 0; i < events.Length; i++) {
                var valueC1 = (string) events[i].Get("c1");
                Assert.AreEqual("y", valueC1);
            }
        }
    }
} // end of namespace