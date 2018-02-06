///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// Test for multithread-safety of context.
    /// </summary>
    [TestFixture]
    public class TestMTContext 
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
    
        [TearDown]
        public void TearDown() {
        }
    
        [Test]
        public void TestContextCountSimple()
        {
            _engine.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _engine.EPAdministrator.CreateEPL("create context HashByUserCtx as coalesce by Consistent_hash_crc32(TheString) from SupportBean granularity 10000000");
            _engine.EPAdministrator.CreateEPL("@Name('select') context HashByUserCtx select TheString from SupportBean");
    
            TrySendContextCountSimple(4, 5);
        }
    
        [Test]
        public void TestContextUnique() {
            String epl = "create schema ScoreCycle (userId string, keyword string, productId string, score long);\n" +
                    "\n" +
                    "create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                    "\n" +
                    "create context HashByUserCtx as\n" +
                    "coalesce by consistent_hash_crc32(userId) from ScoreCycle,\n" +
                    "consistent_hash_crc32(userId) from UserKeywordTotalStream \n" +
                    "granularity 10000000;\n" +
                    "\n" +
                    "context HashByUserCtx create window ScoreCycleWindow#unique(userId, keyword, productId) as ScoreCycle;\n" +
                    "\n" +
                    "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                    "\n" +
                    "@Name('Select') context HashByUserCtx insert into UserKeywordTotalStream\n" +
                    "select userId, keyword, sum(score) as sumScore from ScoreCycleWindow group by userId, keyword;";
    
            _engine.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new MyUpdateListener();
            _engine.EPAdministrator.GetStatement("Select").Events += listener.Update;
    
            var sendsT1 = new List<Map>();
            sendsT1.Add(MakeEvent("A", "house", "P0", 1));
            sendsT1.Add(MakeEvent("B", "house", "P0", 2));
            var sendsT2 = new List<Map>();
            sendsT2.Add(MakeEvent("B", "house", "P0", 3));
            sendsT1.Add(MakeEvent("A", "house", "P0", 4));
    
            var threadPool = Executors.NewFixedThreadPool(2);
            threadPool.Submit((new SendEventRunnable(_engine, sendsT1, "ScoreCycle")).Run);
            threadPool.Submit((new SendEventRunnable(_engine, sendsT2, "ScoreCycle")).Run);
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(1));
    
            // compare
            List<Object> received = listener.Received;
            foreach (Object item in received) {
                Console.WriteLine(item);
            }
            Assert.AreEqual(4, received.Count);
        }
    
        private void TrySendContextCountSimple(int numThreads, int numRepeats)
        {
            var listener = new SupportMTUpdateListener();
            _engine.EPAdministrator.GetStatement("select").Events += listener.Update;
    
            var events = new List<Object>();
            for (int i = 0; i < numRepeats; i++) {
                events.Add(new SupportBean("E" + i, i));
            }
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<object>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new SendEventCallable(i, _engine, events.GetEnumerator());
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(TimeSpan.FromSeconds(10));
    
            EventBean[] result = listener.GetNewDataListFlattened();
            Assert.AreEqual(numRepeats * numThreads, result.Length);
        }
    
        private static IDictionary<String, Object> MakeEvent(String userId, String keyword, String productId, long score) {
            IDictionary<String, Object> theEvent = new LinkedHashMap<String, Object>();
            theEvent["userId"] = userId;
            theEvent["keyword"] = keyword;
            theEvent["productId"] = productId;
            theEvent["score"] = score;
            return theEvent;
        }
    
        public class MyUpdateListener
        {
            private readonly List<Object> _received = new List<Object>();
    
            public void Update(Object sender, UpdateEventArgs e)
            {
                Update(e.NewEvents, e.OldEvents);
            }

            public void Update(EventBean[] newEvents, EventBean[] oldEvents) {
                lock (this)
                {
                    for (int i = 0; i < newEvents.Length; i++)
                    {
                        _received.Add(newEvents[i].Underlying);
                    }
                }
            }

            public List<object> Received
            {
                get { return _received; }
            }
        }
    
        public class SendEventRunnable : IRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly List<Map> _events;
            private readonly String _type;
    
            public SendEventRunnable(EPServiceProvider engine, List<Map> events, String type) {
                _engine = engine;
                _events = events;
                _type = type;
            }
    
            public void Run()
            {
                foreach (Map theEvent in _events) {
                    _engine.EPRuntime.SendEvent(theEvent, _type);
                }
            }
        }
    }
}
