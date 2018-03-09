///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
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
    /// Test for multithread-safety of named windows and fire-and-forget queries.
    /// </summary>
    public class ExecMTStmtNamedWindowFAF : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                    "create window MyWindow#keepall as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
    
            epService.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                            " select symbol, volume \n" +
                            " from " + typeof(SupportMarketDataBean).FullName);
    
            TryIterate(epService, 2, 500);
        }
    
        private void TryIterate(EPServiceProvider epService, int numThreads, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowQueryCallable(Convert.ToString(i), epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            Thread.Sleep(100);
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValue(10, TimeUnit.SECONDS));
            }
        }
    }
} // end of namespace
