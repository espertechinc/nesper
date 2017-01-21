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
    /// Getter for a dynamic mappeds property for maps.
    /// </summary>
    public class ObjectArrayMappedPropertyGetter : ObjectArrayEventPropertyGetterAndMapped
    {
        private readonly int _propertyIndex;
        private readonly String _key;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="key">get the element at</param>
        public ObjectArrayMappedPropertyGetter(int propertyIndex, String key)
        {
            _propertyIndex = propertyIndex;
            _key = key;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            return GetObjectArrayInternal(array, _key);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            Object value = array[_propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyExists(value, _key);
        }
    
        public Object Get(EventBean eventBean, String mapKey)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArrayInternal(data, mapKey);
        }
    
        public Object Get(EventBean eventBean)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(data);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            Object[] data = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsObjectArrayExistsProperty(data);
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    
        private Object GetObjectArrayInternal(Object[] objectArray, String providedKey)
        {
            Object value = objectArray[_propertyIndex];
            return BaseNestableEventUtil.GetMappedPropertyValue(value, providedKey);
        }
    }
}
