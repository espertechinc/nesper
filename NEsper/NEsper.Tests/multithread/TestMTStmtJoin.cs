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
    /// <summary>Test for multithread-safety for joins. </summary>
    [TestFixture]
    public class TestMTStmtJoin 
    {
        private EPServiceProvider _engine;
    
        private readonly static String EVENT_NAME = typeof(SupportBean).FullName;
    
        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            _engine = EPServiceProviderManager.GetProvider("TestMTStmtJoin", config);
        }
    
        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }
    
        [Test]
        public void TestJoin()
        {
            EPStatement stmt = _engine.EPAdministrator.CreateEPL("select istream * \n" +
                    "  from " + EVENT_NAME + "(TheString='s0').win:length(1000000) as s0,\n" +
                    "       " + EVENT_NAME + "(TheString='s1').win:length(1000000) as s1\n" +
                    "where s0.LongPrimitive = s1.LongPrimitive\n"
                    );
            TrySendAndReceive(4, stmt, 1000);
            TrySendAndReceive(2, stmt, 2000);
        }
    
        private void TrySendAndReceive(int numThreads, EPStatement statement, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtJoinCallable(i, _engine, statement, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            for (int i = 0; i < numThreads; i++)
            {
                Assert.IsTrue(future[i].GetValueOrDefault(), "Failed in " + statement.Text);
            }
        }
    }
}
