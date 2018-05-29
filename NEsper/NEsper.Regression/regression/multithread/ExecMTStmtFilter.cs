///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class ExecMTStmtFilter : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            var plainFilter = "select count(*) as mycount from " + typeof(SupportBean).FullName;
            TryCount(epService, 2, 1000, plainFilter, GeneratorIterator.DEFAULT_SUPPORTEBEAN_CB);
            TryCount(epService, 4, 1000, plainFilter, GeneratorIterator.DEFAULT_SUPPORTEBEAN_CB);


            var vals = Collections.List("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            var enumCallback = new GeneratorIteratorCallback(numEvent =>
            {
                var bean = new SupportCollection();
                bean.Strvals = vals;
                return bean;
            });

            var enumFilter = "select count(*) as mycount from " + typeof(SupportCollection).FullName + "(Strvals.anyOf(v => v = 'j'))";
            TryCount(epService, 4, 1000, enumFilter, enumCallback);
        }
    
        public void TryCount(EPServiceProvider epService, int numThreads, int numMessages, string epl, GeneratorIteratorCallback generatorIteratorCallback) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new MTListener("mycount");
            stmt.Events += listener.Update;
    
            var future = new Future<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new SendEventCallable(i, epService, new GeneratorIterator(numMessages, generatorIteratorCallback)));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (var i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // verify results
            Assert.AreEqual(numMessages * numThreads, listener.Values.Count);
            var result = new SortedSet<int>();
            foreach (var row in listener.Values)
            {
                result.Add(row.AsInt());
            }
            Assert.AreEqual(numMessages * numThreads, result.Count);
            Assert.AreEqual(1, result.First());
            Assert.AreEqual(numMessages * numThreads, result.Last());
        }
    }
} // end of namespace
