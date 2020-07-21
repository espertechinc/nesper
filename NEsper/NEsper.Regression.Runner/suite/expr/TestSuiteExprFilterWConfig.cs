///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprFilterWConfig
    {
        private const string HOOK = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" + typeof(SupportFilterPlanHook).Name + "')";

        private void RunAssertionFilter(
            FilterIndexPlanning config,
            ICollection<RegressionExecution> executions)
        {
            var session = RegressionRunner.Session();
            Configure(session.Configuration);
            session.Configuration.Compiler.Execution.FilterIndexPlanning = config;
            RegressionRunner.Run(session, executions);
            session.Destroy();
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
                throw new RuntimeException(e);
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

        private Configuration MakeConfig(FilterIndexPlanning setting)
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            foreach (var bean in new[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                configuration.Common.AddEventType(bean);
            }

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
            configuration.Common.AddVariable("var_optimizable_array", "int[]", new int?[] {10, 11}, true);
            configuration.Common.AddVariable("var_optimizable_start_string", typeof(string), "c", true);
            configuration.Common.AddVariable("var_optimizable_end_string", typeof(string), "d", true);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportType(typeof(DefaultSupportCaptureOp));

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
                "myCustomBigDecimalEquals",
                typeof(ExprFilterOptimizable),
                "MyCustomDecimalEquals");

            var func = new ConfigurationCompilerPlugInSingleRowFunction();
            func.FunctionClassName = typeof(ExprFilterOptimizable).FullName;
            func.FunctionMethodName = "myCustomOkFunction";
            func.FilterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            func.RethrowExceptions = true;
            func.Name = "myCustomOkFunction";
            configuration.Compiler.PlugInSingleRowFunctions.Add(func);

            configuration.Compiler.AddPlugInSingleRowFunction("getLocalValue", typeof(ExprFilterPlanOneFilterNonNested), "GetLocalValue");

            configuration.Compiler.Logging.IsEnableFilterPlan = true;
        }

        [Test]
        public void TestExprFilterAdvancedPlanningDisable()
        {
            var none = MakeConfig(FilterIndexPlanning.NONE);
            var basic = MakeConfig(FilterIndexPlanning.BASIC);
            var advanced = MakeConfig(FilterIndexPlanning.ADVANCED);

            // composite-value-expression planning
            var hintValue = "@Hint('filterindex(valuecomposite)')";
            var eplValue = HOOK + "select * from SupportBean(theString = 'a' || 'b')";
            RunAssertionBooleanExpression(none, eplValue, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplValue, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintValue + eplValue, FilterOperator.EQUAL);
            RunAssertionBooleanExpression(advanced, eplValue, FilterOperator.EQUAL);

            // composite-lookup-expression planning
            var hintLookup = "@Hint('filterindex(lkupcomposite)')";
            var eplLookup = HOOK + "select * from SupportBean(theString || 'a' = 'b')";
            RunAssertionBooleanExpression(none, eplLookup, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplLookup, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintLookup + eplLookup, FilterOperator.EQUAL);
            RunAssertionBooleanExpression(advanced, eplLookup, FilterOperator.EQUAL);

            // no reusable-boolean planning
            var hintRebool = "@Hint('filterindex(boolcomposite)')";
            var eplRebool = HOOK + "select * from SupportBean(theString regexp 'a')";
            RunAssertionBooleanExpression(none, eplRebool, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, eplRebool, FilterOperator.BOOLEAN_EXPRESSION);
            RunAssertionBooleanExpression(basic, hintRebool + eplRebool, FilterOperator.REBOOL);
            RunAssertionBooleanExpression(advanced, eplRebool, FilterOperator.REBOOL);

            // conditions
            var hintCondition = "@Hint('filterindex(condition)')";
            var eplContext = "create context MyContext start SupportBean_S0 as s0;\n";
            var eplCondition = HOOK + "context MyContext select * from SupportBean(theString = 'a' or context.s0.p00 = 'x');\n";
            RunAssertionBooleanExpression(none, eplContext + eplCondition, FilterOperator.BOOLEAN_EXPRESSION);
            Assert.AreEqual(2, CompileGetPlan(basic, eplContext + eplCondition).Paths.Length);
            var planBasicWithHint = CompileGetPlan(basic, eplContext + hintCondition + eplCondition);
            Assert.AreEqual(1, planBasicWithHint.Paths.Length);
            Assert.IsNotNull(planBasicWithHint.FilterConfirm);
            var planAdvanced = CompileGetPlan(advanced, eplContext + eplCondition);
            Assert.AreEqual(1, planAdvanced.Paths.Length);
            Assert.IsNotNull(planAdvanced.FilterConfirm);
        }

        [Test]
        public void TestExprFilterExpressions()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterExpressions.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterExpressions.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterExpressions.Executions());
        }

        [Test]
        public void TestExprFilterInAndBetween()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterInAndBetween.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterInAndBetween.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterInAndBetween.Executions());
        }

        [Test]
        public void TestExprFilterLargeThreading()
        {
            var session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.AddEventType(typeof(SupportTradeEvent));
            session.Configuration.Common.Execution.ThreadingProfile = ThreadingProfile.LARGE;
            session.Configuration.Compiler.Logging.IsEnableFilterPlan = true;
            RegressionRunner.Run(session, new ExprFilterLargeThreading());
            session.Destroy();
        }

        [Test]
        public void TestExprFilterOptimizable()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizable.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizable.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizable.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprAdvanced()
        {
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprBasic()
        {
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprNone()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmAdvanced()
        {
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmBasic()
        {
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmNone()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprAdvanced()
        {
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprBasic()
        {
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprNone()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableOrRewrite()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableOrRewrite.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableOrRewrite.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableOrRewrite.Executions());
        }

        [Test]
        public void TestExprFilterOptimizablePerf()
        {
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizablePerf.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizablePerf.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExpr()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableValueLimitedExpr.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableValueLimitedExpr.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprAdvanced()
        {
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprBasic()
        {
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprNone()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterPlanInRangeAndBetween()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanInRangeAndBetween.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanInRangeAndBetween.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanInRangeAndBetween.Executions());
        }

        [Test]
        public void TestExprFilterPlanNoFilter()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanNoFilter.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanNoFilter.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanNoFilter.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedFourLvl()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterNestedFourLvl.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterNestedFourLvl.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterNestedFourLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedThreeLvl()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterNestedThreeLvl.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterNestedThreeLvl.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterNestedThreeLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedTwoLvl()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterNestedTwoLvl.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterNestedTwoLvl.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterNestedTwoLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNonNested()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterNonNested.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterNonNested.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterNonNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterTwoPathNested()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterTwoPathNested.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterTwoPathNested.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterTwoPathNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterTwoPathNonNested()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanOneFilterTwoPathNonNested.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanOneFilterTwoPathNonNested.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanOneFilterTwoPathNonNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanThreeFilterIndexReuse()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanThreeFilterIndexReuse.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanThreeFilterIndexReuse.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanThreeFilterIndexReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterDifferent()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterDifferent.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterDifferent.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterDifferent.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterIndexReuse()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterIndexReuse.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterIndexReuse.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterIndexReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterIndexWFilterForValueReuse()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterNestedTwoDiff()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterNestedTwoSame()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterNestedTwoSame.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterNestedTwoSame.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterNestedTwoSame.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterSame()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterSame.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterSame.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterSame.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterTwoPathNestedSame()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(false));
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(true));
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(false));
        }

        [Test]
        public void TestExprFilterWhereClause()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterWhereClause.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterWhereClause.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterWhereClause.Executions());
        }

        [Test]
        public void TestExprFilterWhereClauseNoDataWindowPerformance()
        {
            RunAssertionFilter(FilterIndexPlanning.NONE, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
            RunAssertionFilter(FilterIndexPlanning.BASIC, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
            RunAssertionFilter(FilterIndexPlanning.ADVANCED, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
        }
    }
} // end of namespace