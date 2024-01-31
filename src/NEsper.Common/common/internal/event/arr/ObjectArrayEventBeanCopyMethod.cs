///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Copy method for Object array-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly ObjectArrayEventType objectArrayEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="objectArrayEventType">map event type</param>
        /// <param name="eventAdapterService">for copying events</param>
        public ObjectArrayEventBeanCopyMethod(
            ObjectArrayEventType objectArrayEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            this.objectArrayEventType = objectArrayEventType;
            this.eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var array = ((ObjectArrayBackedEventBean)theEvent).Properties;
            var copy = new object[array.Length];
            Array.Copy(array, 0, copy, 0, copy.Length);
            return eventAdapterService.AdapterForTypedObjectArray(copy, objectArrayEventType);
        }
    }
} // end of namespace