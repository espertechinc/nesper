///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;


namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    [TestFixture]
    public class TestMTDeterminismListener 
    {
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }
    
        [Test]
        public void TestOrderedDeliverySuspend()
        {
            TrySend(4, 10000, true, ConfigurationEngineDefaults.Threading.Locking.SUSPEND);
        }
    
        [Test]
        public void TestOrderedDeliverySpin()
        {
            TrySend(4, 10000, true, ConfigurationEngineDefaults.Threading.Locking.SPIN);
        }
    
        public void ManualTestOrderedDeliveryFail()
        {
            // Commented out as this is a manual test -- it should fail since the disable
            // preserve order.
            TrySend(3, 1000, false, null);
        }

        private void TrySend(int numThreads, int numEvents, bool isPreserveOrder, ConfigurationEngineDefaults.Threading.Locking? locking)
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder = isPreserveOrder;
            config.EngineDefaults.ThreadingConfig.ListenerDispatchLocking = locking.GetValueOrDefault();
    
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
            _engine.Initialize();
    
            // setup statements
            var stmtInsert = _engine.EPAdministrator.CreateEPL("select count(*) as cnt from " + typeof(SupportBean).FullName);
            var listener = new SupportMTUpdateListener();       
            stmtInsert.Events += listener.Update;
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<object>[numThreads];
            for (var i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new SendEventCallable(i, _engine, EnumerationGenerator.Create(numEvents)));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 10));
    
            for (var i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault().AsBoolean());
            }
    
            var events = listener.GetNewDataListFlattened();
            var result = new long[events.Length];
            for (var i = 0; i < events.Length; i++)
            {
                result[i] = (long) events[i].Get("cnt");
            }
            //Log.Info(".trySend result=" + CompatExtensions.Render(result));
    
            // assert result
            Assert.AreEqual(numEvents * numThreads, events.Length);
            for (var i = 0; i < numEvents * numThreads; i++)
            {
                Assert.AreEqual(result[i], (long) i + 1);
            }
        }
    }
}
