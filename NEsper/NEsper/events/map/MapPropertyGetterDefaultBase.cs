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

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// Getter for map entry.
    /// </summary>
    public abstract class MapPropertyGetterDefaultBase : MapEventPropertyGetter
    {
        private readonly String _propertyName;
        protected readonly EventType FragmentEventType;
        protected readonly EventAdapterService EventAdapterService;

        /// <summary>Ctor. </summary>
        /// <param name="propertyNameAtomic">property name</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        protected MapPropertyGetterDefaultBase(String propertyNameAtomic, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            _propertyName = propertyNameAtomic;
            FragmentEventType = fragmentEventType;
            EventAdapterService = eventAdapterService;
        }

        protected abstract Object HandleCreateFragment(Object value);

        public Object GetMap(IDictionary<String, Object> map)
        {
            return map.Get(_propertyName);
        }

        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true;
        }

        public Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object value = Get(eventBean);
            return HandleCreateFragment(value);
        }
    }
}
