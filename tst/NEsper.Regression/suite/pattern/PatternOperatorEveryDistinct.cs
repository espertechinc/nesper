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

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternOperatorEveryDistinct
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithEveryDistinctSimple(execs);
            WithEveryDistinctWTime(execs);
            WithExpireSeenBeforeKey(execs);
            WithEveryDistinctOverFilter(execs);
            WithRepeatOverDistinct(execs);
            WithTimerWithinOverDistinct(execs);
            WithEveryDistinctOverRepeat(execs);
            WithEveryDistinctOverTimerWithin(execs);
            WithEveryDistinctOverAnd(execs);
            WithEveryDistinctOverOr(execs);
            WithEveryDistinctOverNot(execs);
            WithEveryDistinctOverFollowedBy(execs);
            WithEveryDistinctWithinFollowedBy(execs);
            WithFollowedByWithDistinct(execs);
            WithInvalid(execs);
            WithMonthScoped(execs);
            WithEveryDistinctMultikeyWArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctMultikeyWArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByWithDistinct(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternFollowedByWithDistinct());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctWithinFollowedBy(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctWithinFollowedBy());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverFollowedBy(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverFollowedBy());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverNot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverNot());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverOr());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverAnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverAnd());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverTimerWithin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverTimerWithin());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverRepeat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverRepeat());
            return execs;
        }

        public static IList<RegressionExecution> WithTimerWithinOverDistinct(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternTimerWithinOverDistinct());
            return execs;
        }

        public static IList<RegressionExecution> WithRepeatOverDistinct(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternRepeatOverDistinct());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctOverFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctOverFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithExpireSeenBeforeKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternExpireSeenBeforeKey());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctWTime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctWTime());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryDistinctSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryDistinctSimple());
            return execs;
        }

        internal class PatternEveryDistinctMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select * from pattern[every-distinct(a.array) a=SupportEventWithIntArray]");
                env.AddListener("s0");

                SendAssertReceived(env, "E1", new int[] { 1, 2 }, true);
                SendAssertReceived(env, "E2", new int[] { 1, 2 }, false);
                SendAssertReceived(env, "E3", new int[] { 1 }, true);
                SendAssertReceived(env, "E4", new int[] { }, true);
                SendAssertReceived(env, "E5", null, true);

                env.Milestone(0);

                SendAssertReceived(env, "E10", new int[] { 1, 2 }, false);
                SendAssertReceived(env, "E11", new int[] { 1 }, false);
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
                env.AssertListenerInvokedFlag("s0", received);
            }
        }

        internal class PatternEveryDistinctSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl =
                    "@name('s0') select a.theString as c0 from pattern [every-distinct(a.theString) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctWTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select a.theString as c0 from pattern [every-distinct(a.theString, 5 sec) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(15000);

                env.Milestone(0);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.AdvanceTime(18000);

                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                env.AdvanceTime(19999);
                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(20000);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                SendSupportBean(env, "E2");
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                SendSupportBean(env, "E1");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E2");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.UndeployAll();
            }
        }

        internal class PatternExpireSeenBeforeKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, 1 sec) a=SupportBean(theString like 'A%')]";
                env.CompileDeploy(expression).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A1", 1));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A1" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A2", 1));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(new SupportBean("A3", 2));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A3" });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("A5", 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.AdvanceTime(1000);

                env.SendEventBean(new SupportBean("A4", 1));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A4" });
                env.SendEventBean(new SupportBean("A5", 2));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A5" });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("A6", 1));
                env.AdvanceTime(1999);
                env.SendEventBean(new SupportBean("A7", 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);

                env.AdvanceTime(2000);
                env.SendEventBean(new SupportBean("A7", 2));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A7" });

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var expression = "@name('s0') select * from pattern [every-distinct(intPrimitive) a=SupportBean]";
                RunEveryDistinctOverFilter(env, expression, milestone);

                expression = "@name('s0') select * from pattern [every-distinct(intPrimitive,2 minutes) a=SupportBean]";
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
                AssertAString(env, "E1");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                AssertAString(env, "E3");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                AssertAString(env, "E4");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E5", 2));
                env.SendEventBean(new SupportBean("E6", 3));
                env.SendEventBean(new SupportBean("E7", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E8", 0));
                AssertAString(env, "E8");

                env.EplToModelCompileDeploy(expression);

                env.UndeployAll();
            }
        }

        internal class PatternRepeatOverDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression = "@name('s0') select * from pattern [[2] every-distinct(a.intPrimitive) a=SupportBean]";
                RunRepeatOverDistinct(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [[2] every-distinct(a.intPrimitive, 1 hour) a=SupportBean]";
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
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual("E1", theEvent.Get("a[0].theString"));
                        Assert.AreEqual("E3", theEvent.Get("a[1].theString"));
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                env.SendEventBean(new SupportBean("E5", 2));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverRepeat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a[0].intPrimitive) [2] a=SupportBean]";
                RunEveryDistinctOverRepeat(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a[0].intPrimitive, a[0].intPrimitive, 1 hour) [2] a=SupportBean]";
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
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual("E1", theEvent.Get("a[0].theString"));
                        Assert.AreEqual("E2", theEvent.Get("a[1].theString"));
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 1));
                env.SendEventBean(new SupportBean("E4", 2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E5", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 1));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual("E5", theEvent.Get("a[0].theString"));
                        Assert.AreEqual("E6", theEvent.Get("a[1].theString"));
                    });

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
                    "@name('s0') select * from pattern [(every-distinct(a.intPrimitive) a=SupportBean) where timer:within(10 sec)]";
                RunTimerWithinOverDistinct(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [(every-distinct(a.intPrimitive, 2 days 2 minutes) a=SupportBean) where timer:within(10 sec)]";
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
                AssertAString(env, "E1");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                AssertAString(env, "E3");

                env.MilestoneInc(milestone);

                SendTimer(11000, env);
                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverTimerWithin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive) (a=SupportBean where timer:within(10 sec))]";
                RunEveryDistinctOverTimerWithin(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, 1 hour) (a=SupportBean where timer:within(10 sec))]";
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
                AssertAString(env, "E1");

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                SendTimer(5000, env);
                env.SendEventBean(new SupportBean("E3", 2));
                AssertAString(env, "E3");

                env.MilestoneInc(milestone);

                SendTimer(10000, env);
                env.SendEventBean(new SupportBean("E4", 1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 2));
                env.AssertListenerNotInvoked("s0");

                SendTimer(15000, env);
                env.SendEventBean(new SupportBean("E7", 2));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                SendTimer(20000, env);
                env.SendEventBean(new SupportBean("E8", 2));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                SendTimer(25000, env);
                env.SendEventBean(new SupportBean("E9", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                SendTimer(50000, env);
                env.SendEventBean(new SupportBean("E10", 1));
                AssertAString(env, "E10");

                env.SendEventBean(new SupportBean("E11", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E12", 2));
                AssertAString(env, "E12");

                env.SendEventBean(new SupportBean("E13", 2));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, b.intPrimitive) (a=SupportBean(theString like 'A%') and b=SupportBean(theString like 'B%'))]";
                RunEveryDistinctOverAnd(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, b.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') and b=SupportBean(theString like 'B%'))]";
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
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 10));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B1" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                env.SendEventBean(new SupportBean("B2", 10));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("A3", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 10));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A3", "B3" });

                env.SendEventBean(new SupportBean("A4", 1));
                env.SendEventBean(new SupportBean("B4", 20));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A4", "B4" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("B5", 10));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A6", 2));
                env.SendEventBean(new SupportBean("B6", 20));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A6", "B6" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A7", 2));
                env.SendEventBean(new SupportBean("B7", 20));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(coalesce(a.intPrimitive, 0) + coalesce(b.intPrimitive, 0)) (a=SupportBean(theString like 'A%') or b=SupportBean(theString like 'B%'))]";
                RunEveryDistinctOverOr(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(coalesce(a.intPrimitive, 0) + coalesce(b.intPrimitive, 0), 1 hour) (a=SupportBean(theString like 'A%') or b=SupportBean(theString like 'B%'))]";
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
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", null });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 2));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { null, "B1" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B2", 1));
                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("A3", 2));
                env.SendEventBean(new SupportBean("B3", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 3));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { null, "B4" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B5", 4));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { null, "B5" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B6", 3));
                env.SendEventBean(new SupportBean("A4", 3));
                env.SendEventBean(new SupportBean("A5", 4));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverNot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive) (a=SupportBean(theString like 'A%') and not SupportBean(theString like 'B%'))]";
                RunEveryDistinctOverNot(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') and not SupportBean(theString like 'B%'))]";
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
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A1" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 2));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A3" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B1", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A4", 1));
                env.AssertPropsNew("s0", "a.theString".SplitCsv(), new object[] { "A4" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A5", 1));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctOverFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive + b.intPrimitive) (a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%'))]";
                RunEveryDistinctOverFollowedBy(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive + b.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%'))]";
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
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportBean("B1", 1));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B1" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 1));
                env.SendEventBean(new SupportBean("B2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 10));
                env.SendEventBean(new SupportBean("B3", -8));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A4", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 1));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A4", "B4" });

                env.SendEventBean(new SupportBean("A5", 3));
                env.SendEventBean(new SupportBean("B5", 0));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class PatternEveryDistinctWithinFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [(every-distinct(a.intPrimitive) a=SupportBean(theString like 'A%')) -> b=SupportBean(intPrimitive=a.intPrimitive)]";
                RunEveryDistinctWithinFollowedBy(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [(every-distinct(a.intPrimitive, 2 hours 1 minute) a=SupportBean(theString like 'A%')) -> b=SupportBean(intPrimitive=a.intPrimitive)]";
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
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportBean("B2", 1));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B2" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("A3", 3));
                env.SendEventBean(new SupportBean("A4", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 3));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A3", "B3" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B5", 2));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A2", "B5" });

                env.SendEventBean(new SupportBean("A5", 2));
                env.SendEventBean(new SupportBean("B6", 2));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A6", 4));
                env.SendEventBean(new SupportBean("B7", 4));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A6", "B7" });

                env.UndeployAll();
            }
        }

        internal class PatternFollowedByWithDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive) a=SupportBean(theString like 'A%') -> every-distinct(b.intPrimitive) b=SupportBean(theString like 'B%')]";
                RunFollowedByWithDistinct(env, expression, milestone);

                expression =
                    "@name('s0') select * from pattern [every-distinct(a.intPrimitive, 1 day) a=SupportBean(theString like 'A%') -> every-distinct(b.intPrimitive) b=SupportBean(theString like 'B%')]";
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
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B1" });
                env.SendEventBean(new SupportBean("B2", 1));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B2" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B3", 0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("A2", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B4", 2));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A1", "B4" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 2));
                env.SendEventBean(new SupportBean("B5", 1));
                env.AssertPropsNew("s0", "a.theString,b.theString".SplitCsv(), new object[] { "A3", "B5" });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B6", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("B7", 3));
                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.GetAndResetLastNewData();
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            events,
                            "a.theString,b.theString".SplitCsv(),
                            new object[][] { new object[] { "A1", "B7" }, new object[] { "A3", "B7" } });
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
                    "a=SupportBean_A->every-distinct(a.intPrimitive) SupportBean_B",
                    "Failed to validate pattern every-distinct expression 'a.intPrimitive': Failed to resolve property 'a.intPrimitive' to a stream or nested property in a stream");

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
                var fields = "a.theString,a.intPrimitive".SplitCsv();

                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@name('s0') select * from pattern [every-distinct(theString, 1 month) a=SupportBean]";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.SendEventBean(new SupportBean("E1", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E1", 3));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");

                env.SendEventBean(new SupportBean("E1", 4));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 4 });

                env.UndeployAll();
            }
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
            env.TryInvalidCompile("select * from pattern[" + statement + "]", message);
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

        private static void AssertAString(
            RegressionEnvironment env,
            string expected)
        {
            env.AssertEqualsNew("s0", "a.theString", expected);
        }
    }
} // end of namespace