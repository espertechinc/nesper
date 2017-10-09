///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety and named window updates.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowMerge 
    {
        public readonly static int NUM_STRINGS = 1;
        public readonly static int NUM_INTS = 1;
    
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }
    
        [Test]
        public void TestConcurrentMerge3Thread()
        {
            TrySend(3, 100);
        }
    
        [Test]
        public void TestConcurrentMerge2Thread()
        {
            TrySend(2, 1000);
        }
    
        private void TrySend(int numThreads, int numEventsPerThread)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
            _engine.Initialize();
    
            // setup statements
            _engine.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            _engine.EPAdministrator.CreateEPL("on SupportBean sb " +
                    "merge MyWindow nw where nw.TheString = sb.TheString " +
                    " when not matched then insert select * " +
                    " when matched then Update set IntPrimitive = nw.IntPrimitive + 1");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool?>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new StmtNamedWindowMergeCallable(_engine, numEventsPerThread));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // total up result
            for (int i = 0; i < numThreads; i++)
            {
                bool? result = future[i].GetValueOrDefault();
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Value);
            }
    
            // compare
            EventBean[] rows = _engine.EPRuntime.ExecuteQuery("select * from MyWindow").Array;
            Assert.AreEqual(numEventsPerThread, rows.Length);
            foreach (EventBean row in rows)
            {
                Assert.AreEqual(numThreads - 1, row.Get("IntPrimitive"));
            }
            //long deltaTime = endTime - startTime;
            //Console.Out.WriteLine("Totals updated: " + totalUpdates + "  Delta cumu: " + deltaCumulative + "  Delta pooled: " + deltaTime);
        }
    }
}
