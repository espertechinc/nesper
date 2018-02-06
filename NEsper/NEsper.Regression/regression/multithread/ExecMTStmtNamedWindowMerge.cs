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


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety and named window updates.</summary>
    public class ExecMTStmtNamedWindowMerge : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 3, 100);
            epService.EPAdministrator.DestroyAllStatements();
            TrySend(epService, 2, 1000);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numEventsPerThread) {
            // setup statements
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean sb " +
                    "merge MyWindow nw where nw.TheString = sb.TheString " +
                    " when not matched then insert select * " +
                    " when matched then update set IntPrimitive = nw.IntPrimitive + 1");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool?>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new StmtNamedWindowMergeCallable(epService, numEventsPerThread));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            // total up result
            for (int i = 0; i < numThreads; i++) {
                bool result = future[i].GetValueOrDefault().GetValueOrDefault();
                Assert.IsTrue(result);
            }
    
            // compare
            EventBean[] rows = epService.EPRuntime.ExecuteQuery("select * from MyWindow").Array;
            Assert.AreEqual(numEventsPerThread, rows.Length);
            foreach (EventBean row in rows) {
                Assert.AreEqual(numThreads - 1, row.Get("IntPrimitive"));
            }
            //long deltaTime = endTime - startTime;
            //Log.Info("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);
        }
    }
} // end of namespace
