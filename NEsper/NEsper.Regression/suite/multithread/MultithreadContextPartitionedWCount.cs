///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextPartitionedWCount : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            var choices = new [] { "A","B","C","D" };
            TrySend(env, 4, 1000, choices);
        }

        private static void TrySend(
            RegressionEnvironment env,
            int numThreads,
            int numEvents,
            string[] choices)
        {
            if (numEvents < choices.Length) {
                throw new ArgumentException("Number of events must at least match number of choices");
            }

            env.AdvanceTime(0);
            var path = new RegressionPath();
            env.CompileDeploy("@Name('var') create variable boolean myvar = false", path);
            env.CompileDeploy("create context SegmentedByString as partition by TheString from SupportBean", path);
            env.CompileDeploy(
                "@Name('s0') context SegmentedByString select TheString, count(*) - 1 as cnt from SupportBean output snapshot when myvar = true",
                path);
            var listener = new SupportUpdateListener();
            env.Statement("s0").AddListener(listener);

            // preload - since concurrently sending same-category events an event can be dropped
            for (var i = 0; i < choices.Length; i++) {
                env.SendEventBean(new SupportBean(choices[i], 0));
            }

            var runnables = new EventRunnable[numThreads];
            for (var i = 0; i < runnables.Length; i++) {
                runnables[i] = new EventRunnable(env, numEvents, choices);
            }

            // start
            var threads = new Thread[runnables.Length];
            for (var i = 0; i < runnables.Length; i++) {
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Name = typeof(MultithreadContextPartitionedWCount).Name;
                threads[i].Start();
            }

            // join
            log.Info("Waiting for completion");
            for (var i = 0; i < runnables.Length; i++) {
                SupportCompileDeployUtil.ThreadJoin(threads[i]);
            }

            IDictionary<string, long> totals = new Dictionary<string, long>();
            foreach (var choice in choices) {
                totals.Put(choice, 0L);
            }

            // verify
            var sum = 0;
            for (var i = 0; i < runnables.Length; i++) {
                Assert.IsNull(runnables[i].Exception);
                foreach (var entry in runnables[i].GetTotals()) {
                    var current = totals.Get(entry.Key);
                    current += entry.Value;
                    sum += entry.Value;
                    totals.Put(entry.Key, current);
                    //System.out.println("Thread " + i + " key " + entry.getKey() + " count " + entry.getValue());
                }
            }

            Assert.AreEqual(numThreads * numEvents, sum);

            env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "myvar", true);
            env.AdvanceTime(10000);
            var result = listener.LastNewData;
            Assert.AreEqual(choices.Length, result.Length);
            foreach (var item in result) {
                var theString = (string) item.Get("TheString");
                var count = item.Get("cnt").AsInt64();
                //System.out.println("String " + string + " count " + count);
                Assert.AreEqual(count, totals.Get(theString));
            }

            env.UndeployAll();
        }

        public class EventRunnable : IRunnable
        {
            private readonly string[] choices;

            private readonly RegressionEnvironment env;
            private readonly int numEvents;
            private readonly IDictionary<string, int> totals = new Dictionary<string, int>();

            public EventRunnable(
                RegressionEnvironment env,
                int numEvents,
                string[] choices)
            {
                this.env = env;
                this.numEvents = numEvents;
                this.choices = choices;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                log.Info("Started event send");

                try {
                    for (var i = 0; i < numEvents; i++) {
                        var chosen = choices[i % choices.Length];
                        env.SendEventBean(new SupportBean(chosen, 1));

                        if (!totals.TryGetValue(chosen, out var current)) {
                            current = 0;
                        }

                        current += 1;
                        totals.Put(chosen, current);
                    }
                }
                catch (Exception ex) {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }

                log.Info("Completed event send");
            }

            public IDictionary<string, int> GetTotals()
            {
                return totals;
            }
        }
    }
} // end of namespace