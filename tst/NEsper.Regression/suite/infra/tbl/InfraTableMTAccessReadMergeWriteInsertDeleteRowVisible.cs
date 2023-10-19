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
using com.espertech.esper.common.client.scopetest;
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
    public class InfraTableMTAccessReadMergeWriteInsertDeleteRowVisible : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Table:
        ///     create table MyTable(key string primary key, p0 int, p1 int, p2, int, p3 int, p4 int)
        ///     <para />
        ///     For a given number of seconds:
        ///     - Single writer uses merge in a loop:
        ///     - inserts MyTable={key='K1', p0=1, p1=1, p2=1, p3=1, p4=1}
        ///     - deletes the row
        ///     - Single reader outputs p0 to p4 using "MyTable['K1'].px"
        ///     Row should either exist with all values found or not exist.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 1, true);
                TryMT(env, 1, false);
            }
            catch (ThreadInterruptedException ex) {
                throw new EPException(ex);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds,
            bool grouped)
        {
            var path = new RegressionPath();
            var eplCreateTable = "@public create table MyTable (key string " +
                                 (grouped ? "primary key" : "") +
                                 ", p0 int, p1 int, p2 int, p3 int, p4 int, p5 int)";
            env.CompileDeploy(eplCreateTable, path);

            var eplSelect = grouped
                ? "@name('s0') select MyTable['K1'].p0 as c0, MyTable['K1'].p1 as c1, MyTable['K1'].p2 as c2, " +
                  "MyTable['K1'].p3 as c3, MyTable['K1'].p4 as c4, MyTable['K1'].p5 as c5 from SupportBean_S0"
                : "@name('s0') select MyTable.p0 as c0, MyTable.p1 as c1, MyTable.p2 as c2, " +
                  "MyTable.p3 as c3, MyTable.p4 as c4, MyTable.p5 as c5 from SupportBean_S0";
            env.CompileDeploy(eplSelect, path).AddListener("s0");

            var eplMerge = "on SupportBean merge MyTable " +
                           "when not matched then insert select 'K1' as key, 1 as p0, 1 as p1, 1 as p2, 1 as p3, 1 as p4, 1 as p5 " +
                           "when matched then delete";
            env.CompileDeploy(eplMerge, path);

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, env.Listener("s0"));

            // start
            var t1 = new Thread(writeRunnable.Run)
                { Name = nameof(InfraTableMTAccessReadMergeWriteInsertDeleteRowVisible) + "::Write" };
            var t2 = new Thread(readRunnable.Run)
                { Name = nameof(InfraTableMTAccessReadMergeWriteInsertDeleteRowVisible) + "::Read" };

            t1.Start();
            t2.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            Console.WriteLine("WriteRunnable.Shutdown: true");
            writeRunnable.Shutdown = true;
            Console.WriteLine("ReadRunnable.Shutdown: true");
            readRunnable.Shutdown = true;

            // join
            log.Info("Waiting for completion");
            t1.Join(TimeSpan.FromSeconds(30));
            t2.Join(TimeSpan.FromSeconds(30));

            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.numEvents > 100);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(readRunnable.numQueries > 100);
            Assert.IsTrue(readRunnable.NotFoundCount > 2);
            Assert.IsTrue(readRunnable.FoundCount > 2);
            Console.Out.WriteLine(
                "Send " +
                writeRunnable.numEvents +
                " and performed " +
                readRunnable.numQueries +
                " reads (found " +
                readRunnable.FoundCount +
                ") (not found " +
                readRunnable.NotFoundCount +
                ")");

            env.UndeployAll();
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;

            internal Exception exception;
            internal int numEvents;
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
                        env.SendEventBean(new SupportBean(null, 0));
                        numEvents++;
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
            internal int foundCount;
            internal int notFoundCount;
            internal int numQueries;
            internal bool shutdown;

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

            public Exception Exception => exception;

            public int FoundCount => foundCount;

            public int NotFoundCount => notFoundCount;

            public void Run()
            {
                log.Info("Started event send for read");

                try {
                    var fields = new[] { "c0", "c1", "c2", "c3", "c4", "c5" };
                    object[] expected = { 1, 1, 1, 1, 1, 1 };
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean_S0(0));
                        var @event = listener.AssertOneGetNewAndReset();
                        if (@event.Get("c0") == null) {
                            notFoundCount++;
                        }
                        else {
                            foundCount++;
                            EPAssertionUtil.AssertProps(@event, fields, expected);
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