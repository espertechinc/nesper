///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class ExecMTStmtNamedWindowIterate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            SetupStmts(epService);
            TryIterate(epService, 4, 250);
            epService.EPAdministrator.DestroyAllStatements();
    
            SetupStmts(epService);
            TryIterate(epService, 2, 500);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SetupStmts(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                    "create window MyWindow#groupwin(TheString)#keepall as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
    
            epService.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                            " select symbol, volume \n" +
                            " from " + typeof(SupportMarketDataBean).FullName);
        }
    
        private void TryIterate(EPServiceProvider epService, int numThreads, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowIterateCallable(Convert.ToString(i), epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
} // end of namespace
