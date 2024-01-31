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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        ///     Single group that always exists is {0,0}. Topgroup is always zero.
        ///     For a given number of seconds:
        ///     Single writer merge-inserts such as {0,1}, new object[] {0,2} to {0, N} then merge-deletes all rows one by one.
        ///     Single reader subquery-selects the count all values where subgroup equals 0, should always receive a count of 1 and
        ///     up.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 3);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds)
        {
            var path = new RegressionPath();
            var eplCreateVariable =
                "@public create table vartotal (topgroup int primary key, subgroup int primary key)";
            env.CompileDeploy(eplCreateVariable, path);

            var eplCreateIndex = "create index myindex on vartotal (topgroup)";
            env.CompileDeploy(eplCreateIndex, path);

            // insert and delete merge
            var eplMergeInsDel = "on SupportTopGroupSubGroupEvent as lge merge vartotal as vt " +
                                 "where vt.topgroup = lge.Topgroup and vt.subgroup = lge.Subgroup " +
                                 "when not matched and lge.Op = 'insert' then insert select lge.Topgroup as topgroup, lge.Subgroup as subgroup " +
                                 "when matched and lge.Op = 'delete' then delete";
            env.CompileDeploy(eplMergeInsDel, path);

            // seed with {0, 0} group
            env.SendEventBean(new SupportTopGroupSubGroupEvent(0, 0, "insert"));

            // select/read
            var eplSubselect =
                "@name('s0') select (select count(*) from vartotal where topgroup=sb.IntPrimitive) as c0 " +
                "from SupportBean as sb";
            env.CompileDeploy(eplSubselect, path).AddListener("s0");

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, env.Listener("s0"));

            // start
            var writeThread = new Thread(writeRunnable.Run);
            writeThread.Name = nameof(InfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd) + "-write";
            var readThread = new Thread(readRunnable.Run);
            readThread.Name = nameof(InfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd) + "-read";
            writeThread.Start();
            readThread.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;

            // join
            Log.Info("Waiting for completion");
            writeThread.Join();
            readThread.Join();

            env.UndeployAll();
            ClassicAssert.IsNull(writeRunnable.Exception);
            ClassicAssert.IsNull(readRunnable.Exception);
            ClassicAssert.IsTrue(writeRunnable.numLoops > 100);
            ClassicAssert.IsTrue(readRunnable.numQueries > 100);
            Console.Out.WriteLine(
                "Send " + writeRunnable.numLoops + " and performed " + readRunnable.numQueries + " reads");
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
                Log.Info("Started event send for write");

                try {
                    while (!shutdown) {
                        for (var i = 0; i < 10; i++) {
                            env.SendEventBean(new SupportTopGroupSubGroupEvent(0, i + 1, "insert"));
                        }

                        for (var i = 0; i < 10; i++) {
                            env.SendEventBean(new SupportTopGroupSubGroupEvent(0, i + 1, "delete"));
                        }

                        numLoops++;
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }

                Log.Info("Completed event send for write");
            }
        }

        public class ReadRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly SupportListener listener;
            internal Exception exception;

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

            public int NumQueries => numQueries;

            public void Run()
            {
                Log.Info("Started event send for read");

                try {
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean(null, 0));
                        var value = listener.AssertOneGetNewAndReset().Get("c0");
                        ClassicAssert.IsTrue((long?)value >= 1);
                        numQueries++;
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }

                Log.Info("Completed event send for read");
            }
        }
    }
} // end of namespace