///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.multithread;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of a time window -based statement.
    /// </summary>
    public class MultithreadViewTimeWindow : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var numThreads = 2;
            var numEvents = 10000;
            var numStmt = 25;

            log.Info(
                "Processing " + numEvents + " events for " + numThreads + " threads and " + numStmt + " statements");
            var listeners = new SupportCountListener[numStmt];
            for (var i = 0; i < numStmt; i++) {
                listeners[i] = new SupportCountListener();
                var stmtName = "stmt" + i;
                var nameAnnotation = "@Name('" + stmtName + "')";
                var epl = nameAnnotation +
                          "select irstream intPrimitive, theString as key from SupportBean#time(1 sec)";
                env.CompileDeploy(epl).Statement(stmtName).AddListener(listeners[i]);
            }

            try {
                TrySend(env, numThreads, numEvents, numStmt, listeners);
            }
            catch (Exception t) {
                throw new EPException(t.Message, t);
            }

            env.UndeployAll();
        }

        private void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats,
            int numStmts,
            SupportCountListener[] listeners)
        {
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadViewTimeWindow)).ThreadFactory);
            var future = new IFuture<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, env.Runtime, new GeneratorEnumerator(numRepeats));
                future[i] = threadPool.Submit(callable);
            }

            log.Info("Waiting for threadpool shutdown");
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(30));

            for (var i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].Get());
            }

            // set time to a large value
            log.Info("Waiting for calm down");
            Thread.Sleep(5000);

            // Assert results
            var totalExpected = numThreads * numRepeats;

            // assert new data
            for (var i = 0; i < numStmts; i++) {
                var count = listeners[i].CountNew;
                Assert.AreEqual(count, totalExpected);
                var countOld = listeners[i].CountNew;
                Assert.AreEqual(countOld, totalExpected);
            }
        }
    }
} // end of namespace