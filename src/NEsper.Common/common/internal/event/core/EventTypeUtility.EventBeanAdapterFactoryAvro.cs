using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.avro;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class EventTypeUtility
    {
        public class EventBeanAdapterFactoryAvro : EventBeanAdapterFactory
        {
            private readonly EventType eventType;
            private readonly EventTypeAvroHandler eventTypeAvroHandler;

            public EventBeanAdapterFactoryAvro(
                EventType eventType,
                EventTypeAvroHandler eventTypeAvroHandler)
            {
                this.eventType = eventType;
                this.eventTypeAvroHandler = eventTypeAvroHandler;
            }

            public EventBean MakeAdapter(object underlying)
            {
                return eventTypeAvroHandler.AdapterForTypeAvro(underlying, eventType);
            }
        }
    }
}