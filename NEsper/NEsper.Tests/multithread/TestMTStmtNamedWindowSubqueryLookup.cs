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
    /// <summary>
    /// Test for multithread-safety and named window subqueries and direct index-based 
    /// lookup.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowSubqueryLookup 
    {
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }
    
        [Test]
        public void TestConcurrentSubquery()
        {
            TrySend(3, 10000);
        }
    
        private void TrySend(int numThreads, int numEventsPerThread)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.EventMetaConfig.DefaultEventRepresentation = EventRepresentation.MAP; // use Map-type events for testing
            config.AddEventType("SupportBean", typeof(SupportBean));
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
            _engine.Initialize();
    
            // setup statements
            _engine.EPAdministrator.CreateEPL("create schema MyUpdateEvent as (key string, intupd int)");
            _engine.EPAdministrator.CreateEPL("create schema MySchema as (TheString string, intval int)");
            EPStatement namedWindow = _engine.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as MySchema");
            _engine.EPAdministrator.CreateEPL("on MyUpdateEvent mue merge MyWindow mw " +
                    "where mw.TheString = mue.key " +
                    "when not matched then insert select key as TheString, intupd as intval " +
                    "when matched then delete");
            EPStatement targetStatement = _engine.EPAdministrator.CreateEPL("select (select intval from MyWindow mw where mw.TheString = sb.TheString) as val from SupportBean sb");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new StmtNamedWindowSubqueryLookupCallable(i, _engine, numEventsPerThread, targetStatement));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // total up result
            for (int i = 0; i < numThreads; i++)
            {
                bool? result = future[i].GetValueOrDefault();
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Value);
            }

            EventBean[] events = namedWindow.ToArray();
            Assert.AreEqual(0, events.Length);
        }
    }
}
