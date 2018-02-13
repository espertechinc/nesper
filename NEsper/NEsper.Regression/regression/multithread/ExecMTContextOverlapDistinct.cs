///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertTrue;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextOverlapDistinct : RegressionExecution {
    
        public override void Run(EPServiceProvider defaultEpService) {
            // Test uses system time
            //
            var configuration = new Configuration();
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(this.GetType().Name, configuration);
            engine.Initialize();
    
            engine.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            engine.EPAdministrator.CreateEPL("create context theContext " +
                    " initiated by Distinct(partitionKey) TestEvent as test " +
                    " terminated after 100 milliseconds");
    
            EPStatement stmt = engine.EPAdministrator.CreateEPL("context theContext " +
                    "select sum(value) as thesum, count(*) as thecnt " +
                    "from TestEvent output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            int numLoops = 2000000;
            int numEvents = numLoops * 4;
            for (int i = 0; i < numLoops; i++) {
                if (i % 100000 == 0) {
                    Log.Info("Completed: " + i);
                }
                engine.EPRuntime.SendEvent(new TestEvent("TEST", 10));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", -10));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", 25));
                engine.EPRuntime.SendEvent(new TestEvent("TEST", -25));
            }
    
            int numDeliveries = listener.NewDataList.Count;
            Log.Info("Done " + numLoops + " loops, have " + numDeliveries + " deliveries");
            Assert.IsTrue(numDeliveries > 3);
    
            Thread.Sleep(1000);
    
            int sum = 0;
            long count = 0;
            foreach (EventBean @event in listener.GetNewDataListFlattened()) {
                int? sumBatch = (int?) @event.Get("thesum");
                // Comment-Me-In: Log.Info(EventBeanUtility.Summarize(@event));
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch;
                    count += (long) @event.Get("thecnt");
                }
            }
            Log.Info("count=" + count + "  sum=" + sum);
            Assert.AreEqual(numEvents, count);
            Assert.AreEqual(0, sum);
    
            engine.Destroy();
        }
    
        public class TestEvent {
            private readonly string partitionKey;
            private readonly int value;
    
            public TestEvent(string partitionKey, int value) {
                this.partitionKey = partitionKey;
                this.value = value;
            }
    
            public string GetPartitionKey() {
                return partitionKey;
            }
    
            public int GetValue() {
                return value;
            }
        }
    }
} // end of namespace
