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
using com.espertech.esper.client.util;
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
    /// Test for multithread-safety and named window subqueries and direct index-based lookup.
    /// </summary>
    public class ExecMTStmtNamedWindowSubqueryLookup : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 3, 10000);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numEventsPerThread) {
            // setup statements
            epService.EPAdministrator.CreateEPL("create schema MyUpdateEvent as (key string, intupd int)");
            epService.EPAdministrator.CreateEPL("create schema MySchema as (TheString string, intval int)");
            EPStatement namedWindow = epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MySchema");
            epService.EPAdministrator.CreateEPL("on MyUpdateEvent mue merge MyWindow mw " +
                    "where mw.TheString = mue.key " +
                    "when not matched then insert select key as TheString, intupd as intval " +
                    "when matched then delete");
            EPStatement targetStatement = epService.EPAdministrator.CreateEPL("select (select intval from MyWindow mw where mw.TheString = sb.TheString) as val from SupportBean sb");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new StmtNamedWindowSubqueryLookupCallable(i, epService, numEventsPerThread, targetStatement));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            // total up result
            for (int i = 0; i < numThreads; i++) {
                bool? result = future[i].GetValueOrDefault();
                Assert.IsTrue(result);
            }
    
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(namedWindow.GetEnumerator());
            Assert.AreEqual(0, events.Length);
        }
    }
} // end of namespace
