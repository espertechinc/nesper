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
    /// Getter for an array of event bean using a nested getter.
    /// </summary>
    public class MapEventBeanArrayIndexedElementPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        private readonly int _index;
        private readonly EventPropertyGetter _nestedGetter;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">array index</param>
        /// <param name="nestedGetter">nested getter</param>
        public MapEventBeanArrayIndexedElementPropertyGetter(String propertyName, int index, EventPropertyGetter nestedGetter)
        {
            _propertyName = propertyName;
            _index = index;
            _nestedGetter = nestedGetter;
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[]) map.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyValue(wrapper, _index, _nestedGetter);
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
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            var wrapper = (EventBean[]) map.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyFragment(wrapper, _index, _nestedGetter);
        }
    }
}
