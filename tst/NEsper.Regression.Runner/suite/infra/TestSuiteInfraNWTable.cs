///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.infra.nwtable;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.infra
{
    [TestFixture]
    public class TestSuiteInfraNWTable
    {
        [SetUp]
        public void SetUp()
        {
            _session = RegressionRunner.Session();
            Configure(_session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
            _session = null;
        }

        private RegressionSession _session;

        internal static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBeanRange),
                typeof(SupportSimpleBeanOne),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportBean_ST0),
                typeof(SupportSpatialPoint),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_Container),
                typeof(SupportEnum),
                typeof(OrderBean),
                typeof(SupportEventWithIntArray),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            configuration.Compiler.AddPlugInSingleRowFunction("doubleInt", typeof(InfraNWTableFAF), "DoubleInt");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "justCount",
                typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter),
                "JustCount");
            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableFAFSubstitutionParams()
        {
            RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraComparative()
        {
            RegressionRunner.Run(_session, InfraNWTableComparative.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraContext()
        {
            RegressionRunner.Run(_session, InfraNWTableContext.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraCreateIndex()
        {
            RegressionRunner.Run(_session, InfraNWTableCreateIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraCreateIndexAdvancedSyntax()
        {
            RegressionRunner.Run(_session, new InfraNWTableCreateIndexAdvancedSyntax());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraEventType()
        {
            RegressionRunner.Run(_session, InfraNWTableEventType.Executions());
        }
        
        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraFAFIndex()
        {
            RegressionRunner.Run(_session, InfraNWTableFAFIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableFAFResolve()
        {
            RegressionRunner.Run(_session, InfraNWTableFAFResolve.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnDelete()
        {
            RegressionRunner.Run(_session, InfraNWTableOnDelete.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnMergePerf()
        {
            RegressionRunner.RunPerformanceSensitive(_session, InfraNWTableOnMergePerf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableOnSelectWDelete() {
            RegressionRunner.Run(_session, InfraNWTableOnSelectWDelete.Executions());
        }
        
        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraStartStop()
        {
            RegressionRunner.Run(_session, InfraNWTableStartStop.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqCorrelJoin()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqCorrelJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubquery()
        {
            RegressionRunner.Run(_session, InfraNWTableSubquery.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqueryAtEventBean()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqueryAtEventBean.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqUncorrel()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqUncorrel.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableJoin()
        {
            RegressionRunner.Run(_session, InfraNWTableJoin.Executions());
        }
                           
        /// <summary>
        /// Infrastructure testing: InfraNWTableOnMerge
        ///  <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnMerge.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnMerge : AbstractTestBase
        {
            public TestInfraNWTableOnMerge() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOnMergeSimpleInsert() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeSimpleInsert());

            [Test, RunInApplicationDomain]
            public void WithOnMergeMatchNoMatch() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeMatchNoMatch());

            [Test, RunInApplicationDomain]
            public void WithUpdateNestedEvent() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithUpdateNestedEvent());

            [Test, RunInApplicationDomain]
            public void WithOnMergeInsertStream() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeInsertStream());

            [Test, RunInApplicationDomain]
            public void WithInsertOtherStream() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInsertOtherStream());

            [Test, RunInApplicationDomain]
            public void WithMultiactionDeleteUpdate() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithMultiactionDeleteUpdate());

            [Test, RunInApplicationDomain]
            public void WithUpdateOrderOfFields() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithUpdateOrderOfFields());

            [Test, RunInApplicationDomain]
            public void WithInfraSubqueryNotMatched() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInfraSubqueryNotMatched());

            [Test, RunInApplicationDomain]
            public void WithPatternMultimatch() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPatternMultimatch());

            [Test, RunInApplicationDomain]
            public void WithNoWhereClause() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithNoWhereClause());

            [Test, RunInApplicationDomain]
            public void WithMultipleInsert() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithMultipleInsert());

            [Test, RunInApplicationDomain]
            public void WithFlow() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithFlow());

            [Test, RunInApplicationDomain]
            public void WithInnerTypeAndVariable() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInnerTypeAndVariable());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithInsertOnly() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInsertOnly());

            [Test, RunInApplicationDomain]
            public void WithDeleteThenUpdate() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithDeleteThenUpdate());

            [Test, RunInApplicationDomain]
            public void WithPropertyEvalUpdate() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPropertyEvalUpdate());

            [Test, RunInApplicationDomain]
            public void WithPropertyEvalInsertNoMatch() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPropertyEvalInsertNoMatch());

            [Test, RunInApplicationDomain]
            public void WithSetArrayElementWithIndex() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithSetArrayElementWithIndex());

            [Test, RunInApplicationDomain]
            public void WithSetArrayElementWithIndexInvalid() => RegressionRunner.Run(_session, InfraNWTableOnMerge.WithSetArrayElementWithIndexInvalid());
        }

        /// <summary>
        /// Infrastructure testing: InfraNWTableFAF
        ///  <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAF.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAF : AbstractTestBase
        {
            public TestInfraNWTableFAF() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSelectWildcard() => RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectWildcard());

            [Test, RunInApplicationDomain]
            public void WithSelectWildcardSceneTwo() => RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectWildcardSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithInsert() => RegressionRunner.Run(_session, InfraNWTableFAF.WithInsert());

            [Test, RunInApplicationDomain]
            public void WithUpdate() => RegressionRunner.Run(_session, InfraNWTableFAF.WithUpdate());

            [Test, RunInApplicationDomain]
            public void WithDelete() => RegressionRunner.Run(_session, InfraNWTableFAF.WithDelete());

            [Test, RunInApplicationDomain]
            public void WithDeleteContextPartitioned() => RegressionRunner.Run(_session, InfraNWTableFAF.WithDeleteContextPartitioned());

            [Test, RunInApplicationDomain]
            public void WithSelectCountStar() => RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectCountStar());

            [Test, RunInApplicationDomain]
            public void WithAggUngroupedRowForAll() => RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForAll());

            [Test, RunInApplicationDomain]
            public void WithInClause() => RegressionRunner.Run(_session, InfraNWTableFAF.WithInClause());

            [Test, RunInApplicationDomain]
            public void WithAggUngroupedRowForGroup() => RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForGroup());

            [Test, RunInApplicationDomain]
            public void WithJoin() => RegressionRunner.Run(_session, InfraNWTableFAF.WithJoin());

            [Test, RunInApplicationDomain]
            public void WithAggUngroupedRowForEvent() => RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForEvent());

            [Test, RunInApplicationDomain]
            public void WithJoinWhere() => RegressionRunner.Run(_session, InfraNWTableFAF.WithJoinWhere());

            [Test, RunInApplicationDomain]
            public void With3StreamInnerJoin() => RegressionRunner.Run(_session, InfraNWTableFAF.With3StreamInnerJoin());

            [Test, RunInApplicationDomain]
            public void WithExecuteFilter() => RegressionRunner.Run(_session, InfraNWTableFAF.WithExecuteFilter());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNWTableFAF.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSelectDistinct() => RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectDistinct());
        }
        
        /// <summary>
        /// Infrastructure testing: InfraNWTableOnUpdate
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnUpdate.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableOnUpdate : AbstractTestBase
        {
            public TestInfraNWTableOnUpdate() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOnUpdateSceneOne() => RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithOnUpdateSceneOne());

            [Test, RunInApplicationDomain]
            public void WithUpdateOrderOfFields() => RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithUpdateOrderOfFields());

            [Test, RunInApplicationDomain]
            public void WithSubquerySelf() => RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithSubquerySelf());

            [Test, RunInApplicationDomain]
            public void WithSubqueryMultikeyWArray() => RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithSubqueryMultikeyWArray());
        }

        /// <summary>
        /// Infrastructure testing: InfraNWTableFAFSubquery
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFSubquery.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableFAFSubquery : AbstractTestBase
        {
            public TestInfraNWTableFAFSubquery() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSimple());

            [Test, RunInApplicationDomain]
            public void WithSimpleJoin() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSimpleJoin());

            [Test, RunInApplicationDomain]
            public void WithInsert() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithInsert());

            [Test, RunInApplicationDomain]
            public void WithUpdateUncorrelated() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateUncorrelated());

            [Test, RunInApplicationDomain]
            public void WithDeleteUncorrelated() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithDeleteUncorrelated());

            [Test, RunInApplicationDomain]
            public void WithSelectCorrelated() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectCorrelated());

            [Test, RunInApplicationDomain]
            public void WithUpdateCorrelatedSet() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateCorrelatedSet());

            [Test, RunInApplicationDomain]
            public void WithUpdateCorrelatedWhere() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateCorrelatedWhere());

            [Test, RunInApplicationDomain]
            public void WithDeleteCorrelatedWhere() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithDeleteCorrelatedWhere());

            [Test, RunInApplicationDomain]
            public void WithContextBothWindows() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithContextBothWindows());

            [Test, RunInApplicationDomain]
            public void WithContextSelect() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithContextSelect());

            [Test, RunInApplicationDomain]
            public void WithSelectWhere() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectWhere());

            [Test, RunInApplicationDomain]
            public void WithSelectGroupBy() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectGroupBy());

            [Test, RunInApplicationDomain]
            public void WithSelectIndexPerfWSubstitution() => RegressionRunner.RunPerformanceSensitive(_session, InfraNWTableFAFSubquery.WithSelectIndexPerfWSubstitution());

            [Test, RunInApplicationDomain]
            public void WithSelectIndexPerfCorrelated() => RegressionRunner.RunPerformanceSensitive(_session, InfraNWTableFAFSubquery.WithSelectIndexPerfCorrelated());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithInvalid());
        }

        /// <summary>
        /// Infrastructure testing: InfraNWTableOnSelect
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnSelect.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableOnSelect : AbstractTestBase
        {
            public TestInfraNWTableOnSelect() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithSelectIndexSimple() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectIndexSimple());

            [Test, RunInApplicationDomain]
            public void WithOnSelectIndexChoice() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithOnSelectIndexChoice());

            [Test, RunInApplicationDomain]
            public void WithWindowAgg() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithWindowAgg());

            [Test, RunInApplicationDomain]
            public void WithSelectAggregationHavingStreamWildcard() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationHavingStreamWildcard());

            [Test, RunInApplicationDomain]
            public void WithPatternTimedSelect() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithPatternTimedSelect());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSelectCondition() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectCondition());

            [Test, RunInApplicationDomain]
            public void WithSelectJoinColumnsLimit() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectJoinColumnsLimit());

            [Test, RunInApplicationDomain]
            public void WithSelectAggregation() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregation());

            [Test, RunInApplicationDomain]
            public void WithSelectAggregationCorrelated() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationCorrelated());

            [Test, RunInApplicationDomain]
            public void WithSelectAggregationGrouping() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationGrouping());

            [Test, RunInApplicationDomain]
            public void WithSelectCorrelationDelete() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectCorrelationDelete());

            [Test, RunInApplicationDomain]
            public void WithPatternCorrelation() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithPatternCorrelation());

            [Test, RunInApplicationDomain]
            public void WithOnSelectMultikeyWArray() => RegressionRunner.Run(_session, InfraNWTableOnSelect.WithOnSelectMultikeyWArray());
        }
        
        /// <summary>
        /// Infrastructure testing: InfraNWTableSubqCorrelCoerce
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableSubqCorrelCoerce : AbstractTestBase
        {
            public TestInfraNWTableSubqCorrelCoerce() : base(Configure) { }

            [Test]
            public void WithCoerceSimpleWithNamedWindowsShare() =>
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsShare());

            [Test]
            public void WithCoerceSimpleWithNamedWindowsNoShare() =>
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsNoShare());

            [Test]
            public void WithCoerceSimpleWithNamedWindowsDisableShare() =>
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsDisableShare());

            [Test, RunInApplicationDomain]
            public void WithCoerceSimpleWithTables() =>
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithTables());

        }

        /// <summary>
        /// Auto-test(s): InfraNWTableSubqCorrelIndex
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableSubqCorrelIndex : AbstractTestBase
        {
            public TestInfraNWTableSubqCorrelIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIndexShareMultikeyWArrayTwoArray() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithIndexShareMultikeyWArrayTwoArray());

            [Test, RunInApplicationDomain]
            public void WithIndexShareMultikeyWArraySingleArray() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithIndexShareMultikeyWArraySingleArray());

            [Test, RunInApplicationDomain]
            public void WithCorrelIndexNoIndexShareIndexChoice() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithCorrelIndexNoIndexShareIndexChoice());

            [Test, RunInApplicationDomain]
            public void WithCorrelIndexShareIndexChoice() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithCorrelIndexShareIndexChoice());

            [Test, RunInApplicationDomain]
            public void WithCorrelIndexMultipleIndexHints() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithCorrelIndexMultipleIndexHints());

            [Test, RunInApplicationDomain]
            public void WithCorrelIndexAssertion() => RegressionRunner.Run(
                _session,
                InfraNWTableSubqCorrelIndex.WithCorrelIndexAssertion());
        }
    }
} // end of namespace