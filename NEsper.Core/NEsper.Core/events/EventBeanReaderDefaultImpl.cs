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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Reader implementation that utilizes event property getters and thereby works with all
    /// event types regardsless of whether a type returns an event reader when asked for.
    /// </summary>
    public class EventBeanReaderDefaultImpl : EventBeanReader
    {
        private readonly EventPropertyGetter[] _gettersArray;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">the type of events to read</param>
        public EventBeanReaderDefaultImpl(EventType eventType)
        {
            var properties = eventType.PropertyNames;
            var getters = new List<EventPropertyGetter>();
            foreach (var property in properties)
            {
                var getter = eventType.GetGetter(property);
                if (getter != null)
                {
                    getters.Add(getter);
                }
            }
            _gettersArray = getters.ToArray();
        }

        public Object[] Read(EventBean theEvent)
        {
            var values = new Object[_gettersArray.Length];
            for (var i = 0; i < _gettersArray.Length; i++)
            {
                values[i] = _gettersArray[i].Get(theEvent);
            }
            return values;
        }
    }
} // end of namespace
