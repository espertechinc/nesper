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
    /// <summary>
    /// Test for multithread-safety for adding and removing listener.
    /// </summary>
    public class ExecMTStmtListenerAddRemove : RegressionExecution {
        private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).FullName;
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.ListenerDispatchTimeout = Int64.MaxValue;
        }
    
        public override void Run(EPServiceProvider epService) {
            int numThreads = 2;
    
            EPStatement stmt = epService.EPAdministrator.CreatePattern("every a=" + EVENT_NAME + "(symbol='IBM')");
            TryStatementListenerAddRemove(epService, numThreads, stmt, false, 10000);
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select * from " + EVENT_NAME + " (symbol='IBM', feed='RT')");
            TryStatementListenerAddRemove(epService, numThreads, stmt, true, 10000);
            stmt.Dispose();
        }
    
        private void TryStatementListenerAddRemove(EPServiceProvider epService, int numThreads, EPStatement statement, bool isEPL, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtListenerAddRemoveCallable(epService, statement, isEPL, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed stmt=" + statement.Text);
            }
        }
    }
} // end of namespace
