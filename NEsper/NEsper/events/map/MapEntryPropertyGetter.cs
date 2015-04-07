///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.events.map
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class MapEntryPropertyGetter : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        private readonly EventAdapterService _eventAdapterService;
        private readonly BeanEventType _eventType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="eventType">type of the entry returned</param>
        public MapEntryPropertyGetter(String propertyName, BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            _propertyName = propertyName;
            _eventAdapterService = eventAdapterService;
            _eventType = eventType;
        }

        public Object GetMap(DataMap map)
        {
            return map.Get(_propertyName);
        }

        public bool IsMapExistsProperty(DataMap map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_eventType == null)
            {
                return null;
            }
            Object result = Get(eventBean);
            return BaseNestableEventUtil.GetFragmentPono(result, _eventType, _eventAdapterService);
        }
    }
}
