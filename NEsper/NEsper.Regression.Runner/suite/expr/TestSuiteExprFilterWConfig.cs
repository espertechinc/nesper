///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
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
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprFilterWConfig
    {
        private static readonly string HOOK = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" + typeof(SupportFilterPlanHook).FullName + "')";

        private void RunAssertionFilter<T>(
            ConfigurationCompilerExecution.FilterIndexPlanningEnum config,
            ICollection<T> executions)
            where T : RegressionExecution
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
            var none = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE);
            var basic = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC);
            var advanced = MakeConfig(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED);

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
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterExpressions.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterExpressions.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterExpressions.Executions());
        }

        [Test]
        public void TestExprFilterInAndBetween()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterInAndBetween.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterInAndBetween.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterInAndBetween.Executions());
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
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizable.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizable.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizable.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprAdvanced()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprBasic()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableBooleanLimitedExprNone()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableBooleanLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmAdvanced()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmBasic()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableConditionNegateConfirmNone()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableConditionNegateConfirm.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprAdvanced()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprBasic()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableLookupableLimitedExprNone()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableLookupableLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableOrRewrite()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableOrRewrite.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableOrRewrite.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableOrRewrite.Executions());
        }

        [Test]
        public void TestExprFilterOptimizablePerf()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizablePerf.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizablePerf.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExpr()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableValueLimitedExpr.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableValueLimitedExpr.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprAdvanced()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprBasic()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterOptimizableValueLimitedExprNone()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterOptimizableValueLimitedExpr.Executions());
        }

        [Test]
        public void TestExprFilterPlanInRangeAndBetween()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanInRangeAndBetween.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanInRangeAndBetween.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanInRangeAndBetween.Executions());
        }

        [Test]
        public void TestExprFilterPlanNoFilter()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanNoFilter.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanNoFilter.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanNoFilter.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedFourLvl()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterNestedFourLvl.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterNestedFourLvl.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterNestedFourLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedThreeLvl()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterNestedThreeLvl.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterNestedThreeLvl.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterNestedThreeLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNestedTwoLvl()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterNestedTwoLvl.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterNestedTwoLvl.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterNestedTwoLvl.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterNonNested()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterNonNested.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterNonNested.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterNonNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterTwoPathNested()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterTwoPathNested.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterTwoPathNested.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterTwoPathNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanOneFilterTwoPathNonNested()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanOneFilterTwoPathNonNested.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanOneFilterTwoPathNonNested.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanOneFilterTwoPathNonNested.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanThreeFilterIndexReuse()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanThreeFilterIndexReuse.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanThreeFilterIndexReuse.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanThreeFilterIndexReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterDifferent()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterDifferent.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterDifferent.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterDifferent.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterIndexReuse()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterIndexReuse.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterIndexReuse.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterIndexReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterIndexWFilterForValueReuse()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(true));
            RunAssertionFilter(
                ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED,
                ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterNestedTwoDiff()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterNestedTwoDiff.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterNestedTwoSame()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterNestedTwoSame.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterNestedTwoSame.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterNestedTwoSame.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterSame()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterSame.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterSame.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterSame.Executions(false));
        }

        [Test]
        public void TestExprFilterPlanTwoFilterTwoPathNestedSame()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(false));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(true));
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions(false));
        }

        [Test]
        public void TestExprFilterWhereClause()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterWhereClause.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterWhereClause.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterWhereClause.Executions());
        }

        [Test]
        public void TestExprFilterWhereClauseNoDataWindowPerformance()
        {
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.BASIC, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
            RunAssertionFilter(ConfigurationCompilerExecution.FilterIndexPlanningEnum.ADVANCED, ExprFilterWhereClauseNoDataWindowPerformance.Executions());
        }
    }
} // end of namespace