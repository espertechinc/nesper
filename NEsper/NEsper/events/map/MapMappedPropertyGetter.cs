///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.map
{
    using DataMap = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>
    /// Getter for a dynamic mappeds property for maps.
    /// </summary>
    public class MapMappedPropertyGetter 
        : MapEventPropertyGetter
        , MapEventPropertyGetterAndMapped
    {
        private readonly String _key;
        private readonly String _fieldName;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="key">get the element at</param>
        public MapMappedPropertyGetter(String fieldName, String key)
        {
            _key = key;
            _fieldName = fieldName;
        }
    
        public Object GetMap(DataMap asMap)
        {
            return GetMapInternal(asMap, _key);
        }

        public bool IsMapExistsProperty(DataMap asMap)
        {
            var value = asMap.Get(_fieldName);
            return BaseNestableEventUtil.GetMappedPropertyExists(value, _key);
        }

        public Object Get(EventBean eventBean, String mapKey)
        {
            DataMap data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(data, mapKey);
        }

        public Object Get(EventBean eventBean)
        {
            DataMap data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMap(data);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            DataMap data = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return IsMapExistsProperty(data);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        private Object GetMapInternal(DataMap map, String providedKey)
        {
            Object value = map.Get(_fieldName);
            return BaseNestableEventUtil.GetMappedPropertyValue(value, providedKey);
        }
    }
}
