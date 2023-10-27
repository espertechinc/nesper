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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternGuardWhile
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithGuardWhileSimple(execs);
            WithOp(execs);
            WithVariable(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOp());
            return execs;
        }

        public static IList<RegressionExecution> WithGuardWhileSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternGuardWhileSimple());
            return execs;
        }

        public class PatternGuardWhileSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl =
                    "@name('s0') select a.TheString as c0 from pattern [(every a=SupportBean) while (a.TheString like 'E%')]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                SendSupportBean(env, "X");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                SendSupportBean(env, "E3");
                SendSupportBean(env, "X");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase = null;

                testCase = new EventExpressionCase("a=SupportBean_A -> (every b=SupportBean_B) while(b.Id != 'B2')");
                testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A -> (every b=SupportBean_B) while(b.Id != 'B3')");
                testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
                testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) while(b.Id != 'B3')");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [(every b=SupportBean_B) while (b.Id!=\"B3\")]";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                model = env.CopyMayFail(model);
                Expression guardExpr = Expressions.Neq("b.Id", "B3");
                PatternExpr every = Patterns.Every(Patterns.Filter(Filter.Create("SupportBean_B"), "b"));
                PatternExpr patternGuarded = Patterns.WhileGuard(every, guardExpr);
                model.FromClause = FromClause.Create(PatternStream.Create(patternGuarded));
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every b=SupportBean_B) while(b.Id != 'B1')");
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('var') @public create variable boolean myVariable = true", path);

                var expression =
                    "@name('s0') select * from pattern [every a=SupportBean(TheString like 'A%') -> (every b=SupportBean(TheString like 'B%')) while (myVariable)]";
                env.CompileDeploy(expression, path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B1", 100));
                env.AssertListener("s0", listener => Assert.AreEqual(2, listener.GetAndResetLastNewData().Length));

                env.RuntimeSetVariable("var", "myVariable", false);

                env.SendEventBean(new SupportBean("A3", 3));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A4", 4));
                env.SendEventBean(new SupportBean("B2", 200));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from pattern [every SupportBean while ('abc')]",
                    "Invalid parameter for pattern guard 'SupportBean while (\"abc\")': Expression pattern guard requires a single expression as a parameter returning a true or false (boolean) value [select * from pattern [every SupportBean while ('abc')]]");
                env.TryInvalidCompile(
                    "select * from pattern [every SupportBean while (abc)]",
                    "Failed to validate pattern guard expression 'abc': Property named 'abc' is not valid in any stream [select * from pattern [every SupportBean while (abc)]]");
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }
    }
} // end of namespace