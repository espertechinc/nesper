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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety of a lookup statement.</summary>
    public class ExecMTStmtSubquery : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 4, 10000);
            TrySend(epService, 3, 10000);
            TrySend(epService, 2, 10000);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numRepeats) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select (select id from S0#length(1000000) where id = s1.id) as value from S1 as s1");
    
            var listener = new SupportMTUpdateListener();
            stmt.Events += listener.Update;
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtSubqueryCallable(i, epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // Assert results
            int totalExpected = numThreads * numRepeats;
    
            // assert new data
            EventBean[] resultNewData = listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, resultNewData.Length);
    
            var values = new HashSet<int?>();
            foreach (EventBean theEvent in resultNewData) {
                values.Add((int?) theEvent.Get("value"));
            }
            Assert.AreEqual(totalExpected, values.Count, "Unexpected duplicates");
    
            listener.Reset();
            stmt.Stop();
        }
    }
} // end of namespace
