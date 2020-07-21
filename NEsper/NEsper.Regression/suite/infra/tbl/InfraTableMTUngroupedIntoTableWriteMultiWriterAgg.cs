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
    public class InfraTableMTUngroupedIntoTableWriteMultiWriterAgg : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     For a given number of seconds:
        ///     Configurable number of into-writers update a shared aggregation.
        ///     At the end of the test we read and assert.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 3, 10000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numThreads,
            int numEvents)
        {
            var path = new RegressionPath();
            var eplCreateVariable = "create table varagg (theEvents window(*) @type(SupportBean))";
            env.CompileDeploy(eplCreateVariable, path);

            var threads = new Thread[numThreads];
            var runnables = new WriteRunnable[numThreads];
            for (var i = 0; i < threads.Length; i++) {
                runnables[i] = new WriteRunnable(env, path, numEvents, i);
                threads[i] = new Thread(runnables[i].Run) {
                    Name = typeof(InfraTableMTUngroupedIntoTableWriteMultiWriterAgg).Name + "-write"
                };
                threads[i].Start();
            }

            // join
            log.Info("Waiting for completion");
            for (var i = 0; i < threads.Length; i++) {
                threads[i].Join();
                Assert.IsNull(runnables[i].Exception);
            }

            // verify
            env.CompileDeploy("@name('s0') select varagg.theEvents as c0 from SupportBean_S0", path).AddListener("s0");
            env.SendEventBean(new SupportBean_S0(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var window = (SupportBean[]) @event.Get("c0");
            Assert.AreEqual(numThreads * 3, window.Length);

            env.UndeployAll();
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly int numEvents;
            private readonly RegressionPath path;
            private readonly int threadNum;

            public WriteRunnable(
                RegressionEnvironment env,
                RegressionPath path,
                int numEvents,
                int threadNum)
            {
                this.env = env;
                this.path = path;
                this.numEvents = numEvents;
                this.threadNum = threadNum;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for write");

                try {
                    var eplInto = "into table varagg select window(*) as theEvents from SupportBean(TheString='E" +
                                  threadNum +
                                  "')#length(3)";
                    env.CompileDeploy(eplInto, path);

                    for (var i = 0; i < numEvents; i++) {
                        env.SendEventBean(new SupportBean("E" + threadNum, i));
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                log.Info("Completed event send for write");
            }
        }
    }
} // end of namespace