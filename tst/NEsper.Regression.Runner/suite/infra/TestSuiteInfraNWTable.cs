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
    [Parallelizable(ParallelScope.Children)]
    public class TestSuiteInfraNWTable : AbstractTestBase
    {
        public TestSuiteInfraNWTable() : base(Configure)
        {
        }

        internal static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[]
                     {
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
                     }
                    )
                configuration.Common.AddEventType(clazz);

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Compiler.AddPlugInSingleRowFunction("doubleInt", typeof(InfraNWTableFAF), "DoubleInt");
            configuration.Compiler.AddPlugInSingleRowFunction("justCount",
                typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter), "JustCount");
            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
        }

        [Test]
        [RunInApplicationDomain]
        public void TestInfraNWTableInfraCreateIndexAdvancedSyntax()
        {
            RegressionRunner.Run(_session, new InfraNWTableCreateIndexAdvancedSyntax());
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableOnMerge
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnMerge.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnMerge : AbstractTestBase
        {
            public TestInfraNWTableOnMerge() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnMergeSimpleInsert()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeSimpleInsert());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnMergeMatchNoMatch()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeMatchNoMatch());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateNestedEvent()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithUpdateNestedEvent());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnMergeInsertStream()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithOnMergeInsertStream());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsertOtherStream([Values] EventRepresentationChoice rep)
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInsertOtherStream(rep));
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultiactionDeleteUpdate()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithMultiactionDeleteUpdate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateOrderOfFields()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithUpdateOrderOfFields());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubqueryNotMatched()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithSubqueryNotMatched());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPatternMultimatch()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPatternMultimatch());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithNoWhereClause()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithNoWhereClause());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultipleInsert()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithMultipleInsert());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithFlow()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithFlow());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInnerTypeAndVariable([Values] EventRepresentationChoice rep)
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInnerTypeAndVariable(rep));
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsertOnly([Values] bool namedWindow)
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithInsertOnly(namedWindow));
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDeleteThenUpdate()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithDeleteThenUpdate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPropertyEvalUpdate()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPropertyEvalUpdate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPropertyEvalInsertNoMatch()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithPropertyEvalInsertNoMatch());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSetArrayElementWithIndex()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithSetArrayElementWithIndex());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSetArrayElementWithIndexInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMerge.WithSetArrayElementWithIndexInvalid());
            }
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableFAF
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAF.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAF : AbstractTestBase
        {
            public TestInfraNWTableFAF() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectWildcard()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectWildcard());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectWildcardSceneTwo()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectWildcardSceneTwo());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsert()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithInsert());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdate()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithUpdate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDelete()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithDelete());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDeleteContextPartitioned()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithDeleteContextPartitioned());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectCountStar()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectCountStar());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithAggUngroupedRowForAll()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForAll());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInClause()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithInClause());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithAggUngroupedRowForGroup()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForGroup());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithJoin()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithJoin());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithAggUngroupedRowForEvent()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithAggUngroupedRowForEvent());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithJoinWhere()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithJoinWhere());
            }

            [Test]
            [RunInApplicationDomain]
            [TestCase(EventRepresentationChoice.MAP)]
            [TestCase(EventRepresentationChoice.AVRO)]
            [TestCase(EventRepresentationChoice.JSON)]
            [TestCase(EventRepresentationChoice.JSONCLASSPROVIDED)]
            [TestCase(EventRepresentationChoice.OBJECTARRAY)]
            public void With3StreamInnerJoin(EventRepresentationChoice rep)
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.With3StreamInnerJoin(rep));
            }

            [Test]
            [RunInApplicationDomain]
            public void WithExecuteFilter()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithExecuteFilter());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectDistinct()
            {
                RegressionRunner.Run(_session, InfraNWTableFAF.WithSelectDistinct());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalidInsert([Values] EventRepresentationChoice rep) =>
                RegressionRunner.Run(_session, InfraNWTableFAF.WithInvalidInsert(rep));
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableOnUpdate
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnUpdate.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnUpdate : AbstractTestBase
        {
            public TestInfraNWTableOnUpdate() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnUpdateSceneOne()
            {
                RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithNWTableOnUpdateSceneOne());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateOrderOfFields()
            {
                RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithUpdateOrderOfFields());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubquerySelf()
            {
                RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithSubquerySelf());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubqueryMultikeyWArray()
            {
                RegressionRunner.Run(_session, InfraNWTableOnUpdate.WithSubqueryMultikeyWArray());
            }
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableFAFSubquery
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFSubquery.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAFSubquery : AbstractTestBase
        {
            public TestInfraNWTableFAFSubquery() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSimple()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSimple());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSimpleJoin()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSimpleJoin());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsert()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithInsert());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateUncorrelated()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateUncorrelated());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDeleteUncorrelated()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithDeleteUncorrelated());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectCorrelated()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectCorrelated());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateCorrelatedSet()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateCorrelatedSet());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUpdateCorrelatedWhere()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithUpdateCorrelatedWhere());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDeleteCorrelatedWhere()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithDeleteCorrelatedWhere());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithContextBothWindows()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithContextBothWindows());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithContextSelect()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithContextSelect());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectWhere()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectWhere());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectGroupBy()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithSelectGroupBy());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectIndexPerfWSubstitution()
            {
                RegressionRunner.RunPerformanceSensitive(_session,
                    InfraNWTableFAFSubquery.WithSelectIndexPerfWSubstitution());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectIndexPerfCorrelated()
            {
                RegressionRunner.RunPerformanceSensitive(_session,
                    InfraNWTableFAFSubquery.WithSelectIndexPerfCorrelated());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubquery.WithInvalid());
            }
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableOnSelect
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnSelect.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnSelect : AbstractTestBase
        {
            public TestInfraNWTableOnSelect() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectIndexSimple()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithOnSelectIndexSimple());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnSelectIndexChoice()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithOnSelectIndexChoice());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithWindowAgg()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithWindowAgg());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectAggregationHavingStreamWildcard()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationHavingStreamWildcard());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPatternTimedSelect()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithPatternTimedSelect());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectCondition()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectCondition());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectJoinColumnsLimit()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectJoinColumnsLimit());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectAggregation()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregation());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectAggregationCorrelated()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationCorrelated());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectAggregationGrouping()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectAggregationGrouping());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectCorrelationDelete()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithSelectCorrelationDelete());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPatternCorrelation()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithPatternCorrelation());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnSelectMultikeyWArray()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelect.WithOnSelectMultikeyWArray());
            }
        }

        /// <summary>
        ///     Infrastructure testing: InfraNWTableSubqCorrelCoerce
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubqCorrelCoerce : AbstractTestBase
        {
            public TestInfraNWTableSubqCorrelCoerce() : base(Configure)
            {
            }

            [Test]
            public void WithCoerceSimpleWithNamedWindowsShare()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsShare());
            }

            [Test]
            public void WithCoerceSimpleWithNamedWindowsNoShare()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsNoShare());
            }

            [Test]
            public void WithCoerceSimpleWithNamedWindowsDisableShare()
            {
                RegressionRunner.Run(_session,
                    InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithNamedWindowsDisableShare());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCoerceSimpleWithTables()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.WithCoerceSimpleWithTables());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableSubqCorrelIndex
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubqCorrelIndex : AbstractTestBase
        {
            public TestInfraNWTableSubqCorrelIndex() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithIndexShareMultikeyWArrayTwoArray()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithIndexShareMultikeyWArrayTwoArray());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithIndexShareMultikeyWArraySingleArray()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithIndexShareMultikeyWArraySingleArray());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCorrelIndexNoIndexShareIndexChoice()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithCorrelIndexNoIndexShareIndexChoice());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCorrelIndexShareIndexChoice()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithCorrelIndexShareIndexChoice());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCorrelIndexMultipleIndexHints()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithCorrelIndexMultipleIndexHints());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCorrelIndexAssertion()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.WithCorrelIndexAssertion());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableFAFSubstitutionParams
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAFSubstitutionParams : AbstractTestBase
        {
            public TestInfraNWTableFAFSubstitutionParams() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void Withy()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.Withy());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithyNamedParameter()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.WithyNamedParameter());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithyInvalidUse()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.WithyInvalidUse());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithyInvalidInsufficientValues()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.WithyInvalidInsufficientValues());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithyInvalidParametersUntyped()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.WithyInvalidParametersUntyped());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithyInvalidParametersTyped()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.WithyInvalidParametersTyped());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableComparative
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableComparative.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableComparative : AbstractTestBase
        {
            public TestInfraNWTableComparative() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithGroupByTopLevelSingleAgg()
            {
                RegressionRunner.Run(_session, InfraNWTableComparative.WithGroupByTopLevelSingleAgg());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableContext
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableContext.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableContext : AbstractTestBase
        {
            public TestInfraNWTableContext() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithContext()
            {
                RegressionRunner.Run(_session, InfraNWTableContext.WithContext());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableCreateIndex
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableCreateIndex.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableCreateIndex : AbstractTestBase
        {
            public TestInfraNWTableCreateIndex() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultiRangeAndKey()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithMultiRangeAndKey());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithHashBTreeWidening()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithHashBTreeWidening());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithWidening()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithWidening());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCompositeIndex()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithCompositeIndex());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithLateCreate()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithLateCreate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithLateCreateSceneTwo()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithLateCreateSceneTwo());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultipleColumnMultipleIndex()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithMultipleColumnMultipleIndex());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDropCreate()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithDropCreate());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithOnSelectReUse()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithOnSelectReUse());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultikeyIndexFAF()
            {
                RegressionRunner.Run(_session, InfraNWTableCreateIndex.WithMultikeyIndexFAF());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableEventType
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableEventType.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableEventType : AbstractTestBase
        {
            public TestInfraNWTableEventType() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalid()
            {
                RegressionRunner.Run(_session, InfraNWTableEventType.WithInvalid());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDefineFields()
            {
                RegressionRunner.Run(_session, InfraNWTableEventType.WithDefineFields());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInsertIntoProtected()
            {
                RegressionRunner.Run(_session, InfraNWTableEventType.WithInsertIntoProtected());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableFAFIndex
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFIndex.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAFIndex : AbstractTestBase
        {
            public TestInfraNWTableFAFIndex() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithChoiceJoin()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithChoiceJoin());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithChoice()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithChoice());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultikeyWArray()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithMultikeyWArray());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultikeyWArrayTwoField()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithMultikeyWArrayTwoField());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultikeyWArrayCompositeArray()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithMultikeyWArrayCompositeArray());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithMultikeyWArrayCompositeTwoArray()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFIndex.WithMultikeyWArrayCompositeTwoArray());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableFAFResolve
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFResolve.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableFAFResolve : AbstractTestBase
        {
            public TestInfraNWTableFAFResolve() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSelectWildcard()
            {
                RegressionRunner.Run(_session, InfraNWTableFAFResolve.WithSelectWildcard());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableOnDelete
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnDelete.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnDelete : AbstractTestBase
        {
            public TestInfraNWTableOnDelete() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithCondition()
            {
                RegressionRunner.Run(_session, InfraNWTableOnDelete.WithCondition());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPattern()
            {
                RegressionRunner.Run(_session, InfraNWTableOnDelete.WithPattern());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithAll()
            {
                RegressionRunner.Run(_session, InfraNWTableOnDelete.WithAll());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableOnMergePerf
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnMergePerf.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnMergePerf : AbstractTestBase
        {
            public TestInfraNWTableOnMergePerf() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithPerformance()
            {
                RegressionRunner.Run(_session, InfraNWTableOnMergePerf.WithPerformance());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableOnSelectWDelete
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableOnSelectWDelete.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableOnSelectWDelete : AbstractTestBase
        {
            public TestInfraNWTableOnSelectWDelete() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithDeleteAssertion()
            {
                RegressionRunner.Run(_session, InfraNWTableOnSelectWDelete.WithDeleteAssertion());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableStartStop
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableStartStop.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableStartStop : AbstractTestBase
        {
            public TestInfraNWTableStartStop() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithConsumer()
            {
                RegressionRunner.Run(_session, InfraNWTableStartStop.WithConsumer());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInserter()
            {
                RegressionRunner.Run(_session, InfraNWTableStartStop.WithInserter());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableSubqCorrelJoin
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqCorrelJoin.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubqCorrelJoin : AbstractTestBase
        {
            public TestInfraNWTableSubqCorrelJoin() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithJoinAssertion()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqCorrelJoin.WithJoinAssertion());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableSubquery
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubquery.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubquery : AbstractTestBase
        {
            public TestInfraNWTableSubquery() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubquerySceneOne()
            {
                RegressionRunner.Run(_session, InfraNWTableSubquery.WithSubquerySceneOne());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubquerySelfCheck()
            {
                RegressionRunner.Run(_session, InfraNWTableSubquery.WithSubquerySelfCheck());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubqueryDeleteInsertReplace()
            {
                RegressionRunner.Run(_session, InfraNWTableSubquery.WithSubqueryDeleteInsertReplace());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithInvalidSubquery()
            {
                RegressionRunner.Run(_session, InfraNWTableSubquery.WithInvalidSubquery());
            }

            [Test]
            [RunInApplicationDomain]
            public void WithUncorrelatedSubqueryAggregation()
            {
                RegressionRunner.Run(_session, InfraNWTableSubquery.WithUncorrelatedSubqueryAggregation());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableSubqueryAtEventBean
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqueryAtEventBean.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubqueryAtEventBean : AbstractTestBase
        {
            public TestInfraNWTableSubqueryAtEventBean() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubSelStar()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqueryAtEventBean.WithSubSelStar());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableSubqUncorrel
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableSubqUncorrel.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableSubqUncorrel : AbstractTestBase
        {
            public TestInfraNWTableSubqUncorrel() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithSubqUncorrelAssertion()
            {
                RegressionRunner.Run(_session, InfraNWTableSubqUncorrel.WithSubqUncorrelAssertion());
            }
        }

        /// <summary>
        ///     Auto-test(s): InfraNWTableJoin
        ///     <code>
        /// RegressionRunner.Run(_session, InfraNWTableJoin.Executions());
        /// </code>
        /// </summary>
        public class TestInfraNWTableJoin : AbstractTestBase
        {
            public TestInfraNWTableJoin() : base(Configure)
            {
            }

            [Test]
            [RunInApplicationDomain]
            public void WithJoinSimple()
            {
                RegressionRunner.Run(_session, InfraNWTableJoin.WithJoinSimple());
            }
        }
    }
} // end of namespace