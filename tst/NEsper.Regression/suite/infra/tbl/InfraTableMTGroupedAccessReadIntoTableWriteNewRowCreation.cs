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

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     Table:
        ///     create table varTotal (key string primary key, total sum(int));
        ///     <para />
        ///     For a given number of events
        ///     - Single writer expands the group-key space by sending additional keys.
        ///     - Single reader against a last-inserted group gets the non-zero-value.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 10000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numEvents)
        {
            var epl =
                "@public create table varTotal (key string primary key, total sum(int));\n" +
                "into table varTotal select TheString, sum(IntPrimitive) as total from SupportBean group by TheString;\n" +
                "@name('s0') select varTotal[P00].total as c0 from SupportBean_S0;\n";
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean("A", 10));

            var queueCreated = new LinkedBlockingQueue<string>();
            var writeRunnable = new WriteRunnable(env, numEvents, queueCreated);
            var readRunnable = new ReadRunnable(env, numEvents, queueCreated);

            // start
            var t1 = new Thread(writeRunnable.Run);
            t1.Name = nameof(InfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation) + "-write";
            var t2 = new Thread(readRunnable.Run);
            t2.Name = nameof(InfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation) + "-read";
            t1.Start();
            t2.Start();

            // join
            log.Info("Waiting for completion");
            t1.Join();
            t2.Join();

            env.UndeployAll();
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
        }

        public class WriteRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly int numEvents;
            private readonly IBlockingQueue<string> queueCreated;

            public WriteRunnable(
                RegressionEnvironment env,
                int numEvents,
                IBlockingQueue<string> queueCreated)
            {
                this.env = env;
                this.numEvents = numEvents;
                this.queueCreated = queueCreated;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for write");

                try {
                    for (var i = 0; i < numEvents; i++) {
                        var key = "E" + i;
                        env.SendEventBean(new SupportBean(key, 10));
                        queueCreated.Push(key);
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
            private readonly int numEvents;
            private readonly IBlockingQueue<string> queueCreated;

            public ReadRunnable(
                RegressionEnvironment env,
                int numEvents,
                IBlockingQueue<string> queueCreated)
            {
                this.env = env;
                this.numEvents = numEvents;
                this.queueCreated = queueCreated;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send for read");
                try {
                    var listener = env.Listener("s0");
                    var currentEventId = "A";

                    for (var i = 0; i < numEvents; i++) {
                        if (!queueCreated.IsEmpty()) {
                            currentEventId = queueCreated.Pop();
                        }

                        env.SendEventBean(new SupportBean_S0(0, currentEventId));
                        var value = listener.AssertOneGetNewAndReset().Get("c0").AsInt32();
                        Assert.AreEqual(10, value);
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