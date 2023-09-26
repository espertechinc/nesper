using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryBean : EventBeanAdapterFactory
        {
            private readonly EventType eventType;
            private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

            public EventBeanAdapterFactoryBean(
                EventType eventType,
                EventBeanTypedEventFactory eventBeanTypedEventFactory)
            {
                this.eventType = eventType;
                this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            }

            public EventBean MakeAdapter(object underlying)
            {
                return eventBeanTypedEventFactory.AdapterForTypedObject(underlying, eventType);
            }
        }
    }
}