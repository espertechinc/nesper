using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryWrapperWithProps : EventBeanAdapterFactory
        {
            private readonly WrapperEventType eventType;
            private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
            private readonly EventBeanAdapterFactory factoryWrapped;

            public EventBeanAdapterFactoryWrapperWithProps(
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
                var pair = (Pair<object, IDictionary<string, object>>)underlying;
                var inner = factoryWrapped.MakeAdapter(pair.First);
                return eventBeanTypedEventFactory.AdapterForTypedWrapper(inner, pair.Second, eventType);
            }
        }
    }
}