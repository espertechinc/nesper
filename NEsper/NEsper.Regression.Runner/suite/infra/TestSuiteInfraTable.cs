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
using com.espertech.esper.regressionlib.suite.infra.tbl;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.Runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Table tests
    [TestFixture]
    public class TestSuiteInfraTable
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
                typeof(SupportIntrusionEvent),
                typeof(SupportTrafficEvent),
                typeof(SupportMySortValueEvent),
                typeof(SupportBean_S2),
                typeof(SupportBeanSimple),
                typeof(SupportByteArrEventStringId),
                typeof(SupportBeanRange),
                typeof(SupportTwoKeyEvent),
                typeof(SupportCtorSB2WithObjectArray),
                typeof(Support10ColEvent),
                typeof(SupportTopGroupSubGroupEvent),
                typeof(SupportBeanNumeric),
                typeof(SupportEventWithManyArray),
                typeof(SupportEventWithManyArray),
                typeof(SupportEventWithIntArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction(
                "singlerow",
                typeof(InfraTableInvalid),
                "MySingleRowFunction");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "pluginServiceEventBean",
                typeof(InfraTableSelect),
                "MyServiceEventBean");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "toIntArray",
                typeof(InfraTableOnUpdate),
                "ToIntArray");

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "myaggsingle",
                typeof(SupportCountBackAggregationFunctionForge));
            configuration.Compiler.AddPlugInAggregationFunctionForge("csvWords", typeof(SupportSimpleWordCSVForge));

            var config = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new[] {"referenceCountedMap"},
                typeof(SupportReferenceCountedMapForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(config);
            var configMultiFuncAgg = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new[] {"se1"},
                typeof(SupportAggMFMultiRTForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(configMultiFuncAgg);

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableFilters()
        {
            RegressionRunner.Run(session, new InfraTableFilters());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableIterate()
        {
            RegressionRunner.Run(session, new InfraTableIterate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTAccessReadMergeWriteInsertDeleteRowVisible()
        {
            RegressionRunner.Run(session, new InfraTableMTAccessReadMergeWriteInsertDeleteRowVisible());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedFAFReadFAFWriteChain()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedFAFReadFAFWriteChain());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedWContextIntoTableWriteAsContextTable()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedWContextIntoTableWriteAsContextTable());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedWContextIntoTableWriteAsSharedTable()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedWContextIntoTableWriteAsSharedTable());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadInotTableWriteIterate()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadInotTableWriteIterate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadIntoTableWriteFilterUse()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadIntoTableWriteFilterUse());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadMergeWrite()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadMergeWrite());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessWithinRowFAFConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessWithinRowFAFConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedIntoTableWriteMultiWriterAgg()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedIntoTableWriteMultiWriterAgg());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedJoinColumnConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedJoinColumnConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableNonAccessDotSubqueryAndJoin()
        {
            RegressionRunner.Run(session, new InfraTableNonAccessDotSubqueryAndJoin());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableOnSelect()
        {
            RegressionRunner.Run(session, new InfraTableOnSelect());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableOutputRateLimiting()
        {
            RegressionRunner.Run(session, new InfraTableOutputRateLimiting());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableWNamedWindow()
        {
            RegressionRunner.Run(session, new InfraTableWNamedWindow());
        }

        /// <summary>
        /// Auto-test(s): InfraTableAccessAggregationState
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableAccessAggregationState.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableAccessAggregationState : AbstractTestBase
        {
            public TestInfraTableAccessAggregationState() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNestedMultivalueAccess() => RegressionRunner.Run(_session, InfraTableAccessAggregationState.WithNestedMultivalueAccess());

            [Test, RunInApplicationDomain]
            public void WithTableAccessGroupedThreeKey() => RegressionRunner.Run(_session, InfraTableAccessAggregationState.WithTableAccessGroupedThreeKey());

            [Test, RunInApplicationDomain]
            public void WithTableAccessGroupedMixed() => RegressionRunner.Run(_session, InfraTableAccessAggregationState.WithTableAccessGroupedMixed());

            [Test, RunInApplicationDomain]
            public void WithAccessAggShare() => RegressionRunner.Run(_session, InfraTableAccessAggregationState.WithAccessAggShare());
        }

        /// <summary>
        /// Auto-test(s): InfraTableAccessCore
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableAccessCore.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableAccessCore : AbstractTestBase
        {
            public TestInfraTableAccessCore() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTableAccessMultikeyWArrayTwoArrayKey() => RegressionRunner.Run(
                _session,
                InfraTableAccessCore.WithTableAccessMultikeyWArrayTwoArrayKey());

            [Test, RunInApplicationDomain]
            public void WithTableAccessMultikeyWArrayOneArrayKey() => RegressionRunner.Run(
                _session,
                InfraTableAccessCore.WithTableAccessMultikeyWArrayOneArrayKey());

            [Test, RunInApplicationDomain]
            public void WithTableAccessCoreSplitStream() => RegressionRunner.Run(_session, InfraTableAccessCore.WithTableAccessCoreSplitStream());

            [Test, RunInApplicationDomain]
            public void WithOnMergeExpressions() => RegressionRunner.Run(_session, InfraTableAccessCore.WithOnMergeExpressions());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, InfraTableAccessCore.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowAndFireAndForget() => RegressionRunner.Run(_session, InfraTableAccessCore.WithNamedWindowAndFireAndForget());

            [Test, RunInApplicationDomain]
            public void WithGroupedMixedMethodAndAccess() => RegressionRunner.Run(_session, InfraTableAccessCore.WithGroupedMixedMethodAndAccess());

            [Test, RunInApplicationDomain]
            public void WithMultiStmtContributing() => RegressionRunner.Run(_session, InfraTableAccessCore.WithMultiStmtContributing());

            [Test, RunInApplicationDomain]
            public void WithOrderOfAggregationsAndPush() => RegressionRunner.Run(_session, InfraTableAccessCore.WithOrderOfAggregationsAndPush());

            [Test, RunInApplicationDomain]
            public void WithUngroupedWContext() => RegressionRunner.Run(_session, InfraTableAccessCore.WithUngroupedWContext());

            [Test, RunInApplicationDomain]
            public void WithGroupedSingleKeyNoContext() => RegressionRunner.Run(_session, InfraTableAccessCore.WithGroupedSingleKeyNoContext());

            [Test, RunInApplicationDomain]
            public void WithGroupedThreeKeyNoContext() => RegressionRunner.Run(_session, InfraTableAccessCore.WithGroupedThreeKeyNoContext());

            [Test, RunInApplicationDomain]
            public void WithGroupedTwoKeyNoContext() => RegressionRunner.Run(_session, InfraTableAccessCore.WithGroupedTwoKeyNoContext());

            [Test, RunInApplicationDomain]
            public void WithExpressionAliasAndDecl() => RegressionRunner.Run(_session, InfraTableAccessCore.WithExpressionAliasAndDecl());

            [Test, RunInApplicationDomain]
            public void WithTopLevelReadUnGrouped() => RegressionRunner.Run(_session, InfraTableAccessCore.WithTopLevelReadUnGrouped());

            [Test, RunInApplicationDomain]
            public void WithTopLevelReadGrouped2Keys() => RegressionRunner.Run(_session, InfraTableAccessCore.WithTopLevelReadGrouped2Keys());

            [Test, RunInApplicationDomain]
            public void WithExprSelectClauseRenderingUnnamedCol() => RegressionRunner.Run(
                _session,
                InfraTableAccessCore.WithExprSelectClauseRenderingUnnamedCol());

            [Test, RunInApplicationDomain]
            public void WithFilterBehavior() => RegressionRunner.Run(_session, InfraTableAccessCore.WithFilterBehavior());

            [Test, RunInApplicationDomain]
            public void WithIntegerIndexedPropertyLookAlike() => RegressionRunner.Run(_session, InfraTableAccessCore.WithIntegerIndexedPropertyLookAlike());

            [Test, RunInApplicationDomain]
            public void WithTableAccessCoreUnGroupedWindowAndSum() => RegressionRunner.Run(
                _session,
                InfraTableAccessCore.WithTableAccessCoreUnGroupedWindowAndSum());
        }

        /// <summary>
        /// Auto-test(s): InfraTableAccessDotMethod
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableAccessDotMethod.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableAccessDotMethod : AbstractTestBase
        {
            public TestInfraTableAccessDotMethod() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNestedDotMethod() => RegressionRunner.Run(_session, InfraTableAccessDotMethod.WithNestedDotMethod());

            [Test, RunInApplicationDomain]
            public void WithAggDatetimeAndEnumerationAndMethod() => RegressionRunner.Run(
                _session,
                InfraTableAccessDotMethod.WithAggDatetimeAndEnumerationAndMethod());

            [Test, RunInApplicationDomain]
            public void WithPlainPropDatetimeAndEnumerationAndMethod() => RegressionRunner.Run(
                _session,
                InfraTableAccessDotMethod.WithPlainPropDatetimeAndEnumerationAndMethod());
        }

        /// <summary>
        /// Auto-test(s): InfraTableContext
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableContext.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableContext : AbstractTestBase
        {
            public TestInfraTableContext() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTableContextInvalid() => RegressionRunner.Run(_session, InfraTableContext.WithTableContextInvalid());

            [Test, RunInApplicationDomain]
            public void WithNonOverlapping() => RegressionRunner.Run(_session, InfraTableContext.WithNonOverlapping());

            [Test, RunInApplicationDomain]
            public void WithPartitioned() => RegressionRunner.Run(_session, InfraTableContext.WithPartitioned());
        }

        /// <summary>
        /// Auto-test(s): InfraTableCountMinSketch
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableCountMinSketch.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableCountMinSketch : AbstractTestBase
        {
            public TestInfraTableCountMinSketch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraTableCountMinSketch.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNonStringType() => RegressionRunner.Run(_session, InfraTableCountMinSketch.WithNonStringType());

            [Test, RunInApplicationDomain]
            public void WithDocSamples() => RegressionRunner.Run(_session, InfraTableCountMinSketch.WithDocSamples());

            [Test, RunInApplicationDomain]
            public void WithFrequencyAndTopk() => RegressionRunner.Run(_session, InfraTableCountMinSketch.WithFrequencyAndTopk());
        }

        /// <summary>
        /// Auto-test(s): InfraTableDocSamples
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableDocSamples.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableDocSamples : AbstractTestBase
        {
            public TestInfraTableDocSamples() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDoc() => RegressionRunner.Run(_session, InfraTableDocSamples.WithDoc());

            [Test, RunInApplicationDomain]
            public void WithIncreasingUseCase() => RegressionRunner.Run(_session, InfraTableDocSamples.WithIncreasingUseCase());
        }

        /// <summary>
        /// Auto-test(s): InfraTableFAFExecuteQuery
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableFAFExecuteQuery.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableFAFExecuteQuery : AbstractTestBase
        {
            public TestInfraTableFAFExecuteQuery() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSelect() => RegressionRunner.Run(_session, InfraTableFAFExecuteQuery.WithSelect());

            [Test, RunInApplicationDomain]
            public void WithUpdate() => RegressionRunner.Run(_session, InfraTableFAFExecuteQuery.WithUpdate());

            [Test, RunInApplicationDomain]
            public void WithDelete() => RegressionRunner.Run(_session, InfraTableFAFExecuteQuery.WithDelete());

            [Test, RunInApplicationDomain]
            public void WithInsert() => RegressionRunner.Run(_session, InfraTableFAFExecuteQuery.WithInsert());
        }

        /// <summary>
        /// Auto-test(s): InfraTableInsertInto
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableInsertInto.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableInsertInto : AbstractTestBase
        {
            public TestInfraTableInsertInto() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSplitStream() => RegressionRunner.Run(_session, InfraTableInsertInto.WithSplitStream());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoSameModuleKeyed() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoSameModuleKeyed());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoFromNamedWindow() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoFromNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoWildcard() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoWildcard());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowMergeInsertIntoTable() => RegressionRunner.Run(_session, InfraTableInsertInto.WithNamedWindowMergeInsertIntoTable());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoSelfAccess() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoSelfAccess());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoTwoModulesUnkeyed() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoTwoModulesUnkeyed());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoSameModuleUnkeyed() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoSameModuleUnkeyed());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoAndDelete() => RegressionRunner.Run(_session, InfraTableInsertInto.WithInsertIntoAndDelete());
        }

        /// <summary>
        /// Auto-test(s): InfraTableIntoTable
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableIntoTable.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableIntoTable : AbstractTestBase
        {
            public TestInfraTableIntoTable() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIntoTableMultikeyWArrayTwoKeyed() => RegressionRunner.Run(_session, InfraTableIntoTable.WithIntoTableMultikeyWArrayTwoKeyed());

            [Test, RunInApplicationDomain]
            public void WithIntoTableMultikeyWArraySingleArrayKeyed() => RegressionRunner.Run(
                _session,
                InfraTableIntoTable.WithIntoTableMultikeyWArraySingleArrayKeyed());

            [Test, RunInApplicationDomain]
            public void WithTableBigNumberAggregation() => RegressionRunner.Run(_session, InfraTableIntoTable.WithTableBigNumberAggregation());

            [Test, RunInApplicationDomain]
            public void WithTableIntoTableWithKeys() => RegressionRunner.Run(_session, InfraTableIntoTable.WithTableIntoTableWithKeys());

            [Test, RunInApplicationDomain]
            public void WithTableIntoTableNoKeys() => RegressionRunner.Run(_session, InfraTableIntoTable.WithTableIntoTableNoKeys());

            [Test, RunInApplicationDomain]
            public void WithIntoTableWindowSortedFromJoin() => RegressionRunner.Run(_session, InfraTableIntoTable.WithIntoTableWindowSortedFromJoin());

            [Test, RunInApplicationDomain]
            public void WithBoundUnbound() => RegressionRunner.Run(_session, InfraTableIntoTable.WithBoundUnbound());

            [Test, RunInApplicationDomain]
            public void WithIntoTableUnkeyedSimpleTwoModule() => RegressionRunner.Run(_session, InfraTableIntoTable.WithIntoTableUnkeyedSimpleTwoModule());

            [Test, RunInApplicationDomain]
            public void WithIntoTableUnkeyedSimpleSameModule() => RegressionRunner.Run(_session, InfraTableIntoTable.WithIntoTableUnkeyedSimpleSameModule());
        }

        /// <summary>
        /// Auto-test(s): InfraTableInvalid
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableInvalid.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableInvalid : AbstractTestBase
        {
            public TestInfraTableInvalid() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraTableInvalid.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithInvalidAnnotations() => RegressionRunner.Run(_session, InfraTableInvalid.WithInvalidAnnotations());

            [Test, RunInApplicationDomain]
            public void WithInvalidAggMatchMultiFunc() => RegressionRunner.Run(_session, InfraTableInvalid.WithInvalidAggMatchMultiFunc());

            [Test, RunInApplicationDomain]
            public void WithInvalidAggMatchSingleFunc() => RegressionRunner.Run(_session, InfraTableInvalid.WithInvalidAggMatchSingleFunc());
        }

        /// <summary>
        /// Auto-test(s): InfraTableJoin
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableJoin.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableJoin : AbstractTestBase
        {
            public TestInfraTableJoin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOuterJoin() => RegressionRunner.Run(_session, InfraTableJoin.WithOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithUnkeyedTable() => RegressionRunner.Run(_session, InfraTableJoin.WithUnkeyedTable());

            [Test, RunInApplicationDomain]
            public void WithCoercion() => RegressionRunner.Run(_session, InfraTableJoin.WithCoercion());

            [Test, RunInApplicationDomain]
            public void WithJoinIndexChoice() => RegressionRunner.Run(_session, InfraTableJoin.WithJoinIndexChoice());

            [Test, RunInApplicationDomain]
            public void WithFromClause() => RegressionRunner.Run(_session, InfraTableJoin.WithFromClause());
        }

        /// <summary>
        /// Auto-test(s): InfraTableOnDelete
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableOnDelete.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableOnDelete : AbstractTestBase
        {
            public TestInfraTableOnDelete() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSecondaryIndexUpd() => RegressionRunner.Run(_session, InfraTableOnDelete.WithSecondaryIndexUpd());

            [Test, RunInApplicationDomain]
            public void WithFlow() => RegressionRunner.Run(_session, InfraTableOnDelete.WithFlow());
        }

        /// <summary>
        /// Auto-test(s): InfraTableOnMerge
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableOnMerge.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableOnMerge : AbstractTestBase
        {
            public TestInfraTableOnMerge() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTableArrayAssignmentBoxed() => RegressionRunner.Run(_session, InfraTableOnMerge.WithTableArrayAssignmentBoxed());

            [Test, RunInApplicationDomain]
            public void WithTableEMACompute() => RegressionRunner.Run(_session, InfraTableOnMerge.WithTableEMACompute());

            [Test, RunInApplicationDomain]
            public void WithMergeTwoTables() => RegressionRunner.Run(_session, InfraTableOnMerge.WithMergeTwoTables());

            [Test, RunInApplicationDomain]
            public void WithMergeSelectWithAggReadAndEnum() => RegressionRunner.Run(_session, InfraTableOnMerge.WithMergeSelectWithAggReadAndEnum());

            [Test, RunInApplicationDomain]
            public void WithMergeWhereWithMethodRead() => RegressionRunner.Run(_session, InfraTableOnMerge.WithMergeWhereWithMethodRead());

            [Test, RunInApplicationDomain]
            public void WithOnMergePlainPropsAnyKeyed() => RegressionRunner.Run(_session, InfraTableOnMerge.WithOnMergePlainPropsAnyKeyed());

            [Test, RunInApplicationDomain]
            public void WithTableOnMergeSimple() => RegressionRunner.Run(_session, InfraTableOnMerge.WithTableOnMergeSimple());
        }

        /// <summary>
        /// Auto-test(s): InfraTableOnUpdate
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableOnUpdate.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableOnUpdate : AbstractTestBase
        {
            public TestInfraTableOnUpdate() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayTwoArrayNonGetter() => RegressionRunner.Run(_session, InfraTableOnUpdate.WithMultikeyWArrayTwoArrayNonGetter());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayTwoArray() => RegressionRunner.Run(_session, InfraTableOnUpdate.WithMultikeyWArrayTwoArray());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayOneArray() => RegressionRunner.Run(_session, InfraTableOnUpdate.WithMultikeyWArrayOneArray());

            [Test, RunInApplicationDomain]
            public void WithTwoKey() => RegressionRunner.Run(_session, InfraTableOnUpdate.WithTwoKey());
        }

        /// <summary>
        /// Auto-test(s): InfraTablePlugInAggregation
        /// <code>
        /// RegressionRunner.Run(_session, InfraTablePlugInAggregation.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTablePlugInAggregation : AbstractTestBase
        {
            public TestInfraTablePlugInAggregation() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAccessRefCountedMap() => RegressionRunner.Run(_session, InfraTablePlugInAggregation.WithAccessRefCountedMap());

            [Test, RunInApplicationDomain]
            public void WithAggMethodCSVLast3Strings() => RegressionRunner.Run(_session, InfraTablePlugInAggregation.WithAggMethodCSVLast3Strings());
        }

        /// <summary>
        /// Auto-test(s): InfraTableRollup
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableRollup.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableRollup : AbstractTestBase
        {
            public TestInfraTableRollup() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupingSetThreeDim() => RegressionRunner.Run(_session, InfraTableRollup.WithGroupingSetThreeDim());

            [Test, RunInApplicationDomain]
            public void WithRollupTwoDim() => RegressionRunner.Run(_session, InfraTableRollup.WithRollupTwoDim());

            [Test, RunInApplicationDomain]
            public void WithRollupOneDim() => RegressionRunner.Run(_session, InfraTableRollup.WithRollupOneDim());
        }

        /// <summary>
        /// Auto-test(s): InfraTableSelect
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableSelect.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableSelect : AbstractTestBase
        {
            public TestInfraTableSelect() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayComposite() => RegressionRunner.Run(
                _session,
                InfraTableSelect.WithMultikeyWArrayComposite());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayTwoArray() => RegressionRunner.Run(
                _session,
                InfraTableSelect.WithMultikeyWArrayTwoArray());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArraySingleArray() => RegressionRunner.Run(
                _session,
                InfraTableSelect.WithMultikeyWArraySingleArray());

            [Test, RunInApplicationDomain]
            public void WithEnum() => RegressionRunner.Run(
                _session,
                InfraTableSelect.WithEnum());

            [Test, RunInApplicationDomain]
            public void WithStarPublicTypeVisibility() => RegressionRunner.Run(
                _session,
                InfraTableSelect.WithStarPublicTypeVisibility());
        }

        /// <summary>
        /// Auto-test(s): InfraTableSubquery
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableSubquery.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableSubquery : AbstractTestBase
        {
            public TestInfraTableSubquery() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInFilter() => RegressionRunner.Run(_session, InfraTableSubquery.WithInFilter());

            [Test, RunInApplicationDomain]
            public void WithSecondaryIndex() => RegressionRunner.Run(_session, InfraTableSubquery.WithSecondaryIndex());

            [Test, RunInApplicationDomain]
            public void WithAgainstUnkeyed() => RegressionRunner.Run(_session, InfraTableSubquery.WithAgainstUnkeyed());

            [Test, RunInApplicationDomain]
            public void WithAgainstKeyed() => RegressionRunner.Run(_session, InfraTableSubquery.WithAgainstKeyed());
        }

        /// <summary>
        /// Auto-test(s): InfraTableUpdateAndIndex
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableUpdateAndIndex.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableUpdateAndIndex : AbstractTestBase
        {
            public TestInfraTableUpdateAndIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTableKeyUpdateMultiKey() => RegressionRunner.Run(_session, InfraTableUpdateAndIndex.WithTableKeyUpdateMultiKey());

            [Test, RunInApplicationDomain]
            public void WithTableKeyUpdateSingleKey() => RegressionRunner.Run(_session, InfraTableUpdateAndIndex.WithTableKeyUpdateSingleKey());

            [Test, RunInApplicationDomain]
            public void WithFAFUpdate() => RegressionRunner.Run(_session, InfraTableUpdateAndIndex.WithFAFUpdate());

            [Test, RunInApplicationDomain]
            public void WithLateUniqueIndexViolation() => RegressionRunner.Run(_session, InfraTableUpdateAndIndex.WithLateUniqueIndexViolation());

            [Test, RunInApplicationDomain]
            public void WithEarlyUniqueIndexViolation() => RegressionRunner.Run(_session, InfraTableUpdateAndIndex.WithEarlyUniqueIndexViolation());
        }

        /// <summary>
        /// Auto-test(s): InfraTableResetAggregationState
        /// <code>
        /// RegressionRunner.Run(_session, InfraTableResetAggregationState.Executions());
        /// </code>
        /// </summary>

        public class TestInfraTableResetAggregationState : AbstractTestBase
        {
            public TestInfraTableResetAggregationState() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithVariousAggs() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithVariousAggs());

            [Test, RunInApplicationDomain]
            public void WithSelective() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithSelective());

            [Test, RunInApplicationDomain]
            public void WithRowSumWTableAlias() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithRowSumWTableAlias());

            [Test, RunInApplicationDomain]
            public void WithRowSum() => RegressionRunner.Run(_session, InfraTableResetAggregationState.WithRowSum());
        }
    }
} // end of namespace