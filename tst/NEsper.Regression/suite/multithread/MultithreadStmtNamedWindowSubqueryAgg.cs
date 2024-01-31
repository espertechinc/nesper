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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and named window subqueries and aggregation.
    /// </summary>
    public class MultithreadStmtNamedWindowSubqueryAgg : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 3, 1000, false);
            TrySend(env, 3, 1000, true);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numEventsPerThread,
            bool indexShare)
        {
            // setup statements
            var path = new RegressionPath();
            var schemas =
                "@public @buseventtype create schema UpdateEvent as (uekey string, ueint int);\n" +
                "@public @buseventtype create schema WindowSchema as (wskey string, wsint int);\n";
            env.CompileDeploy(schemas, path);

            var createEpl = "@public @Name('namedWindow') create window MyWindow#keepall as WindowSchema";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }

            env.CompileDeploy(createEpl, path);

            env.CompileDeploy("create index ABC on MyWindow(wskey)", path);
            env.CompileDeploy(
                "on UpdateEvent mue merge MyWindow mw " +
                "where uekey = wskey and ueint = wsint " +
                "when not matched then insert select uekey as wskey, ueint as wsint " +
                "when matched then delete",
                path);
            // note: here all threads use the same string key to insert/delete and different values for the int
            env.CompileDeploy(
                "@name('target') select (select intListAgg(wsint) from MyWindow mw where wskey = sb.TheString) as val from SupportBean sb",
                path);

            // execute
            var executor = Executors.NewMultiThreadedExecutor(numThreads);
            // new SupportThreadFactory(typeof(MultithreadStmtNamedWindowSubqueryAgg)).ThreadFactory)
            var futures = new List<IFuture<bool?>>();
            for (var i = 0; i < numThreads; i++) {
                futures.Add(
                    executor.Submit(
                        new StmtNamedWindowSubqueryAggCallable(
                            i,
                            env.Runtime,
                            numEventsPerThread,
                            env.Statement("target"))));
            }

            // Give the futures 10 seconds to complete the futures...
            futures.AsParallel().ForAll(future => future.Wait(TimeSpan.FromSeconds(10)));

            executor.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(executor, TimeSpan.FromSeconds(10));
            SupportCompileDeployUtil.AssertFutures(futures);

            var events = EPAssertionUtil.EnumeratorToArray(env.Statement("namedWindow").GetEnumerator());
            ClassicAssert.AreEqual(0, events.Length);

            env.UndeployAll();
        }
    }
} // end of namespace