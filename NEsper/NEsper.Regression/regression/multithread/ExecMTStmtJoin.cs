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
    /// <summary>Test for multithread-safety for joins.</summary>
    public class ExecMTStmtJoin : RegressionExecution {
        private static readonly string EVENT_NAME = typeof(SupportBean).FullName;
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select istream * \n" +
                    "  from " + EVENT_NAME + "(TheString='s0')#length(1000000) as s0,\n" +
                    "       " + EVENT_NAME + "(TheString='s1')#length(1000000) as s1\n" +
                    "where s0.LongPrimitive = s1.LongPrimitive\n"
            );
            TrySendAndReceive(epService, 4, stmt, 1000);
            TrySendAndReceive(epService, 2, stmt, 2000);
        }
    
        private void TrySendAndReceive(EPServiceProvider epService, int numThreads, EPStatement statement, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtJoinCallable(i, epService, statement, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statement.Text);
            }
        }
    }
} // end of namespace
