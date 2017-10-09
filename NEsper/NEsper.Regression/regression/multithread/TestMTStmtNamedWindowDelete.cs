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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety of insert-into and aggregation per group. </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowDelete 
    {
        private EPServiceProvider _engine;
        private SupportMTUpdateListener _listenerWindow;
        private SupportMTUpdateListener _listenerConsumer;
    
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
            _listenerConsumer = null;
        }
    
        [Test]
        public void TestNamedWindow()
        {
            EPStatement stmtWindow = _engine.EPAdministrator.CreateEPL(
                    "create window MyWindow#keepall as select TheString, LongPrimitive from " + typeof(SupportBean).FullName);
            _listenerWindow = new SupportMTUpdateListener();
            stmtWindow.Events += _listenerWindow.Update;
    
            _engine.EPAdministrator.CreateEPL(
                    "insert into MyWindow(TheString, LongPrimitive) " +
                    " select Symbol, Volume \n" +
                    " from " + typeof(SupportMarketDataBean).FullName);

            String stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " as s0 delete from MyWindow as win where win.TheString = s0.Id";
            _engine.EPAdministrator.CreateEPL(stmtTextDelete);

            EPStatement stmtConsumer = _engine.EPAdministrator.CreateEPL("select irstream TheString, LongPrimitive from MyWindow");
            _listenerConsumer = new SupportMTUpdateListener();
            stmtConsumer.Events += _listenerConsumer.Update;
    
            TrySend(4, 1000);
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<IList<string>>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtNamedWindowDeleteCallable(Convert.ToString(i), _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // Compute list of expected
            List<String> expectedIdsList = new List<String>();
            for (int i = 0; i < numThreads; i++)
            {
                expectedIdsList.AddAll(future[i].GetValueOrDefault());
            }
            String[] expectedIds = expectedIdsList.ToArray();
    
            Assert.AreEqual(2 * numThreads * numRepeats, _listenerWindow.GetNewDataList().Count);  // old and new each
            Assert.AreEqual(2 * numThreads * numRepeats, _listenerConsumer.GetNewDataList().Count);  // old and new each
    
            // Compute list of received
            EventBean[] newEvents = _listenerWindow.GetNewDataListFlattened();
            String[] receivedIds = new String[newEvents.Length];
            for (int i = 0; i < newEvents.Length; i++)
            {
                receivedIds[i] = (String) newEvents[i].Get("TheString");
            }
            Assert.AreEqual(receivedIds.Length, expectedIds.Length);
    
            Array.Sort(receivedIds);
            Array.Sort(expectedIds);

            CompatExtensions.DeepEquals(expectedIds, receivedIds);
        }
    }
}
