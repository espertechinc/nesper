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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using static com.espertech.esper.regressionlib.framework.RegressionFlag; // OBSERVEROPS
using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A; // assertEquals

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternUseResult
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNumeric(execs);
            WithObjectId(execs);
            WithFollowedByFilter(execs);
            WithPatternTypeCacheForRepeat(execs);
            WithBooleanExprRemoveConsiderTag(execs);
            WithBooleanExprRemoveConsiderArrayTag(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExprRemoveConsiderArrayTag(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternBooleanExprRemoveConsiderArrayTag());
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExprRemoveConsiderTag(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternBooleanExprRemoveConsiderTag());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternTypeCacheForRepeat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternPatternTypeCacheForRepeat());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternFollowedByFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectId(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternObjectId());
            return execs;
        }

        public static IList<RegressionExecution> WithNumeric(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternNumeric());
            return execs;
        }

        private class PatternBooleanExprRemoveConsiderArrayTag : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sb[1].intPrimitive as c0 from pattern[every [2] sb=SupportBean -> SupportBean_A(id like sb[1].theString)]";
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

        private class PatternBooleanExprRemoveConsiderTag : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sb.intPrimitive as c0 from pattern[every sb=SupportBean -> SupportBean_A(id like sb.theString)]";
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

                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternPatternTypeCacheForRepeat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // UEJ-229-28464 bug fix for type reuse for dissimilar types
                var epl = "@public @buseventtype create objectarray schema TypeOne(symbol string, price double);\n" +
                          "@public @buseventtype create objectarray schema TypeTwo(symbol string, market string, price double);\n" +
                          "\n" +
                          "@name('Out2') select a[0].symbol from pattern [ [2] a=TypeOne ]\n;" +
                          "@name('Out3') select a[0].market from pattern [ [2] a=TypeTwo ];";
                env.CompileDeploy(epl, new RegressionPath());

                env.AddListener("Out2");
                env.AddListener("Out3");

                env.SendEventObjectArray(new object[] { "GE", 10 }, "TypeOne");
                env.SendEventObjectArray(new object[] { "GE", 10 }, "TypeOne");
                env.AssertListenerInvoked("Out2");

                env.Milestone(0);

                env.SendEventObjectArray(new object[] { "GE", "m1", 5 }, "TypeTwo");
                env.SendEventObjectArray(new object[] { "GE", "m2", 5 }, "TypeTwo");
                env.AssertListenerInvoked("Out3");

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
                    "na=SupportBean_N -> nb=SupportBean_N(doublePrimitive = na.doublePrimitive)");
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(intPrimitive=87) -> nb=SupportBean_N(intPrimitive > na.intPrimitive)");
                testCase.Add("N8", "na", events.GetEvent("N3"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(intPrimitive=87) -> nb=SupportBean_N(intPrimitive < na.intPrimitive)");
                testCase.Add("N4", "na", events.GetEvent("N3"), "nb", events.GetEvent("N4"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(intPrimitive=66) -> every nb=SupportBean_N(intPrimitive >= na.intPrimitive)");
                testCase.Add("N3", "na", events.GetEvent("N2"), "nb", events.GetEvent("N3"));
                testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(boolBoxed=false) -> every nb=SupportBean_N(boolPrimitive = na.boolPrimitive)");
                testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N -> every nb=SupportBean_N(intPrimitive=na.intPrimitive)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N() -> every nb=SupportBean_N(doublePrimitive=na.doublePrimitive)");
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every na=SupportBean_N(boolBoxed=false) -> every nb=SupportBean_N(boolBoxed=na.boolBoxed)");
                testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
                testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
                testCase.Add("N8", "na", events.GetEvent("N5"), "nb", events.GetEvent("N8"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(boolBoxed=false) -> nb=SupportBean_N(intPrimitive<na.intPrimitive)" +
                    " -> nc=SupportBean_N(intPrimitive > nb.intPrimitive)");
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
                    "na=SupportBean_N(intPrimitive=86) -> nb=SupportBean_N(intPrimitive<na.intPrimitive)" +
                    " -> nc=SupportBean_N(intPrimitive > na.intPrimitive)");
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
                    "na=SupportBean_N(intPrimitive=86) -> (nb=SupportBean_N(intPrimitive<na.intPrimitive)" +
                    " or nc=SupportBean_N(intPrimitive > na.intPrimitive))");
                testCase.Add("N5", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(intPrimitive=86) -> (nb=SupportBean_N(intPrimitive>na.intPrimitive)" +
                    " or nc=SupportBean_N(intBoxed < na.intBoxed))");
                testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N8"), "nc", null);
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N(intPrimitive=86) -> (nb=SupportBean_N(intPrimitive>na.intPrimitive)" +
                    " and nc=SupportBean_N(intBoxed < na.intBoxed))");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(doublePrimitive in [0:na.doublePrimitive])");
                testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(doublePrimitive in (0:na.doublePrimitive))");
                testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(intPrimitive in (na.intPrimitive:na.doublePrimitive))");
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "na=SupportBean_N() -> every nb=SupportBean_N(intPrimitive in (na.intPrimitive:60))");
                testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
                testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
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

                testCase = new EventExpressionCase("X1=SupportBean_S0() -> X2=SupportBean_S0(p00=X1.p00)");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("X1=SupportBean_S0(p00='B') -> X2=SupportBean_S0(p00=X1.p00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("X1=SupportBean_S0(p00='B') -> every X2=SupportBean_S0(p00=X1.p00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every X1=SupportBean_S0(p00='B') -> every X2=SupportBean_S0(p00=X1.p00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every X1=SupportBean_S0() -> X2=SupportBean_S0(p00=X1.p00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
                testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every X1=SupportBean_S0() -> every X2=SupportBean_S0(p00=X1.p00)");
                testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
                testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
                testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
                testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
                testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
                testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList);
                util.RunTest(env);
            }
        }

        private class PatternFollowedByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select * from pattern [" +
                                 "every tradeevent1=SupportTradeEvent(userId in ('U1000','U1001','U1002') ) -> " +
                                 "(tradeevent2=SupportTradeEvent(userId in ('U1000','U1001','U1002') and " +
                                 "  userId != tradeevent1.userId and " +
                                 "  ccypair = tradeevent1.ccypair and " +
                                 "  direction = tradeevent1.direction) -> " +
                                 " tradeevent3=SupportTradeEvent(userId in ('U1000','U1001','U1002') and " +
                                 "  userId != tradeevent1.userId and " +
                                 "  userId != tradeevent2.userId and " +
                                 "  ccypair = tradeevent1.ccypair and " +
                                 "  direction = tradeevent1.direction)" +
                                 ") where timer:within(600 sec)]";

                env.CompileDeploy(expression);
                var listener = new MyUpdateListener();
                env.Statement("s0").AddListener(listener);

                var random = new Random();
                string[] users = { "U1000", "U1001", "U1002" };
                string[] ccy = { "USD", "JPY", "EUR" };
                string[] direction = { "B", "S" };

                for (var i = 0; i < 100; i++) {
                    var theEvent = new
                        SupportTradeEvent(
                            i,
                            users[random.Next(users.Length)],
                            ccy[random.Next(ccy.Length)],
                            direction[random.Next(
                                direction.Length
                            )]);
                    env.SendEventBean(theEvent);
                }

                Assert.AreEqual(0, listener.BadMatchCount);
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(OBSERVEROPS);
            }
        }

        private class MyUpdateListener : UpdateListener
        {
            private int badMatchCount;

            public int BadMatchCount => badMatchCount;

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
                var tradeevent1 = (SupportTradeEvent)eventBean.Get("tradeevent1");
                var tradeevent2 = (SupportTradeEvent)eventBean.Get("tradeevent2");
                var tradeevent3 = (SupportTradeEvent)eventBean.Get("tradeevent3");

                if (tradeevent1.UserId.Equals(tradeevent2.UserId) ||
                    tradeevent1.UserId.Equals(tradeevent3.UserId) ||
                    tradeevent2.UserId.Equals(tradeevent3.UserId)) {
                    /*
                    Console.WriteLine("Bad Match : ");
                    Console.WriteLine(tradeevent1);
                    Console.WriteLine(tradeevent2);
                    Console.WriteLine(tradeevent3 + "\n");
                    */
                    badMatchCount++;
                }
            }
        }

        private static void SendBeanAAssert(
            RegressionEnvironment env,
            string id,
            int intPrimitiveExpected,
            int numFiltersRemaining)
        {
            env.SendEventBean(new SupportBean_A(id));
            var fields = "c0".SplitCsv();
            env.AssertPropsNew("s0", fields, new object[] { intPrimitiveExpected });
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(
                    numFiltersRemaining,
                    SupportFilterServiceHelper.GetFilterSvcCount(statement, "SupportBean_A")));
        }

        private static void SendBeanAMiss(
            RegressionEnvironment env,
            string idCSV)
        {
            foreach (var id in idCSV.SplitCsv()) {
                env.SendEventBean(new SupportBean_A(id));
                env.AssertListenerNotInvoked("s0");
            }
        }
    }
} // end of namespace