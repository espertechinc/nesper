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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of a time window -based statement. </summary>
    [TestFixture]
    public class TestMTStmtTimeWindow 
    {
        private EPServiceProvider _engine;
        private SupportMTUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtTimeWindow", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _engine.Dispose();
        }
    
        [Test]
        public void TestTimeWin()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL(
                    " select irstream IntPrimitive, TheString as key " +
                    " from " + typeof(SupportBean).FullName + ".win:time(1 sec)");
    
            _listener = new SupportMTUpdateListener();
            stmt.Events += _listener.Update;
    
            TrySend(10, 5000);
            TrySend(6, 2000);
            TrySend(2, 10000);
            TrySend(3, 5000);
            TrySend(5, 2500);
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            // set time to 0
            _engine.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new SendEventCallable(i, _engine, EventGenerator.MakeEvents(numRepeats));
                future[i] = threadPool.Submit(callable);
            }
    
            // Advance time window every 100 milliseconds for 1 second
            for (int i = 0; i < 10; i++)
            {
                _engine.EPRuntime.SendEvent(new CurrentTimeEvent(i * 1000));
                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault().AsBoolean());
            }
    
            // set time to a large value
            _engine.EPRuntime.SendEvent(new CurrentTimeEvent(10000000000L));
    
            // Assert results
            int totalExpected = numThreads * numRepeats;
    
            // assert new data
            EventBean[] resultNewData = _listener.GetNewDataListFlattened();
            Assert.AreEqual(totalExpected, resultNewData.Length);
            IDictionary<int, IList<String>> resultsNewData = SortPerIntKey(resultNewData);
            AssertResult(numRepeats, numThreads, resultsNewData);
    
            // assert old data
            EventBean[] resultOldData = _listener.GetOldDataListFlattened();
            Assert.AreEqual(totalExpected, resultOldData.Length);
            IDictionary<int, IList<String>> resultsOldData = SortPerIntKey(resultOldData);
            AssertResult(numRepeats, numThreads, resultsOldData);
    
            _listener.Reset();
        }

        private IDictionary<int, IList<String>> SortPerIntKey(EventBean[] result)
        {
            IDictionary<int, IList<String>> results = new LinkedHashMap<int, IList<String>>();
            foreach (EventBean theEvent in result)
            {
                var count = (int) theEvent.Get("IntPrimitive");
                var key = (String) theEvent.Get("key");
    
                var entries = results.Get(count);
                if (entries == null)
                {
                    entries = new List<String>();
                    results.Put(count, entries);
                }
                entries.Add(key);
            }
            return results;
        }
    
        // Each integer value must be there with 2 entries of the same value
        private void AssertResult(int numRepeats, int numThreads, IDictionary<int, IList<String>> results)
        {
            for (int i = 0; i < numRepeats; i++)
            {
                var values = results.Get(i);
                Assert.AreEqual(numThreads, values.Count);
                foreach (String value in values)
                {
                    Assert.AreEqual(Convert.ToString(i), value);
                }
            }
        }
    }
}
