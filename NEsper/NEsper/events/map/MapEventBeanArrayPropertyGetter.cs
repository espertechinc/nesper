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
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Returns the event bean or the underlying array.
    /// </summary>
    public class MapEventBeanArrayPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        private readonly Type _underlyingType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <param name="underlyingType">type of property</param>
        public MapEventBeanArrayPropertyGetter(String propertyName, Type underlyingType)
        {
            _propertyName = propertyName;
            _underlyingType = underlyingType;
        }

        public Object GetMap(DataMap asMap) 
        {
            // If the map does not contain the key, this is allowed and represented as null
            var mapValue = asMap.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArray(
                _underlyingType, (EventBean[]) mapValue);
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
            return asMap.Get(_propertyName);
        }
    }
}
