///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Writer for wrapper events.
    /// </summary>
    public class WrapperEventBeanMapWriter : EventBeanWriter
    {
        private readonly String[] _properties;
    
        /// <summary>Ctor. </summary>
        /// <param name="properties">to write</param>
        public WrapperEventBeanMapWriter(String[] properties)
        {
            _properties = properties;
        }
    
        public void Write(Object[] values, EventBean theEvent)
        {
            DecoratingEventBean mappedEvent = (DecoratingEventBean) theEvent;
            IDictionary<String, Object> map = mappedEvent.DecoratingProperties;
    
            for (int i = 0; i < _properties.Length; i++)
            {
                map.Put(_properties[i], values[i]);
            }
        }
    }
}
