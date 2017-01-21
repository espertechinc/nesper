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
    /// <summary>Getter for array events. </summary>
    public class MapEventBeanArrayIndexedPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        private readonly int _index;

        /// <summary>Ctor. </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">array index</param>
        public MapEventBeanArrayIndexedPropertyGetter(String propertyName, int index)
        {
            _propertyName = propertyName;
            _index = index;
        }

        public Object GetMap(IDictionary<String, Object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[])map.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyUnderlying(wrapper, _index);
        }

        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true;
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
            var wrapper = (EventBean[])map.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyBean(wrapper, _index);
        }
    }
}
