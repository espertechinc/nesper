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
    /// Test for multithread-safety and named window subqueries and aggregation.
    /// </summary>
    public class ExecMTStmtNamedWindowSubqueryAgg : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddPlugInAggregationFunctionFactory("intListAgg", typeof(MyIntListAggregationFactory));
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
        }
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 3, 1000, false);
            epService.EPAdministrator.DestroyAllStatements();
            TrySend(epService, 3, 1000, true);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numEventsPerThread, bool indexShare) {
            // setup statements
            epService.EPAdministrator.CreateEPL("create schema UpdateEvent as (uekey string, ueint int)");
            epService.EPAdministrator.CreateEPL("create schema WindowSchema as (wskey string, wsint int)");
    
            string createEpl = "create window MyWindow#keepall as WindowSchema";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            EPStatement namedWindow = epService.EPAdministrator.CreateEPL(createEpl);
    
            epService.EPAdministrator.CreateEPL("create index ABC on MyWindow(wskey)");
            epService.EPAdministrator.CreateEPL("on UpdateEvent mue merge MyWindow mw " +
                    "where uekey = wskey and ueint = wsint " +
                    "when not matched then insert select uekey as wskey, ueint as wsint " +
                    "when matched then delete");
            // note: here all threads use the same string key to insert/delete and different values for the int
            EPStatement targetStatement = epService.EPAdministrator.CreateEPL("select (select IntListAgg(wsint) from MyWindow mw where wskey = sb.TheString) as val from SupportBean sb");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(
                    new StmtNamedWindowSubqueryAggCallable(i, epService, numEventsPerThread, targetStatement));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // total up result
            for (int i = 0; i < numThreads; i++) {
                bool result = future[i].GetValue(TimeSpan.FromSeconds(60));
                Assert.IsTrue(result);
            }
    
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(namedWindow.GetEnumerator());
            Assert.AreEqual(0, events.Length);
        }
    }
} // end of namespace
