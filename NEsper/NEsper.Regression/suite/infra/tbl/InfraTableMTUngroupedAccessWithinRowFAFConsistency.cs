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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTUngroupedAccessWithinRowFAFConsistency : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     For a given number of seconds:
        ///     Single writer updates the group (round-robin) count, sum and avg.
        ///     A FAF reader thread pulls the value and checks they are consistent.
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
            var path = new RegressionPath();
            var eplCreateVariable = "create table vartotal (cnt count(*), sumint sum(int), avgint avg(int))";
            env.CompileDeploy(eplCreateVariable, path);

            var eplInto =
                "into table vartotal select count(*) as cnt, sum(IntPrimitive) as sumint, avg(IntPrimitive) as avgint from SupportBean";
            env.CompileDeploy(eplInto, path);

            env.CompileDeploy("create window MyWindow#lastevent as SupportBean_S0", path);
            env.CompileDeploy("insert into MyWindow select * from SupportBean_S0", path);
            env.SendEventBean(new SupportBean_S0(0));

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, path);

            // start
            var t1 = new Thread(writeRunnable.Run);
            t1.Name = typeof(InfraTableMTUngroupedAccessWithinRowFAFConsistency).Name + "-write";
            var t2 = new Thread(readRunnable.Run);
            t2.Name = typeof(InfraTableMTUngroupedAccessWithinRowFAFConsistency).Name + "-read";
            t1.Start();
            t2.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;

            // join
            log.Info("Waiting for completion");
            t1.Join();
            t2.Join();

            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            log.Info("Send " + writeRunnable.numEvents + " and performed " + readRunnable.numQueries + " reads");
            Assert.IsTrue(writeRunnable.numEvents > 100);
            Assert.IsTrue(readRunnable.numQueries > 20);

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
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean("E1", 2));
                        numEvents++;
                        try {
                            Thread.Sleep(1);
                        }
                        catch (ThreadInterruptedException) {
                            shutdown = true;
                        }
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
            private readonly RegressionPath path;

            internal int numQueries;
            internal bool shutdown;

            public ReadRunnable(
                RegressionEnvironment env,
                RegressionPath path)
            {
                this.env = env;
                this.path = path;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for read");

                // warmup
                try {
                    Thread.Sleep(100);
                }
                catch (ThreadInterruptedException) {
                }

                try {
                    var eplSelect =
                        "select vartotal.cnt as c0, vartotal.sumint as c1, vartotal.avgint as c2 from MyWindow";
                    var compiled = env.CompileFAF(eplSelect, path);

                    while (!shutdown) {
                        var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
                        var count = result.Array[0].Get("c0").AsLong();
                        var sumint = result.Array[0].Get("c1").AsInt();
                        var avgint = (double) result.Array[0].Get("c2");
                        Assert.AreEqual(2d, avgint, 0);
                        Assert.AreEqual(sumint, count * 2);
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