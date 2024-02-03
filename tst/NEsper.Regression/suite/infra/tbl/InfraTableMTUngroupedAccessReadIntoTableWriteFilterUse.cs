///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class InfraTableMTUngroupedAccessReadIntoTableWriteFilterUse : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        /// <summary>
        ///     For a given number of seconds:
        ///     Single writer updates a total sum, continuously adding 1 and subtracting 1.
        ///     Two statements are set up, one listens to "0" and the other to "1"
        ///     Single reader sends event and that event must be received by any one of the listeners.
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
            var eplCreateVariable = "@public create table vartotal (Total sum(int))";
            env.CompileDeploy(eplCreateVariable, path);

            var eplInto = "into table vartotal select sum(IntPrimitive) as Total from SupportBean";
            env.CompileDeploy(eplInto, path);

            env.CompileDeploy("@name('s0') select * from SupportBean_S0(1 = vartotal.Total)", path).AddListener("s0");

            env.CompileDeploy("@name('s1') select * from SupportBean_S0(0 = vartotal.Total)", path).AddListener("s1");

            var writeRunnable = new WriteRunnable(env);
            var readRunnable = new ReadRunnable(env, env.Listener("s0"), env.Listener("s1"));

            // start
            var t1 = new Thread(writeRunnable.Run) {
                Name = nameof(InfraTableMTUngroupedAccessReadIntoTableWriteFilterUse) + "-write"
            };
            var t2 = new Thread(readRunnable.Run) {
                Name = nameof(InfraTableMTUngroupedAccessReadIntoTableWriteFilterUse) + "-read"
            };
            t1.Start();
            t2.Start();

            // wait
            Thread.Sleep(numSeconds * 1000);

            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;

            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();

            env.UndeployAll();
            ClassicAssert.IsNull(writeRunnable.Exception);
            ClassicAssert.IsNull(readRunnable.Exception);
            ClassicAssert.IsTrue(writeRunnable.numEvents > 100);
            ClassicAssert.IsTrue(readRunnable.numQueries > 100);
            Console.Out.WriteLine(
                "Send " + writeRunnable.numEvents + " and performed " + readRunnable.numQueries + " reads");
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
                Log.Info("Started event send for write");

                try {
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean("E", 1));
                        env.SendEventBean(new SupportBean("E", -1));
                        numEvents++;
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send for write");
            }
        }

        public class ReadRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly SupportListener listenerOne;
            private readonly SupportListener listenerZero;

            internal int numQueries;
            internal bool shutdown;

            public ReadRunnable(
                RegressionEnvironment env,
                SupportListener listenerZero,
                SupportListener listenerOne)
            {
                this.env = env;
                this.listenerZero = listenerZero;
                this.listenerOne = listenerOne;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                Log.Info("Started event send for read");

                try {
                    while (!shutdown) {
                        env.SendEventBean(new SupportBean_S0(0));
                        listenerZero.Reset();
                        listenerOne.Reset();
                        numQueries++;
                    }
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                Log.Info("Completed event send for read");
            }
        }
    }
} // end of namespace