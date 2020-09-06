using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.core
{
    public class AbstractTestBase
    {
        internal RegressionSession _session;
        internal Action<Configuration> _configure;

        public AbstractTestBase(Action<Configuration> configure)
        {
            _configure = configure;
        }

        public AbstractTestBase()
        {
            _configure = null;
        }

        [SetUp]
        public void SetUp()
        {
            _session = RegressionRunner.Session();
            _configure?.Invoke(_session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Destroy();
            _session = null;
        }
    }
}