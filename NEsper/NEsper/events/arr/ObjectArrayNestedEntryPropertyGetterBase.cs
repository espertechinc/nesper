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
    public abstract class ObjectArrayNestedEntryPropertyGetterBase : ObjectArrayEventPropertyGetter
    {
        protected readonly int PropertyIndex;
        protected readonly EventType FragmentType;
        protected readonly EventAdapterService EventAdapterService;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        protected ObjectArrayNestedEntryPropertyGetterBase(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            PropertyIndex = propertyIndex;
            FragmentType = fragmentType;
            EventAdapterService = eventAdapterService;
        }

        public abstract Object HandleNestedValue(Object value);
        public abstract bool HandleNestedValueExists(Object value);
        public abstract Object HandleNestedValueFragment(Object value);

        public Object GetObjectArray(Object[] array)
        {
            var value = array[PropertyIndex];
            if (value == null)
            {
                return null;
            }
            return HandleNestedValue(value);
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
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            var value = array[PropertyIndex];
            if (value == null)
            {
                return false;
            }
            return HandleNestedValueExists(value);
        }

        public Object GetFragment(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            var value = array[PropertyIndex];
            if (value == null)
            {
                return null;
            }
            return HandleNestedValueFragment(value);
        }
    }
}
