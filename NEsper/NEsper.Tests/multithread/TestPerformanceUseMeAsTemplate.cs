///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestPerformanceUseMeAsTemplate
    {
        [Test]
        public void TestPerformance()
        {
            int numEvents = 1;
            int numThreads = 2;
    
            Configuration config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder = false;
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider(config);
    
            engine.EPAdministrator.Configuration.AddEventType(typeof(TransactionEvent));
            engine.EPAdministrator.Configuration.AddPlugInSingleRowFunction("MyDynModel", GetType().FullName, "MyDynModel");
    
            String epl = "select MyDynModel({Col_001, Col_002, Col_003}) as model_score from TransactionEvent";
            EPStatement stmt = engine.EPAdministrator.CreateEPL(epl);
            stmt.Subscriber = new MySubscriber();
    
            var queue = new LinkedBlockingQueue<Runnable>();
            var latch = new CountDownLatch(numEvents);
            for (int i = 0; i < numEvents; i++) {
                queue.Push((new MyRunnable(engine.EPRuntime, latch, new TransactionEvent(1,2,3))).Run);
            }

            var threads = Executors.NewFixedThreadPool(numThreads);

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int ii = 0; ii < numThreads; ii++)
                    {
                        threads.Submit(
                            () =>
                            {
                                Runnable runnable;
                                while (queue.Pop(0, out runnable))
                                {
                                    runnable.Invoke();
                                }
                            });
                    }


                    //ThreadPoolExecutor threads = new ThreadPoolExecutor(numThreads, numThreads, 10, TimeUnit.SECONDS, queue);
                    //threads.PrestartAllCoreThreads();
                    latch.Await(TimeSpan.FromMinutes(1));
                    if (latch.Count > 0)
                    {
                        throw new Exception("Failed to complete in 1 minute");
                    }
                });

            Console.WriteLine("Took " + delta + " millis");
            threads.Shutdown();
            threads.AwaitTermination(TimeSpan.FromSeconds(10));
        }
    
        public static int MyDynModel(int?[] args)
        {
            try {
                Thread.Sleep(1);
            }
            catch (ThreadInterruptedException e) {
                Console.Error.WriteLine(e.StackTrace);
            }
            return 0;
        }
    
        public class MySubscriber
        {
            public void Update(Object[] args) {}
        }
    
        public class MyRunnable
        {
            private readonly EPRuntime _runtime;
            private readonly CountDownLatch _latch;
            private readonly Object _event;
    
            public MyRunnable(EPRuntime runtime, CountDownLatch latch, Object @event)
            {
                _runtime = runtime;
                _latch = latch;
                _event = @event;
            }
    
            public void Run()
            {
                try
                {
                    _runtime.SendEvent(_event);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
                finally
                {
                    _latch.CountDown();
                }
            }
        }
    
        public class TransactionEvent
        {
            public TransactionEvent(int col_001, int col_002, int col_003)
            {
                Col_001 = col_001;
                Col_002 = col_002;
                Col_003 = col_003;
            }

            public int Col_001 { get; private set; }

            public int Col_002 { get; private set; }

            public int Col_003 { get; private set; }
        }
    }
}
