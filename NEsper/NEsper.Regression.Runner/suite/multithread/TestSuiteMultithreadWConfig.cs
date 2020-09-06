///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.suite.multithread;
using com.espertech.esper.regressionrun.Runner;

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
            RegressionRunner.RunConfigurable(new MultithreadPatternTimer());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextDBAccess()
        {
            RegressionRunner.RunConfigurable(new MultithreadContextDBAccess());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextMultiStmtStartEnd()
        {
            new MultithreadContextMultiStmtStartEnd().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextNestedNonOverlapAtNow()
        {
            new MultithreadContextNestedNonOverlapAtNow().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextTerminated()
        {
            RegressionRunner.RunConfigurable(new MultithreadContextTerminated());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeterminismInsertIntoLockConfig()
        {
            new MultithreadDeterminismInsertIntoLockConfig().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeterminismListener()
        {
            new MultithreadDeterminismListener().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadInsertIntoTimerConcurrency()
        {
            new MultithreadInsertIntoTimerConcurrency().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtListenerAddRemove()
        {
            RegressionRunner.RunConfigurable(new MultithreadStmtListenerAddRemove());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowPriority()
        {
            RegressionRunner.RunConfigurable(new MultithreadStmtNamedWindowPriority());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtPatternFollowedByReadMostly()
        {
            new MultithreadStmtPatternFollowedBy().RunReadMostly(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtPatternFollowedByReadWrite()
        {
            new MultithreadStmtPatternFollowedBy().RunReadWrite(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowUniqueTwoWJoinConsumer()
        {
            new MultithreadStmtNamedWindowUniqueTwoWJoinConsumer().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextOverlapDistinct()
        {
            new MultithreadContextOverlapDistinct().Run(SupportConfigFactory.GetConfiguration());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextPartitionedWTerm()
        {
            RegressionRunner.RunConfigurable(new MultithreadContextPartitionedWTerm());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextStartedBySameEvent()
        {
            RegressionRunner.RunConfigurable(new MultithreadContextStartedBySameEvent());
        }
    }
} // end of namespace