///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class MapEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly MapEventType mapEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="mapEventType">map event type</param>
        /// <param name="eventAdapterService">for copying events</param>
        public MapEventBeanCopyMethod(
            MapEventType mapEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            this.mapEventType = mapEventType;
            this.eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var mapped = (MappedEventBean) theEvent;
            var props = mapped.Properties;
            return eventAdapterService.AdapterForTypedMap(new Dictionary<string, object>(props), mapEventType);
        }
    }
} // end of namespace