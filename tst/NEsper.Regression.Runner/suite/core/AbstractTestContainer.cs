using System;

using com.espertech.esper.container;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.core
{
    public class AbstractTestContainer
    {
        // Unique identifier for the "test container"
        public readonly string Id = Guid.NewGuid().ToString();
        // the container
        private IContainer _container;
        public IContainer Container {
            get {
                lock (this) {
                    if (_container == null) {
                        _container = SupportContainer.CreateContainer();
                    }

                    return _container;
                }
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (_container is IDisposable disposable) {
                disposable.Dispose();
            }
            _container = null;
        }
    }
}