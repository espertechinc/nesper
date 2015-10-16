///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>
    /// Test for multithread-safety for adding and removing listener.
    /// </summary>
    [TestFixture]
    public class TestMTStmtListenerAddRemove
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.ListenerDispatchTimeout = Int64.MaxValue;
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtListenerAddRemove", config);
        }

        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }

        #endregion

        private EPServiceProvider _engine;

        private static readonly String EVENT_NAME = typeof(SupportMarketDataBean).FullName;

        private void TryStatementListenerAddRemove(int numThreads, EPStatement statement, bool isEPL, int numRepeats)
        {
            var threadPool = new DedicatedExecutorService("test", numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new StmtListenerAddRemoveCallable(_engine, statement, isEPL, numRepeats);
                future[i] = threadPool.Submit(callable);
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 30));

            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed stmt=" + statement.Text);
            }
        }

        [Test]
        public void TestEPL()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL("select * from " + EVENT_NAME + " (Symbol='IBM', Feed='RT')");
            int numThreads = 2;
            TryStatementListenerAddRemove(numThreads, stmt, true, 10000);
        }

        [Test]
        public void TestPatterns()
        {
            EPStatement stmt = _engine.EPAdministrator.CreatePattern("every a=" + EVENT_NAME + "(Symbol='IBM')");
            int numThreads = 2;
            TryStatementListenerAddRemove(numThreads, stmt, false, 10000);
        }
    }
}
