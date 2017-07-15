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
    /// Getter for map entry.
    /// </summary>
    public abstract class ObjectArrayPropertyGetterDefaultBase : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        protected readonly EventType FragmentEventType;
        protected readonly EventAdapterService EventAdapterService;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        protected ObjectArrayPropertyGetterDefaultBase(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            FragmentEventType = fragmentEventType;
            EventAdapterService = eventAdapterService;
        }

        protected abstract Object HandleCreateFragment(Object value);

        public Object GetObjectArray(Object[] array)
        {
            return array[_propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return array.Length > _propertyIndex;
        }

        public Object Get(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            return HandleCreateFragment(value);
        }
    }
}
