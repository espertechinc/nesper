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
using com.espertech.esper.client.scopetest;
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
    /// <summary>Test for multithread-safety of @priority and named windows.</summary>
    public class ExecMTStmtNamedWindowPriority : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Execution.IsPrioritized = true;
            configuration.EngineDefaults.Threading.IsInsertIntoDispatchPreserveOrder = false;
        }
    
        public override void Run(EPServiceProvider epService) {
    
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (c0 string, c1 string)");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select p00 as c0, p01 as c1 from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("@Priority(1) on SupportBean_S1 s1 merge MyWindow s0 where s1.p10 = c0 " +
                    "when matched then update set c1 = s1.p11");
            epService.EPAdministrator.CreateEPL("@Priority(0) on SupportBean_S1 s1 merge MyWindow s0 where s1.p10 = c0 " +
                    "when matched then update set c1 = s1.p12");
    
            TrySend(epService, stmtWindow, 4, 1000);
        }
    
        private void TrySend(EPServiceProvider epService, EPStatement stmtWindow, int numThreads, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowPriorityCallable(i, epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            for (int i = 0; i < numThreads; i++) {
                future[i].GetValueOrDefault();
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmtWindow.GetEnumerator());
            Assert.AreEqual(numThreads * numRepeats, events.Length);
            for (int i = 0; i < events.Length; i++) {
                string valueC1 = (string) events[i].Get("c1");
                Assert.AreEqual("y", valueC1);
            }
        }
    }
} // end of namespace
