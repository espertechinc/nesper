///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTStmtNamedWindowJoinUniqueView : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType(typeof(MyEventA));
            configuration.AddEventType(typeof(MyEventB));
        }

        public override void Run(EPServiceProvider epService)
        {
            var epl =
                "create window A#unique(key) as MyEventA;\n" +
                "create window B#unique(key) as MyEventB;\n" +
                "insert into A select * from MyEventA;\n" +
                "insert into B select * from MyEventB;\n" +
                "\n" +
                "@Name('stmt') select sum(A.data) as aTotal,sum(B.data) as bTotal " +
                "from A unidirectional, B where A.key = B.key;\n";
            var deployment = epService.EPAdministrator.DeploymentAdmin;
            deployment.ParseDeploy(epl);

            var es = Executors.NewFixedThreadPool(10);
            var runnables = new List<MyRunnable>();
            for (var i = 0; i < 6; i++)
            {
                runnables.Add(new MyRunnable(epService.EPRuntime));
            }

            foreach (var toRun in runnables)
            {
                es.Submit(toRun.Run);
            }

            es.Shutdown();
            es.AwaitTermination(20, TimeUnit.SECONDS);

            foreach (var runnable in runnables)
            {
                Assert.IsNull(runnable.Exception);
            }
        }

        public class MyRunnable
        {
            private readonly EPRuntime _runtime;

            public MyRunnable(EPRuntime runtime)
            {
                _runtime = runtime;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try
                {
                    var random = new Random();

                    for (var i = 0; i < 1000; i++)
                    {
                        _runtime.SendEvent(new MyEventA("key1", random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventA("key2", random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventB("key1", random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventB("key2", random.Next(0, 1000000)));
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }
        }

        public class MyEventA
        {
            public MyEventA(string key, int data)
            {
                Key = key;
                Data = data;
            }

            public string Key { get; }

            public int Data { get; }
        }

        public class MyEventB
        {
            public MyEventB(string key, int data)
            {
                Key = key;
                Data = data;
            }

            public string Key { get; }

            public int Data { get; }
        }
    }
} // end of namespace