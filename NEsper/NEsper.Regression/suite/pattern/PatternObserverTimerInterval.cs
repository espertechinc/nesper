///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class PatternObserverTimerInterval
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new PatternOp());
            execs.Add(new PatternIntervalSpec());
            execs.Add(new PatternIntervalSpecVariables());
            execs.Add(new PatternIntervalSpecExpression());
            execs.Add(new PatternIntervalSpecExpressionWithProperty());
            execs.Add(new PatternIntervalSpecPreparedStmt());
            execs.Add(new PatternMonthScoped());
            execs.Add(new PatternIntervalSpecExpressionWithPropertyArray());
            return execs;
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
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

        internal class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                // The wait is done when 2 seconds passed
                testCase = new EventExpressionCase("timer:interval(1999 msec)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [timer:interval(1.999d)]";
                var model = new EPStatementObjectModel();
                model.Select(SelectClause.CreateWildcard());
                PatternExpr pattern = Patterns.TimerInterval(1.999d);
                model.SetFrom(FromClause.Create(PatternStream.Create(pattern)));
                model = env.CopyMayFail(model);
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(2 sec)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                // 3 seconds (>2001 microseconds) passed
                testCase = new EventExpressionCase("timer:interval(2.001)");
                testCase.Add("C1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(2999 milliseconds)");
                testCase.Add("C1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(3 seconds)");
                testCase.Add("C1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(3.001 seconds)");
                testCase.Add("B2");
                testCaseList.AddTest(testCase);

                // Try with an all ... repeated timer every 3 seconds
                testCase = new EventExpressionCase("every timer:interval(3.001 sec)");
                testCase.Add("B2");
                testCase.Add("F1");
                testCase.Add("D3");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every timer:interval(5000 msec)");
                testCase.Add("A2");
                testCase.Add("B3");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(3.999 second) -> b=SupportBean_B");
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(4 sec) -> b=SupportBean_B");
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(4.001 sec) -> b=SupportBean_B");
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(0) -> b=SupportBean_B");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                // Try with an followed-by as a second argument
                testCase = new EventExpressionCase("b=SupportBean_B => timer:interval(0.001)");
                testCase.Add("C1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B => timer:interval(0)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B => timer:interval(1 sec)");
                testCase.Add("C1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B => timer:interval(1.001)");
                testCase.Add("B2", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                // Try in a 3-way followed by
                testCase = new EventExpressionCase("b=SupportBean_B() => timer:interval(6.000) => d=SupportBean_D");
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B() => timer:interval(2.001) => d=SupportBean_D())");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B() => timer:interval(2.000) => d=SupportBean_D())");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                // Try with an "or"
                testCase = new EventExpressionCase("b=SupportBean_B() or timer:interval(1.001)");
                testCase.Add("B1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B() or timer:interval(2.001)");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B(Id='B3') or timer:interval(8.500)");
                testCase.Add("D2");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(8.500) or timer:interval(7.500)");
                testCase.Add("F1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(999999 msec) or g=SupportBean_G");
                testCase.Add("G1", "g", events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                // Try with an "and"
                testCase = new EventExpressionCase("b=SupportBean_B() and timer:interval(4000 msec)");
                testCase.Add("B2", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B() and timer:interval(4001 msec)");
                testCase.Add("A2", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(9999999 msec) and b=SupportBean_B");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(1 msec) and b=SupportBean_B(Id='B2')");
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                // Try with an "within"
                testCase = new EventExpressionCase("timer:interval(3.000) where timer:within(2.000)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("timer:interval(3.000) where timer:within (3.000)");
                testCaseList.AddTest(testCase);

                // Run all tests
                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }

            // As of release 1.6 this no longer updates listeners when the statement is started.
            // The reason is that the dispatch view only gets attached after a pattern started, therefore
            // ZeroDepthEventStream looses the event.
            // There should be no use case requiring this
            // <para />testCase = new EventExpressionCase("not timer:interval(5000 millisecond)");
            // testCase.Add(EventCollection.ON_START_EVENT_ID);
            // testCaseList.AddTest(testCase);
            // </summary>
        }

        internal class PatternIntervalSpec : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                env.CompileDeploy("@Name('s0') select * from pattern [timer:interval(1 minute 2 seconds)]");
                env.AddListener("s0");

                SendTimer(62 * 1000 - 1, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(62 * 1000, env);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternIntervalSpecVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                var path = new RegressionPath();
                env.CompileDeploy("create variable double M_isv=1", path);
                env.CompileDeploy("create variable double S_isv=2", path);
                env.CompileDeploy(
                    "@Name('s0') select * from pattern [timer:interval(M_isv minute S_isv seconds)]",
                    path);
                env.AddListener("s0");

                SendTimer(62 * 1000 - 1, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(62 * 1000, env);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternIntervalSpecExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                var path = new RegressionPath();
                env.CompileDeploy("create variable double MOne=1", path);
                env.CompileDeploy("create variable double SOne=2", path);
                env.CompileDeploy("@Name('s0') select * from pattern [timer:interval(MOne*60+SOne seconds)]", path);
                env.AddListener("s0");

                SendTimer(62 * 1000 - 1, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(62 * 1000, env);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternIntervalSpecExpressionWithProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                env.CompileDeploy(
                    "@Name('s0') select a.TheString as Id from pattern [every a=SupportBean => timer:interval(IntPrimitive seconds)]");
                env.AddListener("s0");

                SendTimer(10000, env);
                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 2));

                SendTimer(11999, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(12000, env);
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                SendTimer(12999, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(13000, env);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class PatternIntervalSpecExpressionWithPropertyArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                env.CompileDeploy(
                    "@Name('s0') select a[0].TheString as a0Id, a[1].TheString as a1Id from pattern [ [2] a=SupportBean => timer:interval(a[0].IntPrimitive+a[1].IntPrimitive seconds)]");
                env.AddListener("s0");

                SendTimer(10000, env);
                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 2));

                SendTimer(14999, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(15000, env);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a0Id,a1Id".SplitCsv(),
                    "E1,E2".SplitCsv());

                env.UndeployAll();
            }
        }

        internal class PatternIntervalSpecPreparedStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // External clocking
                SendTimer(0, env);

                // Set up a timer:within
                var compiled = env
                    .Compile("@Name('s0') select * from pattern [timer:interval(?::int minute ?::int seconds)]");
                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(
                        prepared => {
                            prepared.SetObject(1, 1);
                            prepared.SetObject(2, 2);
                        }));
                env.AddListener("s0");

                SendTimer(62 * 1000 - 1, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(62 * 1000, env);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                env.CompileDeploy("@Name('s0') select * from pattern [timer:interval(1 month)]").AddListener("s0");

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
}