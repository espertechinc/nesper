///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class MapNestedEntryPropertyGetterArrayMap : MapNestedEntryPropertyGetterBase
    {
        private readonly int _index;
        private readonly MapEventPropertyGetter _getter;

        public MapNestedEntryPropertyGetterArrayMap(String propertyMap, EventType fragmentType, EventAdapterService eventAdapterService, int index, MapEventPropertyGetter getter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _index = index;
            _getter = getter;
        }

        public override Object HandleNestedValue(Object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, _index, _getter);
        }

        public override Object HandleNestedValueFragment(Object value)
        {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapFragment(value, _index, _getter, EventAdapterService, FragmentType);
        }
    }
}
