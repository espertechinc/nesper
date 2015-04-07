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
    /// Copy method for Map-underlying events.
    /// </summary>
    public class MapEventBeanCopyMethodWithArrayMap : EventBeanCopyMethod
    {
        private readonly MapEventType _mapEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly ICollection<String> _mapPropertiesToCopy;
        private readonly ICollection<String> _arrayPropertiesToCopy;
    
        /// <summary>Ctor. </summary>
        /// <param name="mapEventType">map event type</param>
        /// <param name="eventAdapterService">for copying events</param>
        /// <param name="mapPropertiesToCopy"></param>
        /// <param name="arrayPropertiesToCopy"></param>
        public MapEventBeanCopyMethodWithArrayMap(MapEventType mapEventType, EventAdapterService eventAdapterService, ICollection<String> mapPropertiesToCopy, ICollection<String> arrayPropertiesToCopy) {
            _mapEventType = mapEventType;
            _eventAdapterService = eventAdapterService;
            _mapPropertiesToCopy = mapPropertiesToCopy;
            _arrayPropertiesToCopy = arrayPropertiesToCopy;
        }
    
        public EventBean Copy(EventBean theEvent)
        {
            var mapped = (MappedEventBean) theEvent;
            var props = mapped.Properties;
            var shallowCopy = new Dictionary<String, Object>(props);
    
            foreach (var name in _mapPropertiesToCopy) {
                var innerMap = (IDictionary<String, Object>) props.Get(name);
                if (innerMap != null) {
                    var copy = new Dictionary<String, Object>(innerMap);
                    shallowCopy.Put(name, copy);
                }
            }
    
            foreach (var name in _arrayPropertiesToCopy)
            {
                var array = props.Get(name) as Array;
                if (array != null && (array.Length != 0))
                {
                    var copied = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
                    Array.Copy(array, 0, copied, 0, array.Length);
                    shallowCopy.Put(name, copied);
                }
            }

            return _eventAdapterService.AdapterForTypedMap(shallowCopy, _mapEventType);
        }
    }
}
