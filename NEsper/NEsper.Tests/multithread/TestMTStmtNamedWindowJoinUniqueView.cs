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
using com.espertech.esper.client.deploy;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTStmtNamedWindowJoinUniqueView
    {
        private EPServiceProvider _service;
    
        [SetUp]
        public void SetUp()  {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType(typeof(MyEventA));
            configuration.AddEventType(typeof(MyEventB));
            _service = EPServiceProviderManager.GetDefaultProvider(configuration);
            _service.Initialize();
    
            string epl =
                    "create window A.std:unique(key) as MyEventA;\n" +
                    "create window B.std:unique(key) as MyEventB;\n" +
                    "insert into A select * from MyEventA;\n" +
                    "insert into B select * from MyEventB;\n" +
                    "\n" +
                    "@Name('stmt') select sum(A.data) as aTotal,sum(B.data) as bTotal " +
                    "from A unidirectional, B where A.key = B.key;\n";
            EPDeploymentAdmin deployment = _service.EPAdministrator.DeploymentAdmin;
            deployment.ParseDeploy(epl);
        }
    
        [Test]
        public void TestJoin() 
        {
            var es = Executors.NewFixedThreadPool(10);
            var runnables = new List<MyRunnable>();
            for (int i = 0; i < 6; i++) {
                runnables.Add(new MyRunnable(_service.EPRuntime));
            }
    
            foreach(var toRun in runnables) {
                es.Submit(toRun.Run);
            }
            es.Shutdown();
            es.AwaitTermination(TimeSpan.FromSeconds(20));
    
            foreach (MyRunnable runnable in runnables) {
                Assert.IsNull(runnable.Exception);
            }
        }
    
        public class MyRunnable
        {
            private readonly Random _random = new Random();
            private readonly EPRuntime _runtime;
            private Exception _exception;
    
            public MyRunnable(EPRuntime runtime)
            {
                _runtime = runtime;
            }
    
            public void Run()
            {
                try
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _runtime.SendEvent(new MyEventA("key1", _random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventA("key2", _random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventB("key1", _random.Next(0, 1000000)));
                        _runtime.SendEvent(new MyEventB("key2", _random.Next(0, 1000000)));
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public Exception Exception
            {
                get { return _exception; }
            }
        }
    
        public class MyEventA
        {
            public MyEventA(string key, int data) {
                Key = key;
                Data = data;
            }

            public string Key { get; private set; }

            public int Data { get; private set; }
        }
    
        public class MyEventB
        {
            public MyEventB(string key, int data)
            {
                Key = key;
                Data = data;
            }

            public string Key { get; private set; }

            public int Data { get; private set; }
        }
    }
    
}
