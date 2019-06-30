using com.espertech.esper.common.@internal.supportunit.db;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.function;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common
{
    public class CommonTest
    {
        private Supplier<SupportEventTypeFactory> supportEventTypeFactorySupplier;
        private Supplier<SupportExprNodeFactory> supportExprNodeFactorySupplier;
        private Supplier<SupportDatabaseService> supportDatabaseServiceSupplier;
        private Supplier<SupportJoinResultNodeFactory> supportJoinResultNodeFactorySupplier;

        protected internal IContainer container;

        protected internal SupportEventTypeFactory supportEventTypeFactory =>
            supportEventTypeFactorySupplier.Invoke();

        protected internal SupportExprNodeFactory supportExprNodeFactory =>
            supportExprNodeFactorySupplier.Invoke();

        protected internal SupportDatabaseService supportDatabaseService =>
            supportDatabaseServiceSupplier.Invoke();

        protected internal SupportJoinResultNodeFactory supportJoinResultNodeFactory =>
            supportJoinResultNodeFactorySupplier.Invoke();

        [SetUp]
        public virtual void SetUpCommon()
        {
            container = SupportContainer.Reset();
            supportEventTypeFactorySupplier = Suppliers.Memoize(() => 
                SupportEventTypeFactory.GetInstance(container));
            supportExprNodeFactorySupplier = Suppliers.Memoize(() =>
                SupportExprNodeFactory.GetInstance(container));
            supportDatabaseServiceSupplier = Suppliers.Memoize(() =>
                SupportDatabaseService.GetInstance(container));
            supportJoinResultNodeFactorySupplier = Suppliers.Memoize(() =>
                SupportJoinResultNodeFactory.GetInstance(container));
        }
    }
}