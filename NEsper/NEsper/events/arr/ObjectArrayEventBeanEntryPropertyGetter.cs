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
    /// A getter that works on EventBean events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayEventBeanEntryPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly EventPropertyGetter _eventBeanEntryGetter;
        private readonly int _propertyIndex;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventBeanEntryGetter">the getter for the map entry</param>
        public ObjectArrayEventBeanEntryPropertyGetter(int propertyIndex, EventPropertyGetter eventBeanEntryGetter)
        {
            _propertyIndex = propertyIndex;
            _eventBeanEntryGetter = eventBeanEntryGetter;
        }

        #region ObjectArrayEventPropertyGetter Members

        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = array[_propertyIndex];

            if (value == null)
            {
                return null;
            }

            // Object within the map
            var theEvent = (EventBean) value;
            return _eventBeanEntryGetter.Get(theEvent);
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
            // If the map does not contain the key, this is allowed and represented as null
            Object value = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj)[_propertyIndex];

            if (value == null)
            {
                return null;
            }

            // Object within the map
            var theEvent = (EventBean) value;
            return _eventBeanEntryGetter.GetFragment(theEvent);
        }

        #endregion
    }
}