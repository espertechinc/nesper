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
    /// Copy method for Object array-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly ObjectArrayEventType _objectArrayEventType;
        private readonly EventAdapterService _eventAdapterService;
    
        /// <summary>Ctor. </summary>
        /// <param name="objectArrayEventType">map event type</param>
        /// <param name="eventAdapterService">for copying events</param>
        public ObjectArrayEventBeanCopyMethod(ObjectArrayEventType objectArrayEventType, EventAdapterService eventAdapterService)
        {
            _objectArrayEventType = objectArrayEventType;
            _eventAdapterService = eventAdapterService;
        }
    
        public EventBean Copy(EventBean theEvent)
        {
            Object[] array = ((ObjectArrayBackedEventBean) theEvent).Properties;
            Object[] copy = new Object[array.Length];
            Array.Copy(array, 0, copy, 0, copy.Length);
            return _eventAdapterService.AdapterForTypedObjectArray(copy, _objectArrayEventType);
        }
    }
}
