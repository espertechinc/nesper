///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of @priority and named windows. </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowPriority 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportBean_S1>();
            configuration.EngineDefaults.ExecutionConfig.IsPrioritized = true;
            configuration.EngineDefaults.ThreadingConfig.IsInsertIntoDispatchPreserveOrder = false;
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [Test]
        public void TestPriority()
        {
            EPStatement stmtWindow = _engine.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as (c0 string, c1 string)");
            _engine.EPAdministrator.CreateEPL("insert into MyWindow select p00 as c0, p01 as c1 from SupportBean_S0");
            _engine.EPAdministrator.CreateEPL("@Priority(1) on SupportBean_S1 s1 merge MyWindow s0 where s1.p10 = c0 " +
                    "when matched then Update set c1 = s1.p11");
            _engine.EPAdministrator.CreateEPL("@Priority(0) on SupportBean_S1 s1 merge MyWindow s0 where s1.p10 = c0 " +
                    "when matched then Update set c1 = s1.p12");
    
            TrySend(stmtWindow, 4, 1000);
        }
    
        private void TrySend(EPStatement stmtWindow, int numThreads, int numRepeats)
        {
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new StmtNamedWindowPriorityCallable(i, _engine, numRepeats);
                future[i] = threadPool.Submit(callable);
            }
    
            for (int i = 0; i < numThreads; i++)
            {
                future[i].GetValueOrDefault();
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));

            EventBean[] events = stmtWindow.ToArray();
            Assert.AreEqual(numThreads * numRepeats, events.Length);
            for (int i = 0; i < events.Length; i++) {
                String valueC1 = (String) events[i].Get("c1");
                Assert.AreEqual("y", valueC1);
            }
        }
    }
}
