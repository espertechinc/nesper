using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryMap : EventBeanAdapterFactory
        {
            private readonly EventType eventType;
            private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

            public EventBeanAdapterFactoryMap(
                EventType eventType,
                EventBeanTypedEventFactory eventBeanTypedEventFactory)
            {
                this.eventType = eventType;
                this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            }

            public EventBean MakeAdapter(object underlying)
            {
                return eventBeanTypedEventFactory.AdapterForTypedMap(
                    (IDictionary<string, object>)underlying,
                    eventType);
            }
        }
    }
}