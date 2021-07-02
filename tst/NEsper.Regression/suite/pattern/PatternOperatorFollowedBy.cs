///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorFollowedBy
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithOpWHarness(execs);
            WithFollowedByWithNot(execs);
            WithFollowedByTimer(execs);
            WithMemoryRFIDEvent(execs);
            WithRFIDZoneExit(execs);
            WithRFIDZoneEnter(execs);
            WithFollowedNotEvery(execs);
            WithFollowedEveryMultiple(execs);
            WithFilterGreaterThen(execs);
            WithFollowedOrPermFalse(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedOrPermFalse(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedOrPermFalse());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterGreaterThen(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFilterGreaterThen());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedEveryMultiple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedEveryMultiple());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedNotEvery(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedNotEvery());
            return execs;
        }

        public static IList<RegressionExecution> WithRFIDZoneEnter(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternRFIDZoneEnter());
            return execs;
        }

        public static IList<RegressionExecution> WithRFIDZoneExit(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternRFIDZoneExit());
            return execs;
        }

        public static IList<RegressionExecution> WithMemoryRFIDEvent(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternMemoryRFIDEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByTimer(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedByTimer());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByWithNot(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternFollowedByWithNot());
            return execs;
        }

        public static IList<RegressionExecution> WithOpWHarness(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new PatternOpWHarness());
            return execs;
        }

        private static long DateToLong(string dateText)
        {
            var format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.fff");
            var date = format.Parse(dateText);
            log.Debug(".dateToLong out=" + date);
            return date.UtcMillis;
        }

        private static SupportCallEvent SendEvent(
            EPEventService runtime,
            long callId,
            string source,
            string destination,
            long startTime,
            long endTime)
        {
            var theEvent = new SupportCallEvent(callId, source, destination, startTime, endTime);
            runtime.SendEventBean(theEvent, nameof(SupportCallEvent));
            return theEvent;
        }

        private static SupportBean_A SendA(
            string id,
            RegressionEnvironment env)
        {
            var a = new SupportBean_A(id);
            env.SendEventBean(a);
            return a;
        }

        private static void SendB(
            string id,
            RegressionEnvironment env)
        {
            var b = new SupportBean_B(id);
            env.SendEventBean(b);
        }

        private static void SendC(
            string id,
            RegressionEnvironment env)
        {
            var c = new SupportBean_C(id);
            env.SendEventBean(c);
        }

        private static void SendTimer(
            long time,
            RegressionEnvironment env)
        {
            env.AdvanceTime(time);
        }

        internal class PatternOpWHarness : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var events = EventCollectionFactory.GetEventSetOne(0, 1000);
                var testCaseList = new CaseList();
                EventExpressionCase testCase;

                testCase = new EventExpressionCase("b=SupportBean_B -> (d=SupportBean_D or not d=SupportBean_D)");
                testCase.Add("B1", "b", events.GetEvent("B1"), "d", null);
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B -[1000]> (d=SupportBean_D or not d=SupportBean_D)");
                testCase.Add("B1", "b", events.GetEvent("B1"), "d", null);
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B -> every d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B -> d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B -> not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("b=SupportBean_B -[1000]> not d=SupportBean_D");
                testCase.Add("B1", "b", events.GetEvent("B1"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B -> every d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B -> d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every b=SupportBean_B -[10]> d=SupportBean_D");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (b=SupportBean_B -> every d=SupportBean_D)");
                testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
                testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
                testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (a_1=SupportBean_A() -> b=SupportBean_B -> a_2=SupportBean_A)");
                testCase.Add(
                    "A2",
                    "a_1",
                    events.GetEvent("A1"),
                    "b",
                    events.GetEvent("B1"),
                    "a_2",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("c=SupportBean_C() -> d=SupportBean_D -> a=SupportBean_A");
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (a_1=SupportBean_A() -> b=SupportBean_B() -> a_2=SupportBean_A())");
                testCase.Add(
                    "A2",
                    "a_1",
                    events.GetEvent("A1"),
                    "b",
                    events.GetEvent("B1"),
                    "a_2",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase(
                    "every (a_1=SupportBean_A() -[10]> b=SupportBean_B() -[10]> a_2=SupportBean_A())");
                testCase.Add(
                    "A2",
                    "a_1",
                    events.GetEvent("A1"),
                    "b",
                    events.GetEvent("B1"),
                    "a_2",
                    events.GetEvent("A2"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every ( every a=SupportBean_A -> every b=SupportBean_B)");
                testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
                testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
                testCase.Add("B3", "a", events.GetEvent("A1"), "b", events.GetEvent("B3"));
                testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
                testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
                testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                testCase = new EventExpressionCase("every (a=SupportBean_A() -> every b=SupportBean_B())");
                testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
                testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
                testCase.Add("B3", "a", events.GetEvent("A1"), "b", events.GetEvent("B3"));
                testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
                testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
                testCaseList.AddTest(testCase);

                var util = new PatternTestHarness(events, testCaseList, GetType());
                util.RunTest(env);
            }
        }

        internal class PatternFollowedByWithNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "@Name('s0') select * from pattern [" +
                           " every a=SupportBean_A -> (timer:interval(10 seconds) and not (SupportBean_B(Id=a.Id) or SupportBean_C(Id=a.Id)))" +
                           "] ";

                SendTimer(0, env);
                env.CompileDeploy(stmt);
                env.AddListener("s0");

                SupportBean_A eventA;
                EventBean received;

                // test case where no Completed or Cancel event arrives
                eventA = SendA("A1", env);
                SendTimer(9999, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(10000, env);
                received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(eventA, received.Get("a"));

                // test case where Completed event arrives within the time set
                SendTimer(20000, env);
                SendA("A2", env);
                SendTimer(29999, env);
                SendB("A2", env);
                SendTimer(30000, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // test case where Cancelled event arrives within the time set
                SendTimer(30000, env);
                SendA("A3", env);
                SendTimer(30000, env);
                SendC("A3", env);
                SendTimer(40000, env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // test case where no matching Completed or Cancel event arrives
                eventA = SendA("A4", env);
                SendB("B4", env);
                SendC("A5", env);
                SendTimer(50000, env);
                received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(eventA, received.Get("a"));

                env.UndeployAll();
            }
        }

        internal class PatternFollowedByTimer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select * from pattern " +
                                 "[every A=SupportCallEvent -> every B=SupportCallEvent(Dest=A.Dest, StartTime in [A.StartTime:A.EndTime]) where timer:within (7200000)]" +
                                 "where B.Source != A.Source";
                env.CompileDeploy(expression);

                env.AddListener("s0");

                var eventOne = SendEvent(
                    env.EventService,
                    2000002601,
                    "18",
                    "123456789014795",
                    DateToLong("2005-09-26 13:02:53.200"),
                    DateToLong("2005-09-26 13:03:34.400"));
                var eventTwo = SendEvent(
                    env.EventService,
                    2000002607,
                    "20",
                    "123456789014795",
                    DateToLong("2005-09-26 13:03:17.300"),
                    DateToLong("2005-09-26 13:03:58.600"));

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(eventOne, theEvent.Get("A"));
                Assert.AreSame(eventTwo, theEvent.Get("B"));

                var eventThree = SendEvent(
                    env.EventService,
                    2000002610,
                    "22",
                    "123456789014795",
                    DateToLong("2005-09-26 13:03:31.300"),
                    DateToLong("2005-09-26 13:04:12.100"));
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                theEvent = env.Listener("s0").LastNewData[0];
                Assert.AreSame(eventOne, theEvent.Get("A"));
                Assert.AreSame(eventThree, theEvent.Get("B"));
                theEvent = env.Listener("s0").LastNewData[1];
                Assert.AreSame(eventTwo, theEvent.Get("A"));
                Assert.AreSame(eventThree, theEvent.Get("B"));

                env.UndeployAll();
            }
        }

        internal class PatternMemoryRFIDEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select 'Tag May Be Broken' as alert, " +
                                 "tagMayBeBroken.Mac, " +
                                 "tagMayBeBroken.ZoneID " +
                                 "from pattern [" +
                                 "every tagMayBeBroken=SupportRFIDEvent -> (timer:interval(10 sec) and not SupportRFIDEvent(Mac=tagMayBeBroken.Mac))" +
                                 "]";

                env.CompileDeploy(expression);

                env.AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    var theEvent = new SupportRFIDEvent("a", "111");
                    env.SendEventBean(theEvent);

                    theEvent = new SupportRFIDEvent("a", "111");
                    env.SendEventBean(theEvent);
                }

                env.UndeployAll();
            }
        }

        internal class PatternRFIDZoneExit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every LR event with a zone of '1' activates a new sub-expression after
                // the followed-by operator. The sub-expression instance can end two different ways:
                // It ends when a LR for the same mac and a different exit-zone comes in, or
                // it ends when a LR for the same max and the same zone come in. The latter also starts the
                // sub-expression again.
                var expression = "@Name('s0') select * " +
                                 "from pattern [" +
                                 "every a=SupportRFIDEvent(ZoneID='1') -> (b=SupportRFIDEvent(Mac=a.Mac,ZoneID!='1') and not SupportRFIDEvent(Mac=a.Mac,ZoneID='1'))" +
                                 "]";
                env.CompileDeploy(expression).AddListener("s0");

                var theEvent = new SupportRFIDEvent("a", "1");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("a", "2");
                env.SendEventBean(theEvent);
                Assert.AreEqual(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("b"));

                theEvent = new SupportRFIDEvent("b", "1");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("b", "1");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("b", "2");
                env.SendEventBean(theEvent);
                Assert.AreEqual(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("b"));

                env.UndeployAll();
            }
        }

        internal class PatternRFIDZoneEnter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every LR event with a zone other then '1' activates a new sub-expression after
                // the followed-by operator. The sub-expression instance can end two different ways:
                // It ends when a LR for the same mac and the enter-zone comes in, or
                // it ends when a LR for the same max and the same zone come in. The latter also starts the
                // sub-expression again.
                var expression = "@Name('s0') select * " +
                                 "from pattern [" +
                                 "every a=SupportRFIDEvent(ZoneID!='1') -> (b=SupportRFIDEvent(Mac=a.Mac,ZoneID='1') and not SupportRFIDEvent(Mac=a.Mac,ZoneID=a.ZoneID))" +
                                 "]";

                env.CompileDeploy(expression);

                env.AddListener("s0");

                var theEvent = new SupportRFIDEvent("a", "2");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("a", "1");
                env.SendEventBean(theEvent);
                Assert.AreEqual(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("b"));

                theEvent = new SupportRFIDEvent("b", "2");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("b", "2");
                env.SendEventBean(theEvent);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                theEvent = new SupportRFIDEvent("b", "1");
                env.SendEventBean(theEvent);
                Assert.AreEqual(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Get("b"));

                env.UndeployAll();
            }
        }

        internal class PatternFollowedNotEvery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@Name('s0') select * from pattern [every A=SupportBean -> (timer:interval(1 seconds) and not SupportBean_A)]";

                env.AdvanceTime(0);

                env.CompileDeploy(expression);

                env.AddListener("s0");

                object eventOne = new SupportBean();
                env.SendEventBean(eventOne);

                object eventTwo = new SupportBean();
                env.SendEventBean(eventTwo);

                env.AdvanceTime(1000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").NewDataList[0].Length);

                env.UndeployAll();
            }
        }

        internal class PatternFollowedEveryMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@Name('s0') select * from pattern [every a=SupportBean_A -> b=SupportBean_B -> c=SupportBean_C -> d=SupportBean_D]";

                env.CompileDeploy(expression);

                env.AddListener("s0");

                var events = new object[10];
                events[0] = new SupportBean_A("A1");
                env.SendEventBean(events[0]);

                events[1] = new SupportBean_A("A2");
                env.SendEventBean(events[1]);

                events[2] = new SupportBean_B("B1");
                env.SendEventBean(events[2]);

                events[3] = new SupportBean_C("C1");
                env.SendEventBean(events[3]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                events[4] = new SupportBean_D("D1");
                env.SendEventBean(events[4]);
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                string[] fields = {"a", "b", "c", "d"};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new[] {events[0], events[2], events[3], events[4]});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[1],
                    fields,
                    new[] {events[1], events[2], events[3], events[4]});

                env.UndeployAll();
            }
        }

        internal class PatternFilterGreaterThen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-411
                env.CompileDeploy(
                    "@Name('s0') select * from pattern[every a=SupportBean -> b=SupportBean(b.IntPrimitive <= a.IntPrimitive)]");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                env.CompileDeploy(
                    "@Name('s0') select * from pattern [every a=SupportBean -> b=SupportBean(a.IntPrimitive >= b.IntPrimitive)]");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternFollowedOrPermFalse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var pattern = "@Name('s0') select * from pattern [every s=SupportBean(TheString='E') -> " +
                              "(timer:interval(10) and not SupportBean(TheString='C1'))" +
                              "or" +
                              "(SupportBean(TheString='C2') and not timer:interval(10))]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E", 0));

                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(11000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace