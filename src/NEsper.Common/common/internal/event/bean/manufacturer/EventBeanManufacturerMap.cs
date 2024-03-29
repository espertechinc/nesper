///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for Map-underlying events.
    /// </summary>
    public class EventBeanManufacturerMap : EventBeanManufacturer
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly MapEventType mapEventType;
        private readonly WriteablePropertyDescriptor[] writables;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="mapEventType">type to create</param>
        /// <param name="eventAdapterService">event factory</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerMap(
            MapEventType mapEventType,
            EventBeanTypedEventFactory eventAdapterService,
            WriteablePropertyDescriptor[] properties)
        {
            this.eventAdapterService = eventAdapterService;
            this.mapEventType = mapEventType;
            writables = properties;
        }

        public EventBean Make(object[] properties)
        {
            var values = MakeUnderlying(properties);
            return eventAdapterService.AdapterForTypedMap(values, mapEventType);
        }

        object EventBeanManufacturer.MakeUnderlying(object[] properties)
        {
            return MakeUnderlying(properties);
        }

        public IDictionary<string, object> MakeUnderlying(object[] properties)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            for (var i = 0; i < properties.Length; i++) {
                values.Put(writables[i].PropertyName, properties[i]);
            }

            return values;
        }
    }
} // end of namespace