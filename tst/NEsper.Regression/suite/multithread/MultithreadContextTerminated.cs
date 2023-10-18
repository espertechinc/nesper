///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextTerminated : RegressionExecutionWithConfigure
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }
        
        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Common.AddEventType(typeof(StartContextEvent));
            configuration.Common.AddEventType(typeof(PayloadEvent));
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var eplStatement = "@public create context StartThenTwoSeconds start StartContextEvent end after 2 seconds";
            env.CompileDeploy(eplStatement, path);

            var aggStatement =
                "@name('select') context StartThenTwoSeconds " +
                "select Account, count(*) as totalCount " +
                "from PayloadEvent " +
                "group by Account " +
                "output snapshot when terminated";
            env.CompileDeploy(aggStatement, path);
            env.Statement("select").Events += (
                sender,
                args) => {
                // no action, still listening to make sure select-clause evaluates
            };

            // start context
            env.SendEventBean(new StartContextEvent());

            // start threads
            IList<Thread> threads = new List<Thread>();
            IList<MyRunnable> runnables = new List<MyRunnable>();
            for (var i = 0; i < 8; i++) {
                var myRunnable = new MyRunnable(env.Runtime);
                runnables.Add(myRunnable);
                var thread = new Thread(myRunnable.Run);
                thread.Name = GetType().Name + "-Thread" + i;
                thread.Start();
                threads.Add(thread);
            }

            // join
            foreach (var thread in threads) {
                ThreadJoin(thread);
            }

            // assert
            foreach (var runnable in runnables) {
                Assert.IsNull(runnable.Exception);
            }

            env.UndeployAll();
        }

        public class StartContextEvent
        {
        }

        public class PayloadEvent
        {
            public PayloadEvent(string account)
            {
                Account = account;
            }

            public string Account { get; }
        }

        public class MyRunnable : IRunnable
        {
            private readonly EPRuntime runtime;

            public MyRunnable(EPRuntime runtime)
            {
                this.runtime = runtime;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try {
                    for (var i = 0; i < 2000000; i++) {
                        var payloadEvent = new PayloadEvent("A1");
                        runtime.EventService.SendEventBean(payloadEvent, "PayloadEvent");
                    }
                }
                catch (Exception ex) {
                    Exception = ex;
                }
            }
        }
    }
} // end of namespace