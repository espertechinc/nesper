///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Writer method for writing to Map-type events.
    /// </summary>
    public class MapEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly string[] properties;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">names of properties to write</param>
        public MapEventBeanWriterSimpleProps(string[] properties)
        {
            this.properties = properties;
        }

        /// <summary>
        ///     Write values to an event.
        /// </summary>
        /// <param name="values">to write</param>
        /// <param name="theEvent">to write to</param>
        public void Write(object[] values, EventBean theEvent)
        {
            var mappedEvent = (MappedEventBean) theEvent;
            var map = mappedEvent.Properties;

            for (var i = 0; i < properties.Length; i++) {
                map.Put(properties[i], values[i]);
            }
        }
    }
} // end of namespace