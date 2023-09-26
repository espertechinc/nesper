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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorNot
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOperatorNotWHarness(execs);
            WithOp(execs);
            WithUniformEvents(execs);
            WithNotFollowedBy(execs);
            WithNotTimeInterval(execs);
            WithNotWithEvery(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNotWithEvery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternNotWithEvery());
            return execs;
        }

        public static IList<RegressionExecution> WithNotTimeInterval(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternNotTimeInterval());
            return execs;
        }

        public static IList<RegressionExecution> WithNotFollowedBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternNotFollowedBy());
            return execs;
        }

        public static IList<RegressionExecution> WithUniformEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternUniformEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOp());
            return execs;
        }

        public static IList<RegressionExecution> WithOperatorNotWHarness(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorNotWHarness());
            return execs;
        }

        private class PatternOperatorNotWHarness : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("b=SupportBean_B and not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [every b=SupportBean_B and not g=SupportBean_G]";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.And()
                    .Add(Patterns.EveryFilter("SupportBean_B", "b"))
                    .Add(Patterns.NotFilter("SupportBean_G", "g"));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and not g=SupportBean_G");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and not a=SupportBean_A(id='A1')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and not a2=SupportBean_A(id='A2')");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B and not b3=SupportBean_B(id='B3'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B or not SupportBean_D())");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (every b=SupportBean_B and not SupportBean_B(id='B2'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B and not SupportBean_B(id='B2'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_A");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G(id='x')");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("b=SupportBean_B and not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                var text = "select * from pattern [every b=SupportBean_B and not g=SupportBean_G]";
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                PatternExpr pattern = Patterns.And()
                    .Add(Patterns.EveryFilter("SupportBean_B", "b"))
                    .Add(Patterns.NotFilter("SupportBean_G", "g"));
                model.FromClause = FromClause.Create(PatternStream.Create(pattern));
                model = env.CopyMayFail(model);
                Assert.AreEqual(text, model.ToEPL());
                testCase = new EventExpressionCase(model);
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and not g=SupportBean_G");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B and not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and not a=SupportBean_A(id='A1')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B and not a2=SupportBean_A(id='A2')");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B and not b3=SupportBean_B(id='B3'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B or not SupportBean_D())");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (every b=SupportBean_B and not SupportBean_B(id='B2'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B and not SupportBean_B(id='B2'))");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_A");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (b=SupportBean_B -> d=SupportBean_D) and " +
                    " not SupportBean_G(id='x')");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternUniformEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetSetTwoExternalClock(0, 1000);
                var results = new CaseList();

                var desc = new EventExpressionCase("every a=SupportBean_A() and not a1=SupportBean_A(id='A4')");
                desc.Add("B1", "a", events.GetEvent("B1"));
                desc.Add("B2", "a", events.GetEvent("B2"));
                desc.Add("B3", "a", events.GetEvent("B3"));
                results.AddTest(desc);

                var util = new PatternTestHarness(events, results);
                util.RunTest(env);
            }
        }

        private class PatternNotTimeInterval : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select A.theString as theString from pattern " +
                           "[every A=SupportBean(intPrimitive=123) -> (timer:interval(30 seconds) and not SupportMarketDataBean(volume=123, symbol=A.theString))]";
                env.CompileDeploy(text);

                env.AddListener("s0");

                SendTimer(0, env);
                env.SendEventBean(new SupportBean("E1", 123));

                SendTimer(10000, env);
                env.SendEventBean(new SupportBean("E2", 123));

                env.Milestone(0);

                SendTimer(20000, env);
                env.SendEventBean(new SupportMarketDataBean("E1", 0, 123L, ""));

                SendTimer(30000, env);
                env.SendEventBean(new SupportBean("E3", 123));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendTimer(40000, env);
                var fields = new string[] { "theString" };
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.UndeployAll();
            }
        }

        private class PatternNotFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select * from pattern [ every( SupportBean(intPrimitive>0) -> (SupportMarketDataBean and not SupportBean(intPrimitive=0) ) ) ]";
                env.CompileDeploy(stmtText);

                env.AddListener("s0");

                // A(a=1) A(a=2) A(a=0) A(a=3) ...
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportMarketDataBean("E5", "M1", 1d));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        public class PatternNotWithEvery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl =
                    "@name('s0') select a.theString as c0 from pattern [(every a=SupportBean(intPrimitive>=0)) and not SupportBean(intPrimitive<0)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                SendSupportBean(env, "E2", 2);
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(1);

                SendSupportBean(env, "E3", 3);
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                SendSupportBean(env, "E4", -1);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                SendSupportBean(env, "E5", 3);
                SendSupportBean(env, "E6", -1);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }
    }
} // end of namespace