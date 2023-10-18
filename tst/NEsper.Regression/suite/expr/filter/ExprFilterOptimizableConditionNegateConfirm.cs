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
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper; //hasFilterIndexPlanAdvanced
using static com.espertech.esper.regressionlib.support.filter.SupportFilterPlanHook;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;
using static com.espertech.esper.regressionlib.support.stage.SupportStageUtil; // stageIt, unstageIt

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
	public class ExprFilterOptimizableConditionNegateConfirm {
	    private static readonly string HOOK = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" + typeof(SupportFilterPlanHook).FullName + "')";

	    public static ICollection<RegressionExecution> Executions() {
	        var executions = new List<RegressionExecution>();
	        executions.Add(new ExprFilterAndOrUnwinding());
	        executions.Add(new ExprFilterOnePathNegate1Eq2WithDataflow());
	        executions.Add(new ExprFilterOnePathNegate1Eq2WithStage());
	        executions.Add(new ExprFilterOnePathNegate1Eq2WithContextFilter());
	        executions.Add(new ExprFilterOnePathNegate1Eq2WithContextCategory());
	        executions.Add(new ExprFilterOnePathOrLeftLRightV());
	        executions.Add(new ExprFilterOnePathOrLeftLRightVWithPattern());
	        executions.Add(new ExprFilterOnePathAndLeftLRightV());
	        executions.Add(new ExprFilterOnePathAndLeftLRightVWithPattern());
	        executions.Add(new ExprFilterOnePathAndLeftLOrVRightLOrV());
	        executions.Add(new ExprFilterOnePathOrLeftVRightAndWithLL());
	        executions.Add(new ExprFilterOnePathOrWithLVV());
	        executions.Add(new ExprFilterOnePathAndWithOrLVVOrLVOrLV());
	        executions.Add(new ExprFilterTwoPathOrWithLLV());
	        executions.Add(new ExprFilterTwoPathOrLeftLRightAndLWithV());
	        executions.Add(new ExprFilterTwoPathOrLeftOrLVRightOrLV());
	        executions.Add(new ExprFilterTwoPathAndLeftOrLLRightV());
	        executions.Add(new ExprFilterTwoPathAndLeftOrLVRightOrLL());
	        executions.Add(new ExprFilterThreePathOrWithAndLVAndLVAndLV());
	        executions.Add(new ExprFilterFourPathAndWithOrLLOrLL());
	        executions.Add(new ExprFilterFourPathAndWithOrLLOrLLWithV());
	        executions.Add(new ExprFilterFourPathAndWithOrLLOrLLOrVV());
	        executions.Add(new ExprFilterTwoPathAndLeftOrLVVRightLL());
	        executions.Add(new ExprFilterSixPathAndLeftOrLLVRightOrLL());
	        executions.Add(new ExprFilterEightPathLeftOrLLVRightOrLLV());
	        executions.Add(new ExprFilterAnyPathCompileMore());
	        return executions;
	    }

	    private class ExprFilterAnyPathCompileMore : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            {
	                var confirm = "context.s0.p00=\"x\" or context.s0.p01=\"y\"";
	                var pathOne = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", confirm), MakeTriplet("p11", EQUAL, "c"));
	                var pathTwo = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", confirm), MakeTriplet("p12", EQUAL, "d"));
	                var plan = new SupportFilterPlan(pathOne, pathTwo);
	                RunAssertion(env, plan, advanced, "(p10='a' or context.s0.p00='x' or context.s0.p01='y') and (p11='c' or p12='d')");
	            }
	        }

	        private void RunAssertion(RegressionEnvironment env, SupportFilterPlan plan, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.Compile(epl);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", plan);
	            }
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathAndLeftOrLVVRightLL : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or context.s0.p00='x' or context.s0.p00='y') and (p11='b' or p12='c')");
	            RunAssertion(env, milestone, advanced, "('c'=p12 or p11='b') and (context.s0.p00='x' or context.s0.p00='y' or 'a'=p10)");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");

	            var pathWhenXOrY = "context.s0.p00=\"x\" or context.s0.p00=\"y\"";
	            var pathOne = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", pathWhenXOrY), MakeTriplet("p11", EQUAL, "b"));
	            var pathTwo = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", pathWhenXOrY), MakeTriplet("p12", EQUAL, "c"));
	            var plan = new SupportFilterPlan(pathOne, pathTwo);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", plan);
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p11", EQUAL)},
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)},
	                });
	            }
	            SendS1Assert(env, 10, "a", "-", "-", false);
	            SendS1Assert(env, 11, "a", "-", "c", true);
	            SendS1Assert(env, 12, "a", "b", "-", true);
	            SendS1Assert(env, 13, "-", "b", "c", false);
	            env.SendEventBean(new SupportBean_S2(1));

	            env.SendEventBean(new SupportBean_S0(2, "x"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p11", EQUAL)},
	                    new FilterItem[]{new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "-", "-", false);
	            SendS1Assert(env, 21, "-", "-", "c", true);
	            SendS1Assert(env, 22, "-", "b", "-", true);
	            SendS1Assert(env, 23, "-", "b", "c", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterEightPathLeftOrLLVRightOrLLV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or p11='b' or context.s0.p00='x') and (p12='c' or p13='d' or context.s0.p00='y')");
	            RunAssertion(env, milestone, advanced, "(p11='b' or context.s0.p00='x' or p10='a') and (context.s0.p00='y' or p12='c' or p13='d')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");

	            var whenNotXAndNotY = "not context.s0.p00=\"x\" and not context.s0.p00=\"y\"";
	            var whenYAndNotX = "context.s0.p00=\"y\" and not context.s0.p00=\"x\"";
	            var whenXAndNotY = "context.s0.p00=\"x\" and not context.s0.p00=\"y\"";
	            var confirm = "context.s0.p00=\"x\" and context.s0.p00=\"y\"";
	            var pathOne = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p12", EQUAL, "c"));
	            var pathTwo = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p13", EQUAL, "d"));
	            var pathThree = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p12", EQUAL, "c"));
	            var pathFour = new SupportFilterPlanPath(whenNotXAndNotY, MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p13", EQUAL, "d"));
	            var pathFive = new SupportFilterPlanPath(whenYAndNotX, MakeTriplet("p10", EQUAL, "a"));
	            var pathSix = new SupportFilterPlanPath(whenYAndNotX, MakeTriplet("p11", EQUAL, "b"));
	            var pathSeven = new SupportFilterPlanPath(whenXAndNotY, MakeTriplet("p12", EQUAL, "c"));
	            var pathEight = new SupportFilterPlanPath(whenXAndNotY, MakeTriplet("p13", EQUAL, "d"));
	            var plan = new SupportFilterPlan(confirm, null, pathOne, pathTwo, pathThree, pathFour, pathFive, pathSix, pathSeven, pathEight);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", plan);
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p13", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p13", EQUAL)}
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
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p13", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "-", "-", "-", false);
	            SendS1Assert(env, 21, "-", "-", "c", "-", true);
	            SendS1Assert(env, 22, "-", "-", "-", "d", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.SendEventBean(new SupportBean_S0(3, "y"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 30, "-", "-", "-", "-", false);
	            SendS1Assert(env, 31, "a", "-", "-", "-", true);
	            SendS1Assert(env, 32, "-", "b", "-", "-", true);
	            env.SendEventBean(new SupportBean_S2(3));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterSixPathAndLeftOrLLVRightOrLL : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or p11='b' or context.s0.p00='x') and (p12='c' or p13='d')");
	            RunAssertion(env, milestone, advanced, "(p13='d' or 'c'=p12) and (context.s0.p00='x' or p11='b' or p10='a')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");

	            var pathWhenX = "context.s0.p00=\"x\"";
	            var pathWhenNotX = "not " + pathWhenX;
	            var pathOne = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p12", EQUAL, "c"));
	            var pathTwo = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p13", EQUAL, "d"));
	            var pathThree = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p12", EQUAL, "c"));
	            var pathFour = new SupportFilterPlanPath(pathWhenNotX, MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p13", EQUAL, "d"));
	            var pathFive = new SupportFilterPlanPath(pathWhenX, MakeTriplet("p12", EQUAL, "c"));
	            var pathSix = new SupportFilterPlanPath(pathWhenX, MakeTriplet("p13", EQUAL, "d"));
	            var plan = new SupportFilterPlan(pathOne, pathTwo, pathThree, pathFour, pathFive, pathSix);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", plan);
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p13", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p13", EQUAL)}
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
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p12", EQUAL)},
	                    new FilterItem[]{new FilterItem("p13", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "-", "-", "-", false);
	            SendS1Assert(env, 21, "-", "-", "c", "-", true);
	            SendS1Assert(env, 22, "-", "-", "-", "d", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathAndLeftOrLVRightOrLL : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or context.s0.p00='x') and (p11='c' or p12='d')");
	            RunAssertion(env, milestone, advanced, "(p12='d' or p11='c') and (context.s0.p00='x' or p10='a')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");

	            var pathOne = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", "context.s0.p00=\"x\""), MakeTriplet("p11", EQUAL, "c"));
	            var pathTwo = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a", "context.s0.p00=\"x\""), MakeTriplet("p12", EQUAL, "d"));
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p11", EQUAL)},
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 10, "a", "c", "-", true);
	            SendS1Assert(env, 11, "a", "-", "d", true);
	            SendS1Assert(env, 12, "a", "c", "d", true);
	            SendS1Assert(env, 13, "-", "c", "d", false);
	            env.SendEventBean(new SupportBean_S2(1));

	            env.SendEventBean(new SupportBean_S0(2, "x"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p11", EQUAL)},
	                    new FilterItem[]{new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "c", "-", true);
	            SendS1Assert(env, 21, "-", "-", "d", true);
	            SendS1Assert(env, 22, "-", "-", "-", false);
	            env.SendEventBean(new SupportBean_S2(1));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterFourPathAndWithOrLLOrLLOrVV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or p11='b') and (p12='c' or p13='d') and (s0.p00='x' or s0.p00='y')");
	            RunAssertion(env, milestone, advanced, "(s0.p00='x' or s0.p00='y') and ('d'=p13 or 'c'=p12) and ('b'=p11 or 'a'=p10)");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(null, "s0.p00=\"x\" or s0.p00=\"y\"", MakeABCDCombinationPath()));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", MakeABCDCombinationFilterItems());
	            }
	            SendS1Assert(env, 10, "-", "-", "-", "-", false);
	            SendS1Assert(env, 11, "a", "-", "c", "-", true);

	            env.SendEventBean(new SupportBean_S0(2, "y"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", MakeABCDCombinationFilterItems());
	            }
	            SendS1Assert(env, 20, "-", "b", "c", "-", true);

	            env.SendEventBean(new SupportBean_S0(3, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 30, "a", "-", "c", "-", false);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterFourPathAndWithOrLLOrLLWithV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or p11='b') and (p12='c' or p13='d') and s0.p00='x'");
	            RunAssertion(env, milestone, advanced, "s0.p00='x' and ('d'=p13 or 'c'=p12) and ('b'=p11 or 'a'=p10)");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(null, "s0.p00=\"x\"", MakeABCDCombinationPath()));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", MakeABCDCombinationFilterItems());
	            }
	            SendS1Assert(env, 10, "-", "-", "-", "-", false);
	            SendS1Assert(env, 11, "a", "-", "c", "-", true);

	            env.SendEventBean(new SupportBean_S0(2, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 20, "a", "-", "c", "-", false);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterFourPathAndWithOrLLOrLL : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or p11='b') and (p12='c' or p13='d')");
	            RunAssertion(env, milestone, advanced, "('d'=p13 or 'c'=p12) and ('b'=p11 or 'a'=p10)");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(MakeABCDCombinationPath()));
	            }
	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", MakeABCDCombinationFilterItems());
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

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterThreePathOrWithAndLVAndLVAndLV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10 = 'a' and s0.p00 like '%1%') or (p10 = 'b' and s0.p00 like '%2%') or (p10 = 'c' and s0.p00 like '%3%')");
	            RunAssertion(env, milestone, advanced, "(s0.p00 like '%2%' and p10 = 'b') or (s0.p00 like '%3%' and 'c' = p10) or (p10 = 'a' and s0.p00 like '%1%')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var pathOne = new SupportFilterPlanPath("s0.p00 like \"%1%\"", MakeTriplet("p10", EQUAL, "a"));
	            var pathTwo = new SupportFilterPlanPath("s0.p00 like \"%2%\"", MakeTriplet("p10", EQUAL, "b"));
	            var pathThree = new SupportFilterPlanPath("s0.p00 like \"%3%\"", MakeTriplet("p10", EQUAL, "c"));
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo, pathThree));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 10, "a", false);
	            SendS1Assert(env, 11, "b", false);
	            SendS1Assert(env, 12, "c", false);

	            env.SendEventBean(new SupportBean_S0(2, "1"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 20, "c", false);
	            SendS1Assert(env, 21, "b", false);
	            SendS1Assert(env, 22, "a", true);

	            env.SendEventBean(new SupportBean_S0(3, "2"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 30, "a", false);
	            SendS1Assert(env, 31, "c", false);
	            SendS1Assert(env, 32, "b", true);

	            env.SendEventBean(new SupportBean_S0(4, "3"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 40, "a", false);
	            SendS1Assert(env, 41, "b", false);
	            SendS1Assert(env, 42, "c", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathAndWithOrLVVOrLVOrLV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10 = 'a' or s0.p00 like '%1%' or s0.p00 like '%2%') and " +
	                "(p11 = 'b' or s0.p00 like '%3%') and (p12 = 'c' or s0.p00 like '%4%')");
	            RunAssertion(env, milestone, advanced, "('c' = p12 or s0.p00 like '%4%') and" +
	                "(s0.p00 like '%3%' or p11 = 'b') and" +
	                "(s0.p00 like '%1%' or p10 = 'a' or s0.p00 like '%2%')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean_S1(" + filter + ")];\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var tripletOne = MakeTriplet("p10", EQUAL, "a", "s0.p00 like \"%1%\" or s0.p00 like \"%2%\"");
	            var tripletTwo = MakeTriplet("p11", EQUAL, "b", "s0.p00 like \"%3%\"");
	            var tripletThree = MakeTriplet("p12", EQUAL, "c", "s0.p00 like \"%4%\"");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(new SupportFilterPlanPath(tripletOne, tripletTwo, tripletThree)));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 10, "a", "b", "-", false);
	            SendS1Assert(env, 11, "-", "b", "c", false);
	            SendS1Assert(env, 12, "a", "b", "c", true);

	            env.SendEventBean(new SupportBean_S0(2, "1"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "b", "-", false);
	            SendS1Assert(env, 21, "-", "-", "c", false);
	            SendS1Assert(env, 22, "-", "b", "c", true);

	            env.SendEventBean(new SupportBean_S0(3, "2"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 30, "-", "b", "-", false);
	            SendS1Assert(env, 31, "-", "-", "c", false);
	            SendS1Assert(env, 32, "-", "b", "c", true);

	            env.SendEventBean(new SupportBean_S0(4, "3"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 40, "a", "-", "-", false);
	            SendS1Assert(env, 41, "-", "-", "c", false);
	            SendS1Assert(env, 42, "a", "-", "c", true);

	            env.SendEventBean(new SupportBean_S0(5, "4"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p11", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 50, "a", "-", "-", false);
	            SendS1Assert(env, 51, "-", "b", "-", false);
	            SendS1Assert(env, 52, "a", "b", "-", true);

	            env.SendEventBean(new SupportBean_S0(6, "1234"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 60, "-", "-", "-", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathAndLeftOrLLRightV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10 = 'a' or p11 = 'b') and context.s0.p00 = 'x'");
	            RunAssertion(env, milestone, advanced, "context.s0.p00 = 'x' and (p10 = 'a' or p11 = 'b')");
	            RunAssertion(env, milestone, advanced, "(p10 = 'a' or p11 = 'b') and context.s0.p00 = 'x'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var tripletOne = MakeTriplet("p10", EQUAL, "a");
	            var tripletTwo = MakeTriplet("p11", EQUAL, "b");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(null, "context.s0.p00=\"x\"", new SupportFilterPlanPath(tripletOne), new SupportFilterPlanPath(tripletTwo)));
	            }
	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 10, "a", "b", false);
	            env.SendEventBean(new SupportBean_S2(1));

	            env.SendEventBean(new SupportBean_S0(2, "x"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "-", false);
	            SendS1Assert(env, 21, "a", "-", true);
	            SendS1Assert(env, 22, "-", "b", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathOrLeftOrLVRightOrLV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10 regexp '.*a.*' or context.s0.p00 = 'x') or (p11 regexp '.*b.*' or context.s0.p01 = 'y')");
	            RunAssertion(env, milestone, advanced, "context.s0.p00 = 'x' or context.s0.p01 = 'y' or p10 regexp '.*a.*' or p11 regexp '.*b.*'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S2;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean_S1(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var tripletOne = MakeTripletRebool(".p10 regexp ?", "\".*a.*\"");
	            var tripletTwo = MakeTripletRebool(".p11 regexp ?", "\".*b.*\"");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("context.s0.p00=\"x\" or context.s0.p01=\"y\"", null, new SupportFilterPlanPath(tripletOne), new SupportFilterPlanPath(tripletTwo)));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "-", "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem(".p10 regexp ?", REBOOL)},
	                    new FilterItem[]{new FilterItem(".p11 regexp ?", REBOOL)}
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
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 20, "-", "-", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.SendEventBean(new SupportBean_S0(3, "-", "y"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 30, "-", "-", true);
	            env.SendEventBean(new SupportBean_S2(2));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathOrLeftVRightAndWithLL : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "s0.p00='x' or (p10 = 'a' and p11 regexp '.*b.*')");
	            RunAssertion(env, milestone, advanced, "(p10 = 'a' and p11 regexp '.*b.*') or (s0.p00='x')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
	                      "SupportBean_S1(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var tripletOne = MakeTriplet("p10", EQUAL, "a");
	            var tripletTwo = MakeTripletRebool(".p11 regexp ?", "\".*b.*\"");
	            var path = new SupportFilterPlanPath(tripletOne, tripletTwo);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.p00=\"x\"", null, path));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem(".p11 regexp ?", REBOOL)}
	                });
	            }
	            SendS1Assert(env, 10, "-", "b", false);
	            SendS1Assert(env, 11, "a", "-", false);
	            env.MilestoneInc(milestone);
	            SendS1Assert(env, 12, "a", "globe", true);

	            env.SendEventBean(new SupportBean_S0(2, "x"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 20, "-", "-", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterAndOrUnwinding : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            RunAssertion(env, "a=1 or (b=2 or c=3)", "a=1 or b=2 or c=3");
	            RunAssertion(env, "a=1 and (b=2 and c=3)", "a=1 and b=2 and c=3");

	            RunAssertion(env, "a=1 or (b=2 or (c=3 or d=4))", "a=1 or b=2 or c=3 or d=4");
	            RunAssertion(env, "a=1 or (b=2 or (c=3 or (d=4 or e=5)))", "a=1 or b=2 or c=3 or d=4 or e=5");
	            RunAssertion(env, "a=1 or (b=2 or (c=3 or (d=4 or (e=5 or f=6))))", "a=1 or b=2 or c=3 or d=4 or e=5 or f=6");
	            RunAssertion(env, "a=1 or (b=2 or (c=3 or (d=4 or (e=5 or f=6 or g=7))))", "a=1 or b=2 or c=3 or d=4 or e=5 or f=6 or g=7");
	            RunAssertion(env, "(((a=1 or b=2) or c=3) or d=4 or e=5) or f=6 or g=7", "a=1 or b=2 or c=3 or d=4 or e=5 or f=6 or g=7");

	            RunAssertion(env, "a=1 and (b=2 and (c=3 and d=4))", "a=1 and b=2 and c=3 and d=4");
	            RunAssertion(env, "a=1 and (b=2 and (c=3 and (d=4 and e=5)))", "a=1 and b=2 and c=3 and d=4 and e=5");
	            RunAssertion(env, "a=1 and (b=2 and (c=3 and (d=4 and (e=5 and f=6))))", "a=1 and b=2 and c=3 and d=4 and e=5 and f=6");
	            RunAssertion(env, "a=1 and (b=2 and (c=3 and (d=4 and (e=5 and f=6))))", "a=1 and b=2 and c=3 and d=4 and e=5 and f=6");
	            RunAssertion(env, "a=1 and (b=2 and (c=3 and (d=4 and (e=5 and f=6 and g=7))))", "a=1 and b=2 and c=3 and d=4 and e=5 and f=6 and g=7");
	            RunAssertion(env, "(((a=1 and b=2) and c=3) and d=4 and e=5) and f=6 and g=7", "a=1 and b=2 and c=3 and d=4 and e=5 and f=6 and g=7");

	            RunAssertion(env, "(a=1 and (b=2 and c=3)) or (d=4 or (e=5 or f=6))", "(a=1 and b=2 and c=3) or d=4 or e=5 or f=6");
	            RunAssertion(env, "((a=1 or b=2) or (c=3)) and (d=5 and e=6)", "(a=1 or b=2 or c=3) and d=5 and e=6");
	            RunAssertion(env, "a=1 or b=2 and c=3 or d=4 and e=5", "a=1 or (b=2 and c=3) or (d=4 and e=5)");
	            RunAssertion(env, "((a=1 and b=2 and c=3 and d=4) and e=5) or (f=6 or (g=7 or h=8 or i=9))", "(a=1 and b=2 and c=3 and d=4 and e=5) or f=6 or g=7 or h=8 or i=9");
	        }

	        private void RunAssertion(RegressionEnvironment env, string filter, string expectedText) {
	            var epl = HOOK + "@name('s0') select * from SupportBeanSimpleNumber(" + filter + ")";
	            SupportFilterPlanHook.Reset();
	            env.Compile(epl);
	            var plan = SupportFilterPlanHook.AssertPlanSingleAndReset();
	            var receivedNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(plan.PlanNodes);

	            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured("SupportBeanSimpleNumber");
	            var typesPerStream = new EventType[]{eventType};
	            var typeAliases = new string[]{"sbsn"};
	            var expectedNode = ((EPRuntimeSPI) env.Runtime).ReflectiveCompileSvc.ReflectiveCompileExpression(expectedText, typesPerStream, typeAliases);

	            Assert.IsTrue(ExprNodeUtilityCompare.DeepEquals(expectedNode, receivedNode, true));
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathAndLeftLOrVRightLOrV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "(p10='a' or s0.p00='x') and (p11='b' or s0.p01='y')");
	            RunAssertion(env, milestone, advanced, "(s0.p01='y' or p11='b') and (s0.p00='x' or p10='a')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK  + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
	                      "SupportBean_S1(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var tripletOne = MakeTriplet("p10", EQUAL, "a", "s0.p00=\"x\"");
	            var tripletTwo = MakeTriplet("p11", EQUAL, "b", "s0.p01=\"y\"");
	            var path = new SupportFilterPlanPath(tripletOne, tripletTwo);
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(path));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "-", "-"));
	            env.MilestoneInc(milestone);
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p11", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 10, "-", "b", false);
	            SendS1Assert(env, 11, "a", "-", false);
	            SendS1Assert(env, 12, "a", "b", true);

	            env.SendEventBean(new SupportBean_S0(2, "x", "-"));
	            env.MilestoneInc(milestone);
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p11", EQUAL));
	            }
	            SendS1Assert(env, 21, "a", "-", false);
	            SendS1Assert(env, 20, "-", "b", true);

	            env.SendEventBean(new SupportBean_S0(2, "-", "y"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 30, "-", "b", false);
	            SendS1Assert(env, 31, "a", "-", true);

	            env.SendEventBean(new SupportBean_S0(2, "x", "y"));
	            env.MilestoneInc(milestone);
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 40, "-", "-", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathOrLeftLRightAndLWithV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "p10='a' or (p11='b' and s0.p00='x')");
	            RunAssertion(env, milestone, advanced, "(s0.p00='x' and p11='b') or p10='a'");
	            RunAssertion(env, milestone, advanced, "(p11='b' and s0.p00='x') or p10='a'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
	                      "SupportBean_S1(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var pathOne = MakePathFromSingle("p10", EQUAL, "a");
	            var pathTwo = new SupportFilterPlanPath("s0.p00=\"x\"", MakeTriplet("p11", EQUAL, "b"));
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan(pathOne, pathTwo));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            env.MilestoneInc(milestone);
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL)}
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
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 30, "-", "b", false);
	            SendS1Assert(env, 31, "a", "b", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterTwoPathOrWithLLV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "p10='a' or p11='b' or s0.p00='x'");
	            RunAssertion(env, milestone, advanced, "p11='b' or s0.p00='x' or p10='a'");
	            RunAssertion(env, milestone, advanced, "s0.p00='x' or p11='b' or p10='a'");
	            RunAssertion(env, milestone, advanced, "(s0.p00='x' or p11='b') or p10='a'");
	            RunAssertion(env, milestone, advanced, "s0.p00='x' or (p11='b' or p10='a')");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
	                      "SupportBean_S1(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var pathOne = MakePathFromSingle("p10", EQUAL, "a");
	            var pathTwo = MakePathFromSingle("p11", EQUAL, "b");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.p00=\"x\"", null, pathOne, pathTwo));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 10, "-", "-", true);

	            env.SendEventBean(new SupportBean_S0(2, "y"));
	            env.MilestoneInc(milestone);
	            if (advanced) {
	                AssertFilterSvcByTypeMulti(env, "s0", "SupportBean_S1", new FilterItem[][]{
	                    new FilterItem[]{new FilterItem("p10", EQUAL)},
	                    new FilterItem[]{new FilterItem("p11", EQUAL)}
	                });
	            }
	            SendS1Assert(env, 20, "-", "-", false);
	            SendS1Assert(env, 21, "a", "-", true);

	            env.SendEventBean(new SupportBean_S0(3, "y"));
	            SendS1Assert(env, 30, "-", "-", false);
	            SendS1Assert(env, 31, "-", "b", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathOrWithLVV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, advanced, "p10='a' or s0.p00='x' or s0.p01='y'");
	            RunAssertion(env, advanced, "s0.p00='x' or p10='a' or s0.p01='y'");
	            RunAssertion(env, advanced, "s0.p00='x' or (s0.p01='y' or p10='a')");
	        }

	        private void RunAssertion(RegressionEnvironment env, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> " +
	                      "SupportBean_S1(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            var path = MakePathFromSingle("p10", EQUAL, "a");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean_S1", new SupportFilterPlan("s0.p00=\"x\" or s0.p01=\"y\"", null, path));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x", "-"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 10, "-", true);

	            env.SendEventBean(new SupportBean_S0(2, "-", "y"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean_S1");
	            }
	            SendS1Assert(env, 20, "-", true);

	            env.SendEventBean(new SupportBean_S0(3, "-", "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean_S1", new FilterItem("p10", EQUAL));
	            }
	            SendS1Assert(env, 30, "-", false);
	            SendS1Assert(env, 31, "a", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathNegate1Eq2WithContextFilter : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "select * from SupportBean_S0;\n" +
	                      "create context MyContext start SupportBean_S0(1=2);\n" +
	                      "@name('s0') context MyContext select * from SupportBean;\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(1));
	            SendSBAssert(env, "E1", false);

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOnePathNegate1Eq2WithStage : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = HOOK + "@name('s0') select * from SupportBean(1=2)";
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingle(new SupportFilterPlan(null, "1=2", MakePathsFromEmpty()));
	                AssertFilterSvcNone(env, "s0", "SupportBean");
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

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathNegate1Eq2WithDataflow : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy("@name('flow') create dataflow MyDataFlowOne " +
	                "EventBusSource -> ReceivedStream<SupportBean> { filter : 1 = 2 } " +
	                "DefaultSupportCaptureOp(ReceivedStream) {}");

	            var future = new DefaultSupportCaptureOp<object>(env.Container.LockManager());
	            var options = new EPDataFlowInstantiationOptions()
	                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
	            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
	            df.Start();

	            env.SendEventBean(new SupportBean());

	            try {
	                Thread.Sleep(100);
	            } catch (ThreadInterruptedException e) {
Assert.Fail(e.Message);
	            }
	            Assert.AreEqual(0, future.Current.Length);

	            df.Cancel();
	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.DATAFLOW);
	        }
	    }

	    private class ExprFilterOnePathNegate1Eq2WithContextCategory : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create context MyContext group by theString='abc' and 1=2 as categoryOne from SupportBean;\n" +
	                      "@name('s0') context MyContext select * from SupportBean;\n";
	            var compiled = env.Compile(epl);
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            if (advanced) {
	                SupportMessageAssertUtil.TryInvalidDeploy(env, compiled, "Failed to deploy: Category context 'MyContext' for category 'categoryOne' has evaluated to a condition that cannot become true");
	            }
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.INVALIDITY);
	        }
	    }

	    private class ExprFilterOnePathOrLeftLRightVWithPattern : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "theString = 'abc' or (s0.p00 || s1.p10 || s2[0].p20 || s2[1].p20 = 'QRST')");
	            RunAssertion(env, milestone, advanced, "(s0.p00 || s1.p10 || s2[0].p20 || s2[1].p20 = 'QRST') or theString = 'abc'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> s1=SupportBean_S1 -> [2] s2=SupportBean_S2 -> " +
	                      "SupportBean(" + filter + ")]";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean", new SupportFilterPlan("s0.p00||s1.p10||s2[0].p20||s2[1].p20=\"QRST\"", null, MakePathsFromSingle("theString", EQUAL, "abc")));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "Q"));
	            env.SendEventBean(new SupportBean_S1(2, "R"));
	            env.SendEventBean(new SupportBean_S2(3, "S"));
	            env.SendEventBean(new SupportBean_S2(4, "T"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean");
	            }

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "x", true);

	            env.SendEventBean(new SupportBean_S0(11, "Q"));
	            env.SendEventBean(new SupportBean_S1(12, "-"));
	            env.SendEventBean(new SupportBean_S2(13, "-"));
	            env.SendEventBean(new SupportBean_S2(14, "-"));
	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
	            }

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
	            }
	            SendSBAssert(env, "x", false);
	            SendSBAssert(env, "abc", true);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathAndLeftLRightVWithPattern : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "theString = 'abc' and s0.p00 = 'x'");
	            RunAssertion(env, milestone, advanced, "s0.p00 = 'x' and theString = 'abc'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = HOOK + "@name('s0') select * from pattern[every s0=SupportBean_S0 -> SupportBean(" + filter + ")];\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingleByType("SupportBean", new SupportFilterPlan(null, "s0.p00=\"x\"", MakePathsFromSingle("theString", EQUAL, "abc")));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x"));

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
	            }
	            SendSBAssert(env, "def", false);
	            SendSBAssert(env, "abc", true);

	            env.SendEventBean(new SupportBean_S0(2, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean");
	            }

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "abc", false);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathAndLeftLRightV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, advanced, "theString = 'abc' and context.s0.p00 = 'x'");
	            RunAssertion(env, milestone, advanced, "context.s0.p00 = 'x' and theString = 'abc'");
	            RunAssertion(env, milestone, advanced, "context.s0.p00 = 'x' and theString = 'abc'");
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, bool advanced, string filter) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S1;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingle(new SupportFilterPlan(null, "context.s0.p00=\"x\"", MakePathsFromSingle("theString", EQUAL, "abc")));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            if (advanced) {
	                AssertFilterSvcSingle(env, "s0", "theString", EQUAL);
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", false);

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcSingle(env, "s0", "theString", EQUAL);
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", false);
	            env.SendEventBean(new SupportBean_S1(1));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean");
	            }

	            env.SendEventBean(new SupportBean_S0(2, "-"));
	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "abc", false);
	            SendSBAssert(env, "def", false);

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcNone(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "abc", false);
	            SendSBAssert(env, "def", false);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOnePathOrLeftLRightV : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var advanced = HasFilterIndexPlanAdvanced(env);
	            RunAssertion(env, milestone, "theString = 'abc' or context.s0.p00 = 'x'", advanced);
	            RunAssertion(env, milestone, "context.s0.p00 = 'x' or theString = 'abc'", advanced);
	            RunAssertion(env, milestone, "context.s0.p00 = 'x' or theString = 'abc'", advanced);
	        }

	        private void RunAssertion(RegressionEnvironment env, AtomicLong milestone, string filter, bool advanced) {
	            var epl = "create context MyContext start SupportBean_S0 as s0 end SupportBean_S1;\n" +
	                      HOOK + "@name('s0') context MyContext select * from SupportBean(" + filter + ");\n";
	            SupportFilterPlanHook.Reset();
	            env.CompileDeploy(epl).AddListener("s0");
	            if (advanced) {
	                AssertPlanSingle(new SupportFilterPlan("context.s0.p00=\"x\"", null, MakePathsFromSingle("theString", EQUAL, "abc")));
	            }

	            env.SendEventBean(new SupportBean_S0(1, "x"));
	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", true);

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcEmpty(env, "s0", "SupportBean");
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", true);
	            env.SendEventBean(new SupportBean_S1(1));

	            env.SendEventBean(new SupportBean_S0(2, "-"));
	            if (advanced) {
	                AssertFilterSvcSingle(env, "s0", "theString", EQUAL);
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", false);

	            env.MilestoneInc(milestone);

	            if (advanced) {
	                AssertFilterSvcSingle(env, "s0", "theString", EQUAL);
	            }
	            SendSBAssert(env, "abc", true);
	            SendSBAssert(env, "def", false);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private static SupportFilterPlanPath[] MakeABCDCombinationPath() {
	        var pathOne = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p12", EQUAL, "c"));
	        var pathTwo = new SupportFilterPlanPath(MakeTriplet("p10", EQUAL, "a"), MakeTriplet("p13", EQUAL, "d"));
	        var pathThree = new SupportFilterPlanPath(MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p12", EQUAL, "c"));
	        var pathFour = new SupportFilterPlanPath(MakeTriplet("p11", EQUAL, "b"), MakeTriplet("p13", EQUAL, "d"));
	        return new SupportFilterPlanPath[]{pathOne, pathTwo, pathThree, pathFour};
	    }

	    private static FilterItem[][] MakeABCDCombinationFilterItems() {
	        return new FilterItem[][]{
	            new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p12", EQUAL)},
	            new FilterItem[]{new FilterItem("p10", EQUAL), new FilterItem("p13", EQUAL)},
	            new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p12", EQUAL)},
	            new FilterItem[]{new FilterItem("p11", EQUAL), new FilterItem("p13", EQUAL)}
	        };
	    }

	    private static void SendSBAssert(RegressionEnvironment env, string theString, bool received) {
	        env.SendEventBean(new SupportBean(theString, 0));
	        env.AssertListenerInvokedFlag("s0", received);
	    }

	    private static void SendS1Assert(RegressionEnvironment env, int id, string p10, string p11, string p12, string p13, bool expected) {
	        env.SendEventBean(new SupportBean_S1(id, p10, p11, p12, p13));
	        env.AssertListenerInvokedFlag("s0", expected);
	    }

	    private static void SendS1Assert(RegressionEnvironment env, int id, string p10, string p11, string p12, bool expected) {
	        SendS1Assert(env, id, p10, p11, p12, null, expected);
	    }

	    private static void SendS1Assert(RegressionEnvironment env, int id, string p10, string p11, bool expected) {
	        SendS1Assert(env, id, p10, p11, null, expected);
	    }

	    private static void SendS1Assert(RegressionEnvironment env, int id, string p10, bool expected) {
	        SendS1Assert(env, id, p10, null, expected);
	    }
	}
} // end of namespace
