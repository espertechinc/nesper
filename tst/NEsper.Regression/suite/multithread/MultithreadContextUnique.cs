///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety of context.
    /// </summary>
    public class MultithreadContextUnique : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "create schema ScoreCycle (userId string, keyword string, ProductId string, score long);\n" +
                      "\n" +
                      "create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                      "\n" +
                      "create context HashByUserCtx as\n" +
                      "coalesce by consistent_hash_crc32(userId) from ScoreCycle,\n" +
                      "consistent_hash_crc32(userId) from UserKeywordTotalStream \n" +
                      "granularity 10000000;\n" +
                      "\n" +
                      "context HashByUserCtx create window ScoreCycleWindow#unique(userId, keyword, ProductId) as ScoreCycle;\n" +
                      "\n" +
                      "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                      "\n" +
                      "@Name('Select') context HashByUserCtx insert into UserKeywordTotalStream\n" +
                      "select userId, keyword, sum(score) as sumScore from ScoreCycleWindow group by userId, keyword;";
            env.CompileDeployWBusPublicType(epl, new RegressionPath());
            var listener = new MyUpdateListener();
            env.Statement("Select").AddListener(listener);

            IList<IDictionary<string, object>> sendsT1 = new List<IDictionary<string, object>>();
            sendsT1.Add(MakeEvent("A", "house", "P0", 1));
            sendsT1.Add(MakeEvent("B", "house", "P0", 2));
            IList<IDictionary<string, object>> sendsT2 = new List<IDictionary<string, object>>();
            sendsT2.Add(MakeEvent("B", "house", "P0", 3));
            sendsT1.Add(MakeEvent("A", "house", "P0", 4));

            var threadPool = Executors.NewFixedThreadPool(
                2,
                new SupportThreadFactory(typeof(MultithreadContextUnique)).ThreadFactory);
            var runnableOne = new SendEventRunnable(env.Runtime, sendsT1, "ScoreCycle");
            var runnableTwo = new SendEventRunnable(env.Runtime, sendsT2, "ScoreCycle");
            threadPool.Submit(runnableOne.Run);
            threadPool.Submit(runnableTwo.Run);

            threadPool.Shutdown();
            SupportCompileDeployUtil.ExecutorAwait(threadPool, 1, TimeUnit.SECONDS);

            Assert.IsNull(runnableOne.LastException);
            Assert.IsNull(runnableTwo.LastException);

            // compare
            var received = listener.Received;
            foreach (var item in received) {
                Console.Out.WriteLine(item);
            }

            Assert.AreEqual(4, received.Count);

            env.UndeployAll();
        }

        private IDictionary<string, object> MakeEvent(
            string userId,
            string keyword,
            string productId,
            long score)
        {
            IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("userId", userId);
            theEvent.Put("keyword", keyword);
            theEvent.Put("ProductId", productId);
            theEvent.Put("score", score);
            return theEvent;
        }

        public class MyUpdateListener : UpdateListener
        {
            public IList<object> Received { get; } = new List<object>();

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                lock (this) {
                    var newEvents = eventArgs.NewEvents;
                    for (var i = 0; i < newEvents.Length; i++) {
                        Received.Add(newEvents[i].Underlying);
                    }
                }
            }
        }

        public class SendEventRunnable : IRunnable
        {
            private readonly IList<IDictionary<string, object>> events;
            private readonly EPRuntime runtime;
            private readonly string type;

            public SendEventRunnable(
                EPRuntime runtime,
                IList<IDictionary<string, object>> events,
                string type)
            {
                this.runtime = runtime;
                this.events = events;
                this.type = type;
            }

            public Exception LastException { get; private set; }

            public void Run()
            {
                try {
                    foreach (var theEvent in events) {
                        runtime.EventService.SendEventMap(theEvent, type);
                    }
                }
                catch (Exception t) {
                    LastException = t;
                    //t.PrintStackTrace();
                }
            }
        }
    }
} // end of namespace