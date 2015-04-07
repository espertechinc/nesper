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
    /// <summary>Getter for array events. </summary>
    public class ObjectArrayEventBeanArrayIndexedPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly int _index;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">array index</param>
        public ObjectArrayEventBeanArrayIndexedPropertyGetter(int propertyIndex, int index)
        {
            _propertyIndex = propertyIndex;
            _index = index;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var wrapper = (EventBean[]) array[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyUnderlying(wrapper, _index);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true;
        }
    
        public Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetFragment(EventBean obj)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            EventBean[] wrapper = (EventBean[]) array[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyBean(wrapper, _index);
        }
    }
}
