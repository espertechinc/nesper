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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd : RegressionExecution
    {
        private const int NUM_KEYS = 10;
        private const int OFFSET_ADDED = 100000000;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Tests concurrent updates on a secondary index also read by a join:
        ///     create table MyTable (key string primary key, value int)
        ///     create index MyIndex on MyTable (value)
        ///     select * from SupportBean_S0, MyTable where intPrimitive = id
        ///     <para>
        ///         Prefill MyTable with MyTable={key='A_N', value=N} with N between 0 and NUM_KEYS-1
        ///     </para>
        ///     <para>
        ///         For x seconds:
        ///         Single reader thread sends SupportBean events, asserts that either one or two rows are found (A_N and maybe
        ///         B_N)
        ///         Single writer thread inserts MyTable={key='B_N', value=100000+N} and deletes each row.
        ///     </para>
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 2);
            }
            catch (ThreadInterruptedException ex) {
                throw new EPException(ex);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds)
        {
            var epl =
                "create table MyTable (key1 string primary key, value int);\n" +
                "create index MyIndex on MyTable (value);\n" +
                "on SupportBean merge MyTable where theString = key1 when not matched then insert select TheString as key1, intPrimitive as value;\n" +
                "@Name('out') select * from SupportBean_S0, MyTable where value = id;\n" +
                "on SupportBean_S1 delete from MyTable where key1 like 'B%';\n";
            env.CompileDeploy(epl).AddListener("out");

            // preload A_n events
            for (var i = 0; i < NUM_KEYS; i++) {
                env.SendEventBean(new SupportBean("A_" + i, i));
            }

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env);

            // start
            var threadWrite = new Thread(writeRunnable.Run) {
                Name = typeof(InfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd).Name + "-write"
            };
            var threadRead = new Thread(readRunnable.Run) {
                Name = typeof(InfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd).Name + "-read"
            };
            threadWrite.Start();
            threadRead.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;

            // join
            log.Info("Waiting for completion");
            threadWrite.Join();
            threadRead.Join();

            env.UndeployAll();

            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Console.Out.WriteLine(
                "Write loops " + writeRunnable.numLoops + " and performed " + readRunnable.numQueries + " reads");
            Assert.IsTrue(writeRunnable.numLoops > 1);
            Assert.IsTrue(readRunnable.numQueries > 100);
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;

            internal Exception exception;
            internal int numLoops;
            internal bool shutdown;

            public WriteRunnable(RegressionEnvironment env)
            {
                this.env = env;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception => exception;

            public void Run()
            {
                log.Info("Started event send for write");

                try {
                    while (!shutdown) {
                        // write additional B_n events
                        for (var i = 0; i < 10000; i++) {
                            env.SendEventBean(new SupportBean("B_" + i, i + OFFSET_ADDED));
                        }

                        // delete B_n events
                        env.SendEventBean(new SupportBean_S1(0));
                        numLoops++;
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }

                log.Info("Completed event send for write");
            }
        }

        public class ReadRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly SupportListener listener;

            internal Exception exception;
            internal int numQueries;
            internal bool shutdown;

            public ReadRunnable(RegressionEnvironment env)
            {
                this.env = env;
                listener = env.Listener("out");
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception => exception;

            public void Run()
            {
                log.Info("Started event send for read");

                try {
                    while (!shutdown) {
                        for (var i = 0; i < NUM_KEYS; i++) {
                            env.SendEventBean(new SupportBean_S0(i));
                            var events = listener.GetAndResetLastNewData();
                            Assert.IsTrue(events.Length > 0);
                        }

                        numQueries++;
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }

                log.Info("Completed event send for read");
            }
        }
    }
} // end of namespace