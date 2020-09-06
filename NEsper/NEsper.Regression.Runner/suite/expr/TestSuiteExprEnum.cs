///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.enummethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.lrreport;
using com.espertech.esper.regressionlib.support.sales;
using com.espertech.esper.regressionrun.Runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprEnum
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
                typeof(SupportBean_ST0_Container),
                typeof(SupportBean),
                typeof(SupportBean_ST0_Container),
                typeof(SupportCollection),
                typeof(PersonSales),
                typeof(SupportBean_A),
                typeof(SupportBean_ST0),
                typeof(SupportSelectorWithListEvent),
                typeof(SupportEnumTwoEvent),
                typeof(SupportSelectorEvent),
                typeof(SupportContainerEvent),
                typeof(Item),
                typeof(LocationReport),
                typeof(Zone),
                typeof(SupportBeanComplexProps),
                typeof(SupportEventWithLongArray),
                typeof(SupportContainerLevelEvent),
                typeof(SupportSelectorWithListEvent),
                typeof(SupportBean_Container),
                typeof(SupportContainerLevel1Event),
                typeof(BookDesc),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportBean_ST0_Container));
            configuration.Common.AddImportType(typeof(LocationReportFactory));
            configuration.Common.AddImportType(typeof(SupportCollection));
            configuration.Common.AddImportType(typeof(ZoneFactory));
            configuration.Compiler.Expression.UdfCache = false;

            var configurationCompiler = configuration.Compiler;
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleList", typeof(SupportBean_ST0_Container), "MakeSampleList");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArray", typeof(SupportBean_ST0_Container), "MakeSampleArray");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleListString", typeof(SupportCollection), "MakeSampleListString");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArrayString", typeof(SupportCollection), "MakeSampleArrayString");
            configurationCompiler.AddPlugInSingleRowFunction("convertToArray", typeof(SupportSelectorWithListEvent), "ConvertToArray");
            configurationCompiler.AddPlugInSingleRowFunction("extractAfterUnderscore", typeof(ExprEnumGroupBy), "ExtractAfterUnderscore");
            configurationCompiler.AddPlugInSingleRowFunction("extractNum", typeof(ExprEnumMinMax.MyService), "ExtractNum");
            configurationCompiler.AddPlugInSingleRowFunction("extractDecimal", typeof(ExprEnumMinMax.MyService), "ExtractDecimal");
            configurationCompiler.AddPlugInSingleRowFunction("inrect", typeof(LRUtil), "Inrect");
            configurationCompiler.AddPlugInSingleRowFunction("distance", typeof(LRUtil), "Distance");
            configurationCompiler.AddPlugInSingleRowFunction("getZoneNames", typeof(Zone), "GetZoneNames");
            configurationCompiler.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container), "MakeTest");
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumAggregate()
        {
            RegressionRunner.Run(session, ExprEnumAggregate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumAllOfAnyOf()
        {
            RegressionRunner.Run(session, ExprEnumAllOfAnyOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumArrayOf()
        {
            RegressionRunner.Run(session, ExprEnumArrayOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumAverage()
        {
            RegressionRunner.Run(session, ExprEnumAverage.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumChained()
        {
            RegressionRunner.Run(session, new ExprEnumChained());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumCountOf()
        {
            RegressionRunner.Run(session, ExprEnumCountOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumFirstLastOf()
        {
            RegressionRunner.Run(session, ExprEnumFirstLastOf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumInvalid()
        {
            RegressionRunner.Run(session, new ExprEnumInvalid());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumMinMaxBy()
        {
            RegressionRunner.Run(session, ExprEnumMinMaxBy.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumMostLeastFrequent()
        {
            RegressionRunner.Run(session, ExprEnumMostLeastFrequent.Executions());
        }

        [Test]
        public void TestExprEnumNamedWindowPerformance()
        {
            RegressionRunner.Run(session, new ExprEnumNamedWindowPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumNested()
        {
            RegressionRunner.Run(session, ExprEnumNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumNestedPerformance()
        {
            RegressionRunner.Run(session, new ExprEnumNestedPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumReverse()
        {
            RegressionRunner.Run(session, ExprEnumReverse.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumSelectFrom()
        {
            RegressionRunner.Run(session, ExprEnumSelectFrom.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumSequenceEqual()
        {
            RegressionRunner.Run(session, ExprEnumSequenceEqual.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumTakeAndTakeLast()
        {
            RegressionRunner.Run(session, ExprEnumTakeAndTakeLast.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumTakeWhileAndWhileLast()
        {
            RegressionRunner.Run(session, ExprEnumTakeWhileAndWhileLast.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestExprEnumWhere()
        {
            RegressionRunner.Run(session, ExprEnumWhere.Executions());
        }

        /// <summary>
        /// Auto-test(s): ExprEnumExceptIntersectUnion
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumExceptIntersectUnion : AbstractTestBase
        {
            public TestExprEnumExceptIntersectUnion() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUnionWhere() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithUnionWhere());

            [Test, RunInApplicationDomain]
            public void WithSetLogicWithEvents() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithSetLogicWithEvents());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithInheritance() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithInheritance());

            [Test, RunInApplicationDomain]
            public void WithSetLogicWithScalar() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithSetLogicWithScalar());

            [Test, RunInApplicationDomain]
            public void WithSetLogicWithContained() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithSetLogicWithContained());

            [Test, RunInApplicationDomain]
            public void WithStringArrayIntersection() => RegressionRunner.Run(_session, ExprEnumExceptIntersectUnion.WithStringArrayIntersection());
        }

        /// <summary>
        /// Auto-test(s): ExprEnumDistinct
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumDistinct.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumDistinct : AbstractTestBase
        {
            public TestExprEnumDistinct() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithScalarMultikeyWArray() => RegressionRunner.Run(_session, ExprEnumDistinct.WithScalarMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithEventsMultikeyWArray() => RegressionRunner.Run(_session, ExprEnumDistinct.WithEventsMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithScalar() => RegressionRunner.Run(_session, ExprEnumDistinct.WithScalar());

            [Test, RunInApplicationDomain]
            public void WithEvents() => RegressionRunner.Run(_session, ExprEnumDistinct.WithEvents());
        }

        /// <summary>
        /// Auto-test(s): ExprEnumDataSources
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumDataSources.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumDataSources : AbstractTestBase
        {
            public TestExprEnumDataSources() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCast() => RegressionRunner.Run(_session, ExprEnumDataSources.WithCast());

            [Test, RunInApplicationDomain]
            public void WithMatchRecognizeMeasures() => RegressionRunner.Run(_session, ExprEnumDataSources.WithMatchRecognizeMeasures());

            [Test, RunInApplicationDomain]
            public void WithMatchRecognizeDefine() => RegressionRunner.Run(_session, ExprEnumDataSources.WithMatchRecognizeDefine());

            [Test, RunInApplicationDomain]
            public void WithTableRow() => RegressionRunner.Run(_session, ExprEnumDataSources.WithTableRow());

            [Test, RunInApplicationDomain]
            public void WithVariable() => RegressionRunner.Run(_session, ExprEnumDataSources.WithVariable());

            [Test, RunInApplicationDomain]
            public void WithPatternFilter() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPatternFilter());

            [Test, RunInApplicationDomain]
            public void WithPatternInsertIntoAtEventBean() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPatternInsertIntoAtEventBean());

            [Test, RunInApplicationDomain]
            public void WithPropertyInsertIntoAtEventBean() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPropertyInsertIntoAtEventBean());

            [Test]
            public void WithPropertySchema() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPropertySchema());

            [Test, RunInApplicationDomain]
            public void WithUDFStaticMethod() => RegressionRunner.Run(_session, ExprEnumDataSources.WithUDFStaticMethod());

            [Test, RunInApplicationDomain]
            public void WithPrevFuncs() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPrevFuncs());

            [Test, RunInApplicationDomain]
            public void WithAccessAggregation() => RegressionRunner.Run(_session, ExprEnumDataSources.WithAccessAggregation());

            [Test, RunInApplicationDomain]
            public void WithSubselect() => RegressionRunner.Run(_session, ExprEnumDataSources.WithSubselect());

            [Test, RunInApplicationDomain]
            public void WithNamedWindow() => RegressionRunner.Run(_session, ExprEnumDataSources.WithNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithPrevWindowSorted() => RegressionRunner.Run(_session, ExprEnumDataSources.WithPrevWindowSorted());

            [Test, RunInApplicationDomain]
            public void WithJoin() => RegressionRunner.Run(_session, ExprEnumDataSources.WithJoin());

            [Test, RunInApplicationDomain]
            public void WithSortedMaxMinBy() => RegressionRunner.Run(_session, ExprEnumDataSources.WithSortedMaxMinBy());

            [Test, RunInApplicationDomain]
            public void WithEnumObject() => RegressionRunner.Run(_session, ExprEnumDataSources.WithEnumObject());

            [Test, RunInApplicationDomain]
            public void WithSubstitutionParameter() => RegressionRunner.Run(_session, ExprEnumDataSources.WithSubstitutionParameter());

            [Test, RunInApplicationDomain]
            public void WithProperty() => RegressionRunner.Run(_session, ExprEnumDataSources.WithProperty());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumDocSamples
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumDocSamples.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumDocSamples : AbstractTestBase
        {
            public TestExprEnumDocSamples() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithDeclared() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithDeclared());

            [Test, RunInApplicationDomain]
            public void WithScalarArray() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithScalarArray());

            [Test, RunInApplicationDomain]
            public void WithUDFSingleRow() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithUDFSingleRow());

            [Test, RunInApplicationDomain]
            public void WithProperties() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithProperties());

            [Test, RunInApplicationDomain]
            public void WithPrevWindow() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithPrevWindow());

            [Test, RunInApplicationDomain]
            public void WithAccessAggWindow() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithAccessAggWindow());

            [Test, RunInApplicationDomain]
            public void WithNamedWindow() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithHowToUse() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithHowToUse());

            [Test, RunInApplicationDomain]
            public void WithExpressions() => RegressionRunner.Run(_session, ExprEnumDocSamples.WithExpressions());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumGroupBy
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumGroupBy.Executions());
        /// </code>
        /// </summary>
        public class TestExprEnumGroupBy : AbstractTestBase
        {
            public TestExprEnumGroupBy() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithTwoParamScalar() => RegressionRunner.Run(_session, ExprEnumGroupBy.WithTwoParamScalar());

            [Test, RunInApplicationDomain]
            public void WithTwoParamEvent() => RegressionRunner.Run(_session, ExprEnumGroupBy.WithTwoParamEvent());

            [Test, RunInApplicationDomain]
            public void WithOneParamScalar() => RegressionRunner.Run(_session, ExprEnumGroupBy.WithOneParamScalar());

            [Test, RunInApplicationDomain]
            public void WithOneParamEvent() => RegressionRunner.Run(_session, ExprEnumGroupBy.WithOneParamEvent());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumSumOf
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumSumOf.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumSumOf : AbstractTestBase
        {
            public TestExprEnumSumOf() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSumArray() => RegressionRunner.Run(_session, ExprEnumSumOf.WithSumArray());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprEnumSumOf.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSumScalarStringValue() => RegressionRunner.Run(_session, ExprEnumSumOf.WithSumScalarStringValue());

            [Test, RunInApplicationDomain]
            public void WithSumScalar() => RegressionRunner.Run(_session, ExprEnumSumOf.WithSumScalar());

            [Test, RunInApplicationDomain]
            public void WithSumEventsPlus() => RegressionRunner.Run(_session, ExprEnumSumOf.WithSumEventsPlus());

            [Test, RunInApplicationDomain]
            public void WithSumEvents() => RegressionRunner.Run(_session, ExprEnumSumOf.WithSumEvents());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumToMap
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumToMap.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumToMap : AbstractTestBase
        {
            public TestExprEnumToMap() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprEnumToMap.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithScalar() => RegressionRunner.Run(_session, ExprEnumToMap.WithScalar());

            [Test, RunInApplicationDomain]
            public void WithEvent() => RegressionRunner.Run(_session, ExprEnumToMap.WithEvent());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumOrderBy
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumOrderBy.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumOrderBy : AbstractTestBase
        {
            public TestExprEnumOrderBy() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprEnumOrderBy.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithOrderByScalarWithParam() => RegressionRunner.Run(_session, ExprEnumOrderBy.WithOrderByScalarWithParam());

            [Test, RunInApplicationDomain]
            public void WithOrderByScalar() => RegressionRunner.Run(_session, ExprEnumOrderBy.WithOrderByScalar());

            [Test, RunInApplicationDomain]
            public void WithOrderByEventsPlus() => RegressionRunner.Run(_session, ExprEnumOrderBy.WithOrderByEventsPlus());

            [Test, RunInApplicationDomain]
            public void WithOrderByEvents() => RegressionRunner.Run(_session, ExprEnumOrderBy.WithOrderByEvents());
        }
        
        /// <summary>
        /// Auto-test(s): ExprEnumMinMax
        /// <code>
        /// RegressionRunner.Run(_session, ExprEnumMinMax.Executions());
        /// </code>
        /// </summary>

        public class TestExprEnumMinMax : AbstractTestBase
        {
            public TestExprEnumMinMax() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprEnumMinMax.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithMinMaxScalarChain() => RegressionRunner.Run(_session, ExprEnumMinMax.WithMinMaxScalarChain());

            [Test, RunInApplicationDomain]
            public void WithMinMaxScalarWithPredicate() => RegressionRunner.Run(_session, ExprEnumMinMax.WithMinMaxScalarWithPredicate());

            [Test, RunInApplicationDomain]
            public void WithMinMaxScalar() => RegressionRunner.Run(_session, ExprEnumMinMax.WithMinMaxScalar());

            [Test, RunInApplicationDomain]
            public void WithMinMaxEvents() => RegressionRunner.Run(_session, ExprEnumMinMax.WithMinMaxEvents());
        }
    }
} // end of namespace