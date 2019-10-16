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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.expr.filter;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprFilter
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
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
                typeof(SupportBean_StringAlphabetic)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Put("criteria", typeof(bool?));
            configuration.Common.AddEventType("MapEventWithCriteriaBool", dict);

            configuration.Common.AddVariable(
                "myCheckServiceProvider",
                typeof(ExprFilterOptimizable.MyCheckServiceProvider),
                null);
            configuration.Common.AddVariable("var_optimizable_equals", typeof(string), "abc", true);
            configuration.Common.AddVariable("var_optimizable_relop", typeof(int), 10, true);
            configuration.Common.AddVariable("var_optimizable_start", typeof(int), 10, true);
            configuration.Common.AddVariable("var_optimizable_end", typeof(int), 11, true);
            configuration.Common.AddVariable("var_optimizable_array", "int[]", new int?[] {10, 11}, true);
            configuration.Common.AddVariable("var_optimizable_start_string", typeof(string), "c", true);
            configuration.Common.AddVariable("var_optimizable_end_string", typeof(string), "d", true);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

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
                "MyCustomBigDecimalEquals");

            var func = new ConfigurationCompilerPlugInSingleRowFunction();
            func.FunctionClassName = typeof(ExprFilterOptimizable).FullName;
            func.FunctionMethodName = "myCustomOkFunction";
            func.FilterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            func.RethrowExceptions = true;
            func.Name = "MyCustomOkFunction";
            configuration.Compiler.PlugInSingleRowFunctions.Add(func);

            configuration.Compiler.AddPlugInSingleRowFunction(
                "getLocalValue",
                typeof(ExprFilterPlanOneFilterNonNested),
                "GetLocalValue");
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterExpressions()
        {
            RegressionRunner.Run(session, ExprFilterExpressions.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterInAndBetween()
        {
            RegressionRunner.Run(session, ExprFilterInAndBetween.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterOptimizable()
        {
            RegressionRunner.Run(session, ExprFilterOptimizable.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanInRangeAndBetween()
        {
            RegressionRunner.Run(session, ExprFilterPlanInRangeAndBetween.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanNoFilter()
        {
            RegressionRunner.Run(session, ExprFilterPlanNoFilter.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterNestedFourLvl()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterNestedFourLvl.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterNestedThreeLvl()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterNestedThreeLvl.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterNestedTwoLvl()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterNestedTwoLvl.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterNonNested()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterNonNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterTwoPathNested()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterTwoPathNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanOneFilterTwoPathNonNested()
        {
            RegressionRunner.Run(session, ExprFilterPlanOneFilterTwoPathNonNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanThreeFilterIndexReuse()
        {
            RegressionRunner.Run(session, ExprFilterPlanThreeFilterIndexReuse.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterDifferent()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterDifferent.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterIndexReuse()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterIndexReuse.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterIndexWFilterForValueReuse()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterIndexWFilterForValueReuse.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterNestedTwoDiff()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterNestedTwoDiff.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterNestedTwoSame()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterNestedTwoSame.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterSame()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterSame.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterPlanTwoFilterTwoPathNestedSame()
        {
            RegressionRunner.Run(session, ExprFilterPlanTwoFilterTwoPathNestedSame.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterWhereClause()
        {
            RegressionRunner.Run(session, ExprFilterWhereClause.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprFilterWhereClauseNoDataWindowPerformance()
        {
            RegressionRunner.Run(session, new ExprFilterWhereClauseNoDataWindowPerformance());
        }
    }
} // end of namespace