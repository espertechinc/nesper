///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety and named window updates.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowUpdate 
    {
        public readonly static int NUM_STRINGS = 100;
        public readonly static int NUM_INTS = 10;
    
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            EPServiceProviderManager.PurgeAllProviders();

            _engine.Initialize();
        }
    
        [Test]
        public void TestConcurrentUpdate()
        {
            TrySend(5, 10000);
        }
    
        private void TrySend(int numThreads, int numEventsPerThread)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
            _engine.Initialize();
    
            // setup statements
            _engine.EPAdministrator.CreateEPL("create window MyWindow.std:unique(TheString, IntPrimitive) as select * from SupportBean");
            _engine.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(BoolPrimitive = true)");
            _engine.EPAdministrator.CreateEPL("on SupportBean(BoolPrimitive = false) sb " +
                    "Update MyWindow win set IntBoxed = win.IntBoxed + 1, DoublePrimitive = win.DoublePrimitive + sb.DoublePrimitive" +
                    " where sb.TheString = win.TheString and sb.IntPrimitive = win.IntPrimitive");
    
            // send primer events, initialize totals
            IDictionary<MultiKeyUntyped, UpdateTotals> totals = new Dictionary<MultiKeyUntyped, UpdateTotals>();
            for (int i = 0; i < NUM_STRINGS; i++) {
                for (int j = 0; j < NUM_INTS; j++) {
                    var primer = new SupportBean(Convert.ToString(i), j);
                    primer.BoolPrimitive = true;
                    primer.IntBoxed = 0;
                    primer.DoublePrimitive = 0;
    
                    _engine.EPRuntime.SendEvent(primer);

                    var key = new MultiKeyUntyped(primer.TheString, primer.IntPrimitive);
                    totals.Put(key, new UpdateTotals(0,0));
                }
            }
    
            // execute
            long startTime = PerformanceObserver.MilliTime;

            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<StmtNamedWindowUpdateCallable.UpdateResult>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new StmtNamedWindowUpdateCallable("Thread" + i, _engine, numEventsPerThread));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
            long endTime = PerformanceObserver.MilliTime;
    
            // total up result
            long deltaCumulative = 0;
            for (int i = 0; i < numThreads; i++)
            {
                StmtNamedWindowUpdateCallable.UpdateResult result = future[i].GetValueOrDefault();
                deltaCumulative += result.Delta;
                foreach (StmtNamedWindowUpdateCallable.UpdateItem item in result.Updates) {
                    var key = new MultiKeyUntyped(item.TheString, item.Intval);
                    var total = totals.Get(key);
                    if (total == null) {
                        throw new ApplicationException("Totals not found for key " + key);
                    }
                    total.Num = total.Num + 1;
                    total.Sum = total.Sum + item.DoublePrimitive;
                }
            }
    
            // compare
            EventBean[] rows = _engine.EPRuntime.ExecuteQuery("select * from MyWindow").Array;
            Assert.AreEqual(rows.Length, totals.Count);
            long totalUpdates = 0;
            foreach (EventBean row in rows)
            {
                UpdateTotals total = totals.Get(new MultiKeyUntyped(row.Get("TheString"), row.Get("IntPrimitive")));
                Assert.AreEqual(total.Num, row.Get("IntBoxed"));
                Assert.AreEqual(total.Sum, row.Get("DoublePrimitive"));
                totalUpdates += total.Num;
            }
    
            Assert.AreEqual(totalUpdates, numThreads * numEventsPerThread);
            //long deltaTime = endTime - startTime;
            //Console.Out.WriteLine("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);
        }

        public class UpdateTotals
        {
            public UpdateTotals(int num, double sum)
            {
                Num = num;
                Sum = sum;
            }

            public int Num { get; set; }
            public double Sum { get; set; }
        }
    }
}
