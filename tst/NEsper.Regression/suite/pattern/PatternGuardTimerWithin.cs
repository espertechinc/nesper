///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternGuardTimerWithin
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOp(execs);
            WithInterval10Min(execs);
            WithInterval10MinVariable(execs);
            WithIntervalPrepared(execs);
            WithWithinFromExpression(execs);
            WithPatternNotFollowedBy(execs);
            WithWithinMayMaxMonthScoped(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithinMayMaxMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternWithinMayMaxMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternNotFollowedBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternPatternNotFollowedBy());
            return execs;
        }

        public static IList<RegressionExecution> WithWithinFromExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternWithinFromExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithIntervalPrepared(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternIntervalPrepared());
            return execs;
        }

        public static IList<RegressionExecution> WithInterval10MinVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInterval10MinVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithInterval10Min(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInterval10Min());
            return execs;
        }

        public static IList<RegressionExecution> WithOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOp());
            return execs;
        }

        private class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:within(2 sec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:within(2001 msec)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:within(1999 msec)");
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [b=SupportBean_B(Id=\"B3\") where timer:within(10.001d)]";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                model = env.CopyMayFail(model);
                Expression filter = Expressions.Eq("Id", "B3");
                PatternExpr pattern = Patterns.TimerWithin(
                    10.001,
                    Patterns.Filter(Filter.Create("SupportBean_B", filter), "b"));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') where timer:within(10001 msec)");
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') where timer:within(10 sec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') where timer:within(9.999)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:within(2.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:within(4.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B where timer:within(2.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B where timer:within(2001 msec))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every ((every b=SupportBean_B) where timer:within(2.001))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every ((every b=SupportBean_B) where timer:within(6.001))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:within(11.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:within(4001 milliseconds)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B) where timer:within(6.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B -> d=SupportBean_D where timer:within(4001 milliseconds)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B() -> d=SupportBean_D() where timer:within(4 sec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B() where timer:within (4.001) and d=SupportBean_D() where timer:within(6.001))");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2001 msec) and d=SupportBean_D() where timer:within(6001 msec)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2001 msec) and d=SupportBean_D() where timer:within(6000 msec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2000 msec) and d=SupportBean_D() where timer:within(6001 msec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every b=SupportBean_B -> d=SupportBean_D where timer:within(4000 msec)");
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every b=SupportBean_B() -> every d=SupportBean_D where timer:within(4000 msec)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() -> d=SupportBean_D() where timer:within(3999 msec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every b=SupportBean_B() -> (every d=SupportBean_D) where timer:within(2001 msec)");
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B() -> d=SupportBean_D()) where timer:within(6001 msec)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2000 msec) or d=SupportBean_D() where timer:within(6000 msec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(b=SupportBean_B() where timer:within (2000 msec) or d=SupportBean_D() where timer:within(6000 msec)) where timer:within (1999 msec)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B() where timer:within (2001 msec) and d=SupportBean_D() where timer:within(6001 msec))");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2001 msec) or d=SupportBean_D() where timer:within(6001 msec)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "b=SupportBean_B() where timer:within (2000 msec) or d=SupportBean_D() where timer:within(6001 msec)");
                testCase.Add("D1", "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every b=SupportBean_B() where timer:within (2001 msec) and every d=SupportBean_D() where timer:within(6001 msec)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(every b=SupportBean_B) where timer:within (2000 msec) and every d=SupportBean_D() where timer:within(6001 msec)");
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternInterval10Min : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);
                env.AssertRuntime(runtime => Assert.AreEqual(0, runtime.EventService.CurrentTime));

                // Set up a timer:within
                env.CompileDeploy(
                    "@name('s0') select * from pattern [(every SupportBean) where timer:within(1 days 2 hours 3 minutes 4 seconds 5 milliseconds)]");
                env.AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        private class PatternInterval10MinVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                var stmtText =
                    "@name('s0') select * from pattern [(every SupportBean) where timer:within(DD days HH hours MM minutes SS seconds MS milliseconds)]";
                env.CompileDeploy(stmtText).AddListener("s0");

                TryAssertion(env);

                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());

                env.UndeployAll();
            }
        }

        private class PatternIntervalPrepared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                var compiled = env.Compile(
                    "@name('s0') select * from pattern [(every SupportBean) where timer:within(?::int days ?::int hours ?::int minutes ?::int seconds ?::int milliseconds)]");
                var supportPortableDeploySubstitutionParams = new SupportPortableDeploySubstitutionParams()
                    .Add(1, 1)
                    .Add(2, 2)
                    .Add(3, 3)
                    .Add(4, 4)
                    .Add(5, 5);

                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(
                        supportPortableDeploySubstitutionParams.SetStatementParameters));
                env.AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        private class PatternWithinFromExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                env.CompileDeploy(
                    "@name('s0') select b.TheString as Id from pattern[a=SupportBean -> (every b=SupportBean) where timer:within(a.IntPrimitive seconds)]");
                env.AddListener("s0");

                // seed
                env.SendEventBean(new SupportBean("E1", 3));

                SendTimer(2000, env);
                env.SendEventBean(new SupportBean("E2", -1));
                env.AssertEqualsNew("s0", "Id", "E2");

                env.Milestone(0);

                SendTimer(2999, env);
                env.SendEventBean(new SupportBean("E3", -1));
                env.AssertEqualsNew("s0", "Id", "E3");

                env.Milestone(1);

                SendTimer(3000, env);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternPatternNotFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);

                var stmtText =
                    "@name('s0') select * from pattern [ every(SupportBean -> (SupportMarketDataBean where timer:within(5 sec))) ]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                SendTimer(6000, env);

                env.SendEventBean(new SupportBean("E4", 1));

                env.Milestone(1);

                env.SendEventBean(new SupportMarketDataBean("E5", "M1", 1d));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternWithinMayMaxMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionWithinMayMaxMonthScoped(env, false, milestone);
                TryAssertionWithinMayMaxMonthScoped(env, true, milestone);
            }
        }

        private static void TryAssertionWithinMayMaxMonthScoped(
            RegressionEnvironment env,
            bool hasMax,
            AtomicLong milestone)
        {
            SendCurrentTime(env, "2002-02-01T09:00:00.000");

            var epl = "@name('s0') select * from pattern [(every SupportBean) where " +
                      (hasMax ? "timer:withinmax(1 month, 10)]" : "timer:within(1 month)]");
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            env.AssertListenerInvoked("s0");

            SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
            env.SendEventBean(new SupportBean("E2", 0));
            env.AssertListenerInvoked("s0");

            env.MilestoneInc(milestone);

            SendCurrentTime(env, "2002-03-01T09:00:00.000");
            env.SendEventBean(new SupportBean("E3", 0));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            SendEvent(env);
            env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());

            long time = 24 * 60 * 60 * 1000 + 2 * 60 * 60 * 1000 + 3 * 60 * 1000 + 4 * 1000 + 5;
            SendTimer(time - 1, env);
            env.AssertRuntime(runtime => Assert.AreEqual(time - 1, runtime.EventService.CurrentTime));
            SendEvent(env);
            env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());

            env.Milestone(0);

            SendTimer(time, env);
            SendEvent(env);
            env.AssertListenerNotInvoked("s0");
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendEvent(RegressionEnvironment env)
        {
            var theEvent = new SupportBean();
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace