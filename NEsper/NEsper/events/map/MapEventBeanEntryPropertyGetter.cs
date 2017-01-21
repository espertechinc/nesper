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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.map
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// A getter that works on EventBean events residing within a Map as an event
    /// property.
    /// </summary>
    public class MapEventBeanEntryPropertyGetter 
        : MapEventPropertyGetter
    {
        private readonly String _propertyMap;
        private readonly EventPropertyGetter _eventBeanEntryGetter;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="eventBeanEntryGetter">the getter for the map entry</param>
        public MapEventBeanEntryPropertyGetter(String propertyMap, EventPropertyGetter eventBeanEntryGetter)
        {
            _propertyMap = propertyMap;
            _eventBeanEntryGetter = eventBeanEntryGetter;
        }

        public Object GetMap(DataMap asMap)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = asMap.Get(_propertyMap);
            if (value == null) {
                return null;
            }

            // Object within the map
            return _eventBeanEntryGetter.Get((EventBean) value);
        }

        public bool IsMapExistsProperty(DataMap map)
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
            var asMap = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
    
            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = asMap.Get(_propertyMap) as EventBean;
            if (eventBean == null) {
                return null;
            }

            // Object within the map
            return _eventBeanEntryGetter.GetFragment(eventBean);
        }
    }
}
