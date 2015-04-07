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
    public class ObjectArrayNestedEntryPropertyGetterObjectArray : ObjectArrayNestedEntryPropertyGetterBase
    {
        private readonly ObjectArrayEventPropertyGetter _arrayGetter;

        public ObjectArrayNestedEntryPropertyGetterObjectArray(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService, ObjectArrayEventPropertyGetter arrayGetter)
            : base(propertyIndex, fragmentType, eventAdapterService)
        {
            _arrayGetter = arrayGetter;
        }

        public override Object HandleNestedValue(Object value)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return _arrayGetter.Get((EventBean)value);
                }
                return null;
            }
            return _arrayGetter.GetObjectArray((Object[])value);
        }

        public override Object HandleNestedValueFragment(Object value)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return _arrayGetter.GetFragment((EventBean)value);
                }
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = EventAdapterService.AdapterForTypedObjectArray((Object[])value, FragmentType);
            return _arrayGetter.GetFragment(eventBean);
        }
    }
}
