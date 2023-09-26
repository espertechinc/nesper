///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;


namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    /// Descriptor of a property item.
    /// </summary>
    public class PropertySetDescriptorItem
    {
        private EventPropertyDescriptor propertyDescriptor;
        private EventPropertyGetterSPI propertyGetter;
        private FragmentEventType fragmentEventType;

        public PropertySetDescriptorItem(
            EventPropertyDescriptor propertyDescriptor,
            EventPropertyGetterSPI propertyGetter,
            FragmentEventType fragmentEventType)
        {
            this.propertyDescriptor = propertyDescriptor;
            this.propertyGetter = propertyGetter;
            this.fragmentEventType = fragmentEventType;
        }

        public EventPropertyDescriptor PropertyDescriptor => propertyDescriptor;

        public EventPropertyGetterSPI PropertyGetter => propertyGetter;

        public FragmentEventType FragmentEventType => fragmentEventType;
    }
} // end of namespace