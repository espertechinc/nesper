///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// <summary>Test for update listeners that create and stop statements.  </summary>
    [TestFixture]
    public class TestMTStmtListenerCreateStmt 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtListenerCreateStmt", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestListenerCreateStmt()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL(
                    " select * from " + typeof(SupportBean).FullName);
    
            TryListener(2, 100, stmt);
        }
    
        private void TryListener(int numThreads, int numRepeats, EPStatement stmt)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtListenerRouteCallable(i, _engine, stmt, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
}
