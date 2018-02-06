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
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety of insert-into and aggregation per group. </summary>
    [TestFixture]
    public class TestMTStmtInsertInto 
    {
        private EPServiceProvider _engine;
        private SupportMTUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestInsertInto()
        {
            _engine.EPAdministrator.CreateEPL(
                    "insert into XStream " +
                    " select TheString as key, count(*) as mycount\n" +
                    " from " + typeof(SupportBean).FullName + "#time(5 min)" +
                    " group by TheString"
                    );
            _engine.EPAdministrator.CreateEPL(
                    "insert into XStream " +
                    " select symbol as key, count(*) as mycount\n" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(5 min)" +
                    " group by symbol"
                    );
            
            EPStatement stmtConsolidated = _engine.EPAdministrator.CreateEPL("select key, mycount from XStream");
            _listener = new SupportMTUpdateListener();
            stmtConsolidated.Events += _listener.Update;
    
            TrySend(10, 5000);
            TrySend(4, 10000);
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtInsertIntoCallable(Convert.ToString(i), _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // Assert results
            int totalExpected = numThreads * numRepeats * 2;
            EventBean[] result = _listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, result.Length);
            var results = new LinkedHashMap<long, ICollection<String>>();
            foreach (EventBean theEvent in result)
            {
                var count = theEvent.Get("mycount").AsLong();
                var key = (String) theEvent.Get("key");
    
                ICollection<String> entries = results.Get(count);
                if (entries == null)
                {
                    entries = new HashSet<String>();
                    results.Put(count, entries);
                }
                entries.Add(key);
            }
    
            Assert.AreEqual(numRepeats, results.Count);
            foreach (ICollection<String> value in results.Values)
            {
                Assert.AreEqual(2 * numThreads, value.Count);
                for (int i = 0; i < numThreads; i++)
                {
                    Assert.IsTrue(value.Contains("E1_" + i));
                    Assert.IsTrue(value.Contains("E2_" + i));
                }            
            }
    
            _listener.Reset();
        }
    }
}
