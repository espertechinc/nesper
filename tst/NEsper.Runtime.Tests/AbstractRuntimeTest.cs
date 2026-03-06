using com.espertech.esper.compat.function;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime
{
    public class AbstractRuntimeTest
    {
        private Supplier<SupportEventTypeFactory> supportEventTypeFactorySupplier;

        protected IContainer Container;

        protected IReaderWriterLockManager rwLockManager => Container.Resolve<IReaderWriterLockManager>();

        protected FilterServiceGranularLockFactoryReentrant MakeGranularLockFactory()
        {
            return new FilterServiceGranularLockFactoryReentrant(rwLockManager);
        }

        protected FilterServiceLockCoarse MakeFilterServiceLockCoarse()
        {
            return new FilterServiceLockCoarse(rwLockManager, -1);
        }

        protected SupportEventTypeFactory supportEventTypeFactory =>
            supportEventTypeFactorySupplier.Invoke();

        [SetUp]
        public virtual void SetUpCommon()
        {
            Container = CreateContainer();
            supportEventTypeFactorySupplier = Suppliers.Memoize(() => 
                SupportEventTypeFactory.GetInstance(Container));
        }

        protected virtual IContainer CreateContainer()
        {
            return SupportContainer.CreateContainer();
        }
    }
}