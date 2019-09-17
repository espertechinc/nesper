///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternObserverTimerAt
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new PatternTimerAtSimple());
            execs.Add(new PatternOp());
            execs.Add(new PatternCronParameter());
            execs.Add(new PatternAtWeekdays());
            execs.Add(new PatternAtWeekdaysPrepared());
            execs.Add(new PatternAtWeekdaysVariable());
            execs.Add(new PatternExpression());
            execs.Add(new PatternPropertyAndSODAAndTimezone());
            execs.Add(new PatternEvery15thMonth());
            return execs;
        }

        private static void RunSequenceIsolated(
            RegressionEnvironment env,
            string startTime,
            string epl,
            string[] times)
        {
            SendTime(env, startTime);

            env.CompileDeploy("@Name('s0') " + epl).AddListener("s0");
            RunSequence(env, times);

            env.UndeployAll();
        }

        private static void RunSequence(
            RegressionEnvironment env,
            string[] times)
        {
            foreach (var next in times) {
                // send right-before time
                var nextLong = DateTimeParsingFunctions.ParseDefaultMSec(next);
                env.AdvanceTime(nextLong - 1001);
                Assert.IsFalse(env.Listener("s0").IsInvoked, "unexpected callback at " + next);

                // send right-after time
                env.AdvanceTime(nextLong + 1000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked(), "missing callback at " + next);
            }
        }

        private static void SendTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
            dateTimeEx.SetMillis(0);
            dateTimeEx.Set(2008, 7, 3, 10); // start on a Sunday at 6am, August 3 2008

            var invocations = new List<string>();
            for (var i = 0; i < 24 * 60 * 7; i++) { // run for 1 week
                dateTimeEx.AddMinutes(1);
                SendTimer(dateTimeEx.UtcMillis, env);

                if (env.Listener("s0").GetAndClearIsInvoked()) {
                    // System.out.println("invoked at calendar " + cal.getTime().toString());
                    invocations.Add(dateTimeEx.ToString());
                }
            }

            var expectedResult = new string[5];
            dateTimeEx.Set(2008, 7, 4, 8); //"Mon Aug 04 08:00:00 EDT 2008"
            expectedResult[0] = dateTimeEx.ToString();
            dateTimeEx.Set(2008, 7, 5, 8); //"Tue Aug 05 08:00:00 EDT 2008"
            expectedResult[1] = dateTimeEx.ToString();
            dateTimeEx.Set(2008, 7, 6, 8); //"Wed Aug 06 08:00:00 EDT 2008"
            expectedResult[2] = dateTimeEx.ToString();
            dateTimeEx.Set(2008, 7, 7, 8); //"Thu Aug 07 08:00:00 EDT 2008"
            expectedResult[3] = dateTimeEx.ToString();
            dateTimeEx.Set(2008, 7, 8, 8); //"Fri Aug 08 08:00:00 EDT 2008"
            expectedResult[4] = dateTimeEx.ToString();
            EPAssertionUtil.AssertEqualsExactOrder(expectedResult, invocations.ToArray());
        }

        private static void SendTimeEvent(
            string time,
            RegressionEnvironment env)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void AddAll(EventExpressionCase desc)
        {
            desc.Add("A1");
            desc.Add("B1");
            desc.Add("C1");
            desc.Add("B2");
            desc.Add("A2");
            desc.Add("D1");
            desc.Add("E1");
            desc.Add("F1");
            desc.Add("D2");
            desc.Add("B3");
            desc.Add("G1");
            desc.Add("D3");
        }

        public class PatternTimerAtSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent("2002-05-30T9:00:00.000", env);
                var epl = "@Name('s0') select * from pattern [every timer:at(*,*,*,*,*)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendTimeEvent("2002-05-30T9:00:59.999", env);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(1);

                SendTimeEvent("2002-05-30T9:01:00.000", env);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(2);

                SendTimeEvent("2002-05-30T9:01:59.999", env);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendTimeEvent("2002-05-30T9:02:00.000", env);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class PatternEvery15thMonth : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("select * from pattern[every timer:at(*,*,*,*/15,*)]").UndeployAll();
            }
        }

        internal class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
                dateTimeEx.Set(2005, 3, 9, 8);
                dateTimeEx.SetMillis(0);
                var startTime = dateTimeEx.UtcMillis;

                // Start a 2004-12-9 8:00:00am and send events every 10 minutes
                // "A1"    8:10
                // "B1"    8:20
                // "C1"    8:30
                // "B2"    8:40
                // "A2"    8:50
                // "D1"    9:00
                // "E1"    9:10
                // "F1"    9:20
                // "D2"    9:30
                // "B3"    9:40
                // "G1"    9:50
                // "D3"   10:00

                var testData = EventCollectionFactory.GetEventSetOne(startTime, 1000 * 60 * 10);
                var testCaseList = new CaseList();
                EventExpressionCase testCase = null;

                testCase = new EventExpressionCase("timer:at(10, 8, *, *, *)");
                testCase.Add("A1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(10, 8, *, *, *, 1)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(5, 8, *, *, *)");
                testCase.Add("A1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(10, 8, *, *, *, *)");
                testCase.Add("A1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(25, 9, *, *, *)");
                testCase.Add("D2");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(11, 8, *, *, *)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(19, 8, *, *, *, 59)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(* / 5, *, *, *, *, *)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(*, *, *, *, *, * / 10)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(20, 8, *, *, *, 20)");
                testCase.Add("C1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(*, *, *, *, *)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(*, *, *, *, *, *)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(* / 9, *, *, *, *, *)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(* / 10, *, *, *, *, *)");
                AddAll(testCase);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:at(* / 30, *, *, *, *)");
                testCase.Add("C1");
                testCase.Add("D1");
                testCase.Add("D2");
                testCase.Add("D3");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(10, 9, *, *, *, 10) or timer:at(30, 9, *, *, *, *)");
                testCase.Add("F1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') -> timer:at(20, 9, *, *, *, *)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') -> timer:at(45, 9, *, *, *, *)");
                testCase.Add("G1", "b", testData.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(59, 8, *, *, *, 59) -> d=SupportBean_D");
                testCase.Add("D1", "d", testData.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(*, 9, *, *, *, 59) -> d=SupportBean_D");
                testCase.Add("D2", "d", testData.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "timer:at(22, 8, *, *, *) -> b=SupportBean_B(Id='B3') -> timer:at(55, *, *, *, *)");
                testCase.Add("D3", "b", testData.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(40, *, *, *, *, 1) and b=SupportBean_B");
                testCase.Add("A2", "b", testData.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(40, 9, *, *, *, 1) or d=SupportBean_D(Id='D3')");
                testCase.Add("G1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "timer:at(22, 8, *, *, *) -> b=SupportBean_B() -> timer:at(55, 8, *, *, *)");
                testCase.Add("D1", "b", testData.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(22, 8, *, *, *, 1) where timer:within(1 second)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(22, 8, *, *, *, 1) where timer:within(31 minutes)");
                testCase.Add("C1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(*, 9, *, *, *) and timer:at(55, *, *, *, *)");
                testCase.Add("D1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:at(40, 8, *, *, *, 1) and b=SupportBean_B");
                testCase.Add("A2", "b", testData.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [timer:at(10,8,*,*,*,*)]";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.TimerAt(10, 8, null, null, null, null);
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("A1");
                testCaseList.AddTest(testCase);

                // Run all tests
                var util = new PatternTestHarness(testData, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        internal class PatternAtWeekdays : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern [every timer:at(0,8,*,*,[1,2,3,4,5])]";

                var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
                dateTimeEx.SetMillis(0);
                dateTimeEx.Set(2008, 7, 3, 10); // start on a Sunday at 6am, August 3 2008
                SendTimer(dateTimeEx.UtcMillis, env);

                env.CompileDeploy(expression);
                env.AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class PatternAtWeekdaysPrepared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern [every timer:at(?::int,?::int,*,*,[1,2,3,4,5])]";

                var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
                dateTimeEx.SetMillis(0);
                dateTimeEx.Set(2008, 7, 3, 10); // start on a Sunday at 6am, August 3 2008
                SendTimer(dateTimeEx.UtcMillis, env);

                var compiled = env.Compile(expression);
                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(
                        prepared => {
                            prepared.SetObject(1, 0);
                            prepared.SetObject(2, 8);
                        }));
                env.AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class PatternAtWeekdaysVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern [every timer:at(VMIN,VHOUR,*,*,[1,2,3,4,5])]";

                var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
                dateTimeEx.SetMillis(0);
                dateTimeEx.Set(2008, 7, 3, 10); // start on a Sunday at 6am, August 3 2008
                SendTimer(dateTimeEx.UtcMillis, env);

                var compiled = env.Compile(expression);
                env.Deploy(compiled).AddListener("s0");
                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class PatternExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern [every timer:at(7+1-8,4+4,*,*,[1,2,3,4,5])]";

                var dateTimeEx = DateTimeEx.GetInstance(TimeZoneInfo.Utc);
                dateTimeEx.SetMillis(0);
                dateTimeEx.Set(2008, 7, 3, 10); // start on a Sunday at 6am, August 3 2008
                SendTimer(dateTimeEx.UtcMillis, env);

                env.CompileDeploy(expression).AddListener("s0");
                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class PatternPropertyAndSODAAndTimezone : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent("2008-08-3T06:00:00.000", env);
                var expression =
                    "@Name('s0') select * from pattern [a=SupportBean -> every timer:at(2*a.IntPrimitive,*,*,*,*)]";
                env.CompileDeploy(expression);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 20));

                SendTimeEvent("2008-08-3T06:39:59.000", env);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimeEvent("2008-08-3T06:40:00.000", env);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                env.UndeployAll();

                // test SODA
                var epl = "select * from pattern [every timer:at(*/VFREQ,VMIN:VMAX,1 last,*,[8,2:VMAX,*/VREQ])]";
                var model = env.EplToModel(epl);
                Assert.AreEqual(epl, model.ToEPL());

                // test timezone
                var baseUtcOffset = TimeZoneInfo.Utc.BaseUtcOffset;
                var expectedUtcOffset = TimeSpan.FromMilliseconds(-5 * 60 * 60 * 1000);
                if (baseUtcOffset.Equals(expectedUtcOffset)) {
                    // asserting only in EST timezone, see schedule util tests
                    SendTimeEvent("2008-01-4T06:50:00.000", env);
                    env.CompileDeploy("@Name('s0') select * from pattern [timer:at(0, 5, 4, 1, *, 0, 'PST')]")
                        .AddListener("s0");

                    SendTimeEvent("2008-01-4T07:59:59.999", env);
                    Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                    SendTimeEvent("2008-01-4T08:00:00.000", env);
                    Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                }

                env.CompileDeploy("select * from pattern [timer:at(0, 5, 4, 8, *, 0, 'xxx')]");
                env.CompileDeploy("select * from pattern [timer:at(0, 5, 4, 8, *, 0, *)]");

                env.UndeployAll();
            }
        }

        public class PatternCronParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                //
                // LAST
                //
                // Last day of the month, at 5pm
                RunSequenceIsolated(
                    env,
                    "2013-08-23T08:05:00.000",
                    "select * from pattern [ every timer:at(0, 17, last, *, *) ]",
                    new[] {
                        "2013-08-31T17:00:00.000",
                        "2013-09-30T17:00:00.000",
                        "2013-10-31T17:00:00.000",
                        "2013-11-30T17:00:00.000",
                        "2013-12-31T17:00:00.000",
                        "2014-01-31T17:00:00.000",
                        "2014-02-28T17:00:00.000",
                        "2014-03-31T17:00:00.000",
                        "2014-04-30T17:00:00.000",
                        "2014-05-31T17:00:00.000",
                        "2014-06-30T17:00:00.000"
                    });

                // Last day of the month, at the earliest
                RunSequenceIsolated(
                    env,
                    "2013-08-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, last, *, *) ]",
                    new[] {
                        "2013-08-31T00:00:00.000",
                        "2013-09-30T00:00:00.000",
                        "2013-10-31T00:00:00.000",
                        "2013-11-30T00:00:00.000",
                        "2013-12-31T00:00:00.000",
                        "2014-01-31T00:00:00.000",
                        "2014-02-28T00:00:00.000",
                        "2014-03-31T00:00:00.000",
                        "2014-04-30T00:00:00.000",
                        "2014-05-31T00:00:00.000",
                        "2014-06-30T00:00:00.000"
                    });

                // Last Sunday of the month, at 5pm
                RunSequenceIsolated(
                    env,
                    "2013-08-20T08:00:00.000",
                    "select * from pattern [ every timer:at(0, 17, *, *, 0 last, *) ]",
                    new[] {
                        "2013-08-25T17:00:00.000",
                        "2013-09-29T17:00:00.000",
                        "2013-10-27T17:00:00.000",
                        "2013-11-24T17:00:00.000",
                        "2013-12-29T17:00:00.000",
                        "2014-01-26T17:00:00.000",
                        "2014-02-23T17:00:00.000",
                        "2014-03-30T17:00:00.000",
                        "2014-04-27T17:00:00.000",
                        "2014-05-25T17:00:00.000",
                        "2014-06-29T17:00:00.000"
                    });

                // Last Friday of the month, any time
                // 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4= Thursday, 5=Friday, 6=Saturday
                RunSequenceIsolated(
                    env,
                    "2013-08-20T08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, *, 5 last, *) ]",
                    new[] {
                        "2013-08-30T00:00:00.000",
                        "2013-09-27T00:00:00.000",
                        "2013-10-25T00:00:00.000",
                        "2013-11-29T00:00:00.000",
                        "2013-12-27T00:00:00.000",
                        "2014-01-31T00:00:00.000",
                        "2014-02-28T00:00:00.000",
                        "2014-03-28T00:00:00.000"
                    });

                // Last day of week (Saturday)
                RunSequenceIsolated(
                    env,
                    "2013-08-01T08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, *, last, *) ]",
                    new[] {
                        "2013-08-03T00:00:00.000",
                        "2013-08-10T00:00:00.000",
                        "2013-08-17T00:00:00.000",
                        "2013-08-24T00:00:00.000",
                        "2013-08-31T00:00:00.000",
                        "2013-09-07T00:00:00.000"
                    });

                // Last day of month in August
                // For Java: January=0, February=1, March=2, April=3, May=4, June=5,
                //            July=6, August=7, September=8, November=9, October=10, December=11
                // For Esper: January=1, February=2, March=3, April=4, May=5, June=6,
                //            July=7, August=8, September=9, November=10, October=11, December=12
                RunSequenceIsolated(
                    env,
                    "2013-01-01T08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, last, 8, *, *) ]",
                    new[] {
                        "2013-08-31T00:00:00.000",
                        "2014-08-31T00:00:00.000",
                        "2015-08-31T00:00:00.000",
                        "2016-08-31T00:00:00.000"
                    });

                // Last day of month in Feb. (test leap year)
                RunSequenceIsolated(
                    env,
                    "2007-01-01T08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, last, 2, *, *) ]",
                    new[] {
                        "2007-02-28T00:00:00.000",
                        "2008-02-29T00:00:00.000",
                        "2009-02-28T00:00:00.000",
                        "2010-02-28T00:00:00.000",
                        "2011-02-28T00:00:00.000",
                        "2012-02-29T00:00:00.000",
                        "2013-02-28T00:00:00.000"
                    });

                // Observer for last Friday of each June (month 6)
                RunSequenceIsolated(
                    env,
                    "2007-01-01T08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, 6, 5 last, *) ]",
                    new[] {
                        "2007-06-29T00:00:00.000",
                        "2008-06-27T00:00:00.000",
                        "2009-06-26T00:00:00.000",
                        "2010-06-25T00:00:00.000",
                        "2011-06-24T00:00:00.000",
                        "2012-06-29T00:00:00.000",
                        "2013-06-28T00:00:00.000"
                    });

                //
                // LASTWEEKDAY
                //

                // Last weekday (last day that is not a weekend day)
                RunSequenceIsolated(
                    env,
                    "2013-08-23T08:05:00.000",
                    "select * from pattern [ every timer:at(0, 17, lastweekday, *, *) ]",
                    new[] {
                        "2013-08-30T17:00:00.000",
                        "2013-09-30T17:00:00.000",
                        "2013-10-31T17:00:00.000",
                        "2013-11-29T17:00:00.000",
                        "2013-12-31T17:00:00.000",
                        "2014-01-31T17:00:00.000",
                        "2014-02-28T17:00:00.000",
                        "2014-03-31T17:00:00.000",
                        "2014-04-30T17:00:00.000",
                        "2014-05-30T17:00:00.000",
                        "2014-06-30T17:00:00.000"
                    });

                // Last weekday, any time
                RunSequenceIsolated(
                    env,
                    "2013-08-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, *, *, *) ]",
                    new[] {
                        "2013-08-30T00:00:00.000",
                        "2013-09-30T00:00:00.000",
                        "2013-10-31T00:00:00.000",
                        "2013-11-29T00:00:00.000",
                        "2013-12-31T00:00:00.000",
                        "2014-01-31T00:00:00.000"
                    });

                // Observer for last weekday of September, for 2007 it's Friday September 28th
                RunSequenceIsolated(
                    env,
                    "2007-08-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, 9, *, *) ]",
                    new[] {
                        "2007-09-28T00:00:00.000",
                        "2008-09-30T00:00:00.000",
                        "2009-09-30T00:00:00.000",
                        "2010-09-30T00:00:00.000",
                        "2011-09-30T00:00:00.000",
                        "2012-09-28T00:00:00.000"
                    });

                // Observer for last weekday of February
                RunSequenceIsolated(
                    env,
                    "2007-01-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, 2, *, *) ]",
                    new[] {
                        "2007-02-28T00:00:00.000",
                        "2008-02-29T00:00:00.000",
                        "2009-02-27T00:00:00.000",
                        "2010-02-26T00:00:00.000",
                        "2011-02-28T00:00:00.000",
                        "2012-02-29T00:00:00.000"
                    });

                //
                // WEEKDAY
                //
                RunSequenceIsolated(
                    env,
                    "2007-01-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 1 weekday, 9, *, *) ]",
                    new[] {
                        "2007-09-03T00:00:00.000",
                        "2008-09-01T00:00:00.000",
                        "2009-09-01T00:00:00.000",
                        "2010-09-01T00:00:00.000",
                        "2011-09-01T00:00:00.000",
                        "2012-09-03T00:00:00.000",
                        "2013-09-02T00:00:00.000"
                    });

                RunSequenceIsolated(
                    env,
                    "2007-01-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 30 weekday, 9, *, *) ]",
                    new[] {
                        "2007-09-28T00:00:00.000",
                        "2008-09-30T00:00:00.000",
                        "2009-09-30T00:00:00.000",
                        "2010-09-30T00:00:00.000",
                        "2011-09-30T00:00:00.000",
                        "2012-09-28T00:00:00.000",
                        "2013-09-30T00:00:00.000"
                    });

                // nearest weekday for current month on the 10th
                RunSequenceIsolated(
                    env,
                    "2013-01-23T08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 10 weekday, *, *, *) ]",
                    new[] {
                        "2013-02-11T00:00:00.000",
                        "2013-03-11T00:00:00.000",
                        "2013-04-10T00:00:00.000",
                        "2013-05-10T00:00:00.000",
                        "2013-06-10T00:00:00.000",
                        "2013-07-10T00:00:00.000",
                        "2013-08-09T00:00:00.000"
                    });
            }
        }
    }
}