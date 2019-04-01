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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety and named window updates.</summary>
    public class ExecMTStmtNamedWindowUpdate : RegressionExecution {
        public static readonly int NUM_STRINGS = 100;
        public static readonly int NUM_INTS = 10;
    
        public override void Run(EPServiceProvider epService) {
            TrySend(epService, 5, 10000);
        }
    
        private void TrySend(EPServiceProvider epService, int numThreads, int numEventsPerThread) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // setup statements
            epService.EPAdministrator.CreateEPL("create window MyWindow#unique(TheString, IntPrimitive) as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(BoolPrimitive = true)");
            epService.EPAdministrator.CreateEPL("on SupportBean(BoolPrimitive = false) sb " +
                    "update MyWindow win set IntBoxed = win.IntBoxed + 1, DoublePrimitive = win.DoublePrimitive + sb.DoublePrimitive" +
                    " where sb.TheString = win.TheString and sb.IntPrimitive = win.IntPrimitive");
    
            // send primer events, initialize totals
            var totals = new Dictionary<MultiKeyUntyped, UpdateTotals>();
            for (int i = 0; i < NUM_STRINGS; i++) {
                for (int j = 0; j < NUM_INTS; j++) {
                    var primer = new SupportBean(Convert.ToString(i), j);
                    primer.BoolPrimitive = true;
                    primer.IntBoxed = 0;
                    primer.DoublePrimitive = 0;
    
                    epService.EPRuntime.SendEvent(primer);
                    var key = new MultiKeyUntyped(primer.TheString, primer.IntPrimitive);
                    totals.Put(key, new UpdateTotals(0, 0));
                }
            }
    
            // execute
            long startTime = DateTimeHelper.CurrentTimeMillis;
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<StmtNamedWindowUpdateCallable.UpdateResult>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new StmtNamedWindowUpdateCallable("Thread" + i, epService, numEventsPerThread));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
            long endTime = DateTimeHelper.CurrentTimeMillis;
    
            // total up result
            long deltaCumulative = 0;
            for (int i = 0; i < numThreads; i++) {
                StmtNamedWindowUpdateCallable.UpdateResult result = future[i].GetValueOrDefault();
                deltaCumulative += result.Delta;
                foreach (StmtNamedWindowUpdateCallable.UpdateItem item in result.Updates) {
                    var key = new MultiKeyUntyped(item.TheString, item.Intval);
                    UpdateTotals total = totals.Get(key);
                    if (total == null) {
                        throw new EPRuntimeException("Totals not found for key " + key);
                    }
                    total.Num = total.Num + 1;
                    total.Sum = total.Sum + item.DoublePrimitive;
                }
            }
    
            // compare
            EventBean[] rows = epService.EPRuntime.ExecuteQuery("select * from MyWindow").Array;
            Assert.AreEqual(rows.Length, totals.Count);
            long totalUpdates = 0;
            foreach (EventBean row in rows) {
                UpdateTotals total = totals.Get(new MultiKeyUntyped(row.Get("TheString"), row.Get("IntPrimitive")));
                Assert.AreEqual(total.Num, row.Get("IntBoxed"));
                Assert.AreEqual(total.Sum, row.Get("DoublePrimitive"));
                totalUpdates += total.Num;
            }
    
            Assert.AreEqual(totalUpdates, numThreads * numEventsPerThread);
            //long deltaTime = endTime - startTime;
            //Log.Info("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);
        }

        public class UpdateTotals
        {
            private int num;
            private double sum;

            public UpdateTotals(int num, double sum)
            {
                this.num = num;
                this.sum = sum;
            }

            public int Num
            {
                get => num;
                set => num = value;
            }

            public double Sum
            {
                get => sum;
                set => sum = value;
            }
        }
    }
} // end of namespace
