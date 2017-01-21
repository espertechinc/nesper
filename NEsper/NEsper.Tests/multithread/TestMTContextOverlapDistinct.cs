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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTContextOverlapDistinct
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
                    " initiated by distinct(PartitionKey) TestEvent as test " +
                    " terminated after 100 milliseconds");
    
            var stmt = engine.EPAdministrator.CreateEPL("context theContext " +
                    "select sum(Value) as thesum, count(*) as thecnt " +
                    "from TestEvent output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var numLoops = 2000000;
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
    
            int numDeliveries = listener.NewDataList.Count;
            Console.WriteLine("Done " + numLoops + " loops, have " + numDeliveries + " deliveries");
            Assert.IsTrue(numDeliveries > 3);
    
            Thread.Sleep(1000);
    
            var sum = 0;
            long count = 0;
            foreach (EventBean @event in listener.GetNewDataListFlattened())
            {
                var sumBatch = @event.Get("thesum").AsBoxedInt();
                // Comment-Me-In: Console.WriteLine(EventBeanUtility.summarize(event));
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += @event.Get("thecnt").AsLong();
                }
            }
            Console.WriteLine("count=" + count + "  sum=" + sum);
            Assert.AreEqual(numEvents, count);
            Assert.AreEqual(0, sum);
        }
    
        public class TestEvent
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
