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
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// Test for multithread-safety of context.
    /// </summary>
    public class ExecMTContextUnique : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
        }
    
        public override void Run(EPServiceProvider epService)
        {
            string epl = "create schema ScoreCycle (userId string, keyword string, productId string, score long);\n" +
                    "\n" +
                    "create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                    "\n" +
                    "create context HashByUserCtx as\n" +
                    "coalesce by Consistent_hash_crc32(userId) from ScoreCycle,\n" +
                    "Consistent_hash_crc32(userId) from UserKeywordTotalStream \n" +
                    "granularity 10000000;\n" +
                    "\n" +
                    "context HashByUserCtx create window ScoreCycleWindow#unique(userId, keyword, productId) as ScoreCycle;\n" +
                    "\n" +
                    "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                    "\n" +
                    "@Name('Select') context HashByUserCtx insert into UserKeywordTotalStream\n" +
                    "select userId, keyword, sum(score) as sumScore from ScoreCycleWindow group by userId, keyword;";
    
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new MyUpdateListener();
            epService.EPAdministrator.GetStatement("Select").Events += listener.Update;
    
            var sendsT1 = new List<Map>();
            sendsT1.Add(MakeEvent("A", "house", "P0", 1));
            sendsT1.Add(MakeEvent("B", "house", "P0", 2));
            var sendsT2 = new List<Map>();
            sendsT2.Add(MakeEvent("B", "house", "P0", 3));
            sendsT1.Add(MakeEvent("A", "house", "P0", 4));
    
            var threadPool = Executors.NewFixedThreadPool(2);
            threadPool.Submit(new SendEventRunnable(epService, sendsT1, "ScoreCycle").Run);
            threadPool.Submit(new SendEventRunnable(epService, sendsT2, "ScoreCycle").Run);
            threadPool.Shutdown();
            threadPool.AwaitTermination(1, TimeUnit.SECONDS);
    
            // compare
            List<object> received = listener.Received;
            foreach (Object item in received) {
                Log.Info(item.ToString());
            }
            Assert.AreEqual(4, received.Count);
        }
    
        private IDictionary<string, Object> MakeEvent(string userId, string keyword, string productId, long score) {
            var theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("userId", userId);
            theEvent.Put("keyword", keyword);
            theEvent.Put("productId", productId);
            theEvent.Put("score", score);
            return theEvent;
        }
    
        public class MyUpdateListener
        {
            private readonly ILockable _lock = SupportContainer.Instance.LockManager().CreateDefaultLock();

            public void Update(object sender, UpdateEventArgs args) 
            {
                using (_lock.Acquire())
                {
                    for (int i = 0; i < args.NewEvents.Length; i++)
                    {
                        Received.Add(args.NewEvents[i].Underlying);
                    }
                }
            }

            public List<object> Received { get; } = new List<object>();
        }
    
        public class SendEventRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly IList<Map> _events;
            private readonly string _type;
    
            public SendEventRunnable(EPServiceProvider engine, IList<Map> events, string type)
            {
                _engine = engine;
                _events = events;
                _type = type;
            }
    
            public void Run()
            {
                foreach (var theEvent in _events)
                {
                    _engine.EPRuntime.SendEvent(theEvent, _type);
                }
            }
        }
    }
} // end of namespace
