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
    public class InfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Tests column-consistency for subquery:
        ///     create table MyTable(p0 string, p1 string, ..., p4 string)   (5 props)
        ///     Insert row single: MyTable={p0="1", p1="1", p2="1", p3="1", p4="1"}
        ///     <para />
        ///     A writer-thread uses an on-merge statement to update the p0 to p4 columns from "1" to "2", then "2" to "1"
        ///     A reader-thread uses a subquery checking ("p1="1" and p2="1" and p3="1" and p4="1")
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 2);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds)
        {
            var epl =
                "create table MyTable (p0 string, p1 string, p2 string, p3 string, p4 string);\n" +
                "on SupportBean merge MyTable " +
                "  when not matched then insert select '1' as p0, '1' as p1, '1' as p2, '1' as p3, '1' as p4;\n" +
                "on SupportBean_S0 merge MyTable " +
                "  when matched then update set p0=P00, p1=P00, p2=P00, p3=P00, p4=P00;\n" +
                "@Name('out') select " +
                "(select p0 from MyTable where " +
                "  (p0='1' and p1='1' and p2='1' and p3='1' and p4='1') " +
                "  or (p0='2' and p1='2' and p2='2' and p3='2' and p4='2')) as c0 " +
                "from SupportBean_S1;\n";
            env.CompileDeploy(epl);

            // preload
            env.SendEventBean(new SupportBean());

            var writeRunnable = new InfraTableMTUngroupedJoinColumnConsistency.UpdateWriteRunnable(env);
            var readRunnable = new ReadRunnable(env);

            // start
            var threadWrite = new Thread(writeRunnable.Run);
            threadWrite.Name = nameof(InfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd) + "-write";
            var threadRead = new Thread(readRunnable.Run);
            threadRead.Name = nameof(InfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd) + "-read";
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
                "Write loops " + writeRunnable.NumLoops + " and performed " + readRunnable.numQueries + " reads");
            Assert.IsTrue(writeRunnable.NumLoops > 1);
            Assert.IsTrue(readRunnable.numQueries > 100);
        }

        public class ReadRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly SupportListener listener;

            internal int numQueries;
            private bool shutdown;

            public ReadRunnable(RegressionEnvironment env)
            {
                this.env = env;
                env.AddListener("out");
                listener = env.Listener("out");
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
                        env.SendEventBean(new SupportBean_S1(0, null));
                        var value = listener.AssertOneGetNewAndReset().Get("c0");
                        if (value == null) {
                            throw new IllegalStateException("Value is null");
                        }

                        numQueries++;
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