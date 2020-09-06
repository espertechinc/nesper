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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorEveryDistinct
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctSimple());
            execs.Add(new PatternEveryDistinctWTime());
            execs.Add(new PatternExpireSeenBeforeKey());
            execs.Add(new PatternEveryDistinctOverFilter());
            execs.Add(new PatternRepeatOverDistinct());
            execs.Add(new PatternTimerWithinOverDistinct());
            execs.Add(new PatternEveryDistinctOverRepeat());
            execs.Add(new PatternEveryDistinctOverTimerWithin());
            execs.Add(new PatternEveryDistinctOverAnd());
            execs.Add(new PatternEveryDistinctOverOr());
            execs.Add(new PatternEveryDistinctOverNot());
            execs.Add(new PatternEveryDistinctOverFollowedBy());
            execs.Add(new PatternEveryDistinctWithinFollowedBy());
            execs.Add(new PatternFollowedByWithDistinct());
            execs.Add(new PatternInvalid());
            execs.Add(new PatternMonthScoped());
            execs.Add(new PatternEveryDistinctMultikeyWArray());
            return execs;
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

        private static void TryInvalid(
            RegressionEnvironment env,
            string statement,
            string message)
        {
            TryInvalidCompile(env, "select * from pattern[" + statement + "]", message);
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string)
        {
            env.SendEventBean(new SupportBean(@string, 0));
        }

        public class PatternEveryDistinctMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from pattern[every-distinct(a.Array) a=SupportEventWithIntArray]");
                env.AddListener("s0");

                SendAssertReceived(env, "E1", new int[] {1, 2}, true);
                SendAssertReceived(env, "E2", new int[] {1, 2}, false);
                SendAssertReceived(env, "E3", new int[] {1}, true);
                SendAssertReceived(env, "E4", new int[] { }, true);
                SendAssertReceived(env, "E5", null, true);

                env.Milestone(0);

                SendAssertReceived(env, "E10", new int[] {1, 2}, false);
                SendAssertReceived(env, "E11", new int[] {1}, false);
                SendAssertReceived(env, "E12", new int[] { }, false);
                SendAssertReceived(env, "E13", null, false);

                env.UndeployAll();
            }

            private void SendAssertReceived(
                RegressionEnvironment env,
                string id,
                int[] array,
                bool received)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array));
                Assert.AreEqual(received, env.Listener("s0").GetAndClearIsInvoked());
            }
        }

        public class PatternEveryDistinctSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0" };

                var epl =
                    "@Name('s0') select a.TheString as c0 from pattern [every-distinct(a.TheString) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.UndeployAll();
            }
        }

        public class PatternEveryDistinctWTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0" };

                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') select a.TheString as c0 from pattern [every-distinct(a.TheString, 5 sec) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(15000);

                env.Milestone(0);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.AdvanceTime(18000);

                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                env.AdvanceTime(19999);
                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.AdvanceTime(20000);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.UndeployAll();
            }
        }

        internal class PatternExpireSeenBeforeKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, 1 sec) a=SupportBean(TheString like 'A%')]";
                env.CompileDeploy(expression).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A1"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("A3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A3"});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("A5", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.AdvanceTime(1000);

                env.SendEventBean(new SupportBean("A4", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A4"});
                env.SendEventBean(new SupportBean("A5", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A5"});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("A6", 1));
                env.AdvanceTime(1999);
                env.SendEventBean(new SupportBean("A7", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.AdvanceTime(2000);
                env.SendEventBean(new SupportBean("A7", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A7"});

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var expression = "@Name('s0') select * from pattern [every-distinct(IntPrimitive) a=SupportBean]";
                RunEveryDistinctOverFilter(env, expression, milestone);

                expression = "@Name('s0') select * from pattern [every-distinct(IntPrimitive,2 minutes) a=SupportBean]";
                RunEveryDistinctOverFilter(env, expression, milestone);
            }

            private static void RunEveryDistinctOverFilter(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E5", 2));
                env.SendEventBean(new SupportBean("E6", 3));
                env.SendEventBean(new SupportBean("E7", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E8", 0));
                Assert.AreEqual("E8", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.EplToModelCompileDeploy(expression);

                env.UndeployAll();
            }
        }

        internal class PatternRepeatOverDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression = "@Name('s0') select * from pattern [[2] every-distinct(a.IntPrimitive) a=SupportBean]";
                RunRepeatOverDistinct(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [[2] every-distinct(a.IntPrimitive, 1 hour) a=SupportBean]";
                RunRepeatOverDistinct(env, expression, milestone);
            }

            private static void RunRepeatOverDistinct(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("E1", theEvent.Get("a[0].TheString"));
                Assert.AreEqual("E3", theEvent.Get("a[1].TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                env.SendEventBean(new SupportBean("E5", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverRepeat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a[0].IntPrimitive) [2] a=SupportBean]";
                RunEveryDistinctOverRepeat(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a[0].IntPrimitive, a[0].IntPrimitive, 1 hour) [2] a=SupportBean]";
                RunEveryDistinctOverRepeat(env, expression, milestone);
            }

            private static void RunEveryDistinctOverRepeat(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("E1", theEvent.Get("a[0].TheString"));
                Assert.AreEqual("E2", theEvent.Get("a[1].TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 1));
                env.SendEventBean(new SupportBean("E4", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E5", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 1));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("E5", theEvent.Get("a[0].TheString"));
                Assert.AreEqual("E6", theEvent.Get("a[1].TheString"));

                env.UndeployAll();
            }
        }

        internal class PatternTimerWithinOverDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // for 10 seconds, look for every distinct A
                var expression =
                    "@Name('s0') select * from pattern [(every-distinct(a.IntPrimitive) a=SupportBean) where timer:within(10 sec)]";
                RunTimerWithinOverDistinct(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [(every-distinct(a.IntPrimitive, 2 days 2 minutes) a=SupportBean) where timer:within(10 sec)]";
                RunTimerWithinOverDistinct(env, expression, milestone);
            }

            private static void RunTimerWithinOverDistinct(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                SendTimer(0, env);
                env.CompileDeploy(expression).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                SendTimer(11000, env);
                env.SendEventBean(new SupportBean("E4", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E5", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverTimerWithin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive) (a=SupportBean where timer:within(10 sec))]";
                RunEveryDistinctOverTimerWithin(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, 1 hour) (a=SupportBean where timer:within(10 sec))]";
                RunEveryDistinctOverTimerWithin(env, expression, milestone);
            }

            private static void RunEveryDistinctOverTimerWithin(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                SendTimer(0, env);
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                SendTimer(5000, env);
                env.SendEventBean(new SupportBean("E3", 2));
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.MilestoneInc(milestone);

                SendTimer(10000, env);
                env.SendEventBean(new SupportBean("E4", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E5", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(15000, env);
                env.SendEventBean(new SupportBean("E7", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                SendTimer(20000, env);
                env.SendEventBean(new SupportBean("E8", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                SendTimer(25000, env);
                env.SendEventBean(new SupportBean("E9", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                SendTimer(50000, env);
                env.SendEventBean(new SupportBean("E10", 1));
                Assert.AreEqual("E10", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.SendEventBean(new SupportBean("E11", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E12", 2));
                Assert.AreEqual("E12", env.Listener("s0").AssertOneGetNewAndReset().Get("a.TheString"));

                env.SendEventBean(new SupportBean("E13", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, b.IntPrimitive) (a=SupportBean(TheString like 'A%') and b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverAnd(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, b.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') and b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverAnd(env, expression, milestone);
            }

            private static void RunEveryDistinctOverAnd(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B1"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                env.SendEventBean(new SupportBean("B2", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A3", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A3", "B3"});

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("B4", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A4", "B4"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("B5", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A6", 2));
                env.SendEventBean(new SupportBean("B6", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A6", "B6"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A7", 2));
                env.SendEventBean(new SupportBean("B7", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(coalesce(a.IntPrimitive, 0) + coalesce(b.IntPrimitive, 0)) (a=SupportBean(TheString like 'A%') or b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverOr(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(coalesce(a.IntPrimitive, 0) + coalesce(b.IntPrimitive, 0), 1 hour) (a=SupportBean(TheString like 'A%') or b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverOr(env, expression, milestone);
            }

            private static void RunEveryDistinctOverOr(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", null});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {null, "B1"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B2", 1));
                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("A3", 2));
                env.SendEventBean(new SupportBean("B3", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {null, "B4"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B5", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {null, "B5"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B6", 3));
                env.SendEventBean(new SupportBean("A4", 3));
                env.SendEventBean(new SupportBean("A5", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive) (a=SupportBean(TheString like 'A%') and not SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverNot(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') and not SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverNot(env, expression, milestone);
            }

            private static void RunEveryDistinctOverNot(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A1"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A3"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A4", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString" },
                    new object[] {"A4"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A5", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive + b.IntPrimitive) (a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverFollowedBy(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive + b.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%'))]";
                RunEveryDistinctOverFollowedBy(env, expression, milestone);
            }

            private static void RunEveryDistinctOverFollowedBy(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("B1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B1"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                env.SendEventBean(new SupportBean("B2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 10));
                env.SendEventBean(new SupportBean("B3", -8));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A4", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A4", "B4"});

                env.SendEventBean(new SupportBean("A5", 3));
                env.SendEventBean(new SupportBean("B5", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctWithinFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [(every-distinct(a.IntPrimitive) a=SupportBean(TheString like 'A%')) -> b=SupportBean(IntPrimitive=a.IntPrimitive)]";
                RunEveryDistinctWithinFollowedBy(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [(every-distinct(a.IntPrimitive, 2 hours 1 minute) a=SupportBean(TheString like 'A%')) -> b=SupportBean(IntPrimitive=a.IntPrimitive)]";
                RunEveryDistinctWithinFollowedBy(env, expression, milestone);
            }

            private static void RunEveryDistinctWithinFollowedBy(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("B2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B2"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("A3", 3));
                env.SendEventBean(new SupportBean("A4", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A3", "B3"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B5", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A2", "B5"});

                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("B6", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A6", 4));
                env.SendEventBean(new SupportBean("B7", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A6", "B7"});

                env.UndeployAll();
            }
        }

        internal class PatternFollowedByWithDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive) a=SupportBean(TheString like 'A%') -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like 'B%')]";
                RunFollowedByWithDistinct(env, expression, milestone);

                expression =
                    "@Name('s0') select * from pattern [every-distinct(a.IntPrimitive, 1 day) a=SupportBean(TheString like 'A%') -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like 'B%')]";
                RunFollowedByWithDistinct(env, expression, milestone);
            }

            private static void RunFollowedByWithDistinct(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A1", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B1"});
                env.SendEventBean(new SupportBean("B2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B2"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("A2", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A1", "B4"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 2));
                env.SendEventBean(new SupportBean("B5", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.TheString","b.TheString" },
                    new object[] {"A3", "B5"});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B6", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B7", 3));
                var events = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    events,
                    new [] { "a.TheString","b.TheString" },
                    new[] {
                        new object[] {"A1", "B7"},
                        new object[] {"A3", "B7"}
                    });

                env.UndeployAll();
            }
        }

        internal class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalid(
                    env,
                    "a=SupportBean_A->every-distinct(a.IntPrimitive) SupportBean_B",
                    "Failed to validate pattern every-distinct expression 'a.IntPrimitive': Failed to resolve property 'a.IntPrimitive' to a stream or nested property in a stream");

                TryInvalid(
                    env,
                    "every-distinct(dummy) SupportBean_A",
                    "Failed to validate pattern every-distinct expression 'dummy': Property named 'dummy' is not valid in any stream ");

                TryInvalid(
                    env,
                    "every-distinct(2 sec) SupportBean_A",
                    "Every-distinct node requires one or more distinct-value expressions that each return non-constant result values");
            }
        }

        internal class PatternMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a.TheString","a.IntPrimitive" };

                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@Name('s0') select * from pattern [every-distinct(TheString, 1 month) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.SendEventBean(new SupportBean("E1", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E1", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");

                env.SendEventBean(new SupportBean("E1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 4});

                env.UndeployAll();
            }
        }
    }
} // end of namespace