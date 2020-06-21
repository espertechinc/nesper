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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorAnd
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithWHarness(execs);
            WithWithEveryAndTerminationOptimization(execs);
            WithNotDefaultTrue(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNotDefaultTrue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorAndNotDefaultTrue());
            return execs;
        }

        public static IList<RegressionExecution> WithWithEveryAndTerminationOptimization(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorAndWithEveryAndTerminationOptimization());
            return execs;
        }

        public static IList<RegressionExecution> WithWHarness(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorAndWHarness());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorAndSimple());
            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        public class PatternOperatorAndSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};

                var epl =
                    "@Name('s0') select a.TheString as c0, b.TheString as c1 from pattern [a=SupportBean(IntPrimitive=0) and b=SupportBean(IntPrimitive=1)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "EB", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendSupportBean(env, "EA", 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"EA", "EB"});

                env.Milestone(2);
                SendSupportBean(env, "EB", 1);
                SendSupportBean(env, "EA", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);
                SendSupportBean(env, "EB", 1);
                SendSupportBean(env, "EA", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternOperatorAndNotDefaultTrue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-402
                var pattern =
                    "@Name('s0') insert into NumberOfWaitingCalls(calls) " +
                    " select count(*)" +
                    " from pattern[every call=SupportBean_A ->" +
                    " (not SupportBean_B(Id=call.Id) and" +
                    " not SupportBean_C(Id=call.Id))]";
                env.CompileDeploy(pattern).AddListener("s0");
                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternOperatorAndWithEveryAndTerminationOptimization : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // When all other sub-expressions to an AND are gone,
                // then there is no need to retain events of the subexpression still active

                var epl = "@Name('s0') select * from pattern [a=SupportBean_A and every b=SupportBean_B]";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean_A("A1"));
                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_B("B" + i));
                }

                env.AddListener("s0");
                env.SendEventBean(new SupportBean_B("B_last"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"a.Id", "b.Id"},
                    new object[] {"A1", "B_last"});

                env.UndeployAll();
            }
        }

        public class PatternOperatorAndWHarness : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("b=SupportBean_B and d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and every d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every(b=SupportBean_B and d=SupportBean_D" + ")");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.Every(
                    Patterns.And(Patterns.Filter("SupportBean_B", "b"), Patterns.Filter("SupportBean_D", "d")));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);
                Assert.AreEqual(
                    "select * from pattern [every (b=SupportBean_B and d=SupportBean_D" + ")]",
                    model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every( b=SupportBean_B and every d=SupportBean_D" + ")");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and every d=SupportBean_D");
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

                testCase = new EventExpressionCase("every( every b=SupportBean_B and d=SupportBean_D" + ")");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every a=SupportBean_A and d=SupportBean_D" + " and b=SupportBean_B");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every( every b=SupportBean_B and every d=SupportBean_D" + ")");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
                for (var i = 0; i < 3; i++) {
                    testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
                }

                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
                for (var i = 0; i < 5; i++) {
                    testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                }

                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A and d=SupportBean_D" + " and b=SupportBean_B");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every a=SupportBean_A and every d=SupportBean_D" + " and b=SupportBean_B");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and b=SupportBean_B");
                testCase.Add("B1", "b", events.GetEvent("B1"), "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every a=SupportBean_A and every d=SupportBean_D" + " and every b=SupportBean_B");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (a=SupportBean_A and every d=SupportBean_D" + " and b=SupportBean_B)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B and b=SupportBean_B)");
                testCase.Add("B1", "b", events.GetEvent("B1"), "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"), "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"), "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }
    }
} // end of namespace