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
    using Map = IDictionary<string, object>;

    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterMap : MapNestedEntryPropertyGetterBase
    {
        private readonly MapEventPropertyGetter _mapGetter;
    
        public MapNestedEntryPropertyGetterMap(String propertyMap, EventType fragmentType, EventAdapterService eventAdapterService, MapEventPropertyGetter mapGetter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _mapGetter = mapGetter;
        }
    
        public override Object HandleNestedValue(Object value) {
            if (!(value is Map))
            {
                if (value is EventBean) {
                    return _mapGetter.Get((EventBean) value);
                }
                return null;
            }
            return _mapGetter.GetMap((IDictionary<String, Object>) value);
        }
    
        public override Object HandleNestedValueFragment(Object value) {
            if (!(value is Map))
            {
                if (value is EventBean) {
                    return _mapGetter.GetFragment((EventBean) value);
                }
                return null;
            }
    
            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = EventAdapterService.AdapterForTypedMap((IDictionary<String, Object>) value, FragmentType);
            return _mapGetter.GetFragment(eventBean);
        }
    }
}
