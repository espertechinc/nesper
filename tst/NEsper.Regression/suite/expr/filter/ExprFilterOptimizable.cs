///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizable
    {
        private static EPLMethodInvocationContext methodInvocationContextFilterOptimized;

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithInAndNotInKeywordMultivalue(execs);
            WithOptimizableMethodInvocationContext(execs);
            WithOptimizableTypeOf(execs);
            WithOptimizableVariableAndSeparateThread(execs);
            WithOptimizableInspectFilter(execs);
            WithOrToInRewrite(execs);
            WithOrContext(execs);
            WithPatternUDFFilterOptimizable(execs);
            WithDeployTimeConstant(execs); // substitution and variables are here
            return execs;
        }

        public static IList<RegressionExecution> WithDeployTimeConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterDeployTimeConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternUDFFilterOptimizable(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterPatternUDFFilterOptimizable());
            return execs;
        }

        public static IList<RegressionExecution> WithOrContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrContext());
            return execs;
        }

        public static IList<RegressionExecution> WithOrToInRewrite(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOrToInRewrite());
            return execs;
        }

        public static IList<RegressionExecution> WithOptimizableInspectFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizableInspectFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithOptimizableVariableAndSeparateThread(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizableVariableAndSeparateThread());
            return execs;
        }

        public static IList<RegressionExecution> WithOptimizableTypeOf(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizableTypeOf());
            return execs;
        }

        public static IList<RegressionExecution> WithOptimizableMethodInvocationContext(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptimizableMethodInvocationContext());
            return execs;
        }

        public static IList<RegressionExecution> WithInAndNotInKeywordMultivalue(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterInAndNotInKeywordMultivalue());
            return execs;
        }

        public class ExprFilterOrContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('ctx') create context MyContext initiated by SupportBean terminated after 24 hours;\n" +
                    "@name('select') context MyContext select * from SupportBean(TheString='A' or IntPrimitive=1)";
                env.CompileDeployAddListenerMileZero(epl, "select");

                env.SendEventBean(new SupportBean("A", 1), nameof(SupportBean));
                env.AssertEventNew("select", @event => { });

                env.UndeployAll();
            }
        }

        private class ExprFilterInAndNotInKeywordMultivalue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                TryInKeyword(env, "Ints", new SupportInKeywordBean(new int[] { 1, 2 }), milestone);
                TryInKeyword(
                    env,
                    "MapOfIntKey",
                    new SupportInKeywordBean(CollectionUtil.TwoEntryMap(1, "x", 2, "y")),
                    milestone);
                TryInKeyword(env, "CollOfInt", new SupportInKeywordBean(Arrays.AsList(1, 2)), milestone);

                TryNotInKeyword(env, "Ints", new SupportInKeywordBean(new int[] { 1, 2 }), milestone);
                TryNotInKeyword(
                    env,
                    "MapOfIntKey",
                    new SupportInKeywordBean(CollectionUtil.TwoEntryMap(1, "x", 2, "y")),
                    milestone);
                TryNotInKeyword(env, "CollOfInt", new SupportInKeywordBean(Arrays.AsList(1, 2)), milestone);

                TryInArrayContextProvided(env, milestone);

                if (HasFilterIndexPlanBasicOrMore(env)) {
                    env.TryInvalidCompile(
                        "select * from pattern[every a=SupportInKeywordBean -> SupportBean(IntPrimitive in (a.Longs))]",
                        "Implicit conversion from datatype 'System.Int64' to 'System.Nullable<System.Int32>' for property 'IntPrimitive' is not allowed (strict filter type coercion)");
                }
            }
        }

        private class ExprFilterOptimizableInspectFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                var milestone = new AtomicLong();
                var path = new RegressionPath();

                epl = "select * from SupportBean(funcOne(TheString) = 0)";
                AssertFilterDeploySingle(
                    env,
                    path,
                    epl,
                    FilterSpecCompilerIndexPlanner.PROPERTY_NAME_BOOLEAN_EXPRESSION,
                    FilterOperator.BOOLEAN_EXPRESSION,
                    milestone);

                epl = "select * from SupportBean(funcOneWDefault(TheString) = 0)";
                AssertFilterDeploySingle(env, path, epl, "funcOneWDefault(TheString)", FilterOperator.EQUAL, milestone);

                epl = "select * from SupportBean(funcTwo(TheString) = 0)";
                AssertFilterDeploySingle(env, path, epl, "funcTwo(TheString)", FilterOperator.EQUAL, milestone);

                epl = "select * from SupportBean(libE1True(TheString))";
                AssertFilterDeploySingle(env, path, epl, "libE1True(TheString)", FilterOperator.EQUAL, milestone);

                epl = "select * from SupportBean(funcTwo( TheString ) > 10)";
                AssertFilterDeploySingle(env, path, epl, "funcTwo(TheString)", FilterOperator.GREATER, milestone);

                epl = "select * from SupportBean(libE1True(TheString))";
                AssertFilterDeploySingle(env, path, epl, "libE1True(TheString)", FilterOperator.EQUAL, milestone);

                epl = "select * from SupportBean(typeof(e) = 'SupportBean') as e";
                AssertFilterDeploySingle(env, path, epl, "typeof(e)", FilterOperator.EQUAL, milestone);

                env.CompileDeploy(
                        "@name('create-expr') @public create expression thesplit {TheString => funcOne(TheString)}",
                        path)
                    .AddListener("create-expr");
                epl = "select * from SupportBean(thesplit(*) = 0)";
                AssertFilterDeploySingle(env, path, epl, "thesplit(*)", FilterOperator.EQUAL, milestone);

                epl = "select * from SupportBean(thesplit(*) > 10)";
                AssertFilterDeploySingle(env, path, epl, "thesplit(*)", FilterOperator.GREATER, milestone);

                epl = "expression housenumber alias for {10} select * from SupportBean(IntPrimitive = housenumber)";
                AssertFilterDeploySingle(env, path, epl, "IntPrimitive", FilterOperator.EQUAL, milestone);

                epl =
                    "expression housenumber alias for {IntPrimitive*10} select * from SupportBean(IntPrimitive = housenumber)";
                AssertFilterDeploySingle(
                    env,
                    path,
                    epl,
                    ".boolean_expression",
                    FilterOperator.BOOLEAN_EXPRESSION,
                    milestone);

                env.UndeployAll();
            }
        }

        private class ExprFilterPatternUDFFilterOptimizable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern[a=SupportBean() -> b=SupportBean(myCustomDecimalEquals(a.DecimalPrimitive, b.DecimalPrimitive))]";
                env.CompileDeploy(epl).AddListener("s0");

                var beanOne = new SupportBean("E1", 0);
                beanOne.DecimalPrimitive = 13m;
                env.SendEventBean(beanOne);

                var beanTwo = new SupportBean("E2", 0);
                beanTwo.DecimalPrimitive = 13m;
                env.SendEventBean(beanTwo);

                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprFilterOrToInRewrite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                // test 'or' rewrite
                var filtersAB = new string[] {
                    "TheString = 'a' or TheString = 'b'",
                    "TheString = 'a' or 'b' = TheString",
                    "'a' = TheString or 'b' = TheString",
                    "'a' = TheString or TheString = 'b'",
                };
                foreach (var filter in filtersAB) {
                    var eplX = "@name('s0') select * from SupportBean(" + filter + ")";
                    env.CompileDeployAddListenerMile(eplX, "s0", milestone.GetAndIncrement());
                    if (HasFilterIndexPlanBasicOrMore(env)) {
                        SupportFilterServiceHelper.AssertFilterSvcSingle(
                            env,
                            "s0",
                            "TheString",
                            FilterOperator.IN_LIST_OF_VALUES);
                    }

                    env.SendEventBean(new SupportBean("a", 0));
                    env.AssertListenerInvoked("s0");
                    env.SendEventBean(new SupportBean("b", 0));
                    env.AssertListenerInvoked("s0");
                    env.SendEventBean(new SupportBean("c", 0));
                    env.AssertListenerNotInvoked("s0");

                    env.UndeployAll();
                }

                var epl =
                    "@name('s0') select * from SupportBean(IntPrimitive = 1 and (TheString='a' or TheString='b'))";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                if (HasFilterIndexPlanBasicOrMore(env)) {
                    SupportFilterServiceHelper.AssertFilterSvcTwo(
                        env,
                        "s0",
                        "IntPrimitive",
                        FilterOperator.EQUAL,
                        "TheString",
                        FilterOperator.IN_LIST_OF_VALUES);
                }

                env.UndeployAll();
            }
        }

        private class ExprFilterDeployTimeConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionEqualsWSubs(env, "select * from SupportBean(TheString=?:p0:string)");
                RunAssertionEqualsWSubs(env, "select * from SupportBean(?:p0:string=TheString)");
                RunAssertionEqualsWVariable(env, "select * from SupportBean(TheString=var_optimizable_equals)");
                RunAssertionEqualsWVariable(env, "select * from SupportBean(var_optimizable_equals=TheString)");
                RunAssertionEqualsWSubsWCoercion(env, "select * from SupportBean(LongPrimitive=?:p0:int)");
                RunAssertionEqualsWSubsWCoercion(env, "select * from SupportBean(?:p0:int=LongPrimitive)");

                if (HasFilterIndexPlanBasicOrMore(env)) {
                    env.TryInvalidCompile(
                        "select * from SupportBean(IntPrimitive=?:p0:long)",
                        "Implicit conversion from datatype 'System.Nullable<System.Int64>' to 'System.Nullable<System.Int32>' for property 'IntPrimitive' is not allowed");
                }

                RunAssertionRelOpWSubs(env, "select * from SupportBean(IntPrimitive>?:p0:int)");
                RunAssertionRelOpWSubs(env, "select * from SupportBean(?:p0:int<IntPrimitive)");
                RunAssertionRelOpWVariable(env, "select * from SupportBean(IntPrimitive>var_optimizable_relop)");
                RunAssertionRelOpWVariable(env, "select * from SupportBean(var_optimizable_relop<IntPrimitive)");

                RunAssertionInWSubs(env, "select * from SupportBean(IntPrimitive in (?:p0:int, ?:p1:int))");
                RunAssertionInWVariable(
                    env,
                    "select * from SupportBean(IntPrimitive in (var_optimizable_start, var_optimizable_end))");

                RunAssertionInWSubsWArray(env, "select * from SupportBean(IntPrimitive in (?:p0:int[primitive]))");
                RunAssertionInWVariableWArray(
                    env,
                    "select * from SupportBean(IntPrimitive in (var_optimizable_array))");

                RunAssertionBetweenWSubsWNumeric(
                    env,
                    "select * from SupportBean(IntPrimitive between ?:p0:int and ?:p1:int)");
                RunAssertionBetweenWVariableWNumeric(
                    env,
                    "select * from SupportBean(IntPrimitive between var_optimizable_start and var_optimizable_end)");

                RunAssertionBetweenWSubsWString(
                    env,
                    "select * from SupportBean(TheString between ?:p0:string and ?:p1:string)");
                RunAssertionBetweenWVariableWString(
                    env,
                    "select * from SupportBean(TheString between var_optimizable_start_string and var_optimizable_end_string)");
            }
        }

        private static void RunAssertionBetweenWSubsWNumeric(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", 10, "p1", 11));
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.RANGE_CLOSED);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void RunAssertionBetweenWVariableWNumeric(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.RANGE_CLOSED);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void RunAssertionBetweenWSubsWString(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", "c", "p1", "d"));
            TryAssertionBetweenDeplotTimeConst(env, epl);
        }

        private static void RunAssertionBetweenWVariableWString(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            TryAssertionBetweenDeplotTimeConst(env, epl);
        }

        private static void TryAssertionBetweenDeplotTimeConst(
            RegressionEnvironment env,
            string epl)
        {
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(env, "s0", "TheString", FilterOperator.RANGE_CLOSED);
            }

            env.SendEventBean(new SupportBean("b", 0));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("c", 0));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("d", 0));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("e", 0));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void RunAssertionInWSubsWArray(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", new int[] { 10, 11 }));
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.IN_LIST_OF_VALUES);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void RunAssertionInWVariableWArray(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.IN_LIST_OF_VALUES);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void TryAssertionWSubsFrom9To12(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean("E1", 9));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("E2", 10));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("E3", 11));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("E1", 12));
            env.AssertListenerNotInvoked("s0");
        }

        private static void RunAssertionInWSubs(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", 10, "p1", 11));
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.IN_LIST_OF_VALUES);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void RunAssertionInWVariable(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(
                    env,
                    "s0",
                    "IntPrimitive",
                    FilterOperator.IN_LIST_OF_VALUES);
            }

            TryAssertionWSubsFrom9To12(env);
            env.UndeployAll();
        }

        private static void RunAssertionRelOpWSubs(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", 10));
            TryAssertionRelOpWDeployTimeConst(env, epl);
        }

        private static void RunAssertionRelOpWVariable(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            TryAssertionRelOpWDeployTimeConst(env, epl);
        }

        private static void TryAssertionRelOpWDeployTimeConst(
            RegressionEnvironment env,
            string epl)
        {
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(env, "s0", "IntPrimitive", FilterOperator.GREATER);
            }

            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("E2", 11));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private static void RunAssertionEqualsWSubs(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", "abc"));
            TryAssertionEqualsWDeployTimeConst(env, epl);
        }

        private static void RunAssertionEqualsWVariable(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy("@name('s0') " + epl).AddListener("s0");
            TryAssertionEqualsWDeployTimeConst(env, epl);
        }

        private static void TryAssertionEqualsWDeployTimeConst(
            RegressionEnvironment env,
            string epl)
        {
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(env, "s0", "TheString", FilterOperator.EQUAL);
            }

            env.SendEventBean(new SupportBean("abc", 0));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("x", 0));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void RunAssertionEqualsWSubsWCoercion(
            RegressionEnvironment env,
            string epl)
        {
            CompileDeployWSubstitution(env, epl, CollectionUtil.BuildMap("p0", 100));
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcSingle(env, "s0", "LongPrimitive", FilterOperator.EQUAL);
            }

            var sb = new SupportBean();
            sb.LongPrimitive = 100;
            env.SendEventBean(sb);
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private static void TryInKeyword(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            TryInKeywordPlain(env, field, prototype, milestone);
            TryInKeywordPattern(env, field, prototype, milestone);
        }

        private static void TryInKeywordPattern(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            var epl =
                "@name('s0') select * from pattern[every a=SupportInKeywordBean -> SupportBean(IntPrimitive in (a." +
                field +
                "))]";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 1, true);
            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 2, true);
            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 3, false);

            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportBean",
                    new FilterItem[][] {
                        new[] { new FilterItem("IntPrimitive", FilterOperator.IN_LIST_OF_VALUES) },
                    });
            }

            env.UndeployAll();
        }

        private static void TryInKeywordPlain(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            var epl = "@name('s0') select * from SupportInKeywordBean#length(2) where 1 in (" + field + ")";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(env.CopyMayFail(prototype));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private static void TryNotInKeyword(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            TryNotInKeywordPlain(env, field, prototype, milestone);
            TryNotInKeywordPattern(env, field, prototype, milestone);
        }

        private static void TryNotInKeywordPlain(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            var epl = "@name('s0') select * from SupportInKeywordBean#length(2) where 1 not in (" + field + ")";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(env.CopyMayFail(prototype));
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void TryNotInKeywordPattern(
            RegressionEnvironment env,
            string field,
            SupportInKeywordBean prototype,
            AtomicLong milestone)
        {
            var epl =
                "@name('s0') select * from pattern[every a=SupportInKeywordBean -> SupportBean(IntPrimitive not in (a." +
                field +
                "))]";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 0, true);
            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 3, true);

            AssertInKeywordReceivedPattern(env, env.CopyMayFail(prototype), 1, false);
            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s0",
                    "SupportBean",
                    new FilterItem[][] {
                        new[] { new FilterItem("IntPrimitive", FilterOperator.NOT_IN_LIST_OF_VALUES) },
                    });
            }

            env.UndeployAll();
        }

        private static void TryInArrayContextProvided(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "create context MyContext initiated by SupportInKeywordBean as mie terminated after 24 hours;\n" +
                      "@name('s1') context MyContext select * from SupportBean#keepall where IntPrimitive in (context.mie.Ints);\n" +
                      "@name('s2') context MyContext select * from SupportBean(IntPrimitive in (context.mie.Ints));\n";
            env.CompileDeploy(epl).AddListener("s1").AddListener("s2");

            env.SendEventBean(new SupportInKeywordBean(new int[] { 1, 2 }));

            env.SendEventBean(new SupportBean("E1", 1));
            env.AssertListenerInvoked("s1");
            env.AssertListenerInvoked("s2");

            env.SendEventBean(new SupportBean("E2", 2));
            env.AssertListenerInvoked("s1");
            env.AssertListenerInvoked("s2");

            env.SendEventBean(new SupportBean("E3", 3));
            env.AssertListenerNotInvoked("s1");
            env.AssertListenerNotInvoked("s2");

            if (HasFilterIndexPlanBasicOrMore(env)) {
                SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
                    env,
                    "s2",
                    "SupportBean",
                    new FilterItem[][] {
                        new[] { new FilterItem("IntPrimitive", FilterOperator.IN_LIST_OF_VALUES) },
                    });
            }

            env.UndeployAll();
        }

        public class ExprFilterOptimizableTypeOf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportOverrideBase(typeof(e) = 'SupportOverrideBase') as e";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                env.SendEventBean(new SupportOverrideBase(""));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportOverrideOne("a", "b"));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        public class ExprFilterOptimizableVariableAndSeparateThread : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.RuntimeSetVariable(null, "myCheckServiceProvider", new MyCheckServiceProvider());

                env.CompileDeploy("@name('s0') select * from SupportBean(myCheckServiceProvider.Check())")
                    .AddListener("s0");
                var latch = new CountDownLatch(1);

                var executorService = Executors.NewSingleThreadExecutor();
                executorService.Submit(
                    () => {
                        env.SendEventBean(new SupportBean());
                        env.AssertListenerInvoked("s0");
                        latch.CountDown();
                    });

                try {
                    Assert.IsTrue(latch.Await(10, TimeUnit.SECONDS));
                }
                catch (ThreadInterruptedException) {
                    Assert.Fail();
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        public class ExprFilterOptimizableMethodInvocationContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                methodInvocationContextFilterOptimized = null;
                env.CompileDeployAddListenerMile(
                    "@name('s0') select * from SupportBean e where myCustomOkFunction(e) = \"OK\"",
                    "s0",
                    0);
                env.SendEventBean(new SupportBean());
                Assert.AreEqual("default", methodInvocationContextFilterOptimized.RuntimeURI);
                Assert.AreEqual("myCustomOkFunction", methodInvocationContextFilterOptimized.FunctionName);
                Assert.IsNull(methodInvocationContextFilterOptimized.StatementUserObject);
                Assert.AreEqual(-1, methodInvocationContextFilterOptimized.ContextPartitionId);
                methodInvocationContextFilterOptimized = null;
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private static void AssertInKeywordReceivedPattern(
            RegressionEnvironment env,
            object @event,
            int intPrimitive,
            bool expected)
        {
            env.SendEventBean(@event);
            env.SendEventBean(new SupportBean(null, intPrimitive));
            env.AssertListenerInvokedFlag("s0", expected);
        }

        private static void AssertFilterDeploySingle(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string expression,
            FilterOperator op,
            AtomicLong milestone)
        {
            env.CompileDeploy("@name('s0')" + epl, path).AddListener("s0").MilestoneInc(milestone);
            env.AssertStatement(
                "s0",
                statement => {
                    var statementSPI = (EPStatementSPI)statement;
                    if (HasFilterIndexPlanBasicOrMore(env)) {
                        var param = SupportFilterServiceHelper.GetFilterSvcSingle(statementSPI);
                        Assert.AreEqual(op, param.Op, "failed for '" + epl + "'");
                        Assert.AreEqual(expression, param.Name);
                    }
                });
            env.UndeployModuleContaining("s0");
        }

        private static void CompileDeployWSubstitution(
            RegressionEnvironment env,
            string epl,
            IDictionary<string, object> @params)
        {
            var compiled = env.Compile("@name('s0') " + epl);
            env.Deploy(
                compiled,
                new DeploymentOptions().WithStatementSubstitutionParameter(
                    new SupportPortableDeploySubstitutionParams(@params).SetStatementParameters));
            env.AddListener("s0");
        }

        public static string MyCustomOkFunction(
            object e,
            EPLMethodInvocationContext ctx)
        {
            methodInvocationContextFilterOptimized = ctx;
            return "OK";
        }

        public static bool MyCustomDecimalEquals(
            decimal first,
            decimal second)
        {
            return first.CompareTo(second) == 0;
        }

        public class MyCheckServiceProvider
        {
            public bool Check()
            {
                return true;
            }
        }
    }
} // end of namespace