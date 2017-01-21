///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Reader implementation that utilizes event property getters and thereby works
    /// with all event types regardsless of whether a type returns an event reader when
    /// asked for.
    /// </summary>
    public class EventBeanReaderDefaultImpl : EventBeanReader
    {
        private EventPropertyGetter[] gettersArray;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">the type of events to read</param>
        public EventBeanReaderDefaultImpl(EventType eventType)
        {
            var properties = eventType.PropertyNames;
            var getters = new List<EventPropertyGetter>();
            foreach (String property in properties)
            {
                EventPropertyGetter getter = eventType.GetGetter(property);
                if (getter != null)
                {
                    getters.Add(getter);
                }
            }
            gettersArray = getters.ToArray();
        }
    
        public Object[] Read(EventBean theEvent)
        {
            var values = new Object[gettersArray.Length];
            for (int i = 0; i < gettersArray.Length; i++)
            {
                values[i] = gettersArray[i].Get(theEvent);
            }
            return values;
        }
    }
}
