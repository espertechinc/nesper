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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Primary key is single: {id}
        ///     For a given number of seconds:
        ///     Single writer insert-into such as {0} to {N}.
        ///     Single reader subquery-selects the count all rows.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 3);
            }
            catch (ThreadInterruptedException ex) {
                throw new EPException(ex);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numSeconds)
        {
            var path = new RegressionPath();
            var eplCreateVariable = "@public create table MyTable (pkey string primary key)";
            env.CompileDeploy(eplCreateVariable, path);

            var eplInsertInto = "insert into MyTable select TheString as pkey from SupportBean";
            env.CompileDeploy(eplInsertInto, path);

            // seed with count 1
            env.SendEventBean(new SupportBean("E0", 0));

            // select/read
            var eplSubselect = "@name('s0') select (select count(*) from MyTable) as c0 from SupportBean_S0";
            env.CompileDeploy(eplSubselect, path).AddListener("s0");

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, env.Listener("s0"));

            // start
            var writeThread = new Thread(writeRunnable.Run);
            writeThread.Name = nameof(InfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr) + "-write";
            var readThread = new Thread(readRunnable.Run);
            readThread.Name = nameof(InfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr) + "-read";
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
                        env.SendEventBean(new SupportBean("E" + numLoops + 1, 0));
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
                        env.SendEventBean(new SupportBean_S0(0));
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