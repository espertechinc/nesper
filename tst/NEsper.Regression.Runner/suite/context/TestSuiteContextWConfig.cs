///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.context;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.context
{
    [TestFixture]
    public class TestSuiteContextWConfig : AbstractTestBase
    {
        public TestSuiteContextWConfig() : base(ConfigurePrioritized)
        {
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedPrioritized()
        {
            RegressionRunner.Run(_session, new ContextKeySegmentedPrioritized());
        }

        public static void ConfigurePrioritized(Configuration configuration)
        {
            foreach (var clazz in new Type[] {
                         typeof(SupportBean),
                         typeof(SupportBean_S0),
                         typeof(SupportBean_S1),
                         typeof(SupportBean_S2),
                         typeof(ISupportA),
                         typeof(ISupportB),
                         typeof(ISupportABCImpl),
                         typeof(ISupportAImpl),
                         typeof(ISupportBImpl),
                         typeof(SupportProductIdEvent)
                     }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Runtime.Execution.IsPrioritized = true;
        }

        /// <summary>
        /// Auto-test(s): ContextKeySegmentedWInitTermPrioritized
        /// <code>
        /// RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.Executions());
        /// </code>
        /// </summary>

        public class TestContextKeySegmentedWInitTermPrioritized : AbstractTestBase
        {
            public TestContextKeySegmentedWInitTermPrioritized() : base(ConfigurePrioritized)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithWithCorrelatedTermFilter() =>
                RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithWithCorrelatedTermFilter());

            [Test, RunInApplicationDomain]
            public void WithInitWCorrelatedTermPattern() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithInitWCorrelatedTermPattern());

            [Test, RunInApplicationDomain]
            public void WithInitWCorrelatedTermFilter() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithInitWCorrelatedTermFilter());

            [Test, RunInApplicationDomain]
            public void WithInitNoTerm() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithInitNoTerm());

            [Test, RunInApplicationDomain]
            public void WithInitTermWithTwoInit() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithInitTermWithTwoInit());

            [Test, RunInApplicationDomain]
            public void WithInitTermWithPartitionFilter() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithInitTermWithPartitionFilter());

            [Test, RunInApplicationDomain]
            public void WithInitTermNoPartitionFilter() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithInitTermNoPartitionFilter());

            [Test, RunInApplicationDomain]
            public void WithTermByPattern3Partition() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByPattern3Partition());

            [Test, RunInApplicationDomain]
            public void WithFilterExprTermByFilter() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithFilterExprTermByFilter());

            [Test, RunInApplicationDomain]
            public void WithFilterExprTermByFilterWExpr() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithFilterExprTermByFilterWExpr());

            [Test, RunInApplicationDomain]
            public void WithTermByFilter2Keys() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByFilter2Keys());

            [Test, RunInApplicationDomain]
            public void WithTermByUnrelated() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByUnrelated());

            [Test, RunInApplicationDomain]
            public void WithTermByPatternTwoFilters() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByPatternTwoFilters());

            [Test, RunInApplicationDomain]
            public void WithTermByCrontabOutputWhenTerminated() => RegressionRunner.Run(
                _session,
                ContextKeySegmentedWInitTermPrioritized.WithTermByCrontabOutputWhenTerminated());

            [Test, RunInApplicationDomain]
            public void WithTermByAfter() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByAfter());

            [Test, RunInApplicationDomain]
            public void WithTermByFilterWSecondType() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByFilterWSecondType());

            [Test, RunInApplicationDomain]
            public void WithTermByFilterWSubtype() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByFilterWSubtype());

            [Test, RunInApplicationDomain]
            public void WithTermByFilter() => RegressionRunner.Run(_session, ContextKeySegmentedWInitTermPrioritized.WithTermByFilter());
        }

        /// <summary>
        /// Auto-test(s): ContextInitTermPrioritized
        /// <code>
        /// RegressionRunner.Run(_session, ContextInitTermPrioritized.Executions());
        /// </code>
        /// </summary>

        public class TestContextInitTermPrioritized : AbstractTestBase
        {
            public TestContextInitTermPrioritized() : base(ConfigurePrioritized)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAtNowWithSelectedEventEnding() => RegressionRunner.Run(_session, ContextInitTermPrioritized.WithAtNowWithSelectedEventEnding());

            [Test, RunInApplicationDomain]
            public void WithNonOverlappingSubqueryAndInvalid() => RegressionRunner.Run(_session, ContextInitTermPrioritized.WithNonOverlappingSubqueryAndInvalid());
        }
    }
} // end of namespace