///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorOr
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithrSimple(execs);
            WithrAndNotAndZeroStart(execs);
            WithperatorOrWHarness(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithperatorOrWHarness(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOperatorOrWHarness());
            return execs;
        }

        public static IList<RegressionExecution> WithrAndNotAndZeroStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOrAndNotAndZeroStart());
            return execs;
        }

        public static IList<RegressionExecution> WithrSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOrSimple());
            return execs;
        }

        public class PatternOrSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();

                var epl =
                    "@name('s0') select a.TheString as c0, b.TheString as c1 from pattern [a=SupportBean(IntPrimitive=0) or b=SupportBean(IntPrimitive=1)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "EB", 1);
                env.AssertPropsNew("s0", fields, new object[] { null, "EB" });

                env.Milestone(1);

                SendSupportBean(env, "EA", 0);
                SendSupportBean(env, "EB", 1);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.UndeployAll();
            }
        }

        private class PatternOrAndNotAndZeroStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryOrAndNot(
                    env,
                    "(a=SupportBean_A -> b=SupportBean_B) or (a=SupportBean_A -> not b=SupportBean_B)",
                    milestone);
                TryOrAndNot(env, "a=SupportBean_A -> (b=SupportBean_B or not SupportBean_B)", milestone);

                // try zero-time start
                env.AssertThat(
                    () => {
                        env.AdvanceTime(0);
                        var listener = new SupportUpdateListener();
                        env.CompileDeploy(
                                "@name('s0') select * from pattern [timer:interval(0) or every timer:interval(1 min)]")
                            .Statement("s0")
                            .AddListenerWithReplay(listener);
                        ClassicAssert.IsTrue(listener.IsInvoked);
                        env.UndeployAll();
                    });
            }
        }

        private class PatternOperatorOrWHarness : RegressionExecution
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

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private static void TryOrAndNot(
            RegressionEnvironment env,
            string pattern,
            AtomicLong milestone)
        {
            var expression = "@name('s0') select * " + "from pattern [" + pattern + "]";
            env.CompileDeploy(expression);
            env.AddListener("s0");

            var eventA1 = new SupportBean_A("A1");
            env.SendEventBean(eventA1);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual(eventA1, theEvent.Get("a"));
                    ClassicAssert.IsNull(theEvent.Get("b"));
                });

            env.MilestoneInc(milestone);

            var eventB1 = new SupportBean_B("B1");
            env.SendEventBean(eventB1);
            env.AssertEventNew(
                "s0",
                theEvent => {
                    ClassicAssert.AreEqual(eventA1, theEvent.Get("a"));
                    ClassicAssert.AreEqual(eventB1, theEvent.Get("b"));
                });

            env.UndeployAll();
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }
    }
} // end of namespace