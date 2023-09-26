using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryObjectArray : EventBeanAdapterFactory
        {
            private readonly EventType eventType;
            private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

            public EventBeanAdapterFactoryObjectArray(
                EventType eventType,
                EventBeanTypedEventFactory eventBeanTypedEventFactory)
            {
                this.eventType = eventType;
                this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            }

            public EventBean MakeAdapter(object underlying)
            {
                return eventBeanTypedEventFactory.AdapterForTypedObjectArray((object[])underlying, eventType);
            }
        }
    }
}