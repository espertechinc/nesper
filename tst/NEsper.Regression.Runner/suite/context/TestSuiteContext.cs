///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class TestSuiteContext
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

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInVirtualDataWindow(
                "test",
                "vdw",
                typeof(SupportVirtualDWForge),
                SupportVirtualDW.ITERATE); // configure with iteration
        }

        [Test, RunInApplicationDomain]
        public void TestContextAdminListen()
        {
            RegressionRunner.Run(session, ContextAdminListen.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextCategory()
        {
            RegressionRunner.Run(session, ContextCategory.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextDocExamples()
        {
            RegressionRunner.Run(session, new ContextDocExamples());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermWithDistinct()
        {
            RegressionRunner.Run(session, ContextInitTermWithDistinct.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermWithNow()
        {
            RegressionRunner.Run(session, ContextInitTermWithNow.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmented()
        {
            RegressionRunner.Run(session, ContextKeySegmented.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedAggregate()
        {
            RegressionRunner.Run(session, ContextKeySegmentedAggregate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedInfra()
        {
            RegressionRunner.Run(session, ContextKeySegmentedInfra.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedNamedWindow()
        {
            RegressionRunner.Run(session, ContextKeySegmentedNamedWindow.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextLifecycle()
        {
            RegressionRunner.Run(session, ContextLifecycle.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextNested()
        {
            RegressionRunner.Run(session, ContextNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextSelectionAndFireAndForget()
        {
            RegressionRunner.Run(session, ContextSelectionAndFireAndForget.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextVariables()
        {
            RegressionRunner.Run(session, ContextVariables.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextWDeclaredExpression()
        {
            RegressionRunner.Run(session, ContextWDeclaredExpression.Executions());
        }
        
        /// <summary>
        /// Auto-test(s): ContextHashSegmented
        /// <code>
        /// RegressionRunner.Run(_session, ContextHashSegmented.Executions());
        /// </code>
        /// </summary>

        public class TestContextHashSegmented : AbstractTestBase
        {
            public TestContextHashSegmented() : base(Configure) { }

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
    }
} // end of namespace