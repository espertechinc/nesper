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
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    public class MultithreadContextStartedBySameEvent : RegressionExecutionWithConfigure
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Common.AddEventType(typeof(PayloadEvent));
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var eplStatement = "@public create context MyContext start PayloadEvent end after 0.5 seconds";
            env.CompileDeploy(eplStatement, path);

            var aggStatement = "@name('select') context MyContext " +
                               "select count(*) as theCount " +
                               "from PayloadEvent " +
                               "output snapshot when terminated";
            env.CompileDeploy(aggStatement, path);
            var listener = new MyListener();
            env.Statement("select").AddListener(listener);

            // start thread
            long numEvents = 10000000;
            var myRunnable = new MyRunnable(env.Runtime, numEvents);
            var thread = new Thread(myRunnable.Run);
            thread.Name = nameof(MultithreadContextStartedBySameEvent);
            thread.Start();
            SupportCompileDeployUtil.ThreadJoin(thread);

            SupportCompileDeployUtil.ThreadSleep(1000);

            // assert
            Assert.IsNull(myRunnable.exception);
            Assert.AreEqual(numEvents, listener.total);

            env.UndeployAll();
        }

        public class PayloadEvent
        {
        }

        public class MyRunnable : IRunnable
        {
            private readonly long numEvents;
            private readonly EPRuntime runtime;

            internal Exception exception;

            public MyRunnable(
                EPRuntime runtime,
                long numEvents)
            {
                this.runtime = runtime;
                this.numEvents = numEvents;
            }

            public void Run()
            {
                try {
                    for (var i = 0; i < numEvents; i++) {
                        var payloadEvent = new PayloadEvent();
                        runtime.EventService.SendEventBean(payloadEvent, "PayloadEvent");
                        if (i > 0 && i % 1000000 == 0) {
                            Console.Out.WriteLine("sent " + i + " events");
                        }
                    }

                    Console.Out.WriteLine("sent " + numEvents + " events");
                }
                catch (Exception ex) {
                    log.Error("Error while processing", ex);
                    exception = ex;
                }
            }
        }

        public class MyListener : UpdateListener
        {
            internal long total;

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var theCount = eventArgs.NewEvents[0].Get("theCount").AsInt64();
                total += theCount;
                Console.Out.WriteLine("count " + theCount + " Total "+ total);
            }
        }
    }
} // end of namespace