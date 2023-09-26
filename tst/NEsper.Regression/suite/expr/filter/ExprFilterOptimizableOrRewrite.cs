///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework; // assertSame

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableOrRewrite
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithTwoOr(execs);
            WithOrRewriteThreeOr(execs);
            WithOrRewriteWithAnd(execs);
            WithOrRewriteThreeWithOverlap(execs);
            WithOrRewriteFourOr(execs);
            WithOrRewriteEightOr(execs);
            WithAndRewriteNotEqualsOr(execs);
            WithAndRewriteNotEqualsConsolidate(execs);
            WithAndRewriteNotEqualsWithOrConsolidateSecond(execs);
            WithAndRewriteInnerOr(execs);
            WithOrRewriteAndOrMulti(execs);
            WithBooleanExprSimple(execs);
            WithBooleanExprAnd(execs);
            WithSubquery(execs);
            WithHint(execs);
            WithContextPartitionedSegmented(execs);
            WithContextPartitionedHash(execs);
            WithContextPartitionedCategory(execs);
            WithContextPartitionedInitiatedSameEvent(execs);
            WithContextPartitionedInitiated(execs);
            WithAndRewriteNotEqualsLimitedExpr(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAndRewriteNotEqualsLimitedExpr(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteAndRewriteNotEqualsLimitedExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionedInitiated(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteContextPartitionedInitiated());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionedInitiatedSameEvent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteContextPartitionedInitiatedSameEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionedCategory(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteContextPartitionedCategory());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionedHash(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteContextPartitionedHash());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionedSegmented(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteContextPartitionedSegmented());
            return execs;
        }

        public static IList<RegressionExecution> WithHint(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteHint());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExprAnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteBooleanExprAnd());
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExprSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteBooleanExprSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteAndOrMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteAndOrMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithAndRewriteInnerOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteAndRewriteInnerOr());
            return execs;
        }

        public static IList<RegressionExecution> WithAndRewriteNotEqualsWithOrConsolidateSecond(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond());
            return execs;
        }

        public static IList<RegressionExecution> WithAndRewriteNotEqualsConsolidate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteAndRewriteNotEqualsConsolidate());
            return execs;
        }

        public static IList<RegressionExecution> WithAndRewriteNotEqualsOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteAndRewriteNotEqualsOr());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteEightOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteEightOr());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteFourOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteFourOr());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteThreeWithOverlap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteThreeWithOverlap());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteWithAnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteWithAnd());
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewriteThreeOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteOrRewriteThreeOr());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrRewriteTwoOr());
            return execs;
        }

        private class ExprFilterOrRewriteAndRewriteNotEqualsLimitedExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBean(cast(intPrimitive, String) != '321' and (cast(intPrimitive, String) != '123'))";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, 123, false);
                SendAssert(env, 321, false);
                SendAssert(env, 13, true);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                int intPrimitive,
                bool expected)
            {
                env.SendEventBean(new SupportBean("", intPrimitive));
                env.AssertListenerInvokedFlag("s0", expected);
            }
        }

        public class ExprFilterOrRewriteHint : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Hint('MAX_FILTER_WIDTH=0') @name('s0') select * from SupportBean_IntAlphabetic((b=1 or c=1) and (d=1 or e=1))";
                env.CompileDeployAddListenerMile(epl, "s0", 0);
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    ".boolean_expression",
                    FilterOperator.BOOLEAN_EXPRESSION);
                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl =
                    "@name('s0') select (select * from SupportBean_IntAlphabetic(a=1 or b=1)#keepall) as c0 from SupportBean";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                var iaOne = IntEvent(1, 1);
                env.SendEventBean(iaOne);
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "c0", iaOne);

                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteContextPartitionedCategory : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('ctx') create context MyContext \n" +
                          "  group a=1 or b=1 as g1,\n" +
                          "  group c=1 as g1\n" +
                          "  from SupportBean_IntAlphabetic;" +
                          "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(d=1 or e=1)";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendAssertEvents(
                    env,
                    new object[] { IntEvent(1, 0, 0, 0, 1), IntEvent(0, 1, 0, 1, 0), IntEvent(0, 0, 1, 1, 1) },
                    new object[] { IntEvent(0, 0, 0, 1, 0), IntEvent(1, 0, 0, 0, 0), IntEvent(0, 0, 1, 0, 0) }
                );

                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteContextPartitionedHash : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext " +
                          "coalesce by consistent_hash_crc32(a) from SupportBean_IntAlphabetic(b=1) granularity 16 preallocate;" +
                          "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(c=1 or d=1)";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendAssertEvents(
                    env,
                    new object[] { IntEvent(100, 1, 0, 1), IntEvent(100, 1, 1, 0) },
                    new object[] { IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0) }
                );
                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteContextPartitionedSegmented : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext partition by a from SupportBean_IntAlphabetic(b=1 or c=1);" +
                          "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(d=1)";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                SendAssertEvents(
                    env,
                    new object[] { IntEvent(100, 1, 0, 1), IntEvent(100, 0, 1, 1) },
                    new object[] { IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0) }
                );
                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteBooleanExprAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filters = new string[] {
                    "(a='a' or a like 'A%') and (b='b' or b like 'B%')",
                };
                foreach (var filter in filters) {
                    var epl = "@name('s0') select * from SupportBean_StringAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasic(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_StringAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL)
                                },
                                new FilterItem[]
                                    { new FilterItem("a", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem },
                                new FilterItem[]
                                    { new FilterItem("b", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem },
                                new FilterItem[] { FilterItem.BoolExprFilterItem },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] {
                            StringEvent("a", "b"), StringEvent("A1", "b"), StringEvent("a", "B1"),
                            StringEvent("A1", "B1")
                        },
                        new object[] {
                            StringEvent("x", "b"), StringEvent("a", "x"), StringEvent("A1", "C"), StringEvent("C", "B1")
                        }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteBooleanExprSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filters = new string[] {
                    "a like 'a%' and (b='b' or c='c')",
                };
                foreach (var filter in filters) {
                    var epl = "@name('s0') select * from SupportBean_StringAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasic(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_StringAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[]
                                    { new FilterItem("b", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem },
                                new FilterItem[]
                                    { new FilterItem("c", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] { StringEvent("a1", "b", null), StringEvent("a1", null, "c") },
                        new object[]
                            { StringEvent("x", "b", null), StringEvent("a1", null, null), StringEvent("a1", null, "x") }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filters = new string[] {
                    "a!=1 and a!=2 and ((a!=3 and a!=4) or (a!=5 and a!=6))",
                };
                foreach (var filter in filters) {
                    var epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_IntAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    FilterItem.BoolExprFilterItem
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    FilterItem.BoolExprFilterItem
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] { IntEvent(3), IntEvent(4), IntEvent(0) },
                        new object[] { IntEvent(2), IntEvent(1) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteAndRewriteNotEqualsConsolidate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filters = new string[] {
                    "a!=1 and a!=2 and (a!=3 or a!=4)",
                };
                foreach (var filter in filters) {
                    var epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_IntAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    new FilterItem("a", FilterOperator.NOT_EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    new FilterItem("a", FilterOperator.NOT_EQUAL)
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] { IntEvent(3), IntEvent(4), IntEvent(0) },
                        new object[] { IntEvent(2), IntEvent(1) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteAndRewriteNotEqualsOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filters = new string[] {
                    "a!=1 and a!=2 and (b=1 or c=1)",
                };
                foreach (var filter in filters) {
                    var epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_IntAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    new FilterItem("b", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES),
                                    new FilterItem("c", FilterOperator.EQUAL)
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] { IntEvent(3, 1, 0), IntEvent(3, 0, 1), IntEvent(0, 1, 0) },
                        new object[] { IntEvent(2, 0, 0), IntEvent(1, 0, 0), IntEvent(3, 0, 0) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteAndRewriteInnerOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "theString='a' and (intPrimitive=1 or longPrimitive=10)",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("theString", FilterOperator.EQUAL),
                                    new FilterItem("intPrimitive", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("theString", FilterOperator.EQUAL),
                                    new FilterItem("longPrimitive", FilterOperator.EQUAL)
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[] { MakeEvent("a", 1, 0), MakeEvent("a", 0, 10), MakeEvent("a", 1, 10) },
                        new SupportBean[] { MakeEvent("x", 0, 0), MakeEvent("a", 2, 20), MakeEvent("x", 1, 10) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteOrRewriteAndOrMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "a=1 and (b=1 or c=1) and (d=1 or e=1)",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean_IntAlphabetic",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.EQUAL),
                                    new FilterItem("b", FilterOperator.EQUAL), new FilterItem("d", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.EQUAL),
                                    new FilterItem("c", FilterOperator.EQUAL), new FilterItem("d", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.EQUAL),
                                    new FilterItem("c", FilterOperator.EQUAL), new FilterItem("e", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("a", FilterOperator.EQUAL),
                                    new FilterItem("b", FilterOperator.EQUAL), new FilterItem("e", FilterOperator.EQUAL)
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new object[] {
                            IntEvent(1, 1, 0, 1, 0), IntEvent(1, 0, 1, 0, 1), IntEvent(1, 1, 0, 0, 1),
                            IntEvent(1, 0, 1, 1, 0)
                        },
                        new object[] {
                            IntEvent(1, 0, 0, 1, 0), IntEvent(1, 0, 0, 1, 0), IntEvent(1, 1, 1, 0, 0),
                            IntEvent(0, 1, 1, 1, 1)
                        }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteOrRewriteEightOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "theString = 'a' or intPrimitive=1 or longPrimitive=10 or doublePrimitive=100 or boolPrimitive=true or " +
                    "intBoxed=2 or longBoxed=20 or doubleBoxed=200",
                    "longBoxed=20 or theString = 'a' or boolPrimitive=true or intBoxed=2 or longPrimitive=10 or doublePrimitive=100 or " +
                    "intPrimitive=1 or doubleBoxed=200",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("intPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("longPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("doublePrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("boolPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("intBoxed", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("longBoxed", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("doubleBoxed", FilterOperator.EQUAL) },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[] {
                            MakeEvent("a", 1, 10, 100, true, 2, 20, 200), MakeEvent("a", 0, 0, 0, true, 0, 0, 0),
                            MakeEvent("a", 0, 0, 0, true, 0, 20, 0), MakeEvent("x", 0, 0, 100, false, 0, 0, 0),
                            MakeEvent("x", 1, 0, 0, false, 0, 0, 200), MakeEvent("x", 0, 0, 0, false, 0, 0, 200),
                        },
                        new SupportBean[] { MakeEvent("x", 0, 0, 0, false, 0, 0, 0) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteOrRewriteFourOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "theString = 'a' or intPrimitive=1 or longPrimitive=10 or doublePrimitive=100",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("intPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("longPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("doublePrimitive", FilterOperator.EQUAL) },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[] {
                            MakeEvent("a", 1, 10, 100), MakeEvent("x", 0, 0, 100), MakeEvent("x", 0, 10, 100),
                            MakeEvent("a", 0, 0, 0)
                        },
                        new SupportBean[] { MakeEvent("x", 0, 0, 0) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteContextPartitionedInitiated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('ctx') create context MyContext initiated by SupportBean(theString='A' or intPrimitive=1) terminated after 24 hours;\n" +
                    "@name('s0') context MyContext select * from SupportBean;\n";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                env.SendEventBean(new SupportBean("A", 1));
                env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteContextPartitionedInitiatedSameEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext initiated by SupportBean terminated after 24 hours;" +
                          "@name('s0') context MyContext select * from SupportBean(theString='A' or intPrimitive=1)";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                env.SendEventBean(new SupportBean("A", 1));
                env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        public class ExprFilterOrRewriteOrRewriteThreeOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "theString = 'a' or intPrimitive = 1 or longPrimitive = 2",
                    "2 = longPrimitive or 1 = intPrimitive or theString = 'a'"
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] { new FilterItem("intPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("longPrimitive", FilterOperator.EQUAL) },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[]
                            { MakeEvent("a", 0, 0), MakeEvent("b", 1, 0), MakeEvent("c", 0, 2), MakeEvent("c", 0, 2) },
                        new SupportBean[] { MakeEvent("v", 0, 0), MakeEvent("c", 2, 1) }
                    );

                    env.UndeployAll();
                }
            }
        }

        private static void SendAssertEvents(
            RegressionEnvironment env,
            object[] matches,
            object[] nonMatches)
        {
            env.ListenerReset("s0");
            foreach (var match in matches) {
                env.SendEventBean(match);
                env.AssertListener(
                    "s0",
                    listener => Assert.AreSame(match, listener.AssertOneGetNewAndReset().Underlying));
            }

            env.ListenerReset("s0");
            foreach (var nonMatch in nonMatches) {
                env.SendEventBean(nonMatch);
                env.AssertListenerNotInvoked("s0");
            }
        }

        public class ExprFilterOrRewriteOrRewriteThreeWithOverlap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "theString = 'a' or theString = 'b' or intPrimitive=1",
                    "intPrimitive = 1 or theString = 'b' or theString = 'a'",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("intPrimitive", FilterOperator.EQUAL) },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[] { MakeEvent("a", 1), MakeEvent("b", 0), MakeEvent("x", 1) },
                        new SupportBean[] { MakeEvent("x", 0) }
                    );
                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteTwoOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // test 'or' rewrite
                var filtersAB = new string[] {
                    "select * from SupportBean(theString = 'a' or intPrimitive = 1)",
                    "select * from SupportBean(theString = 'a' or 1 = intPrimitive)",
                    "select * from SupportBean('a' = theString or 1 = intPrimitive)",
                    "select * from SupportBean('a' = theString or intPrimitive = 1)",
                };

                foreach (var filter in filtersAB) {
                    env.CompileDeployAddListenerMile("@name('s0')" + filter, "s0", milestone.GetAndIncrement());

                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] { new FilterItem("intPrimitive", FilterOperator.EQUAL) },
                                new FilterItem[] { new FilterItem("theString", FilterOperator.EQUAL) },
                            });
                    }

                    env.SendEventBean(new SupportBean("a", 0));
                    env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());
                    env.SendEventBean(new SupportBean("b", 1));
                    env.AssertListener("s0", _ => _.AssertOneGetNewAndReset());
                    env.SendEventBean(new SupportBean("c", 0));
                    env.AssertListenerNotInvoked("s0");

                    env.UndeployAll();
                }
            }
        }

        public class ExprFilterOrRewriteOrRewriteWithAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var filtersAB = new string[] {
                    "(theString = 'a' and intPrimitive = 1) or (theString = 'b' and intPrimitive = 2)",
                    "(intPrimitive = 1 and theString = 'a') or (intPrimitive = 2 and theString = 'b')",
                    "(theString = 'b' and intPrimitive = 2) or (theString = 'a' and intPrimitive = 1)",
                };
                foreach (var filter in filtersAB) {
                    var epl = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                    if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                            env,
                            "s0",
                            "SupportBean",
                            new FilterItem[][] {
                                new FilterItem[] {
                                    new FilterItem("theString", FilterOperator.EQUAL),
                                    new FilterItem("intPrimitive", FilterOperator.EQUAL)
                                },
                                new FilterItem[] {
                                    new FilterItem("theString", FilterOperator.EQUAL),
                                    new FilterItem("intPrimitive", FilterOperator.EQUAL)
                                },
                            });
                    }

                    SendAssertEvents(
                        env,
                        new SupportBean[] { MakeEvent("a", 1), MakeEvent("b", 2) },
                        new SupportBean[] { MakeEvent("x", 0), MakeEvent("a", 0), MakeEvent("a", 2), MakeEvent("b", 1) }
                    );
                    env.UndeployAll();
                }
            }
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive)
        {
            return MakeEvent(theString, intPrimitive, 0L);
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
        }

        private static SupportBean_IntAlphabetic IntEvent(int a)
        {
            return new SupportBean_IntAlphabetic(a);
        }

        private static SupportBean_IntAlphabetic IntEvent(
            int a,
            int b)
        {
            return new SupportBean_IntAlphabetic(a, b);
        }

        private static SupportBean_IntAlphabetic IntEvent(
            int a,
            int b,
            int c,
            int d)
        {
            return new SupportBean_IntAlphabetic(a, b, c, d);
        }

        private static SupportBean_StringAlphabetic StringEvent(
            string a,
            string b)
        {
            return new SupportBean_StringAlphabetic(a, b);
        }

        private static SupportBean_StringAlphabetic StringEvent(
            string a,
            string b,
            string c)
        {
            return new SupportBean_StringAlphabetic(a, b, c);
        }

        private static SupportBean_IntAlphabetic IntEvent(
            int a,
            int b,
            int c)
        {
            return new SupportBean_IntAlphabetic(a, b, c);
        }

        private static SupportBean_IntAlphabetic IntEvent(
            int a,
            int b,
            int c,
            int d,
            int e)
        {
            return new SupportBean_IntAlphabetic(a, b, c, d, e);
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            var @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            @event.DoublePrimitive = doublePrimitive;
            return @event;
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive,
            bool boolPrimitive,
            int intBoxed,
            long longBoxed,
            double doubleBoxed)
        {
            var @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            @event.DoublePrimitive = doublePrimitive;
            @event.BoolPrimitive = boolPrimitive;
            @event.LongBoxed = longBoxed;
            @event.DoubleBoxed = doubleBoxed;
            @event.IntBoxed = intBoxed;
            return @event;
        }
    }
} // end of namespace