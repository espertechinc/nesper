///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorMatchUntil
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithMatchUntilSimple(execs);
            WithOp(execs);
            WithSelectArray(execs);
            WithUseFilter(execs);
            WithRepeatUseTags(execs);
            WithArrayFunctionRepeat(execs);
            WithExpressionBounds(execs);
            WithBoundRepeatWithNot(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundRepeatWithNot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternBoundRepeatWithNot());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionBounds(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternExpressionBounds());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayFunctionRepeat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternArrayFunctionRepeat());
            return execs;
        }

        public static IList<RegressionExecution> WithRepeatUseTags(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternRepeatUseTags());
            return execs;
        }

        public static IList<RegressionExecution> WithUseFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternUseFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternSelectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOp());
            return execs;
        }

        public static IList<RegressionExecution> WithMatchUntilSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternMatchUntilSimple());
            return execs;
        }

        private static void ValidateStmt(
            RegressionEnvironment env,
            string stmtText,
            int numEventsA,
            bool match,
            int? matchCount)
        {
            env.CompileDeploy("@Name('s0') select * from pattern[" + stmtText + "]").AddListener("s0");

            for (var i = 0; i < numEventsA; i++) {
                env.SendEventBean(new SupportBean("A", i));
            }

            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean("B", -1));

            Assert.AreEqual(match, env.Listener("s0").IsInvoked);
            if (!match) {
                env.UndeployAll();
                return;
            }

            var valueATag = env.Listener("s0").AssertOneGetNewAndReset().Get("a");
            if (matchCount == null) {
                Assert.IsNull(valueATag);
            }
            else {
                Assert.That(valueATag, Is.InstanceOf<Array>());
                Assert.AreEqual((int) matchCount, ((Array) valueATag).Length);
            }

            env.UndeployAll();
        }

        private static void TryInvalidPattern(
            RegressionEnvironment env,
            string epl,
            string message)
        {
            TryInvalidCompile(env, "select * from pattern[" + epl + "]", message);
        }

        public class PatternMatchUntilSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};

                var epl =
                    "@Name('s0') select a[0].TheString as c0, a[1].TheString as c1, b.TheString as c2 from pattern [a=SupportBean(IntPrimitive=0) until b=SupportBean(IntPrimitive=1)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "A1", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendSupportBean(env, "A2", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                SendSupportBean(env, "B1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A1", "A2", "B1"});

                env.Milestone(3);

                SendSupportBean(env, "A1", 0);
                SendSupportBean(env, "B1", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }

            private void SendSupportBean(
                RegressionEnvironment env,
                string theString,
                int intPrimitive)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
            }
        }

        internal class PatternOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("a=SupportBean_A(Id='A2') until SupportBean_D");
                testCase.Add("D1", "a[0]", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A until SupportBean_D");
                testCase.Add("D1", "a[0]", events.GetEvent("A1"), "a[1]", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B until a=SupportBean_A");
                testCase.Add("A1", "b[0]", null, "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B until SupportBean_D(Id='D3')");
                testCase.Add(
                    "D3",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(a=SupportBean_A or b=SupportBean_B) until d=SupportBean_D(Id='D3')");
                testCase.Add(
                    "D3",
                    new[] {
                        new[] {"a[0]", events.GetEvent("A1")},
                        new[] {"a[1]", events.GetEvent("A2")},
                        new[] {"b[0]", events.GetEvent("B1")},
                        new[] {"b[1]", events.GetEvent("B2")},
                        new[] {"b[2]", events.GetEvent("B3")},
                        new[] {"d", events.GetEvent("D3")}
                    });
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "(a=SupportBean_A or b=SupportBean_B) until (g=SupportBean_G or d=SupportBean_D)");
                testCase.Add(
                    "D1",
                    new[] {
                        new[] {"a[0]", events.GetEvent("A1")},
                        new[] {"a[1]", events.GetEvent("A2")},
                        new[] {"b[0]", events.GetEvent("B1")},
                        new[] {"b[1]", events.GetEvent("B2")},
                        new[] {"d", events.GetEvent("D1")}
                    });
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(d=SupportBean_D) until a=SupportBean_A(Id='A1')");
                testCase.Add("A1");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("a=SupportBean_A until SupportBean_G(Id='GX')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2] a=SupportBean_A");
                testCase.Add("A2", "a[0]", events.GetEvent("A1"), "a[1]", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2:2] a=SupportBean_A");
                testCase.Add("A2", "a[0]", events.GetEvent("A1"), "a[1]", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1] a=SupportBean_A");
                testCase.Add("A1", "a[0]", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:1] a=SupportBean_A");
                testCase.Add("A1", "a[0]", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[3] a=SupportBean_A");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[3] b=SupportBean_B");
                testCase.Add(
                    "B3",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[4] (a=SupportBean_A or b=SupportBean_B)");
                testCase.Add(
                    "A2",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "a[0]",
                    events.GetEvent("A1"),
                    "a[1]",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                // the until ends the matching returning permanently false
                testCase = new EventExpressionCase("[2] b=SupportBean_B until a=SupportBean_A(Id='A1')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2] b=SupportBean_B until c=SupportBean_C");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2:2] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add("B2", "b[0]", events.GetEvent("B1"), "b[1]", events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[:4] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add(
                    "G1",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"),
                    "g",
                    events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[:3] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add(
                    "G1",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"),
                    "g",
                    events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[:2] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add(
                    "G1",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "g",
                    events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[:1] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add("G1", "b[0]", events.GetEvent("B1"), "g", events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[:1] b=SupportBean_B until a=SupportBean_A(Id='A1')");
                testCase.Add("A1", "b[0]", null, "a", events.GetEvent("A1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:] b=SupportBean_B until g=SupportBean_G(Id='G1')");
                testCase.Add(
                    "G1",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"),
                    "g",
                    events.GetEvent("G1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:] b=SupportBean_B until a=SupportBean_A");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2:] b=SupportBean_B until a=SupportBean_A(Id='A2')");
                testCase.Add(
                    "A2",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "a",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2:] b=SupportBean_B until c=SupportBean_C");
                testCaseList.AddTest(testCase);

                // same event triggering both clauses, until always wins, match does not count
                testCase = new EventExpressionCase("[2:] b=SupportBean_B until e=SupportBean_B(Id='B2')");
                testCaseList.AddTest(testCase);

                // same event triggering both clauses, until always wins, match does not count
                testCase = new EventExpressionCase("[1:] b=SupportBean_B until e=SupportBean_B(Id='B1')");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:2] b=SupportBean_B until a=SupportBean_A(Id='A2')");
                testCase.Add(
                    "A2",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    null,
                    "a",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:3] b=SupportBean_B until SupportBean_G");
                testCase.Add(
                    "G1",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"),
                    "b[3]",
                    null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:2] b=SupportBean_B until SupportBean_G");
                testCase.Add("G1", "b[0]", events.GetEvent("B1"), "b[1]", events.GetEvent("B2"), "b[2]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:10] b=SupportBean_B until SupportBean_F");
                testCase.Add("F1", "b[0]", events.GetEvent("B1"), "b[1]", events.GetEvent("B2"), "b[2]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[1:10] b=SupportBean_B until SupportBean_C");
                testCase.Add("C1", "b[0]", events.GetEvent("B1"), "b[1]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[0:1] b=SupportBean_B until SupportBean_C");
                testCase.Add("C1", "b[0]", events.GetEvent("B1"), "b[1]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("c=SupportBean_C -> [2] b=SupportBean_B -> d=SupportBean_D");
                testCase.Add(
                    "D3",
                    "c",
                    events.GetEvent("C1"),
                    "b[0]",
                    events.GetEvent("B2"),
                    "b[1]",
                    events.GetEvent("B3"),
                    "d",
                    events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[3] d=SupportBean_D or [3] b=SupportBean_B");
                testCase.Add(
                    "B3",
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"),
                    "b[2]",
                    events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[3] d=SupportBean_D or [4] b=SupportBean_B");
                testCase.Add(
                    "D3",
                    "d[0]",
                    events.GetEvent("D1"),
                    "d[1]",
                    events.GetEvent("D2"),
                    "d[2]",
                    events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2] d=SupportBean_D and [2] b=SupportBean_B");
                testCase.Add(
                    "D2",
                    "d[0]",
                    events.GetEvent("D1"),
                    "d[1]",
                    events.GetEvent("D2"),
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("d=SupportBean_D until timer:interval(7 sec)");
                testCase.Add("E1", "d[0]", events.GetEvent("D1"), "d[1]", null, "d[2]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (d=SupportBean_D until b=SupportBean_B)");
                testCase.Add("B1", "d[0]", null, "b", events.GetEvent("B1"));
                testCase.Add("B2", "d[0]", null, "b", events.GetEvent("B2"));
                testCase.Add(
                    "B3",
                    "d[0]",
                    events.GetEvent("D1"),
                    "d[1]",
                    events.GetEvent("D2"),
                    "d[2]",
                    null,
                    "b",
                    events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                // note precendence: every is higher then until
                testCase = new EventExpressionCase("every d=SupportBean_D until b=SupportBean_B");
                testCase.Add("B1", "d[0]", null, "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(every d=SupportBean_D) until b=SupportBean_B");
                testCase.Add("B1", "d[0]", null, "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "a=SupportBean_A until (every (timer:interval(6 sec) and not SupportBean_A))");
                testCase.Add("G1", "a[0]", events.GetEvent("A1"), "a[1]", events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "SupportBean_A until (every (timer:interval(7 sec) and not SupportBean_A))");
                testCase.Add("D3");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[2] (a=SupportBean_A or b=SupportBean_B)");
                testCase.Add("B1", "a[0]", events.GetEvent("A1"), "b[0]", events.GetEvent("B1"), "b[1]", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every [2] a=SupportBean_A");
                testCase.Add(
                    "A2",
                    new[] {
                        new[] {"a[0]", events.GetEvent("A1")},
                        new[] {"a[1]", events.GetEvent("A2")}
                    });
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every [2] a=SupportBean_A until d=SupportBean_D"); // every has precedence; ESPER-339
                testCase.Add(
                    "D1",
                    new[] {
                        new[] {"a[0]", events.GetEvent("A1")},
                        new[] {"a[1]", events.GetEvent("A2")},
                        new[] {"d", events.GetEvent("D1")}
                    });
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("[3] (a=SupportBean_A or b=SupportBean_B)");
                testCase.Add(
                    "B2",
                    "a[0]",
                    events.GetEvent("A1"),
                    "b[0]",
                    events.GetEvent("B1"),
                    "b[1]",
                    events.GetEvent("B2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(a=SupportBean_A until b=SupportBean_B) until c=SupportBean_C");
                testCase.Add(
                    "C1",
                    "a[0]",
                    events.GetEvent("A1"),
                    "b[0]",
                    events.GetEvent("B1"),
                    "c",
                    events.GetEvent("C1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("(a=SupportBean_A until b=SupportBean_B) until g=SupportBean_G");
                testCase.Add(
                    "G1",
                    new[] {
                        new[] {"a[0]", events.GetEvent("A1")}, new[] {"b[0]", events.GetEvent("B1")},
                        new[] {"a[1]", events.GetEvent("A2")}, new[] {"b[1]", events.GetEvent("B2")},
                        new[] {"b[2]", events.GetEvent("B3")},
                        new[] {"g", events.GetEvent("G1")}
                    });
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("SupportBean_B until not SupportBean_B");
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        internal class PatternSelectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt =
                    "@Name('s0') select a, b, a[0] as a0, a[0].Id as a0Id, a[1] as a1, a[1].Id as a1Id, a[2] as a2, a[2].Id as a2Id from pattern [a=SupportBean_A until b=SupportBean_B]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.Milestone(0);

                var eventA1 = new SupportBean_A("A1");
                env.SendEventBean(eventA1);

                env.Milestone(1);

                var eventA2 = new SupportBean_A("A2");
                env.SendEventBean(eventA2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                var eventB1 = new SupportBean_B("B1");
                env.SendEventBean(eventB1);

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("a"),
                    new object[] {eventA1, eventA2});
                Assert.AreEqual(eventA1, theEvent.Get("a0"));
                Assert.AreEqual(eventA2, theEvent.Get("a1"));
                Assert.IsNull(theEvent.Get("a2"));
                Assert.AreEqual("A1", theEvent.Get("a0Id"));
                Assert.AreEqual("A2", theEvent.Get("a1Id"));
                Assert.IsNull(theEvent.Get("a2Id"));
                Assert.AreEqual(eventB1, theEvent.Get("b"));

                env.UndeployModuleContaining("s0");

                // try wildcard
                stmt = "@Name('s0') select * from pattern [a=SupportBean_A until b=SupportBean_B]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(eventA1);
                env.SendEventBean(eventA2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(eventB1);

                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("a"),
                    new object[] {eventA1, eventA2});
                Assert.AreSame(eventA1, theEvent.Get("a[0]"));
                Assert.AreSame(eventA2, theEvent.Get("a[1]"));
                Assert.IsNull(theEvent.Get("a[2]"));
                Assert.AreEqual("A1", theEvent.Get("a[0].Id"));
                Assert.AreEqual("A2", theEvent.Get("a[1].Id"));
                Assert.IsNull(theEvent.Get("a[2].Id"));
                Assert.AreSame(eventB1, theEvent.Get("b"));

                env.UndeployAll();
            }
        }

        internal class PatternUseFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string stmt;
                EventBean theEvent;

                stmt =
                    "@Name('s0') select * from pattern [a=SupportBean_A until b=SupportBean_B -> c=SupportBean_C(Id = ('C' || a[0].Id || a[1].Id || b.Id))]";
                env.CompileDeploy(stmt).AddListener("s0");

                object eventA1 = new SupportBean_A("A1");
                env.SendEventBean(eventA1);

                env.Milestone(0);

                object eventA2 = new SupportBean_A("A2");
                env.SendEventBean(eventA2);

                env.Milestone(1);

                object eventB1 = new SupportBean_B("B1");
                env.SendEventBean(eventB1);

                env.Milestone(2);

                env.SendEventBean(new SupportBean_C("C1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                object eventC1 = new SupportBean_C("CA1A2B1");
                env.SendEventBean(eventC1);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(eventA1, theEvent.Get("a[0]"));
                Assert.AreEqual(eventA2, theEvent.Get("a[1]"));
                Assert.IsNull(theEvent.Get("a[2]"));
                Assert.AreEqual(eventB1, theEvent.Get("b"));
                Assert.AreEqual(eventC1, theEvent.Get("c"));
                env.UndeployAll();

                // Test equals-optimization with array event
                stmt =
                    "@Name('s0') select * from pattern [a=SupportBean_A until b=SupportBean_B -> c=SupportBean(TheString = a[1].Id)]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_B("B1"));

                env.SendEventBean(new SupportBean("A3", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A2", 10));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10, theEvent.Get("c.IntPrimitive"));
                env.UndeployAll();

                // Test in-optimization
                stmt =
                    "@Name('s0') select * from pattern [a=SupportBean_A until b=SupportBean_B -> c=SupportBean(TheString in(a[2].Id))]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_B("B1"));

                env.SendEventBean(new SupportBean("A2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A3", 5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(5, theEvent.Get("c.IntPrimitive"));
                env.UndeployAll();

                // Test not-in-optimization
                stmt =
                    "@Name('s0') select * from pattern [a=SupportBean_A until b=SupportBean_B -> c=SupportBean(TheString!=a[0].Id and TheString!=a[1].Id and TheString!=a[2].Id)]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_B("B1"));

                env.SendEventBean(new SupportBean("A2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("A1", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A6", 5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(5, theEvent.Get("c.IntPrimitive"));
                env.UndeployAll();

                // Test range-optimization
                stmt = "@Name('s0') select * from pattern [" +
                       " a=SupportBean(TheString like 'A%') until " +
                       " b=SupportBean(TheString like 'B%') -> " +
                       " c=SupportBean(IntPrimitive between a[0].IntPrimitive and a[1].IntPrimitive)" +
                       "]";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 5));
                env.SendEventBean(new SupportBean("A2", 8));
                env.SendEventBean(new SupportBean("B1", -1));

                env.SendEventBean(new SupportBean("E1", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("E2", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(5, theEvent.Get("c.IntPrimitive"));

                env.UndeployAll();
            }
        }

        internal class PatternRepeatUseTags : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt =
                    "@Name('s0') select * from pattern [every [2] (a=SupportBean_A() -> b=SupportBean_B(Id=a.Id))]";

                env.CompileDeploy(stmt);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_B("A2"));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                // test with timer:interval
                env.AdvanceTime(0);
                var query =
                    "@Name('s0') select * from pattern [every ([2:]e1=SupportBean(TheString='2') until timer:interval(5))->([2:]e2=SupportBean(TheString='3') until timer:interval(2))]";
                env.CompileDeploy(query).AddListener("s0");

                env.SendEventBean(new SupportBean("2", 0));
                env.SendEventBean(new SupportBean("2", 0));
                env.AdvanceTime(5000);

                env.SendEventBean(new SupportBean("3", 0));
                env.SendEventBean(new SupportBean("3", 0));
                env.SendEventBean(new SupportBean("3", 0));
                env.SendEventBean(new SupportBean("3", 0));
                env.AdvanceTime(10000);

                env.SendEventBean(new SupportBean("2", 0));
                env.SendEventBean(new SupportBean("2", 0));
                env.AdvanceTime(15000);

                // test followed by 3 streams
                env.UndeployAll();

                var epl = "@Name('s0') select * from pattern [ every [2] A=SupportBean(TheString='1') " +
                          "-> [2] B=SupportBean(TheString='2' and IntPrimitive=A[0].IntPrimitive)" +
                          "-> [2] C=SupportBean(TheString='3' and IntPrimitive=A[0].IntPrimitive)]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("1", 10));
                env.SendEventBean(new SupportBean("1", 20));
                env.SendEventBean(new SupportBean("2", 10));
                env.SendEventBean(new SupportBean("2", 10));
                env.SendEventBean(new SupportBean("3", 10));
                env.SendEventBean(new SupportBean("3", 10));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternArrayFunctionRepeat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt =
                    $"@Name('s0') select " +
                    $" SupportStaticMethodLib.ArrayLength(a) as length, " +
                    $" {typeof(Arrays).FullName}.GetLength(a) as l2 from pattern [[1:] a=SupportBean_A until SupportBean_B]";

                env.CompileDeploy(stmt);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_A("A2"));
                env.SendEventBean(new SupportBean_A("A3"));
                env.SendEventBean(new SupportBean_B("A2"));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(3, theEvent.Get("length"));
                Assert.AreEqual(3, theEvent.Get("l2"));

                env.UndeployAll();
            }
        }

        internal class PatternExpressionBounds : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test variables - closed bounds
                env.Runtime.VariableService.SetVariableValue(null, "lower", 2);
                env.Runtime.VariableService.SetVariableValue(null, "upper", 3);
                var stmtOne = "[lower:upper] a=SupportBean (TheString = 'A') until b=SupportBean (TheString = 'B')";
                ValidateStmt(env, stmtOne, 0, false, null);
                ValidateStmt(env, stmtOne, 1, false, null);
                ValidateStmt(env, stmtOne, 2, true, 2);
                ValidateStmt(env, stmtOne, 3, true, 3);
                ValidateStmt(env, stmtOne, 4, true, 3);
                ValidateStmt(env, stmtOne, 5, true, 3);

                // test variables - half open
                env.Runtime.VariableService.SetVariableValue(null, "lower", 3);
                env.Runtime.VariableService.SetVariableValue(null, "upper", null);
                var stmtTwo = "[lower:] a=SupportBean (TheString = 'A') until b=SupportBean (TheString = 'B')";
                ValidateStmt(env, stmtTwo, 0, false, null);
                ValidateStmt(env, stmtTwo, 1, false, null);
                ValidateStmt(env, stmtTwo, 2, false, null);
                ValidateStmt(env, stmtTwo, 3, true, 3);
                ValidateStmt(env, stmtTwo, 4, true, 4);
                ValidateStmt(env, stmtTwo, 5, true, 5);

                // test variables - half closed
                env.Runtime.VariableService.SetVariableValue(null, "lower", null);
                env.Runtime.VariableService.SetVariableValue(null, "upper", 2);
                var stmtThree = "[:upper] a=SupportBean (TheString = 'A') until b=SupportBean (TheString = 'B')";
                ValidateStmt(env, stmtThree, 0, true, null);
                ValidateStmt(env, stmtThree, 1, true, 1);
                ValidateStmt(env, stmtThree, 2, true, 2);
                ValidateStmt(env, stmtThree, 3, true, 2);
                ValidateStmt(env, stmtThree, 4, true, 2);
                ValidateStmt(env, stmtThree, 5, true, 2);

                // test followed-by - bounded
                env.CompileDeploy("@Name('s0') select * from pattern [S0=SupportBean_S0 -> [S0.Id] b=SupportBean]")
                    .AddListener("s0");
                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"b[0].TheString", "b[1].TheString"},
                    new object[] {"E1", "E2"});

                env.UndeployAll();

                // test substitution parameter
                var epl = "select * from pattern[[?::int] SupportBean]";
                var compiled = env.Compile(epl);
                env.Deploy(
                    compiled,
                    new DeploymentOptions().WithStatementSubstitutionParameter(prepared => prepared.SetObject(1, 2)));
                env.UndeployAll();

                // test exactly-1
                env.AdvanceTime(0);
                var eplExact1 =
                    "@Name('s0') select * from pattern [a=SupportBean_A -> [1] every (timer:interval(10) and not SupportBean_B)]";
                env.CompileDeploy(eplExact1).AddListener("s0");

                env.AdvanceTime(5000);
                env.SendEventBean(new SupportBean_A("A1"));

                env.AdvanceTime(6000);
                env.SendEventBean(new SupportBean_B("B1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(15999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(16000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"a.Id"},
                    new object[] {"A1"});

                env.AdvanceTime(999999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployAll();

                // test until
                env.AdvanceTime(1000000);
                var eplUntilOne =
                    "@Name('s0') select * from pattern [a=SupportBean_A -> b=SupportBean_B until ([1] every (timer:interval(10) and not SupportBean_C))]";
                env.CompileDeploy(eplUntilOne).AddListener("s0");

                env.AdvanceTime(1005000);
                env.SendEventBean(new SupportBean_A("A1"));

                env.AdvanceTime(1006000);
                env.SendEventBean(new SupportBean_B("B1"));
                env.AdvanceTime(1014999);
                env.SendEventBean(new SupportBean_B("B2"));
                env.SendEventBean(new SupportBean_C("C1"));
                env.AdvanceTime(1015000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(1024998);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(1024999);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"a.Id", "b[0].Id", "b[1].Id"},
                    new object[] {"A1", "B1", "B2"});

                env.AdvanceTime(1999999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternBoundRepeatWithNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"e[0].IntPrimitive", "e[1].IntPrimitive"};
                var epl =
                    "@Name('s0') select * from pattern [every [2] (e = SupportBean(TheString='A') and not SupportBean(TheString='B'))]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 2});

                env.SendEventBean(new SupportBean("A", 3));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 4));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("A", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, 6});

                env.UndeployAll();
            }
        }

        internal class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidPattern(
                    env,
                    "[:0] SupportBean_A until SupportBean_B",
                    "Incorrect range specification, a bounds value of zero or negative value is not allowed ");
                TryInvalidPattern(
                    env,
                    "[10:4] SupportBean_A",
                    "Incorrect range specification, lower bounds value '10' is higher then higher bounds '4'");
                TryInvalidPattern(
                    env,
                    "[-1] SupportBean_A",
                    "Incorrect range specification, a bounds value of zero or negative value is not allowed");
                TryInvalidPattern(
                    env,
                    "[4:6] SupportBean_A",
                    "Variable bounds repeat operator requires an until-expression");
                TryInvalidPattern(
                    env,
                    "[0:0] SupportBean_A",
                    "Incorrect range specification, a bounds value of zero or negative value is not allowed");
                TryInvalidPattern(
                    env,
                    "[0] SupportBean_A",
                    "Incorrect range specification, a bounds value of zero or negative value is not allowed");
                TryInvalidPattern(
                    env,
                    "[1] a=SupportBean_A(a[0].Id='a')",
                    "Failed to validate filter expression 'a[0].Id=\"a\"': Property named 'a[0].Id' is not valid in any stream");
                TryInvalidPattern(
                    env,
                    "a=SupportBean_A -> SupportBean_B(a[0].Id='a')",
                    "Failed to validate filter expression 'a[0].Id=\"a\"': Property named 'a[0].Id' is not valid in any stream");
                TryInvalidPattern(
                    env,
                    "(a=SupportBean_A until c=SupportBean_B) -> c=SupportBean_C",
                    "Tag 'c' for event 'SupportBean_C' has already been declared for events of type " +
                    typeof(SupportBean_B).Name);
                TryInvalidPattern(
                    env,
                    "((a=SupportBean_A until b=SupportBean_B) until a=SupportBean_A)",
                    "Tag 'a' for event 'SupportBean_A' used in the repeat-until operator cannot also appear in other filter expressions");
                TryInvalidPattern(
                    env,
                    "a=SupportBean -> [a.TheString] b=SupportBean",
                    "Match-until bounds value expressions must return a numeric value");
                TryInvalidPattern(
                    env,
                    "a=SupportBean -> [:a.TheString] b=SupportBean",
                    "Match-until bounds value expressions must return a numeric value");
                TryInvalidPattern(
                    env,
                    "a=SupportBean -> [a.TheString:1] b=SupportBean",
                    "Match-until bounds value expressions must return a numeric value");
            }
        }
    }
} // end of namespace