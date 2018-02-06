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
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety for case of 2 patterns: 1. Thread 1 starts pattern
    /// "every event1=SupportEvent(userID in ('100','101'), amount>=1000)" 2. Thread 1
    /// repeats sending 100 events and tests 5% received 3. Main thread starts pattern: (
    /// every event1=SupportEvent(userID in ('100','101')) -> (SupportEvent(userID in
    /// ('100','101'), direction = event1.direction ) -> SupportEvent(userID in ('100','101'),
    /// direction = event1.direction ) ) where timer:within(8 hours) and not
    /// eventNC=SupportEvent(userID in ('100','101'), direction!= event1.direction ) ) ->
    /// eventFinal=SupportEvent(userID in ('100','101'), direction != event1.direction ) where
    /// timer:within(1 hour) namespace com.espertech.esper.regression.multithread { 4. Main thread waits
    /// for 2 seconds and stops all threads
    /// </summary>
    [TestFixture]
    public class TestMTStmtTwoPatterns
    {
        private EPServiceProvider engine;

        [SetUp]
        public void SetUp()
        {
            Configuration config = new Configuration();
            config.AddEventType("SupportEvent", typeof(SupportTradeEvent));

            engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            engine.Initialize();
        }

        [Test]
        public void Test2Patterns()
        {
            String statementTwo = "( every event1=SupportEvent(UserId in ('100','101')) ->\n" +
                    "         (SupportEvent(UserId in ('100','101'), Direction = event1.Direction ) ->\n" +
                    "          SupportEvent(UserId in ('100','101'), Direction = event1.Direction )\n" +
                    "         ) where timer:within(8 hours)\n" +
                    "         and not eventNC=SupportEvent(UserId in ('100','101'), Direction!= event1.Direction )\n" +
                    "        ) -> eventFinal=SupportEvent(UserId in ('100','101'), Direction != event1.Direction ) where timer:within(1 hour)";

            TwoPatternRunnable runnable = new TwoPatternRunnable(engine);
            Thread t = new Thread(runnable.Run);
            t.Start();
            Thread.Sleep(100);

            // Create a second pattern, wait 500 msec, destroy second pattern in a loop
            for (int i = 0; i < 10; i++)
            {
                EPStatement statement = engine.EPAdministrator.CreatePattern(statementTwo);
                Thread.Sleep(200);
                statement.Dispose();
            }

            runnable.Shutdown = true;
            Thread.Sleep(1000);
            Assert.IsFalse(t.IsAlive);
        }
    }
}
