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

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// Copy method for Map-underlying events.
    /// </summary>
    public class MapEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly MapEventType _mapEventType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="mapEventType">map event type</param>
        /// <param name="eventAdapterService">for copying events</param>
        public MapEventBeanCopyMethod(MapEventType mapEventType,
                                      EventAdapterService eventAdapterService)
        {
            _mapEventType = mapEventType;
            _eventAdapterService = eventAdapterService;
        }

        #region EventBeanCopyMethod Members

        public EventBean Copy(EventBean theEvent)
        {
            var mapped = (MappedEventBean) theEvent;
            IDictionary<string, object> props = mapped.Properties;
            return _eventAdapterService.AdapterForTypedMap(new Dictionary<String, Object>(props), _mapEventType);
        }

        #endregion
    }
}