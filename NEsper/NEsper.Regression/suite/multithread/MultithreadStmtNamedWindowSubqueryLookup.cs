///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and named window subqueries and direct index-based lookup.
    /// </summary>
    public class MultithreadStmtNamedWindowSubqueryLookup : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 3, 10000);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numEventsPerThread)
        {
            var path = new RegressionPath();
            var schemas = "create schema MyUpdateEvent as (key string, intupd int);\n" +
                          "create schema MySchema as (TheString string, intval int);\n";
            env.CompileDeployWBusPublicType(schemas, path);

            env.CompileDeploy("@Name('window') create window MyWindow#keepall as MySchema", path);
            env.CompileDeploy(
                "on MyUpdateEvent mue merge MyWindow mw " +
                "where mw.TheString = mue.key " +
                "when not matched then insert select key as TheString, intupd as intval " +
                "when matched then delete",
                path);
            env.CompileDeploy(
                "@Name('target') select (select intval from MyWindow mw where mw.TheString = sb.TheString) as val from SupportBean sb",
                path);

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowSubqueryLookup)).ThreadFactory);
            var future = new IFuture<bool?>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(
                    new StmtNamedWindowSubqueryLookupCallable(
                        i,
                        env.Runtime,
                        numEventsPerThread,
                        env.Statement("target")));
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("window"));
            Assert.AreEqual(0, events.Length);

            env.UndeployAll();
        }
    }
} // end of namespace