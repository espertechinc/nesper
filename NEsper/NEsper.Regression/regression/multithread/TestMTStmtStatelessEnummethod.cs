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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for a simple aggregation case using count(*).
    /// </summary>
    [TestFixture]
    public class TestMTStmtStatelessEnummethod 
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
        public void TestStatelessEnmeration()
        {
            ICollection<String> vals = Collections.List("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            GeneratorIteratorCallback enumCallback = numEvent =>
            {
                var bean = new SupportCollection { Strvals = vals };
                return bean;
            };
    
            String enumFilter = "select Strvals.anyOf(v => v = 'j') from " + typeof(SupportCollection).FullName;
            TryCount(4, 1000, enumFilter, enumCallback);
        }
    
        public void TryCount(int numThreads, int numMessages, String epl, GeneratorIteratorCallback generatorIteratorCallback)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var stmt = _engine.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new SendEventCallable(i, _engine, EventGenerator.MakeEvents(numMessages, generatorIteratorCallback)));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault().AsBoolean());
            }

            Assert.AreEqual(numMessages * numThreads, listener.GetNewDataListFlattened().Length);
        }
    }
}
