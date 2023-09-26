using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class WrapperEventType
    {
        public class PropertyDescriptorComposite
        {
            private readonly Dictionary<string, EventPropertyDescriptor> propertyDescriptorMap;
            private readonly string[] propertyNames;
            private readonly EventPropertyDescriptor[] descriptors;

            public PropertyDescriptorComposite(
                Dictionary<string, EventPropertyDescriptor> propertyDescriptorMap,
                string[] propertyNames,
                EventPropertyDescriptor[] descriptors)
            {
                this.propertyDescriptorMap = propertyDescriptorMap;
                this.propertyNames = propertyNames;
                this.descriptors = descriptors;
            }

            public Dictionary<string, EventPropertyDescriptor> PropertyDescriptorMap => propertyDescriptorMap;

            public string[] PropertyNames => propertyNames;

            public EventPropertyDescriptor[] Descriptors => descriptors;
        }
    }
}