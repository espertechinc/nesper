///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextOverlapDistinct : RegressionExecutionPreConfigured
    {
        private readonly Configuration _configuration;

        public MultithreadContextOverlapDistinct(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            // Test uses system time
            //
            _configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            _configuration.Common.AddEventType(typeof(TestEvent));

            var runtimeProvider = new EPRuntimeProvider();
            var runtime = runtimeProvider.GetRuntimeInstance(GetType().Name, _configuration);
            runtime.Initialize();

            var path = new RegressionPath();
            var eplCtx =
                "@name('ctx') @public create context theContext " +
                " initiated by distinct(PartitionKey) TestEvent as test " +
                " terminated after 100 milliseconds";
            var compiledContext = Compile(eplCtx, _configuration, path);
            path.Add(compiledContext);
            Deploy(compiledContext, runtime);

            var eplStmt = "context theContext " +
                          "select sum(Value) as thesum, count(*) as thecnt " +
                          "from TestEvent output snapshot when terminated";
            var compiledStmt = Compile(eplStmt, _configuration, path);
            var listener = new SupportUpdateListener();
            DeployAddListener(compiledStmt, "s0", listener, runtime);

            var numLoops = 2000000;
            var numEvents = numLoops * 4;
            for (var i = 0; i < numLoops; i++) {
                if (i % 100000 == 0) {
                    Console.Out.WriteLine("Completed: " + i);
                }

                runtime.EventService.SendEventBean(new TestEvent("TEST", 10), "TestEvent");
                runtime.EventService.SendEventBean(new TestEvent("TEST", -10), "TestEvent");
                runtime.EventService.SendEventBean(new TestEvent("TEST", 25), "TestEvent");
                runtime.EventService.SendEventBean(new TestEvent("TEST", -25), "TestEvent");
            }

            var numDeliveries = listener.NewDataList.Count;
            Console.Out.WriteLine("Done " + numLoops + " loops, have " + numDeliveries + " deliveries");
            Assert.IsTrue(numDeliveries > 3);

            ThreadSleep(1000);

            var sum = 0;
            long count = 0;
            foreach (var @event in listener.NewDataListFlattened) {
                var sumBatch = @event.Get("thesum").AsBoxedInt32();
                // Comment-Me-In: Console.WriteLine(EventBeanUtility.summarize(event));
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += @event.Get("thecnt").AsInt64();
                }
            }

            Console.Out.WriteLine($"count={count}  sum={sum}");
            Assert.AreEqual(numEvents, count);
            Assert.AreEqual(0, sum);

            runtime.Destroy();
        }

        public class TestEvent
        {
            public TestEvent(
                string partitionKey,
                int value)
            {
                PartitionKey = partitionKey;
                Value = value;
            }

            public string PartitionKey { get; }

            public int Value { get; }
        }
    }
} // end of namespace