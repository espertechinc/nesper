///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Getter for map array.
    /// </summary>
    public class ObjectArrayFragmentArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;
    
        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">event type of fragment</param>
        /// <param name="eventAdapterService">for creating event instances</param>
        public ObjectArrayFragmentArrayPropertyGetter(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            return array[_propertyIndex];
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
            return true;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            if (value is EventBean[])
            {
                return value;
            }

            if (value is Object[])
            {
                var objectArray = (Object[]) value;
                if (Enumerable.All(objectArray, av => av == null || av is EventBean))
                {
                    return Enumerable.Cast<object>(objectArray).ToArray();
                }
            }

            return BaseNestableEventUtil.GetFragmentArray(_eventAdapterService, value, _fragmentEventType);
        }
    }
}
