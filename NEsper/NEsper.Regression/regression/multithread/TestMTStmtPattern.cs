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
using com.espertech.esper.client.util;
using com.espertech.esper.compat.threading;
using com.espertech.esper.regression.multithread;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for pattern statement parallel execution by threads.
    /// </summary>
    [TestFixture]
    public class TestMTStmtPattern 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [Test]
        public void TestPattern()
        {
            String type = typeof(SupportBean).FullName;
    
            String pattern = "a=" + type;
            TryPattern(pattern, 4, 20);
    
            pattern = "a=" + type + " or a=" + type;
            TryPattern(pattern, 2, 20);
        }

        private void TryPattern(String pattern, int numThreads, int numEvents)
        {
            var sendLock = new Object();
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var callables = new SendEventWaitCallable[numThreads];
            for (int i = 0; i < numThreads; i++) {
                callables[i] = new SendEventWaitCallable(i, _engine, sendLock, EnumerationGenerator.Create(numEvents));
                future[i] = threadPool.Submit(callables[i]);
            }

            var listener = new SupportMTUpdateListener[numEvents];
            for (int i = 0; i < numEvents; i++) {
                EPStatement stmt = _engine.EPAdministrator.CreatePattern(pattern);
                listener[i] = new SupportMTUpdateListener();
                stmt.Events += listener[i].Update;

                lock (sendLock) {
                    Monitor.PulseAll(sendLock);
                }
            }

            foreach (SendEventWaitCallable callable in callables) {
                callable.SetShutdown(true);
            }

            lock (sendLock) {
                Monitor.PulseAll(sendLock);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 10));


            for (int i = 0; i < numEvents; i++) {
                Assert.That(listener[i].AssertOneGetNewAndReset().Get("a"), Is.InstanceOf<SupportBean>());
            }
        }
    }
}
