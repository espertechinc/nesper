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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextNestedNonOverlapAtNow : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider defaultEpService) {
            var configuration = new Configuration(SupportContainer.Instance);
            var epService = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, GetType().FullName, configuration);
            epService.Initialize();
            Thread.Sleep(100); // allow time for start up
    
            epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            epService.EPAdministrator.CreateEPL("create context theContext " +
                    "context perPartition partition by PartitionKey from TestEvent," +
                    "context per10Seconds start @now end after 100 milliseconds");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context theContext " +
                    "select sum(Value) as thesum, count(*) as thecnt, context.perPartition.key1 as thekey " +
                    "from TestEvent output snapshot when terminated");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            int numLoops = 200000;
            int numEvents = numLoops * 4;
            for (int i = 0; i < numLoops; i++) {
                if (i % 100000 == 0) {
                    Log.Info("Completed: " + i);
                }
                epService.EPRuntime.SendEvent(new TestEvent("TEST", 10));
                epService.EPRuntime.SendEvent(new TestEvent("TEST", -10));
                epService.EPRuntime.SendEvent(new TestEvent("TEST", 25));
                epService.EPRuntime.SendEvent(new TestEvent("TEST", -25));
            }
    
            Thread.Sleep(250);
    
            int numDeliveries = listener.NewDataList.Count;
            Assert.IsTrue(numDeliveries >= 2, "Done " + numLoops + " loops, have " + numDeliveries + " deliveries");
    
            int sum = 0;
            long count = 0;
            foreach (EventBean @event in listener.GetNewDataListFlattened()) {
                int? sumBatch = (int?) @event.Get("thesum");
                if (sumBatch != null) { // can be null when there is nothing to deliver
                    sum += sumBatch.Value;
                    count += (long) @event.Get("thecnt");
                }
            }
            Assert.AreEqual(0, sum);
            Assert.AreEqual(numEvents, count);
            epService.Dispose();
        }
    
        public class TestEvent {
            private readonly string partitionKey;
            private readonly int value;
    
            public TestEvent(string partitionKey, int value) {
                this.partitionKey = partitionKey;
                this.value = value;
            }

            public string PartitionKey => partitionKey;

            public int Value => value;
        }
    }
} // end of namespace
