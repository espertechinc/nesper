///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    ///     Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    public class ExecMTStmtStatelessEnummethod : RegressionExecution
    {
        private readonly ICollection<string> _vals = new List<string>(
            new string[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j"});

        public override void Run(EPServiceProvider epService)
        {
            var enumCallback = new GeneratorIteratorCallback(
                numEvent =>
                {
                    var bean = new SupportCollection();
                    bean.Strvals = _vals;
                    return bean;
                });

            var enumFilter = "select Strvals.anyOf(v => v = 'j') from " + typeof(SupportCollection).FullName;
            TryCount(epService, 4, 1000, enumFilter, enumCallback);
        }

        private void TryCount(
            EPServiceProvider epService, int numThreads, int numMessages, string epl,
            GeneratorIteratorCallback generatorIteratorCallback)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var future = new Future<bool>[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(
                    new SendEventCallable(
                        i, epService, new GeneratorIterator(numMessages, generatorIteratorCallback)));
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);

            for (var i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }

            Assert.AreEqual(numMessages * numThreads, listener.GetNewDataListFlattened().Length);
        }
    }
} // end of namespace