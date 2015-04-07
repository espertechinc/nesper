///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTContextNestedNonOverlapAtNow
    {
        [Test]
        public void TestContextMultistmt()
        {
            // Test uses system time
            //
            var configuration = new Configuration();
            var engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            engine.Initialize();
    
            engine.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            engine.EPAdministrator.CreateEPL("create context theContext " +
                    "context perPartition partition by PartitionKey from TestEvent," +
                    "context per10Seconds start @now end after 100 milliseconds");
    
            var stmt = engine.EPAdministrator.CreateEPL("context theContext " +
                    "select sum(Value) as thesum, count(*) as thecnt, context.perPartition.key1 as thekey " +
                    "from TestEvent output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var numLoops = 200000;
            var numEvents = numLoops * 4;
            for (var i = 0; i < numLoops; i++) {
                if (i%100000 == 0) {
                    Console.WriteLine("Completed: " + i);
                }
                engine.EPRuntime.SendEvent(new TestEvent("TEST", 10));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", -10));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", 25));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", -25));
            }
    
            var numDeliveries = listener.NewDataList.Count;
            Console.WriteLine("Done " + numLoops + " loops, have " + numDeliveries + " deliveries");
            Assert.IsTrue(numDeliveries > 3);
    
            Thread.Sleep(250);
    
            var sum = 0;
            long count = 0;
            foreach (var @event in listener.GetNewDataListFlattened())
            {
                var sumBatch = (int?) @event.Get("thesum");
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += @event.Get("thecnt").AsLong();
                }
            }
            Assert.AreEqual(0, sum);
            Assert.AreEqual(numEvents, count);
        }
    
        internal class TestEvent
        {
            public TestEvent(string partitionKey, int value)
            {
                PartitionKey = partitionKey;
                Value = value;
            }

            public string PartitionKey { get; private set; }

            public int Value { get; private set; }
        }
    }
}
