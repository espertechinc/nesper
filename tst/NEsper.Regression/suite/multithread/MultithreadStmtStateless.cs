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

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.wordexample;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadStmtStateless : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            TrySend(env, 4, 1000);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numRepeats)
        {
            env.CompileDeploy("@Name('s0') select * from SentenceEvent[Words]");
            AssertStatelessStmt(env, "s0", true);

            var runnables = new StatelessRunnable[numThreads];
            for (var i = 0; i < runnables.Length; i++) {
                runnables[i] = new StatelessRunnable(env.Runtime, numRepeats);
            }

            var threads = new Thread[numThreads];
            for (var i = 0; i < runnables.Length; i++) {
                threads[i] = new Thread(runnables[i].Run) {
                    Name = typeof(MultithreadStmtStateless).Name
                };
            }

            var start = PerformanceObserver.MilliTime;
            foreach (var t in threads) {
                t.Start();
            }

            foreach (var t in threads) {
                ThreadJoin(t);
            }

            var delta = PerformanceObserver.MilliTime - start;
            log.Info("Delta=" + delta + " for " + numThreads * numRepeats + " events");

            foreach (var r in runnables) {
                Assert.IsNull(r.Exception);
            }

            env.UndeployAll();
        }

        public class StatelessRunnable : IRunnable
        {
            private readonly int numRepeats;

            private readonly EPRuntime runtime;

            public StatelessRunnable(
                EPRuntime runtime,
                int numRepeats)
            {
                this.runtime = runtime;
                this.numRepeats = numRepeats;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try {
                    for (var i = 0; i < numRepeats; i++) {
                        runtime.EventService.SendEventBean(
                            new SentenceEvent("This is stateless statement testing"),
                            "SentenceEvent");

                        if (i % 10000 == 0) {
                            log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + i);
                        }
                    }
                }
                catch (Exception t) {
                    Exception = t;
                }
            }
        }
    }
} // end of namespace