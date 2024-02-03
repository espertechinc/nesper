using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryWrapperNoProps : EventBeanAdapterFactory
        {
            private readonly WrapperEventType eventType;
            private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
            private readonly EventBeanAdapterFactory factoryWrapped;

            public EventBeanAdapterFactoryWrapperNoProps(
                EventType eventType,
                EventBeanTypedEventFactory eventBeanTypedEventFactory,
                EventBeanAdapterFactory factoryWrapped)
            {
                this.eventType = (WrapperEventType)eventType;
                this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
                this.factoryWrapped = factoryWrapped;
            }

            public EventBean MakeAdapter(object underlying)
            {
                var inner = factoryWrapped.MakeAdapter(underlying);
                return eventBeanTypedEventFactory.AdapterForTypedWrapper(
                    inner,
                    EmptyDictionary<string, object>.Instance,
                    eventType);
            }
        }
    }
}