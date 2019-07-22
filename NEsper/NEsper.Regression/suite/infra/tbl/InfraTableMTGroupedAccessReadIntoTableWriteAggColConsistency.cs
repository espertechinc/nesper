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

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Table:
        ///     create table vartotal (key string primary key, tc0 sum(int), tc1 sum(int) ... tc9 sum(int))
        ///     <para />
        ///     Seed the table with a number of groups, no new ones are added or deleted during the test.
        ///     For a given number of seconds and a given number of groups:
        ///     - Single writer updates a group (round-robin), each group associates with 10 columns .
        ///     - N readers pull a group's columns, round-robin, check that all 10 values are consistent.
        ///     - The 10 values are sum-int totals that are expected to all have the same value.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 10, 3);
            }
            catch (ThreadInterruptedException e) {
                throw new IllegalStateException("Unexpected Exception", e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numGroups,
            int numSeconds)
        {
            var path = new RegressionPath();
            var eplCreateVariable = "create table vartotal (key string primary key, " +
                                    CollectionUtil.ToString(GetDeclareCols()) +
                                    ")";
            env.CompileDeploy(eplCreateVariable, path);

            var eplInto = "into table vartotal select " +
                          CollectionUtil.ToString(GetIntoCols()) +
                          " from Support10ColEvent group by groupKey";
            env.CompileDeploy(eplInto, path);

            // initialize groups
            var groups = new string[numGroups];
            for (var i = 0; i < numGroups; i++) {
                groups[i] = "G" + i;
                env.SendEventBean(new Support10ColEvent(groups[i], 0));
            }

            var writeRunnable = new WriteRunnable(env, groups);
            var readRunnable = new ReadRunnable(env, path, groups);

            // start
            var t1 = new Thread(writeRunnable.Run) {
                Name = typeof(InfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency).Name + "-write"
            };
            var t2 = new Thread(readRunnable.Run) {
                Name = typeof(InfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency).Name + "-read"
            };
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

            env.UndeployAll();
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.numEvents > 100);
            Assert.IsTrue(readRunnable.numQueries > 100);
            Console.Out.WriteLine(
                "Send " + writeRunnable.numEvents + " and performed " + readRunnable.numQueries + " reads");
        }

        private static ICollection<string> GetDeclareCols()
        {
            IList<string> cols = new List<string>();
            for (var i = 0; i < 10; i++) { // 10 columns, not configurable
                cols.Add("tc" + i + " sum(int)");
            }

            return cols;
        }

        private static ICollection<string> GetIntoCols()
        {
            IList<string> cols = new List<string>();
            for (var i = 0; i < 10; i++) { // 10 columns, not configurable
                cols.Add("sum(c" + i + ") as tc" + i);
            }

            return cols;
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly string[] groups;

            internal Exception exception;
            internal int numEvents;
            internal bool shutdown;

            public WriteRunnable(
                RegressionEnvironment env,
                string[] groups)
            {
                this.env = env;
                this.groups = groups;
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
                        var groupNum = numEvents % groups.Length;
                        env.SendEventBean(new Support10ColEvent(groups[groupNum], numEvents));
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
            private readonly string[] groups;
            private readonly RegressionPath path;

            internal Exception exception;
            internal int numQueries;
            internal bool shutdown;

            public ReadRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                string[] groups)
            {
                this.env = env;
                this.path = path;
                this.groups = groups;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception => exception;

            public void Run()
            {
                log.Info("Started event send for read");

                try {
                    var eplSelect = "@Name('s0') select vartotal[TheString] as out from SupportBean";
                    env.CompileDeploy(eplSelect, path).AddListener("s0");
                    var listener = env.Listener("s0");

                    while (!shutdown) {
                        var groupNum = numQueries % groups.Length;
                        env.SendEventBean(new SupportBean(groups[groupNum], 0));
                        var @event = listener.AssertOneGetNewAndReset();
                        AssertEvent((IDictionary<string, object>) @event.Get("out"));
                        numQueries++;
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }

                log.Info("Completed event send for read");
            }

            private static void AssertEvent(IDictionary<string, object> info)
            {
                var tc0 = info.Get("tc0");
                for (var i = 1; i < 10; i++) {
                    Assert.AreEqual(tc0, info.Get("tc" + i));
                }
            }
        }
    }
} // end of namespace