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

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadPatternTimer : RegressionExecutionWithConfigure
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static IDictionary<string, SupportCountListener> _supportCountListeners =
            new Dictionary<string, SupportCountListener>();

        public void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(SupportByteArrEventLongId));
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
        }

        public bool HAWithCOnly => true;

        public bool EnableHATest => true;

        public void Run(RegressionEnvironment env)
        {
            // configure
            var numThreads = 2;
            var numStatements = 100;
            var numEvents = 50000;

            // set up threading
            var queue = new LinkedBlockingQueue<Runnable>();
            var threadPool = Executors.NewFixedThreadPool(
                numThreads,
                new SupportThreadFactory(typeof(MultithreadPatternTimer)).ThreadFactory);

            // create statements
            log.Info("Creating statements");
            for (var i = 0; i < numStatements; i++) {
                var statementName = "s" + i;
                var stmtText =
                    $"@Name('s{i}')select * from pattern" +
                    $" [ every e1=SupportByteArrEventLongId(Id={i}) -> timer:interval(1 seconds)]";

                var supportCountListener = new SupportCountListener();
                _supportCountListeners[statementName] = supportCountListener;
                
                env.CompileDeploy(
                    stmtText,
                    options => options.SetStatementUserObject(
                        _ => supportCountListener));

                var statement = env.Statement(statementName);
                statement.AddListener(supportCountListener);
            }

            // submit events
            var startTime = DateTimeHelper.CurrentTimeMillis;
            log.Info("Submitting " + numEvents + " events to queue");
            var random = new Random();
            for (var i = 0; i < numEvents; i++) {
                var @event = new SupportByteArrEventLongId(random.Next(numStatements), 0);
                threadPool.Submit(() => env.SendEventBean(@event));
            }

            log.Info("Waiting for completion");
            while (!queue.IsEmpty()) {
                try {
                    Thread.Sleep(5000);
                }
                catch (ThreadInterruptedException) {
                    Assert.Fail();
                }

                log.Info("Queue size is " + queue.Count);
            }

            var endTime = DateTimeHelper.CurrentTimeMillis;
            log.Info("Time to complete: " + (endTime - startTime) / 1000 + " sec");

            // wait for completion
            log.Info("Waiting for remaining callbacks");
            var startWaitTime = DateTimeHelper.CurrentTimeMillis;
            while (true) {
                try {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException) {
                    Assert.Fail();
                }

                var countTotal = GetCount(env, numStatements);
                if (countTotal >= numEvents) {
                    break;
                }

                if (DateTimeHelper.CurrentTimeMillis - startWaitTime > 20000) {
                    Assert.Fail();
                }

                log.Info("Waiting for remaining callbacks: " + countTotal + " of " + numEvents);
            }

            // assert
            var total = GetCount(env, numStatements);
            Assert.AreEqual(numEvents, total);

            env.UndeployAll();
        }
        
        private long GetCount(
            RegressionEnvironment env,
            int numStatements)
        {
            var total = 0L;
            for (var i = 0; i < numStatements; i++) {
                var statement = env.Statement("s" + Convert.ToString(i));
                var supportCountListener = _supportCountListeners.Get(statement.Name);
                total += supportCountListener.CountNew;
            }

            return total;
        }
    }
} // end of namespace