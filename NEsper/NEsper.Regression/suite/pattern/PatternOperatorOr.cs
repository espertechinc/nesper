///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorOr
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new PatternOrSimple());
            execs.Add(new PatternOrAndNotAndZeroStart());
            execs.Add(new PatternOperatorOrWHarness());
            return execs;
        }

        private static void TryOrAndNot(
            RegressionEnvironment env,
            string pattern)
        {
            var expression = "@Name('s0') select * " + "from pattern [" + pattern + "]";
            env.CompileDeploy(expression);
            env.AddListener("s0");

            var eventA1 = new SupportBean_A("A1");
            env.SendEventBean(eventA1);
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.IsNull(theEvent.Get("b"));

            var eventB1 = new SupportBean_B("B1");
            env.SendEventBean(eventB1);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.AreEqual(eventB1, theEvent.Get("b"));

            env.UndeployAll();
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        public class PatternOrSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();

                var epl =
                    "@Name('s0') select a.TheString as c0, b.TheString as c1 from pattern [a=SupportBean(intPrimitive=0) or b=SupportBean(intPrimitive=1)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "EB", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, "EB"});

                env.Milestone(1);

                SendSupportBean(env, "EA", 0);
                SendSupportBean(env, "EB", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.UndeployAll();
            }
        }

        internal class PatternOrAndNotAndZeroStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryOrAndNot(env, "(a=SupportBean_A => b=SupportBean_B) or (a=SupportBean_A => not b=SupportBean_B)");
                TryOrAndNot(env, "a=SupportBean_A => (b=SupportBean_B or not SupportBean_B)");

                // try zero-time start
                env.AdvanceTime(0);
                var listener = new SupportUpdateListener();
                env.CompileDeploy(
                        "@Name('s0') select * from pattern [timer:interval(0) or every timer:interval(1 min)]")
                    .Statement("s0")
                    .AddListenerWithReplay(listener);
                Assert.IsTrue(listener.IsInvoked);
                env.UndeployAll();
            }
        }

        internal class PatternOperatorOrWHarness : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("a=SupportBean_A or a=SupportBean_A");
                testCase.Add("A1", "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A or b=SupportBean_B" + " or c=SupportBean_C");
                testCase.Add("A1", "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B" + " or every d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("D1", "d", events.GetEvent("D1"));
                testCase.Add("D2", "d", events.GetEvent("D2"));
                testCase.Add("B3", "b", events.GetEvent("B3"));
                testCase.Add("D3", "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A or b=SupportBean_B");
                testCase.Add("A1", "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A or every b=SupportBean_B");
                testCase.Add("A1", "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every a=SupportBean_A or d=SupportBean_D");
                testCase.Add("A1", "a", events.GetEvent("A1"));
                testCase.Add("A2", "a", events.GetEvent("A2"));
                testCase.Add("D1", "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (every b=SupportBean_B" + "() or d=SupportBean_D" + "())");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                for (var i = 0; i < 4; i++) {
                    testCase.Add("D1", "d", events.GetEvent("D1"));
                }

                for (var i = 0; i < 4; i++) {
                    testCase.Add("D2", "d", events.GetEvent("D2"));
                }

                for (var i = 0; i < 4; i++) {
                    testCase.Add("B3", "b", events.GetEvent("B3"));
                }

                for (var i = 0; i < 8; i++) {
                    testCase.Add("D3", "d", events.GetEvent("D3"));
                }

                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B" + "() or every d=SupportBean_D" + "())");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("D1", "d", events.GetEvent("D1"));
                testCase.Add("D2", "d", events.GetEvent("D2"));
                testCase.Add("D2", "d", events.GetEvent("D2"));
                for (var i = 0; i < 4; i++) {
                    testCase.Add("B3", "b", events.GetEvent("B3"));
                }

                for (var i = 0; i < 4; i++) {
                    testCase.Add("D3", "d", events.GetEvent("D3"));
                }

                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (every d=SupportBean_D" + "() or every b=SupportBean_B" + "())");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                testCase.Add("B2", "b", events.GetEvent("B2"));
                for (var i = 0; i < 4; i++) {
                    testCase.Add("D1", "d", events.GetEvent("D1"));
                }

                for (var i = 0; i < 8; i++) {
                    testCase.Add("D2", "d", events.GetEvent("D2"));
                }

                for (var i = 0; i < 16; i++) {
                    testCase.Add("B3", "b", events.GetEvent("B3"));
                }

                for (var i = 0; i < 32; i++) {
                    testCase.Add("D3", "d", events.GetEvent("D3"));
                }

                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }
    }
} // end of namespace