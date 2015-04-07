///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Getter for an array of event bean using a nested getter.
    /// </summary>
    public class ObjectArrayEventBeanArrayIndexedElementPropertyGetter 
        : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly int _index;
        private readonly EventPropertyGetter _nestedGetter;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        /// <param name="nestedGetter">nested getter</param>
        public ObjectArrayEventBeanArrayIndexedElementPropertyGetter(int propertyIndex, int index, EventPropertyGetter nestedGetter)
        {
            _propertyIndex = propertyIndex;
            _index = index;
            _nestedGetter = nestedGetter;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            EventBean[] wrapper = (EventBean[]) array[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyValue(wrapper, _index, _nestedGetter);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object Get(EventBean eventBean)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetFragment(EventBean obj)
        {
            EventBean[] wrapper = (EventBean[]) BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyFragment(wrapper, _index, _nestedGetter);
        }
    }
}
