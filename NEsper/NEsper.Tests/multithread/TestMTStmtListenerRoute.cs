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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for Update listeners that route events. </summary>
    [TestFixture]
    public class TestMTStmtListenerRoute 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
        }
    
        [Test]
        public void TestListenerCreateStmt()
        {
            TryListener(4, 500);
        }
    
        private void TryListener(int numThreads, int numRoutes)
        {
            EPStatement stmtTrigger = _engine.EPAdministrator.CreateEPL(
                    " select * from " + typeof(SupportBean).FullName);
    
            EPStatement stmtListen = _engine.EPAdministrator.CreateEPL(
                    " select * from " + typeof(SupportMarketDataBean).FullName);
            SupportMTUpdateListener listener = new SupportMTUpdateListener();
            stmtListen.Events += listener.Update;
    
            // Set of events routed by each listener
            ICollection<SupportMarketDataBean> routed = new HashSet<SupportMarketDataBean>().AsSyncCollection();
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtListenerCreateStmtCallable(i, _engine, stmtTrigger, numRoutes, routed);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            // assert
            EventBean[] results = listener.GetNewDataListFlattened();
            Assert.IsTrue(results.Length >= numThreads * numRoutes);
    
            foreach (var found in routed.Select(routedEvent => results.Any(eventBean => eventBean.Underlying == routedEvent)))
            {
                Assert.IsTrue(found);
            }
        }
    }
}
