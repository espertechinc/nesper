///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternConsumingPattern
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithOrOp(execs);
            WithFollowedByOp(execs);
            WithMatchUntilOp(execs);
            WithObserverOp(execs);
            WithAndOp(execs);
            WithNotOpNotImpacted(execs);
            WithGuardOp(execs);
            WithEveryOp(execs);
            WithCombination(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithCombination(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternCombination());
            return execs;
        }

        public static IList<RegressionExecution> WithEveryOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternEveryOp());
            return execs;
        }

        public static IList<RegressionExecution> WithGuardOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternGuardOp());
            return execs;
        }

        public static IList<RegressionExecution> WithNotOpNotImpacted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternNotOpNotImpacted());
            return execs;
        }

        public static IList<RegressionExecution> WithAndOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternAndOp());
            return execs;
        }

        public static IList<RegressionExecution> WithObserverOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternObserverOp());
            return execs;
        }

        public static IList<RegressionExecution> WithMatchUntilOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternMatchUntilOp());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedByOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternFollowedByOp());
            return execs;
        }

        public static IList<RegressionExecution> WithOrOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOrOp());
            return execs;
        }

        private class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.TryInvalidCompile(
                    path,
                    "select * from pattern @XX [SupportIdEventA]",
                    "Unrecognized pattern-level annotation 'XX' [select * from pattern @XX [SupportIdEventA]]");

                var expected =
                    "Discard-partials and suppress-matches is not supported in a joins, context declaration and on-action ";
                env.TryInvalidCompile(
                    path,
                    "select * from pattern " +
                    GetText(TargetEnum.DISCARD_AND_SUPPRESS) +
                    "[SupportIdEventA]#keepall, A#keepall",
                    expected +
                    "[select * from pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [SupportIdEventA]#keepall, A#keepall]");

                env.CompileDeploy("@public create window AWindow#keepall as SupportIdEventA", path);
                env.TryInvalidCompile(
                    path,
                    "on pattern " +
                    GetText(TargetEnum.DISCARD_AND_SUPPRESS) +
                    "[SupportIdEventA] select * from AWindow",
                    expected +
                    "[on pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [SupportIdEventA] select * from AWindow]");

                env.UndeployAll();
            }
        }

        private class PatternCombination : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var testsoda in new bool[] { false, true }) {
                    foreach (var target in EnumHelper.GetValues<TargetEnum>()) {
                        TryAssertionTargetCurrentMatch(env, testsoda, target);
                        TryAssertionTargetNextMatch(env, testsoda, target);
                    }
                }

                // test order-by
                var epl =
                    "@name('s0') select * from pattern @DiscardPartialsOnMatch [every a=SupportIdEventA -> SupportIdEventB] order by a.id desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportIdEventA("A1", null, null));
                env.SendEventBean(new SupportIdEventA("A2", null, null));
                env.SendEventBean(new SupportIdEventB("B1", null));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    "a.id".SplitCsv(),
                    new object[][] { new object[] { "A2" }, new object[] { "A1" } });

                env.UndeployAll();
            }
        }

        private class PatternFollowedByOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunFollowedByOp(env, milestone, "every a1=SupportIdEventA -> a2=SupportIdEventA", false);
                RunFollowedByOp(env, milestone, "every a1=SupportIdEventA -> a2=SupportIdEventA", true);
                RunFollowedByOp(env, milestone, "every a1=SupportIdEventA -[10]> a2=SupportIdEventA", false);
                RunFollowedByOp(env, milestone, "every a1=SupportIdEventA -[10]> a2=SupportIdEventA", true);
            }
        }

        private class PatternMatchUntilOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionMatchUntilBoundOp(env, milestone, true);
                TryAssertionMatchUntilBoundOp(env, milestone, false);
                TryAssertionMatchUntilWChildMatcher(env, milestone, true);
                TryAssertionMatchUntilWChildMatcher(env, milestone, false);
                TryAssertionMatchUntilRangeOpWTime(env, milestone); // with time
            }
        }

        private class PatternObserverOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a.id,b.id".SplitCsv();
                SendTime(env, 0);

                var epl = "@name('s0') select * from pattern " +
                          GetText(TargetEnum.DISCARD_ONLY) +
                          " [" +
                          "every a=SupportIdEventA -> b=SupportIdEventB -> timer:interval(a.mysec)]";
                env.CompileDeploy(epl).AddListener("s0");

                SendAEvent(env, "A1", 5); // 5 seconds for this one

                env.Milestone(0);

                SendAEvent(env, "A2", 1); // 1 seconds for this one
                SendBEvent(env, "B1");

                env.Milestone(1);

                SendTime(env, 1000);
                env.AssertPropsNew("s0", fields, new object[] { "A2", "B1" });

                env.Milestone(2);

                SendTime(env, 5000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternAndOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                RunAndWAndState(env, milestone, true);
                RunAndWAndState(env, milestone, false);

                RunAndWChild(env, milestone, true);
                RunAndWChild(env, milestone, false);
            }
        }

        private class PatternNotOpNotImpacted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a.id".SplitCsv();
                SendTime(env, 0);

                var epl = "@name('s0') select * from pattern " +
                          GetText(TargetEnum.DISCARD_ONLY) +
                          " [" +
                          "every a=SupportIdEventA -> timer:interval(a.mysec) and not (SupportIdEventB -> SupportIdEventC)]";
                env.CompileDeploy(epl).AddListener("s0");

                SendAEvent(env, "A1", 5); // 5 sec
                SendAEvent(env, "A2", 1); // 1 sec
                SendBEvent(env, "B1");
                SendTime(env, 1000);
                env.AssertPropsNew("s0", fields, new object[] { "A2" });

                SendCEvent(env, "C1", null);
                SendTime(env, 5000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternGuardOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunGuardOpBeginState(env, milestone, true);
                RunGuardOpBeginState(env, milestone, false);
                RunGuardOpChildState(env, milestone, true);
                RunGuardOpChildState(env, milestone, false);
            }
        }

        private class PatternOrOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a.id,b.id,c.id".SplitCsv();
                SendTime(env, 0);

                var epl = "@name('s0') select * from pattern " +
                          GetText(TargetEnum.DISCARD_ONLY) +
                          " [" +
                          "every a=SupportIdEventA -> (b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa)) or timer:interval(1000)]";
                env.CompileDeploy(epl).AddListener("s0");

                SendAEvent(env, "A1", "x");
                SendAEvent(env, "A2", "y");

                env.Milestone(0);

                SendBEvent(env, "B1");

                env.Milestone(1);

                SendCEvent(env, "C1", "y");
                env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

                env.Milestone(2);

                SendCEvent(env, "C1", "x");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class PatternEveryOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                TryAssertionEveryBeginState(env, milestone, "");
                TryAssertionEveryBeginState(env, milestone, "-distinct(id)");
                TryAssertionEveryBeginState(env, milestone, "-distinct(id, 10 seconds)");

                TryAssertionEveryChildState(env, milestone, "", true);
                TryAssertionEveryChildState(env, milestone, "", false);
                TryAssertionEveryChildState(env, milestone, "-distinct(id)", true);
                TryAssertionEveryChildState(env, milestone, "-distinct(id)", false);
                TryAssertionEveryChildState(env, milestone, "-distinct(id, 10 seconds)", true);
                TryAssertionEveryChildState(env, milestone, "-distinct(id, 10 seconds)", false);
            }
        }

        private static void TryAssertionEveryChildState(
            RegressionEnvironment env,
            AtomicLong milestone,
            string everySuffix,
            bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      " [" +
                      "every a=SupportIdEventA-> every" +
                      everySuffix +
                      " (b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void TryAssertionEveryBeginState(
            RegressionEnvironment env,
            AtomicLong milestone,
            string distinct)
        {
            var fields = "a.id,b.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      GetText(TargetEnum.DISCARD_ONLY) +
                      "[" +
                      "every a=SupportIdEventA-> every" +
                      distinct +
                      " b=SupportIdEventB]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");
            env.AssertPropsNew("s0", fields, new object[] { "A1", "B1" });

            env.MilestoneInc(milestone);

            SendBEvent(env, "B2");
            env.AssertListenerNotInvoked("s0");

            SendAEvent(env, "A2");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B3");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B3" });

            env.MilestoneInc(milestone);

            SendBEvent(env, "B4");
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void RunFollowedByOp(
            RegressionEnvironment env,
            AtomicLong milestone,
            string pattern,
            bool matchDiscard)
        {
            var fields = "a1.id,a2.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      "[" +
                      pattern +
                      "]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "E1");
            SendAEvent(env, "E2");
            env.AssertPropsNew("s0", fields, new object[] { "E1", "E2" });

            env.MilestoneInc(milestone);

            SendAEvent(env, "E3");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E3" });
            }

            env.MilestoneInc(milestone);

            SendAEvent(env, "E4");
            env.AssertPropsNew("s0", fields, new object[] { "E3", "E4" });

            env.MilestoneInc(milestone);

            SendAEvent(env, "E5");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "E4", "E5" });
            }

            SendAEvent(env, "E6");
            env.AssertPropsNew("s0", fields, new object[] { "E5", "E6" });

            env.UndeployAll();
        }

        private static void TryAssertionTargetNextMatch(
            RegressionEnvironment env,
            bool testSoda,
            TargetEnum target)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();
            var epl = "@name('s0') select * from pattern " +
                      GetText(target) +
                      "[every a=SupportIdEventA -> b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa)]";
            env.CompileDeploy(testSoda, epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");
            SendBEvent(env, "B1");
            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            SendCEvent(env, "C2", "x");
            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.NONE) {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }
            else {
                env.AssertListenerNotInvoked("s0");
            }

            env.UndeployAll();
        }

        private static void TryAssertionMatchUntilBoundOp(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b[0].id,b[1].id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      "[" +
                      "every a=SupportIdEventA-> [2] b=SupportIdEventB(pb in (a.pa, '-'))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1", "-"); // applies to both matches

            env.MilestoneInc(milestone);

            SendBEvent(env, "B2", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "B2" });

            env.MilestoneInc(milestone);

            SendBEvent(env, "B3", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "B3" });
            }

            env.UndeployAll();
        }

        private static void TryAssertionMatchUntilWChildMatcher(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b[0].id,c[0].id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      " [" +
                      "every a=SupportIdEventA-> [1] (b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void TryAssertionMatchUntilRangeOpWTime(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "a1.id,aarr[0].id".SplitCsv();
            SendTime(env, 0);

            var epl = "@name('s0') select * from pattern " +
                      GetText(TargetEnum.DISCARD_ONLY) +
                      "[" +
                      "every a1=SupportIdEventA -> ([:100] aarr=SupportIdEventA until (timer:interval(10 sec) and not b=SupportIdEventB))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1");

            env.MilestoneInc(milestone);

            SendTime(env, 1000);
            SendAEvent(env, "A2");
            SendTime(env, 10000);
            env.AssertPropsNew("s0", fields, new object[] { "A1", "A2" });

            env.MilestoneInc(milestone);

            SendTime(env, 11000);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void TryAssertionTargetCurrentMatch(
            RegressionEnvironment env,
            bool testSoda,
            TargetEnum target)
        {
            var fields = "a1.id,aarr[0].id,b.id".SplitCsv();
            var epl = "@name('s0') select * from pattern " +
                      GetText(target) +
                      "[every a1=SupportIdEventA -> [:10] aarr=SupportIdEventA until b=SupportIdEventB]";
            env.CompileDeploy(testSoda, epl).AddListener("s0");

            SendAEvent(env, "A1");
            SendAEvent(env, "A2");
            SendBEvent(env, "B1");

            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.DISCARD_AND_SUPPRESS) {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "A2", "B1" });
            }
            else {
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] { new object[] { "A1", "A2", "B1" }, new object[] { "A2", null, "B1" } }));
            }

            env.UndeployAll();
        }

        private static void RunAndWAndState(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      " [" +
                      "every a=SupportIdEventA-> b=SupportIdEventB and c=SupportIdEventC(pc=a.pa)]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void RunAndWChild(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      " [" +
                      "every a=SupportIdEventA-> SupportIdEventD and (b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa))]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");

            env.MilestoneInc(milestone);

            SendAEvent(env, "A2", "y");
            SendDEvent(env, "D1");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void RunGuardOpBeginState(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      "[" +
                      "every a=SupportIdEventA-> b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa) where timer:within(1)]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");

            env.MilestoneInc(milestone);

            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void RunGuardOpChildState(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".SplitCsv();

            var epl = "@name('s0') select * from pattern " +
                      (matchDiscard ? GetText(TargetEnum.DISCARD_ONLY) : "") +
                      " [" +
                      "every a=SupportIdEventA-> (b=SupportIdEventB -> c=SupportIdEventC(pc=a.pa)) where timer:within(1)]";
            env.CompileDeploy(epl).AddListener("s0");

            SendAEvent(env, "A1", "x");
            SendAEvent(env, "A2", "y");
            SendBEvent(env, "B1");

            env.MilestoneInc(milestone);

            SendCEvent(env, "C1", "y");
            env.AssertPropsNew("s0", fields, new object[] { "A2", "B1", "C1" });

            env.MilestoneInc(milestone);

            SendCEvent(env, "C2", "x");
            if (matchDiscard) {
                env.AssertListenerNotInvoked("s0");
            }
            else {
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C2" });
            }

            env.UndeployAll();
        }

        private static void SendTime(
            RegressionEnvironment env,
            long msec)
        {
            env.AdvanceTime(msec);
        }

        private static void SendAEvent(
            RegressionEnvironment env,
            string id)
        {
            SendAEvent(id, null, null, env);
        }

        private static void SendAEvent(
            RegressionEnvironment env,
            string id,
            string pa)
        {
            SendAEvent(id, pa, null, env);
        }

        private static void SendDEvent(
            RegressionEnvironment env,
            string id)
        {
            env.SendEventBean(new SupportIdEventD(id));
        }

        private static void SendAEvent(
            RegressionEnvironment env,
            string id,
            int mysec)
        {
            SendAEvent(id, null, mysec, env);
        }

        private static void SendAEvent(
            string id,
            string pa,
            int? mysec,
            RegressionEnvironment env)
        {
            env.SendEventBean(new SupportIdEventA(id, pa, mysec));
        }

        private static void SendBEvent(
            RegressionEnvironment env,
            string id)
        {
            SendBEvent(env, id, null);
        }

        private static void SendBEvent(
            RegressionEnvironment env,
            string id,
            string pb)
        {
            env.SendEventBean(new SupportIdEventB(id, pb));
        }

        private static void SendCEvent(
            RegressionEnvironment env,
            string id,
            string pc)
        {
            env.SendEventBean(new SupportIdEventC(id, pc));
        }

        internal enum TargetEnum
        {
            DISCARD_ONLY,
            DISCARD_AND_SUPPRESS,
            SUPPRESS_ONLY,
            NONE
        }

        internal static string GetText(TargetEnum value)
        {
            return value switch {
                TargetEnum.DISCARD_ONLY => ("@DiscardPartialsOnMatch "),
                TargetEnum.DISCARD_AND_SUPPRESS => ("@DiscardPartialsOnMatch @SuppressOverlappingMatches "),
                TargetEnum.SUPPRESS_ONLY => ("@SuppressOverlappingMatches "),
                TargetEnum.NONE => (""),
                _ => throw new ArgumentException(nameof(value))
            };
        }
    }
} // end of namespace