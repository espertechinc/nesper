using System;
using System.Reflection;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.core
{
    [Parallelizable(ParallelScope.Self)]
    public class AbstractTestBase : AbstractTestContainer
    {
        private readonly Action<Configuration> _configure;

        internal RegressionSession _session;

        public RegressionSession Session => _session;

        public AbstractTestBase(Action<Configuration> configure)
        {
            _configure = configure;
        }

        public AbstractTestBase()
        {
            _configure = null;

            // Check for a default "Configure" method on the test class
            var testType = GetType();
            var configureMethod = testType.GetMethod("Configure", new Type[] { typeof(Configuration) });
            if (configureMethod != null) {
                if (configureMethod.IsStatic) {
                    _configure = configuration => configureMethod.Invoke(null, new object[] { configuration });
                }
                else {
                    _configure = configuration => configureMethod.Invoke(this, new object[] { configuration });
                }
            }
        }

        protected virtual bool UseDefaultRuntime => false;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _session = RegressionRunner.Session(Container, UseDefaultRuntime);
            _configure?.Invoke(_session.Configuration);
        }

        [TearDown]
        public override void TearDown()
        {
            _session?.Dispose();
            _session = null;
            base.TearDown();
        }
    }
}