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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternUseResult
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new PatternNumeric());
            execs.Add(new PatternObjectId());
            execs.Add(new PatternFollowedByFilter());
            execs.Add(new PatternPatternTypeCacheForRepeat());
            execs.Add(new PatternBooleanExprRemoveConsiderTag());
            execs.Add(new PatternBooleanExprRemoveConsiderArrayTag());
            return execs;
        }

        private static void SendBeanAAssert(
            RegressionEnvironment env,
            string id,
            int intPrimitiveExpected,
            int numFiltersRemaining)
        {
            env.SendEventBean(new SupportBean_A(id));
            var fields = new [] { "c0" };
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {intPrimitiveExpected});
            Assert.AreEqual(
                numFiltersRemaining,
                SupportFilterHelper.GetFilterCount(env.Statement("s0"), "SupportBean_A"));
        }

        private static void SendBeanAMiss(
            RegressionEnvironment env,
            string idCSV)
        {
            foreach (var id in idCSV.SplitCsv()) {
                env.SendEventBean(new SupportBean_A(id));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
            }
        }

        private class PatternBooleanExprRemoveConsiderArrayTag : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select sb[1].IntPrimitive as c0 from pattern[every [2] sb=SupportBean -> SupportBean_A(Id like sb[1].TheString)]";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 6; i++) {
                    env.SendEventBean(new SupportBean("X" + i, i));
                    env.SendEventBean(new SupportBean("Y" + i, i));
                }

                env.Milestone(0);

                SendBeanAAssert(env, "Y2", 2, 5);
                SendBeanAMiss(env, "Y2");

                env.Milestone(1);

                SendBeanAAssert(env, "Y1", 1, 4);
                SendBeanAMiss(env, "Y1,Y2");

                env.Milestone(2);

                SendBeanAAssert(env, "Y4", 4, 3);
                SendBeanAMiss(env, "Y1,Y2,Y4");

                env.Milestone(3);

                SendBeanAAssert(env, "Y0", 0, 2);
                SendBeanAMiss(env, "Y0,Y1,Y2,Y4");

                env.Milestone(4);

                SendBeanAAssert(env, "Y5", 5, 1);
                SendBeanAMiss(env, "Y0,Y1,Y2,Y4,Y5");

                env.Milestone(5);

                SendBeanAAssert(env, "Y3", 3, 0);
                SendBeanAMiss(env, "Y0,Y1,Y2,Y3,Y4,Y5");

                env.UndeployAll();
            }
        }

        internal class PatternBooleanExprRemoveConsiderTag : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select sb.IntPrimitive as c0 from pattern[every sb=SupportBean -> SupportBean_A(Id like sb.TheString)]";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean("E" + i, i));
                }

                env.Milestone(0);

                SendBeanAAssert(env, "E5", 5, 9);
                SendBeanAMiss(env, "E5");

                env.Milestone(1);

                SendBeanAAssert(env, "E3", 3, 8);
                SendBeanAMiss(env, "E5,E3");

                env.Milestone(2);

                SendBeanAAssert(env, "E1", 1, 7);
                SendBeanAAssert(env, "E8", 8, 6);

                env.Milestone(3);

                SendBeanAAssert(env, "E4", 4, 5);
                SendBeanAMiss(env, "E1,E3,E4,E5,E8");

                SendBeanAAssert(env, "E2", 2, 4);
                SendBeanAAssert(env, "E9", 9, 3);
                SendBeanAAssert(env, "E7", 7, 2);
                SendBeanAMiss(env, "E1,E2,E3,E4,E5,E7,E8,E9");

                SendBeanAAssert(env, "E0", 0, 1);

                env.Milestone(4);

                SendBeanAAssert(env, "E6", 6, 0);

                env.Milestone(5);

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_A("E" + i));
                }

                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternPatternTypeCacheForRepeat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // UEJ-229-28464 bug fix for type reuse for dissimilar types
                var epl = "create objectarray schema TypeOne(Symbol string, Price double);\n" +
                          "create objectarray schema TypeTwo(Symbol string, market string, Price double);\n" +
                          "\n" +
                          "@Name('Out2') select a[0].Symbol from pattern [ [2] a=TypeOne ]\n;" +
                          "@Name('Out3') select a[0].market from pattern [ [2] a=TypeTwo ];";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());

                env.AddListener("Out2");
                env.AddListener("Out3");

                env.SendEventObjectArray(new object[] {"GE", 10}, "TypeOne");
                env.SendEventObjectArray(new object[] {"GE", 10}, "TypeOne");
                Assert.IsTrue(env.Listener("Out2").IsInvokedAndReset());

                env.SendEventObjectArray(new object[] {"GE", "m1", 5}, "TypeTwo");
                env.SendEventObjectArray(new object[] {"GE", "m2", 5}, "TypeTwo");
                Assert.IsTrue(env.Listener("Out3").IsInvokedAndReset());

                env.UndeployAll();
            }
        }

        private class PatternNumeric : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetSetThreeExternalClock(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase(
                    "na=SupportBean_N -> nb=SupportBean_N(DoublePrimitive = na.DoublePrimitive)");
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=87) -> nb=SupportBean_N(IntPrimitive > na.IntPrimitive)");
                testCase.Add("N8", "na", events.GetEvent("N3"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=87) -> nb=SupportBean_N(IntPrimitive < na.IntPrimitive)");
                testCase.Add("N4", "na", events.GetEvent("N3"), "nb", events.GetEvent("N4"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=66) -> every nb=SupportBean_N(IntPrimitive >= na.IntPrimitive)");
                testCase.Add("N3", "na", events.GetEvent("N2"), "nb", events.GetEvent("N3"));
                testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(BoolBoxed=false) -> every nb=SupportBean_N(BoolPrimitive = na.BoolPrimitive)");
                testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N -> every nb=SupportBean_N(IntPrimitive=na.IntPrimitive)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N() -> every nb=SupportBean_N(DoublePrimitive=na.DoublePrimitive)");
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N(BoolBoxed=false) -> every nb=SupportBean_N(BoolBoxed=na.BoolBoxed)");
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCase.Add("N8", "na", events.GetEvent("N5"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(BoolBoxed=false) -> nb=SupportBean_N(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=SupportBean_N(IntPrimitive > nb.IntPrimitive)");
                testCase.Add(
                    "N6",
                    "na",
                    events.GetEvent("N2"),
                    "nb",
                    events.GetEvent("N5"),
                    "nc",
                    events.GetEvent("N6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=86) -> nb=SupportBean_N(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=SupportBean_N(IntPrimitive > na.IntPrimitive)");
                testCase.Add(
                    "N8",
                    "na",
                    events.GetEvent("N4"),
                    "nb",
                    events.GetEvent("N5"),
                    "nc",
                    events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=86) -> (nb=SupportBean_N(IntPrimitive<na.IntPrimitive)" +
                    " or nc=SupportBean_N(IntPrimitive > na.IntPrimitive))");
                testCase.Add("N5", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=86) -> (nb=SupportBean_N(IntPrimitive>na.IntPrimitive)" +
                    " or nc=SupportBean_N(IntBoxed < na.IntBoxed))");
                testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N8"), "nc", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(IntPrimitive=86) -> (nb=SupportBean_N(IntPrimitive>na.IntPrimitive)" +
                    " and nc=SupportBean_N(IntBoxed < na.IntBoxed))");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(DoublePrimitive in [0:na.DoublePrimitive])");
                testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(DoublePrimitive in (0:na.DoublePrimitive))");
                testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(IntPrimitive in (na.IntPrimitive:na.DoublePrimitive))");
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(IntPrimitive in (na.IntPrimitive:60))");
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        private class PatternObjectId : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetSetFourExternalClock(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("X1=SupportBean_S0() -> X2=SupportBean_S0(P00=X1.P00)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("X1=SupportBean_S0(P00='B') -> X2=SupportBean_S0(P00=X1.P00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("X1=SupportBean_S0(P00='B') -> every X2=SupportBean_S0(P00=X1.P00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every X1=SupportBean_S0(P00='B') -> every X2=SupportBean_S0(P00=X1.P00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every X1=SupportBean_S0() -> X2=SupportBean_S0(P00=X1.P00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
                testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every X1=SupportBean_S0() -> every X2=SupportBean_S0(P00=X1.P00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
                testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        private class PatternFollowedByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern [" +
                                 "every tradeevent1=SupportTradeEvent(UserId in ('U1000','U1001','U1002') ) -> " +
                                 "(tradeevent2=SupportTradeEvent(UserId in ('U1000','U1001','U1002') and " +
                                 "  UserId != tradeevent1.UserId and " +
                                 "  Ccypair = tradeevent1.Ccypair and " +
                                 "  Direction = tradeevent1.Direction) -> " +
                                 "  tradeevent3=SupportTradeEvent(UserId in ('U1000','U1001','U1002') and " +
                                 "  UserId != tradeevent1.UserId and " +
                                 "  UserId != tradeevent2.UserId and " +
                                 "  Ccypair = tradeevent1.Ccypair and " +
                                 "  Direction = tradeevent1.Direction)" +
                                 ") where timer:within(600 sec)]";

                env.CompileDeploy(expression);
                var listener = new MyUpdateListener();
                env.Statement("s0").AddListener(listener);

                var random = new Random();
                string[] users = {"U1000", "U1001", "U1002"};
                string[] ccy = {"USD", "JPY", "EUR"};
                string[] direction = {"B", "S"};

                for (var i = 0; i < 100; i++) {
                    var theEvent = new
                        SupportTradeEvent(
                            i,
                            users[random.Next(users.Length)],
                            ccy[random.Next(ccy.Length)],
                            direction[random.Next(direction.Length)]);
                    env.SendEventBean(theEvent);
                }

                Assert.AreEqual(0, listener.BadMatchCount);
                env.UndeployAll();
            }
        }

        private class MyUpdateListener : UpdateListener
        {
            internal int BadMatchCount;

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                if (eventArgs.NewEvents != null) {
                    foreach (var eventBean in eventArgs.NewEvents) {
                        HandleEvent(eventBean);
                    }
                }
            }

            private void HandleEvent(EventBean eventBean)
            {
                var tradeevent1 = (SupportTradeEvent)
                    eventBean.Get("tradeevent1");
                var tradeevent2 = (SupportTradeEvent)
                    eventBean.Get("tradeevent2");
                var tradeevent3 = (SupportTradeEvent)
                    eventBean.Get("tradeevent3");

                if (tradeevent1.UserId.Equals(tradeevent2.UserId) ||
                    tradeevent1.UserId.Equals(tradeevent3.UserId) ||
                    tradeevent2.UserId.Equals(tradeevent3.UserId)) {
                    /*
                    System.out.println("Bad Match : ");
                    System.out.println(tradeevent1);
                    System.out.println(tradeevent2);
                    System.out.println(tradeevent3 + "\n");
                    */
                    BadMatchCount++;
                }
            }
        }
    }
} // end of namespace