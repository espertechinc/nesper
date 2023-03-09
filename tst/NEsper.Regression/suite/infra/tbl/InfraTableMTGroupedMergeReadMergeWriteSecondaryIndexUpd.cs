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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        ///     For a given number of seconds:
        ///     Single writer inserts such as {0,1}, new object[] {0,2} to {0, N}, each event a new subgroup and topgroup always 0.
        ///     Single reader tries to count all values where subgroup equals 0, should always receive a count of 1 and increasing.
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
                "create table vartotal (topgroup int primary key, subgroup int primary key, thecnt count(*))";
            env.CompileDeploy(eplCreateVariable, path);

            var eplCreateIndex = "create index myindex on vartotal (topgroup)";
            env.CompileDeploy(eplCreateIndex, path);

            // populate
            var eplInto =
                "into table vartotal select count(*) as thecnt from SupportTopGroupSubGroupEvent#length(100) group by Topgroup, Subgroup";
            env.CompileDeploy(eplInto, path);

            // delete empty groups
            var eplDelete = "on SupportBean_S0 merge vartotal when matched and thecnt = 0 then delete";
            env.CompileDeploy(eplDelete, path);

            // seed with {0, 0} group
            env.SendEventBean(new SupportTopGroupSubGroupEvent(0, 0));

            // select/read
            var eplMergeSelect = "on SupportBean merge vartotal as vt " +
                                 "where vt.topgroup = IntPrimitive and vt.thecnt > 0 " +
                                 "when matched then insert into MyOutputStream select *";
            env.CompileDeploy(eplMergeSelect, path);
            env.CompileDeploy("@Name('s0') select * from MyOutputStream", path).AddListener("s0");
            var listener = env.Listener("s0");

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, listener);

            // start
            var writeThread = new Thread(writeRunnable.Run);
            writeThread.Name = nameof(InfraTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd) + "-write";
            var readThread = new Thread(readRunnable.Run);
            readThread.Name = nameof(InfraTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd) + "-read";
            writeThread.Start();
            readThread.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;

            // join
            log.Info("Waiting for completion");
            writeThread.Join();
            readThread.Join();

            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.numEvents > 100);
            Assert.IsTrue(readRunnable.numQueries > 100);
            Console.Out.WriteLine(
                "Send " + writeRunnable.numEvents + " and performed " + readRunnable.numQueries + " reads");

            env.UndeployAll();
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;

            internal int numEvents;
            internal bool shutdown;

            public WriteRunnable(RegressionEnvironment env)
            {
                this.env = env;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for write");

                try {
                    var subgroup = 1;
                    while (!shutdown) {
                        env.SendEventBean(new SupportTopGroupSubGroupEvent(0, subgroup));
                        subgroup++;

                        // send delete event
                        if (subgroup % 100 == 0) {
                            env.SendEventBean(new SupportBean_S0(0));
                        }

                        numEvents++;
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

            public void Run()
            {
                log.Info("Started event send for read");

                try {
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean(null, 0));
                        var len = listener.NewDataList.Count;
                        // Comment me in: System.out.println("Number of events found: " + len);
                        listener.Reset();
                        Assert.IsTrue(len >= 1);
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