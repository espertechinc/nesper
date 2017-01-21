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
    /// Getter for a dynamic indexed property for maps.
    /// </summary>
    public class ObjectArrayIndexedPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly int _index;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="index">index to get the element at</param>
        public ObjectArrayIndexedPropertyGetter(int propertyIndex, int index)
        {
            _propertyIndex = propertyIndex;
            _index = index;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            Object value = array[_propertyIndex];
            return BaseNestableEventUtil.GetIndexedValue(value, _index);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            Object value = array[_propertyIndex];
            return BaseNestableEventUtil.IsExistsIndexedValue(value, _index);
        }
    
        public Object Get(EventBean eventBean)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsObjectArrayExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean));
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
