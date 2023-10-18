///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for adding and removing listener.
    /// </summary>
    public class MultithreadStmtListenerAddRemove : RegressionExecutionWithConfigure
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.ListenerDispatchTimeout = long.MaxValue;
            configuration.Common.AddEventType(typeof(SupportMarketDataBean));
        }

        public void Run(RegressionEnvironment env)
        {
            var numThreads = 2;

            env.CompileDeploy("@name('s0') select * from pattern[every a=SupportMarketDataBean(Symbol='IBM')]");
            TryStatementListenerAddRemove(env, numThreads, env.Statement("s0"), false, 10000);
            env.UndeployModuleContaining("s0");

            env.CompileDeploy("@name('s0') select * from SupportMarketDataBean(Symbol='IBM', Feed='RT')");
            TryStatementListenerAddRemove(env, numThreads, env.Statement("s0"), true, 10000);
            env.UndeployModuleContaining("s0");
        }

        private static void TryStatementListenerAddRemove(
            RegressionEnvironment env,
            int numThreads,
            EPStatement statement,
            bool isEPL,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtListenerAddRemove)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtListenerAddRemoveCallable(env.Runtime, statement, isEPL, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace