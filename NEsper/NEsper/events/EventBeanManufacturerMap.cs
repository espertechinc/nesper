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
using com.espertech.esper.events.map;

namespace com.espertech.esper.events
{
    /// <summary>Factory for Map-underlying events. </summary>
    public class EventBeanManufacturerMap : EventBeanManufacturer
    {
        private readonly MapEventType _mapEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly IList<WriteablePropertyDescriptor> _writables;
    
        /// <summary>Ctor. </summary>
        /// <param name="mapEventType">type to create</param>
        /// <param name="eventAdapterService">event factory</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerMap(MapEventType mapEventType, EventAdapterService eventAdapterService, IList<WriteablePropertyDescriptor> properties)
        {
            _eventAdapterService = eventAdapterService;
            _mapEventType = mapEventType;
            _writables = properties;
        }
    
        public EventBean Make(Object[] properties)
        {
            var values = (IDictionary<string, object>) MakeUnderlying(properties);
            return _eventAdapterService.AdapterForTypedMap(values, _mapEventType);
        }

        public object MakeUnderlying(Object[] properties)
        {
            IDictionary<String, Object> values = new Dictionary<String, Object>();
            for (int i = 0; i < properties.Length; i++)
            {
                values.Put(_writables[i].PropertyName, properties[i]);
            }

            return values;
        }
    }
}
