///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternObserverTimerSchedule
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithTimerScheduleSimple(execs);
            WithObserverTimerScheduleMultiform(execs);
            WithTimerScheduleLimitedWDateAndPeriod(execs);
            WithTimerScheduleJustDate(execs);
            WithTimerScheduleJustPeriod(execs);
            WithTimerScheduleDateWithPeriod(execs);
            WithTimerScheduleUnlimitedRecurringPeriod(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleUnlimitedRecurringPeriod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleUnlimitedRecurringPeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleDateWithPeriod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleDateWithPeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleJustPeriod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleJustPeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleJustDate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleJustDate());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleLimitedWDateAndPeriod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleLimitedWDateAndPeriod());
            return execs;
        }

        public static IList<RegressionExecution> WithObserverTimerScheduleMultiform(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternObserverTimerScheduleMultiform());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerScheduleSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerScheduleSimple());
            return execs;
        }

        public class PatternTimerScheduleSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-01T9:00:00.000");
                var epl = "@name('s0') select * from pattern [every timer:schedule(period:1 day, repetitions: 3)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEvent(env, "2002-05-02T8:59:59.999");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEvent(env, "2002-05-02T9:00:00.000");
                env.AssertListenerInvoked("s0");

                env.Milestone(2);

                SendTimeEvent(env, "2002-05-03T8:59:59.999");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimeEvent(env, "2002-05-03T9:00:00.000");
                env.AssertListenerInvoked("s0");

                env.Milestone(4);

                SendTimeEvent(env, "2002-05-04T8:59:59.999");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                SendTimeEvent(env, "2002-05-04T9:00:00.000");
                env.AssertListenerInvoked("s0");

                env.Milestone(6);

                env.Milestone(7);

                SendTimeEvent(env, "2002-05-30T9:00:00.000");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternObserverTimerScheduleMultiform : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // just-date: "<date>" : non-recurring, typically a future start time, no period
                RunAssertionJustFutureDate(env, milestone);
                RunAssertionJustPastDate(env, milestone);

                // just-period: "P<...>" : non-recurring
                RunAssertionJustPeriod(env, milestone);

                // partial-form-2: "<date>/P<period>": non-recurring, no start date (starts from current date), with period
                TryAssertionDateWithPeriod(env, milestone);

                // partial-form-1: "R<?>/P<period>": recurring, no start date (starts from current date), with period
                RunAssertionRecurringLimitedWithPeriod(env, milestone);
                RunAssertionRecurringUnlimitedWithPeriod(env, milestone);
                RunAssertionRecurringAnchoring(env, milestone);

                // full form: "R<?>/<date>/P<period>" : recurring, start time, with period
                RunAssertionFullFormLimitedFutureDated(env, milestone);
                RunAssertionFullFormLimitedPastDated(env, milestone);
                RunAssertionFullFormUnlimitedFutureDated(env, milestone);
                RunAssertionFullFormUnlimitedPastDated(env, milestone);
                RunAssertionFullFormUnlimitedPastDatedAnchoring(env, milestone);

                // equivalent formulations
                RunAssertionEquivalent(env);

                // invalid tests
                RunAssertionInvalid(env);

                // followed-by
                RunAssertionFollowedBy(env, milestone);
                RunAssertionFollowedByDynamicallyComputed(env, milestone);

                // named parameters
                RunAssertionNameParameters(env);

                // For Testing, could also use this:
            }
        }

        public class PatternTimerScheduleLimitedWDateAndPeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEventWithZone(env, "2012-10-01T05:51:00.000GMT-0:00");

                // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
                var epl =
                    "@name('s0') select * from pattern[every timer:schedule(iso: 'R3/2012-10-01T05:52:00Z/PT2S')]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEventWithZone(env, "2012-10-01T5:51:59.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEventWithZone(env, "2012-10-01T5:52:00.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(2);

                SendTimeEventWithZone(env, "2012-10-01T5:52:01.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimeEventWithZone(env, "2012-10-01T5:52:02.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(4);

                SendTimeEventWithZone(env, "2012-10-01T5:52:03.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                SendTimeEventWithZone(env, "2012-10-01T5:52:04.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(6);

                SendTimeEventWithZone(env, "2012-10-01T5:53:00.000GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        public class PatternTimerScheduleJustDate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEventWithZone(env, "2012-10-01T5:51:00.000GMT-0:00");

                // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
                var epl = "@name('s0') select * from pattern[every timer:schedule(date: '2012-10-02T00:00:00Z')]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEventWithZone(env, "2012-10-01T23:59:59.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEventWithZone(env, "2012-10-02T0:0:00.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(2);

                SendTimeEventWithZone(env, "2012-10-10T0:0:00.000GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        public class PatternTimerScheduleJustPeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEventWithZone(env, "2012-10-01T5:51:00.000GMT-0:00");

                // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
                var epl = "@name('s0') select * from pattern[every timer:schedule(period: 1 minute)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEventWithZone(env, "2012-10-01T5:51:59.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEventWithZone(env, "2012-10-02T5:52:00.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        public class PatternTimerScheduleDateWithPeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEventWithZone(env, "2012-10-01T5:51:00.000GMT-0:00");

                // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
                var epl =
                    "@name('s0') select * from pattern[every timer:schedule(period: 1 day, date: '2012-10-02T00:00:00Z')]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEventWithZone(env, "2012-10-02T23:59:59.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEventWithZone(env, "2012-10-03T0:0:00.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        public class PatternTimerScheduleUnlimitedRecurringPeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEventWithZone(env, "2012-10-01T5:51:00.000GMT-0:00");

                var epl = "@name('s0') select * from pattern[every timer:schedule(repetitions:-1, period: 1 sec)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEventWithZone(env, "2012-10-01T5:51:0.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimeEventWithZone(env, "2012-10-01T5:51:1.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(2);

                SendTimeEventWithZone(env, "2012-10-01T5:51:1.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimeEventWithZone(env, "2012-10-01T5:51:2.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.Milestone(4);

                SendTimeEventWithZone(env, "2012-10-01T5:51:2.999GMT-0:00");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                SendTimeEventWithZone(env, "2012-10-01T5:51:3.000GMT-0:00");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void RunAssertionFollowedByDynamicallyComputed(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:07.000GMT-0:00");

            var epl =
                "@name('s0') select * from pattern[every sb=SupportBean -> timer:schedule(iso: computeISO8601String(sb))]";
            env.CompileDeploy(epl).AddListener("s0");

            var b1 = MakeSendEvent(env, "E1", 5);

            SendCurrentTime(env, "2012-10-01T05:51:9.999GMT-0:00");
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-10-01T05:51:10.000GMT-0:00");
            env.AssertEqualsNew("s0", "sb", b1);

            env.UndeployAll();
        }

        private static void RunAssertionFollowedBy(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:07.000GMT-0:00");

            var epl =
                "@name('s0') select * from pattern[every sb=SupportBean -> timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S')]";
            env.CompileDeploy(epl).AddListener("s0");

            var b1 = MakeSendEvent(env, "E1");

            SendCurrentTime(env, "2012-10-01T05:51:14.999GMT-0:00");
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-10-01T05:51:15.000GMT-0:00");
            env.AssertEqualsNew("s0", "sb", b1);

            SendCurrentTime(env, "2012-10-01T05:51:16.000GMT-0:00");
            var b2 = MakeSendEvent(env, "E2");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-10-01T05:51:18.000GMT-0:00");
            var b3 = MakeSendEvent(env, "E3");

            SendCurrentTime(env, "2012-10-01T05:51:30.000GMT-0:00");
            env.AssertPropsPerRowLastNew(
                "s0",
                "sb".SplitCsv(),
                new object[][] { new object[] { b2 }, new object[] { b3 } });

            env.UndeployAll();
        }

        private static void RunAssertionInvalid(RegressionEnvironment env)
        {
            // the ISO 8601 parse tests reside with the parser
            env.TryInvalidCompile(
                "select * from pattern[every timer:schedule(iso: 'x')]",
                "Invalid parameter for pattern observer 'timer:schedule(iso:\"x\")': Failed to parse 'x': Exception parsing date 'x', the date is not a supported ISO 8601 date");

            // named parameter tests: absence, typing, etc.
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule()]",
                "Invalid parameter for pattern observer 'timer:schedule()': No parameters provided");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(x:1)]",
                "Invalid parameter for pattern observer 'timer:schedule(x:1)': Unexpected named parameter 'x', expecting any of the following: [\"iso\", \"repetitions\", \"date\", \"period\"]");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(period:1)]",
                "Invalid parameter for pattern observer 'timer:schedule(period:1)': Failed to validate named parameter 'period', expected a single expression returning a TimePeriod-typed value");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(repetitions:'a', period:1 seconds)]",
                "Invalid parameter for pattern observer 'timer:schedule(repetitions:\"a\",period:1 seconds)': Failed to validate named parameter 'repetitions', expected a single expression returning any of the following types: int,long");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(date:1 seconds)]",
                "Invalid parameter for pattern observer 'timer:schedule(date:1 seconds)': Failed to validate named parameter 'date', expected a single expression returning any of the following types: string,DateTimeEx,DateTimeOffset,DateTime,long");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(repetitions:1)]",
                "Invalid parameter for pattern observer 'timer:schedule(repetitions:1)': Either the date or period parameter is required");
            env.TryInvalidCompile(
                "select * from pattern[timer:schedule(iso: 'R/1980-01-01T00:00:00Z/PT15S', repetitions:1)]",
                "Invalid parameter for pattern observer 'timer:schedule(iso:\"R/1980-01-01T00:00:00Z/PT15S\",repetitions:1)': The 'iso' parameter is exclusive of other parameters");
        }

        private static void RunAssertionEquivalent(RegressionEnvironment env)
        {
            var first =
                "@name('s0') select * from pattern[every timer:schedule(iso: 'R2/2008-03-01T13:00:00Z/P1Y2M10DT2H30M')]";
            TryAssertionEquivalent(env, first);

            var second = "@name('s0') select * from pattern[every " +
                         "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
                         " timer:schedule(iso: '2009-05-11T15:30:00Z'))]";
            TryAssertionEquivalent(env, second);

            var third = "@name('s0') select * from pattern[every " +
                        "(timer:schedule(iso: '2008-03-01T13:00:00Z') or" +
                        " timer:schedule(iso: '2008-03-01T13:00:00Z/P1Y2M10DT2H30M'))]";
            TryAssertionEquivalent(env, third);
        }

        private static void TryAssertionEquivalent(
            RegressionEnvironment env,
            string epl)
        {
            SendCurrentTime(env, "2001-10-01T05:51:00.000GMT-0:00");

            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2008-03-01T13:00:00.000GMT-0:00");
            AssertReceivedAtTime(env, "2009-05-11T15:30:00.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:52:04.000GMT-0:00");

            env.UndeployAll();
        }

        private static void TryAssertionDateWithPeriod(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionDateWithPeriod(env, "iso: '2012-10-01T05:52:00Z/PT2S'", milestone);
            TryAssertionDateWithPeriod(env, "date: '2012-10-01T05:52:00Z', period: 2 seconds", milestone);
        }

        private static void TryAssertionDateWithPeriod(
            RegressionEnvironment env,
            string parameters,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:00.000GMT-0:00");

            // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
            var epl = "@name('s0') select * from pattern[timer:schedule(" + parameters + ")]";
            env.CompileDeploy(epl).AddListener("s0");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:02.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:52:04.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionFullFormLimitedFutureDated(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionFullFormLimitedFutureDated(env, true, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'", milestone);
            TryAssertionFullFormLimitedFutureDated(env, false, "iso: 'R3/2012-10-01T05:52:00Z/PT2S'", milestone);
            TryAssertionFullFormLimitedFutureDated(
                env,
                false,
                "repetitions: 3L, date:'2012-10-01T05:52:00Z', period: 2 seconds",
                milestone);
        }

        private static void TryAssertionFullFormLimitedFutureDated(
            RegressionEnvironment env,
            bool audit,
            string parameters,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:00.000GMT-0:00");

            // Repeat 3 times, starting "2012-10-01T05:52:00Z" (UTC), period of 2 seconds
            var epl = (audit ? "@Audit " : "") +
                      "@name('s0') select * from pattern[every timer:schedule(" +
                      parameters +
                      ")]";
            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:52:02.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:04.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:52:06.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionJustFutureDate(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionJustFutureDate(env, true, "iso: '2012-10-01T05:52:00Z'", milestone);
            TryAssertionJustFutureDate(env, false, "iso: '2012-10-01T05:52:00Z'", milestone);
            TryAssertionJustFutureDate(env, false, "date: '2012-10-01T05:52:00Z'", milestone);
        }

        private static void TryAssertionJustFutureDate(
            RegressionEnvironment env,
            bool hasEvery,
            string parameters,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:00.000GMT-0:00");

            // Fire once at "2012-10-01T05:52:00Z" (UTC)
            var epl = "@name('s0') select * from pattern[" +
                      (hasEvery ? "every " : "") +
                      "timer:schedule(" +
                      parameters +
                      ")]";
            env.CompileDeploy(epl).AddListener("s0");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:53:00.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionJustPastDate(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionJustPastDate(env, true, milestone);
            TryAssertionJustPastDate(env, false, milestone);
        }

        private static void TryAssertionJustPastDate(
            RegressionEnvironment env,
            bool hasEvery,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:00.000GMT-0:00");

            // Fire once at "2012-10-01T05:52:00Z" (UTC)
            var epl = "@name('s0') select * from pattern[" +
                      (hasEvery ? "every " : "") +
                      "timer:schedule(iso: '2010-10-01T05:52:00Z')]";
            env.CompileDeploy(epl).AddListener("s0");

            env.MilestoneInc(milestone);

            AssertSendNoMoreCallback(env, "2012-10-01T05:53:00.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionJustPeriod(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionJustPeriod(env, "iso:'P1DT2H'", milestone);
            TryAssertionJustPeriod(env, "period: 1 day 2 hours", milestone);
        }

        private static void TryAssertionJustPeriod(
            RegressionEnvironment env,
            string parameters,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2012-10-01T05:51:00.000GMT-0:00");

            // Fire once after 1 day and 2 hours
            var epl = "@name('s0') select * from pattern[timer:schedule(" + parameters + ")]";
            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-02T07:51:00.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertSendNoMoreCallback(env, "2012-10-03T09:51:00.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionRecurringLimitedWithPeriod(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            TryAssertionRecurringLimitedWithPeriod(env, "iso:'R3/PT2S'", milestone);
            TryAssertionRecurringLimitedWithPeriod(env, "repetitions:3L, period: 2 seconds", milestone);
        }

        private static void TryAssertionRecurringLimitedWithPeriod(
            RegressionEnvironment env,
            string parameters,
            AtomicLong milestone)
        {
            // Fire 3 times after 2 seconds from current time
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(" + parameters + ")]";
            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:52:04.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:06.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:52:08.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionRecurringUnlimitedWithPeriod(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Fire 3 times after 2 seconds from current time
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(iso:'R/PT1M10S')]";

            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-01T05:53:10.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:54:20.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:55:30.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:56:40.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionFullFormUnlimitedPastDated(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT1S')]";

            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-01T05:52:01.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:52:02.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:03.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionNameParameters(RegressionEnvironment env)
        {
            TryAssertionNameParameters(env, "repetitions:-1L, date:'1980-01-01T00:00:00Z', period: 1 seconds");
            TryAssertionNameParameters(env, "repetitions:-1, date:getThe1980Calendar(), period: 1 seconds");
            TryAssertionNameParameters(env, "repetitions:-1, date:getThe1980Date(), period: getTheSeconds() seconds");
            TryAssertionNameParameters(env, "repetitions:-1, date:getThe1980Long(), period: 1 seconds");
            TryAssertionNameParameters(env, "repetitions:-1, date:getThe1980DateTimeOffset(), period: 1 seconds");
            TryAssertionNameParameters(env, "repetitions:-1, date:getThe1980DateTime(), period: 1 seconds");
        }

        private static void TryAssertionNameParameters(
            RegressionEnvironment env,
            string parameters)
        {
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(" + parameters + ")]";
            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2012-10-01T05:52:01.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:52:02.000GMT-0:00");
            AssertReceivedAtTime(env, "2012-10-01T05:52:03.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionFullFormUnlimitedPastDatedAnchoring(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(env, "2012-01-01T00:0:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(iso:'R/1980-01-01T00:00:00Z/PT10S')]";
            env.CompileDeploy(epl).AddListener("s0");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-01-01T00:0:15.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            SendCurrentTime(env, "2012-01-01T00:0:20.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-01-01T00:0:30.000GMT-0:00");

            SendCurrentTime(env, "2012-01-01T00:0:55.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            AssertReceivedAtTime(env, "2012-01-01T00:1:00.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionRecurringAnchoring(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(env, "2012-01-01T00:0:00.000GMT-0:00");

            var epl = "@name('s0') select * from pattern[every timer:schedule(iso: 'R/PT10S')]";
            env.CompileDeploy(epl).AddListener("s0");

            SendCurrentTime(env, "2012-01-01T00:0:15.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-01-01T00:0:20.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            AssertReceivedAtTime(env, "2012-01-01T00:0:30.000GMT-0:00");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2012-01-01T00:0:55.000GMT-0:00");
            env.AssertListenerInvoked("s0");

            AssertReceivedAtTime(env, "2012-01-01T00:1:00.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionFullFormLimitedPastDated(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Repeat unlimited number of times, reference-dated to "1980-01-01T00:00:00Z" (UTC), period of 1 second
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(iso: 'R8/2012-10-01T05:51:00Z/PT10S')]";
            env.CompileDeploy(epl).AddListener("s0");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2012-10-01T05:52:10.000GMT-0:00");
            AssertSendNoMoreCallback(env, "2012-10-01T05:52:20.000GMT-0:00");

            env.UndeployAll();
        }

        private static void RunAssertionFullFormUnlimitedFutureDated(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // Repeat unlimited number of times, reference-dated to future date, period of 1 day
            SendCurrentTime(env, "2012-10-01T05:52:00.000GMT-0:00");
            var epl = "@name('s0') select * from pattern[every timer:schedule(iso: 'R/2013-01-01T02:00:05Z/P1D')]";

            env.CompileDeploy(epl).AddListener("s0");

            AssertReceivedAtTime(env, "2013-01-01T02:00:05.000GMT-0:00");

            env.MilestoneInc(milestone);

            AssertReceivedAtTime(env, "2013-01-02T02:00:05.000GMT-0:00");
            AssertReceivedAtTime(env, "2013-01-03T02:00:05.000GMT-0:00");
            AssertReceivedAtTime(env, "2013-01-04T02:00:05.000GMT-0:00");

            env.UndeployAll();
        }

        private static void AssertSendNoMoreCallback(
            RegressionEnvironment env,
            string time)
        {
            SendCurrentTime(env, time);
            env.AssertListenerNotInvoked("s0");
            SendCurrentTime(env, "2999-01-01T00:0:00.000GMT-0:00");
            env.AssertListenerNotInvoked("s0");
        }

        private static void AssertReceivedAtTime(
            RegressionEnvironment env,
            string time)
        {
            var msec = DateTimeParsingFunctions.ParseDefaultMSecWZone(time);

            env.AdvanceTime(msec - 1);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(msec);
            env.AssertListener(
                "s0",
                listener => ClassicAssert.IsTrue(listener.IsInvokedAndReset(), "expected but not received at " + time));
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSecWZone(time));
        }

        private static SupportBean MakeSendEvent(
            RegressionEnvironment env,
            string theString)
        {
            return MakeSendEvent(env, theString, 0);
        }

        private static SupportBean MakeSendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            env.SendEventBean(b, b.GetType().Name);
            return b;
        }

        public static string ComputeISO8601String(SupportBean bean)
        {
            return "R/1980-01-01T00:00:00Z/PT" + bean.IntPrimitive + "S";
        }

        public static DateTimeEx GetThe1980Calendar()
        {
            var dateTimeEx = DateTimeEx.GetInstance(
                TimeZoneHelper.GetTimeZoneInfo("GMT-0:00"),
                DateTimeParsingFunctions.ParseDefaultMSecWZone("1980-01-01T00:0:0.000GMT-0:00"));
            return dateTimeEx;
        }

        public static DateTimeOffset GetThe1980Date()
        {
            return GetThe1980Calendar().DateTime;
        }

        public static DateTimeOffset GetThe1980DateTimeOffset()
        {
            return GetThe1980Calendar().DateTime;
        }

        public static DateTime GetThe1980DateTime()
        {
            return GetThe1980Calendar().DateTime.DateTime;
        }

        public static long GetThe1980Long()
        {
            return GetThe1980Calendar().UtcMillis;
        }

        public static int GetTheSeconds()
        {
            return 1;
        }

        private static void SendTimeEventWithZone(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSecWZone(time));
        }
    }
} // end of namespace