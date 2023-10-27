///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    /// Test for multithread-safety of named windows and fire-and-forget queries:
    /// This test has a single inserting thread that produces unique id-numbers from 0 to N.
    /// The test has multiple delete-threads that each poll for a just-inserted number and issue a FAF-delete.
    /// </summary>
    public class MultithreadStmtNamedWindowFAFDelete : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        ISet<RegressionFlag> RegressionExecution.Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var numDeleteThreads = 4;
            var milliDuration = 2000;

            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyWindow#unique(Id) as SupportBean_S0", path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean_S0", path);

            Deque<int> ids = new ArrayDeque<int>(); //ConcurrentLinkedQueue<>();

            // insert into the named window producing new int-ids
            var insertRunnable = new InsertRunnable(env, _ => ids.Add(_));
            var insertThread = new Thread(insertRunnable.Run);
            insertThread.Start();

            // delete those that were inserted with FAF query
            var threadPool = Executors.NewFixedThreadPool(
                numDeleteThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowFAFDelete)).ThreadFactory);
            var future = new IFuture<bool>[numDeleteThreads];
            var callables = new DeleteCallable[numDeleteThreads];
            for (var i = 0; i < numDeleteThreads; i++) {
                callables[i] = new DeleteCallable(env, path, ids);
                future[i] = threadPool.Submit(callables[i]);
            }

            // wait a little
            try {
                Thread.Sleep(milliDuration);
            }
            catch (ThreadInterruptedException e) {
                throw new EPRuntimeException(e);
            }

            // shutdown insert
            insertRunnable.SetShutdown(true);
            try {
                insertThread.Join();
            }
            catch (ThreadInterruptedException e) {
                throw new EPRuntimeException(e);
            }

            Assert.IsNull(insertRunnable.Exception);

            // shutdown delete
            foreach (var deleteCallable in callables) {
                deleteCallable.SetShutdown(true);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeUnitHelper.ToTimeSpan(10, TimeUnit.SECONDS));

            ThreadSleep(100);
            AssertFutures(future);

            var countDeleted = 0;
            foreach (var deleteCallable in callables) {
                countDeleted += deleteCallable.NumDeletes;
            }

            Assert.IsTrue(insertRunnable.NumInserts > 1000);
            Assert.IsTrue(countDeleted > 100);

            env.UndeployAll();
        }

        public class DeleteCallable : ICallable<bool>
        {
            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly RegressionEnvironment env;

            private readonly RegressionPath path;

            //private readonly ConcurrentLinkedQueue<int> queue;
            private readonly Deque<int> queue;
            private int numDeletes;
            private bool shutdown;

            public DeleteCallable(
                RegressionEnvironment env,
                RegressionPath path,
                Deque<int> queue)
            {
                this.env = env;
                this.path = path;
                this.queue = queue;
            }

            public bool Call()
            {
                try {
                    var fafDelete = "delete from MyWindow where Id = ?::int";
                    var compiled = env.CompileFAF(fafDelete, path);
                    var queryDelete = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);

                    while (!shutdown) {
                        int? next = queue.Poll();
                        if (next == null) {
                            continue;
                        }

                        queryDelete.SetObject(1, next);
                        var queryResult = env.Runtime.FireAndForgetService.ExecuteQuery(queryDelete);
                        var numDeleted = queryResult.Array.Length;
                        Assert.AreEqual(1, numDeleted);
                        numDeletes++;
                    }
                }
                catch (Exception ex) {
                    Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                    return false;
                }

                return true;
            }

            public void SetShutdown(bool shutdown)
            {
                this.shutdown = shutdown;
            }

            public int NumDeletes => numDeletes;
        }

        private class InsertRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly Consumer<int> idConsumer;
            private Exception exception;
            private bool shutdown;
            private int numInserts;

            public InsertRunnable(
                RegressionEnvironment env,
                Consumer<int> idConsumer)
            {
                this.env = env;
                this.idConsumer = idConsumer;
            }

            public Exception Exception => exception;

            public int NumInserts => numInserts;

            public void Run()
            {
                try {
                    while (!shutdown) {
                        var id = numInserts++;
                        env.SendEventBean(new SupportBean_S0(id));
                        idConsumer.Invoke(id);
                    }
                }
                catch (Exception ex) {
                    this.exception = ex;
                }
            }

            public void SetShutdown(bool shutdown)
            {
                this.shutdown = shutdown;
            }
        }
    }
} // end of namespace