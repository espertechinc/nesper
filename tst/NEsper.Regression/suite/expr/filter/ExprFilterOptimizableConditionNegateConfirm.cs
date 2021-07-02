///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterPlanHook;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableConditionNegateConfirm
    {
        private static readonly string HOOK = "@Hook(HookType=HookType.INTERNAL_FILTERSPEC, Hook='" + typeof(SupportFilterPlanHook).FullName + "')";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAndOrUnwinding(execs);
            WithOnePathNegate1Eq2WithDataflow(execs);
            WithOnePathNegate1Eq2WithStage(execs);
            WithOnePathNegate1Eq2WithContextFilter(execs);
            WithOnePathNegate1Eq2WithContextCategory(execs);
            WithOnePathOrLeftLRightV(execs);
            WithOnePathOrLeftLRightVWithPattern(execs);
            WithOnePathAndLeftLRightV(execs);
            WithOnePathAndLeftLRightVWithPattern(execs);
            WithOnePathAndLeftLOrVRightLOrV(execs);
            WithOnePathOrLeftVRightAndWithLL(execs);
            WithOnePathOrWithLVV(execs);
            WithOnePathAndWithOrLVVOrLVOrLV(execs);
            WithTwoPathOrWithLLV(execs);
            WithTwoPathOrLeftLRightAndLWithV(execs);
            WithTwoPathOrLeftOrLVRightOrLV(execs);
            WithTwoPathAndLeftOrLLRightV(execs);
            WithTwoPathAndLeftOrLVRightOrLL(execs);
            WithThreePathOrWithAndLVAndLVAndLV(execs);
            WithFourPathAndWithOrLLOrLL(execs);
            WithFourPathAndWithOrLLOrLLWithV(execs);
            WithFourPathAndWithOrLLOrLLOrVV(execs);
            WithTwoPathAndLeftOrLVVRightLL(execs);
            WithSixPathAndLeftOrLLVRightOrLL(execs);
            WithEightPathLeftOrLLVRightOrLLV(execs);
            WithAnyPathCompileMore(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAnyPathCompileMore(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterAnyPathCompileMore());
            return execs;
        }

        public static IList<RegressionExecution> WithEightPathLeftOrLLVRightOrLLV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterEightPathLeftOrLLVRightOrLLV());
            return execs;
        }

        public static IList<RegressionExecution> WithSixPathAndLeftOrLLVRightOrLL(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterSixPathAndLeftOrLLVRightOrLL());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathAndLeftOrLVVRightLL(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathAndLeftOrLVVRightLL());
            return execs;
        }

        public static IList<RegressionExecution> WithFourPathAndWithOrLLOrLLOrVV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterFourPathAndWithOrLLOrLLOrVV());
            return execs;
        }

        public static IList<RegressionExecution> WithFourPathAndWithOrLLOrLLWithV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterFourPathAndWithOrLLOrLLWithV());
            execs.Add(new ExprFilterFourPathAndWithOrLLOrLLWithV());
            return execs;
        }

        public static IList<RegressionExecution> WithFourPathAndWithOrLLOrLL(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterFourPathAndWithOrLLOrLL());
            return execs;
        }

        public static IList<RegressionExecution> WithThreePathOrWithAndLVAndLVAndLV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterThreePathOrWithAndLVAndLVAndLV());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathAndLeftOrLVRightOrLL(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathAndLeftOrLVRightOrLL());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathAndLeftOrLLRightV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathAndLeftOrLLRightV());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathOrLeftOrLVRightOrLV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathOrLeftOrLVRightOrLV());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathOrLeftLRightAndLWithV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathOrLeftLRightAndLWithV());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoPathOrWithLLV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterTwoPathOrWithLLV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathAndWithOrLVVOrLVOrLV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathAndWithOrLVVOrLVOrLV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathOrWithLVV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathOrWithLVV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathOrLeftVRightAndWithLL(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathOrLeftVRightAndWithLL());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathAndLeftLOrVRightLOrV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathAndLeftLOrVRightLOrV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathAndLeftLRightVWithPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathAndLeftLRightVWithPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathAndLeftLRightV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathAndLeftLRightV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathOrLeftLRightVWithPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathOrLeftLRightVWithPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathOrLeftLRightV(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathOrLeftLRightV());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathNegate1Eq2WithContextCategory(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathNegate1Eq2WithContextCategory());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathNegate1Eq2WithContextFilter(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathNegate1Eq2WithContextFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathNegate1Eq2WithStage(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathNegate1Eq2WithStage());
            return execs;
        }

        public static IList<RegressionExecution> WithOnePathNegate1Eq2WithDataflow(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOnePathNegate1Eq2WithDataflow());
            return execs;
        }

        public static IList<RegressionExecution> WithAndOrUnwinding(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterAndOrUnwinding());
            return execs;
        }

        private class ExprFilterAnyPathCompileMore : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var advanced = HasFilterIndexPlanAdvanced(env);
                {
                    var confirm = "context.s0.P00=\"x\" or context.s0.P01=\"y\"";
                    var pathOne = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a", confirm), MakeTriplet("P11", EQUAL, "c"));
                    var pathTwo = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a", confirm), MakeTriplet("P12", EQUAL, "d"));
                    var plan = new SupportFilterPlan(pathOne, pathTwo);
                    RunAssertion(env, plan, advanced, "(P10='a' or context.s0.P00='x' or context.s0.P01='y') and (P11='c' or P12='d')");
                }
            }

            private void RunAssertion(
                RegressionEnvironment env,
                SupportFilterPlan plan,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.Compile(epl);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", plan);
                }
            }
        }

        private class ExprFilterTwoPathAndLeftOrLVVRightLL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or context.s0.P00='x' or context.s0.P00='y') and (P11='b' or P12='c')");
                RunAssertion(env, milestone, advanced, "('c'=P12 or P11='b') and (context.s0.P00='x' or context.s0.P00='y' or 'a'=P10)");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");

                var pathWhenXOrY = "context.s0.P00=\"x\" or context.s0.P00=\"y\"";
                var pathOne = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a", pathWhenXOrY), MakeTriplet("P11", EQUAL, "b"));
                var pathTwo = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a", pathWhenXOrY), MakeTriplet("P12", EQUAL, "c"));
                var plan = new SupportFilterPlan(pathOne, pathTwo);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", plan);
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P11", EQUAL)},
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)},
                        });
                }

                SendS1Assert(env, 10, "a", "-", "-", false);
                SendS1Assert(env, 11, "a", "-", "c", true);
                SendS1Assert(env, 12, "a", "b", "-", true);
                SendS1Assert(env, 13, "-", "b", "c", false);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P11", EQUAL)},
                            new FilterItem[] {new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "-", "-", false);
                SendS1Assert(env, 21, "-", "-", "c", true);
                SendS1Assert(env, 22, "-", "b", "-", true);
                SendS1Assert(env, 23, "-", "b", "c", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.UndeployAll();
            }
        }

        private class ExprFilterEightPathLeftOrLLVRightOrLLV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or P11='b' or context.s0.P00='x') and (P12='c' or P13='d' or context.s0.P00='y')");
                RunAssertion(env, milestone, advanced, "(P11='b' or context.s0.P00='x' or P10='a') and (context.s0.P00='y' or P12='c' or P13='d')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");

                var whenNotXAndNotY = "not context.s0.P00=\"x\" and not context.s0.P00=\"y\"";
                var whenYAndNotX = "context.s0.P00=\"y\" and not context.s0.P00=\"x\"";
                var whenXAndNotY = "context.s0.P00=\"x\" and not context.s0.P00=\"y\"";
                var confirm = "context.s0.P00=\"x\" and context.s0.P00=\"y\"";
                var pathOne = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P12", EQUAL, "c"));
                var pathTwo = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P13", EQUAL, "d"));
                var pathThree = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P12", EQUAL, "c"));
                var pathFour = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P13", EQUAL, "d"));
                var pathFive = new SupportFilterPlanPath(whenYAndNotX, MakeTriplet("P10", EQUAL, "a"));
                var pathSix = new SupportFilterPlanPath(whenYAndNotX, MakeTriplet("P11", EQUAL, "b"));
                var pathSeven = new SupportFilterPlanPath(whenXAndNotY, MakeTriplet("P12", EQUAL, "c"));
                var pathEight = new SupportFilterPlanPath(whenXAndNotY, MakeTriplet("P13", EQUAL, "d"));
                var plan = new SupportFilterPlan(confirm, null, pathOne, pathTwo, pathThree, pathFour, pathFive, pathSix, pathSeven, pathEight);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", plan);
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P13", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P13", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "a", "b", "-", "-", false);
                SendS1Assert(env, 11, "-", "-", "c", "d", false);
                SendS1Assert(env, 12, "a", "-", "c", "-", true);
                SendS1Assert(env, 13, "-", "b", "c", "-", true);
                SendS1Assert(env, 14, "a", "-", "-", "d", true);
                SendS1Assert(env, 15, "-", "b", "-", "d", true);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P13", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "-", "-", "-", false);
                SendS1Assert(env, 21, "-", "-", "c", "-", true);
                SendS1Assert(env, 22, "-", "-", "-", "d", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.SendEventBean(new SupportBean_S0(3, "y"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 30, "-", "-", "-", "-", false);
                SendS1Assert(env, 31, "a", "-", "-", "-", true);
                SendS1Assert(env, 32, "-", "b", "-", "-", true);
                env.SendEventBean(new SupportBean_S2(3));

                env.UndeployAll();
            }
        }

        private class ExprFilterSixPathAndLeftOrLLVRightOrLL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or P11='b' or context.s0.P00='x') and (P12='c' or P13='d')");
                RunAssertion(env, milestone, advanced, "(P13='d' or 'c'=P12) and (context.s0.P00='x' or P11='b' or P10='a')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");

                var pathWhenX = "context.s0.P00=\"x\"";
                var pathWhenNotX = "not " + pathWhenX;
                var pathOne = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P12", EQUAL, "c"));
                var pathTwo = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P13", EQUAL, "d"));
                var pathThree = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P12", EQUAL, "c"));
                var pathFour = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P13", EQUAL, "d"));
                var pathFive = new SupportFilterPlanPath(pathWhenX, MakeTriplet("P12", EQUAL, "c"));
                var pathSix = new SupportFilterPlanPath(pathWhenX, MakeTriplet("P13", EQUAL, "d"));
                var plan = new SupportFilterPlan(pathOne, pathTwo, pathThree, pathFour, pathFive, pathSix);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", plan);
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P13", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P13", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "a", "b", "-", "-", false);
                SendS1Assert(env, 11, "-", "-", "c", "d", false);
                SendS1Assert(env, 12, "a", "-", "c", "-", true);
                SendS1Assert(env, 13, "-", "b", "c", "-", true);
                SendS1Assert(env, 14, "a", "-", "-", "d", true);
                SendS1Assert(env, 15, "-", "b", "-", "d", true);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P12", EQUAL)},
                            new FilterItem[] {new FilterItem("P13", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "-", "-", "-", false);
                SendS1Assert(env, 21, "-", "-", "c", "-", true);
                SendS1Assert(env, 22, "-", "-", "-", "d", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.UndeployAll();
            }
        }

        private class ExprFilterTwoPathAndLeftOrLVRightOrLL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or context.s0.P00='x') and (P11='c' or P12='d')");
                RunAssertion(env, milestone, advanced, "(P12='d' or P11='c') and (context.s0.P00='x' or P10='a')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");

                var pathOne = new SupportFilterPlanPath(
                    MakeTriplet("P10", EQUAL, "a", "context.s0.P00=\"x\""),
                    MakeTriplet("P11", EQUAL, "c"));
                var pathTwo = new SupportFilterPlanPath(
                    MakeTriplet("P10", EQUAL, "a", "context.s0.P00=\"x\""),
                    MakeTriplet("P12", EQUAL, "d"));
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P11", EQUAL)},
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "a", "c", "-", true);
                SendS1Assert(env, 11, "a", "-", "d", true);
                SendS1Assert(env, 12, "a", "c", "d", true);
                SendS1Assert(env, 13, "-", "c", "d", false);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P11", EQUAL)},
                            new FilterItem[] {new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "c", "-", true);
                SendS1Assert(env, 21, "-", "-", "d", true);
                SendS1Assert(env, 22, "-", "-", "-", false);
                env.SendEventBean(new SupportBean_S2(1));

                env.UndeployAll();
            }
        }

        private class ExprFilterFourPathAndWithOrLLOrLLOrVV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or P11='b') and (P12='c' or P13='d') and (s0.P00='x' or s0.P00='y')");
                RunAssertion(env, milestone, advanced, "(s0.P00='x' or s0.P00='y') and ('d'=P13 or 'c'=P12) and ('b'=P11 or 'a'=P10)");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(null, "s0.P00=\"x\" or s0.P00=\"y\"", MakeABCDCombinationPath()));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(env.Statement("s0"), "SupportBean_S1", MakeABCDCombinationFilterItems());
                }

                SendS1Assert(env, 10, "-", "-", "-", "-", false);
                SendS1Assert(env, 11, "a", "-", "c", "-", true);

                env.SendEventBean(new SupportBean_S0(2, "y"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(env.Statement("s0"), "SupportBean_S1", MakeABCDCombinationFilterItems());
                }

                SendS1Assert(env, 20, "-", "b", "c", "-", true);

                env.SendEventBean(new SupportBean_S0(3, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 30, "a", "-", "c", "-", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterFourPathAndWithOrLLOrLLWithV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or P11='b') and (P12='c' or P13='d') and s0.P00='x'");
                RunAssertion(env, milestone, advanced, "s0.P00='x' and ('d'=P13 or 'c'=P12) and ('b'=P11 or 'a'=P10)");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(null, "s0.P00=\"x\"", MakeABCDCombinationPath()));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(env.Statement("s0"), "SupportBean_S1", MakeABCDCombinationFilterItems());
                }

                SendS1Assert(env, 10, "-", "-", "-", "-", false);
                SendS1Assert(env, 11, "a", "-", "c", "-", true);

                env.SendEventBean(new SupportBean_S0(2, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 20, "a", "-", "c", "-", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterFourPathAndWithOrLLOrLL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or P11='b') and (P12='c' or P13='d')");
                RunAssertion(env, milestone, advanced, "('d'=P13 or 'c'=P12) and ('b'=P11 or 'a'=P10)");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from SupportBean_S1(" + filter + ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(MakeABCDCombinationPath()));
                }

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcByTypeMulti(env.Statement("s0"), "SupportBean_S1", MakeABCDCombinationFilterItems());
                }

                SendS1Assert(env, 10, "-", "-", "-", "-", false);
                SendS1Assert(env, 11, "a", "-", "c", "-", true);
                SendS1Assert(env, 12, "a", "-", "-", "d", true);
                SendS1Assert(env, 13, "-", "b", "c", "-", true);
                SendS1Assert(env, 14, "-", "b", "-", "d", true);
                SendS1Assert(env, 15, "a", "b", "-", "-", false);
                SendS1Assert(env, 16, "-", "-", "c", "d", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterThreePathOrWithAndLVAndLVAndLV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(
                    env,
                    milestone,
                    advanced,
                    "(P10 = 'a' and s0.P00 like '%1%') or (P10 = 'b' and s0.P00 like '%2%') or (P10 = 'c' and s0.P00 like '%3%')");
                RunAssertion(
                    env,
                    milestone,
                    advanced,
                    "(s0.P00 like '%2%' and P10 = 'b') or (s0.P00 like '%3%' and 'c' = P10) or (P10 = 'a' and s0.P00 like '%1%')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var pathOne = new SupportFilterPlanPath("s0.P00 like \"%1%\"", MakeTriplet("P10", EQUAL, "a"));
                var pathTwo = new SupportFilterPlanPath("s0.P00 like \"%2%\"", MakeTriplet("P10", EQUAL, "b"));
                var pathThree = new SupportFilterPlanPath("s0.P00 like \"%3%\"", MakeTriplet("P10", EQUAL, "c"));
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo, pathThree));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 10, "a", false);
                SendS1Assert(env, 11, "b", false);
                SendS1Assert(env, 12, "c", false);

                env.SendEventBean(new SupportBean_S0(2, "1"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 20, "c", false);
                SendS1Assert(env, 21, "b", false);
                SendS1Assert(env, 22, "a", true);

                env.SendEventBean(new SupportBean_S0(3, "2"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 30, "a", false);
                SendS1Assert(env, 31, "c", false);
                SendS1Assert(env, 32, "b", true);

                env.SendEventBean(new SupportBean_S0(4, "3"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 40, "a", false);
                SendS1Assert(env, 41, "b", false);
                SendS1Assert(env, 42, "c", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathAndWithOrLVVOrLVOrLV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(
                    env,
                    milestone,
                    advanced,
                    "(P10 = 'a' or s0.P00 like '%1%' or s0.P00 like '%2%') and " +
                    "(P11 = 'b' or s0.P00 like '%3%') and (P12 = 'c' or s0.P00 like '%4%')");
                RunAssertion(
                    env,
                    milestone,
                    advanced,
                    "('c' = P12 or s0.P00 like '%4%') and" +
                    "(s0.P00 like '%3%' or P11 = 'b') and" +
                    "(s0.P00 like '%1%' or P10 = 'a' or s0.P00 like '%2%')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var tripletOne = MakeTriplet("P10", EQUAL, "a", "s0.P00 like \"%1%\" or s0.P00 like \"%2%\"");
                var tripletTwo = MakeTriplet("P11", EQUAL, "b", "s0.P00 like \"%3%\"");
                var tripletThree = MakeTriplet("P12", EQUAL, "c", "s0.P00 like \"%4%\"");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(new SupportFilterPlanPath(tripletOne, tripletTwo, tripletThree)));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "a", "b", "-", false);
                SendS1Assert(env, 11, "-", "b", "c", false);
                SendS1Assert(env, 12, "a", "b", "c", true);

                env.SendEventBean(new SupportBean_S0(2, "1"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "b", "-", false);
                SendS1Assert(env, 21, "-", "-", "c", false);
                SendS1Assert(env, 22, "-", "b", "c", true);

                env.SendEventBean(new SupportBean_S0(3, "2"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 30, "-", "b", "-", false);
                SendS1Assert(env, 31, "-", "-", "c", false);
                SendS1Assert(env, 32, "-", "b", "c", true);

                env.SendEventBean(new SupportBean_S0(4, "3"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)}
                        });
                }

                SendS1Assert(env, 40, "a", "-", "-", false);
                SendS1Assert(env, 41, "-", "-", "c", false);
                SendS1Assert(env, 42, "a", "-", "c", true);

                env.SendEventBean(new SupportBean_S0(5, "4"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 50, "a", "-", "-", false);
                SendS1Assert(env, 51, "-", "b", "-", false);
                SendS1Assert(env, 52, "a", "b", "-", true);

                env.SendEventBean(new SupportBean_S0(6, "1234"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 60, "-", "-", "-", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterTwoPathAndLeftOrLLRightV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10 = 'a' or P11 = 'b') and context.s0.P00 = 'x'");
                RunAssertion(env, milestone, advanced, "context.s0.P00 = 'x' and (P10 = 'a' or P11 = 'b')");
                RunAssertion(env, milestone, advanced, "(P10 = 'a' or P11 = 'b') and context.s0.P00 = 'x'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var tripletOne = MakeTriplet("P10", EQUAL, "a");
                var tripletTwo = MakeTriplet("P11", EQUAL, "b");
                if (advanced) {
                    AssertPlanSingleByType(
                        "SupportBean_S1",
                        new SupportFilterPlan(null, "context.s0.P00=\"x\"", new SupportFilterPlanPath(tripletOne), new SupportFilterPlanPath(tripletTwo)));
                }

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 10, "a", "b", false);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "-", false);
                SendS1Assert(env, 21, "a", "-", true);
                SendS1Assert(env, 22, "-", "b", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.UndeployAll();
            }
        }

        private class ExprFilterTwoPathOrLeftOrLVRightOrLV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10 regexp '.*a.*' or context.s0.P00 = 'x') or (P11 regexp '.*b.*' or context.s0.P01 = 'y')");
                RunAssertion(env, milestone, advanced, "context.s0.P00 = 'x' or context.s0.P01 = 'y' or P10 regexp '.*a.*' or P11 regexp '.*b.*'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean_S1(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var tripletOne = MakeTripletRebool(".P10 regexp ?", "\".*a.*\"");
                var tripletTwo = MakeTripletRebool(".P11 regexp ?", "\".*b.*\"");
                if (advanced) {
                    AssertPlanSingleByType(
                        "SupportBean_S1",
                        new SupportFilterPlan(
                            "context.s0.P00=\"x\" or context.s0.P01=\"y\"",
                            null,
                            new SupportFilterPlanPath(tripletOne),
                            new SupportFilterPlanPath(tripletTwo)));
                }

                env.SendEventBean(new SupportBean_S0(1, "-", "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem(".P10 regexp ?", REBOOL)},
                            new FilterItem[] {new FilterItem(".P11 regexp ?", REBOOL)}
                        });
                }

                env.MilestoneInc(milestone);
                SendS1Assert(env, 10, "-", "-", false);
                SendS1Assert(env, 11, "-", "globe", true);
                SendS1Assert(env, 12, "garden", "-", true);
                SendS1Assert(env, 13, "globe", "garden", false);
                env.SendEventBean(new SupportBean_S2(1));

                env.SendEventBean(new SupportBean_S0(2, "x", "-"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 20, "-", "-", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.SendEventBean(new SupportBean_S0(3, "-", "y"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 30, "-", "-", true);
                env.SendEventBean(new SupportBean_S2(2));

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathOrLeftVRightAndWithLL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "s0.P00='x' or (P10 = 'a' and P11 regexp '.*b.*')");
                RunAssertion(env, milestone, advanced, "(P10 = 'a' and P11 regexp '.*b.*') or (s0.P00='x')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
                          "SupportBean_S1(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var tripletOne = MakeTriplet("P10", EQUAL, "a");
                var tripletTwo = MakeTripletRebool(".P11 regexp ?", "\".*b.*\"");
                var path = new SupportFilterPlanPath(tripletOne, tripletTwo);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.P00=\"x\"", null, path));
                }

                env.SendEventBean(new SupportBean_S0(1, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem(".P11 regexp ?", REBOOL)}
                        });
                }

                SendS1Assert(env, 10, "-", "b", false);
                SendS1Assert(env, 11, "a", "-", false);
                env.MilestoneInc(milestone);
                SendS1Assert(env, 12, "a", "globe", true);

                env.SendEventBean(new SupportBean_S0(2, "x"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 20, "-", "-", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterAndOrUnwinding : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "A=1 or (B=2 or C=3)", "A=1 or B=2 or C=3");
                RunAssertion(env, "A=1 and (B=2 and C=3)", "A=1 and B=2 and C=3");

                RunAssertion(env, "A=1 or (B=2 or (C=3 or D=4))", "A=1 or B=2 or C=3 or D=4");
                RunAssertion(env, "A=1 or (B=2 or (C=3 or (D=4 or E=5)))", "A=1 or B=2 or C=3 or D=4 or E=5");
                RunAssertion(env, "A=1 or (B=2 or (C=3 or (D=4 or (E=5 or F=6))))", "A=1 or B=2 or C=3 or D=4 or E=5 or F=6");
                RunAssertion(env, "A=1 or (B=2 or (C=3 or (D=4 or (E=5 or F=6 or G=7))))", "A=1 or B=2 or C=3 or D=4 or E=5 or F=6 or G=7");
                RunAssertion(env, "(((A=1 or B=2) or C=3) or D=4 or E=5) or F=6 or G=7", "A=1 or B=2 or C=3 or D=4 or E=5 or F=6 or G=7");

                RunAssertion(env, "A=1 and (B=2 and (C=3 and D=4))", "A=1 and B=2 and C=3 and D=4");
                RunAssertion(env, "A=1 and (B=2 and (C=3 and (D=4 and E=5)))", "A=1 and B=2 and C=3 and D=4 and E=5");
                RunAssertion(env, "A=1 and (B=2 and (C=3 and (D=4 and (E=5 and F=6))))", "A=1 and B=2 and C=3 and D=4 and E=5 and F=6");
                RunAssertion(env, "A=1 and (B=2 and (C=3 and (D=4 and (E=5 and F=6))))", "A=1 and B=2 and C=3 and D=4 and E=5 and F=6");
                RunAssertion(env, "A=1 and (B=2 and (C=3 and (D=4 and (E=5 and F=6 and G=7))))", "A=1 and B=2 and C=3 and D=4 and E=5 and F=6 and G=7");
                RunAssertion(env, "(((A=1 and B=2) and C=3) and D=4 and E=5) and F=6 and G=7", "A=1 and B=2 and C=3 and D=4 and E=5 and F=6 and G=7");

                RunAssertion(env, "(A=1 and (B=2 and C=3)) or (D=4 or (E=5 or F=6))", "(A=1 and B=2 and C=3) or D=4 or E=5 or F=6");
                RunAssertion(env, "((A=1 or B=2) or (C=3)) and (D=5 and E=6)", "(A=1 or B=2 or C=3) and D=5 and E=6");
                RunAssertion(env, "A=1 or B=2 and C=3 or D=4 and E=5", "A=1 or (B=2 and C=3) or (D=4 and E=5)");
                RunAssertion(
                    env,
                    "((A=1 and B=2 and C=3 and D=4) and E=5) or (F=6 or (G=7 or H=8 or I=9))",
                    "(A=1 and B=2 and C=3 and D=4 and E=5) or F=6 or G=7 or H=8 or I=9");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                string filter,
                string expectedText)
            {
                var epl = HOOK + "@Name('s0') select * from SupportBeanSimpleNumber(" + filter + ")";
                SupportFilterPlanHook.Reset();
                env.Compile(epl);
                var plan = SupportFilterPlanHook.AssertPlanSingleAndReset();
                var receivedNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(plan.PlanNodes);

                var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured("SupportBeanSimpleNumber");
                var typesPerStream = new EventType[] {eventType};
                var typeAliases = new string[] {"sbsn"};
                var expectedNode =
                    ((EPRuntimeSPI) env.Runtime).ReflectiveCompileSvc.ReflectiveCompileExpression(expectedText, typesPerStream, typeAliases);

                Assert.IsTrue(ExprNodeUtilityCompare.DeepEquals(expectedNode, receivedNode, true));
            }
        }

        private class ExprFilterOnePathAndLeftLOrVRightLOrV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "(P10='a' or s0.P00='x') and (P11='b' or s0.P01='y')");
                RunAssertion(env, milestone, advanced, "(s0.P01='y' or P11='b') and (s0.P00='x' or P10='a')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
                          "SupportBean_S1(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var tripletOne = MakeTriplet("P10", EQUAL, "a", "s0.P00=\"x\"");
                var tripletTwo = MakeTriplet("P11", EQUAL, "b", "s0.P01=\"y\"");
                var path = new SupportFilterPlanPath(tripletOne, tripletTwo);
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(path));
                }

                env.SendEventBean(new SupportBean_S0(1, "-", "-"));
                env.MilestoneInc(milestone);
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "-", "b", false);
                SendS1Assert(env, 11, "a", "-", false);
                SendS1Assert(env, 12, "a", "b", true);

                env.SendEventBean(new SupportBean_S0(2, "x", "-"));
                env.MilestoneInc(milestone);
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P11", EQUAL));
                }

                SendS1Assert(env, 21, "a", "-", false);
                SendS1Assert(env, 20, "-", "b", true);

                env.SendEventBean(new SupportBean_S0(2, "-", "y"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 30, "-", "b", false);
                SendS1Assert(env, 31, "a", "-", true);

                env.SendEventBean(new SupportBean_S0(2, "x", "y"));
                env.MilestoneInc(milestone);
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 40, "-", "-", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterTwoPathOrLeftLRightAndLWithV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "P10='a' or (P11='b' and s0.P00='x')");
                RunAssertion(env, milestone, advanced, "(s0.P00='x' and P11='b') or P10='a'");
                RunAssertion(env, milestone, advanced, "(P11='b' and s0.P00='x') or P10='a'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
                          "SupportBean_S1(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var pathOne = MakePathFromSingle("P10", EQUAL, "a");
                var pathTwo = new SupportFilterPlanPath("s0.P00=\"x\"", MakeTriplet("P11", EQUAL, "b"));
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo));
                }

                env.SendEventBean(new SupportBean_S0(1, "x"));
                env.MilestoneInc(milestone);
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 10, "-", "-", false);
                SendS1Assert(env, 11, "-", "b", true);

                env.SendEventBean(new SupportBean_S0(2, "x"));
                env.MilestoneInc(milestone);
                SendS1Assert(env, 20, "-", "-", false);
                SendS1Assert(env, 21, "a", "-", true);

                env.SendEventBean(new SupportBean_S0(3, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 30, "-", "b", false);
                SendS1Assert(env, 31, "a", "b", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterTwoPathOrWithLLV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "P10='a' or P11='b' or s0.P00='x'");
                RunAssertion(env, milestone, advanced, "P11='b' or s0.P00='x' or P10='a'");
                RunAssertion(env, milestone, advanced, "s0.P00='x' or P11='b' or P10='a'");
                RunAssertion(env, milestone, advanced, "(s0.P00='x' or P11='b') or P10='a'");
                RunAssertion(env, milestone, advanced, "s0.P00='x' or (P11='b' or P10='a')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
                          "SupportBean_S1(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var pathOne = MakePathFromSingle("P10", EQUAL, "a");
                var pathTwo = MakePathFromSingle("P11", EQUAL, "b");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.P00=\"x\"", null, pathOne, pathTwo));
                }

                env.SendEventBean(new SupportBean_S0(1, "x"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 10, "-", "-", true);

                env.SendEventBean(new SupportBean_S0(2, "y"));
                env.MilestoneInc(milestone);
                if (advanced) {
                    AssertFilterSvcByTypeMulti(
                        env.Statement("s0"),
                        "SupportBean_S1",
                        new FilterItem[][] {
                            new FilterItem[] {new FilterItem("P10", EQUAL)},
                            new FilterItem[] {new FilterItem("P11", EQUAL)}
                        });
                }

                SendS1Assert(env, 20, "-", "-", false);
                SendS1Assert(env, 21, "a", "-", true);

                env.SendEventBean(new SupportBean_S0(3, "y"));
                SendS1Assert(env, 30, "-", "-", false);
                SendS1Assert(env, 31, "-", "b", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathOrWithLVV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, advanced, "P10='a' or s0.P00='x' or s0.P01='y'");
                RunAssertion(env, advanced, "s0.P00='x' or P10='a' or s0.P01='y'");
                RunAssertion(env, advanced, "s0.P00='x' or (s0.P01='y' or P10='a')");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
                          "SupportBean_S1(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                var path = MakePathFromSingle("P10", EQUAL, "a");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.P00=\"x\" or s0.P01=\"y\"", null, path));
                }

                env.SendEventBean(new SupportBean_S0(1, "x", "-"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 10, "-", true);

                env.SendEventBean(new SupportBean_S0(2, "-", "y"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean_S1");
                }

                SendS1Assert(env, 20, "-", true);

                env.SendEventBean(new SupportBean_S0(3, "-", "-"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean_S1", new FilterItem("P10", EQUAL));
                }

                SendS1Assert(env, 30, "-", false);
                SendS1Assert(env, 31, "a", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathNegate1Eq2WithContextFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select * from SupportBean_S0;\n" +
                          "create context MyContext start SupportBean_S0(1=2);\n" +
                          "@Name('s0') context MyContext select * from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                SendSBAssert(env, "E1", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathNegate1Eq2WithStage : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = HOOK + "@Name('s0') select * from SupportBean(1=2)";
                var advanced = HasFilterIndexPlanAdvanced(env);
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingle(new SupportFilterPlan(null, "1=2", MakePathsFromEmpty()));
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                var deploymentId = env.DeploymentId("s0");

                SendSBAssert(env, "E1", false);

                env.StageService.GetStage("P1");
                StageIt(env, "P1", deploymentId);

                env.StageService.GetStage("P1").EventService.SendEventBean(new SupportBean("E1", 1), "SupportBean");
                Assert.IsFalse(env.ListenerStage("P1", "s0").IsInvokedAndReset());

                UnstageIt(env, "P1", deploymentId);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathNegate1Eq2WithDataflow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<SupportBean> { filter : 1 = 2 } " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");

                var future = new DefaultSupportCaptureOp(env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                df.Start();

                env.SendEventBean(new SupportBean());

                Thread.Sleep(100);

                Assert.AreEqual(0, future.Current.Length);

                df.Cancel();
                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathNegate1Eq2WithContextCategory : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext group by TheString='abc' and 1=2 as categoryOne from SupportBean;\n" +
                          "@Name('s0') context MyContext select * from SupportBean;\n";
                var compiled = env.Compile(epl);
                var advanced = HasFilterIndexPlanAdvanced(env);
                if (advanced) {
                    SupportMessageAssertUtil.TryInvalidDeploy(
                        env,
                        compiled,
                        "Failed to deploy: Category context 'MyContext' for category 'categoryOne' has evaluated to a condition that cannot become true");
                }
            }
        }

        private class ExprFilterOnePathOrLeftLRightVWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "TheString = 'abc' or (s0.P00 || s1.P10 || s2[0].P20 || s2[1].P20 = 'QRST')");
                RunAssertion(env, milestone, advanced, "(s0.P00 || s1.P10 || s2[0].P20 || s2[1].P20 = 'QRST') or TheString = 'abc'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK +
                          "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> s1=SupportBean_S1 -> [2] s2=SupportBean_S2 -> " +
                          "SupportBean(" +
                          filter +
                          ")]";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingleByType(
                        "SupportBean",
                        new SupportFilterPlan("s0.P00||s1.P10||s2[0].P20||s2[1].P20=\"QRST\"", null, MakePathsFromSingle("TheString", EQUAL, "abc")));
                }

                env.SendEventBean(new SupportBean_S0(1, "Q"));
                env.SendEventBean(new SupportBean_S1(2, "R"));
                env.SendEventBean(new SupportBean_S2(3, "S"));
                env.SendEventBean(new SupportBean_S2(4, "T"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean");
                }

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "x", true);

                env.SendEventBean(new SupportBean_S0(11, "Q"));
                env.SendEventBean(new SupportBean_S1(12, "-"));
                env.SendEventBean(new SupportBean_S2(13, "-"));
                env.SendEventBean(new SupportBean_S2(14, "-"));
                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
                }

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
                }

                SendSBAssert(env, "x", false);
                SendSBAssert(env, "abc", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathAndLeftLRightVWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "TheString = 'abc' and s0.P00 = 'x'");
                RunAssertion(env, milestone, advanced, "s0.P00 = 'x' and TheString = 'abc'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = HOOK + "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean(" + filter + ")];\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingleByType("SupportBean", new SupportFilterPlan(null, "s0.P00=\"x\"", MakePathsFromSingle("TheString", EQUAL, "abc")));
                }

                env.SendEventBean(new SupportBean_S0(1, "x"));

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
                }

                SendSBAssert(env, "def", false);
                SendSBAssert(env, "abc", true);

                env.SendEventBean(new SupportBean_S0(2, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "abc", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathAndLeftLRightV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, advanced, "TheString = 'abc' and context.s0.P00 = 'x'");
                RunAssertion(env, milestone, advanced, "context.s0.P00 = 'x' and TheString = 'abc'");
                RunAssertion(env, milestone, advanced, "context.s0.P00 = 'x' and TheString = 'abc'");
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                bool advanced,
                string filter)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S1;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingle(new SupportFilterPlan(null, "context.s0.P00=\"x\"", MakePathsFromSingle("TheString", EQUAL, "abc")));
                }

                env.SendEventBean(new SupportBean_S0(1, "x"));
                if (advanced) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", EQUAL);
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", false);

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", EQUAL);
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", false);
                env.SendEventBean(new SupportBean_S1(1));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                env.SendEventBean(new SupportBean_S0(2, "-"));
                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "abc", false);
                SendSBAssert(env, "def", false);

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcNone(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "abc", false);
                SendSBAssert(env, "def", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterOnePathOrLeftLRightV : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var advanced = HasFilterIndexPlanAdvanced(env);
                RunAssertion(env, milestone, "TheString = 'abc' or context.s0.P00 = 'x'", advanced);
                RunAssertion(env, milestone, "context.s0.P00 = 'x' or TheString = 'abc'", advanced);
                RunAssertion(env, milestone, "context.s0.P00 = 'x' or TheString = 'abc'", advanced);
            }

            private void RunAssertion(
                RegressionEnvironment env,
                AtomicLong milestone,
                string filter,
                bool advanced)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S1;\n" +
                          HOOK +
                          "@Name('s0') context MyContext select * from SupportBean(" +
                          filter +
                          ");\n";
                SupportFilterPlanHook.Reset();
                env.CompileDeploy(epl).AddListener("s0");
                if (advanced) {
                    AssertPlanSingle(new SupportFilterPlan("context.s0.P00=\"x\"", null, MakePathsFromSingle("TheString", EQUAL, "abc")));
                }

                env.SendEventBean(new SupportBean_S0(1, "x"));
                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", true);

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcEmpty(env.Statement("s0"), "SupportBean");
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", true);
                env.SendEventBean(new SupportBean_S1(1));

                env.SendEventBean(new SupportBean_S0(2, "-"));
                if (advanced) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", EQUAL);
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", false);

                env.MilestoneInc(milestone);

                if (advanced) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", EQUAL);
                }

                SendSBAssert(env, "abc", true);
                SendSBAssert(env, "def", false);

                env.UndeployAll();
            }
        }

        private static SupportFilterPlanPath[] MakeABCDCombinationPath()
        {
            var pathOne = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P12", EQUAL, "c"));
            var pathTwo = new SupportFilterPlanPath(MakeTriplet("P10", EQUAL, "a"), MakeTriplet("P13", EQUAL, "d"));
            var pathThree = new SupportFilterPlanPath(MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P12", EQUAL, "c"));
            var pathFour = new SupportFilterPlanPath(MakeTriplet("P11", EQUAL, "b"), MakeTriplet("P13", EQUAL, "d"));
            return new SupportFilterPlanPath[] {pathOne, pathTwo, pathThree, pathFour};
        }

        private static FilterItem[][] MakeABCDCombinationFilterItems()
        {
            return new FilterItem[][] {
                new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P12", EQUAL)},
                new FilterItem[] {new FilterItem("P10", EQUAL), new FilterItem("P13", EQUAL)},
                new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P12", EQUAL)},
                new FilterItem[] {new FilterItem("P11", EQUAL), new FilterItem("P13", EQUAL)}
            };
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            bool received)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            int id,
            string p10,
            string p11,
            string p12,
            string p13,
            bool expected)
        {
            env.SendEventBean(new SupportBean_S1(id, p10, p11, p12, p13));
            Assert.AreEqual(expected, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            int id,
            string p10,
            string p11,
            string p12,
            bool expected)
        {
            SendS1Assert(env, id, p10, p11, p12, null, expected);
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            int id,
            string p10,
            string p11,
            bool expected)
        {
            SendS1Assert(env, id, p10, p11, null, expected);
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            int id,
            string p10,
            bool expected)
        {
            SendS1Assert(env, id, p10, null, expected);
        }
    }
} // end of namespace