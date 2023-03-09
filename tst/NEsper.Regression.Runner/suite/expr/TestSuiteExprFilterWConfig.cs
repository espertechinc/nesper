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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.suite.expr.filter;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class TestSuiteExprFilterWConfig : AbstractTestContainer
    {
        private static readonly string HOOK =
            "@Hook(" +
            "HookType=HookType.INTERNAL_FILTERSPEC, " +
            "Hook='" +
            typeof(SupportFilterPlanHook).FullName +
            "'" +
            ")";

        private static void RunAssertionFilter<T>(
            RegressionSession session,
            ConfigurationCompilerExecution.FilterIndexPlanningEnum config,
            ICollection<T> executions)
            where T : RegressionExecution
        {
            Configure(session.Configuration);
            session.Configuration.Compiler.Execution.FilterIndexPlanning = config;
            RegressionRunner.Run(session, executions);
        }

        private void RunAssertionFilter<T>(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum config,
            ICollection<T> executions)
            where T : RegressionExecution
        {
            using var session = RegressionRunner.Session(Container, true);
            RunAssertionFilter<T>(session, config, executions);
        }

        private void RunAssertionBooleanExpression(
            Configuration configuration,
            string epl,
            FilterOperator expected)
        {
            SupportFilterPlanHook.Reset();
            try {
                EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(configuration));
            }
            catch (EPCompileException e) {
                throw new EPRuntimeException(e);
            }

            FilterSpecParamForge forge = SupportFilterPlanHook.AssertPlanSingleTripletAndReset("SupportBean");
            Assert.AreEqual(expected, forge.FilterOperator);
        }

        private FilterSpecPlanForge CompileGetPlan(
            Configuration configuration,
            string epl)
        {
            SupportFilterPlanHook.Reset();
            EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(configuration));
            return SupportFilterPlanHook.AssertPlanSingleAndReset().Plan;
        }

        private Configuration MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum setting)
        {
            var configuration = SupportConfigFactory.GetConfiguration(Container);
            foreach (var bean in new[] { typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1) }) {
                configuration.Common.AddEventType(bean);
            }

            configuration.Common.AddImportType(typeof(HookType));

            configuration.Compiler.Execution.FilterIndexPlanning = setting;
            configuration.Compiler.Logging.IsEnableFilterPlan = true;
            return configuration;
        }

        protected static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                         typeof(SupportBean),
                         typeof(SupportBeanArrayCollMap),
                         typeof(SupportTradeEvent),
                         typeof(SupportInstanceMethodBean),
                         typeof(SupportRuntimeExBean),
                         typeof(SupportBeanWithEnum),
                         typeof(SupportBeanComplexProps),
                         typeof(SupportMarketDataBean),
                         typeof(SupportBeanNumeric),
                         typeof(SupportBean_S0),
                         typeof(SupportInKeywordBean),
                         typeof(SupportOverrideBase),
                         typeof(SupportOverrideOne),
                         typeof(SupportBean_IntAlphabetic),
                         typeof(SupportBean_StringAlphabetic),
                         typeof(SupportBean_S1),
                         typeof(SupportBean_S2),
                         typeof(SupportBeanSimpleNumber)
                     }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Put("criteria", typeof(bool?));
            configuration.Common.AddEventType("MapEventWithCriteriaBool", dict);

            configuration.Common.AddVariable("myCheckServiceProvider", typeof(ExprFilterOptimizable.MyCheckServiceProvider), null);
            configuration.Common.AddVariable("var_optimizable_equals", typeof(string), "abc", true);
            configuration.Common.AddVariable("var_optimizable_relop", typeof(int), 10, true);
            configuration.Common.AddVariable("var_optimizable_start", typeof(int), 10, true);
            configuration.Common.AddVariable("var_optimizable_end", typeof(int), 11, true);
            configuration.Common.AddVariable("var_optimizable_array", "int[]", new int?[] { 10, 11 }, true);
            configuration.Common.AddVariable("var_optimizable_start_string", typeof(string), "c", true);
            configuration.Common.AddVariable("var_optimizable_end_string", typeof(string), "d", true);

            configuration.Common.AddImportType(typeof(HookType));
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportType(typeof(DefaultSupportCaptureOp));
            configuration.Common.AddImportType(typeof(DefaultSupportCaptureOpForge));
            configuration.Common.AddImportType(typeof(SupportFilterPlanHook));

            configuration.Compiler.AddPlugInSingleRowFunction(
                "libSplit",
                typeof(SupportStaticMethodLib),
                "LibSplit",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "funcOne",
                typeof(SupportStaticMethodLib),
                "LibSplit",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "funcOneWDefault",
                typeof(SupportStaticMethodLib),
                "LibSplit");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "funcTwo",
                typeof(SupportStaticMethodLib),
                "LibSplit",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "libE1True",
                typeof(SupportStaticMethodLib),
                "LibE1True",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "myCustomDecimalEquals",
                typeof(ExprFilterOptimizable),
                "MyCustomDecimalEquals");

            var func = new ConfigurationCompilerPlugInSingleRowFunction();
            func.FunctionClassName = typeof(ExprFilterOptimizable).FullName;
            func.FunctionMethodName = "MyCustomOkFunction";
            func.FilterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            func.RethrowExceptions = true;
            func.Name = "myCustomOkFunction";
            configuration.Compiler.PlugInSingleRowFunctions.Add(func);

            configuration.Compiler.AddPlugInSingleRowFunction("getLocalValue", typeof(ExprFilterPlanOneFilterNonNested), "GetLocalValue");

            configuration.Compiler.Logging.IsEnableFilterPlan = true;
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterAdvancedPlanningDisable()
        {
            var none = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE);
            var basic = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC);
            var advanced = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED);

            // composite-value-expression planning
            var hintValue = "@Hint('filterindex(valuecomposite)')";
            var eplValue = HOOK + "select * from SupportBean(TheString = 'a' || 'b')";
            RunAssertionBooleanExpression(none, eplValue, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplValue, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintValue + eplValue, FilterOperator.EQUAL);
            RunAssertionBooleanExpression(advanced, eplValue, FilterOperator.EQUAL);

            // composite-lookup-expression planning
            var hintLookup = "@Hint('filterindex(lkupcomposite)')";
            var eplLookup = HOOK + "select * from SupportBean(TheString || 'a' = 'b')";
            RunAssertionBooleanExpression(none, eplLookup, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplLookup, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintLookup + eplLookup, FilterOperator.EQUAL);
            RunAssertionBooleanExpression(advanced, eplLookup, FilterOperator.EQUAL);

            // no reusable-boolean planning
            var hintRebool = "@Hint('filterindex(boolcomposite)')";
            var eplRebool = HOOK + "select * from SupportBean(TheString regexp 'a')";
            RunAssertionBooleanExpression(none, eplRebool, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplRebool, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintRebool + eplRebool, FilterOperator.REBOOL);
            RunAssertionBooleanExpression(advanced, eplRebool, FilterOperator.REBOOL);

            // conditions
            var hintCondition = "@Hint('filterindex(conditions)')";
            var eplContext = "create context MyContext start SupportBean_S0 as s0;\n";
            var eplCondition = HOOK + "context MyContext select * from SupportBean(TheString = 'a' or context.s0.P00 = 'x');\n";
            RunAssertionBooleanExpression(none, eplContext + eplCondition, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.AreEqual(2, CompileGetPlan(basic, eplContext + eplCondition).Paths.Length);

            var planBasicWithHint = CompileGetPlan(basic, eplContext + hintCondition + eplCondition);
            Assert.AreEqual(1, planBasicWithHint.Paths.Length);
            Assert.IsNotNull(planBasicWithHint.FilterConfirm);

            var planAdvanced = CompileGetPlan(advanced, eplContext + eplCondition);
            Assert.AreEqual(1, planAdvanced.Paths.Length);
            Assert.IsNotNull(planAdvanced.FilterConfirm);
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterLargeThreading()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.AddEventType(typeof(SupportTradeEvent));
            session.Configuration.Common.Execution.ThreadingProfile = ThreadingProfile.LARGE;
            session.Configuration.Compiler.Logging.IsEnableFilterPlan = true;
            RegressionRunner.Run(session, new ExprFilterLargeThreading());
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanInRangeAndBetween(ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanInRangeAndBetween.Executions());
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanNoFilter(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanNoFilter.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterNestedFourLvl(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterNestedFourLvl.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterNestedThreeLvl(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterNestedThreeLvl.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterNestedTwoLvl(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterNestedTwoLvl.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterNonNested(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterNonNested.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterTwoPathNested(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterTwoPathNested.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanOneFilterTwoPathNonNested(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanOneFilterTwoPathNonNested.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanThreeFilterIndexReuse(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanThreeFilterIndexReuse.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterDifferent(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterDifferent.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        public void TestExprFilterPlanTwoFilterIndexReuse(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterIndexReuse.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterIndexWFilterForValueReuse(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterNestedTwoDiff(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterNestedTwoSame(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterNestedTwoSame.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterSame(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterSame.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, false)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, true)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, false)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterPlanTwoFilterTwoPathNestedSame(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning,
            bool withStats)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(withStats));
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.All)]
        public void TestExprFilterWhereClause(ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterWhereClause.Executions());
        }

        [Test, RunInApplicationDomain]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestCase(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]

        public void TestExprFilterWhereClauseNoDataWindowPerformance(ConfigurationCompilerExecution.FilterIndexPlanningEnum filterIndexPlanning)
        {
            RunAssertionFilter(filterIndexPlanning, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizablePerf
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizablePerf.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]
        public class TestExprFilterOptimizablePerf : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizablePerf(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithTrueDeclaredExpr()
            {
                using (new PerformanceContext()) {
                    RunAssertionFilter(
                        _session,
                        _indexPlanning,
                        ExprFilterOptimizablePerf.WithTrueDeclaredExpr());
                }
            }

            [Test, RunInApplicationDomain]
            public void WithEqualsDeclaredExpr()
            {
                using (new PerformanceContext()) {
                    RunAssertionFilter(
                        _session,
                        _indexPlanning,
                        ExprFilterOptimizablePerf.WithEqualsDeclaredExpr());
                }
            }

            [Test, RunInApplicationDomain]
            public void WithTrueWithFunc()
            {
                using (new PerformanceContext()) {
                    RunAssertionFilter(
                        _session,
                        _indexPlanning,
                        ExprFilterOptimizablePerf.WithTrueWithFunc());
                }
            }

            [Test, RunInApplicationDomain]
            public void WithEqualsWithFunc()
            {
                using (new PerformanceContext()) {
                    RunAssertionFilter(
                        _session,
                        _indexPlanning,
                        ExprFilterOptimizablePerf.WithEqualsWithFunc());
                }
            }

            [Test, RunInApplicationDomain]
            public void WithOr()
            {
                using (new PerformanceContext()) {
                    RunAssertionFilter(
                        _session,
                        _indexPlanning,
                        ExprFilterOptimizablePerf.WithOr());
                }
            }
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizableConditionNegateConfirm
        /// <code>
        /// RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableConditionNegateConfirm.Executions());
        /// RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableConditionNegateConfirm.Executions());
        /// RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableConditionNegateConfirm.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]
        public class TestExprFilterOptimizableConditionNegateConfirm : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizableConditionNegateConfirm(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning) : base(_ => { }) {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithAnyPathCompileMore() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithAnyPathCompileMore());

            [Test, RunInApplicationDomain]
            public void WithEightPathLeftOrLLVRightOrLLV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithEightPathLeftOrLLVRightOrLLV());

            [Test, RunInApplicationDomain]
            public void WithSixPathAndLeftOrLLVRightOrLL() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithSixPathAndLeftOrLLVRightOrLL());

            [Test, RunInApplicationDomain]
            public void WithTwoPathAndLeftOrLVVRightLL() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathAndLeftOrLVVRightLL());

            [Test, RunInApplicationDomain]
            public void WithFourPathAndWithOrLLOrLLOrVV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithFourPathAndWithOrLLOrLLOrVV());

            [Test, RunInApplicationDomain]
            public void WithFourPathAndWithOrLLOrLLWithV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithFourPathAndWithOrLLOrLLWithV());

            [Test, RunInApplicationDomain]
            public void WithFourPathAndWithOrLLOrLL() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithFourPathAndWithOrLLOrLL());

            [Test, RunInApplicationDomain]
            public void WithThreePathOrWithAndLVAndLVAndLV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithThreePathOrWithAndLVAndLVAndLV());

            [Test, RunInApplicationDomain]
            public void WithTwoPathAndLeftOrLVRightOrLL() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathAndLeftOrLVRightOrLL());

            [Test, RunInApplicationDomain]
            public void WithTwoPathAndLeftOrLLRightV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathAndLeftOrLLRightV());

            [Test, RunInApplicationDomain]
            public void WithTwoPathOrLeftOrLVRightOrLV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathOrLeftOrLVRightOrLV());

            [Test, RunInApplicationDomain]
            public void WithTwoPathOrLeftLRightAndLWithV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathOrLeftLRightAndLWithV());

            [Test, RunInApplicationDomain]
            public void WithTwoPathOrWithLLV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithTwoPathOrWithLLV());

            [Test, RunInApplicationDomain]
            public void WithOnePathAndWithOrLVVOrLVOrLV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathAndWithOrLVVOrLVOrLV());

            [Test, RunInApplicationDomain]
            public void WithOnePathOrWithLVV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathOrWithLVV());

            [Test, RunInApplicationDomain]
            public void WithOnePathOrLeftVRightAndWithLL() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathOrLeftVRightAndWithLL());

            [Test, RunInApplicationDomain]
            public void WithOnePathAndLeftLOrVRightLOrV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathAndLeftLOrVRightLOrV());

            [Test, RunInApplicationDomain]
            public void WithOnePathAndLeftLRightVWithPattern() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathAndLeftLRightVWithPattern());

            [Test, RunInApplicationDomain]
            public void WithOnePathAndLeftLRightV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathAndLeftLRightV());

            [Test, RunInApplicationDomain]
            public void WithOnePathOrLeftLRightVWithPattern() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathOrLeftLRightVWithPattern());

            [Test, RunInApplicationDomain]
            public void WithOnePathOrLeftLRightV() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathOrLeftLRightV());

            [Test, RunInApplicationDomain]
            public void WithOnePathNegate1Eq2WithContextCategory() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathNegate1Eq2WithContextCategory());

            [Test, RunInApplicationDomain]
            public void WithOnePathNegate1Eq2WithContextFilter() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathNegate1Eq2WithContextFilter());

            [Test, RunInApplicationDomain]
            public void WithOnePathNegate1Eq2WithStage() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathNegate1Eq2WithStage());

            [Test, RunInApplicationDomain]
            public void WithOnePathNegate1Eq2WithDataflow() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithOnePathNegate1Eq2WithDataflow());

            [Test, RunInApplicationDomain]
            public void WithAndOrUnwinding() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableConditionNegateConfirm.WithAndOrUnwinding());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizableBooleanLimitedExpr
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]
        public class TestExprFilterOptimizableBooleanLimitedExpr : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizableBooleanLimitedExpr(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithDisqualify() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithDisqualify());

            [Test, RunInApplicationDomain]
            public void WithMultiple() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithMultiple());

            [Test, RunInApplicationDomain]
            public void WithWithEquals() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithWithEquals());

            [Test, RunInApplicationDomain]
            public void WithPatternValueWithConst() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithPatternValueWithConst());

            [Test, RunInApplicationDomain]
            public void WithContextValueWithConst() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithContextValueWithConst());

            [Test, RunInApplicationDomain]
            public void WithContextValueDeep() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithContextValueDeep());

            [Test, RunInApplicationDomain]
            public void WithNoValueConcat() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithNoValueConcat());

            [Test, RunInApplicationDomain]
            public void WithNoValueExprRegexpSelf() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithNoValueExprRegexpSelf());

            [Test, RunInApplicationDomain]
            public void WithConstValueRegexpLHS() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithConstValueRegexpLHS());

            [Test]
            public void WithConstValueRegexpRHSPerformance() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithConstValueRegexpRHSPerformance());

            [Test, RunInApplicationDomain]
            public void WithMixedValueRegexpRHS() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithMixedValueRegexpRHS());

            [Test, RunInApplicationDomain]
            public void WithConstValueRegexpRHS() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableBooleanLimitedExpr.WithConstValueRegexpRHS());
        }


        /// <summary>
        /// Auto-test(s): ExprFilterOptimizableValueLimitedExpr
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizableValueLimitedExpr.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]
        public class TestExprFilterOptimizableValueLimitedExpr : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizableValueLimitedExpr(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithOrRewrite() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithOrRewrite());

            [Test, RunInApplicationDomain]
            public void WithInRangeWCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithInRangeWCoercion());

            [Test, RunInApplicationDomain]
            public void WithInSetOfValueWPatternWCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithInSetOfValueWPatternWCoercion());

            [Test, RunInApplicationDomain]
            public void WithDisqualify() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithDisqualify());

            [Test, RunInApplicationDomain]
            public void WithRelOpCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithRelOpCoercion());

            [Test, RunInApplicationDomain]
            public void WithEqualsCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsCoercion());

            [Test, RunInApplicationDomain]
            public void WithEqualsConstantVariable() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsConstantVariable());

            [Test, RunInApplicationDomain]
            public void WithEqualsSubstitutionParams() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsSubstitutionParams());

            [Test, RunInApplicationDomain]
            public void WithEqualsContextWithStart() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsContextWithStart());

            [Test, RunInApplicationDomain]
            public void WithEqualsFromPatternWithDotMethod() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsFromPatternWithDotMethod());

            [Test, RunInApplicationDomain]
            public void WithEqualsFromPatternHalfConstant() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsFromPatternHalfConstant());

            [Test, RunInApplicationDomain]
            public void WithEqualsFromPatternConstant() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsFromPatternConstant());

            [Test, RunInApplicationDomain]
            public void WithEqualsFromPatternMulti() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsFromPatternMulti());

            [Test, RunInApplicationDomain]
            public void WithEqualsFromPatternSingle() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsFromPatternSingle());

            [Test, RunInApplicationDomain]
            public void WithEqualsIsConstant() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableValueLimitedExpr.WithEqualsIsConstant());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizableOrRewrite
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizableOrRewrite.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.All)]
        public class TestExprFilterOptimizableOrRewrite : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizableOrRewrite(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithContextPartitionedInitiated() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithContextPartitionedInitiated());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionedInitiatedSameEvent() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithContextPartitionedInitiatedSameEvent());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionedCategory() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithContextPartitionedCategory());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionedHash() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithContextPartitionedHash());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionedSegmented() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithContextPartitionedSegmented());

            [Test, RunInApplicationDomain]
            public void WithHint() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithHint());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithBooleanExprAnd() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithBooleanExprAnd());

            [Test, RunInApplicationDomain]
            public void WithBooleanExprSimple() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithBooleanExprSimple());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteAndOrMulti() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteAndOrMulti());

            [Test, RunInApplicationDomain]
            public void WithAndRewriteInnerOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithAndRewriteInnerOr());

            [Test, RunInApplicationDomain]
            public void WithAndRewriteNotEqualsWithOrConsolidateSecond() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithAndRewriteNotEqualsWithOrConsolidateSecond());

            [Test, RunInApplicationDomain]
            public void WithAndRewriteNotEqualsConsolidate() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithAndRewriteNotEqualsConsolidate());

            [Test, RunInApplicationDomain]
            public void WithAndRewriteNotEqualsOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithAndRewriteNotEqualsOr());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteEightOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteEightOr());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteFourOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteFourOr());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteThreeWithOverlap() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteThreeWithOverlap());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteWithAnd() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteWithAnd());

            [Test, RunInApplicationDomain]
            public void WithOrRewriteThreeOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithOrRewriteThreeOr());

            [Test, RunInApplicationDomain]
            public void WithTwoOr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableOrRewrite.WithTwoOr());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterInAndBetween
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterInAndBetween.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.All)]
        public class TestExprFilterInAndBetween : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterInAndBetween(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithInMultipleWithBool() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithInMultipleWithBool());

            [Test, RunInApplicationDomain]
            public void WithInMultipleNonMatchingFirst() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithInMultipleNonMatchingFirst());

            [Test, RunInApplicationDomain]
            public void WithReuseNot() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithReuseNot());

            [Test, RunInApplicationDomain]
            public void WithReuse() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithReuse());

            [Test, RunInApplicationDomain]
            public void WithInInvalid() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithInInvalid());

            [Test, RunInApplicationDomain]
            public void WithNotIn() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithNotIn());

            [Test, RunInApplicationDomain]
            public void WithInExpr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithInExpr());

            [Test, RunInApplicationDomain]
            public void WithSimpleIntAndEnumWrite() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithSimpleIntAndEnumWrite());

            [Test, RunInApplicationDomain]
            public void WithInDynamic() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterInAndBetween.WithInDynamic());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterExpressions
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterExpressions.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.Self)]
        public class TestExprFilterExpressions : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterExpressions(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithInstanceMethodWWildcard() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithInstanceMethodWWildcard());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEqualsSemanticExpr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithEqualsSemanticExpr());

            [Test, RunInApplicationDomain]
            public void WithCombinationEqualsOp() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithCombinationEqualsOp());

            [Test, RunInApplicationDomain]
            public void WithNotEqualsOp() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithNotEqualsOp());

            [Test, RunInApplicationDomain]
            public void WithRewriteWhere() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithRewriteWhere());

            [Test, RunInApplicationDomain]
            public void WithExprReversed() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithExprReversed());

            [Test, RunInApplicationDomain]
            public void WithPatternWithExpr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithPatternWithExpr());

            [Test, RunInApplicationDomain]
            public void WithEqualsSemanticFilter() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithEqualsSemanticFilter());

            [Test, RunInApplicationDomain]
            public void WithWithEqualsSameCompare() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithWithEqualsSameCompare());

            [Test, RunInApplicationDomain]
            public void WithStaticFunc() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithStaticFunc());

            [Test, RunInApplicationDomain]
            public void WithPatternFunc() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithPatternFunc());

            [Test, RunInApplicationDomain]
            [Category("Debugging")]
            public void WithPatternFunc3Stream() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithPatternFunc3Stream());

            [Test, RunInApplicationDomain]
            public void WithEnumSyntaxTwo() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithEnumSyntaxTwo());

            [Test, RunInApplicationDomain]
            public void WithEnumSyntaxOne() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithEnumSyntaxOne());

            [Test, RunInApplicationDomain]
            public void WithNullBooleanExpr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithNullBooleanExpr());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpConstantFirst() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithRelationalOpConstantFirst());

            [Test, RunInApplicationDomain]
            public void WithShortCircuitEvalAndOverspecified() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithShortCircuitEvalAndOverspecified());

            [Test, RunInApplicationDomain]
            public void WithPromoteIndexToSetNotIn() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithPromoteIndexToSetNotIn());

            [Test, RunInApplicationDomain]
            public void WithNotEqualsConsolidate() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithNotEqualsConsolidate());

            [Test, RunInApplicationDomain]
            public void WithOverInClause() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithOverInClause());

            [Test, RunInApplicationDomain]
            public void WithInSet() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithInSet());

            [Test, RunInApplicationDomain]
            public void WithNotEqualsNull() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithNotEqualsNull());

            [Test, RunInApplicationDomain]
            public void WithIn3ValuesAndNull() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithIn3ValuesAndNull());

            [Test, RunInApplicationDomain]
            public void WithBooleanExpr() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithBooleanExpr());

            [Test, RunInApplicationDomain]
            public void WithMathExpression() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithMathExpression());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpRange() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithRelationalOpRange());

            [Test, RunInApplicationDomain]
            public void WithConstant() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterExpressions.WithConstant());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizable
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizable.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.Self)]
        public class TestExprFilterOptimizable : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizable(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning) : base(_ => { })
            {
                _indexPlanning = indexPlanning;
            }

            protected override bool UseDefaultRuntime => true;

            [Test, RunInApplicationDomain]
            public void WithDeployTimeConstant() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithDeployTimeConstant());

            [Test, RunInApplicationDomain]
            public void WithPatternUDFFilterOptimizable() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithPatternUDFFilterOptimizable());

            [Test, RunInApplicationDomain]
            public void WithOrContext() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOrContext());

            [Test, RunInApplicationDomain]
            public void WithOrToInRewrite() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOrToInRewrite());

            [Test, RunInApplicationDomain]
            public void WithOptimizableInspectFilter() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOptimizableInspectFilter());

            [Test, RunInApplicationDomain]
            public void WithOptimizableVariableAndSeparateThread() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOptimizableVariableAndSeparateThread());

            [Test, RunInApplicationDomain]
            public void WithOptimizableTypeOf() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOptimizableTypeOf());

            [Test, RunInApplicationDomain]
            public void WithOptimizableMethodInvocationContext() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithOptimizableMethodInvocationContext());

            [Test, RunInApplicationDomain]
            public void WithInAndNotInKeywordMultivalue() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizable.WithInAndNotInKeywordMultivalue());
        }

        /// <summary>
        /// Auto-test(s): ExprFilterOptimizableLookupableLimitedExpr
        /// <code>
        /// RegressionRunner.Run(_session, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        /// </code>
        /// </summary>

        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC)]
        [TestFixture(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED)]
        [Parallelizable(ParallelScope.None)]
        public class TestExprFilterOptimizableLookupableLimitedExpr : AbstractTestBase
        {
            private readonly ConfigurationCompilerExecution.FilterIndexPlanningEnum _indexPlanning;

            public TestExprFilterOptimizableLookupableLimitedExpr(ConfigurationCompilerExecution.FilterIndexPlanningEnum indexPlanning)
                : base(_ => { })
            {
                _indexPlanning = indexPlanning;
            }

            [Test, RunInApplicationDomain]
            public void WithCurrentTimestamp() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithCurrentTimestamp());

            [Test, RunInApplicationDomain]
            public void WithDisqualify() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithDisqualify());

            [Test, RunInApplicationDomain]
            public void WithInRangeWCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithInRangeWCoercion());

            [Test, RunInApplicationDomain]
            public void WithInSetOfValue() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithInSetOfValue());

            [Test, RunInApplicationDomain]
            public void WithEqualsCoercion() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithEqualsCoercion());

            [Test, RunInApplicationDomain]
            public void WithEqualsMultiStmtSharingIndex() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithEqualsMultiStmtSharingIndex());

            [Test, RunInApplicationDomain]
            public void WithEqualsOneStmtWPatternSharingIndex() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithEqualsOneStmtWPatternSharingIndex());

            [Test, RunInApplicationDomain]
            public void WithEqualsOneStmt() => RunAssertionFilter(
                _session,
                _indexPlanning,
                ExprFilterOptimizableLookupableLimitedExpr.WithEqualsOneStmt());
        }
    }
} // end of namespace