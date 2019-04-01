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
    /// <summary>Test for update listeners that route events.</summary>
    public class ExecMTStmtListenerRoute : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            TryListener(epService, 4, 500);
        }
    
        private void TryListener(EPServiceProvider epService, int numThreads, int numRoutes) {
            EPStatement stmtTrigger = epService.EPAdministrator.CreateEPL(
                    " select * from " + typeof(SupportBean).FullName);
    
            EPStatement stmtListen = epService.EPAdministrator.CreateEPL(
                    " select * from " + typeof(SupportMarketDataBean).FullName);
            var listener = new SupportMTUpdateListener();
            stmtListen.Events += listener.Update;
    
            // Set of events routed by each listener
            var routed = CompatExtensions.AsSyncSet(new HashSet<SupportMarketDataBean>());
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtListenerCreateStmtCallable(i, epService, stmtTrigger, numRoutes, routed);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // assert
            EventBean[] results = listener.GetNewDataListFlattened();
            Assert.IsTrue(results.Length >= numThreads * numRoutes);
    
            foreach (SupportMarketDataBean routedEvent in routed) {
                bool found = false;
                for (int i = 0; i < results.Length; i++) {
                    if (results[i].Underlying == routedEvent) {
                        found = true;
                    }
                }
                Assert.IsTrue(found);
            }
        }
    }
} // end of namespace
