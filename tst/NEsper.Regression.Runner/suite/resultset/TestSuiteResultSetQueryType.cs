///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.resultset.querytype;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetQueryType : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                         typeof(SupportBean),
                         typeof(SupportBean_S0),
                         typeof(SupportBean_S1),
                         typeof(SupportMarketDataBean),
                         typeof(SupportCarEvent),
                         typeof(SupportCarInfoEvent),
                         typeof(SupportEventABCProp),
                         typeof(SupportBeanString),
                         typeof(SupportPriceEvent),
                         typeof(SupportMarketDataIDBean),
                         typeof(SupportBean_A),
                         typeof(SupportBean_B),
                         typeof(SupportEventWithIntArray),
                         typeof(SupportThreeArrayEvent)
                     }
                    ) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction(
                "myfunc",
                typeof(ResultSetQueryTypeRollupGroupingFuncs.GroupingSupportFunc),
                "Myfunc");
            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "concatstring",
                typeof(SupportConcatWManagedAggregationFunctionForge));
            var mfAggConfig = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new[] { "sc" },
                typeof(SupportAggMFMultiRTForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(mfAggConfig);
            configuration.Common.AddVariable("MyVar", typeof(string), "");
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeGroupByReclaimMicrosecondResolution()
        {
            RegressionRunner.Run(_session, new ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(5000));
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerEventPerformance()
        {
            RegressionRunner.Run(_session, new ResultSetQueryTypeRowPerEventPerformance());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRowForAll
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRowForAll : AbstractTestBase
        {
            public TestResultSetQueryTypeRowForAll() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowForAllStaticMethodDoubleNested() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithStaticMethodDoubleNested());

            [Test, RunInApplicationDomain]
            public void WithRowForAllNamedWindowWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithNamedWindowWindow());

            [Test, RunInApplicationDomain]
            public void WithSelectAvgStdGroupByUni() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithSelectAvgStdGroupByUni());

            [Test, RunInApplicationDomain]
            public void WithSelectAvgExprStdGroupBy() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithSelectAvgExprStdGroupBy());

            [Test, RunInApplicationDomain]
            public void WithSelectExprGroupWin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithSelectExprGroupWin());

            [Test, RunInApplicationDomain]
            public void WithSelectStarStdGroupBy() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithSelectStarStdGroupBy());

            [Test, RunInApplicationDomain]
            public void WithAvgPerSym() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithAvgPerSym());

            [Test, RunInApplicationDomain]
            public void WithSumJoin() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSumJoin());

            [Test, RunInApplicationDomain]
            public void WithSumOneView() =>
                RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSumOneView());

            [Test, RunInApplicationDomain]
            public void WithAllMinMaxWindowed() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithAllMinMaxWindowed());

            [Test, RunInApplicationDomain]
            public void WithAllWWindowAgg() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithAllWWindowAgg());

            [Test, RunInApplicationDomain]
            public void WithAllSumMinMax() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAll.WithAllSumMinMax());

            [Test, RunInApplicationDomain]
            public void WithAllSimple() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithAllSimple());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRollupDimensionality
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRollupDimensionality : AbstractTestBase
        {
            public TestResultSetQueryTypeRollupDimensionality() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRollupMultikeyWArrayGroupingSet() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithRollupMultikeyWArrayGroupingSet());

            [Test, RunInApplicationDomain]
            public void WithRollupMultikeyWArray() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithRollupMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowCube2Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithNamedWindowCube2Dim());

            [Test, RunInApplicationDomain]
            public void WithOnSelect() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithOnSelect());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionAlsoRollup() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithContextPartitionAlsoRollup());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenTerminated() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithOutputWhenTerminated());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithUnboundCube4Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundCube4Dim());

            [Test, RunInApplicationDomain]
            public void WithBoundGroupingSet2LevelTopAndDetail() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundGroupingSet2LevelTopAndDetail());

            [Test, RunInApplicationDomain]
            public void WithBoundGroupingSet2LevelNoTopNoDetail() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundGroupingSet2LevelNoTopNoDetail());

            [Test, RunInApplicationDomain]
            public void WithBoundCube3Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundCube3Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundGroupingSet2LevelUnenclosed() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundGroupingSet2LevelUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithUnboundCubeUnenclosed() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundCubeUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollupUnenclosed() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollupUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithGroupByWithComputation() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithGroupByWithComputation());

            [Test, RunInApplicationDomain]
            public void WithNonBoxedTypeWithRollup() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithNonBoxedTypeWithRollup());

            [Test, RunInApplicationDomain]
            public void WithMixedAccessAggregation() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithMixedAccessAggregation());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup3Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollup3Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup2DimBatchWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollup2DimBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup1Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollup1Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup2Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollup2Dim());

            [Test, RunInApplicationDomain]
            public void WithBoundRollup2Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundRollup2Dim());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeLocalGroupBy
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeLocalGroupBy : AbstractTestBase
        {
            public TestResultSetQueryTypeLocalGroupBy() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLocalMultikeyWArray() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggAdditionalAndPlugin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggAdditionalAndPlugin());

            [Test]
            public void WithLocalEnumMethods() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalEnumMethods());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedOrderBy() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedOrderBy());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedOnSelect() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedOnSelect());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedRowRemove() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedRowRemove());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedRowRemove() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedRowRemove());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSameKey() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSameKey());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedSameKey() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedSameKey());

            [Test, RunInApplicationDomain]
            public void WithAggregateFullyVersusNotFullyAgg() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithAggregateFullyVersusNotFullyAgg());

            [Test, RunInApplicationDomain]
            public void WithLocalInvalid() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalInvalid());

            [Test, RunInApplicationDomain]
            public void WithLocalPlanning() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalPlanning());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelNoDefaultLvl() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelNoDefaultLvl());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelAccess() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelAccess());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSolutionPattern() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSolutionPattern());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelMethod() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelMethod());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSimple() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSimple());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedThreeLevelWTop() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedThreeLevelWTop());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedUnidirectionalJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedUnidirectionalJoin());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedHaving());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedColNameRendering() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedColNameRendering());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedParenSODA() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedParenSODA());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggIterator() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggIterator());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggEvent() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggEvent());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggSQLStandard() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggSQLStandard());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedSumSimple() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedSumSimple());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeAggregateGrouped
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeAggregateGrouped.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeAggregateGrouped : AbstractTestBase
        {
            public TestResultSetQueryTypeAggregateGrouped() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCriteriaByDotMethod() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithCriteriaByDotMethod());

            [Test, RunInApplicationDomain]
            public void WithIterateUnbound() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithIterateUnbound());

            [Test, RunInApplicationDomain]
            public void WithUnaggregatedHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithUnaggregatedHaving());

            [Test, RunInApplicationDomain]
            public void WithWildcard() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithWildcard());

            [Test, RunInApplicationDomain]
            public void WithAggregationOverGroupedProps() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithAggregationOverGroupedProps());

            [Test, RunInApplicationDomain]
            public void WithSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithSumOneView());

            [Test, RunInApplicationDomain]
            public void WithSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithSumJoin());

            [Test, RunInApplicationDomain]
            public void WithInsertInto() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithInsertInto());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArray() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGrouped.WithMultikeyWArray());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeAggregateGroupedHaving
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeAggregateGroupedHaving.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeAggregateGroupedHaving : AbstractTestBase
        {
            public TestResultSetQueryTypeAggregateGroupedHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupByHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGroupedHaving.WithGroupByHaving());

            [Test, RunInApplicationDomain]
            public void WithSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGroupedHaving.WithSumOneView());

            [Test, RunInApplicationDomain]
            public void WithSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeAggregateGroupedHaving.WithSumJoin());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeHaving
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeHaving.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeHaving : AbstractTestBase
        {
            public TestResultSetQueryTypeHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithHavingWildcardSelect() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithHavingWildcardSelect());

            [Test, RunInApplicationDomain]
            public void WithStatementOM() => RegressionRunner.Run(_session, ResultSetQueryTypeHaving.WithStatementOM());

            [Test, RunInApplicationDomain]
            public void WithStatement() => RegressionRunner.Run(_session, ResultSetQueryTypeHaving.WithStatement());

            [Test, RunInApplicationDomain]
            public void WithStatementJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithStatementJoin());

            [Test, RunInApplicationDomain]
            public void WithSumHavingNoAggregatedProp() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithSumHavingNoAggregatedProp());

            [Test, RunInApplicationDomain]
            public void WithNoAggregationJoinHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithNoAggregationJoinHaving());

            [Test, RunInApplicationDomain]
            public void WithNoAggregationJoinWhere() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithNoAggregationJoinWhere());

            [Test, RunInApplicationDomain]
            public void WithSubstreamSelectHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithSubstreamSelectHaving());

            [Test, RunInApplicationDomain]
            public void WithHavingSum() => RegressionRunner.Run(_session, ResultSetQueryTypeHaving.WithHavingSum());

            [Test, RunInApplicationDomain]
            public void WithHavingSumIStream() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeHaving.WithHavingSumIStream());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeEnumerator
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeEnumerator.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeEnumerator : AbstractTestBase
        {
            public TestResultSetQueryTypeEnumerator() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPatternNoWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithPatternNoWindow());

            [Test, RunInApplicationDomain]
            public void WithPatternWithWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithPatternWithWindow());

            [Test, RunInApplicationDomain]
            public void WithOrderByWildcard() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithOrderByWildcard());

            [Test, RunInApplicationDomain]
            public void WithOrderByProps() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithOrderByProps());

            [Test, RunInApplicationDomain]
            public void WithFilter() => RegressionRunner.Run(_session, ResultSetQueryTypeEnumerator.WithFilter());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupOrdered() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerGroupOrdered());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroup() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerGroupHaving());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupComplex() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerGroupComplex());

            [Test, RunInApplicationDomain]
            public void WithAggregateGroupedOrdered() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithAggregateGroupedOrdered());

            [Test, RunInApplicationDomain]
            public void WithAggregateGrouped() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithAggregateGrouped());

            [Test, RunInApplicationDomain]
            public void WithAggregateGroupedHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithAggregateGroupedHaving());

            [Test, RunInApplicationDomain]
            public void WithRowPerEvent() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerEvent());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventOrdered() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerEventOrdered());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowPerEventHaving());

            [Test, RunInApplicationDomain]
            public void WithRowForAll() => RegressionRunner.Run(_session, ResultSetQueryTypeEnumerator.WithRowForAll());

            [Test, RunInApplicationDomain]
            public void WithRowForAllHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeEnumerator.WithRowForAllHaving());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRollupGroupingFuncs
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRollupGroupingFuncs.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRollupGroupingFuncs : AbstractTestBase
        {
            public TestResultSetQueryTypeRollupGroupingFuncs() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSampleCarEventAndGroupingFunc() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupGroupingFuncs.WithDocSampleCarEventAndGroupingFunc());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupGroupingFuncs.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithFAFCarEventAndGroupingFunc() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupGroupingFuncs.WithFAFCarEventAndGroupingFunc());

            [Test, RunInApplicationDomain]
            public void WithGroupingFuncExpressionUse() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupGroupingFuncs.WithGroupingFuncExpressionUse());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRollupHavingAndOrderBy
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRollupHavingAndOrderBy.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRollupHavingAndOrderBy : AbstractTestBase
        {
            public TestResultSetQueryTypeRollupHavingAndOrderBy() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupHavingAndOrderBy.WithHaving());

            [Test, RunInApplicationDomain]
            public void WithIteratorWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupHavingAndOrderBy.WithIteratorWindow());

            [Test, RunInApplicationDomain]
            public void WithOrderByTwoCriteriaAsc() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupHavingAndOrderBy.WithOrderByTwoCriteriaAsc());

            [Test, RunInApplicationDomain]
            public void WithUnidirectional() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupHavingAndOrderBy.WithUnidirectional());

            [Test, RunInApplicationDomain]
            public void WithOrderByOneCriteriaDesc() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupHavingAndOrderBy.WithOrderByOneCriteriaDesc());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRowForAllHaving
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRowForAllHaving.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRowForAllHaving : AbstractTestBase
        {
            public TestResultSetQueryTypeRowForAllHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowForAllWHavingSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAllHaving.WithRowForAllWHavingSumOneView());

            [Test, RunInApplicationDomain]
            public void WithRowForAllWHavingSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAllHaving.WithRowForAllWHavingSumJoin());

            [Test, RunInApplicationDomain]
            public void WithAvgRowForAllWHavingGroupWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowForAllHaving.WithAvgRowForAllWHavingGroupWindow());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRowPerEvent
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRowPerEvent.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRowPerEvent : AbstractTestBase
        {
            public TestResultSetQueryTypeRowPerEvent() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowPerEventSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithRowPerEventSumOneView());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithRowPerEventSumJoin());

            [Test, RunInApplicationDomain]
            public void WithAggregatedSelectTriggerEvent() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithAggregatedSelectTriggerEvent());

            [Test, RunInApplicationDomain]
            public void WithAggregatedSelectUnaggregatedHaving() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithAggregatedSelectUnaggregatedHaving());

            [Test, RunInApplicationDomain]
            public void WithSumAvgWithWhere() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithSumAvgWithWhere());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventDistinct() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithRowPerEventDistinct());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventDistinctNullable() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerEvent.WithRowPerEventDistinctNullable());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRowPerGroup
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRowPerGroup.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRowPerGroup : AbstractTestBase
        {
            public TestResultSetQueryTypeRowPerGroup() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupSimple() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGroupSimple());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGroupSumOneView());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGroupSumJoin());

            [Test, RunInApplicationDomain]
            public void WithCriteriaByDotMethod() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithCriteriaByDotMethod());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowDelete() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithNamedWindowDelete());

            [Test, RunInApplicationDomain]
            public void WithUnboundStreamUnlimitedKey() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithUnboundStreamUnlimitedKey());

            [Test, RunInApplicationDomain]
            public void WithAggregateGroupedProps() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithAggregateGroupedProps());

            [Test, RunInApplicationDomain]
            public void WithAggregateGroupedPropsPerGroup() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithAggregateGroupedPropsPerGroup());

            [Test, RunInApplicationDomain]
            public void WithAggregationOverGroupedProps() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithAggregationOverGroupedProps());

            [Test, RunInApplicationDomain]
            public void WithUniqueInBatch() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithUniqueInBatch());

            [Test, RunInApplicationDomain]
            public void WithSelectAvgExprGroupBy() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithSelectAvgExprGroupBy());

            [Test, RunInApplicationDomain]
            public void WithUnboundStreamIterate() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithUnboundStreamIterate());

            [Test, RunInApplicationDomain]
            public void WithReclaimSideBySide() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithReclaimSideBySide());

            [Test, RunInApplicationDomain]
            public void WithRowPerGrpMultikeyWArray() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGrpMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithRowPerGrpMultikeyWReclaim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGrpMultikeyWReclaim());

            [Test, RunInApplicationDomain]
            public void WithRowPerGrpNullGroupKey() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroup.WithRowPerGrpNullGroupKey());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeRowPerGroupHaving
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeRowPerGroupHaving.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeRowPerGroupHaving : AbstractTestBase
        {
            public TestResultSetQueryTypeRowPerGroupHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithHavingCount() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroupHaving.WithHavingCount());

            [Test, RunInApplicationDomain]
            public void WithSumJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroupHaving.WithSumJoin());

            [Test, RunInApplicationDomain]
            public void WithSumOneView() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroupHaving.WithSumOneView());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupBatch() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroupHaving.WithRowPerGroupBatch());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupDefinedExpr() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRowPerGroupHaving.WithRowPerGroupDefinedExpr());
        }

        /// <summary>
        /// Auto-test(s): ResultSetQueryTypeWTimeBatch
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetQueryTypeWTimeBatch.Executions());
        /// </code>
        /// </summary>
        public class TestResultSetQueryTypeWTimeBatch : AbstractTestBase
        {
            public TestResultSetQueryTypeWTimeBatch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowForAllNoJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowForAllNoJoin());

            [Test, RunInApplicationDomain]
            public void WithRowForAllJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowForAllJoin());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventNoJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowPerEventNoJoin());

            [Test, RunInApplicationDomain]
            public void WithRowPerEventJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowPerEventJoin());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupNoJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowPerGroupNoJoin());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithRowPerGroupJoin());

            [Test, RunInApplicationDomain]
            public void WithAggrGroupedNoJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithAggrGroupedNoJoin());

            [Test, RunInApplicationDomain]
            public void WithAggrGroupedJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeWTimeBatch.WithAggrGroupedJoin());
        }
    }
} // end of namespace