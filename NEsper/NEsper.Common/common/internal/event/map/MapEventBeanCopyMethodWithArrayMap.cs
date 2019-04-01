///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
	/// <summary>
	/// Copy method for Map-underlying events.
	/// </summary>
	public class MapEventBeanCopyMethodWithArrayMap : EventBeanCopyMethod {
	    private readonly MapEventType mapEventType;
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
	    private readonly string[] mapPropertiesToCopy;
	    private readonly string[] arrayPropertiesToCopy;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="mapEventType">map event type</param>
	    /// <param name="eventBeanTypedEventFactory">for copying events</param>
	    /// <param name="mapPropertiesToCopy">map props</param>
	    /// <param name="arrayPropertiesToCopy">array props</param>
	    public MapEventBeanCopyMethodWithArrayMap(MapEventType mapEventType, EventBeanTypedEventFactory eventBeanTypedEventFactory, string[] mapPropertiesToCopy, string[] arrayPropertiesToCopy) {
	        this.mapEventType = mapEventType;
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	        this.mapPropertiesToCopy = mapPropertiesToCopy;
	        this.arrayPropertiesToCopy = arrayPropertiesToCopy;
	    }

	    public EventBean Copy(EventBean theEvent) {
	        MappedEventBean mapped = (MappedEventBean) theEvent;
	        IDictionary<string, object> props = mapped.Properties;
	        Dictionary<string, object> shallowCopy = new Dictionary<string, object>(props);

	        foreach (string name in mapPropertiesToCopy) {
	            IDictionary<string, object> innerMap = (IDictionary<string, object>) props.Get(name);
	            if (innerMap != null) {
	                var copy = new Dictionary<string, object>(innerMap);
	                shallowCopy.Put(name, copy);
	            }
	        }

	        foreach (string name in arrayPropertiesToCopy) {
	            object raw = props.Get(name);
                if ((raw is Array array) && (array.Length != 0)) {
	                var copied = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
                    Array.Copy(array, 0, copied, 0, array.Length);
	                shallowCopy.Put(name, copied);
	            }
	        }

	        return eventBeanTypedEventFactory.AdapterForTypedMap(shallowCopy, mapEventType);
	    }
	}
} // end of namespace