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

namespace com.espertech.esper.events.map
{
    public abstract class MapNestedEntryPropertyGetterBase : MapEventPropertyGetter
    {
        protected readonly String PropertyMap;
        protected readonly EventType FragmentType;
        protected readonly EventAdapterService EventAdapterService;

        /// <summary>Ctor. </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        protected MapNestedEntryPropertyGetterBase(String propertyMap, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            PropertyMap = propertyMap;
            FragmentType = fragmentType;
            EventAdapterService = eventAdapterService;
        }

        public abstract Object HandleNestedValue(Object value);
        public abstract Object HandleNestedValueFragment(Object value);

        public Object GetMap(IDictionary<String, Object> map)
        {
            Object value = map.Get(PropertyMap);
            if (value == null)
            {
                return null;
            }
            return HandleNestedValue(value);
        }

        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetFragment(EventBean obj)
        {
            IDictionary<String, Object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            Object value = map.Get(PropertyMap);
            if (value == null)
            {
                return null;
            }
            return HandleNestedValueFragment(value);
        }
    }
}
