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
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of a lookup statement. </summary>
    [TestFixture]
    public class TestMTStmtSubquery 
    {
        private EPServiceProvider _engine;
        private SupportMTUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtSubquery", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _engine.Dispose();
        }
    
        [Test]
        public void TestSubquery()
        {
            TrySend(4, 10000);
            TrySend(3, 10000);
            TrySend(2, 10000);
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL(
                    "select (select id from S0.win:length(1000000) where id = s1.id) as value from S1 as s1");
    
            _listener = new SupportMTUpdateListener();
            stmt.Events += _listener.Update;
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtSubqueryCallable(i, _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // Assert results
            int totalExpected = numThreads * numRepeats;
    
            // assert new data
            EventBean[] resultNewData = _listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, resultNewData.Length);
    
            ICollection<int> values = new HashSet<int>();
            foreach (EventBean theEvent in resultNewData)
            {
                values.Add(theEvent.Get("value").AsInt());
            }
            Assert.AreEqual(totalExpected, values.Count, "Unexpected duplicates");
    
            _listener.Reset();
            stmt.Stop();
        }
    }
}
