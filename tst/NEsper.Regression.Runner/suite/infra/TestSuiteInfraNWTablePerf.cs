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
using com.espertech.esper.regressionlib.suite.infra.nwtable;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.infra
{
    [TestFixture]
    public class TestSuiteInfraNWTablePerf : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[] {typeof(SupportBean)}) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction("justCount", typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter), "JustCount");
        }

        /// <summary>
        /// Auto-test(s): InfraNWTableFAFIndexPerfWNoQueryPlanLog
        /// <code>
        /// RegressionRunner.Run(_session, InfraNWTableFAFIndexPerfWNoQueryPlanLog.Executions());
        /// </code>
        /// </summary>

        public class TestInfraNWTableFAFIndexPerfWNoQueryPlanLog : AbstractTestBase
        {
            public TestInfraNWTableFAFIndexPerfWNoQueryPlanLog() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInKeywordSingleIndex() => RegressionRunner.RunPerformanceSensitive(
                _session,
                InfraNWTableFAFIndexPerfWNoQueryPlanLog.WithInKeywordSingleIndex());

            [Test, RunInApplicationDomain]
            public void WithKeyPerformance() => RegressionRunner.RunPerformanceSensitive(
                _session,
                InfraNWTableFAFIndexPerfWNoQueryPlanLog.WithKeyPerformance());

            [Test, RunInApplicationDomain]
            public void WithRangePerformance() => RegressionRunner.RunPerformanceSensitive(
                _session,
                InfraNWTableFAFIndexPerfWNoQueryPlanLog.WithRangePerformance());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRangePerformance() => RegressionRunner.RunPerformanceSensitive(
                _session,
                InfraNWTableFAFIndexPerfWNoQueryPlanLog.WithKeyAndRangePerformance());

            [Test, RunInApplicationDomain]
            public void WithKeyBTreePerformance() => RegressionRunner.RunPerformanceSensitive(
                _session,
                InfraNWTableFAFIndexPerfWNoQueryPlanLog.WithKeyBTreePerformance());
        }
    }
} // end of namespace