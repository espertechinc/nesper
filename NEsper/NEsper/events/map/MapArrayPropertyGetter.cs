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
    /// <summary>
    /// Getter for Map-entries with well-defined fragment type.
    /// </summary>
    public class MapArrayPropertyGetter 
        : MapEventPropertyGetter
        , MapEventPropertyGetterAndIndexed
    {
        private readonly String _propertyName;
        private readonly int _index;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyNameAtomic">property name</param>
        /// <param name="index">array index</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public MapArrayPropertyGetter(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            _propertyName = propertyNameAtomic;
            _index = index;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true;
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            return GetMapInternal(map, _index);
        }
    
        public Object Get(EventBean eventBean, int index)
        {
            IDictionary<String, Object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
        }
    
        public Object Get(EventBean eventBean)
        {
            IDictionary<String, Object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMap(map);
        }
    
        private Object GetMapInternal(IDictionary<String, Object> map, int index)
        {
            Object value = map.Get(_propertyName);
            return BaseNestableEventUtil.GetIndexedValue(value, index);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }
    
        public Object GetFragment(EventBean obj)
        {
            Object fragmentUnderlying = Get(obj);
            return BaseNestableEventUtil.GetFragmentNonPono(_eventAdapterService, fragmentUnderlying, _fragmentType);
        }
    }
}
