///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextOverlapDistinct : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider defaultEpService) {
            // Test uses system time
            //
            var configuration = new Configuration(SupportContainer.Instance);
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, this.GetType().Name, configuration);
            engine.Initialize();
    
            engine.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            engine.EPAdministrator.CreateEPL(
                "create context theContext " +
                " initiated by distinct(PartitionKey) TestEvent as test " +
                " terminated after 100 milliseconds");
    
            EPStatement stmt = engine.EPAdministrator.CreateEPL(
                "context theContext " +
                "select sum(Value) as thesum, count(*) as thecnt " +
                "from TestEvent output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
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
                int? sumBatch = @event.Get("thesum").AsBoxedInt();
                // Comment-Me-In: Log.Info(EventBeanUtility.Summarize(@event));
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += (long) @event.Get("thecnt");
                }
            }
            Log.Info("count=" + count + "  sum=" + sum);
            Assert.AreEqual(numEvents, count);
            Assert.AreEqual(0, sum);
    
            engine.Dispose();
        }
    
        public class TestEvent {
            public TestEvent(string partitionKey, int value) {
                this.PartitionKey = partitionKey;
                this.Value = value;
            }

            public string PartitionKey { get; }

            public int Value { get; }
        }
    }
} // end of namespace
