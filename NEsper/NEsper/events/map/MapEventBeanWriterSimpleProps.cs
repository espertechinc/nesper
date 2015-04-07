///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.map
{
    /// <summary>Writer method for writing to Map-type events. </summary>
    public class MapEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly String[] _properties;
    
        /// <summary>Ctor. </summary>
        /// <param name="properties">names of properties to write</param>
        public MapEventBeanWriterSimpleProps(String[] properties)
        {
            _properties = properties;
        }
    
        /// <summary>Write values to an event. </summary>
        /// <param name="values">to write</param>
        /// <param name="theEvent">to write to</param>
        public void Write(Object[] values, EventBean theEvent)
        {
            var mappedEvent = (MappedEventBean) theEvent;
            var map = mappedEvent.Properties;
    
            for (int i = 0; i < _properties.Length; i++)
            {
                map.Put(_properties[i], values[i]);
            }
        }
    }
}
