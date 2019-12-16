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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableMTUngroupedAccessReadInotTableWriteIterate : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Proof that multiple threads iterating the same statement
        ///     can safely access a row that is currently changing.
        /// </summary>
        public void Run(RegressionEnvironment env)
        {
            try {
                TryMT(env, 3, 3);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        private static void TryMT(
            RegressionEnvironment env,
            int numReadThreads,
            int numSeconds)
        {
            var path = new RegressionPath();
            var eplCreateVariable = "create table vartotal (S0 sum(int), S1 sum(double), S2 sum(long))";
            env.CompileDeploy(eplCreateVariable, path);

            var eplInto = "into table vartotal select " +
                          "sum(IntPrimitive) as S0, " +
                          "sum(DoublePrimitive) as S1, " +
                          "sum(LongPrimitive) as S2 " +
                          "from SupportBean";
            env.CompileDeploy(eplInto, path);
            env.SendEventBean(MakeSupportBean("E", 1, 1, 1));

            env.CompileDeploy(
                "@Name('iterate') select vartotal.S0 as c0, vartotal.S1 as c1, vartotal.S2 as c2 from SupportBean_S0#lastevent",
                path);
            env.SendEventBean(new SupportBean_S0(0));

            // setup writer
            var writeRunnable = new WriteRunnable(env);
            var writeThread = new Thread(writeRunnable.Run) {
                Name = typeof(InfraTableMTUngroupedAccessReadInotTableWriteIterate).Name + "-write"
            };

            // setup readers
            var readThreads = new Thread[numReadThreads];
            var readRunnables = new ReadRunnable[numReadThreads];
            for (var i = 0; i < readThreads.Length; i++) {
                readRunnables[i] = new ReadRunnable(env.Statement("iterate"));
                readThreads[i] = new Thread(readRunnables[i].Run) {
                    Name = typeof(InfraTableMTUngroupedAccessReadInotTableWriteIterate).Name + "-read"
                };
            }

            // start
            foreach (var readThread in readThreads) {
                readThread.Start();
            }

            writeThread.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            foreach (var readRunnable in readRunnables) {
                readRunnable.Shutdown = true;
            }

            // join
            log.Info("Waiting for completion");
            writeThread.Join();
            foreach (var readThread in readThreads) {
                readThread.Join();
            }

            env.UndeployAll();

            // assert
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.numEvents > 100);
            foreach (var readRunnable in readRunnables) {
                Assert.IsNull(readRunnable.Exception);
                Assert.IsTrue(readRunnable.numQueries > 100);
            }
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            double doublePrimitive,
            long longPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            b.DoublePrimitive = doublePrimitive;
            b.LongPrimitive = longPrimitive;
            return b;
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
                        env.SendEventBean(MakeSupportBean("E", 1, 1, 1));
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
            private readonly EPStatement iterateStatement;

            internal Exception exception;
            internal int numQueries;
            internal bool shutdown;

            public ReadRunnable(EPStatement iterateStatement)
            {
                this.iterateStatement = iterateStatement;
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
                        using (var iterator = iterateStatement.GetSafeEnumerator()) {
                            var @event = iterator.Advance();
                            var c0 = @event.Get("c0").AsInt32();
                            Assert.AreEqual((double) c0, @event.Get("c1"));
                            Assert.AreEqual((long) c0, @event.Get("c2"));
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