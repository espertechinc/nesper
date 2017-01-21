///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;

using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    [TestFixture]
    public class TestMTIsolation 
    {
        [Test]
        public void TestSceneOne()
        {
            TryIsolated(2, 500);
        }
    
        private void TryIsolated(int numThreads, int numLoops)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
            config.AddEventType<SupportBean>();
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];

            var sharedStartLock = ReaderWriterLockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            using (sharedStartLock.AcquireWriteLock())
            {
                for (int i = 0; i < numThreads; i++)
                {
                    var isolate = new IsolateUnisolateCallable(i, engine, numLoops);
                    future[i] = threadPool.Submit((Func<bool>) isolate.Call);
                }
                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
}
