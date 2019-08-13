///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for database joins.
    /// </summary>
    public class MultithreadStmtDatabaseJoin : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@Name('s0') select * \n" +
                "  from SupportBean#length(1000) as s0,\n" +
                "      sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myBigint'] as s1"
            );
            TrySendAndReceive(env, 4, env.Statement("s0"), 1000);
            TrySendAndReceive(env, 2, env.Statement("s0"), 2000);
            env.UndeployAll();
        }

        private static void TrySendAndReceive(
            RegressionEnvironment env,
            int numThreads,
            EPStatement statement,
            int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtDatabaseJoin)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtDatabaseJoinCallable(env.Runtime, statement, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace