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
    ///     Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected
    ///     behavior
    /// </summary>
    public class MultithreadStmtIterate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionIteratorSingleStmt(env);
            RunAssertionIteratorMultiStmtNoViewShare(env);
        }

        private static void RunAssertionIteratorSingleStmt(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('s0') select TheString from SupportBean#time(5 min)");
            EPStatement[] stmt = {env.Statement("s0")};
            TrySend(env, 2, 10, stmt);
            env.UndeployAll();
        }

        private static void RunAssertionIteratorMultiStmtNoViewShare(RegressionEnvironment env)
        {
            var stmt = new EPStatement[3];
            for (var i = 0; i < stmt.Length; i++) {
                var name = "Stmt_" + i;
                var stmtText = "@Name('" + name + "') select TheString from SupportBean#time(5 min)";
                env.CompileDeploy(stmtText);
                stmt[i] = env.Statement(name);
            }

            TrySend(env, 4, 10, stmt);

            env.UndeployAll();
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats,
            EPStatement[] stmt)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtIterate)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtIterateCallable(i, env.Runtime, stmt, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 5, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);
        }
    }
} // end of namespace