///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety for a simple aggregation case using count(*). </summary>
    [TestFixture]
    public class TestMTStmtFilter 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtFilter", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestCount()
        {
            String plainFilter = "select count(*) as mycount from " + typeof(SupportBean).FullName;
            TryCount(2, 1000, plainFilter, EventGenerator.DEFAULT_SUPPORTEBEAN_CB);
            TryCount(4, 1000, plainFilter, EventGenerator.DEFAULT_SUPPORTEBEAN_CB);

            var vals = Collections.List("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            GeneratorIteratorCallback enumCallback = numEvent =>
            {
                var bean = new SupportCollection();
                bean.Strvals = vals;
                return bean;
            };

            String enumFilter = "select count(*) as mycount from " + typeof(SupportCollection).FullName + "(Strvals.anyOf(v => v = 'j'))";
            TryCount(4, 1000, enumFilter, enumCallback);

        }
    
        public void TryCount(int numThreads, int numMessages, String epl, GeneratorIteratorCallback generatorCallback)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var stmt = _engine.EPAdministrator.CreateEPL(epl);
            var listener = new MTListener("mycount");
            stmt.Events += listener.Update;
    
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new SendEventCallable(i, _engine, EventGenerator.MakeEvents(numMessages, generatorCallback)));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault().AsBoolean());
            }
    
            // verify results
            Assert.AreEqual(numMessages * numThreads, listener.Values.Count);
            SortedSet<int> result = new SortedSet<int>();
            foreach (Object row in listener.Values)
            {
                result.Add(row.AsInt());
            }
            Assert.AreEqual(numMessages * numThreads, result.Count);
            Assert.AreEqual(1, (Object) result.First());
            Assert.AreEqual(numMessages * numThreads, (Object) result.Last());
        }
    }
}
