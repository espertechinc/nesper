///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.view;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.view
{
    [TestFixture]
    public class TestSuiteView
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
                typeof(SupportMarketDataBean),
                typeof(SupportBeanComplexProps),
                typeof(SupportBean),
                typeof(SupportBeanWithEnum),
                typeof(SupportBeanTimestamp),
                typeof(SupportEventIdWithTimestamp),
                typeof(SupportSensorEvent),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_A),
                typeof(SupportBean_N),
                typeof(SupportContextInitEventWLength),
                typeof(SupportEventWithLongArray),
                typeof(SupportObjectArrayOneDim)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddEventType(
                "OAEventStringInt",
                new[] {"P1", "P2"},
                new object[] {typeof(string), typeof(int)});

            configuration.Common.AddVariable("TIME_WIN_ONE", typeof(int), 4);
            configuration.Common.AddVariable("TIME_WIN_TWO", typeof(double), 4000);

            configuration.Compiler.AddPlugInSingleRowFunction(
                "udf",
                typeof(ViewExpressionWindow.LocalUDF),
                "EvaluateExpiryUDF");

            configuration.Common.AddImportNamespace(typeof(DefaultSupportSourceOp));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportCaptureOp));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportSourceOpForge));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportCaptureOpForge));
        }

        [Test, RunInApplicationDomain]
        public void TestViewInvalid()
        {
            RegressionRunner.Run(session, new ViewInvalid());
        }

        /// <summary>
        /// Auto-test(s): ViewMultikeyWArray
        /// <code>
        /// RegressionRunner.Run(_session, ViewMultikeyWArray.Executions());
        /// </code>
        /// </summary>

        public class TestViewMultikeyWArray : AbstractTestBase
        {
            public TestViewMultikeyWArray() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeyDataflow() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeyDataflow());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeySubqueryInFilter() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeySubqueryInFilter());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeyNamedWindow() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeyNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeySubquery() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeySubquery());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeyUnion() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeyUnion());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueArrayKeyIntersection() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueArrayKeyIntersection());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueTwoKeyAllArrayOfObject() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueTwoKeyAllArrayOfObject());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueTwoKeyAllArrayOfPrimitive() =>
                RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueTwoKeyAllArrayOfPrimitive());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueOneKey2DimArray() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueOneKey2DimArray());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueOneKeyArrayOfObjectArray() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueOneKeyArrayOfObjectArray());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueOneKeyArrayOfLongPrimitive() => RegressionRunner.Run(
                _session,
                ViewMultikeyWArray.WithLastUniqueOneKeyArrayOfLongPrimitive());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueThreeKey() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueThreeKey());

            [Test, RunInApplicationDomain]
            public void WithRank() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithRank());

            [Test, RunInApplicationDomain]
            public void WithGroupWin() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithGroupWin());

            [Test, RunInApplicationDomain]
            public void WithFirstUnique() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithFirstUnique());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueTwoKey() => RegressionRunner.Run(_session, ViewMultikeyWArray.WithLastUniqueTwoKey());
        }

        /// <summary>
        /// Auto-test(s): ViewDerived
        /// <code>
        /// RegressionRunner.Run(_session, ViewDerived.Executions());
        /// </code>
        /// </summary>

        public class TestViewDerived : AbstractTestBase
        {
            public TestViewDerived() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWCorrelation() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWCorrelation());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWRegressionLinestSceneTwo() =>
                RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWRegressionLinestSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWRegressionLinestSceneOne() =>
                RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWRegressionLinestSceneOne());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWWeightedAvgSceneTwo() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWWeightedAvgSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWWeightedAvgSceneOne() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWWeightedAvgSceneOne());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWUniSceneThree() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWUniSceneThree());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWUniSceneTwo() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWUniSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithDerivedLengthWUniSceneOne() => RegressionRunner.Run(_session, ViewDerived.WithDerivedLengthWUniSceneOne());

            [Test, RunInApplicationDomain]
            public void WithDerivedAll() => RegressionRunner.Run(_session, ViewDerived.WithDerivedAll());

            [Test, RunInApplicationDomain]
            public void WithSizeAddProps() => RegressionRunner.Run(_session, ViewDerived.WithSizeAddProps());

            [Test, RunInApplicationDomain]
            public void WithSizeSceneTwo() => RegressionRunner.Run(_session, ViewDerived.WithSizeSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSizeSceneOne() => RegressionRunner.Run(_session, ViewDerived.WithSizeSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewExpressionBatch
        /// <code>
        /// RegressionRunner.Run(_session, ViewExpressionBatch.Executions());
        /// </code>
        /// </summary>

        public class TestViewExpressionBatch : AbstractTestBase
        {
            public TestViewExpressionBatch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithVariableBatch() => RegressionRunner.Run(_session, ViewExpressionBatch.WithVariableBatch());

            [Test, RunInApplicationDomain]
            public void WithDynamicTimeBatch() => RegressionRunner.Run(_session, ViewExpressionBatch.WithDynamicTimeBatch());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowDelete() => RegressionRunner.Run(_session, ViewExpressionBatch.WithNamedWindowDelete());

            [Test, RunInApplicationDomain]
            public void WithAggregationOnDelete() => RegressionRunner.Run(_session, ViewExpressionBatch.WithAggregationOnDelete());

            [Test, RunInApplicationDomain]
            public void WithAggregationWGroupwin() => RegressionRunner.Run(_session, ViewExpressionBatch.WithAggregationWGroupwin());

            [Test, RunInApplicationDomain]
            public void WithAggregationUngrouped() => RegressionRunner.Run(_session, ViewExpressionBatch.WithAggregationUngrouped());

            [Test, RunInApplicationDomain]
            public void WithEventPropBatch() => RegressionRunner.Run(_session, ViewExpressionBatch.WithEventPropBatch());

            [Test, RunInApplicationDomain]
            public void WithPrev() => RegressionRunner.Run(_session, ViewExpressionBatch.WithPrev());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewExpressionBatch.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithUDFBuiltin() => RegressionRunner.Run(_session, ViewExpressionBatch.WithUDFBuiltin());

            [Test, RunInApplicationDomain]
            public void WithTimeBatch() => RegressionRunner.Run(_session, ViewExpressionBatch.WithTimeBatch());

            [Test, RunInApplicationDomain]
            public void WithLengthBatch() => RegressionRunner.Run(_session, ViewExpressionBatch.WithLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithNewestEventOldestEvent() => RegressionRunner.Run(_session, ViewExpressionBatch.WithNewestEventOldestEvent());
        }

        /// <summary>
        /// Auto-test(s): ViewExpressionWindow
        /// <code>
        /// RegressionRunner.Run(_session, ViewExpressionWindow.Executions());
        /// </code>
        /// </summary>

        public class TestViewExpressionWindow : AbstractTestBase
        {
            public TestViewExpressionWindow() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDynamicTimeWindow() => RegressionRunner.Run(_session, ViewExpressionWindow.WithDynamicTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithVariable() => RegressionRunner.Run(_session, ViewExpressionWindow.WithVariable());

            [Test, RunInApplicationDomain]
            public void WithAggregationWOnDelete() => RegressionRunner.Run(_session, ViewExpressionWindow.WithAggregationWOnDelete());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowDelete() => RegressionRunner.Run(_session, ViewExpressionWindow.WithNamedWindowDelete());

            [Test, RunInApplicationDomain]
            public void WithAggregationWGroupwin() => RegressionRunner.Run(_session, ViewExpressionWindow.WithAggregationWGroupwin());

            [Test, RunInApplicationDomain]
            public void WithAggregationUngrouped() => RegressionRunner.Run(_session, ViewExpressionWindow.WithAggregationUngrouped());

            [Test, RunInApplicationDomain]
            public void WithPrev() => RegressionRunner.Run(_session, ViewExpressionWindow.WithPrev());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewExpressionWindow.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithUDFBuiltin() => RegressionRunner.Run(_session, ViewExpressionWindow.WithUDFBuiltin());

            [Test, RunInApplicationDomain]
            public void WithTimeWindow() => RegressionRunner.Run(_session, ViewExpressionWindow.WithTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindow() => RegressionRunner.Run(_session, ViewExpressionWindow.WithLengthWindow());

            [Test, RunInApplicationDomain]
            public void WithNewestEventOldestEvent() => RegressionRunner.Run(_session, ViewExpressionWindow.WithNewestEventOldestEvent());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewExpressionWindow.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewExternallyTimedBatched
        /// <code>
        /// RegressionRunner.Run(_session, ViewExternallyTimedBatched.Executions());
        /// </code>
        /// </summary>

        public class TestViewExternallyTimedBatched : AbstractTestBase
        {
            public TestViewExternallyTimedBatched() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMonthScoped() => RegressionRunner.Run(_session, ViewExternallyTimedBatched.WithMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithRefWithPrev() => RegressionRunner.Run(_session, ViewExternallyTimedBatched.WithRefWithPrev());

            [Test, RunInApplicationDomain]
            public void WithedWithRefTime() => RegressionRunner.Run(_session, ViewExternallyTimedBatched.WithedWithRefTime());

            [Test, RunInApplicationDomain]
            public void WithedNoReference() => RegressionRunner.Run(_session, ViewExternallyTimedBatched.WithedNoReference());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewExternallyTimedBatched.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewExternallyTimedWin
        /// <code>
        /// RegressionRunner.Run(_session, ViewExternallyTimedWin.Executions());
        /// </code>
        /// </summary>

        public class TestViewExternallyTimedWin : AbstractTestBase
        {
            public TestViewExternallyTimedWin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWindowPrev() => RegressionRunner.Run(_session, ViewExternallyTimedWin.WithWindowPrev());

            [Test, RunInApplicationDomain]
            public void WithTimedMonthScoped() => RegressionRunner.Run(_session, ViewExternallyTimedWin.WithTimedMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithWinSceneShort() => RegressionRunner.Run(_session, ViewExternallyTimedWin.WithWinSceneShort());

            [Test, RunInApplicationDomain]
            public void WithBatchSceneTwo() => RegressionRunner.Run(_session, ViewExternallyTimedWin.WithBatchSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithWindowSceneOne() => RegressionRunner.Run(_session, ViewExternallyTimedWin.WithWindowSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewFirstEvent
        /// <code>
        /// RegressionRunner.Run(_session, ViewFirstEvent.Executions());
        /// </code>
        /// </summary>

        public class TestViewFirstEvent : AbstractTestBase
        {
            public TestViewFirstEvent() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMarketData() => RegressionRunner.Run(_session, ViewFirstEvent.WithMarketData());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewFirstEvent.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewFirstLength
        /// <code>
        /// RegressionRunner.Run(_session, ViewFirstLength.Executions());
        /// </code>
        /// </summary>

        public class TestViewFirstLength : AbstractTestBase
        {
            public TestViewFirstLength() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMarketData() => RegressionRunner.Run(_session, ViewFirstLength.WithMarketData());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewFirstLength.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewFirstTime
        /// <code>
        /// RegressionRunner.Run(_session, ViewFirstTime.Executions());
        /// </code>
        /// </summary>

        public class TestViewFirstTime : AbstractTestBase
        {
            public TestViewFirstTime() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithceneTwo() => RegressionRunner.Run(_session, ViewFirstTime.WithceneTwo());

            [Test, RunInApplicationDomain]
            public void WithceneOne() => RegressionRunner.Run(_session, ViewFirstTime.WithceneOne());

            [Test, RunInApplicationDomain]
            public void Withimple() => RegressionRunner.Run(_session, ViewFirstTime.Withimple());
        }

        /// <summary>
        /// Auto-test(s): ViewFirstUnique
        /// <code>
        /// RegressionRunner.Run(_session, ViewFirstUnique.Executions());
        /// </code>
        /// </summary>

        public class TestViewFirstUnique : AbstractTestBase
        {
            public TestViewFirstUnique() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithceneOne() => RegressionRunner.Run(_session, ViewFirstUnique.WithceneOne());

            [Test, RunInApplicationDomain]
            public void Withimple() => RegressionRunner.Run(_session, ViewFirstUnique.Withimple());
        }

        /// <summary>
        /// Auto-test(s): ViewGroup
        /// <code>
        /// RegressionRunner.Run(_session, ViewGroup.Executions());
        /// </code>
        /// </summary>

        public class TestViewGroup : AbstractTestBase
        {
            public TestViewGroup() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEscapedPropertyText() => RegressionRunner.Run(_session, ViewGroup.WithEscapedPropertyText());

            [Test, RunInApplicationDomain]
            public void WithExpressionBatch() => RegressionRunner.Run(_session, ViewGroup.WithExpressionBatch());

            [Test, RunInApplicationDomain]
            public void WithExpressionGrouped() => RegressionRunner.Run(_session, ViewGroup.WithExpressionGrouped());

            [Test, RunInApplicationDomain]
            public void WithTimeWin() => RegressionRunner.Run(_session, ViewGroup.WithTimeWin());

            [Test, RunInApplicationDomain]
            public void WithLengthBatch() => RegressionRunner.Run(_session, ViewGroup.WithLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithLengthWin() => RegressionRunner.Run(_session, ViewGroup.WithLengthWin());

            [Test, RunInApplicationDomain]
            public void WithTimeLengthBatch() => RegressionRunner.Run(_session, ViewGroup.WithTimeLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithTimeOrder() => RegressionRunner.Run(_session, ViewGroup.WithTimeOrder());

            [Test, RunInApplicationDomain]
            public void WithTimeAccum() => RegressionRunner.Run(_session, ViewGroup.WithTimeAccum());

            [Test, RunInApplicationDomain]
            public void WithTimeBatch() => RegressionRunner.Run(_session, ViewGroup.WithTimeBatch());

            [Test, RunInApplicationDomain]
            public void WithReclaimWithFlipTime() => RegressionRunner.Run(_session, ViewGroup.WithReclaimWithFlipTime());

            [Test, RunInApplicationDomain]
            public void WithLengthWinWeightAvg() => RegressionRunner.Run(_session, ViewGroup.WithLengthWinWeightAvg());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewGroup.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithMultiProperty() => RegressionRunner.Run(_session, ViewGroup.WithMultiProperty());

            [Test, RunInApplicationDomain]
            public void WithLinest() => RegressionRunner.Run(_session, ViewGroup.WithLinest());

            [Test, RunInApplicationDomain]
            public void WithCorrel() => RegressionRunner.Run(_session, ViewGroup.WithCorrel());

            [Test, RunInApplicationDomain]
            public void WithReclaimAgedHint() => RegressionRunner.Run(_session, ViewGroup.WithReclaimAgedHint());

            [Test, RunInApplicationDomain]
            public void WithReclaimTimeWindow() => RegressionRunner.Run(_session, ViewGroup.WithReclaimTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithStats() => RegressionRunner.Run(_session, ViewGroup.WithStats());

            [Test, RunInApplicationDomain]
            public void WithObjectArrayEvent() => RegressionRunner.Run(_session, ViewGroup.WithObjectArrayEvent());
        }

        /// <summary>
        /// Auto-test(s): ViewIntersect
        /// <code>
        /// RegressionRunner.Run(_session, ViewIntersect.Executions());
        /// </code>
        /// </summary>

        public class TestViewIntersect : AbstractTestBase
        {
            public TestViewIntersect() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimeWinNamedWindowDelete() => RegressionRunner.Run(_session, ViewIntersect.WithTimeWinNamedWindowDelete());

            [Test, RunInApplicationDomain]
            public void WithTimeWinNamedWindow() => RegressionRunner.Run(_session, ViewIntersect.WithTimeWinNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstUniqueAndLengthOnDelete() => RegressionRunner.Run(_session, ViewIntersect.WithFirstUniqueAndLengthOnDelete());

            [Test, RunInApplicationDomain]
            public void WithSubselect() => RegressionRunner.Run(_session, ViewIntersect.WithSubselect());

            [Test, RunInApplicationDomain]
            public void WithGroupTimeUnique() => RegressionRunner.Run(_session, ViewIntersect.WithGroupTimeUnique());

            [Test, RunInApplicationDomain]
            public void WithTimeUniqueMultikey() => RegressionRunner.Run(_session, ViewIntersect.WithTimeUniqueMultikey());

            [Test, RunInApplicationDomain]
            public void WithLengthOneUnique() => RegressionRunner.Run(_session, ViewIntersect.WithLengthOneUnique());

            [Test, RunInApplicationDomain]
            public void WithTimeWinSODA() => RegressionRunner.Run(_session, ViewIntersect.WithTimeWinSODA());

            [Test, RunInApplicationDomain]
            public void WithTimeWinReversed() => RegressionRunner.Run(_session, ViewIntersect.WithTimeWinReversed());

            [Test, RunInApplicationDomain]
            public void WithTimeWin() => RegressionRunner.Run(_session, ViewIntersect.WithTimeWin());

            [Test, RunInApplicationDomain]
            public void WithSorted() => RegressionRunner.Run(_session, ViewIntersect.WithSorted());

            [Test, RunInApplicationDomain]
            public void WithTwoUnique() => RegressionRunner.Run(_session, ViewIntersect.WithTwoUnique());

            [Test, RunInApplicationDomain]
            public void WithPattern() => RegressionRunner.Run(_session, ViewIntersect.WithPattern());

            [Test, RunInApplicationDomain]
            public void WithThreeUnique() => RegressionRunner.Run(_session, ViewIntersect.WithThreeUnique());

            [Test, RunInApplicationDomain]
            public void WithGroupBy() => RegressionRunner.Run(_session, ViewIntersect.WithGroupBy());

            [Test, RunInApplicationDomain]
            public void WithAndDerivedValue() => RegressionRunner.Run(_session, ViewIntersect.WithAndDerivedValue());

            [Test, RunInApplicationDomain]
            public void WithBatchWindow() => RegressionRunner.Run(_session, ViewIntersect.WithBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstUniqueAndFirstLength() => RegressionRunner.Run(_session, ViewIntersect.WithFirstUniqueAndFirstLength());

            [Test, RunInApplicationDomain]
            public void WithUniqueAndFirstLength() => RegressionRunner.Run(_session, ViewIntersect.WithUniqueAndFirstLength());
        }

        /// <summary>
        /// Auto-test(s): ViewKeepAll
        /// <code>
        /// RegressionRunner.Run(_session, ViewKeepAll.Executions());
        /// </code>
        /// </summary>

        public class TestViewKeepAll : AbstractTestBase
        {
            public TestViewKeepAll() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWindowStats() => RegressionRunner.Run(_session, ViewKeepAll.WithWindowStats());

            [Test, RunInApplicationDomain]
            public void WithIterator() => RegressionRunner.Run(_session, ViewKeepAll.WithIterator());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ViewKeepAll.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): ViewLastEvent
        /// <code>
        /// RegressionRunner.Run(_session, ViewLastEvent.Executions());
        /// </code>
        /// </summary>

        public class TestViewLastEvent : AbstractTestBase
        {
            public TestViewLastEvent() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMarketData() => RegressionRunner.Run(_session, ViewLastEvent.WithMarketData());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewLastEvent.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewLengthBatch
        /// <code>
        /// RegressionRunner.Run(_session, ViewLengthBatch.Executions());
        /// </code>
        /// </summary>

        public class TestViewLengthBatch : AbstractTestBase
        {
            public TestViewLengthBatch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDelete() => RegressionRunner.Run(_session, ViewLengthBatch.WithDelete());

            [Test, RunInApplicationDomain]
            public void WithPrev() => RegressionRunner.Run(_session, ViewLengthBatch.WithPrev());

            [Test, RunInApplicationDomain]
            public void WithNormal() => RegressionRunner.Run(_session, ViewLengthBatch.WithNormal());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewLengthBatch.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSize3() => RegressionRunner.Run(_session, ViewLengthBatch.WithSize3());

            [Test, RunInApplicationDomain]
            public void WithSize1() => RegressionRunner.Run(_session, ViewLengthBatch.WithSize1());

            [Test, RunInApplicationDomain]
            public void WithSize2() => RegressionRunner.Run(_session, ViewLengthBatch.WithSize2());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewLengthBatch.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewLengthWin
        /// <code>
        /// RegressionRunner.Run(_session, ViewLengthWin.Executions());
        /// </code>
        /// </summary>

        public class TestViewLengthWin : AbstractTestBase
        {
            public TestViewLengthWin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithdowIterator() => RegressionRunner.Run(_session, ViewLengthWin.WithdowIterator());

            [Test, RunInApplicationDomain]
            public void WithWPropertyDetail() => RegressionRunner.Run(_session, ViewLengthWin.WithWPropertyDetail());

            [Test, RunInApplicationDomain]
            public void WithdowWPrevPrior() => RegressionRunner.Run(_session, ViewLengthWin.WithdowWPrevPrior());

            [Test, RunInApplicationDomain]
            public void WithdowSceneOne() => RegressionRunner.Run(_session, ViewLengthWin.WithdowSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewParameterizedByContext
        /// <code>
        /// RegressionRunner.Run(_session, ViewParameterizedByContext.Executions());
        /// </code>
        /// </summary>

        public class TestViewParameterizedByContext : AbstractTestBase
        {
            public TestViewParameterizedByContext() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMoreWindows() => RegressionRunner.Run(_session, ViewParameterizedByContext.WithMoreWindows());

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, ViewParameterizedByContext.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithLengthWindow() => RegressionRunner.Run(_session, ViewParameterizedByContext.WithLengthWindow());
        }

        /// <summary>
        /// Auto-test(s): ViewRank
        /// <code>
        /// RegressionRunner.Run(_session, ViewRank.Executions());
        /// </code>
        /// </summary>

        public class TestViewRank : AbstractTestBase
        {
            public TestViewRank() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewRank.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithedSceneOne() => RegressionRunner.Run(_session, ViewRank.WithedSceneOne());

            [Test, RunInApplicationDomain]
            public void WithRanked() => RegressionRunner.Run(_session, ViewRank.WithRanked());

            [Test, RunInApplicationDomain]
            public void WithRemoveStream() => RegressionRunner.Run(_session, ViewRank.WithRemoveStream());

            [Test, RunInApplicationDomain]
            public void WithMultiexpression() => RegressionRunner.Run(_session, ViewRank.WithMultiexpression());

            [Test, RunInApplicationDomain]
            public void WithPrevAndGroupWin() => RegressionRunner.Run(_session, ViewRank.WithPrevAndGroupWin());

            [Test, RunInApplicationDomain]
            public void WithedPrev() => RegressionRunner.Run(_session, ViewRank.WithedPrev());
        }

        /// <summary>
        /// Auto-test(s): ViewSort
        /// <code>
        /// RegressionRunner.Run(_session, ViewSort.Executions());
        /// </code>
        /// </summary>

        public class TestViewSort : AbstractTestBase
        {
            public TestViewSort() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithedPrev() => RegressionRunner.Run(_session, ViewSort.WithedPrev());

            [Test, RunInApplicationDomain]
            public void WithedPrimitiveKey() => RegressionRunner.Run(_session, ViewSort.WithedPrimitiveKey());

            [Test, RunInApplicationDomain]
            public void WithedMultikey() => RegressionRunner.Run(_session, ViewSort.WithedMultikey());

            [Test, RunInApplicationDomain]
            public void WithedSingleKeyBuiltin() => RegressionRunner.Run(_session, ViewSort.WithedSingleKeyBuiltin());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.Run(_session, ViewSort.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewSort.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewTimeAccum
        /// <code>
        /// RegressionRunner.Run(_session, ViewTimeAccum.Executions());
        /// </code>
        /// </summary>

        public class TestViewTimeAccum : AbstractTestBase
        {
            public TestViewTimeAccum() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupedWindow() => RegressionRunner.Run(_session, ViewTimeAccum.WithGroupedWindow());

            [Test, RunInApplicationDomain]
            public void WithSum() => RegressionRunner.Run(_session, ViewTimeAccum.WithSum());

            [Test, RunInApplicationDomain]
            public void WithMonthScoped() => RegressionRunner.Run(_session, ViewTimeAccum.WithMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithPreviousAndPriorSceneTwo() => RegressionRunner.Run(_session, ViewTimeAccum.WithPreviousAndPriorSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithPreviousAndPriorSceneOne() => RegressionRunner.Run(_session, ViewTimeAccum.WithPreviousAndPriorSceneOne());

            [Test, RunInApplicationDomain]
            public void WithRStream() => RegressionRunner.Run(_session, ViewTimeAccum.WithRStream());

            [Test, RunInApplicationDomain]
            public void WithSceneThree() => RegressionRunner.Run(_session, ViewTimeAccum.WithSceneThree());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.Run(_session, ViewTimeAccum.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewTimeAccum.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewTimeBatch
        /// <code>
        /// RegressionRunner.Run(_session, ViewTimeBatch.Executions());
        /// </code>
        /// </summary>

        public class TestViewTimeBatch : AbstractTestBase
        {
            public TestViewTimeBatch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRefPoint() => RegressionRunner.Run(_session, ViewTimeBatch.WithRefPoint());

            [Test, RunInApplicationDomain]
            public void WithNoRefPoint() => RegressionRunner.Run(_session, ViewTimeBatch.WithNoRefPoint());

            [Test, RunInApplicationDomain]
            public void WithMultiBatch() => RegressionRunner.Run(_session, ViewTimeBatch.WithMultiBatch());

            [Test, RunInApplicationDomain]
            public void WithMultirow() => RegressionRunner.Run(_session, ViewTimeBatch.WithMultirow());

            [Test, RunInApplicationDomain]
            public void WithLonger() => RegressionRunner.Run(_session, ViewTimeBatch.WithLonger());

            [Test, RunInApplicationDomain]
            public void WithStartEagerForceUpdate() => RegressionRunner.Run(_session, ViewTimeBatch.WithStartEagerForceUpdate());

            [Test, RunInApplicationDomain]
            public void WithMonthScoped() => RegressionRunner.Run(_session, ViewTimeBatch.WithMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithStartEagerForceUpdateSceneTwo() => RegressionRunner.Run(_session, ViewTimeBatch.WithStartEagerForceUpdateSceneTwo());

            [Test, RunInApplicationDomain]
            public void With10Sec() => RegressionRunner.Run(_session, ViewTimeBatch.With10Sec());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewTimeBatch.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewTimeLengthBatch
        /// <code>
        /// RegressionRunner.Run(_session, ViewTimeLengthBatch.Executions());
        /// </code>
        /// </summary>

        public class TestViewTimeLengthBatch : AbstractTestBase
        {
            public TestViewTimeLengthBatch() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupBySumStartEager() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithGroupBySumStartEager());

            [Test, RunInApplicationDomain]
            public void WithPreviousAndPrior() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithPreviousAndPrior());

            [Test, RunInApplicationDomain]
            public void WithForceOutputStartNoEagerSum() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithForceOutputStartNoEagerSum());

            [Test, RunInApplicationDomain]
            public void WithForceOutputStartEagerSum() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithForceOutputStartEagerSum());

            [Test, RunInApplicationDomain]
            public void WithStartEager() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithStartEager());

            [Test, RunInApplicationDomain]
            public void WithForceOutputSum() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithForceOutputSum());

            [Test, RunInApplicationDomain]
            public void WithForceOutputTwo() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithForceOutputTwo());

            [Test, RunInApplicationDomain]
            public void WithForceOutputOne() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithForceOutputOne());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewTimeLengthBatch.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewTimeOrderAndTimeToLive
        /// <code>
        /// RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.Executions());
        /// </code>
        /// </summary>

        public class TestViewTimeOrderAndTimeToLive : AbstractTestBase
        {
            public TestViewTimeOrderAndTimeToLive() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTTLPreviousAndPriorSceneTwo() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLPreviousAndPriorSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithTTLPreviousAndPriorSceneOne() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLPreviousAndPriorSceneOne());

            [Test, RunInApplicationDomain]
            public void WithTTLInvalid() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLInvalid());

            [Test, RunInApplicationDomain]
            public void WithTTLGroupedWindow() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLGroupedWindow());

            [Test, RunInApplicationDomain]
            public void WithTTLTimeOrder() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLTimeOrder());

            [Test, RunInApplicationDomain]
            public void WithTTLTimeOrderRemoveStream() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLTimeOrderRemoveStream());

            [Test, RunInApplicationDomain]
            public void WithTTLMonthScoped() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithTTLTimeToLive() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithTTLTimeToLive());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.Run(_session, ViewTimeOrderAndTimeToLive.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewTimeWin
        /// <code>
        /// RegressionRunner.Run(_session, ViewTimeWin.Executions());
        /// </code>
        /// </summary>

        public class TestViewTimeWin : AbstractTestBase
        {
            public TestViewTimeWin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWindowFlipTimer() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowFlipTimer());

            [Test, RunInApplicationDomain]
            public void WithWindowTimePeriodParams() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowTimePeriodParams());

            [Test, RunInApplicationDomain]
            public void WithWindowVariableTimePeriodStmt() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowVariableTimePeriodStmt());

            [Test, RunInApplicationDomain]
            public void WithWindowTimePeriod() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowTimePeriod());

            [Test, RunInApplicationDomain]
            public void WithWindowVariableStmt() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowVariableStmt());

            [Test, RunInApplicationDomain]
            public void WithWindowPreparedStmt() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowPreparedStmt());

            [Test, RunInApplicationDomain]
            public void WithWindowWPrev() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowWPrev());

            [Test, RunInApplicationDomain]
            public void WithWindowMonthScoped() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowMonthScoped());

            [Test, RunInApplicationDomain]
            public void WithSumWFilter() => RegressionRunner.Run(_session, ViewTimeWin.WithSumWFilter());

            [Test, RunInApplicationDomain]
            public void WithSumGroupBy() => RegressionRunner.Run(_session, ViewTimeWin.WithSumGroupBy());

            [Test, RunInApplicationDomain]
            public void WithSum() => RegressionRunner.Run(_session, ViewTimeWin.WithSum());

            [Test, RunInApplicationDomain]
            public void WithJustSelectStar() => RegressionRunner.Run(_session, ViewTimeWin.WithJustSelectStar());

            [Test, RunInApplicationDomain]
            public void WithWindowSceneTwo() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithWindowSceneOne() => RegressionRunner.Run(_session, ViewTimeWin.WithWindowSceneOne());
        }

        /// <summary>
        /// Auto-test(s): ViewUnion
        /// <code>
        /// RegressionRunner.Run(_session, ViewUnion.Executions());
        /// </code>
        /// </summary>

        public class TestViewUnion : AbstractTestBase
        {
            public TestViewUnion() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTimeWinNamedWindowDelete() => RegressionRunner.Run(_session, ViewUnion.WithTimeWinNamedWindowDelete());

            [Test, RunInApplicationDomain]
            public void WithTimeWinNamedWindow() => RegressionRunner.Run(_session, ViewUnion.WithTimeWinNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstUniqueAndLengthOnDelete() => RegressionRunner.Run(_session, ViewUnion.WithFirstUniqueAndLengthOnDelete());

            [Test, RunInApplicationDomain]
            public void WithSubselect() => RegressionRunner.Run(_session, ViewUnion.WithSubselect());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ViewUnion.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithTimeWinSODA() => RegressionRunner.Run(_session, ViewUnion.WithTimeWinSODA());

            [Test, RunInApplicationDomain]
            public void WithTimeWin() => RegressionRunner.Run(_session, ViewUnion.WithTimeWin());

            [Test, RunInApplicationDomain]
            public void WithSorted() => RegressionRunner.Run(_session, ViewUnion.WithSorted());

            [Test, RunInApplicationDomain]
            public void WithTwoUnique() => RegressionRunner.Run(_session, ViewUnion.WithTwoUnique());

            [Test, RunInApplicationDomain]
            public void WithPattern() => RegressionRunner.Run(_session, ViewUnion.WithPattern());

            [Test, RunInApplicationDomain]
            public void WithThreeUnique() => RegressionRunner.Run(_session, ViewUnion.WithThreeUnique());

            [Test, RunInApplicationDomain]
            public void WithGroupBy() => RegressionRunner.Run(_session, ViewUnion.WithGroupBy());

            [Test, RunInApplicationDomain]
            public void WithAndDerivedValue() => RegressionRunner.Run(_session, ViewUnion.WithAndDerivedValue());

            [Test, RunInApplicationDomain]
            public void WithBatchWindow() => RegressionRunner.Run(_session, ViewUnion.WithBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithFirstUniqueAndFirstLength() => RegressionRunner.Run(_session, ViewUnion.WithFirstUniqueAndFirstLength());
        }

        /// <summary>
        /// Auto-test(s): ViewUnique
        /// <code>
        /// RegressionRunner.Run(_session, ViewUnique.Executions());
        /// </code>
        /// </summary>

        public class TestViewUnique : AbstractTestBase
        {
            public TestViewUnique() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUniqueTwoWindows() => RegressionRunner.Run(_session, ViewUnique.WithUniqueTwoWindows());

            [Test, RunInApplicationDomain]
            public void WithUniqueExpressionParameter() => RegressionRunner.Run(_session, ViewUnique.WithUniqueExpressionParameter());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueWithAnnotationPrefix() => RegressionRunner.Run(_session, ViewUnique.WithLastUniqueWithAnnotationPrefix());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueSceneTwo() => RegressionRunner.Run(_session, ViewUnique.WithLastUniqueSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithLastUniqueSceneOne() => RegressionRunner.Run(_session, ViewUnique.WithLastUniqueSceneOne());
        }
    }
} // end of namespace