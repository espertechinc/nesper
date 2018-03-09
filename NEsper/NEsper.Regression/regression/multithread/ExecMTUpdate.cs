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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    ///     Test for multithread-safety (or lack thereof) for iterators: iterators fail with concurrent mods as expected
    ///     behavior
    /// </summary>
    public class ExecMTUpdate : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL("select TheString from " + typeof(SupportBean).FullName);

            var strings = new List<string>().AsSyncList();
            stmt.Events += (sender, eventArgs) => strings.Add((string) eventArgs.NewEvents[0].Get("TheString"));

            TrySend(epService, 2, 50000);

            var found = false;
            foreach (var value in strings)
            {
                if (value == "a")
                {
                    found = true;
                }
            }

            Assert.IsTrue(found);
        }

        private void TrySend(EPServiceProvider epService, int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (var i = 0; i < numThreads; i++) {
                var callable = new StmtUpdateSendCallable(i, epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            for (var i = 0; i < 50; i++)
            {
                var stmtUpd = epService.EPAdministrator.CreateEPL(
                    "update istream " + typeof(SupportBean).FullName + " set TheString='a'");
                Thread.Sleep(10);
                stmtUpd.Dispose();
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 5));

            for (var i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
} // end of namespace