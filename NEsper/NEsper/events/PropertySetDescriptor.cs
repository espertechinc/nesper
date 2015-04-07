///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Descriptor of a property set.
    /// </summary>
    public class PropertySetDescriptor
    {
        public PropertySetDescriptor(
            IList<string> propertyNameList,
            IList<EventPropertyDescriptor> propertyDescriptors,
            IDictionary<String, PropertySetDescriptorItem> propertyItems,
            IDictionary<String, Object> nestableTypes)
        {
            PropertyNameList = propertyNameList;
            PropertyDescriptors = propertyDescriptors;
            PropertyItems = propertyItems;
            NestableTypes = nestableTypes;
        }

        public IDictionary<string, PropertySetDescriptorItem> PropertyItems { get; private set; }

        /// <summary>Returns property name list. </summary>
        /// <value>property name list</value>
        public IList<string> PropertyNameList { get; private set; }

        /// <summary>Returns the property descriptors. </summary>
        /// <value>property descriptors</value>
        public IList<EventPropertyDescriptor> PropertyDescriptors { get; private set; }

        public IDictionary<string, object> NestableTypes { get; private set; }

        public string[] PropertyNameArray
        {
            get { return PropertyNameList.ToArray(); }
        }
    }
}