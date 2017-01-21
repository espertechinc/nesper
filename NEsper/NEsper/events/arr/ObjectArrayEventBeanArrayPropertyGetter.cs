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
    /// Returns the event bean or the underlying array.
    /// </summary>
    public class ObjectArrayEventBeanArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly Type _underlyingType;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property to get</param>
        /// <param name="underlyingType">type of property</param>
        public ObjectArrayEventBeanArrayPropertyGetter(int propertyIndex, Type underlyingType)
        {
            _propertyIndex = propertyIndex;
            _underlyingType = underlyingType;
        }
    
        public Object GetObjectArray(Object[] arrayEvent)
        {
            Object innerValue = arrayEvent[_propertyIndex];
            return BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArray(_underlyingType, (EventBean[]) innerValue);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
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
            return array[_propertyIndex];
        }
    }
}
