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
    /// A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class ObjectArrayEventBeanPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property to get</param>
        public ObjectArrayEventBeanPropertyGetter(int propertyIndex)
        {
            _propertyIndex = propertyIndex;
        }

        #region ObjectArrayEventPropertyGetter Members

        public Object GetObjectArray(Object[] array)
        {
            Object eventBean = array[_propertyIndex];
            if (eventBean == null)
            {
                return null;
            }

            var theEvent = (EventBean) eventBean;
            return theEvent.Underlying;
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
            return BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[_propertyIndex];
        }

        #endregion
    }
}