///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.epl.join;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLJoin
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
            session.Destroy();
            session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamSimple()
        {
            RegressionRunner.Run(session, new EPLJoin2StreamSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinSelectClause()
        {
            RegressionRunner.Run(session, new EPLJoinSelectClause());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin5StreamPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin5StreamPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin2StreamExprPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin2StreamExprPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinDerivedValueViews()
        {
            RegressionRunner.Run(session, new EPLJoinDerivedValueViews());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinNoTableName()
        {
            RegressionRunner.Run(session, new EPLJoinNoTableName());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinInheritAndInterface()
        {
            RegressionRunner.Run(session, new EPLJoinInheritAndInterface());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoinUniqueIndex()
        {
            RegressionRunner.Run(session, new EPLJoinUniqueIndex());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin20Stream()
        {
            RegressionRunner.Run(session, new EPLJoin20Stream());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLJoin3StreamInKeywordPerformance()
        {
            RegressionRunner.Run(session, new EPLJoin3StreamInKeywordPerformance());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinMultiKeyAndRange
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinMultiKeyAndRange.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinMultiKeyAndRange : AbstractTestBase
        {
            public TestEPLJoinMultiKeyAndRange() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayComposite2Prop() => RegressionRunner.Run(
                _session,
                EPLJoinMultiKeyAndRange.WithMultikeyWArrayComposite2Prop());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayCompositeArray() => RegressionRunner.Run(
                _session,
                EPLJoinMultiKeyAndRange.WithMultikeyWArrayCompositeArray());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayHashJoin2Prop() => RegressionRunner.Run(
                _session,
                EPLJoinMultiKeyAndRange.WithMultikeyWArrayHashJoin2Prop());

            [Test, RunInApplicationDomain]
            public void WithMultikeyWArrayHashJoinArray() => RegressionRunner.Run(
                _session,
                EPLJoinMultiKeyAndRange.WithMultikeyWArrayHashJoinArray());

            [Test, RunInApplicationDomain]
            public void WithRangeNullAndDupAndInvalid() => RegressionRunner.Run(
                _session,
                EPLJoinMultiKeyAndRange.WithRangeNullAndDupAndInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin2StreamRangePerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin2StreamRangePerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin2StreamRangePerformance : AbstractTestBase
        {
            public TestEPLJoin2StreamRangePerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUnidirectionalRelOp() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamRangePerformance.WithUnidirectionalRelOp());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRangeInverted() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamRangePerformance.WithKeyAndRangeInverted());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRange() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamRangePerformance.WithKeyAndRange());

            [Test, RunInApplicationDomain]
            public void WithRelationalOp() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamRangePerformance.WithRelationalOp());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRangeOuterJoin() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamRangePerformance.WithKeyAndRangeOuterJoin());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin2StreamSimpleCoercionPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin2StreamSimpleCoercionPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin2StreamSimpleCoercionPerformance : AbstractTestBase
        {
            public TestEPLJoin2StreamSimpleCoercionPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBack() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamSimpleCoercionPerformance.WithBack());

            [Test, RunInApplicationDomain]
            public void WithForward() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamSimpleCoercionPerformance.WithForward());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoin2Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoin2Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoin2Stream : AbstractTestBase
        {
            public TestEPLOuterJoin2Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFullOuterMultikeyWArrayPrimitive() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithFullOuterMultikeyWArrayPrimitive());

            [Test, RunInApplicationDomain]
            public void WithEventType() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithEventType());

            [Test, RunInApplicationDomain]
            public void WithLeftOuterJoin() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithLeftOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithRightOuterJoin() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithRightOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithMultiColumnRightCoercion() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithMultiColumnRightCoercion());

            [Test, RunInApplicationDomain]
            public void WithMultiColumnRight() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithMultiColumnRight());

            [Test, RunInApplicationDomain]
            public void WithMultiColumnLeft() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithMultiColumnLeft());

            [Test, RunInApplicationDomain]
            public void WithMultiColumnLeftOM() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithMultiColumnLeftOM());

            [Test, RunInApplicationDomain]
            public void WithFullOuterJoin() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithFullOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithFullOuterIteratorGroupBy() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithFullOuterIteratorGroupBy());

            [Test, RunInApplicationDomain]
            public void WithRangeOuterJoin() => RegressionRunner.Run(
                _session,
                EPLOuterJoin2Stream.WithRangeOuterJoin());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinEventRepresentation
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinEventRepresentation.Executions());
        /// </code>
        /// </summary>
        public class TestEPLJoinEventRepresentation : AbstractTestBase
        {
            public TestEPLJoinEventRepresentation() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWrapperEventNotUnique() => RegressionRunner.Run(
                _session,
                EPLJoinEventRepresentation.WithWrapperEventNotUnique());

            [Test, RunInApplicationDomain]
            public void WithMapEventNotUnique() => RegressionRunner.Run(
                _session,
                EPLJoinEventRepresentation.WithMapEventNotUnique());

            [Test, RunInApplicationDomain]
            public void WithEventRepresentations() => RegressionRunner.Run(
                _session,
                EPLJoinEventRepresentation.WithEventRepresentations());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinSingleOp3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinSingleOp3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinSingleOp3Stream : AbstractTestBase
        {
            public TestEPLJoinSingleOp3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithdCompile() => RegressionRunner.Run(
                _session,
                EPLJoinSingleOp3Stream.WithdCompile());

            [Test, RunInApplicationDomain]
            public void WithdOM() => RegressionRunner.Run(
                _session,
                EPLJoinSingleOp3Stream.WithdOM());

            [Test, RunInApplicationDomain]
            public void Withd() => RegressionRunner.Run(
                _session,
                EPLJoinSingleOp3Stream.Withd());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin2StreamAndPropertyPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin2StreamAndPropertyPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin2StreamAndPropertyPerformance : AbstractTestBase
        {
            public TestEPLJoin2StreamAndPropertyPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With3Properties() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamAndPropertyPerformance.With3Properties());

            [Test, RunInApplicationDomain]
            public void With2Properties() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamAndPropertyPerformance.With2Properties());

            [Test, RunInApplicationDomain]
            public void WithRemoveStream() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamAndPropertyPerformance.WithRemoveStream());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin2StreamSimplePerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin2StreamSimplePerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin2StreamSimplePerformance : AbstractTestBase
        {
            public TestEPLJoin2StreamSimplePerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithJoinPerformanceStreamB() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamSimplePerformance.WithJoinPerformanceStreamB());

            [Test, RunInApplicationDomain]
            public void WithJoinPerformanceStreamA() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamSimplePerformance.WithJoinPerformanceStreamA());

            [Test, RunInApplicationDomain]
            public void WithPerformanceJoinNoResults() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamSimplePerformance.WithPerformanceJoinNoResults());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin3StreamRangePerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin3StreamRangePerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin3StreamRangePerformance : AbstractTestBase
        {
            public TestEPLJoin3StreamRangePerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithUnidirectionalKeyAndRange() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamRangePerformance.WithUnidirectionalKeyAndRange());

            [Test, RunInApplicationDomain]
            public void WithRangeOnly() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamRangePerformance.WithRangeOnly());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRange() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamRangePerformance.WithKeyAndRange());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinCoercion
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinCoercion.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinCoercion : AbstractTestBase
        {
            public TestEPLJoinCoercion() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withn() => RegressionRunner.Run(
                _session,
                EPLJoinCoercion.Withn());

            [Test, RunInApplicationDomain]
            public void WithnRange() => RegressionRunner.Run(
                _session,
                EPLJoinCoercion.WithnRange());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinStartStop
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinStartStop.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinStartStop : AbstractTestBase
        {
            public TestEPLJoinStartStop() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidJoin() => RegressionRunner.Run(
                _session,
                EPLJoinStartStop.WithInvalidJoin());

            [Test, RunInApplicationDomain]
            public void WithStartStopSceneOne() => RegressionRunner.Run(
                _session,
                EPLJoinStartStop.WithStartStopSceneOne());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinNoWhereClause
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinNoWhereClause.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinNoWhereClause : AbstractTestBase
        {
            public TestEPLJoinNoWhereClause() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNoWhereClause() => RegressionRunner.Run(
                _session,
                EPLJoinNoWhereClause.WithNoWhereClause());

            [Test, RunInApplicationDomain]
            public void WithWInnerKeywordWOOnClause() => RegressionRunner.Run(
                _session,
                EPLJoinNoWhereClause.WithWInnerKeywordWOOnClause());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinPatterns
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinPatterns.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinPatterns : AbstractTestBase
        {
            public TestEPLJoinPatterns() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With2PatternJoinWildcard() => RegressionRunner.Run(
                _session,
                EPLJoinPatterns.With2PatternJoinWildcard());

            [Test, RunInApplicationDomain]
            public void With2PatternJoinSelect() => RegressionRunner.Run(
                _session,
                EPLJoinPatterns.With2PatternJoinSelect());

            [Test, RunInApplicationDomain]
            public void WithPatternFilterJoin() => RegressionRunner.Run(
                _session,
                EPLJoinPatterns.WithPatternFilterJoin());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin2StreamInKeywordPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin2StreamInKeywordPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin2StreamInKeywordPerformance : AbstractTestBase
        {
            public TestEPLJoin2StreamInKeywordPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultiIndexLookup() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamInKeywordPerformance.WithMultiIndexLookup());

            [Test, RunInApplicationDomain]
            public void WithSingleIndexLookup() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin2StreamInKeywordPerformance.WithSingleIndexLookup());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterFullJoin3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterFullJoin3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterFullJoin3Stream : AbstractTestBase
        {
            public TestEPLOuterFullJoin3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withs() => RegressionRunner.Run(
                _session,
                EPLOuterFullJoin3Stream.Withs());

            [Test, RunInApplicationDomain]
            public void WithsMulticolumn() => RegressionRunner.Run(
                _session,
                EPLOuterFullJoin3Stream.WithsMulticolumn());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterInnerJoin3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterInnerJoin3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterInnerJoin3Stream : AbstractTestBase
        {
            public TestEPLOuterInnerJoin3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRightJoinVariantOne() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithRightJoinVariantOne());

            [Test, RunInApplicationDomain]
            public void WithLeftJoinVariantTwo() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithLeftJoinVariantTwo());

            [Test, RunInApplicationDomain]
            public void WithLeftJoinVariantThree() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithLeftJoinVariantThree());

            [Test, RunInApplicationDomain]
            public void WithFullJoinVariantOne() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithFullJoinVariantOne());

            [Test, RunInApplicationDomain]
            public void WithFullJoinVariantTwo() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithFullJoinVariantTwo());

            [Test, RunInApplicationDomain]
            public void WithFullJoinVariantThree() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin3Stream.WithFullJoinVariantThree());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterInnerJoin4Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterInnerJoin4Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterInnerJoin4Stream : AbstractTestBase
        {
            public TestEPLOuterInnerJoin4Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithStarJoinVariantOne() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithStarJoinVariantOne());

            [Test, RunInApplicationDomain]
            public void WithStarJoinVariantTwo() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithStarJoinVariantTwo());

            [Test, RunInApplicationDomain]
            public void WithFullSidedJoinVariantOne() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithFullSidedJoinVariantOne());

            [Test, RunInApplicationDomain]
            public void WithFullSidedJoinVariantTwo() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithFullSidedJoinVariantTwo());

            [Test, RunInApplicationDomain]
            public void WithFullMiddleJoinVariantOne() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithFullMiddleJoinVariantOne());

            [Test, RunInApplicationDomain]
            public void WithFullMiddleJoinVariantTwo() => RegressionRunner.Run(
                _session,
                EPLOuterInnerJoin4Stream.WithFullMiddleJoinVariantTwo());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoin6Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoin6Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoin6Stream : AbstractTestBase
        {
            public TestEPLOuterJoin6Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With5() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With5());

            [Test, RunInApplicationDomain]
            public void With4() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With4());

            [Test, RunInApplicationDomain]
            public void With3() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With3());

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoin6Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoin7Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoin7Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoin7Stream : AbstractTestBase
        {
            public TestEPLOuterJoin7Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRootS6() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS6());

            [Test, RunInApplicationDomain]
            public void WithRootS5() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS5());

            [Test, RunInApplicationDomain]
            public void WithRootS4() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS4());

            [Test, RunInApplicationDomain]
            public void WithRootS3() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS3());

            [Test, RunInApplicationDomain]
            public void WithRootS2() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS2());

            [Test, RunInApplicationDomain]
            public void WithRootS1() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS1());

            [Test, RunInApplicationDomain]
            public void WithRootS0() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithRootS0());

            [Test, RunInApplicationDomain]
            public void WithKeyPerStream() => RegressionRunner.Run(
                _session,
                EPLOuterJoin7Stream.WithKeyPerStream());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinCart4Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinCart4Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinCart4Stream : AbstractTestBase
        {
            public TestEPLOuterJoinCart4Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With3() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart4Stream.With3());

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart4Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart4Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart4Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinCart5Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinCart5Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinCart5Stream : AbstractTestBase
        {
            public TestEPLOuterJoinCart5Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With4Order2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With4Order2());

            [Test, RunInApplicationDomain]
            public void With4() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With4());

            [Test, RunInApplicationDomain]
            public void With3Order2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With3Order2());

            [Test, RunInApplicationDomain]
            public void With3() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With3());

            [Test, RunInApplicationDomain]
            public void With2Order2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With2Order2());

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1Order2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With1Order2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinCart5Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinChain4Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinChain4Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinChain4Stream : AbstractTestBase
        {
            public TestEPLOuterJoinChain4Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With3() => RegressionRunner.Run(
                _session,
                EPLOuterJoinChain4Stream.With3());

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinChain4Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinChain4Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinChain4Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinUnidirectional
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinUnidirectional.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinUnidirectional : AbstractTestBase
        {
            public TestEPLOuterJoinUnidirectional() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOuterInvalid() => RegressionRunner.Run(
                _session,
                EPLOuterJoinUnidirectional.WithOuterInvalid());

            [Test, RunInApplicationDomain]
            public void With4StreamWhereClause() => RegressionRunner.Run(
                _session,
                EPLOuterJoinUnidirectional.With4StreamWhereClause());

            [Test, RunInApplicationDomain]
            public void With3StreamMixed() => RegressionRunner.Run(
                _session,
                EPLOuterJoinUnidirectional.With3StreamMixed());

            [Test, RunInApplicationDomain]
            public void With3StreamAllUnidirectional() => RegressionRunner.Run(
                _session,
                EPLOuterJoinUnidirectional.With3StreamAllUnidirectional());

            [Test, RunInApplicationDomain]
            public void With2Stream() => RegressionRunner.Run(
                _session,
                EPLOuterJoinUnidirectional.With2Stream());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinVarA3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinVarA3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinVarA3Stream : AbstractTestBase
        {
            public TestEPLOuterJoinVarA3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidMulticolumn() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithInvalidMulticolumn());

            [Test, RunInApplicationDomain]
            public void WithRightOuterJoinS1RootS1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithRightOuterJoinS1RootS1());

            [Test, RunInApplicationDomain]
            public void WithRightOuterJoinS2RootS2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithRightOuterJoinS2RootS2());

            [Test, RunInApplicationDomain]
            public void WithLeftOuterJoinRootS0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithLeftOuterJoinRootS0());

            [Test, RunInApplicationDomain]
            public void WithLeftOuterJoinRootS0Compiled() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithLeftOuterJoinRootS0Compiled());

            [Test, RunInApplicationDomain]
            public void WithLeftOuterJoinRootS0OM() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithLeftOuterJoinRootS0OM());

            [Test, RunInApplicationDomain]
            public void WithLeftJoin2SidesMulticolumn() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithLeftJoin2SidesMulticolumn());

            [Test, RunInApplicationDomain]
            public void WithMapLeftJoinUnsortedProps() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarA3Stream.WithMapLeftJoinUnsortedProps());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinVarB3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinVarB3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinVarB3Stream : AbstractTestBase
        {
            public TestEPLOuterJoinVarB3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarB3Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarB3Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarB3Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinVarC3Stream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinVarC3Stream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinVarC3Stream : AbstractTestBase
        {
            public TestEPLOuterJoinVarC3Stream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void With2() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarC3Stream.With2());

            [Test, RunInApplicationDomain]
            public void With1() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarC3Stream.With1());

            [Test, RunInApplicationDomain]
            public void With0() => RegressionRunner.Run(
                _session,
                EPLOuterJoinVarC3Stream.With0());
        }

        /// <summary>
        /// Auto-test(s): EPLOuterJoinLeftWWhere
        /// <code>
        /// RegressionRunner.Run(_session, EPLOuterJoinLeftWWhere.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOuterJoinLeftWWhere : AbstractTestBase
        {
            public TestEPLOuterJoinLeftWWhere() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEventType() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithEventType());

            [Test, RunInApplicationDomain]
            public void WithWhereJoin() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereJoin());

            [Test, RunInApplicationDomain]
            public void WithWhereJoinOrNull() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereJoinOrNull());

            [Test, RunInApplicationDomain]
            public void WithWhereNullEq() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereNullEq());

            [Test, RunInApplicationDomain]
            public void WithWhereNullIs() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereNullIs());

            [Test, RunInApplicationDomain]
            public void WithWhereNotNullNE() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereNotNullNE());

            [Test, RunInApplicationDomain]
            public void WithWhereNotNullIs() => RegressionRunner.Run(
                _session,
                EPLOuterJoinLeftWWhere.WithWhereNotNullIs());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinUnidirectionalStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinUnidirectionalStream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinUnidirectionalStream : AbstractTestBase
        {
            public TestEPLJoinUnidirectionalStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.WithInvalid());

            [Test, RunInApplicationDomain]
            public void With2TableBackwards() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableBackwards());

            [Test, RunInApplicationDomain]
            public void With2TableJoin() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableJoin());

            [Test, RunInApplicationDomain]
            public void With2TableFullOuterJoinBackwards() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableFullOuterJoinBackwards());

            [Test, RunInApplicationDomain]
            public void With2TableFullOuterJoinOM() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableFullOuterJoinOM());

            [Test, RunInApplicationDomain]
            public void With2TableFullOuterJoinCompile() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableFullOuterJoinCompile());

            [Test, RunInApplicationDomain]
            public void With2TableFullOuterJoin() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableFullOuterJoin());

            [Test, RunInApplicationDomain]
            public void With3TableJoinVar3() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableJoinVar3());

            [Test, RunInApplicationDomain]
            public void With3TableJoinVar2B() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableJoinVar2B());

            [Test, RunInApplicationDomain]
            public void With3TableJoinVar2A() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableJoinVar2A());

            [Test, RunInApplicationDomain]
            public void With3TableJoinVar1() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableJoinVar1());

            [Test, RunInApplicationDomain]
            public void WithPatternJoinOutputRate() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.WithPatternJoinOutputRate());

            [Test, RunInApplicationDomain]
            public void WithPatternJoin() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.WithPatternJoin());

            [Test, RunInApplicationDomain]
            public void With3TableOuterJoinVar2() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableOuterJoinVar2());

            [Test, RunInApplicationDomain]
            public void With3TableOuterJoinVar1() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With3TableOuterJoinVar1());

            [Test, RunInApplicationDomain]
            public void With2TableJoinRowForAll() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableJoinRowForAll());

            [Test, RunInApplicationDomain]
            public void With2TableJoinGrouped() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.With2TableJoinGrouped());

            [Test, RunInApplicationDomain]
            public void WithPatternUnidirectionalOuterJoinNoOn() => RegressionRunner.Run(
                _session,
                EPLJoinUnidirectionalStream.WithPatternUnidirectionalOuterJoinNoOn());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin3StreamAndPropertyPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin3StreamAndPropertyPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin3StreamAndPropertyPerformance : AbstractTestBase
        {
            public TestEPLJoin3StreamAndPropertyPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPartialStreams() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamAndPropertyPerformance.WithPartialStreams());

            [Test, RunInApplicationDomain]
            public void WithPartialProps() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamAndPropertyPerformance.WithPartialProps());

            [Test, RunInApplicationDomain]
            public void WithAllProps() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamAndPropertyPerformance.WithAllProps());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin3StreamCoercionPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin3StreamCoercionPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin3StreamCoercionPerformance : AbstractTestBase
        {
            public TestEPLJoin3StreamCoercionPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithThree() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamCoercionPerformance.WithThree());

            [Test, RunInApplicationDomain]
            public void WithTwo() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamCoercionPerformance.WithTwo());

            [Test, RunInApplicationDomain]
            public void WithOne() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamCoercionPerformance.WithOne());
        }

        /// <summary>
        /// Auto-test(s): EPLJoin3StreamOuterJoinCoercionPerformance
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoin3StreamOuterJoinCoercionPerformance.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoin3StreamOuterJoinCoercionPerformance : AbstractTestBase
        {
            public TestEPLJoin3StreamOuterJoinCoercionPerformance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRange() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamOuterJoinCoercionPerformance.WithRange());

            [Test, RunInApplicationDomain]
            public void WithSceneThree() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamOuterJoinCoercionPerformance.WithSceneThree());

            [Test, RunInApplicationDomain]
            public void WithSceneTwo() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamOuterJoinCoercionPerformance.WithSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSceneOne() => RegressionRunner.RunPerformanceSensitive(
                _session,
                EPLJoin3StreamOuterJoinCoercionPerformance.WithSceneOne());
        }

        /// <summary>
        /// Auto-test(s): EPLJoinPropertyAccess
        /// <code>
        /// RegressionRunner.Run(_session, EPLJoinPropertyAccess.Executions());
        /// </code>
        /// </summary>

        public class TestEPLJoinPropertyAccess : AbstractTestBase
        {
            public TestEPLJoinPropertyAccess() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOuterJoin() => RegressionRunner.Run(
                _session,
                EPLJoinPropertyAccess.WithOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithRegularJoin() => RegressionRunner.Run(
                _session,
                EPLJoinPropertyAccess.WithRegularJoin());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_C),
                typeof(SupportBean_D),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_S3),
                typeof(SupportBean_S4),
                typeof(SupportBean_S5),
                typeof(SupportBean_S6),
                typeof(SupportBeanComplexProps),
                typeof(ISupportA),
                typeof(ISupportB),
                typeof(ISupportAImpl),
                typeof(ISupportBImpl),
                typeof(SupportBeanCombinedProps),
                typeof(SupportSimpleBeanOne),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1),
                typeof(SupportBeanRange),
                typeof(SupportEventWithManyArray),
                typeof(SupportEventWithIntArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> typeInfo = new Dictionary<string, object>();
            typeInfo.Put("Id", typeof(string));
            typeInfo.Put("P00", typeof(int));
            configuration.Common.AddEventType("MapS0", typeInfo);
            configuration.Common.AddEventType("MapS1", typeInfo);

            IDictionary<string, object> mapType = new Dictionary<string, object>();
            mapType.Put("col1", typeof(string));
            mapType.Put("col2", typeof(string));
            configuration.Common.AddEventType("Type1", mapType);
            configuration.Common.AddEventType("Type2", mapType);
            configuration.Common.AddEventType("Type3", mapType);

            IDictionary<string, object> typeInfoS0S0 = new Dictionary<string, object>();
            typeInfoS0S0.Put("Id", typeof(string));
            typeInfoS0S0.Put("P00", typeof(int));
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.MAP.GetName(), typeInfoS0S0);

            string[] names = new[] {"Id", "P00"};
            object[] types = new object[] {typeof(string), typeof(int)};
            configuration.Common.AddEventType("S0_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);
            configuration.Common.AddEventType("S1_" + EventUnderlyingType.OBJECTARRAY.GetName(), names, types);

            var schema = SchemaBuilder.Record(
                "name",
                Field("Id", StringType(AvroConstant.PROP_STRING)),
                RequiredInt("P00"));
            configuration.Common.AddEventTypeAvro("S0_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));
            configuration.Common.AddEventTypeAvro("S1_" + EventUnderlyingType.AVRO.GetName(), new ConfigurationCommonEventTypeAvro().SetAvroSchema(schema));

            configuration.Compiler.AddPlugInSingleRowFunction(
                "myStaticEvaluator",
                typeof(EPLJoin2StreamAndPropertyPerformance.MyStaticEval),
                "MyStaticEvaluator");

            configuration.Common.Logging.IsEnableQueryPlan = true;
        }
    }
} // end of namespace