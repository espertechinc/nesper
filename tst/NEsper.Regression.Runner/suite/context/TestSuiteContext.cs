///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.context;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.context
{
    [TestFixture]
    public class TestSuiteContext : AbstractTestBase
    {
        public TestSuiteContext() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                         typeof(SupportBean),
                         typeof(SupportBean_S0),
                         typeof(SupportBean_S1),
                         typeof(SupportBean_S2),
                         typeof(SupportBean_S3),
                         typeof(ISupportBaseAB),
                         typeof(ISupportA),
                         typeof(SupportWebEvent),
                         typeof(ISupportAImpl),
                         typeof(SupportGroupSubgroupEvent),
                         typeof(SupportEventWithIntArray)
                     }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }
            
            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            
            configuration.Common.AddEventType(typeof(ContextDocExamples.BankTxn));
            configuration.Common.AddEventType(typeof(ContextDocExamples.LoginEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.LogoutEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.SecurityEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.SensorEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrafficEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrainEnterEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrainLeaveEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.CumulativePrice));
            configuration.Common.AddEventType(typeof(ContextDocExamples.PassengerScanEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyInitEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTermEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyEvent));
            configuration.Common.AddEventType("StartEventOne", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("StartEventTwo", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("MyOtherEvent", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("EndEventOne", typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType("EndEventTwo", typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTwoKeyInit));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTwoKeyTerm));

            configuration.Compiler.AddPlugInSingleRowFunction("myHash", typeof(ContextHashSegmented), "MyHashFunc");
            configuration.Compiler.AddPlugInSingleRowFunction("mySecond", typeof(ContextHashSegmented), "MySecondFunc");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "makeBean",
                typeof(ContextInitTermTemporalFixed),
                "SingleRowPluginMakeBean");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "toArray",
                typeof(ContextKeySegmentedAggregate),
                "ToArray");

            configuration.Compiler.AddPlugInSingleRowFunction(
                "customEnabled",
                typeof(ContextNested),
                "CustomMatch",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "customDisabled",
                typeof(ContextNested),
                "CustomMatch",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "stringContainsX",
                typeof(ContextKeySegmented),
                "StringContainsX");

            configuration.Common.AddImportType(typeof(ContextHashSegmented));

            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configuration.Common.AddDatabaseReference("MyDB", configDB);

            configuration.Compiler.ByteCode.IsAllowSubscriber =true;
            configuration.Compiler.AddPlugInVirtualDataWindow(
                "test",
                "vdw",
                typeof(SupportVirtualDWForge),
                SupportVirtualDW.ITERATE); // configure with iteration
        }

        [Test, RunInApplicationDomain]
        public void TestContextDocExamples()
        {
            RegressionRunner.Run(_session, new ContextDocExamples());
        }

        /// <summary>
        /// Auto-test(s): ContextHashSegmented
        /// <code>
        /// RegressionRunner.Run(_session, ContextHashSegmented.Executions());
        /// </code>
        /// </summary>

        public class TestContextHashSegmented : AbstractTestBase
        {
            public TestContextHashSegmented() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextHashSegmented.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithPartitionSelection() => RegressionRunner.Run(_session, ContextHashSegmented.WithPartitionSelection());

            [Test, RunInApplicationDomain]
            public void WithScoringUseCase() => RegressionRunner.Run(_session, ContextHashSegmented.WithScoringUseCase());

            [Test, RunInApplicationDomain]
            public void WithSegmentedBySingleRowFunc() => RegressionRunner.Run(_session, ContextHashSegmented.WithSegmentedBySingleRowFunc());

            [Test, RunInApplicationDomain]
            public void WithSegmentedMulti() => RegressionRunner.Run(_session, ContextHashSegmented.WithSegmentedMulti());

            [Test, RunInApplicationDomain]
            public void WithSegmentedManyArg() => RegressionRunner.Run(_session, ContextHashSegmented.WithSegmentedManyArg());

            [Test, RunInApplicationDomain]
            public void WithNoPreallocate() => RegressionRunner.Run(_session, ContextHashSegmented.WithNoPreallocate());

            [Test, RunInApplicationDomain]
            public void WithSegmentedFilter() => RegressionRunner.Run(_session, ContextHashSegmented.WithSegmentedFilter());

            [Test, RunInApplicationDomain]
            public void WithSegmentedBasic() => RegressionRunner.Run(_session, ContextHashSegmented.WithSegmentedBasic());
        }

        /// <summary>
        /// Auto-test(s): ContextInitTermTemporalFixed
        /// <code>
        /// RegressionRunner.Run(_session, ContextInitTermTemporalFixed.Executions());
        /// </code>
        /// </summary>

        public class TestContextInitTermTemporalFixed : AbstractTestBase
        {
            public TestContextInitTermTemporalFixed() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEndMultiCrontab() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndMultiCrontab());

            [Test, RunInApplicationDomain]
            [Category("DatabaseTest")]
            [Category("IntegrationTest")]
            public void WithEndDBHistorical() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndDBHistorical());

            [Test, RunInApplicationDomain]
            public void With9End5AggGrouped() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.With9End5AggGrouped());

            [Test, RunInApplicationDomain]
            public void With9End5AggUngrouped() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.With9End5AggUngrouped());

            [Test, RunInApplicationDomain]
            public void WithEndStartTurnedOn() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndStartTurnedOn());

            [Test, RunInApplicationDomain]
            public void WithEndStartTurnedOff() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndStartTurnedOff());

            [Test, RunInApplicationDomain]
            public void WithEndNWFireAndForget() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndNWFireAndForget());

            [Test, RunInApplicationDomain]
            public void WithEndNWSameContextOnExpr() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndNWSameContextOnExpr());

            [Test, RunInApplicationDomain]
            public void WithEndSubselectCorrelated() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndSubselectCorrelated());

            [Test, RunInApplicationDomain]
            public void WithEndSubselect() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndSubselect());

            [Test, RunInApplicationDomain]
            public void WithEndPatternWithTime() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndPatternWithTime());

            [Test, RunInApplicationDomain]
            public void WithEndJoin() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndJoin());

            [Test, RunInApplicationDomain]
            public void WithEndPrevPriorAndAggregation() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndPrevPriorAndAggregation());

            [Test, RunInApplicationDomain]
            public void WithEndContextCreateDestroy() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndContextCreateDestroy());

            [Test, RunInApplicationDomain]
            public void WithEndPatternStartedPatternEnded() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndPatternStartedPatternEnded());

            [Test, RunInApplicationDomain]
            public void WithEndFilterStartedFilterEndedOutputSnapshot() => RegressionRunner.Run(
                _session,
                ContextInitTermTemporalFixed.WithEndFilterStartedFilterEndedOutputSnapshot());

            [Test, RunInApplicationDomain]
            public void WithEndStartAfterEndAfter() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndStartAfterEndAfter());

            [Test, RunInApplicationDomain]
            public void WithEndFilterStartedPatternEndedCorrelated() => RegressionRunner.Run(
                _session,
                ContextInitTermTemporalFixed.WithEndFilterStartedPatternEndedCorrelated());

            [Test, RunInApplicationDomain]
            public void WithEndFilterStartedFilterEndedCorrelatedOutputSnapshot() => RegressionRunner.Run(
                _session,
                ContextInitTermTemporalFixed.WithEndFilterStartedFilterEndedCorrelatedOutputSnapshot());

            [Test, RunInApplicationDomain]
            public void WithEndContextPartitionSelection() => RegressionRunner.Run(_session, ContextInitTermTemporalFixed.WithEndContextPartitionSelection());
        }

        /// <summary>
        /// Auto-test(s): ContextInitTerm
        /// <code>
        /// RegressionRunner.Run(_session, ContextInitTerm.Executions());
        /// </code>
        /// </summary>

        public class TestContextInitTerm : AbstractTestBase
        {
            public TestContextInitTerm() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInitTermPatternCorrelated() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermPatternCorrelated());

            [Test, RunInApplicationDomain]
            public void WithStartEndPatternCorrelated() => RegressionRunner.Run(_session, ContextInitTerm.WithStartEndPatternCorrelated());

            [Test, RunInApplicationDomain]
            public void WithInitTermPrevPrior() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermPrevPrior());

            [Test, RunInApplicationDomain]
            public void WithInitTermAggregationGrouped() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermAggregationGrouped());

            [Test, RunInApplicationDomain]
            public void WithStartEndStartNowCalMonthScoped() => RegressionRunner.Run(_session, ContextInitTerm.WithStartEndStartNowCalMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithInitTermCrontab() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermCrontab());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputOnlyWhenTerminatedThenSet() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermOutputOnlyWhenTerminatedThenSet());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputOnlyWhenSetAndWhenTerminatedSet() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermOutputOnlyWhenSetAndWhenTerminatedSet());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputOnlyWhenTerminatedCondition() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermOutputOnlyWhenTerminatedCondition());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputWhenExprWhenTerminatedCondition() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermOutputWhenExprWhenTerminatedCondition());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputAllEvery2AndTerminated() =>
                RegressionRunner.Run(_session, ContextInitTerm.WithInitTermOutputAllEvery2AndTerminated());

            [Test, RunInApplicationDomain]
            public void WithInitTermOutputSnapshotWhenTerminated() =>
                RegressionRunner.Run(_session, ContextInitTerm.WithInitTermOutputSnapshotWhenTerminated());

            [Test, RunInApplicationDomain]
            public void WithInitTermTerminateTwoContextSameTime() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermTerminateTwoContextSameTime());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterBooleanOperator() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermFilterBooleanOperator());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterAllOperators() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermFilterAllOperators());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterInitiatedStraightEquals() =>
                RegressionRunner.Run(_session, ContextInitTerm.WithInitTermFilterInitiatedStraightEquals());

            [Test, RunInApplicationDomain]
            public void WithInitTermPatternInitiatedStraightSelect() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermPatternInitiatedStraightSelect());

            [Test, RunInApplicationDomain]
            public void WithInitTermPatternInclusion() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermPatternInclusion());

            [Test, RunInApplicationDomain]
            public void WithInitTermPatternIntervalZeroInitiatedNow() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermPatternIntervalZeroInitiatedNow());

            [Test, RunInApplicationDomain]
            public void WithInitTermScheduleFilterResources() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermScheduleFilterResources());

            [Test, RunInApplicationDomain]
            public void WithInitTermPatternAndAfter1Min() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermPatternAndAfter1Min());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterAndPattern() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermFilterAndPattern());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterAndAfter1Min() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermFilterAndAfter1Min());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot());

            [Test, RunInApplicationDomain]
            public void WithInitTermFilterInitiatedFilterAllTerminated() => RegressionRunner.Run(
                _session,
                ContextInitTerm.WithInitTermFilterInitiatedFilterAllTerminated());

            [Test, RunInApplicationDomain]
            public void WithInitTermContextPartitionSelection() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermContextPartitionSelection());

            [Test, RunInApplicationDomain]
            public void WithStartEndEndSameEventAsAnalyzed() => RegressionRunner.Run(_session, ContextInitTerm.WithStartEndEndSameEventAsAnalyzed());

            [Test, RunInApplicationDomain]
            public void WithStartEndAfterZeroInitiatedNow() => RegressionRunner.Run(_session, ContextInitTerm.WithStartEndAfterZeroInitiatedNow());

            [Test, RunInApplicationDomain]
            public void WithStartEndNoTerminationCondition() => RegressionRunner.Run(_session, ContextInitTerm.WithStartEndNoTerminationCondition());

            [Test, RunInApplicationDomain]
            public void WithInitTermNoTerminationCondition() => RegressionRunner.Run(_session, ContextInitTerm.WithInitTermNoTerminationCondition());
        }

        /// <summary>
        /// Auto-test(s): ContextKeySegmentedAggregate
        /// <code>
        /// RegressionRunner.Run(_session, ContextKeySegmentedAggregate.Executions());
        /// </code>
        /// </summary>

        public class TestContextKeySegmentedAggregate : AbstractTestBase
        {
            public TestContextKeySegmentedAggregate() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRowPerGroup3Stmts() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerGroup3Stmts());

            [Test, RunInApplicationDomain]
            public void WithRowPerEvent() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerEvent());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupUnidirectionalJoin() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerGroupUnidirectionalJoin());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupWithAccess() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerGroupWithAccess());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupBatchContextProp() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerGroupBatchContextProp());

            [Test, RunInApplicationDomain]
            public void WithRowPerGroupStream() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowPerGroupStream());

            [Test, RunInApplicationDomain]
            public void WithSubqueryWithAggregation() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithSubqueryWithAggregation());

            [Test, RunInApplicationDomain]
            public void WithAccessOnly() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithAccessOnly());

            [Test, RunInApplicationDomain]
            public void WithRowForAll() => RegressionRunner.Run(_session, ContextKeySegmentedAggregate.WithRowForAll());
        }

        /// <summary>
        /// Auto-test(s): ContextKeySegmentedInfra
        /// <code>
        /// RegressionRunner.Run(_session, ContextKeySegmentedInfra.Executions());
        /// </code>
        /// </summary>

        public class TestContextKeySegmentedInfra : AbstractTestBase
        {
            public TestContextKeySegmentedInfra() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithedSegmentedTable() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithedSegmentedTable());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraOnMergeUpdateSubq() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithSegmentedInfraOnMergeUpdateSubq());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraNWConsumeSameContext() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedInfra.WithSegmentedInfraNWConsumeSameContext());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraNWConsumeAll() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithSegmentedInfraNWConsumeAll());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraOnSelect() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithSegmentedInfraOnSelect());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraCreateIndex() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithSegmentedInfraCreateIndex());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraOnDeleteAndUpdate() => RegressionRunner.Run(_session, ContextKeySegmentedInfra.WithSegmentedInfraOnDeleteAndUpdate());

            [Test, RunInApplicationDomain]
            public void WithSegmentedInfraAggregatedSubquery() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedInfra.WithSegmentedInfraAggregatedSubquery());
        }

        /// <summary>
        /// Auto-test(s): ContextAdminListen
        /// <code>
        /// RegressionRunner.Run(_session, ContextAdminListen.Executions());
        /// </code>
        /// </summary>

        public class TestContextAdminListen : AbstractTestBase
        {
            public TestContextAdminListen() : base(Configure)
            {
            }

            protected override bool UseDefaultRuntime => true;

            [Test, RunInApplicationDomain]
            public void WithMinListenMultipleStatements() => RegressionRunner.Run(_session, ContextAdminListen.WithMinListenMultipleStatements());

            [Test, RunInApplicationDomain]
            public void WithMinPartitionAddRemoveListener() => RegressionRunner.Run(_session, ContextAdminListen.WithMinPartitionAddRemoveListener());

            [Test, RunInApplicationDomain]
            public void WithAddRemoveListener() => RegressionRunner.Run(_session, ContextAdminListen.WithAddRemoveListener());

            [Test, RunInApplicationDomain]
            public void WithMinListenNested() => RegressionRunner.Run(_session, ContextAdminListen.WithMinListenNested());

            [Test, RunInApplicationDomain]
            public void WithMinListenCategory() => RegressionRunner.Run(_session, ContextAdminListen.WithMinListenCategory());

            [Test, RunInApplicationDomain]
            public void WithMinListenHash() => RegressionRunner.Run(_session, ContextAdminListen.WithMinListenHash());

            [Test, RunInApplicationDomain]
            public void WithMinListenInitTerm() => RegressionRunner.Run(_session, ContextAdminListen.WithMinListenInitTerm());
        }

        /// <summary>
        /// Auto-test(s): ContextCategory
        /// <code>
        /// RegressionRunner.Run(_session, ContextCategory.Executions());
        /// </code>
        /// </summary>

        public class TestContextCategory : AbstractTestBase
        {
            public TestContextCategory() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDeclaredExpr() => RegressionRunner.Run(_session, ContextCategory.WithDeclaredExpr());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextCategory.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSingleCategorySODAPrior() => RegressionRunner.Run(_session, ContextCategory.WithSingleCategorySODAPrior());

            [Test, RunInApplicationDomain]
            public void WithContextPartitionSelection() => RegressionRunner.Run(_session, ContextCategory.WithContextPartitionSelection());

            [Test, RunInApplicationDomain]
            public void WithBooleanExprFilter() => RegressionRunner.Run(_session, ContextCategory.WithBooleanExprFilter());

            [Test, RunInApplicationDomain]
            public void WithWContextProps() => RegressionRunner.Run(_session, ContextCategory.WithWContextProps());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.Run(_session, ContextCategory.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ContextCategory.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ContextInitTermWithNow
        /// <code>
        /// RegressionRunner.Run(_session, ContextInitTermWithNow.Executions());
        /// </code>
        /// </summary>

        public class TestContextInitTermWithNow : AbstractTestBase
        {
            public TestContextInitTermWithNow() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInitTermWNowInvalid() => RegressionRunner.Run(_session, ContextInitTermWithNow.WithInitTermWNowInvalid());

            [Test, RunInApplicationDomain]
            public void WithInitTermWithPattern() => RegressionRunner.Run(_session, ContextInitTermWithNow.WithInitTermWithPattern());

            [Test, RunInApplicationDomain]
            public void WithStartStopWNow() => RegressionRunner.Run(_session, ContextInitTermWithNow.WithStartStopWNow());
        }

        /// <summary>
        /// Auto-test(s): ContextKeySegmented
        /// <code>
        /// RegressionRunner.Run(_session, ContextKeySegmented.Executions());
        /// </code>
        /// </summary>

        public class TestContextKeySegmented : AbstractTestBase
        {
            public TestContextKeySegmented() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWPatternFireWhenAllocated() => RegressionRunner.Run(_session, ContextKeySegmented.WithWPatternFireWhenAllocated());

            [Test, RunInApplicationDomain]
            public void WithWInitTermEndEvent() => RegressionRunner.Run(_session, ContextKeySegmented.WithWInitTermEndEvent());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayTwoField() => RegressionRunner.Run(_session, ContextKeySegmented.WithMultikeyWArrayTwoField());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayOfPrimitive() => RegressionRunner.Run(_session, ContextKeySegmented.WithMultikeyWArrayOfPrimitive());

            [Test, RunInApplicationDomain]
            public void WithMatchRecognize() => RegressionRunner.Run(_session, ContextKeySegmented.WithMatchRecognize());

            [Test, RunInApplicationDomain]
            public void WithTermByFilter() => RegressionRunner.Run(_session, ContextKeySegmented.WithTermByFilter());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextKeySegmented.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNullKeyMultiKey() => RegressionRunner.Run(_session, ContextKeySegmented.WithNullKeyMultiKey());

            [Test, RunInApplicationDomain]
            public void WithNullSingleKey() => RegressionRunner.Run(_session, ContextKeySegmented.WithNullSingleKey());

            [Test, RunInApplicationDomain]
            public void WithJoinWhereClauseOnPartitionKey() => RegressionRunner.Run(_session, ContextKeySegmented.WithJoinWhereClauseOnPartitionKey());

            [Test, RunInApplicationDomain]
            public void WithViewSceneTwo() => RegressionRunner.Run(_session, ContextKeySegmented.WithViewSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithViewSceneOne() => RegressionRunner.Run(_session, ContextKeySegmented.WithViewSceneOne());

            [Test, RunInApplicationDomain]
            public void WithPatternSceneTwo() => RegressionRunner.Run(_session, ContextKeySegmented.WithPatternSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithPattern() => RegressionRunner.Run(_session, ContextKeySegmented.WithPattern());

            [Test, RunInApplicationDomain]
            public void WithJoin() => RegressionRunner.Run(_session, ContextKeySegmented.WithJoin());

            [Test, RunInApplicationDomain]
            public void WithSubqueryFiltered() => RegressionRunner.Run(_session, ContextKeySegmented.WithSubqueryFiltered());

            [Test, RunInApplicationDomain]
            public void WithPrior() => RegressionRunner.Run(_session, ContextKeySegmented.WithPrior());

            [Test, RunInApplicationDomain]
            public void WithSubselectPrevPrior() => RegressionRunner.Run(_session, ContextKeySegmented.WithSubselectPrevPrior());

            [Test, RunInApplicationDomain]
            public void WithJoinMultitypeMultifield() => RegressionRunner.Run(_session, ContextKeySegmented.WithJoinMultitypeMultifield());

            [Test, RunInApplicationDomain]
            public void WithSubtype() => RegressionRunner.Run(_session, ContextKeySegmented.WithSubtype());

            [Test, RunInApplicationDomain]
            public void WithMultiStatementFilterCount() => RegressionRunner.Run(_session, ContextKeySegmented.WithMultiStatementFilterCount());

            [Test, RunInApplicationDomain]
            public void WithAdditionalFilters() => RegressionRunner.Run(_session, ContextKeySegmented.WithAdditionalFilters());

            [Test, RunInApplicationDomain]
            public void WithLargeNumberPartitions() => RegressionRunner.Run(_session, ContextKeySegmented.WithLargeNumberPartitions());

            [Test, RunInApplicationDomain]
            public void WithSelector() => RegressionRunner.Run(_session, ContextKeySegmented.WithSelector());

            [Test, RunInApplicationDomain]
            public void WithJoinRemoveStream() => RegressionRunner.Run(_session, ContextKeySegmented.WithJoinRemoveStream());

            [Test, RunInApplicationDomain]
            public void WithPatternFilter() => RegressionRunner.Run(_session, ContextKeySegmented.WithPatternFilter());
        }

        /// <summary>
        /// Auto-test(s): ContextKeySegmentedNamedWindow
        /// <code>
        /// RegressionRunner.Run(_session, ContextKeySegmentedNamedWindow.Executions());
        /// </code>
        /// </summary>

        public class TestContextKeySegmentedNamedWindow : AbstractTestBase
        {
            public TestContextKeySegmentedNamedWindow() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSubqueryNamedWindowIndexShared() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedNamedWindow.WithSubqueryNamedWindowIndexShared());

            [Test, RunInApplicationDomain]
            public void WithSubqueryNamedWindowIndexUnShared() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedNamedWindow.WithSubqueryNamedWindowIndexUnShared());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowFAF() => RegressionRunner.Run(_session, ContextKeySegmentedNamedWindow.WithNamedWindowFAF());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowPattern() => RegressionRunner.Run(_session, ContextKeySegmentedNamedWindow.WithNamedWindowPattern());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowNonPattern() => RegressionRunner.Run(_session, ContextKeySegmentedNamedWindow.WithNamedWindowNonPattern());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowBasic() => RegressionRunner.Run(_session, ContextKeySegmentedNamedWindow.WithNamedWindowBasic());
        }

        /// <summary>
        /// Auto-test(s): ContextLifecycle
        /// <code>
        /// RegressionRunner.Run(_session, ContextLifecycle.Executions());
        /// </code>
        /// </summary>

        public class TestContextLifecycle : AbstractTestBase
        {
            public TestContextLifecycle() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ContextLifecycle.WithSimple());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextLifecycle.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNWOtherContextOnExpr() => RegressionRunner.Run(_session, ContextLifecycle.WithNWOtherContextOnExpr());

            [Test, RunInApplicationDomain]
            public void WithVirtualDataWindow() => RegressionRunner.Run(_session, ContextLifecycle.WithVirtualDataWindow());

            [Test, RunInApplicationDomain]
            public void WithSplitStream() => RegressionRunner.Run(_session, ContextLifecycle.WithSplitStream());
        }

        /// <summary>
        /// Auto-test(s): ContextNested
        /// <code>
        /// RegressionRunner.Run(_session, ContextNested.Executions());
        /// </code>
        /// </summary>

        public class TestContextNested : AbstractTestBase
        {
            public TestContextNested() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithKeySegmentedWInitTermEndEvent() => RegressionRunner.Run(_session, ContextNested.WithKeySegmentedWInitTermEndEvent());

            [Test, RunInApplicationDomain]
            public void WithCategoryOverInitTermDistinct() => RegressionRunner.Run(_session, ContextNested.WithCategoryOverInitTermDistinct());

            [Test, RunInApplicationDomain]
            public void WithInitTermOverInitTermIterate() => RegressionRunner.Run(_session, ContextNested.WithInitTermOverInitTermIterate());

            [Test, RunInApplicationDomain]
            public void WithInitTermOverCategoryIterate() => RegressionRunner.Run(_session, ContextNested.WithInitTermOverCategoryIterate());

            [Test, RunInApplicationDomain]
            public void WithInitTermOverPartitionedIterate() => RegressionRunner.Run(_session, ContextNested.WithInitTermOverPartitionedIterate());

            [Test, RunInApplicationDomain]
            public void WithInitTermOverHashIterate() => RegressionRunner.Run(_session, ContextNested.WithInitTermOverHashIterate());

            [Test, RunInApplicationDomain]
            public void WithInitTermWCategoryWHash() => RegressionRunner.Run(_session, ContextNested.WithInitTermWCategoryWHash());

            [Test, RunInApplicationDomain]
            public void WithNonOverlapOverNonOverlapNoEndCondition() =>
                RegressionRunner.Run(_session, ContextNested.WithNonOverlapOverNonOverlapNoEndCondition());

            [Test, RunInApplicationDomain]
            public void WithKeyedFilter() => RegressionRunner.Run(_session, ContextNested.WithKeyedFilter());

            [Test, RunInApplicationDomain]
            public void WithKeyedStartStop() => RegressionRunner.Run(_session, ContextNested.WithKeyedStartStop());

            [Test, RunInApplicationDomain]
            public void WithInitWStartNowSceneTwo() => RegressionRunner.Run(_session, ContextNested.WithInitWStartNowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithInitWStartNow() => RegressionRunner.Run(_session, ContextNested.WithInitWStartNow());

            [Test, RunInApplicationDomain]
            public void WithPartitionedOverPatternInitiated() => RegressionRunner.Run(_session, ContextNested.WithPartitionedOverPatternInitiated());

            [Test, RunInApplicationDomain]
            public void WithNonOverlapping() => RegressionRunner.Run(_session, ContextNested.WithNonOverlapping());

            [Test, RunInApplicationDomain]
            public void WithOverlappingAndPattern() => RegressionRunner.Run(_session, ContextNested.WithOverlappingAndPattern());

            [Test, RunInApplicationDomain]
            public void WithPartitionWithMultiPropsAndTerm() => RegressionRunner.Run(_session, ContextNested.WithPartitionWithMultiPropsAndTerm());

            [Test, RunInApplicationDomain]
            public void WithLateComingStatement() => RegressionRunner.Run(_session, ContextNested.WithLateComingStatement());

            [Test, RunInApplicationDomain]
            public void WithContextProps() => RegressionRunner.Run(_session, ContextNested.WithContextProps());

            [Test, RunInApplicationDomain]
            public void WithPartitionedOverFixedTemporal() => RegressionRunner.Run(_session, ContextNested.WithPartitionedOverFixedTemporal());

            [Test, RunInApplicationDomain]
            public void WithFixedTemporalOverPartitioned() => RegressionRunner.Run(_session, ContextNested.WithFixedTemporalOverPartitioned());

            [Test, RunInApplicationDomain]
            public void WithCategoryOverTemporalOverlapping() => RegressionRunner.Run(_session, ContextNested.WithCategoryOverTemporalOverlapping());

            [Test, RunInApplicationDomain]
            public void WithTemporalFixedOverHash() => RegressionRunner.Run(_session, ContextNested.WithTemporalFixedOverHash());

            [Test, RunInApplicationDomain]
            public void WithTemporalOverCategoryOverPartition() => RegressionRunner.Run(_session, ContextNested.WithTemporalOverCategoryOverPartition());

            [Test, RunInApplicationDomain]
            public void WithFourContextsNested() => RegressionRunner.Run(_session, ContextNested.WithFourContextsNested());

            [Test, RunInApplicationDomain]
            public void WithSingleEventTriggerNested() => RegressionRunner.Run(_session, ContextNested.WithSingleEventTriggerNested());

            [Test, RunInApplicationDomain]
            public void WithCategoryOverPatternInitiated() => RegressionRunner.Run(_session, ContextNested.WithCategoryOverPatternInitiated());

            [Test, RunInApplicationDomain]
            public void WithNestingFilterCorrectness() => RegressionRunner.Run(_session, ContextNested.WithNestingFilterCorrectness());

            [Test, RunInApplicationDomain]
            public void WithPartitionedWithFilterNonOverlap() => RegressionRunner.Run(_session, ContextNested.WithPartitionedWithFilterNonOverlap());

            [Test, RunInApplicationDomain]
            public void WithPartitionedWithFilterOverlap() => RegressionRunner.Run(_session, ContextNested.WithPartitionedWithFilterOverlap());

            [Test, RunInApplicationDomain]
            public void WithIterator() => RegressionRunner.Run(_session, ContextNested.WithIterator());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextNested.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithIterateTargetedCP() => RegressionRunner.Run(_session, ContextNested.WithIterateTargetedCP());

            [Test, RunInApplicationDomain]
            public void WithWithFilterUDF() => RegressionRunner.Run(_session, ContextNested.WithWithFilterUDF());
        }

        /// <summary>
        /// Auto-test(s): ContextSelectionAndFireAndForget
        /// <code>
        /// RegressionRunner.Run(_session, ContextSelectionAndFireAndForget.Executions());
        /// </code>
        /// </summary>

        public class TestContextSelectionAndFireAndForget : AbstractTestBase
        {
            public TestContextSelectionAndFireAndForget() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFAFNestedNamedWindowQuery() => RegressionRunner.Run(_session, ContextSelectionAndFireAndForget.WithFAFNestedNamedWindowQuery());

            [Test, RunInApplicationDomain]
            public void WithAndFireAndForgetNamedWindowQuery() => RegressionRunner.Run(
                _session,
                ContextSelectionAndFireAndForget.WithAndFireAndForgetNamedWindowQuery());

            [Test, RunInApplicationDomain]
            public void WithIterateStatement() => RegressionRunner.Run(_session, ContextSelectionAndFireAndForget.WithIterateStatement());

            [Test, RunInApplicationDomain]
            public void WithAndFireAndForgetInvalid() => RegressionRunner.Run(_session, ContextSelectionAndFireAndForget.WithAndFireAndForgetInvalid());
        }

        /// <summary>
        /// Auto-test(s): ContextVariables
        /// <code>
        /// RegressionRunner.Run(_session, ContextVariables.Executions());
        /// </code>
        /// </summary>

        public class TestContextVariables : AbstractTestBase
        {
            public TestContextVariables() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextVariables.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithGetSetAPI() => RegressionRunner.Run(_session, ContextVariables.WithGetSetAPI());

            [Test, RunInApplicationDomain]
            public void WithIterateAndListen() => RegressionRunner.Run(_session, ContextVariables.WithIterateAndListen());

            [Test, RunInApplicationDomain]
            public void WithOverlapping() => RegressionRunner.Run(_session, ContextVariables.WithOverlapping());

            [Test, RunInApplicationDomain]
            public void WithSegmentedByKey() => RegressionRunner.Run(_session, ContextVariables.WithSegmentedByKey());
        }

        /// <summary>
        /// Auto-test(s): ContextWDeclaredExpression
        /// <code>
        /// RegressionRunner.Run(_session, ContextWDeclaredExpression.Executions());
        /// </code>
        /// </summary>

        public class TestContextWDeclaredExpression : AbstractTestBase
        {
            public TestContextWDeclaredExpression() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWFilter() => RegressionRunner.Run(_session, ContextWDeclaredExpression.WithWFilter());

            [Test, RunInApplicationDomain]
            public void WithAlias() => RegressionRunner.Run(_session, ContextWDeclaredExpression.WithAlias());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ContextWDeclaredExpression.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): ContextInitTermWithDistinct
        /// <code>
        /// RegressionRunner.Run(_session, ContextInitTermWithDistinct.Executions());
        /// </code>
        /// </summary>

        public class TestContextInitTermWithDistinct : AbstractTestBase
        {
            public TestContextInitTermWithDistinct() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArray() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithOverlappingMultiKey() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithOverlappingMultiKey());

            [Test, RunInApplicationDomain]
            public void WithOverlappingSingleKey() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithOverlappingSingleKey());

            [Test, RunInApplicationDomain]
            public void WithNullKeyMultiKey() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithNullKeyMultiKey());

            [Test, RunInApplicationDomain]
            public void WithNullSingleKey() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithNullSingleKey());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextInitTermWithDistinct.WithInvalid());
        }
    }
} // end of namespace