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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of insert-into and aggregation per group. </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowConsume 
    {
        private EPServiceProvider _engine;
        private SupportMTUpdateListener _listenerWindow;
        private SupportMTUpdateListener[] _listenerConsumers;
    
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
            _listenerWindow = null;
            _listenerConsumers = null;
        }
    
        [Test]
        public void TestNamedWindow()
        {
            EPStatement stmtWindow = _engine.EPAdministrator.CreateEPL(
                    "create window MyWindow.win:keepall() as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
            _listenerWindow = new SupportMTUpdateListener();
            stmtWindow.Events += _listenerWindow.Update;
    
            _engine.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                    " select Symbol, Volume \n" +
                    " from " + typeof(SupportMarketDataBean).FullName);

            String stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " as s0 delete from MyWindow as win where win.TheString = s0.id";
            _engine.EPAdministrator.CreateEPL(stmtTextDelete);
    
            TrySend(4, 1000, 8);
        }
    
        private void TrySend(int numThreads, int numRepeats, int numConsumers)
        {
            _listenerConsumers = new SupportMTUpdateListener[numConsumers];
            for (int i = 0; i < _listenerConsumers.Length; i++)
            {
                EPStatement stmtConsumer = _engine.EPAdministrator.CreateEPL("select TheString, LongPrimitive from MyWindow");
                _listenerConsumers[i] = new SupportMTUpdateListener();
                stmtConsumer.Events += _listenerConsumers[i].Update;
            }
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<IList<string>>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtNamedWindowConsumeCallable(Convert.ToString(i), _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // Compute list of expected
            var expectedIdsList = new List<String>();
            for (int i = 0; i < numThreads; i++)
            {
                expectedIdsList.AddAll(future[i].GetValueOrDefault());
            }
            String[] expectedIds = expectedIdsList.ToArray();
    
            Assert.AreEqual(numThreads * numRepeats, _listenerWindow.GetNewDataList().Count);  // old and new each
    
            // Compute list of received
            for (int i = 0; i < _listenerConsumers.Length; i++)
            {
                var newEvents = _listenerConsumers[i].GetNewDataListFlattened();
                var receivedIds = new String[newEvents.Length];
                for (int j = 0; j < newEvents.Length; j++)
                {
                    receivedIds[j] = (String) newEvents[j].Get("TheString");
                }
                Assert.AreEqual(receivedIds.Length, expectedIds.Length);
    
                Array.Sort(receivedIds);
                Array.Sort(expectedIds);
                CompatExtensions.DeepEquals(expectedIds, receivedIds);
            }
        }
    }
}
