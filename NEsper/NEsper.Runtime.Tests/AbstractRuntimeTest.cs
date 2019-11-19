using com.espertech.esper.compat.function;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime
{
    public class AbstractRuntimeTest
    {
        private Supplier<SupportEventTypeFactory> supportEventTypeFactorySupplier;

        protected internal IContainer container;

        protected internal SupportEventTypeFactory supportEventTypeFactory =>
            supportEventTypeFactorySupplier.Invoke();

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
            supportEventTypeFactorySupplier = Suppliers.Memoize(() => 
                SupportEventTypeFactory.GetInstance(container));
        }
    }
}