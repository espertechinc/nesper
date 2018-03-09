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
    /// <summary>
    /// Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class ExecMTStmtInsertInto : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                    "insert into XStream " +
                            " select TheString as key, count(*) as mycount\n" +
                            " from " + typeof(SupportBean).FullName + "#time(5 min)" +
                            " group by TheString"
            );
            epService.EPAdministrator.CreateEPL(
                    "insert into XStream " +
                            " select symbol as key, count(*) as mycount\n" +
                            " from " + typeof(SupportMarketDataBean).FullName + "#time(5 min)" +
                            " group by symbol"
            );
    
            EPStatement stmtConsolidated = epService.EPAdministrator.CreateEPL("select key, mycount from XStream");
            var listener = new SupportMTUpdateListener();
            stmtConsolidated.Events += listener.Update;
    
            TrySend(epService, listener, 10, 5000);
            TrySend(epService, listener, 4, 10000);
        }
    
        private void TrySend(EPServiceProvider epService, SupportMTUpdateListener listener, int numThreads, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtInsertIntoCallable(Convert.ToString(i), epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // Assert results
            int totalExpected = numThreads * numRepeats * 2;
            EventBean[] result = listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, result.Length);
            var results = new LinkedHashMap<long, ISet<string>>();
            foreach (EventBean theEvent in result) {
                long count = (long) theEvent.Get("mycount");
                string key = (string) theEvent.Get("key");
    
                ISet<string> entries = results.Get(count);
                if (entries == null) {
                    entries = new HashSet<string>();
                    results.Put(count, entries);
                }
                entries.Add(key);
            }
    
            Assert.AreEqual(numRepeats, results.Count);
            foreach (ISet<string> value in results.Values) {
                Assert.AreEqual(2 * numThreads, value.Count);
                for (int i = 0; i < numThreads; i++) {
                    Assert.IsTrue(value.Contains("E1_" + i));
                    Assert.IsTrue(value.Contains("E2_" + i));
                }
            }
    
            listener.Reset();
        }
    }
} // end of namespace
