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
    /// Test for multithread-safety and named window subqueries and aggregation.
    /// </summary>
    [TestFixture]
    public class TestMTStmtNamedWindowSubqueryAgg 
    {
        private EPServiceProvider _engine;
    
        [TearDown]
        public void TearDown()
        {
            _engine.Initialize();
        }
    
        [Test]
        public void TestConcurrentSubqueryIndexNoShare()
        {
            TrySend(3, 1000, false);
        }
    
        [Test]
        public void TestConcurrentSubqueryIndexShare()
        {
            TrySend(3, 1000, true);
        }
    
        private void TrySend(int numThreads, int numEventsPerThread, bool indexShare)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddPlugInAggregationFunctionFactory("intListAgg", typeof(MyIntListAggregationFactory).FullName);
            config.EngineDefaults.EventMetaConfig.DefaultEventRepresentation = EventRepresentation.MAP; // use Map-type events for testing
            _engine = EPServiceProviderManager.GetDefaultProvider(config);
            _engine.Initialize();
    
            // setup statements
            _engine.EPAdministrator.CreateEPL("create schema UpdateEvent as (uekey string, ueint int)");
            _engine.EPAdministrator.CreateEPL("create schema WindowSchema as (wskey string, wsint int)");
    
            String createEpl = "create window MyWindow.win:keepall() as WindowSchema";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            EPStatement namedWindow = _engine.EPAdministrator.CreateEPL(createEpl);
            
            _engine.EPAdministrator.CreateEPL("create index ABC on MyWindow(wskey)");
            _engine.EPAdministrator.CreateEPL("on UpdateEvent mue merge MyWindow mw " +
                    "where uekey = wskey and ueint = wsint " +
                    "when not matched then insert select uekey as wskey, ueint as wsint " +
                    "when matched then delete");
            // note: here all threads use the same string key to insert/delete and different values for the int
            EPStatement targetStatement = _engine.EPAdministrator.CreateEPL("select (select IntListAgg(wsint) from MyWindow mw where wskey = sb.TheString) as val from SupportBean sb");
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                future[i] = threadPool.Submit(new StmtNamedWindowSubqueryAggCallable(i, _engine, numEventsPerThread, targetStatement));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            // total up result
            for (int i = 0; i < numThreads; i++)
            {
                bool? result = future[i].GetValueOrDefault();
                Assert.IsTrue(result.GetValueOrDefault(false));
            }

            EventBean[] events = namedWindow.ToArray();
            Assert.AreEqual(0, events.Length);
        }
    }
}
