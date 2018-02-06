///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for case of 2 patterns:
    /// 1. Thread 1 starts pattern "every event1=SupportEvent(userID in ('100','101'), amount>=1000)"
    /// 2. Thread 1 repeats sending 100 events and tests 5% received
    /// 3. Main thread starts pattern:
    /// ( every event1=SupportEvent(userID in ('100','101')) ->
    /// (SupportEvent(userID in ('100','101'), direction = event1.direction ) ->
    /// SupportEvent(userID in ('100','101'), direction = event1.direction )
    /// ) where timer:within(8 hours)
    /// and not eventNC=SupportEvent(userID in ('100','101'), direction!= event1.direction )
    /// ) -> eventFinal=SupportEvent(userID in ('100','101'), direction != event1.direction ) where timer:within(1 hour)
    /// 4. Main thread waits for 2 seconds and stops all threads
    /// </summary>
    public class ExecMTStmtTwoPatternsStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportEvent", typeof(SupportTradeEvent));
    
            string statementTwo = "( every event1=SupportEvent(userId in ('100','101')) ->\n" +
                    "         (SupportEvent(userId in ('100','101'), direction = event1.direction ) ->\n" +
                    "          SupportEvent(userId in ('100','101'), direction = event1.direction )\n" +
                    "         ) where timer:within(8 hours)\n" +
                    "         and not eventNC=SupportEvent(userId in ('100','101'), direction!= event1.direction )\n" +
                    "        ) -> eventFinal=SupportEvent(userId in ('100','101'), direction != event1.direction ) where timer:within(1 hour)";
    
            var runnable = new TwoPatternRunnable(epService);
            var t = new Thread(runnable.Run);
            t.Start();
            Thread.Sleep(200);
    
            // Create a second pattern, wait 200 msec, destroy second pattern in a loop
            for (int i = 0; i < 10; i++) {
                EPStatement statement = epService.EPAdministrator.CreatePattern(statementTwo);
                Thread.Sleep(200);
                statement.Dispose();
            }
    
            runnable.SetShutdown(true);
            Thread.Sleep(1000);
            Assert.IsFalse(t.IsAlive);
        }
    }
} // end of namespace
