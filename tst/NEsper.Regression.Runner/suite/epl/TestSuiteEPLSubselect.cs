///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.subselect;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestSuiteEPLSubselect : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_S3),
                typeof(SupportBean_S4),
                typeof(SupportValueEvent),
                typeof(SupportIdAndValueEvent),
                typeof(SupportBeanArrayCollMap),
                typeof(SupportSensorEvent),
                typeof(SupportBeanRange),
                typeof(SupportSimpleBeanOne),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1),
                typeof(SupportBean_ST2),
                typeof(SupportTradeEventTwo),
                typeof(SupportMaxAmountEvent),
                typeof(SupportMarketDataBean),
                typeof(SupportEventWithIntArray),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            configuration.Compiler.AddPlugInSingleRowFunction("supportSingleRowFunction", typeof(EPLSubselectWithinPattern), "SupportSingleRowFunction");
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectUnfiltered
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectUnfiltered.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectUnfiltered : AbstractTestBase
        {
            public TestEPLSubselectUnfiltered() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidSubselect() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithInvalidSubselect());

            [Test, RunInApplicationDomain]
            public void WithJoinUnfiltered() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithJoinUnfiltered());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseReturningTrue() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithWhereClauseReturningTrue());

            [Test, RunInApplicationDomain]
            public void WithTwoSubqSelect() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithTwoSubqSelect());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredStreamPriorCompile() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredStreamPriorCompile());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredStreamPriorOM() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredStreamPriorOM());

            [Test, RunInApplicationDomain]
            public void WithCustomFunction() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithCustomFunction());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseWithExpression() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithWhereClauseWithExpression());

            [Test, RunInApplicationDomain]
            public void WithFilterInside() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithFilterInside());

            [Test, RunInApplicationDomain]
            public void WithComputedResult() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithComputedResult());

            [Test, RunInApplicationDomain]
            public void WithSelfSubselect() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithSelfSubselect());

            [Test, RunInApplicationDomain]
            public void WithStartStopStatement() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithStartStopStatement());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredLastEvent() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredLastEvent());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredNoAs() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredNoAs());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredWithAsWithinSubselect() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredWithAsWithinSubselect());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredAsAfterSubselect() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredAsAfterSubselect());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredLengthWindow() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredLengthWindow());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredUnlimitedStream() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredUnlimitedStream());

            [Test, RunInApplicationDomain]
            public void WithUnfilteredExpression() => RegressionRunner.Run(_session, EPLSubselectUnfiltered.WithUnfilteredExpression());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectExists
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectExists.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectExists : AbstractTestBase
        {
            public TestEPLSubselectExists() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNotExists() => RegressionRunner.Run(_session, EPLSubselectExists.WithNotExists());

            [Test, RunInApplicationDomain]
            public void WithNotExistsCompile() => RegressionRunner.Run(_session, EPLSubselectExists.WithNotExistsCompile());

            [Test, RunInApplicationDomain]
            public void WithNotExistsOM() => RegressionRunner.Run(_session, EPLSubselectExists.WithNotExistsOM());

            [Test, RunInApplicationDomain]
            public void WithTwoExistsFiltered() => RegressionRunner.Run(_session, EPLSubselectExists.WithTwoExistsFiltered());

            [Test, RunInApplicationDomain]
            public void WithExistsFiltered() => RegressionRunner.Run(_session, EPLSubselectExists.WithExistsFiltered());

            [Test, RunInApplicationDomain]
            public void WithExistsSceneOne() => RegressionRunner.Run(_session, EPLSubselectExists.WithExistsSceneOne());

            [Test, RunInApplicationDomain]
            public void WithExistsInSelectCompile() => RegressionRunner.Run(_session, EPLSubselectExists.WithExistsInSelectCompile());

            [Test, RunInApplicationDomain]
            public void WithExistsInSelectOM() => RegressionRunner.Run(_session, EPLSubselectExists.WithExistsInSelectOM());

            [Test, RunInApplicationDomain]
            public void WithExistsInSelect() => RegressionRunner.Run(_session, EPLSubselectExists.WithExistsInSelect());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectAllAnySomeExpr
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectAllAnySomeExpr : AbstractTestBase
        {
            public TestEPLSubselectAllAnySomeExpr() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithEqualsInNullOrNoRows() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithEqualsInNullOrNoRows());

            [Test, RunInApplicationDomain]
            public void WithEqualsAnyOrSome() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithEqualsAnyOrSome());

            [Test, RunInApplicationDomain]
            public void WithEqualsNotEqualsAll() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithEqualsNotEqualsAll());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpSome() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithRelationalOpSome());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpNullOrNoRows() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithRelationalOpNullOrNoRows());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpAll() => RegressionRunner.Run(_session, EPLSubselectAllAnySomeExpr.WithRelationalOpAll());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectIn
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectIn.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectIn : AbstractTestBase
        {
            public TestEPLSubselectIn() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLSubselectIn.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNotInNullableCoercion() => RegressionRunner.Run(_session, EPLSubselectIn.WithNotInNullableCoercion());

            [Test, RunInApplicationDomain]
            public void WithNotInSelect() => RegressionRunner.Run(_session, EPLSubselectIn.WithNotInSelect());

            [Test, RunInApplicationDomain]
            public void WithNotInNullRow() => RegressionRunner.Run(_session, EPLSubselectIn.WithNotInNullRow());

            [Test, RunInApplicationDomain]
            public void WithInMultiIndex() => RegressionRunner.Run(_session, EPLSubselectIn.WithInMultiIndex());

            [Test, RunInApplicationDomain]
            public void WithInSingleIndex() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSingleIndex());

            [Test, RunInApplicationDomain]
            public void WithInNullRow() => RegressionRunner.Run(_session, EPLSubselectIn.WithInNullRow());

            [Test, RunInApplicationDomain]
            public void WithInNullableCoercion() => RegressionRunner.Run(_session, EPLSubselectIn.WithInNullableCoercion());

            [Test, RunInApplicationDomain]
            public void WithInNullable() => RegressionRunner.Run(_session, EPLSubselectIn.WithInNullable());

            [Test, RunInApplicationDomain]
            public void WithInWildcard() => RegressionRunner.Run(_session, EPLSubselectIn.WithInWildcard());

            [Test, RunInApplicationDomain]
            public void WithInFilterCriteria() => RegressionRunner.Run(_session, EPLSubselectIn.WithInFilterCriteria());

            [Test, RunInApplicationDomain]
            public void WithInSelectWhereExpressions() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSelectWhereExpressions());

            [Test, RunInApplicationDomain]
            public void WithInSelectWhere() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSelectWhere());

            [Test, RunInApplicationDomain]
            public void WithInSelectCompile() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSelectCompile());

            [Test, RunInApplicationDomain]
            public void WithInSelectOM() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSelectOM());

            [Test, RunInApplicationDomain]
            public void WithInSelect() => RegressionRunner.Run(_session, EPLSubselectIn.WithInSelect());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectFiltered
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectFiltered.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectFiltered : AbstractTestBase
        {
            public TestEPLSubselectFiltered() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWhereClauseMultikeyWArrayComposite() =>
                RegressionRunner.Run(_session, EPLSubselectFiltered.WithWhereClauseMultikeyWArrayComposite());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseMultikeyWArray2Field() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithWhereClauseMultikeyWArray2Field());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseMultikeyWArrayPrimitive() =>
                RegressionRunner.Run(_session, EPLSubselectFiltered.WithWhereClauseMultikeyWArrayPrimitive());

            [Test, RunInApplicationDomain]
            public void WithSubselectPrior() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSubselectPrior());

            [Test, RunInApplicationDomain]
            public void WithSubselectMixMax() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSubselectMixMax());

            [Test, RunInApplicationDomain]
            public void WithJoinFilteredTwo() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithJoinFilteredTwo());

            [Test, RunInApplicationDomain]
            public void WithJoinFilteredOne() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithJoinFilteredOne());

            [Test, RunInApplicationDomain]
            public void WithSelectWithWhere2Subqery() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWithWhere2Subqery());

            [Test, RunInApplicationDomain]
            public void WithSelectWhereJoined4BackCoercion() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWhereJoined4BackCoercion());

            [Test, RunInApplicationDomain]
            public void WithSelectWhereJoined4Coercion() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWhereJoined4Coercion());

            [Test, RunInApplicationDomain]
            public void WithSelectWhereJoined3SceneTwo() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWhereJoined3SceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSelectWhereJoined3Streams() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWhereJoined3Streams());

            [Test, RunInApplicationDomain]
            public void WithSelectWhereJoined2Streams() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWhereJoined2Streams());

            [Test, RunInApplicationDomain]
            public void WithSelectWithWhereJoined() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWithWhereJoined());

            [Test, RunInApplicationDomain]
            public void WithWherePreviousCompile() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithWherePreviousCompile());

            [Test, RunInApplicationDomain]
            public void WithWherePreviousOM() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithWherePreviousOM());

            [Test, RunInApplicationDomain]
            public void WithWherePrevious() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithWherePrevious());

            [Test, RunInApplicationDomain]
            public void WithWhereConstant() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithWhereConstant());

            [Test, RunInApplicationDomain]
            public void WithSelectWildcardNoName() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWildcardNoName());

            [Test, RunInApplicationDomain]
            public void WithSelectWildcard() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectWildcard());

            [Test, RunInApplicationDomain]
            public void WithSelectSceneOne() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSelectSceneOne());

            [Test, RunInApplicationDomain]
            public void WithSameEvent() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSameEvent());

            [Test, RunInApplicationDomain]
            public void WithSameEventOM() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSameEventOM());

            [Test, RunInApplicationDomain]
            public void WithSameEventCompile() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithSameEventCompile());

            [Test, RunInApplicationDomain]
            public void WithHavingNoAggWFilterWWhere() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithHavingNoAggWFilterWWhere());

            [Test, RunInApplicationDomain]
            public void WithHavingNoAggWWhere() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithHavingNoAggWWhere());

            [Test, RunInApplicationDomain]
            public void WithHavingNoAggNoFilterNoWhere() => RegressionRunner.Run(_session, EPLSubselectFiltered.WithHavingNoAggNoFilterNoWhere());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectOrderOfEval
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectOrderOfEval.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectOrderOfEval : AbstractTestBase
        {
            public TestEPLSubselectOrderOfEval() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOrderOfEvaluationSubselectFirst() => RegressionRunner.Run(_session, EPLSubselectOrderOfEval.WithOrderOfEvaluationSubselectFirst());

            [Test, RunInApplicationDomain]
            public void WithCorrelatedSubqueryOrder() => RegressionRunner.Run(_session, EPLSubselectOrderOfEval.WithCorrelatedSubqueryOrder());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectFilteredPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectFilteredPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectFilteredPerformance : AbstractTestBase
        {
            public TestEPLSubselectFilteredPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithJoin3CriteriaSceneTwo() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectFilteredPerformance.WithJoin3CriteriaSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithJoin3CriteriaSceneOne() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectFilteredPerformance.WithJoin3CriteriaSceneOne());

            [Test, RunInApplicationDomain]
            public void WithTwoCriteria() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectFilteredPerformance.WithTwoCriteria());

            [Test, RunInApplicationDomain]
            public void WithOneCriteria() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectFilteredPerformance.WithOneCriteria());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectIndex : AbstractTestBase
        {
            public TestEPLSubselectIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUniqueIndexCorrelated() => RegressionRunner.Run(_session, EPLSubselectIndex.WithUniqueIndexCorrelated());

            [Test, RunInApplicationDomain]
            [Parallelizable(ParallelScope.None)]
            public void WithIndexChoicesOverdefinedWhere() => RegressionRunner.Run(_session, EPLSubselectIndex.WithIndexChoicesOverdefinedWhere());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectInKeywordPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectInKeywordPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectInKeywordPerformance : AbstractTestBase
        {
            public TestEPLSubselectInKeywordPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWhereClause() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectInKeywordPerformance.WithWhereClause());

            [Test, RunInApplicationDomain]
            public void WithWhereClauseCoercion() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectInKeywordPerformance.WithWhereClauseCoercion());

            [Test, RunInApplicationDomain]
            public void WithInKeywordAsPartOfSubquery() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectInKeywordPerformance.WithInKeywordAsPartOfSubquery());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectAggregatedSingleValue
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectAggregatedSingleValue : AbstractTestBase
        {
            public TestEPLSubselectAggregatedSingleValue() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupedTableWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithGroupedTableWHaving());

            [Test, RunInApplicationDomain]
            public void WithUngroupedTableWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithUngroupedTableWHaving());

            [Test, RunInApplicationDomain]
            public void WithUngroupedCorrelationInsideHaving() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedCorrelationInsideHaving());

            [Test, RunInApplicationDomain]
            public void WithAggregatedInvalid() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithAggregatedInvalid());

            [Test, RunInApplicationDomain]
            public void WithGroupedCorrelationInsideHaving() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithGroupedCorrelationInsideHaving());

            [Test, RunInApplicationDomain]
            public void WithGroupedCorrelatedWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithGroupedCorrelatedWHaving());

            [Test, RunInApplicationDomain]
            public void WithGroupedUncorrelatedWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithGroupedUncorrelatedWHaving());

            [Test, RunInApplicationDomain]
            public void WithUngroupedJoin2StreamRangeCoercion() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedJoin2StreamRangeCoercion());

            [Test, RunInApplicationDomain]
            public void WithUngroupedJoin3StreamKeyRangeCoercion() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedJoin3StreamKeyRangeCoercion());

            [Test, RunInApplicationDomain]
            public void WithUngroupedCorrelatedWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithUngroupedCorrelatedWHaving());

            [Test, RunInApplicationDomain]
            public void WithUngroupedCorrelatedInWhereClause() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedCorrelatedInWhereClause());

            [Test, RunInApplicationDomain]
            public void WithUngroupedCorrelatedSceneTwo() =>
                RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithUngroupedCorrelatedSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithUngroupedCorrelated() => RegressionRunner.Run(_session, EPLSubselectAggregatedSingleValue.WithUngroupedCorrelated());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedWWhereClause() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedWWhereClause());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedFiltered() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedFiltered());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedInSelectClause() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedInSelectClause());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedInWhereClause() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedInWhereClause());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedWHaving() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedWHaving());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedNoDataWindow() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedNoDataWindow());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedTwoAggStopStart() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedTwoAggStopStart());

            [Test, RunInApplicationDomain]
            public void WithUngroupedUncorrelatedInSelect() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedSingleValue.WithUngroupedUncorrelatedInSelect());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectAggregatedInExistsAnyAll
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectAggregatedInExistsAnyAll : AbstractTestBase
        {
            public TestEPLSubselectAggregatedInExistsAnyAll() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupedWHavingWExists() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithGroupedWHavingWExists());

            [Test, RunInApplicationDomain]
            public void WithGroupedWOHavingWExists() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithGroupedWOHavingWExists());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWHavingWEqualsAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWHavingWEqualsAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWHavingWRelOpAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWHavingWRelOpAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWHavingWIn() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWHavingWIn());

            [Test, RunInApplicationDomain]
            public void WithGroupedWHavingWRelOpAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithGroupedWHavingWRelOpAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithGroupedWHavingWEqualsAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithGroupedWHavingWEqualsAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithGroupedWHavingWIn() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithGroupedWHavingWIn());

            [Test, RunInApplicationDomain]
            public void WithGroupedWOHavingWIn() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithGroupedWOHavingWIn());

            [Test, RunInApplicationDomain]
            public void WithGroupedWOHavingWEqualsAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithGroupedWOHavingWEqualsAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithGroupedWOHavingWRelOpAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithGroupedWOHavingWRelOpAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWHavingWExists() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWHavingWExists());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWOHavingWExists() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWOHavingWExists());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWOHavingWIn() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWOHavingWIn());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWOHavingWEqualsAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWOHavingWEqualsAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWOHavingWRelOpAllAnySome() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedInExistsAnyAll.WithUngroupedWOHavingWRelOpAllAnySome());

            [Test, RunInApplicationDomain]
            public void WithExistsSimple() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithExistsSimple());

            [Test, RunInApplicationDomain]
            public void WithInSimple() => RegressionRunner.Run(_session, EPLSubselectAggregatedInExistsAnyAll.WithInSimple());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectMulticolumn
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectMulticolumn.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectMulticolumn : AbstractTestBase
        {
            public TestEPLSubselectMulticolumn() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCorrelatedAggregation() => RegressionRunner.Run(_session, EPLSubselectMulticolumn.WithCorrelatedAggregation());

            [Test, RunInApplicationDomain]
            public void WithColumnsUncorrelated() => RegressionRunner.Run(_session, EPLSubselectMulticolumn.WithColumnsUncorrelated());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLSubselectMulticolumn.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithMulticolumnAgg() => RegressionRunner.Run(_session, EPLSubselectMulticolumn.WithMulticolumnAgg());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectMultirow
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectMultirow.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectMultirow : AbstractTestBase
        {
            public TestEPLSubselectMultirow() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUnderlyingCorrelated() => RegressionRunner.Run(_session, EPLSubselectMultirow.WithUnderlyingCorrelated());

            [Test, RunInApplicationDomain]
            public void WithSingleColumn() => RegressionRunner.Run(_session, EPLSubselectMultirow.WithSingleColumn());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectAggregatedMultirowAndColumn
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectAggregatedMultirowAndColumn.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectAggregatedMultirowAndColumn : AbstractTestBase
        {
            public TestEPLSubselectAggregatedMultirowAndColumn() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithrowGroupedIndexSharedMultikeyWArray() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedIndexSharedMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedMultikeyWArray() =>
                RegressionRunner.Run(_session, EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithcolumnGroupBy() => RegressionRunner.Run(_session, EPLSubselectAggregatedMultirowAndColumn.WithcolumnGroupBy());

            [Test, RunInApplicationDomain]
            public void WithcolumnInvalid() => RegressionRunner.Run(_session, EPLSubselectAggregatedMultirowAndColumn.WithcolumnInvalid());

            [Test, RunInApplicationDomain]
            public void WithcolumnGroupedWHaving() => RegressionRunner.Run(_session, EPLSubselectAggregatedMultirowAndColumn.WithcolumnGroupedWHaving());

            [Test, RunInApplicationDomain]
            public void WithcolumnGroupedContextPartitioned() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithcolumnGroupedContextPartitioned());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedUncorrelatedIteratorAndExpressionDef() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedUncorrelatedIteratorAndExpressionDef());

            [Test, RunInApplicationDomain]
            public void WithcolumnGroupedUncorrelatedUnfiltered() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithcolumnGroupedUncorrelatedUnfiltered());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedNamedWindowSubqueryIndexShared() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedNamedWindowSubqueryIndexShared());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedCorrelatedWHaving() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedCorrelatedWHaving());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedUncorrelatedWithEnumerationMethod() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedUncorrelatedWithEnumerationMethod());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedCorrelatedWithEnumMethod() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedCorrelatedWithEnumMethod());

            [Test, RunInApplicationDomain]
            public void WithrowGroupedNoDataWindowUncorrelated() => RegressionRunner.Run(
                _session,
                EPLSubselectAggregatedMultirowAndColumn.WithrowGroupedNoDataWindowUncorrelated());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectNamedWindowPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectNamedWindowPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectNamedWindowPerformance : AbstractTestBase
        {
            public TestEPLSubselectNamedWindowPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDisableShareCreate() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithDisableShareCreate());

            [Test, RunInApplicationDomain]
            public void WithDisableShare() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithDisableShare());

            [Test, RunInApplicationDomain]
            public void WithShareCreate() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithShareCreate());

            [Test, RunInApplicationDomain]
            public void WithNoShare() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithNoShare());

            [Test, RunInApplicationDomain]
            public void WithKeyedRange() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithKeyedRange());

            [Test, RunInApplicationDomain]
            public void WithRange() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithRange());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRange() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithKeyAndRange());

            [Test, RunInApplicationDomain]
            public void WithConstantValue() => RegressionRunner.RunPerformanceSensitive(
                _session, EPLSubselectNamedWindowPerformance.WithConstantValue());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectWithinHaving
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectWithinHaving.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectWithinHaving : AbstractTestBase
        {
            public TestEPLSubselectWithinHaving() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withy() => RegressionRunner.Run(_session, EPLSubselectWithinHaving.Withy());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectWithinPattern
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectWithinPattern.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectWithinPattern : AbstractTestBase
        {
            public TestEPLSubselectWithinPattern() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFilterPatternNamedWindowNoAlias() =>
                RegressionRunner.Run(_session, EPLSubselectWithinPattern.WithFilterPatternNamedWindowNoAlias());

            [Test, RunInApplicationDomain]
            public void WithSubqueryAgainstNamedWindowInUDFInPattern() => RegressionRunner.Run(
                _session,
                EPLSubselectWithinPattern.WithSubqueryAgainstNamedWindowInUDFInPattern());

            [Test, RunInApplicationDomain]
            public void WithAggregation() => RegressionRunner.Run(_session, EPLSubselectWithinPattern.WithAggregation());

            [Test, RunInApplicationDomain]
            public void WithCorrelated() => RegressionRunner.Run(_session, EPLSubselectWithinPattern.WithCorrelated());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLSubselectWithinPattern.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLSubselectWithinFilter
        /// <code>
        /// RegressionRunner.Run(_session, EPLSubselectWithinFilter.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSubselectWithinFilter : AbstractTestBase
        {
            public TestEPLSubselectWithinFilter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowWhereAndUDF() => RegressionRunner.Run(_session, EPLSubselectWithinFilter.WithRowWhereAndUDF());

            [Test, RunInApplicationDomain]
            public void WithExistsWhereAndUDF() => RegressionRunner.Run(_session, EPLSubselectWithinFilter.WithExistsWhereAndUDF());
        }
    }
} // end of namespace