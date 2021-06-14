///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Writer for wrapper events.
    /// </summary>
    public class WrapperEventBeanMapWriter : EventBeanWriter
    {
        private readonly string[] properties;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">to write</param>
        public WrapperEventBeanMapWriter(string[] properties)
        {
            this.properties = properties;
        }

        public void Write(
            object[] values,
            EventBean theEvent)
        {
            var mappedEvent = (DecoratingEventBean) theEvent;
            var map = mappedEvent.DecoratingProperties;

            for (var i = 0; i < properties.Length; i++) {
                map.Put(properties[i], values[i]);
            }
        }
    }
} // end of namespace