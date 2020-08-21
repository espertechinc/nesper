///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and deterministic behavior when using insert-into and also listener-dispatch.
    /// </summary>
    public class MultithreadDeterminismInsertInto : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            TryChainedCountSum(env, 3, 100);
            TryMultiInsertGroup(env, 3, 10, 100);
        }

        private static void TryMultiInsertGroup(
            RegressionEnvironment env,
            int numThreads,
            int numStatements,
            int numEvents)
        {
            // This should fail all test in this class
            // config.getEngineDefaults().getThreading().setInsertIntoDispatchPreserveOrder(false);

            // setup statements
            var path = new RegressionPath();
            var insertIntoStmts = new EPStatement[numStatements];
            for (var i = 0; i < numStatements; i++) {
                var epl = $"@Name('s{i}') insert into MyStream select {i} as ident,count(*) as cnt from SupportBean";
                insertIntoStmts[i] = env.CompileDeploy(epl, path).Statement("s" + i);
            }

            env.CompileDeploy("@Name('final') select ident, sum(cnt) as mysum from MyStream group by ident", path);
            var listener = new SupportMTUpdateListener();
            env.Statement("final").AddListener(listener);

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadDeterminismInsertInto)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            var sharedStartLock = new SlimReaderWriterLock();
            using (sharedStartLock.WriteLock.Acquire()) {
                for (var i = 0; i < numThreads; i++) {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(
                            i,
                            sharedStartLock,
                            env.Runtime,
                            new GeneratorEnumerator(numEvents)));
                }

                SupportCompileDeployUtil.ThreadSleep(100);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);

            SupportCompileDeployUtil.AssertFutures(future);

            // assert result
            var newEvents = listener.GetNewDataListFlattened();
            IList<long>[] resultsPerIdent = new List<long>[numStatements];
            foreach (var theEvent in newEvents) {
                var ident = theEvent.Get("ident").AsInt32();
                if (resultsPerIdent[ident] == null) {
                    resultsPerIdent[ident] = new List<long>();
                }

                var mysum = theEvent.Get("mysum").AsInt64();
                resultsPerIdent[ident].Add(mysum);
            }

            for (var statement = 0; statement < numStatements; statement++) {
                for (var i = 0; i < numEvents - 1; i++) {
                    var expected = Total(i + 1);
                    Assert.AreEqual(
                        expected,
                        resultsPerIdent[statement][i],
                        "Failed for statement " + statement);
                }
            }

            env.UndeployAll();
        }

        private static void TryChainedCountSum(
            RegressionEnvironment env,
            int numThreads,
            int numEvents)
        {
            // setup statements
            var path = new RegressionPath();
            env.CompileDeploy("insert into MyStreamOne select count(*) as cnt from SupportBean", path);
            env.CompileDeploy("@Name('s0') insert into MyStreamTwo select sum(cnt) as mysum from MyStreamOne", path);
            var listener = new SupportUpdateListener();
            env.Statement("s0").AddListener(listener);

            // execute
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadDeterminismInsertInto)).ThreadFactory);
            var future = new IFuture<object>[numThreads];
            IReaderWriterLock sharedStartLock = new SlimReaderWriterLock();
            using (sharedStartLock.WriteLock.Acquire()) {
                for (var i = 0; i < numThreads; i++) {
                    future[i] = threadPool.Submit(
                        new SendEventRWLockCallable(
                            i,
                            sharedStartLock,
                            env.Runtime,
                            new GeneratorEnumerator(numEvents)));
                }

                SupportCompileDeployUtil.ThreadSleep(100);
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            SupportCompileDeployUtil.AssertFutures(future);

            // assert result
            var newEvents = listener.NewDataListFlattened;
            for (var i = 0; i < numEvents - 1; i++) {
                var expected = Total(i + 1);
                Assert.AreEqual(expected, newEvents[i].Get("mysum"));
            }

            env.UndeployAll();
        }

        private static long Total(int num)
        {
            long total = 0;
            for (var i = 1; i < num + 1; i++) {
                total += i;
            }

            return total;
        }
    }
} // end of namespace