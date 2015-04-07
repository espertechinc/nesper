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
    /// A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class MapEventBeanPropertyGetter
        : MapEventPropertyGetter
    {
        private readonly String _propertyName;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        public MapEventBeanPropertyGetter(String propertyName) {
            _propertyName = propertyName;
        }

        public Object GetMap(DataMap asMap)
        {
            var theEvent = asMap.Get(_propertyName) as EventBean;
            if (theEvent == null) {
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            return theEvent.Underlying;
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
            return BaseNestableEventUtil.CheckedCastUnderlyingMap(obj).Get(_propertyName);
        }
    }
}
