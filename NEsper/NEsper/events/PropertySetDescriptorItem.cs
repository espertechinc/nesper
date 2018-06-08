///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    ///     Descriptor of a property item.
    /// </summary>
    public class PropertySetDescriptorItem
    {
        public PropertySetDescriptorItem(
            EventPropertyDescriptor propertyDescriptor,
            Type simplePropertyType,
            EventPropertyGetterSPI propertyGetter,
            FragmentEventType fragmentEventType)
        {
            PropertyDescriptor = propertyDescriptor;
            SimplePropertyType = simplePropertyType;
            PropertyGetter = propertyGetter;
            FragmentEventType = fragmentEventType;
        }

        public EventPropertyDescriptor PropertyDescriptor { get; private set; }

        public Type SimplePropertyType { get; private set; }

        public EventPropertyGetterSPI PropertyGetter { get; private set; }

        public FragmentEventType FragmentEventType { get; private set; }
    }
}