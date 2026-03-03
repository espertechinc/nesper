using com.espertech.esper.container;
using com.espertech.esperio.support.util;
using com.espertech.esper.compat.threading.locks;

using NUnit.Framework;

namespace com.espertech.esperio
{
    public class AbstractIOTest
    {
        protected IContainer container;
        protected ILockManager LockManager { get; private set; }

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
            LockManager = container.LockManager();
        }
    }
}