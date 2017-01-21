///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Getter for Map-entries with well-defined fragment type.
    /// </summary>
    public class ObjectArrayArrayPropertyGetter : ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly int _propertyIndex;
        private readonly int _index;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public ObjectArrayArrayPropertyGetter(int propertyIndex, int index, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            _propertyIndex = propertyIndex;
            _index = index;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            return GetObjectArrayInternal(array, _index);
        }
    
        public Object Get(EventBean eventBean, int index)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArrayInternal(array, index);
        }
    
        public Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }
    
        private Object GetObjectArrayInternal(Object[] array, int index)
        {
            Object value = array[_propertyIndex];
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
