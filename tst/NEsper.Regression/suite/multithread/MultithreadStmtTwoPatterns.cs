///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.multithread;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for case of 2 patterns:
    ///     1. Thread 1 starts pattern "every event1=SupportTradeEvent(userID in ('100','101'), Amount&gt;=1000)"
    ///     2. Thread 1 repeats sending 100 events and tests 5% received
    ///     3. Main thread starts pattern:
    ///     ( every event1=SupportTradeEvent(userID in ('100','101')) -&gt;
    ///     (SupportTradeEvent(userID in ('100','101'), Direction = event1.Direction ) -&gt;
    ///     SupportTradeEvent(userID in ('100','101'), Direction = event1.Direction )
    ///     ) where timer:within(8 hours)
    ///     and not eventNC=SupportTradeEvent(userID in ('100','101'), Direction!= event1.Direction )
    ///     ) -&gt; eventFinal=SupportTradeEvent(userID in ('100','101'), Direction != event1.Direction ) where timer:within(1
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

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.MULTITHREADED);
        }

        public void Run(RegressionEnvironment env)
        {
            var statementTwo = "select * from pattern[( every event1=SupportTradeEvent(UserId in ('100','101')) ->\n" +
                               "         (SupportTradeEvent(UserId in ('100','101'), Direction = event1.Direction ) ->\n" +
                               "          SupportTradeEvent(UserId in ('100','101'), Direction = event1.Direction )\n" +
                               "         ) where timer:within(8 hours)\n" +
                               "         and not eventNC=SupportTradeEvent(UserId in ('100','101'), Direction!= event1.Direction )\n" +
                               "        ) -> eventFinal=SupportTradeEvent(UserId in ('100','101'), Direction != event1.Direction ) where timer:within(1 hour)]";
            var compiledTwo = env.Compile(statementTwo);

            var runnable = new TwoPatternRunnable(env);
            var t = new Thread(runnable.Run);
            t.Name = nameof(MultithreadStmtTwoPatterns);
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