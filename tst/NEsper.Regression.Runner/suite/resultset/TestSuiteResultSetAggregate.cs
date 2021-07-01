///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.resultset.aggregate;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetAggregate
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
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBeanString),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanNumeric),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportEventPropertyWithMethod),
                typeof(SupportEventPropertyWithMethod),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportType(typeof(HashableMultiKey));
            configuration.Compiler.ByteCode.IncludeDebugSymbols = true;

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "concatMethodAgg",
                typeof(SupportConcatWManagedAggregationFunctionForge));

            var eventsAsList = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new[] {"eventsAsList"},
                typeof(SupportAggMFEventsAsListForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(eventsAsList);
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFiltered()
        {
            RegressionRunner.Run(session, ResultSetAggregateFiltered.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFirstEverLastEver()
        {
            RegressionRunner.Run(session, ResultSetAggregateFirstEverLastEver.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateLeaving()
        {
            RegressionRunner.Run(session, new ResultSetAggregateLeaving());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMaxMinGroupBy()
        {
            RegressionRunner.Run(session, ResultSetAggregateMaxMinGroupBy.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMedianAndDeviation()
        {
            RegressionRunner.Run(session, ResultSetAggregateMedianAndDeviation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMinMax()
        {
            RegressionRunner.Run(session, ResultSetAggregateMinMax.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateNTh()
        {
            RegressionRunner.Run(session, new ResultSetAggregateNTh());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateRate()
        {
            RegressionRunner.Run(session, ResultSetAggregateRate.Executions());
        }

        /// <summary>
        /// Auto-test(s): ResultSetAggregationMethodSorted
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregationMethodSorted : AbstractTestBase
        {
            public TestResultSetAggregationMethodSorted() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithGrouped() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithGrouped());

            [Test, RunInApplicationDomain]
            public void WithMultiCriteria() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithMultiCriteria());

            [Test, RunInApplicationDomain]
            public void WithOrderedDictionaryReference() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithOrderedDictionaryReference());

            [Test, RunInApplicationDomain]
            public void WithSubmapEventsBetween() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithSubmapEventsBetween());

            [Test, RunInApplicationDomain]
            public void WithGetContainsCounts() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithGetContainsCounts());

            [Test, RunInApplicationDomain]
            public void WithFirstLastEnumerationAndDot() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithFirstLastEnumerationAndDot());

            [Test, RunInApplicationDomain]
            public void WithFirstLast() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithFirstLast());

            [Test, RunInApplicationDomain]
            public void WithCFHLEnumerationAndDot() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithCFHLEnumerationAndDot());

            [Test, RunInApplicationDomain]
            public void WithCFHL() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithCFHL());

            [Test, RunInApplicationDomain]
            public void WithTableIdent() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithTableIdent());

            [Test, RunInApplicationDomain]
            public void WithTableAccess() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithTableAccess());

            [Test, RunInApplicationDomain]
            public void WithNonTable() => RegressionRunner.Run(_session, ResultSetAggregationMethodSorted.WithNonTable());
        }

        /// <summary>
        /// Auto-test(s): ResultSetAggregateCountSum
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregateCountSum.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregateCountSum : AbstractTestBase
        {
            public TestResultSetAggregateCountSum() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCountDistinctMultikeyWArray() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountDistinctMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithSumNamedWindowRemoveGroup() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithSumNamedWindowRemoveGroup());

            [Test, RunInApplicationDomain]
            public void WithCountDistinctGrouped() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountDistinctGrouped());

            [Test, RunInApplicationDomain]
            public void WithCountJoin() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountJoin());

            [Test, RunInApplicationDomain]
            public void WithCountOneView() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountOneView());

            [Test, RunInApplicationDomain]
            public void WithCountOneViewCompile() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountOneViewCompile());

            [Test, RunInApplicationDomain]
            public void WithGroupByCountNestedAggregationAvg() => RegressionRunner.Run(
                _session,
                ResultSetAggregateCountSum.WithGroupByCountNestedAggregationAvg());

            [Test, RunInApplicationDomain]
            public void WithCountOneViewOM() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountOneViewOM());

            [Test, RunInApplicationDomain]
            public void WithSumHaving() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithSumHaving());

            [Test, RunInApplicationDomain]
            public void WithCountHaving() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountHaving());

            [Test, RunInApplicationDomain]
            public void WithCountPlusStar() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountPlusStar());

            [Test, RunInApplicationDomain]
            public void WithCountSimple() => RegressionRunner.Run(_session, ResultSetAggregateCountSum.WithCountSimple());
        }

        /// <summary>
        /// Auto-test(s): ResultSetAggregateFilterNamedParameter
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregateFilterNamedParameter : AbstractTestBase
        {
            public TestResultSetAggregateFilterNamedParameter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIntoTableCountMinSketch() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithIntoTableCountMinSketch());

            [Test, RunInApplicationDomain]
            public void WithIntoTable() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithIntoTable());

            [Test, RunInApplicationDomain]
            public void WithAccessAggPlugIn() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggPlugIn());

            [Test, RunInApplicationDomain]
            public void WithMethodPlugIn() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodPlugIn());

            [Test, RunInApplicationDomain]
            public void WithFilterNamedParamInvalid() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithFilterNamedParamInvalid());

            [Test, RunInApplicationDomain]
            public void WithAuditAndReuse() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAuditAndReuse());

            [Test, RunInApplicationDomain]
            public void WithAccessAggSortedMulticriteria() => RegressionRunner.Run(
                _session,
                ResultSetAggregateFilterNamedParameter.WithAccessAggSortedMulticriteria());

            [Test, RunInApplicationDomain]
            public void WithAccessAggSortedUnbound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggSortedUnbound());

            [Test, RunInApplicationDomain]
            public void WithAccessAggSortedBound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggSortedBound());

            [Test, RunInApplicationDomain]
            public void WithAccessAggLinearBoundMixedFilter() => RegressionRunner.Run(
                _session,
                ResultSetAggregateFilterNamedParameter.WithAccessAggLinearBoundMixedFilter());

            [Test, RunInApplicationDomain]
            public void WithAccessAggLinearWIndex() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggLinearWIndex());

            [Test, RunInApplicationDomain]
            public void WithAccessAggLinearUnbound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggLinearUnbound());

            [Test, RunInApplicationDomain]
            public void WithAccessAggLinearBound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithAccessAggLinearBound());

            [Test, RunInApplicationDomain]
            public void WithMethodAggRateBound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggRateBound());

            [Test, RunInApplicationDomain]
            public void WithMethodAggRateUnbound() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggRateUnbound());

            [Test, RunInApplicationDomain]
            public void WithMethodAggNth() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggNth());

            [Test, RunInApplicationDomain]
            public void WithMethodAggLeaving() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggLeaving());

            [Test, RunInApplicationDomain]
            public void WithMethodAggSQLMixedFilter() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggSQLMixedFilter());

            [Test, RunInApplicationDomain]
            public void WithMethodAggSQLAll() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithMethodAggSQLAll());

            [Test, RunInApplicationDomain]
            public void WithFirstAggSODA() => RegressionRunner.Run(_session, ResultSetAggregateFilterNamedParameter.WithFirstAggSODA());
        }
        
        /// <summary>
        /// Auto-test(s): ResultSetAggregateSortedMinMaxBy
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregateSortedMinMaxBy : AbstractTestBase
        {
            public TestResultSetAggregateSortedMinMaxBy() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNoDataWindow() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithNoDataWindow());

            [Test, RunInApplicationDomain]
            public void WithMultipleCriteria() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithMultipleCriteria());

            [Test, RunInApplicationDomain]
            public void WithMultipleCriteriaSimple() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithMultipleCriteriaSimple());

            [Test, RunInApplicationDomain]
            public void WithNoAlias() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithNoAlias());

            [Test]
            public void WithMinByMaxByOverWindow() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithMinByMaxByOverWindow());

            [Test, RunInApplicationDomain]
            public void WithMultipleOverlappingCategories() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithMultipleOverlappingCategories());

            [Test, RunInApplicationDomain]
            public void WithGroupedSortedMinMax() => RegressionRunner.Run(_session, ResultSetAggregateSortedMinMaxBy.WithGroupedSortedMinMax());
        }
        
        /// <summary>
        /// Auto-test(s): ResultSetAggregationMethodWindow
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregationMethodWindow : AbstractTestBase
        {
            public TestResultSetAggregationMethodWindow() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithListReference() => RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.WithListReference());

            [Test, RunInApplicationDomain]
            public void WithTableIdentWCount() => RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.WithTableIdentWCount());

            [Test, RunInApplicationDomain]
            public void WithTableAccess() => RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.WithTableAccess());

            [Test, RunInApplicationDomain]
            public void WithNonTable() => RegressionRunner.Run(_session, ResultSetAggregationMethodWindow.WithNonTable());
        }

        /// <summary>
        /// Auto-test(s): ResultSetAggregateFirstLastWindow
        /// <code>
        /// RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.Executions());
        /// </code>
        /// </summary>

        public class TestResultSetAggregateFirstLastWindow : AbstractTestBase
        {
            public TestResultSetAggregateFirstLastWindow() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOnDemandQuery() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithOnDemandQuery());

            [Test, RunInApplicationDomain]
            public void WithNoParamChainedAndProperty() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithNoParamChainedAndProperty());

            [Test, RunInApplicationDomain]
            public void WithMixedNamedWindow() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithMixedNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithLateInitialize() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithLateInitialize());

            [Test, RunInApplicationDomain]
            public void WithLastMaxMixedOnSelect() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithLastMaxMixedOnSelect());

            [Test, RunInApplicationDomain]
            public void WithOnDelete() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithOnDelete());

            [Test, RunInApplicationDomain]
            public void WithOutputRateLimiting() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithOutputRateLimiting());

            [Test, RunInApplicationDomain]
            public void WithWindowAndSumWGroup() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithWindowAndSumWGroup());

            [Test, RunInApplicationDomain]
            public void WithFirstLastWindowGroup() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithFirstLastWindowGroup());

            [Test, RunInApplicationDomain]
            public void WithFirstLastWindowNoGroup() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithFirstLastWindowNoGroup());

            [Test, RunInApplicationDomain]
            public void WithBatchWindowGrouped() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithBatchWindowGrouped());

            [Test, RunInApplicationDomain]
            public void WithBatchWindow() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithOuterJoin1Access() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithOuterJoin1Access());

            [Test, RunInApplicationDomain]
            public void WithJoin2Access() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithJoin2Access());

            [Test, RunInApplicationDomain]
            public void WithTypeAndColNameAndEquivalency() => RegressionRunner.Run(
                _session,
                ResultSetAggregateFirstLastWindow.WithTypeAndColNameAndEquivalency());

            [Test, RunInApplicationDomain]
            public void WithMethodAndAccessTogether() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithMethodAndAccessTogether());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithPrevNthIndexedFirstLast() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithPrevNthIndexedFirstLast());

            [Test, RunInApplicationDomain]
            public void WithFirstLastIndexed() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithFirstLastIndexed());

            [Test, RunInApplicationDomain]
            public void WithWindowedGrouped() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithWindowedGrouped());

            [Test, RunInApplicationDomain]
            public void WithWindowedUnGrouped() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithWindowedUnGrouped());

            [Test, RunInApplicationDomain]
            public void WithUnboundedStream() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithUnboundedStream());

            [Test, RunInApplicationDomain]
            public void WithUnboundedSimple() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithUnboundedSimple());

            [Test, RunInApplicationDomain]
            public void WithStar() => RegressionRunner.Run(_session, ResultSetAggregateFirstLastWindow.WithStar());
        }
    }
} // end of namespace