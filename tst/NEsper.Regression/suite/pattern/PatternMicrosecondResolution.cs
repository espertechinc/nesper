///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternMicrosecondResolution : RegressionExecution
    {
        private readonly bool micros;

        public PatternMicrosecondResolution(bool isMicroseconds)
        {
            micros = isMicroseconds;
        }

        public void Run(RegressionEnvironment env)
        {
            var time = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
            var currentTime = PerformanceObserver.MilliTime;

            var millis = !micros;

            if (millis) {
                RunAssertionPattern(env, 0, "timer:interval(1)", 1000);
            }

            if (micros) {
                RunAssertionPattern(env, 0, "timer:interval(1)", 1000000);
            }

            if (millis) {
                RunAssertionPattern(env, 0, "timer:interval(10 sec 5 msec)", 10005);
            }

            if (micros) {
                RunAssertionPattern(env, 0, "timer:interval(10 sec 5 msec 1 usec)", 10005001);
            }

            if (millis) {
                RunAssertionPattern(env, 0, "timer:interval(1 month 10 msec)", TimePlusMonth(0, 1) + 10);
            }

            if (micros) {
                RunAssertionPattern(env, 0, "timer:interval(1 month 10 usec)", TimePlusMonth(0, 1) * 1000 + 10);
            }

            if (millis) {
                RunAssertionPattern(
                    env,
                    currentTime,
                    "timer:interval(1 month 50 msec)",
                    TimePlusMonth(currentTime, 1) + 50);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    currentTime * 1000 + 33,
                    "timer:interval(3 month 100 usec)",
                    TimePlusMonth(currentTime, 3) * 1000 + 33 + 100);
            }

            if (millis) {
                RunAssertionPattern(env, time, "timer:at(1, *, *, *, *, *)", time + 60000);
            }

            if (micros) {
                RunAssertionPattern(env, time * 1000 + 123, "timer:at(1, *, *, *, *, *)", time * 1000 + 60000000 + 123);
            }

            // Schedule Date-only
            if (millis) {
                RunAssertionPattern(env, time, "timer:schedule(iso:'2002-05-30T09:01:00')", time + 60000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 123,
                    "timer:schedule(iso:'2002-05-30T09:01:00')",
                    time * 1000 + 60000000);
            }

            // Schedule Period-only
            if (millis) {
                RunAssertionPattern(env, time, "every timer:schedule(period: 2 minute)", time + 120000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 123,
                    "every timer:schedule(period: 2 minute)",
                    time * 1000 + 123 + 120000000);
            }

            // Schedule Date+period
            if (millis) {
                RunAssertionPattern(env, time, "every timer:schedule(iso:'2002-05-30T09:00:00/PT1M')", time + 60000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 345,
                    "every timer:schedule(iso:'2002-05-30T09:00:00/PT1M')",
                    time * 1000 + 60000000);
            }

            // Schedule recurring period
            if (millis) {
                RunAssertionPattern(env, time, "every timer:schedule(iso:'R2/PT1M')", time + 60000, time + 120000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 345,
                    "every timer:schedule(iso:'R2/PT1M')",
                    time * 1000 + 345 + 60000000,
                    time * 1000 + 345 + 120000000);
            }

            // Schedule date+recurring period
            if (millis) {
                RunAssertionPattern(
                    env,
                    time,
                    "every timer:schedule(iso:'R2/2002-05-30T09:01:00/PT1M')",
                    time + 60000,
                    time + 120000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 345,
                    "every timer:schedule(iso:'R2/2002-05-30T09:01:00/PT1M')",
                    time * 1000 + 60000000,
                    time * 1000 + 120000000);
            }

            // Schedule with date computation
            if (millis) {
                RunAssertionPattern(
                    env,
                    time,
                    "timer:schedule(date: current_timestamp.withTime(9, 1, 0, 0))",
                    time + 60000);
            }

            if (micros) {
                RunAssertionPattern(
                    env,
                    time * 1000 + 345,
                    "timer:schedule(date: current_timestamp.withTime(9, 1, 0, 0))",
                    time * 1000 + 345 + 60000000);
            }
        }

        private static void RunAssertionPattern(
            RegressionEnvironment env,
            long startTime,
            string patternExpr,
            params long[] flipTimes)
        {
            env.AdvanceTime(startTime);

            var epl = "@name('s0') select * from pattern[" + patternExpr + "]";
            env.CompileDeploy(epl).AddListener("s0");

            var count = 0;
            foreach (var flipTime in flipTimes) {
                env.AdvanceTime(flipTime - 1);
                env.AssertListener(
                    "s0",
                    listener => ClassicAssert.IsFalse(listener.GetAndClearIsInvoked(), "Failed for flip " + count));

                ClassicAssert.IsFalse(env.Listener("s0").GetAndClearIsInvoked(), "Failed for flip " + count);

                env.AdvanceTime(flipTime);
                env.AssertListener(
                    "s0",
                    listener => ClassicAssert.IsTrue(listener.GetAndClearIsInvoked(), "Failed for flip " + count));
                count++;
            }

            env.AdvanceTime(long.MaxValue);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static long TimePlusMonth(
            long timeInMillis,
            int monthToAdd)
        {
            return DateTimeEx
                .GetInstance(TimeZoneInfo.Utc, timeInMillis)
                .AddMonths(monthToAdd)
                .UtcMillis;
        }
    }
} // end of namespace