///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Descriptor of a property set.
    /// </summary>
    public class PropertySetDescriptor
    {
        public PropertySetDescriptor(
            IList<string> propertyNameList,
            IList<EventPropertyDescriptor> propertyDescriptors,
            IDictionary<string, PropertySetDescriptorItem> propertyItems,
            IDictionary<string, object> nestableTypes)
        {
            PropertyNameList = propertyNameList;
            PropertyDescriptors = propertyDescriptors;
            PropertyItems = propertyItems;
            NestableTypes = nestableTypes;
        }

        public IDictionary<string, PropertySetDescriptorItem> PropertyItems { get; }

        /// <summary>Returns property name list. </summary>
        /// <value>property name list</value>
        public IList<string> PropertyNameList { get; }

        /// <summary>Returns the property descriptors. </summary>
        /// <value>property descriptors</value>
        public IList<EventPropertyDescriptor> PropertyDescriptors { get; }

        public IDictionary<string, object> NestableTypes { get; }

        public string[] PropertyNameArray => PropertyNameList.ToArray();
    }
}