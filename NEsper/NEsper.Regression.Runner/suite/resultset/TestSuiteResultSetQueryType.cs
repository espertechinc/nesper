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
    public class TestSuiteResultSetQueryType
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
            }) {
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
                new[] {"sc"},
                typeof(SupportAggMFMultiRTForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(mfAggConfig);

            configuration.Common.AddVariable("MyVar", typeof(string), "");
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeAggregateGrouped()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeAggregateGrouped.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeAggregateGroupedHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeAggregateGroupedHaving.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeGroupByReclaimMicrosecondResolution()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(5000));
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeHaving.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeEnumerator()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeEnumerator.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRollupGroupingFuncs()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRollupGroupingFuncs.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRollupHavingAndOrderBy()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRollupHavingAndOrderBy.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRollupPlanningAndSODA()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRollupPlanningAndSODA());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowForAllHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowForAllHaving.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerEvent()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerEvent.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerEventPerformance()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRowPerEventPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerGroup()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerGroup.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerGroupHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerGroupHaving.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeWTimeBatch()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeWTimeBatch.Executions());
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
                ResultSetQueryTypeRowForAll.WithRowForAllStaticMethodDoubleNested());

            [Test, RunInApplicationDomain]
            public void WithRowForAllNamedWindowWindow() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithRowForAllNamedWindowWindow());

            [Test, RunInApplicationDomain]
            public void WithSelectAvgStdGroupByUni() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSelectAvgStdGroupByUni());

            [Test, RunInApplicationDomain]
            public void WithSelectAvgExprStdGroupBy() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSelectAvgExprStdGroupBy());

            [Test, RunInApplicationDomain]
            public void WithSelectExprGroupWin() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSelectExprGroupWin());

            [Test, RunInApplicationDomain]
            public void WithSelectStarStdGroupBy() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSelectStarStdGroupBy());

            [Test, RunInApplicationDomain]
            public void WithAvgPerSym() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithAvgPerSym());

            [Test, RunInApplicationDomain]
            public void WithSumJoin() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSumJoin());

            [Test, RunInApplicationDomain]
            public void WithSumOneView() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithSumOneView());

            [Test, RunInApplicationDomain]
            public void WithRowForAllMinMaxWindowed() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithRowForAllMinMaxWindowed());

            [Test, RunInApplicationDomain]
            public void WithRowForAllWWindowAgg() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithRowForAllWWindowAgg());

            [Test, RunInApplicationDomain]
            public void WithRowForAllSumMinMax() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithRowForAllSumMinMax());

            [Test, RunInApplicationDomain]
            public void WithRowForAllSimple() => RegressionRunner.Run(_session, ResultSetQueryTypeRowForAll.WithRowForAllSimple());
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
            public void WithRollupMultikeyWArray() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithRollupMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowDeleteAndRStream2Dim() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithNamedWindowDeleteAndRStream2Dim());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowCube2Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithNamedWindowCube2Dim());

            [Test, RunInApplicationDomain]
            public void WithOnSelect() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithOnSelect());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionAlsoRollup() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithContextPartitionAlsoRollup());

            [Test, RunInApplicationDomain]
            public void WithOutputWhenTerminated() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithOutputWhenTerminated());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithUnboundCube4Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundCube4Dim());

            [Test, RunInApplicationDomain]
            public void WithBoundGroupingSet2LevelTopAndDetail() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundGroupingSet2LevelTopAndDetail());

            [Test, RunInApplicationDomain]
            public void WithBoundGroupingSet2LevelNoTopNoDetail() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithBoundGroupingSet2LevelNoTopNoDetail());

            [Test, RunInApplicationDomain]
            public void WithBoundCube3Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithBoundCube3Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundGroupingSet2LevelUnenclosed() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundGroupingSet2LevelUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithUnboundCubeUnenclosed() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundCubeUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollupUnenclosed() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundRollupUnenclosed());

            [Test, RunInApplicationDomain]
            public void WithGroupByWithComputation() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithGroupByWithComputation());

            [Test, RunInApplicationDomain]
            public void WithNonBoxedTypeWithRollup() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithNonBoxedTypeWithRollup());

            [Test, RunInApplicationDomain]
            public void WithMixedAccessAggregation() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithMixedAccessAggregation());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup3Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundRollup3Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup2DimBatchWindow() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeRollupDimensionality.WithUnboundRollup2DimBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup1Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundRollup1Dim());

            [Test, RunInApplicationDomain]
            public void WithUnboundRollup2Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithUnboundRollup2Dim());

            [Test, RunInApplicationDomain]
            public void WithBoundRollup2Dim() => RegressionRunner.Run(_session, ResultSetQueryTypeRollupDimensionality.WithBoundRollup2Dim());
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
            public void WithLocalMultikeyWArray() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggAdditionalAndPlugin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggAdditionalAndPlugin());

            [Test]
            public void WithLocalEnumMethods() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalEnumMethods());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedOrderBy() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedOrderBy());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedOnSelect() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedOnSelect());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedRowRemove() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedRowRemove());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedRowRemove() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedRowRemove());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSameKey() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSameKey());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedSameKey() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedSameKey());

            [Test, RunInApplicationDomain]
            public void WithAggregateFullyVersusNotFullyAgg() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithAggregateFullyVersusNotFullyAgg());

            [Test, RunInApplicationDomain]
            public void WithLocalInvalid() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalInvalid());

            [Test, RunInApplicationDomain]
            public void WithLocalPlanning() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalPlanning());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelNoDefaultLvl() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelNoDefaultLvl());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelAccess() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelAccess());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSolutionPattern() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSolutionPattern());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedMultiLevelMethod() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedMultiLevelMethod());

            [Test, RunInApplicationDomain]
            public void WithLocalGroupedSimple() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalGroupedSimple());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedThreeLevelWTop() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedThreeLevelWTop());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedUnidirectionalJoin() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedUnidirectionalJoin());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedHaving() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedHaving());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedColNameRendering() => RegressionRunner.Run(
                _session,
                ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedColNameRendering());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedParenSODA() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedParenSODA());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggIterator() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggIterator());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggEvent() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggEvent());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedAggSQLStandard() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedAggSQLStandard());

            [Test, RunInApplicationDomain]
            public void WithLocalUngroupedSumSimple() => RegressionRunner.Run(_session, ResultSetQueryTypeLocalGroupBy.WithLocalUngroupedSumSimple());
        }
    }
} // end of namespace