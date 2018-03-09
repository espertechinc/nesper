///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternMicrosecondResolution : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            IDictionary<TimeUnit, EPServiceProvider> epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
    
            long time = DateTimeParser.ParseDefaultMSec("2002-05-30T09:00:00.000");
            long currentTime = DateTimeHelper.CurrentTimeMillis;
            EPServiceProvider engineMillis = epServices.Get(TimeUnit.MILLISECONDS);
            EPServiceProvider engineMicros = epServices.Get(TimeUnit.MICROSECONDS);
    
            RunAssertionPattern(engineMillis, 0, "timer:interval(1)", 1000);
            RunAssertionPattern(engineMicros, 0, "timer:interval(1)", 1000000);
    
            RunAssertionPattern(engineMillis, 0, "timer:interval(10 sec 5 msec)", 10005);
            RunAssertionPattern(engineMicros, 0, "timer:interval(10 sec 5 msec 1 usec)", 10005001);
    
            RunAssertionPattern(engineMillis, 0, "timer:interval(1 month 10 msec)", TimePlusMonth(0, 1) + 10);
            RunAssertionPattern(engineMicros, 0, "timer:interval(1 month 10 usec)", TimePlusMonth(0, 1) * 1000 + 10);
    
            RunAssertionPattern(engineMillis, currentTime, "timer:interval(1 month 50 msec)", TimePlusMonth(currentTime, 1) + 50);
            RunAssertionPattern(engineMicros, currentTime * 1000 + 33, "timer:interval(3 month 100 usec)", TimePlusMonth(currentTime, 3) * 1000 + 33 + 100);
    
            RunAssertionPattern(engineMillis, time, "timer:at(1, *, *, *, *, *)", time + 60000);
            RunAssertionPattern(engineMicros, time * 1000 + 123, "timer:at(1, *, *, *, *, *)", time * 1000 + 60000000 + 123);
    
            // Schedule Date-only
            RunAssertionPattern(engineMillis, time, "timer:schedule(iso:'2002-05-30T09:01:00')", time + 60000);
            RunAssertionPattern(engineMicros, time * 1000 + 123, "timer:schedule(iso:'2002-05-30T09:01:00')", time * 1000 + 60000000);
    
            // Schedule Period-only
            RunAssertionPattern(engineMillis, time, "every timer:schedule(period: 2 minute)", time + 120000);
            RunAssertionPattern(engineMicros, time * 1000 + 123, "every timer:schedule(period: 2 minute)", time * 1000 + 123 + 120000000);
    
            // Schedule Date+period
            RunAssertionPattern(engineMillis, time, "every timer:schedule(iso:'2002-05-30T09:00:00/PT1M')", time + 60000);
            RunAssertionPattern(engineMicros, time * 1000 + 345, "every timer:schedule(iso:'2002-05-30T09:00:00/PT1M')", time * 1000 + 60000000);
    
            // Schedule recurring period
            RunAssertionPattern(engineMillis, time, "every timer:schedule(iso:'R2/PT1M')", time + 60000, time + 120000);
            RunAssertionPattern(engineMicros, time * 1000 + 345, "every timer:schedule(iso:'R2/PT1M')", time * 1000 + 345 + 60000000, time * 1000 + 345 + 120000000);
    
            // Schedule date+recurring period
            RunAssertionPattern(engineMillis, time, "every timer:schedule(iso:'R2/2002-05-30T09:01:00/PT1M')", time + 60000, time + 120000);
            RunAssertionPattern(engineMicros, time * 1000 + 345, "every timer:schedule(iso:'R2/2002-05-30T09:01:00/PT1M')", time * 1000 + 60000000, time * 1000 + 120000000);
    
            // Schedule with date computation
            RunAssertionPattern(engineMillis, time, "timer:schedule(date: current_timestamp.WithTime(9, 1, 0, 0))", time + 60000);
            RunAssertionPattern(engineMicros, time * 1000 + 345, "timer:schedule(date: current_timestamp.WithTime(9, 1, 0, 0))", time * 1000 + 345 + 60000000);
        }
    
        private void RunAssertionPattern(EPServiceProvider epService, long startTime, string patternExpr, params long[] flipTimes) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("iso");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));
    
            var listener = new SupportUpdateListener();
            EPStatement stmt = isolated.EPAdministrator.CreateEPL("select * from pattern[" + patternExpr + "]", "s0", null);
            stmt.Events += listener.Update;
    
            int count = 0;
            foreach (long flipTime in flipTimes) {
                isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
                Assert.IsFalse(listener.GetAndClearIsInvoked(), "Failed for flip " + count);
    
                isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
                Assert.IsTrue(listener.GetAndClearIsInvoked(), "Failed for flip " + count);
                count++;
            }
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(Int64.MaxValue));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            isolated.Dispose();
        }
    
        private static long TimePlusMonth(long timeInMillis, int monthToAdd) {
            DateTimeEx cal = DateTimeEx.GetInstance(TimeZoneInfo.Local, timeInMillis);
            cal.AddMonths(monthToAdd, DateTimeMathStyle.Java);
            return cal.TimeInMillis;
        }
    }
} // end of namespace
