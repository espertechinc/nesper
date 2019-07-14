///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for case of 2 patterns:
    ///     1. Thread 1 starts pattern "every event1=SupportTradeEvent(userID in ('100','101'), amount&gt;=1000)"
    ///     2. Thread 1 repeats sending 100 events and tests 5% received
    ///     3. Main thread starts pattern:
    ///     ( every event1=SupportTradeEvent(userID in ('100','101')) -&gt;
    ///     (SupportTradeEvent(userID in ('100','101'), direction = event1.direction ) -&gt;
    ///     SupportTradeEvent(userID in ('100','101'), direction = event1.direction )
    ///     ) where timer:within(8 hours)
    ///     and not eventNC=SupportTradeEvent(userID in ('100','101'), direction!= event1.direction )
    ///     ) -&gt; eventFinal=SupportTradeEvent(userID in ('100','101'), direction != event1.direction ) where timer:within(1
    ///     hour)
    ///     4. Main thread waits for 2 seconds and stops all threads
    /// </summary>
    public class MultithreadStmtTwoPatterns : RegressionExecutionWithConfigure
    {
        public void Configure(Configuration configuration)
        {
        }

        public bool EnableHATest => true;

        public bool HAWithCOnly => true;

        public void Run(RegressionEnvironment env)
        {
            var statementTwo = "select * from pattern[( every event1=SupportTradeEvent(userId in ('100','101')) =>\n" +
                               "         (SupportTradeEvent(userId in ('100','101'), direction = event1.direction ) =>\n" +
                               "          SupportTradeEvent(userId in ('100','101'), direction = event1.direction )\n" +
                               "         ) where timer:within(8 hours)\n" +
                               "         and not eventNC=SupportTradeEvent(userId in ('100','101'), direction!= event1.direction )\n" +
                               "        ) => eventFinal=SupportTradeEvent(userId in ('100','101'), direction != event1.direction ) where timer:within(1 hour)]";
            var compiledTwo = env.Compile(statementTwo);

            var runnable = new TwoPatternRunnable(env);
            var t = new Thread(runnable.Run);
            t.Name = typeof(MultithreadStmtTwoPatterns).Name;
            t.Start();
            SupportCompileDeployUtil.ThreadSleep(100);

            // Create a second pattern, wait 500 msec, destroy second pattern in a loop
            var numRepeats = env.IsHA ? 1 : 10;
            for (var i = 0; i < numRepeats; i++) {
                try {
                    var deployed = env.Deployment.Deploy(compiledTwo);
                    SupportCompileDeployUtil.ThreadSleep(200);
                    env.Undeploy(deployed.DeploymentId);
                }
                catch (Exception ex) {
                    throw new EPException(ex);
                }
            }

            runnable.Shutdown = true;
            SupportCompileDeployUtil.ThreadSleep(1000);
            Assert.IsFalse(t.IsAlive);

            env.UndeployAll();
        }
    }
} // end of namespace