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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedWContextIntoTableWriteAsSharedTable : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Multiple writers share a key space that they aggregate into.
        ///     Writer utilize a hash partition context.
        ///     After all writers are done validate the space.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            // with T, N, G:  Each of T threads loops N times and sends for each loop G events for each group.
            // for a total of T*N*G events being processed, and G aggregations retained in a shared variable.
            // Group is the innermost loop.
            try {
                TryMT(env, 8, 1000, 64);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numThreads,
            int numLoops,
            int numGroups)
        {
            var eplDeclare =
                "@public create table varTotal (key string primary key, Total sum(int));\n" +
                "@public create context ByStringHash\n" +
                "  coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate\n;" +
                "context ByStringHash into table varTotal select TheString, sum(IntPrimitive) as Total from SupportBean group by TheString;\n";
            var eplAssert = "select varTotal[P00].Total as c0 from SupportBean_S0";

            RunAndAssert(env, eplDeclare, eplAssert, numThreads, numLoops, numGroups);
        }

        public static void RunAndAssert(
            RegressionEnvironment env,
            string eplDeclare,
            string eplAssert,
            int numThreads,
            int numLoops,
            int numGroups)
        {
            var path = new RegressionPath();
            env.CompileDeploy(eplDeclare, path);

            // setup readers
            var writeThreads = new Thread[numThreads];
            var writeRunnables = new WriteRunnable[numThreads];
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i] = new WriteRunnable(env, numLoops, numGroups);
                writeThreads[i] = new Thread(writeRunnables[i].Run) {
                    Name = nameof(InfraTableMTGroupedWContextIntoTableWriteAsSharedTable) + "-write"
                };
            }

            // start
            foreach (var writeThread in writeThreads) {
                writeThread.Start();
            }

            // join
            Log.Info("Waiting for completion");
            foreach (var writeThread in writeThreads) {
                writeThread.Join();
            }

            // assert
            foreach (var writeRunnable in writeRunnables) {
                ClassicAssert.IsNull(writeRunnable.Exception);
            }

            // each group should total up to "numLoops*numThreads"
            env.CompileDeploy("@name('s0') " + eplAssert, path).AddListener("s0");

            int? expected = numLoops * numThreads;
            for (var i = 0; i < numGroups; i++) {
                env.SendEventBean(new SupportBean_S0(0, "G" + i));
                env.AssertEqualsNew("s0", "c0", expected);
            }

            env.UndeployAll();
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly int numGroups;
            private readonly int numLoops;

            public WriteRunnable(
                RegressionEnvironment env,
                int numLoops,
                int numGroups)
            {
                this.env = env;
                this.numGroups = numGroups;
                this.numLoops = numLoops;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                Log.Info("Started event send for write");

                try {
                    for (var i = 0; i < numLoops; i++) {
                        for (var j = 0; j < numGroups; j++) {
                            env.SendEventBean(new SupportBean("G" + j, 1));
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send for write");
            }
        }
    }
} // end of namespace