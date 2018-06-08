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
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of insert-into and aggregation per group.
    /// </summary>
    public class ExecMTStmtNamedWindowDelete : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL(
                    "create window MyWindow#keepall as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
            var listenerWindow = new SupportMTUpdateListener();
            stmtWindow.Events += listenerWindow.Update;
    
            epService.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                            " select symbol, volume \n" +
                            " from " + typeof(SupportMarketDataBean).FullName);
    
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " as s0 delete from MyWindow as win where win.TheString = s0.id";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            EPStatement stmtConsumer = epService.EPAdministrator.CreateEPL("select irstream TheString, LongPrimitive from MyWindow");
            var listenerConsumer = new SupportMTUpdateListener();
            stmtConsumer.Events += listenerConsumer.Update;
    
            TrySend(epService, listenerWindow, listenerConsumer, 4, 1000);
        }
    
        private void TrySend(EPServiceProvider epService, SupportMTUpdateListener listenerWindow, SupportMTUpdateListener listenerConsumer, int numThreads, int numRepeats) {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<IList<string>>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtNamedWindowDeleteCallable(Convert.ToString(i), epService, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            // compute list of expected
            var expectedIdsList = new List<string>();
            for (int i = 0; i < numThreads; i++) {
                expectedIdsList.AddAll(future[i].GetValueOrDefault());
            }
            string[] expectedIds = expectedIdsList.ToArray();
    
            Assert.AreEqual(2 * numThreads * numRepeats, listenerWindow.NewDataList.Count);  // old and new each
            Assert.AreEqual(2 * numThreads * numRepeats, listenerConsumer.NewDataList.Count);  // old and new each
    
            // compute list of received
            EventBean[] newEvents = listenerWindow.GetNewDataListFlattened();
            var receivedIds = new string[newEvents.Length];
            for (int i = 0; i < newEvents.Length; i++) {
                receivedIds[i] = (string) newEvents[i].Get("TheString");
            }
            Assert.AreEqual(receivedIds.Length, expectedIds.Length);
    
            Array.Sort(receivedIds);
            Array.Sort(expectedIds);
            CompatExtensions.DeepEquals(expectedIds, receivedIds);
        }
    }
} // end of namespace
