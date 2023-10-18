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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTUngroupedAccessReadMergeWrite : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }
        
        /// <summary>
        ///     For a given number of seconds:
        ///     Multiple writer threads each update their thread-id into a shared ungrouped row with plain props,
        ///     and a single reader thread reads the row and asserts that the values is the same for all cols.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 2, 3);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds,
            int numWriteThreads)
        {
            var path = new RegressionPath();
            var eplCreateVariable = "@public create table varagg (c0 int, c1 int, c2 int, c3 int, c4 int, c5 int)";
            env.CompileDeploy(eplCreateVariable, path);

            var eplMerge = "on SupportBean_S0 merge varagg " +
                           "when not matched then insert select -1 as c0, -1 as c1, -1 as c2, -1 as c3, -1 as c4, -1 as c5 " +
                           "when matched then update set c0=Id, c1=Id, c2=Id, c3=Id, c4=Id, c5=Id";
            env.CompileDeploy(eplMerge, path);

            var eplQuery = "@name('s0') select varagg.c0 as c0, varagg.c1 as c1, varagg.c2 as c2," +
                           "varagg.c3 as c3, varagg.c4 as c4, varagg.c5 as c5 from SupportBean_S1";
            env.CompileDeploy(eplQuery, path).AddListener("s0");

            var writeThreads = new Thread[numWriteThreads];
            var writeRunnables = new WriteRunnable[numWriteThreads];
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i] = new WriteRunnable(env, i);
                writeThreads[i] = new Thread(writeRunnables[i].Run) {
                    Name = nameof(InfraTableMTUngroupedAccessReadMergeWrite) + "-write"
                };
                writeThreads[i].Start();
            }

            var readRunnable = new ReadRunnable(env, env.Listener("s0"));
            var readThread = new Thread(readRunnable.Run) {
                Name = nameof(InfraTableMTUngroupedAccessReadMergeWrite) + "-read"
            };
            readThread.Start();

            Thread.Sleep(numSeconds * 1000);

            // join
            log.Info("Waiting for completion");
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i].Shutdown = true;
                writeThreads[i].Join();
                Assert.IsNull(writeRunnables[i].Exception);
            }

            readRunnable.Shutdown = true;
            readThread.Join();

            env.UndeployAll();
            Assert.IsNull(readRunnable.Exception);
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly int threadNum;

            private bool shutdown;

            public WriteRunnable(
                RegressionEnvironment env,
                int threadNum)
            {
                this.env = env;
                this.threadNum = threadNum;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for write");

                try {
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean_S0(threadNum));
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                log.Info("Completed event send for write");
            }
        }

        public class ReadRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly SupportListener listener;

            private bool shutdown;

            public ReadRunnable(
                RegressionEnvironment env,
                SupportListener listener)
            {
                this.env = env;
                this.listener = listener;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for read");

                try {
                    while (!shutdown) {
                        var fields = new[] {"c1", "c2", "c3", "c4", "c5"};
                        env.SendEventBean(new SupportBean_S1(0));
                        var @event = listener.AssertOneGetNewAndReset();
                        var valueOne = @event.Get("c0");
                        foreach (var field in fields) {
                            Assert.AreEqual(valueOne, @event.Get(field));
                        }
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                log.Info("Completed event send for read");
            }
        }
    }
} // end of namespace