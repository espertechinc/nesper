///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadStmtNamedWindowJoinUniqueView : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "create window A#unique(key) as MyEventA;\n" +
                      "create window B#unique(key) as MyEventB;\n" +
                      "insert into A select * from MyEventA;\n" +
                      "insert into B select * from MyEventB;\n" +
                      "\n" +
                      "@Name('stmt') select sum(A.data) as aTotal,sum(B.data) as bTotal " +
                      "from A unIdirectional, B where A.key = B.key;\n";
            env.CompileDeploy(epl);

            var es = Executors.NewFixedThreadPool(
                10,
                new SupportThreadFactory(typeof(MultithreadStmtNamedWindowJoinUniqueView)).ThreadFactory);
            IList<MyRunnable> runnables = new List<MyRunnable>();
            for (var i = 0; i < 6; i++) {
                runnables.Add(new MyRunnable(env.EventService));
            }

            foreach (var toRun in runnables) {
                es.Submit(toRun.Run);
            }

            SupportCompileDeployUtil.ThreadSleep(2000);
            foreach (var toRun in runnables) {
                toRun.Shutdown = true;
            }

            es.Shutdown();
            SupportCompileDeployUtil.ThreadpoolAwait(es, 20, TimeUnit.SECONDS);

            foreach (var runnable in runnables) {
                Assert.IsNull(runnable.Exception);
            }

            env.UndeployAll();
        }

        public class MyRunnable : IRunnable
        {
            private readonly EPEventService runtime;

            private bool shutdown;

            public MyRunnable(EPEventService runtime)
            {
                this.runtime = runtime;
            }

            public Exception Exception { get; private set; }

            public bool Shutdown {
                set => shutdown = value;
            }

            public void Run()
            {
                try {
                    var random = new Random();
                    for (var i = 0; i < 1000; i++) {
                        runtime.SendEventBean(new MyEventA("key1", random.Next(1000000)), "MyEventA");
                        runtime.SendEventBean(new MyEventA("key2", random.Next(1000000)), "MyEventA");
                        runtime.SendEventBean(new MyEventB("key1", random.Next(1000000)), "MyEventB");
                        runtime.SendEventBean(new MyEventB("key2", random.Next(1000000)), "MyEventB");

                        if (shutdown) {
                            break;
                        }
                    }
                }
                catch (Exception ex) {
                    Exception = ex;
                }
            }
        }

        public class MyEventA
        {
            public MyEventA(
                string key,
                int data)
            {
                Key = key;
                Data = data;
            }

            public string Key { get; }

            public int Data { get; }
        }

        public class MyEventB
        {
            public MyEventB(
                string key,
                int data)
            {
                Key = key;
                Data = data;
            }

            public string Key { get; }

            public int Data { get; }
        }
    }
} // end of namespace