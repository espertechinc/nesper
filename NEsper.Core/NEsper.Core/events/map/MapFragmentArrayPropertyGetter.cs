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
    /// <summary>
    /// Getter for map array.
    /// </summary>
    public class MapFragmentArrayPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyNameAtomic">property type</param>
        /// <param name="fragmentEventType">event type of fragment</param>
        /// <param name="eventAdapterService">for creating event instances</param>
        public MapFragmentArrayPropertyGetter(String propertyNameAtomic, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            _propertyName = propertyNameAtomic;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            return map.Get(_propertyName);
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true;
        }
    
        public Object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            if (value is EventBean[])
            {
                return value;
            }
            return BaseNestableEventUtil.GetFragmentArray(_eventAdapterService, value, _fragmentEventType);
        }
    }
}
