///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.suite.multithread;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.multithread
{
    /// <summary>
    /// When running with a shared/default configuration place test in <seealso cref="TestSuiteMultithread" />since these tests share the runtime via session.
    /// <para />When running with a configuration derived from the default configuration "SupportConfigFactory", use:
    /// <pre>RegressionRunner.RunConfigurable</pre><para />When running with a fully custom configuration, use a separate runtime instance but obtain the base
    /// configuration from SupportConfigFactory:
    /// <pre>new XXX().Run(config)</pre></summary>
    [TestFixture]
    public class TestSuiteMultithreadWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestMultithreadPatternTimer()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadPatternTimer());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextDBAccess()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadContextDBAccess());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextMultiStmtStartEnd()
        {
            using (new PerformanceContext()) {
                new MultithreadContextMultiStmtStartEnd().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextNestedNonOverlapAtNow()
        {
            using (new PerformanceContext()) {
                new MultithreadContextNestedNonOverlapAtNow().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextTerminated()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadContextTerminated());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeterminismInsertIntoLockConfig()
        {
            using (new PerformanceContext()) {
                new MultithreadDeterminismInsertIntoLockConfig().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeterminismListener()
        {
            using (new PerformanceContext()) {
                new MultithreadDeterminismListener().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadInsertIntoTimerConcurrency()
        {
            using (new PerformanceContext()) {
                new MultithreadInsertIntoTimerConcurrency().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtListenerAddRemove()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadStmtListenerAddRemove());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowPriority()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadStmtNamedWindowPriority());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtPatternFollowedByReadMostly()
        {
            using (new PerformanceContext()) {
                new MultithreadStmtPatternFollowedBy().RunReadMostly(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtPatternFollowedByReadWrite()
        {
            using (new PerformanceContext()) {
                new MultithreadStmtPatternFollowedBy().RunReadWrite(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowUniqueTwoWJoinConsumer()
        {
            using (new PerformanceContext()) {
                new MultithreadStmtNamedWindowUniqueTwoWJoinConsumer().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextOverlapDistinct()
        {
            using (new PerformanceContext()) {
                new MultithreadContextOverlapDistinct().Run(SupportConfigFactory.GetConfiguration());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextPartitionedWTerm()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadContextPartitionedWTerm());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextStartedBySameEvent()
        {
            using (new PerformanceContext()) {
                RegressionRunner.RunConfigurable(new MultithreadContextStartedBySameEvent());
            }
        }
    }
} // end of namespace