///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.pattern;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.pattern
{
    [TestFixture]
    public class TestSuitePattern
    {
        private RegressionSession session;

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

        
        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_C),
                typeof(SupportBean_D),
                typeof(SupportBean_E),
                typeof(SupportBean_F),
                typeof(SupportBean_G),
                typeof(SupportBean),
                typeof(SupportCallEvent),
                typeof(SupportRFIDEvent),
                typeof(SupportBean_N),
                typeof(SupportBean_S0),
                typeof(SupportIdEventA),
                typeof(SupportIdEventB),
                typeof(SupportIdEventC),
                typeof(SupportIdEventD),
                typeof(SupportMarketDataBean),
                typeof(SupportTradeEvent),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanCombinedProps),
                typeof(ISupportC),
                typeof(ISupportBaseAB),
                typeof(ISupportA),
                typeof(ISupportB),
                typeof(ISupportD),
                typeof(ISupportBaseD),
                typeof(ISupportBaseDBase),
                typeof(ISupportAImplSuperG),
                typeof(ISupportAImplSuperGImplPlus),
                typeof(SupportOverrideBase),
                typeof(SupportOverrideOne),
                typeof(SupportOverrideOneA),
                typeof(SupportOverrideOneB),
                typeof(ISupportCImpl),
                typeof(ISupportABCImpl),
                typeof(ISupportAImpl),
                typeof(ISupportBImpl),
                typeof(ISupportDImpl),
                typeof(ISupportBCImpl),
                typeof(ISupportBaseABImpl),
                typeof(ISupportAImplSuperGImpl),
                typeof(SupportEventWithIntArray)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            foreach (string name in Collections.List(
                "computeISO8601String",
                "getThe1980Calendar",
                "getThe1980Date",
                "getThe1980DateTime",
                "getThe1980DateTimeOffset",
                "getThe1980Long",
                "getTheSeconds")) {
                configuration.Compiler.AddPlugInSingleRowFunction(name, typeof(PatternObserverTimerSchedule), char.ToUpper(name[0]) + name.Substring(1));
            }

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("lower", typeof(int), null);
            common.AddVariable("upper", typeof(int), null);
            common.AddVariable("VMIN", typeof(int), 0);
            common.AddVariable("VHOUR", typeof(int), 8);
            common.AddVariable("DD", typeof(double), 1);
            common.AddVariable("HH", typeof(double), 2);
            common.AddVariable("MM", typeof(double), 3);
            common.AddVariable("SS", typeof(double), 4);
            common.AddVariable("MS", typeof(double), 5);

            configuration.Runtime.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            configuration.Compiler.ByteCode.AttachEPL = true;
        }

        [Test, RunInApplicationDomain]
        public void TestPatternGuardTimerWithinOrMax()
        {
            RegressionRunner.Run(session, new PatternGuardTimerWithinOrMax());
        }

        [Test, RunInApplicationDomain]
        public void TestPatternOperatorOperatorMix()
        {
            RegressionRunner.Run(session, new PatternOperatorOperatorMix());
        }

        [Test, RunInApplicationDomain]
        public void TestPatternDeadPattern()
        {
            RegressionRunner.Run(session, new PatternDeadPattern());
        }

        [Test, RunInApplicationDomain]
        public void TestPatternStartLoop()
        {
            RegressionRunner.Run(session, new PatternStartLoop());
        }

        [Test, RunInApplicationDomain]
        public void TestPatternMicrosecondResolution()
        {
            RegressionRunner.Run(session, new PatternMicrosecondResolution(false));
        }
        [Test, RunInApplicationDomain]
        public void TestPatternSuperAndInterfaces()
        {
            RegressionRunner.Run(session, new PatternSuperAndInterfaces());
        }

        [Test, RunInApplicationDomain]
        public void TestPatternExpressionText()
        {
            RegressionRunner.Run(session, new PatternExpressionText());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorAnd
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorAnd.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorAnd : AbstractTestBase
        {
            public TestPatternOperatorAnd() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNotDefaultTrue() => RegressionRunner.Run(_session, PatternOperatorAnd.WithNotDefaultTrue());

            [Test, RunInApplicationDomain]
            public void WithWithEveryAndTerminationOptimization() =>
                RegressionRunner.Run(_session, PatternOperatorAnd.WithWithEveryAndTerminationOptimization());

            [Test, RunInApplicationDomain]
            public void WithWHarness() => RegressionRunner.Run(_session, PatternOperatorAnd.WithWHarness());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, PatternOperatorAnd.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorOr
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorOr.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorOr : AbstractTestBase
        {
            public TestPatternOperatorOr() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithperatorOrWHarness() => RegressionRunner.Run(_session, PatternOperatorOr.WithperatorOrWHarness());

            [Test, RunInApplicationDomain]
            public void WithrAndNotAndZeroStart() => RegressionRunner.Run(_session, PatternOperatorOr.WithrAndNotAndZeroStart());

            [Test, RunInApplicationDomain]
            public void WithrSimple() => RegressionRunner.Run(_session, PatternOperatorOr.WithrSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorNot
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorNot.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorNot : AbstractTestBase
        {
            public TestPatternOperatorNot() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNotWithEvery() => RegressionRunner.Run(_session, PatternOperatorNot.WithNotWithEvery());

            [Test, RunInApplicationDomain]
            public void WithNotTimeInterval() => RegressionRunner.Run(_session, PatternOperatorNot.WithNotTimeInterval());

            [Test, RunInApplicationDomain]
            public void WithNotFollowedBy() => RegressionRunner.Run(_session, PatternOperatorNot.WithNotFollowedBy());

            [Test, RunInApplicationDomain]
            public void WithUniformEvents() => RegressionRunner.Run(_session, PatternOperatorNot.WithUniformEvents());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternOperatorNot.WithOp());

            [Test, RunInApplicationDomain]
            public void WithOperatorNotWHarness() => RegressionRunner.Run(_session, PatternOperatorNot.WithOperatorNotWHarness());
        }

        /// <summary>
        /// Auto-test(s): PatternObserverTimerInterval
        /// <code>
        /// RegressionRunner.Run(_session, PatternObserverTimerInterval.Executions());
        /// </code>
        /// </summary>

        public class TestPatternObserverTimerInterval : AbstractTestBase
        {
            public TestPatternObserverTimerInterval() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIntervalSpecExpressionWithPropertyArray() => RegressionRunner.Run(
                _session,
                PatternObserverTimerInterval.WithIntervalSpecExpressionWithPropertyArray());

            [Test, RunInApplicationDomain]
            public void WithMonthScoped() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithIntervalSpecPreparedStmt() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithIntervalSpecPreparedStmt());

            [Test, RunInApplicationDomain]
            public void WithIntervalSpecExpressionWithProperty() => RegressionRunner.Run(
                _session,
                PatternObserverTimerInterval.WithIntervalSpecExpressionWithProperty());

            [Test, RunInApplicationDomain]
            public void WithIntervalSpecExpression() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithIntervalSpecExpression());

            [Test, RunInApplicationDomain]
            public void WithIntervalSpecVariables() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithIntervalSpecVariables());

            [Test, RunInApplicationDomain]
            public void WithIntervalSpec() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithIntervalSpec());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternObserverTimerInterval.WithOp());
        }

        /// <summary>
        /// Auto-test(s): PatternGuardTimerWithin
        /// <code>
        /// RegressionRunner.Run(_session, PatternGuardTimerWithin.Executions());
        /// </code>
        /// </summary>

        public class TestPatternGuardTimerWithin : AbstractTestBase
        {
            public TestPatternGuardTimerWithin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWithinMayMaxMonthScoped() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithWithinMayMaxMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithPatternNotFollowedBy() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithPatternNotFollowedBy());

            [Test, RunInApplicationDomain]
            public void WithWithinFromExpression() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithWithinFromExpression());

            [Test, RunInApplicationDomain]
            public void WithIntervalPrepared() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithIntervalPrepared());

            [Test, RunInApplicationDomain]
            public void WithInterval10MinVariable() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithInterval10MinVariable());

            [Test, RunInApplicationDomain]
            public void WithInterval10Min() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithInterval10Min());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternGuardTimerWithin.WithOp());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorFollowedBy
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorFollowedBy.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorFollowedBy : AbstractTestBase
        {
            public TestPatternOperatorFollowedBy() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFollowedOrPermFalse() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFollowedOrPermFalse());

            [Test, RunInApplicationDomain]
            public void WithFilterGreaterThen() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFilterGreaterThen());

            [Test, RunInApplicationDomain]
            public void WithFollowedEveryMultiple() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFollowedEveryMultiple());

            [Test, RunInApplicationDomain]
            public void WithFollowedNotEvery() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFollowedNotEvery());

            [Test, RunInApplicationDomain]
            public void WithRFIDZoneEnter() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithRFIDZoneEnter());

            [Test, RunInApplicationDomain]
            public void WithRFIDZoneExit() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithRFIDZoneExit());

            [Test, RunInApplicationDomain]
            public void WithMemoryRFIDEvent() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithMemoryRFIDEvent());

            [Test, RunInApplicationDomain]
            public void WithFollowedByTimer() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFollowedByTimer());

            [Test, RunInApplicationDomain]
            public void WithFollowedByWithNot() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithFollowedByWithNot());

            [Test, RunInApplicationDomain]
            public void WithOpWHarness() => RegressionRunner.Run(_session, PatternOperatorFollowedBy.WithOpWHarness());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorEvery
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorEvery.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorEvery : AbstractTestBase
        {
            public TestPatternOperatorEvery() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEveryFollowedBy() => RegressionRunner.Run(_session, PatternOperatorEvery.WithEveryFollowedBy());

            [Test, RunInApplicationDomain]
            public void WithEveryAndNot() => RegressionRunner.Run(_session, PatternOperatorEvery.WithEveryAndNot());

            [Test, RunInApplicationDomain]
            public void WithEveryFollowedByWithin() => RegressionRunner.Run(_session, PatternOperatorEvery.WithEveryFollowedByWithin());

            [Test, RunInApplicationDomain]
            public void WithEveryWithAnd() => RegressionRunner.Run(_session, PatternOperatorEvery.WithEveryWithAnd());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternOperatorEvery.WithOp());

            [Test, RunInApplicationDomain]
            public void WithEverySimple() => RegressionRunner.Run(_session, PatternOperatorEvery.WithEverySimple());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorMatchUntil
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorMatchUntil.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorMatchUntil : AbstractTestBase
        {
            public TestPatternOperatorMatchUntil() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithBoundRepeatWithNot() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithBoundRepeatWithNot());

            [Test, RunInApplicationDomain]
            public void WithExpressionBounds() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithExpressionBounds());

            [Test, RunInApplicationDomain]
            public void WithArrayFunctionRepeat() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithArrayFunctionRepeat());

            [Test, RunInApplicationDomain]
            public void WithRepeatUseTags() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithRepeatUseTags());

            [Test, RunInApplicationDomain]
            public void WithUseFilter() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithUseFilter());

            [Test, RunInApplicationDomain]
            public void WithSelectArray() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithSelectArray());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithOp());

            [Test, RunInApplicationDomain]
            public void WithMatchUntilSimple() => RegressionRunner.Run(_session, PatternOperatorMatchUntil.WithMatchUntilSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorEveryDistinct
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorEveryDistinct.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorEveryDistinct : AbstractTestBase
        {
            public TestPatternOperatorEveryDistinct() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctMultikeyWArray() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithMonthScoped() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithFollowedByWithDistinct() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithFollowedByWithDistinct());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctWithinFollowedBy() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctWithinFollowedBy());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverFollowedBy() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverFollowedBy());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverNot() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverNot());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverOr() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverOr());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverAnd() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverAnd());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverTimerWithin() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverTimerWithin());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverRepeat() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverRepeat());

            [Test, RunInApplicationDomain]
            public void WithTimerWithinOverDistinct() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithTimerWithinOverDistinct());

            [Test, RunInApplicationDomain]
            public void WithRepeatOverDistinct() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithRepeatOverDistinct());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctOverFilter() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctOverFilter());

            [Test, RunInApplicationDomain]
            public void WithExpireSeenBeforeKey() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithExpireSeenBeforeKey());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctWTime() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctWTime());

            [Test, RunInApplicationDomain]
            public void WithEveryDistinctSimple() => RegressionRunner.Run(_session, PatternOperatorEveryDistinct.WithEveryDistinctSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternObserverTimerAt
        /// <code>
        /// RegressionRunner.Run(_session, PatternObserverTimerAt.Executions());
        /// </code>
        /// </summary>

        public class TestPatternObserverTimerAt : AbstractTestBase
        {
            public TestPatternObserverTimerAt() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWMilliseconds() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithWMilliseconds());

            [Test, RunInApplicationDomain]
            public void WithEvery15thMonth() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithEvery15thMonth());

            [Test, RunInApplicationDomain]
            public void WithPropertyAndSODAAndTimezone() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithPropertyAndSODAAndTimezone());

            [Test, RunInApplicationDomain]
            public void WithExpression() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithExpression());

            [Test, RunInApplicationDomain]
            public void WithAtWeekdaysVariable() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithAtWeekdaysVariable());

            [Test, RunInApplicationDomain]
            public void WithAtWeekdaysPrepared() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithAtWeekdaysPrepared());

            [Test, RunInApplicationDomain]
            public void WithAtWeekdays() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithAtWeekdays());

            [Test, RunInApplicationDomain]
            public void WithCronParameter() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithCronParameter());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithOp());

            [Test, RunInApplicationDomain]
            public void WithTimerAtSimple() => RegressionRunner.Run(_session, PatternObserverTimerAt.WithTimerAtSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternObserverTimerSchedule
        /// <code>
        /// RegressionRunner.Run(_session, PatternObserverTimerSchedule.Executions());
        /// </code>
        /// </summary>

        public class TestPatternObserverTimerSchedule : AbstractTestBase
        {
            public TestPatternObserverTimerSchedule() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleUnlimitedRecurringPeriod() => RegressionRunner.Run(
                _session,
                PatternObserverTimerSchedule.WithTimerScheduleUnlimitedRecurringPeriod());

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleDateWithPeriod() => RegressionRunner.Run(_session, PatternObserverTimerSchedule.WithTimerScheduleDateWithPeriod());

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleJustPeriod() => RegressionRunner.Run(_session, PatternObserverTimerSchedule.WithTimerScheduleJustPeriod());

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleJustDate() => RegressionRunner.Run(_session, PatternObserverTimerSchedule.WithTimerScheduleJustDate());

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleLimitedWDateAndPeriod() => RegressionRunner.Run(
                _session,
                PatternObserverTimerSchedule.WithTimerScheduleLimitedWDateAndPeriod());

            [Test, RunInApplicationDomain]
            public void WithObserverTimerScheduleMultiform() => RegressionRunner.Run(
                _session,
                PatternObserverTimerSchedule.WithObserverTimerScheduleMultiform());

            [Test, RunInApplicationDomain]
            public void WithTimerScheduleSimple() => RegressionRunner.Run(_session, PatternObserverTimerSchedule.WithTimerScheduleSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternGuardWhile
        /// <code>
        /// RegressionRunner.Run(_session, PatternGuardWhile.Executions());
        /// </code>
        /// </summary>

        public class TestPatternGuardWhile : AbstractTestBase
        {
            public TestPatternGuardWhile() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, PatternGuardWhile.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithVariable() => RegressionRunner.Run(_session, PatternGuardWhile.WithVariable());

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, PatternGuardWhile.WithOp());

            [Test, RunInApplicationDomain]
            public void WithGuardWhileSimple() => RegressionRunner.Run(_session, PatternGuardWhile.WithGuardWhileSimple());
        }

        /// <summary>
        /// Auto-test(s): PatternUseResult
        /// <code>
        /// RegressionRunner.Run(_session, PatternUseResult.Executions());
        /// </code>
        /// </summary>

        public class TestPatternUseResult : AbstractTestBase
        {
            public TestPatternUseResult() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBooleanExprRemoveConsiderArrayTag() => RegressionRunner.Run(_session, PatternUseResult.WithBooleanExprRemoveConsiderArrayTag());

            [Test, RunInApplicationDomain]
            public void WithBooleanExprRemoveConsiderTag() => RegressionRunner.Run(_session, PatternUseResult.WithBooleanExprRemoveConsiderTag());

            [Test, RunInApplicationDomain]
            public void WithPatternTypeCacheForRepeat() => RegressionRunner.Run(_session, PatternUseResult.WithPatternTypeCacheForRepeat());

            [Test, RunInApplicationDomain]
            public void WithFollowedByFilter() => RegressionRunner.Run(_session, PatternUseResult.WithFollowedByFilter());

            [Test, RunInApplicationDomain]
            public void WithObjectId() => RegressionRunner.Run(_session, PatternUseResult.WithObjectId());

            [Test, RunInApplicationDomain]
            public void WithNumeric() => RegressionRunner.Run(_session, PatternUseResult.WithNumeric());
        }

        /// <summary>
        /// Auto-test(s): PatternComplexPropertyAccess
        /// <code>
        /// RegressionRunner.Run(_session, PatternComplexPropertyAccess.Executions());
        /// </code>
        /// </summary>

        public class TestPatternComplexPropertyAccess : AbstractTestBase
        {
            public TestPatternComplexPropertyAccess() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithIndexedValuePropCompile() => RegressionRunner.Run(_session, PatternComplexPropertyAccess.WithIndexedValuePropCompile());

            [Test, RunInApplicationDomain]
            public void WithIndexedValuePropOM() => RegressionRunner.Run(_session, PatternComplexPropertyAccess.WithIndexedValuePropOM());

            [Test, RunInApplicationDomain]
            public void WithIndexedValueProp() => RegressionRunner.Run(_session, PatternComplexPropertyAccess.WithIndexedValueProp());

            [Test, RunInApplicationDomain]
            public void WithIndexedFilterProp() => RegressionRunner.Run(_session, PatternComplexPropertyAccess.WithIndexedFilterProp());

            [Test, RunInApplicationDomain]
            public void WithComplexProperties() => RegressionRunner.Run(_session, PatternComplexPropertyAccess.WithComplexProperties());
        }

        /// <summary>
        /// Auto-test(s): PatternOperatorFollowedByMax
        /// <code>
        /// RegressionRunner.Run(_session, PatternOperatorFollowedByMax.Executions());
        /// </code>
        /// </summary>

        public class TestPatternOperatorFollowedByMax : AbstractTestBase
        {
            public TestPatternOperatorFollowedByMax() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOperatorFollowedByMaxInvalid() => RegressionRunner.Run(_session, PatternOperatorFollowedByMax.WithOperatorFollowedByMaxInvalid());

            [Test, RunInApplicationDomain]
            public void WithSingleMaxSimple() => RegressionRunner.Run(_session, PatternOperatorFollowedByMax.WithSingleMaxSimple());

            [Test, RunInApplicationDomain]
            public void WithSinglePermFalseAndQuit() => RegressionRunner.Run(_session, PatternOperatorFollowedByMax.WithSinglePermFalseAndQuit());

            [Test, RunInApplicationDomain]
            public void WithMixed() => RegressionRunner.Run(_session, PatternOperatorFollowedByMax.WithMixed());

            [Test, RunInApplicationDomain]
            public void WithMultiple() => RegressionRunner.Run(_session, PatternOperatorFollowedByMax.WithMultiple());
        }

        /// <summary>
        /// Auto-test(s): PatternCompositeSelect
        /// <code>
        /// RegressionRunner.Run(_session, PatternCompositeSelect.Executions());
        /// </code>
        /// </summary>

        public class TestPatternCompositeSelect : AbstractTestBase
        {
            public TestPatternCompositeSelect() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFragment() => RegressionRunner.Run(_session, PatternCompositeSelect.WithFragment());

            [Test, RunInApplicationDomain]
            public void WithFollowedByFilter() => RegressionRunner.Run(_session, PatternCompositeSelect.WithFollowedByFilter());
        }

        /// <summary>
        /// Auto-test(s): PatternConsumingFilter
        /// <code>
        /// RegressionRunner.Run(_session, PatternConsumingFilter.Executions());
        /// </code>
        /// </summary>

        public class TestPatternConsumingFilter : AbstractTestBase
        {
            public TestPatternConsumingFilter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, PatternConsumingFilter.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithOr() => RegressionRunner.Run(_session, PatternConsumingFilter.WithOr());

            [Test, RunInApplicationDomain]
            public void WithFilterAndSceneTwo() => RegressionRunner.Run(_session, PatternConsumingFilter.WithFilterAndSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithAnd() => RegressionRunner.Run(_session, PatternConsumingFilter.WithAnd());

            [Test, RunInApplicationDomain]
            public void WithFollowedBy() => RegressionRunner.Run(_session, PatternConsumingFilter.WithFollowedBy());
        }

        /// <summary>
        /// Auto-test(s): PatternConsumingPattern
        /// <code>
        /// RegressionRunner.Run(_session, PatternConsumingPattern.Executions());
        /// </code>
        /// </summary>

        public class TestPatternConsumingPattern : AbstractTestBase
        {
            public TestPatternConsumingPattern() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, PatternConsumingPattern.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithCombination() => RegressionRunner.Run(_session, PatternConsumingPattern.WithCombination());

            [Test, RunInApplicationDomain]
            public void WithEveryOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithEveryOp());

            [Test, RunInApplicationDomain]
            public void WithGuardOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithGuardOp());

            [Test, RunInApplicationDomain]
            public void WithNotOpNotImpacted() => RegressionRunner.Run(_session, PatternConsumingPattern.WithNotOpNotImpacted());

            [Test, RunInApplicationDomain]
            public void WithAndOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithAndOp());

            [Test, RunInApplicationDomain]
            public void WithObserverOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithObserverOp());

            [Test, RunInApplicationDomain]
            public void WithMatchUntilOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithMatchUntilOp());

            [Test, RunInApplicationDomain]
            public void WithFollowedByOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithFollowedByOp());

            [Test, RunInApplicationDomain]
            public void WithOrOp() => RegressionRunner.Run(_session, PatternConsumingPattern.WithOrOp());
        }

        /// <summary>
        /// Auto-test(s): PatternInvalid
        /// <code>
        /// RegressionRunner.Run(_session, PatternInvalid.Executions());
        /// </code>
        /// </summary>

        public class TestPatternInvalid : AbstractTestBase
        {
            public TestPatternInvalid() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUseResult() => RegressionRunner.Run(_session, PatternInvalid.WithUseResult());

            [Test, RunInApplicationDomain]
            public void WithStatementException() => RegressionRunner.Run(_session, PatternInvalid.WithStatementException());

            [Test, RunInApplicationDomain]
            public void WithInvalidExpr() => RegressionRunner.Run(_session, PatternInvalid.WithInvalidExpr());
        }

        /// <summary>
        /// Auto-test(s): PatternStartStop
        /// <code>
        /// RegressionRunner.Run(_session, PatternStartStop.Executions());
        /// </code>
        /// </summary>

        public class TestPatternStartStop : AbstractTestBase
        {
            public TestPatternStartStop() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithStartStopTwo() => RegressionRunner.Run(_session, PatternStartStop.WithStartStopTwo());

            [Test, RunInApplicationDomain]
            public void WithAddRemoveListener() => RegressionRunner.Run(_session, PatternStartStop.WithAddRemoveListener());

            [Test, RunInApplicationDomain]
            public void WithStartStopOne() => RegressionRunner.Run(_session, PatternStartStop.WithStartStopOne());
        }

        /// <summary>
        /// Auto-test(s): PatternRepeatRouteEvent
        /// <code>
        /// RegressionRunner.Run(_session, PatternRepeatRouteEvent.Executions());
        /// </code>
        /// </summary>

        public class TestPatternRepeatRouteEvent : AbstractTestBase
        {
            public TestPatternRepeatRouteEvent() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimer() => RegressionRunner.Run(_session, PatternRepeatRouteEvent.WithTimer());

            [Test, RunInApplicationDomain]
            public void WithCascade() => RegressionRunner.Run(_session, PatternRepeatRouteEvent.WithCascade());

            [Test, RunInApplicationDomain]
            public void WithSingle() => RegressionRunner.Run(_session, PatternRepeatRouteEvent.WithSingle());
        }
    }
} // end of namespace