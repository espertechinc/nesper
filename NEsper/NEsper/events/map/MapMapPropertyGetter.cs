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
    using Map = IDictionary<string, object>;

    /// <summary>
    /// A getter that interrogates a given property in a map which may itself contain nested 
    /// maps or indexed entries.
    /// </summary>
    public class MapMapPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyMap;
        private readonly MapEventPropertyGetter _getter;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyMap">is the property returning the map to interrogate</param>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        public MapMapPropertyGetter(String propertyMap, MapEventPropertyGetter getter)
        {
            if (getter == null)
            {
                throw new ArgumentException("Getter is a required parameter");
            }
            _propertyMap = propertyMap;
            _getter = getter;
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            var valueTopObj = map.Get(_propertyMap) as Map;
            if (valueTopObj == null)
            {
                return null;
            }
            return _getter.GetMap(valueTopObj);
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            var valueTopObj = map.Get(_propertyMap) as Map;
            if (valueTopObj == null)
            {
                return false;
            }
            return _getter.IsMapExistsProperty(valueTopObj);
        }
    
        public Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
