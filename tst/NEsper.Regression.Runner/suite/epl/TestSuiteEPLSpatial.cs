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
using com.espertech.esper.regressionlib.suite.epl.spatial;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLSpatial
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
                typeof(SupportBean), typeof(SupportSpatialAABB), typeof(SupportSpatialEventRectangle),
                typeof(SupportSpatialDualAABB), typeof(SupportEventRectangleWithOffset), typeof(SupportSpatialPoint),
                typeof(SupportSpatialDualPoint)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialMXCIFQuadTreeEventIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialMXCIFQuadTreeEventIndex : AbstractTestBase
        {
            public TestEPLSpatialMXCIFQuadTreeEventIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRandomRectsWRandomQuery() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithRandomRectsWRandomQuery());

            [Test, RunInApplicationDomain]
            public void WithRandomIntPointsInSquareUnique() => RegressionRunner.Run(
                _session,
                EPLSpatialMXCIFQuadTreeEventIndex.WithRandomIntPointsInSquareUnique());

            [Test, RunInApplicationDomain]
            public void WithRandomMovingPoints() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithRandomMovingPoints());

            [Test, RunInApplicationDomain]
            public void WithEdgeSubdivide() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithEdgeSubdivide());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideDestroy() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithTableSubdivideDestroy());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideDeepAddDestroy() => RegressionRunner.Run(
                _session,
                EPLSpatialMXCIFQuadTreeEventIndex.WithTableSubdivideDeepAddDestroy());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideMergeDestroy() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithTableSubdivideMergeDestroy());

            [Test, RunInApplicationDomain]
            public void WithZeroWidthAndHeight() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithZeroWidthAndHeight());

            [Test, RunInApplicationDomain]
            public void WithTableFireAndForget() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithTableFireAndForget());

            [Test, RunInApplicationDomain]
            public void WithPerformance() => RegressionRunner.RunPerformanceSensitive(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithPerformance());

            [Test, RunInApplicationDomain]
            public void WithUnique() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithUnique());

            [Test, RunInApplicationDomain]
            public void WithOnTriggerNWInsertRemove() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithOnTriggerNWInsertRemove());

            [Test, RunInApplicationDomain]
            public void WithUnindexed() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithUnindexed());

            [Test, RunInApplicationDomain]
            public void WithNamedWindowSimple() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeEventIndex.WithNamedWindowSimple());
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialMXCIFQuadTreeFilterIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeFilterIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialMXCIFQuadTreeFilterIndex : AbstractTestBase
        {
            public TestEPLSpatialMXCIFQuadTreeFilterIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWContext() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeFilterIndex.WithWContext());

            [Test, RunInApplicationDomain]
            public void WithTypeAssertion() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeFilterIndex.WithTypeAssertion());

            [Test, RunInApplicationDomain]
            public void WithPerfPattern() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeFilterIndex.WithPerfPattern());

            [Test, RunInApplicationDomain]
            public void WithPatternSimple() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeFilterIndex.WithPatternSimple());
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialMXCIFQuadTreeInvalid
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialMXCIFQuadTreeInvalid : AbstractTestBase
        {
            public TestEPLSpatialMXCIFQuadTreeInvalid() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithInvalidFilterIndex() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.WithInvalidFilterIndex());

            [Test, RunInApplicationDomain]
            public void WithInvalidMethod() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.WithInvalidMethod());

            [Test, RunInApplicationDomain]
            public void WithInvalidEventIndexRuntime() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.WithInvalidEventIndexRuntime());

            [Test, RunInApplicationDomain]
            public void WithInvalidEventIndexCreate() => RegressionRunner.Run(_session, EPLSpatialMXCIFQuadTreeInvalid.WithInvalidEventIndexCreate());
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialPointRegionQuadTreeEventIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialPointRegionQuadTreeEventIndex : AbstractTestBase
        {
            public TestEPLSpatialPointRegionQuadTreeEventIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSubqNamedWindowIndexShare() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithSubqNamedWindowIndexShare());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideMergeDestroy() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithTableSubdivideMergeDestroy());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideDestroy() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithTableSubdivideDestroy());

            [Test, RunInApplicationDomain]
            public void WithTableSubdivideDeepAddDestroy() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithTableSubdivideDeepAddDestroy());

            [Test, RunInApplicationDomain]
            public void WithTableSimple() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithTableSimple());

            [Test, RunInApplicationDomain]
            public void WithRandomMovingPoints() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithRandomMovingPoints());

            [Test, RunInApplicationDomain]
            public void WithRandomIntPointsInSquareUnique() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithRandomIntPointsInSquareUnique());

            [Test, RunInApplicationDomain]
            public void WithRandomDoublePointsWRandomQuery() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithRandomDoublePointsWRandomQuery());

            [Test, RunInApplicationDomain]
            public void WithEdgeSubdivide() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithEdgeSubdivide());

            [Test, RunInApplicationDomain]
            public void WithExpression() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithExpression());

            [Test, RunInApplicationDomain]
            public void WithOnTriggerContextParameterized() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithOnTriggerContextParameterized());

            [Test, RunInApplicationDomain]
            public void WithTableFireAndForget() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithTableFireAndForget());

            [Test, RunInApplicationDomain]
            public void WithNWFireAndForgetPerformance() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithNWFireAndForgetPerformance());

            [Test, RunInApplicationDomain]
            public void WithChoiceBetweenIndexTypes() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithChoiceBetweenIndexTypes());

            [Test, RunInApplicationDomain]
            public void WithPerformance() => RegressionRunner.RunPerformanceSensitive(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithPerformance());

            [Test, RunInApplicationDomain]
            public void WithUnique() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithUnique());

            [Test, RunInApplicationDomain]
            public void WithChoiceOfTwo() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithChoiceOfTwo());

            [Test, RunInApplicationDomain]
            public void WithOnTriggerTable() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithOnTriggerTable());

            [Test, RunInApplicationDomain]
            public void WithOnTriggerNWInsertRemove() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithOnTriggerNWInsertRemove());

            [Test, RunInApplicationDomain]
            public void WithUnusedNamedWindowFireAndForget() => RegressionRunner.Run(
                _session,
                EPLSpatialPointRegionQuadTreeEventIndex.WithUnusedNamedWindowFireAndForget());

            [Test, RunInApplicationDomain]
            public void WithUnusedOnTrigger() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithUnusedOnTrigger());

            [Test, RunInApplicationDomain]
            public void WithUnindexed() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeEventIndex.WithUnindexed());
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialPointRegionQuadTreeFilterIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialPointRegionQuadTreeFilterIndex : AbstractTestBase
        {
            public TestEPLSpatialPointRegionQuadTreeFilterIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithContext() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithContext());

            [Test, RunInApplicationDomain]
            public void WithPatternSimple() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithPatternSimple());

            [Test, RunInApplicationDomain]
            public void WithTypeAssertion() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithTypeAssertion());

            [Test, RunInApplicationDomain]
            public void WithUnoptimized() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithUnoptimized());

            [Test, RunInApplicationDomain]
            public void WithPerfPattern() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithPerfPattern());

            [Test, RunInApplicationDomain]
            public void WithPerfContextPartition() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithPerfContextPartition());

            [Test, RunInApplicationDomain]
            public void WithPerfStatement() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeFilterIndex.WithPerfStatement());
        }

        /// <summary>
        /// Auto-test(s): EPLSpatialPointRegionQuadTreeInvalid
        /// <code>
        /// RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.Executions());
        /// </code>
        /// </summary>

        public class TestEPLSpatialPointRegionQuadTreeInvalid : AbstractTestBase
        {
            public TestEPLSpatialPointRegionQuadTreeInvalid() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithInvalidFilterIndex() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.WithInvalidFilterIndex());

            [Test, RunInApplicationDomain]
            public void WithInvalidMethod() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.WithInvalidMethod());

            [Test, RunInApplicationDomain]
            public void WithInvalidEventIndexRuntime() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.WithInvalidEventIndexRuntime());

            [Test, RunInApplicationDomain]
            public void WithInvalidEventIndexCreate() => RegressionRunner.Run(_session, EPLSpatialPointRegionQuadTreeInvalid.WithInvalidEventIndexCreate());
        }
    }
} // end of namespace