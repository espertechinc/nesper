///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety and named window updates.
    /// </summary>
    public class MultithreadStmtNamedWindowUpdate : RegressionExecution
    {
        public const int NUM_STRINGS = 100;
        public const int NUM_INTS = 10;

        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 5, 10000);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numEventsPerThread)
        {
            // setup statements
            var path = new RegressionPath();
            env.CompileDeploy(
                "create window MyWindow#unique(TheString, IntPrimitive) as select * from SupportBean",
                path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean(BoolPrimitive = true)", path);
            env.CompileDeploy(
                "on SupportBean(BoolPrimitive = false) sb " +
                "update MyWindow win set IntBoxed = win.IntBoxed + 1, DoublePrimitive = win.DoublePrimitive + sb.DoublePrimitive" +
                " where sb.TheString = win.TheString and sb.IntPrimitive = win.IntPrimitive",
                path);

            // send primer events, initialize totals
            IDictionary<Pair<string, int>, UpdateTotals> totals = new Dictionary<Pair<string, int>, UpdateTotals>();
            for (var i = 0; i < NUM_STRINGS; i++) {
                for (var j = 0; j < NUM_INTS; j++) {
                    var primer = new SupportBean(Convert.ToString(i), j);
                    primer.BoolPrimitive = true;
                    primer.IntBoxed = 0;
                    primer.DoublePrimitive = 0;

                    env.SendEventBean(primer);
                    var key = new Pair<string, int>(primer.TheString, primer.IntPrimitive);
                    totals.Put(key, new UpdateTotals(0, 0));
                }
            }

            // execute
            var startTime = PerformanceObserver.MilliTime;
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowUpdate)).ThreadFactory);
            var future = new IFuture<StmtNamedWindowUpdateCallable.UpdateResult>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(
                    new StmtNamedWindowUpdateCallable("Thread" + i, env.Runtime, numEventsPerThread));
            }

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 10, TimeUnit.SECONDS);
            var endTime = PerformanceObserver.MilliTime;

            // total up result
            long deltaCumulative = 0;
            for (var i = 0; i < numThreads; i++) {
                StmtNamedWindowUpdateCallable.UpdateResult result = null;
                try {
                    result = future[i].GetValueOrDefault();
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                deltaCumulative += result.Delta;
                foreach (var item in result.Updates) {
                    var key = new Pair<string, int>(item.TheString, item.Intval);
                    var total = totals.Get(key);
                    if (total == null) {
                        throw new EPException("Totals not found for key " + key);
                    }

                    total.Num = total.Num + 1;
                    total.Sum = total.Sum + item.DoublePrimitive;
                }
            }

            // compare
            var rows = env.CompileExecuteFAF("select * from MyWindow", path).Array;
            Assert.AreEqual(rows.Length, totals.Count);
            long totalUpdates = 0;
            foreach (var row in rows) {
                var key = new Pair<string, int>((string) row.Get("TheString"), row.Get("IntPrimitive").AsInt32());
                var total = totals.Get(key);
                Assert.AreEqual(total.Num, row.Get("IntBoxed"));
                Assert.AreEqual(total.Sum, row.Get("DoublePrimitive"));
                totalUpdates += total.Num;
            }

            Assert.AreEqual(totalUpdates, numThreads * numEventsPerThread);
            //long deltaTime = endTime - startTime;
            //System.out.println("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);

            env.UndeployAll();
        }

        internal class UpdateTotals
        {
            internal UpdateTotals(
                int num,
                double sum)
            {
                Num = num;
                Sum = sum;
            }

            public int Num { get; set; }

            public double Sum { get; set; }
        }
    }
} // end of namespace