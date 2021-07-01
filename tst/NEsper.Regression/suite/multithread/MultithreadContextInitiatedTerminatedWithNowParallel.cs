///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextInitiatedTerminatedWithNowParallel : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(0);
            var path = new RegressionPath();
            env.CompileDeploy("create context MyCtx start @now end after 1 second", path);
            env.CompileDeploy(
                "@Name('s0') context MyCtx select count(*) as cnt from SupportBean output last when terminated",
                path);
            var listener = new SupportUpdateListener();
            env.Statement("s0").AddListener(listener);

            var latch = new AtomicBoolean(true);
            // With 0-sleep or 1-sleep the counts start to drop because the event is chasing the context partition.
            var t = new Thread(
                new MyTimeAdvancingRunnable(env, latch, 10, -1).Run);
            t.Name = typeof(MultithreadContextInitiatedTerminatedWithNowParallel).Name;
            t.Start();

            var numEvents = 10000;
            for (var i = 0; i < numEvents; i++) {
                env.SendEventBean(new SupportBean());
            }

            latch.Set(false);
            try {
                t.Join();
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            env.AdvanceTime(int.MaxValue);

            long total = 0;
            var deliveries = listener.NewDataListFlattened;
            foreach (var @event in deliveries) {
                var count = @event.Get("cnt").AsInt64();
                total += count;
            }

            Assert.AreEqual(numEvents, total);

            env.UndeployAll();
        }

        public class MyTimeAdvancingRunnable : IRunnable
        {
            private readonly RegressionEnvironment env;
            private readonly AtomicBoolean latch;
            private readonly long maxNumAdvances;
            private readonly long threadSleepTime;

            public MyTimeAdvancingRunnable(
                RegressionEnvironment env,
                AtomicBoolean latch,
                long threadSleepTime,
                long maxNumAdvances)
            {
                this.env = env;
                this.latch = latch;
                this.threadSleepTime = threadSleepTime;
                this.maxNumAdvances = maxNumAdvances;
            }

            public void Run()
            {
                long time = 1000;
                long numAdvances = 0;
                try {
                    while (latch.Get() && (maxNumAdvances == -1 || numAdvances < maxNumAdvances)) {
                        env.AdvanceTime(time);
                        numAdvances++;
                        time += 1000;
                        SupportCompileDeployUtil.ThreadSleep((int) threadSleepTime);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error while processing", ex);
                }
            }
        }
    }
} // end of namespace