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
    public class MultithreadContextNestedNonOverlapAtNow
    {
        public void Run(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Common.AddEventType(typeof(TestEvent));

            var runtime = EPRuntimeProvider.GetRuntime(GetType().Name, configuration);
            runtime.Initialize();
            ThreadSleep(100); // allow time for start up

            var path = new RegressionPath();
            var eplContext = "create context theContext " +
                             "context perPartition partition by PartitionKey from TestEvent," +
                             "context per10Seconds start @now end after 100 milliseconds";
            var compiledContext = Compile(eplContext, configuration, path);
            path.Add(compiledContext);
            Deploy(compiledContext, runtime);

            var eplStmt = "context theContext " +
                          "select sum(Value) as thesum, count(*) as thecnt, context.perPartition.key1 as thekey " +
                          "from TestEvent output snapshot when terminated";
            var compiledStmt = Compile(eplStmt, configuration, path);
            var listener = new SupportUpdateListener();
            DeployAddListener(compiledStmt, "s0", listener, runtime);

            var numLoops = 200000;
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

            ThreadSleep(250);

            var numDeliveries = listener.NewDataList.Count;
            Assert.IsTrue(numDeliveries >= 2, "Done " + numLoops + " loops, have " + numDeliveries + " deliveries");

            var sum = 0;
            long count = 0;
            foreach (var @event in listener.NewDataListFlattened) {
                var sumBatch = @event.Get("thesum").AsBoxedInt32();
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += @event.Get("thecnt").AsInt64();
                }
            }

            Assert.AreEqual(0, sum);
            Assert.AreEqual(numEvents, count);
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