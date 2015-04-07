///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.support.bean.word;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTStmtStateless 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestStateless()
        {
            TrySend(4, 1000);
        }
    
        private void TrySend(int numThreads, int numRepeats)
        {
            _engine.EPAdministrator.Configuration.AddEventType(typeof(SentenceEvent));
            var spi = (EPStatementSPI) _engine.EPAdministrator.CreateEPL("select * from SentenceEvent[Words]");
            Assert.IsTrue(spi.StatementContext.IsStatelessSelect);
    
            var runnables = new StatelessRunnable[numThreads];
            for (int i = 0; i < runnables.Length; i++) {
                runnables[i] = new StatelessRunnable(_engine, numRepeats);
            }
    
            var threads = new Thread[numThreads];
            for (int i = 0; i < runnables.Length; i++) {
                threads[i] = new Thread(runnables[i].Run);
            }
    
            long start = PerformanceObserver.MilliTime;
            foreach (Thread t in threads) {
                t.Start();
            }
    
            foreach (Thread t in threads) {
                t.Join();
            }
            long delta = PerformanceObserver.MilliTime - start;
            Log.Info("Delta=" + delta + " for " + numThreads*numRepeats + " events");
    
            foreach (StatelessRunnable r in runnables) {
                Assert.IsNull(r.GetException());
            }
        }
    
        public class StatelessRunnable 
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numRepeats;
    
            private Exception _exception;
    
            public StatelessRunnable(EPServiceProvider engine, int numRepeats) {
                _engine = engine;
                _numRepeats = numRepeats;
            }
    
            public void Run() {
                try {
                    for (int i = 0; i < _numRepeats; i++) {
                        _engine.EPRuntime.SendEvent(new SentenceEvent("This is stateless statement testing"));
    
                        if (i % 10000 == 0) {
                            Log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + i);
                        }
                    }
                }
                catch (Exception e) {
                    _exception = e;
                }
            }
    
            public Exception GetException() {
                return _exception;
            }
        }
    }
}
